import os

from pymongo import MongoClient
from pymongo.errors import ConnectionFailure
from dotenv import load_dotenv


load_dotenv()

MONGO_URI=os.getenv("MONGO_URI")


def get_database():
    try:
        client = MongoClient(MONGO_URI)

        print("Connected to Mongo!")

        return client["ash_logging_dev"]

    except ConnectionFailure as e:
        print(f"Connection failure: {e}")
        return None
    