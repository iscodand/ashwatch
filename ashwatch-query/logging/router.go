package logging

import "github.com/go-chi/chi/v5"

func Routes(h *LogHandler) chi.Router {
	r := chi.NewRouter()

	r.Get("/", h.GetFilteredLogs)
	r.Get("/{id}", h.GetLogByID)

	return r
}
