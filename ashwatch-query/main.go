package main

import (
	"ashwatchquery/db"
	"ashwatchquery/docs"
	"ashwatchquery/logging"
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/joho/godotenv"
)

func main() {
	_ = godotenv.Load(".env")

	ctx := context.Background()

	dynamoClient, err := db.Connect(ctx)
	if err != nil {
		log.Fatalf("dynamodb connect error: %v", err)
	}

	tableName := os.Getenv("DYNAMODB_TABLE")
	if tableName == "" {
		tableName = "ashwatch"
	}

	logRepo := logging.NewLogRepository(dynamoClient, tableName)
	logHandler := logging.NewLogHandler(logRepo)

	r := chi.NewRouter()
	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)
	r.Use(middleware.Timeout(15 * time.Second))
	r.Use(func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			w.Header().Set("Access-Control-Allow-Origin", "*")
			w.Header().Set("Access-Control-Allow-Methods", "GET, OPTIONS")
			w.Header().Set("Access-Control-Allow-Headers", "Content-Type")
			if r.Method == "OPTIONS" {
				w.WriteHeader(http.StatusOK)
				return
			}
			next.ServeHTTP(w, r)
		})
	})

	r.Get("/health", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		_, _ = w.Write([]byte("ok"))
	})

	r.Get("/swagger/doc.json", docs.SpecHandler)
	r.Get("/swagger/*", docs.UIHandler)

	r.Mount("/logs", logging.Routes(logHandler))

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	fmt.Printf("Server running on port: %s\n", port)

	log.Fatalf("server error: %v", http.ListenAndServe(":"+port, r))
}
