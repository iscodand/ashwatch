import json
from confluent_kafka import Consumer, KafkaError, Message


LOGS_TOPIC: str = "logs.raw"

conf = {
    "bootstrap.servers": "localhost:9092",
    "group.id": "ashwatch-worker",
    "auto.offset.reset": "earliest",
    "enable.auto.commit": False
}

consumer: Consumer = Consumer(conf)
consumer.subscribe([LOGS_TOPIC])

try:
    while True:
        msg: Message = consumer.poll(1.0)
        if msg is None:
            continue
        if msg.error():
            if msg.error().code() == KafkaError._PARTITION_EOF:
                continue
            print("Kafka error: ", msg.error())
        
        data = json.loads(msg.value().decode("utf-8"))

        # todo: process the message and persists on mongodb
        print("LOG: ", data)

        consumer.commit(message=msg)
except KeyboardInterrupt:
    pass
finally:
    consumer.close()





        