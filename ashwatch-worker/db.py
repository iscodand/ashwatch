from pymongo import MongoClient
from pymongo.errors import ConnectionFailure


MONGO_URI="mongodb://root:rootpass@localhost:27017/?authSource=admin"


def get_database():
    try:
        client = MongoClient(MONGO_URI)

        print("Connected to Mongo!")

        return client["ash_logging_dev"]

    except ConnectionFailure as e:
        print(f"Connection failure: {e}")
        return None
    