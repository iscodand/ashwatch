import os
import logging
from pymongo import MongoClient
from dotenv import load_dotenv

load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
MONGO_DB_NAME = os.getenv("MONGO_DB_NAME", "ash_logging")
MONGO_CA_FILE = os.getenv("MONGO_CA_FILE")

_client = None


def get_database():
    global _client

    try:
        if _client is None:
            kwargs = {
                "retryWrites": False,
                "serverSelectionTimeoutMS": 5000,
            }

            if MONGO_CA_FILE and os.path.exists(MONGO_CA_FILE):
                kwargs["tls"] = True
                kwargs["tlsCAFile"] = MONGO_CA_FILE
                kwargs["tlsAllowInvalidHostnames"] = True

            _client = MongoClient(MONGO_URI, **kwargs)
            _client.admin.command("ping")
            logging.info("Connected to MongoDB")

        return _client[MONGO_DB_NAME]

    except Exception as e:
        logging.exception("MongoDB connection error: %s", e)
        return None
