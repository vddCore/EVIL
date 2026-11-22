#ifndef CVIL_DIAG_H
#define CVIL_DIAG_H

#include <stdio.h>
#include <stdbool.h>
#include <stdint.h>

typedef enum {
    CVIL_SEV_NOTE,
    CVIL_SEV_WARNING,
    CVIL_SEV_ERROR,
    CVIL_SEV_FATAL
} cvil_severity_t;

typedef struct cvil_span {
    const char* filename;
    int first_line;
    int first_column;
    int last_line;
    int last_column;
} cvil_span_t;

typedef struct cvil_diagnostic {
    cvil_severity_t severity;
    const char* code;       /* e.g., "CVIL0102" */
    const char* message;    /* e.g., "cannot assign to immutable variable 'x'" */
    cvil_span_t span;       /* primary error span */
    cvil_span_t label_span; /* secondary reference span (e.g., declaration) */
    const char* label_msg;  /* e.g., "originally declared here" */
    const char* help_msg;   /* e.g., "declare variable as mutable using 'rw val'" */
    const char* suggestion; /* code replacement hint */
} cvil_diagnostic_t;

typedef struct cvil_diag_engine {
    int error_count;
    int warning_count;
    bool color_enabled;
    bool json_output;
    FILE* out_stream;
} cvil_diag_engine_t;

extern cvil_diag_engine_t g_diag_engine;

void cvil_diag_init(bool color_enabled, bool json_output);
void cvil_diag_set_file(const char* filename);
void cvil_emit_diag(const cvil_diagnostic_t* diag);
void cvil_diag_error(cvil_span_t span, const char* code, const char* fmt, ...);
void cvil_diag_error_with_help(cvil_span_t span, const char* code, const char* help, const char* fmt, ...);
void cvil_diag_warning(cvil_span_t span, const char* code, const char* fmt, ...);

#endif /* CVIL_DIAG_H */
