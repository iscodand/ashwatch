package logging

import (
	"ashwatchquery/db"
	"context"
	"encoding/json"
	"net/http"

	"go.mongodb.org/mongo-driver/bson"
)

func GetFilteredLogs(w http.ResponseWriter, r *http.Request) {
	client, err := db.Connect()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
	defer client.Disconnect(context.Background())

	var logs []Log
	collection := client.Database("ash_logging_dev").Collection("logs")
	cursor, err := collection.Find(context.Background(), bson.D{})
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
	defer cursor.Close(context.Background())

	for cursor.Next(context.Background()) {
		var log Log
		if err := cursor.Decode(&log); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}

		logs = append(logs, log)
	}

	json.NewEncoder(w).Encode(logs)
}
