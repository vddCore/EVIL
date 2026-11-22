#ifndef CVIL_H
#define CVIL_H

#include "cvil_value.h"
#include "cvil_diag.h"
#include "cvil_ast.h"
#include "cvil_runtime.h"

/* Compiler Configuration */
typedef struct cvil_options {
    const char* input_file;
    const char* output_file;
    bool emit_c_only;
    bool run_immediately;
    bool test_mode;
    bool verbose;
    bool optimize;
    int opt_level;
    bool enable_colors;
    bool json_diagnostics;
} cvil_options_t;

/* Transpiler Pipeline Entrypoints */
cvil_ast_node_t* cvil_parse_file(const char* filename);
cvil_ast_node_t* cvil_resolve_imports(cvil_ast_node_t* root, const char* source_filename);
bool cvil_analyze_semantics(cvil_ast_node_t* root);
bool cvil_generate_c(cvil_ast_node_t* root, FILE* out_c_file, const char* source_filename, bool test_mode);
int cvil_compile_c_to_binary(const char* c_filename, const char* out_binary, const cvil_options_t* opts);

#endif /* CVIL_H */
