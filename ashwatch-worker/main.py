import json
import logging
import os
import signal
import time
from typing import Any
from pymongo import MongoClient
import db
import boto3
from botocore.exceptions import BotoCoreError, ClientError
from dotenv import load_dotenv


load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s"
)

QUEUE_URL=os.getenv("AWS_SQS_URL")
AWS_REGION=os.getenv("AWS_REGION")
QUEUE_NAME=os.getenv("QUEUE_NAME")

running = True

def stop_worker(signum: int, frame: Any) -> None:
    global running
    logging.info("Encerrando working...")
    running = False


signal.signal(signal.SIGINT, stop_worker)
signal.signal(signal.SIGTERM, stop_worker)


def insert_message(msg):
    try:
        data = json.loads(msg["Message"])

        print(data)

        database = db.get_database()
        collection = database["logs"]

        collection.insert_one(data)

        logging.info("Log inserted succesfuly")
    except Exception as e:
        logging.exception(e)


def process_message(payload) -> None:
    logging.info("Processing log")
    insert_message(payload)


def main() -> None:
    endpoint_url = os.getenv("AWS_ENDPOINT_URL")
    sqs = boto3.client("sqs", region_name=AWS_REGION, endpoint_url=endpoint_url)

    logging.info("Worker started, listening main queue...")

    while running:
        try:
            response = sqs.receive_message(
                QueueUrl=QUEUE_URL,
                MaxNumberOfMessages=5,
                WaitTimeSeconds=20,
                VisibilityTimeout=30,
                AttributeNames=["ALL"],
                MessageAttributeNames=["ALL"]
            )

            messages = response.get("Messages", [])

            if not messages:
                continue

            for message in messages:
                message_id = message["MessageId"]
                receipt_handle = message["ReceiptHandle"]
                receive_count = message.get("Attributes", {}).get("ApproximateReceiveCount", "1")
                body = message["Body"]

                logging.info(f"Message successfuly received. {message_id} - {receive_count}")

                print(body)

                try:
                    payload = json.loads(body)

                    process_message(payload)

                    sqs.delete_message(
                        QueueUrl = QUEUE_URL,
                        ReceiptHandle=receipt_handle
                    )

                    logging.info(f"Message successfuly deleted from queue. message_id={message_id}")

                except Exception as e:
                    logging.exception(
                        f"Fail while processing message (message_id={message_id}) ",
                         message_id,
                        f"Error: {e}"
                    )

                    sqs.change_message_visibility(
                        QueueUrl=QUEUE_URL,
                        ReceiptHandle=receipt_handle,
                        VisibilityTimeout=0
                    )

        except (ClientError, BotoCoreError) as e:
            logging.exception("Error connecting to AWS SQS: %s", e)
            time.sleep(2)
        except Exception as e:
            logging.exception("Unexpected error on worker: %s", e)
            time.sleep(2)

    logging.info("Exiting worker...")


if __name__ == "__main__":
    main()
