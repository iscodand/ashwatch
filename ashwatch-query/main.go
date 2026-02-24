package main

import (
	"ashwatchquery/db"
	"ashwatchquery/logging"
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
	err := godotenv.Load(".env")
	if err != nil {
		log.Fatalf("Error loading .env file: %s", err)
	}

	mongoClient, err := db.Connect()
	if err != nil {
		log.Fatalf("mongo connect error: %v", err)
	}

	logRepo := logging.NewLogRepository(mongoClient)
	logHandler := logging.NewLogHandler(logRepo)

	r := chi.NewRouter()
	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)
	r.Use(middleware.Timeout(15 * time.Second))

	r.Get("/health", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		_, _ = w.Write([]byte("ok"))
	})

	r.Mount("/logs", logging.Routes(logHandler))

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	fmt.Printf("Server running on port: %s", port)

	fmt.Printf("Server running on port: %s\n", port)

	log.Fatalf("server error: %v", http.ListenAndServe(":"+port, r))
}
