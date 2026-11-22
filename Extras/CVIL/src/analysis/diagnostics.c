#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <unistd.h>
#include "cvil_diag.h"

cvil_diag_engine_t g_diag_engine = {
    .error_count = 0,
    .warning_count = 0,
    .color_enabled = true,
    .json_output = false,
    .out_stream = NULL
};

static const char* g_current_filename = NULL;

void cvil_diag_set_file(const char* filename) {
    g_current_filename = filename;
}

void cvil_diag_init(bool color_enabled, bool json_output) {
    g_diag_engine.error_count = 0;
    g_diag_engine.warning_count = 0;
    g_diag_engine.color_enabled = color_enabled && isatty(fileno(stderr));
    g_diag_engine.json_output = json_output;
    g_diag_engine.out_stream = stderr;
}

static char* get_line_from_file(const char* filename, int line_num) {
    if (!filename || line_num <= 0) return NULL;
    FILE* f = fopen(filename, "r");
    if (!f) return NULL;

    char* line = NULL;
    size_t len = 0;
    int cur_line = 1;
    ssize_t read;

    while ((read = getline(&line, &len, f)) != -1) {
        if (cur_line == line_num) {
            /* Strip trailing newline */
            while (read > 0 && (line[read - 1] == '\n' || line[read - 1] == '\r')) {
                line[--read] = '\0';
            }
            fclose(f);
            return line;
        }
        cur_line++;
    }
    free(line);
    fclose(f);
    return NULL;
}

void cvil_emit_diag(const cvil_diagnostic_t* diag) {
    FILE* out = g_diag_engine.out_stream ? g_diag_engine.out_stream : stderr;

    const char* filename = diag->span.filename ? diag->span.filename : g_current_filename;

    if (diag->severity == CVIL_SEV_ERROR || diag->severity == CVIL_SEV_FATAL) {
        g_diag_engine.error_count++;
    } else if (diag->severity == CVIL_SEV_WARNING) {
        g_diag_engine.warning_count++;
    }

    if (g_diag_engine.json_output) {
        fprintf(out, "{\"severity\":\"%s\",\"code\":\"%s\",\"message\":\"%s\",\"file\":\"%s\",\"line\":%d,\"col\":%d}\n",
                diag->severity == CVIL_SEV_ERROR ? "error" : (diag->severity == CVIL_SEV_WARNING ? "warning" : "note"),
                diag->code ? diag->code : "",
                diag->message ? diag->message : "",
                filename ? filename : "",
                diag->span.first_line,
                diag->span.first_column);
        return;
    }

    const char* c_red = g_diag_engine.color_enabled ? "\033[1;31m" : "";
    const char* c_yellow = g_diag_engine.color_enabled ? "\033[1;33m" : "";
    const char* c_cyan = g_diag_engine.color_enabled ? "\033[1;36m" : "";
    const char* c_bold = g_diag_engine.color_enabled ? "\033[1m" : "";
    const char* c_reset = g_diag_engine.color_enabled ? "\033[0m" : "";

    const char* sev_str = "note";
    const char* sev_color = c_cyan;
    if (diag->severity == CVIL_SEV_ERROR || diag->severity == CVIL_SEV_FATAL) {
        sev_str = "error";
        sev_color = c_red;
    } else if (diag->severity == CVIL_SEV_WARNING) {
        sev_str = "warning";
        sev_color = c_yellow;
    }

    /* Header: error[CVIL0102]: message */
    if (diag->code) {
        fprintf(out, "%s%s[%s]: %s%s%s\n", sev_color, sev_str, diag->code, c_bold, diag->message, c_reset);
    } else {
        fprintf(out, "%s%s: %s%s%s\n", sev_color, sev_str, c_bold, diag->message, c_reset);
    }

    /* Location: --> file:line:col */
    if (filename) {
        fprintf(out, "  %s-->%s %s:%d:%d\n", c_cyan, c_reset, filename, diag->span.first_line, diag->span.first_column);
    }

    /* Source Line Snippet */
    char* src_line = get_line_from_file(filename, diag->span.first_line);
    if (src_line) {
        fprintf(out, "   %s|%s\n", c_cyan, c_reset);
        fprintf(out, "%2d %s|%s %s\n", diag->span.first_line, c_cyan, c_reset, src_line);
        fprintf(out, "   %s|%s ", c_cyan, c_reset);

        int col = diag->span.first_column > 0 ? diag->span.first_column : 1;
        for (int i = 1; i < col; i++) {
            fprintf(out, " ");
        }

        int span_len = (diag->span.last_column >= diag->span.first_column) ?
                       (diag->span.last_column - diag->span.first_column + 1) : 1;
        if (span_len < 1) span_len = 1;

        fprintf(out, "%s^", sev_color);
        for (int i = 1; i < span_len; i++) {
            fprintf(out, "~");
        }
        if (diag->label_msg) {
            fprintf(out, " %s", diag->label_msg);
        }
        fprintf(out, "%s\n", c_reset);
        free(src_line);
    }

    /* Help suggestion */
    if (diag->help_msg) {
        fprintf(out, "   %s=%s %shelp:%s %s\n", c_cyan, c_reset, c_bold, c_reset, diag->help_msg);
    }
    if (diag->suggestion) {
        fprintf(out, "     %s\n", diag->suggestion);
    }
    fprintf(out, "\n");
}

void cvil_diag_error(cvil_span_t span, const char* code, const char* fmt, ...) {
    char buf[1024];
    va_list args;
    va_start(args, fmt);
    vsnprintf(buf, sizeof(buf), fmt, args);
    va_end(args);

    cvil_diagnostic_t d = {
        .severity = CVIL_SEV_ERROR,
        .code = code,
        .message = buf,
        .span = span,
        .label_span = span,
        .label_msg = NULL,
        .help_msg = NULL,
        .suggestion = NULL
    };
    cvil_emit_diag(&d);
}

void cvil_diag_error_with_help(cvil_span_t span, const char* code, const char* help, const char* fmt, ...) {
    char buf[1024];
    va_list args;
    va_start(args, fmt);
    vsnprintf(buf, sizeof(buf), fmt, args);
    va_end(args);

    cvil_diagnostic_t d = {
        .severity = CVIL_SEV_ERROR,
        .code = code,
        .message = buf,
        .span = span,
        .label_span = span,
        .label_msg = NULL,
        .help_msg = help,
        .suggestion = NULL
    };
    cvil_emit_diag(&d);
}

void cvil_diag_warning(cvil_span_t span, const char* code, const char* fmt, ...) {
    char buf[1024];
    va_list args;
    va_start(args, fmt);
    vsnprintf(buf, sizeof(buf), fmt, args);
    va_end(args);

    cvil_diagnostic_t d = {
        .severity = CVIL_SEV_WARNING,
        .code = code,
        .message = buf,
        .span = span,
        .label_span = span,
        .label_msg = NULL,
        .help_msg = NULL,
        .suggestion = NULL
    };
    cvil_emit_diag(&d);
}
