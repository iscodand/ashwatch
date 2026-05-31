import os
from pymongo import MongoClient
from pymongo.errors import ConnectionFailure
from dotenv import load_dotenv

load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
MONGO_DB_NAME = os.getenv("MONGO_DB_NAME", "ash_logging")
MONGO_CA_FILE = os.getenv("MONGO_CA_FILE")

_client = None


def get_database():
    global _client

    try:
        print(repr(MONGO_CA_FILE))
        print(os.path.exists(MONGO_CA_FILE))

        if not os.path.exists(MONGO_CA_FILE):
            raise Exception(f"CA file not found at: {MONGO_CA_FILE}")

        if _client is None:
            _client = MongoClient(
                MONGO_URI,
                tls=True,
                tlsCAFile=MONGO_CA_FILE,
                retryWrites=False,
                serverSelectionTimeoutMS=5000,
                tlsAllowInvalidHostnames=True
            )

            _client.admin.command("ping")
            print("✅ Connected to Mongo (DocumentDB)!")

        return _client[MONGO_DB_NAME]

    except Exception as e:
        print(f"❌ Error: {e}")
        return None
    