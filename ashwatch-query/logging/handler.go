package logging

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
)

type LogHandler struct {
	repo LogRepository
}

func NewLogHandler(repo LogRepository) *LogHandler {
	return &LogHandler{repo: repo}
}

func (h *LogHandler) GetFilteredLogs(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()

	tenantID := r.URL.Query().Get("tenantId")
	if tenantID == "" {
		http.Error(w, "tenantId is required", http.StatusBadRequest)
		return
	}

	startDateStr := r.URL.Query().Get("startDate")
	endDateStr := r.URL.Query().Get("endDate")

	var startDate, endDate time.Time
	var err error

	if startDateStr != "" {
		startDate, err = time.Parse(time.RFC3339, startDateStr)
		if err != nil {
			http.Error(w, "invalid startDate: "+err.Error(), http.StatusBadRequest)
			return
		}
	}

	if endDateStr != "" {
		endDate, err = time.Parse(time.RFC3339, endDateStr)
		if err != nil {
			http.Error(w, "invalid endDate: "+err.Error(), http.StatusBadRequest)
			return
		}
	}

	filter := GetLogFilter{
		TenantID:  tenantID,
		StartDate: startDate,
		EndDate:   endDate,
	}

	logs, err := h.repo.GetWithFilters(ctx, filter)
	if err != nil {
		http.Error(w, "unexpected error", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(logs)
}

func (h *LogHandler) GetLogByID(w http.ResponseWriter, r *http.Request) {
	id := chi.URLParam(r, "id")

	log, err := h.repo.GetByID(r.Context(), id)
	if err != nil {
		http.Error(w, "unexpected error", http.StatusInternalServerError)
		return
	}
	if log == nil {
		http.Error(w, "log not found", http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(log)
}
