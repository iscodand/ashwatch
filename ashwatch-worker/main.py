import json
import logging
import os
import uuid
from decimal import Decimal

import boto3

logger = logging.getLogger()
logger.setLevel(logging.INFO)

_dynamodb = boto3.resource("dynamodb", region_name=os.environ.get("AWS_REGION", "us-east-1"))
_table = _dynamodb.Table(os.environ["DYNAMODB_TABLE"])


def _parse_record_body(body_str: str) -> dict:
    body = json.loads(body_str)
    if isinstance(body, dict) and body.get("Type") == "Notification":
        return json.loads(body["Message"], parse_float=Decimal)
    return json.loads(json.dumps(body), parse_float=Decimal)


def lambda_handler(event, context):
    batch_item_failures = []

    for record in event["Records"]:
        message_id = record["messageId"]
        try:
            data = _parse_record_body(record["body"])
            data.setdefault("id", str(uuid.uuid4()))
            _table.put_item(Item=data)
            logger.info("Log inserted. message_id=%s id=%s", message_id, data["id"])
        except Exception:
            logger.exception("Failed to process record. message_id=%s", message_id)
            batch_item_failures.append({"itemIdentifier": message_id})

    return {"batchItemFailures": batch_item_failures}
