package logging

import (
	"encoding/json"
	"fmt"
	"net/http"
	"time"
)

type LogHandler struct {
	repo LogRepository
}

func NewLogHandler(repo LogRepository) *LogHandler {
	return &LogHandler{repo: repo}
}

func (h *LogHandler) GetFilteredLogs(w http.ResponseWriter, r *http.Request) {
	if h == nil {
		panic("handler nil")
	}
	if h.repo == nil {
		panic("repo nil")
	}

	ctx := r.Context()

	startDateStr := r.URL.Query().Get("startDate")
	endDateStr := r.URL.Query().Get("endDate")

	var startDate time.Time
	var endDate time.Time
	var err error

	if startDateStr != "" {
		startDate, err = time.Parse(time.RFC3339, startDateStr)
		if err != nil {
			http.Error(w, "startDate inválida"+err.Error(), http.StatusBadRequest)
		}
	}

	if endDateStr != "" {
		endDate, err = time.Parse(time.RFC3339, endDateStr)
		if err != nil {
			http.Error(w, "endDate inválida"+err.Error(), http.StatusBadRequest)
			return
		}
	}

	filter := GetLogFilter{
		StartDate: startDate,
		EndDate:   endDate,
	}

	logs, err := h.repo.GetWithFilters(ctx, filter)
	if err != nil {
		http.Error(w, "erro ao buscar logs", http.StatusInternalServerError)
		return
	}

	fmt.Print(logs)

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(logs)
}
