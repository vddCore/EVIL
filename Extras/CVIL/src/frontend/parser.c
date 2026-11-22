#include <stdio.h>
#include <stdlib.h>
#include "cvil.h"
#include "cvil_ast.h"
#include "cvil_diag.h"

extern int yyparse(void);
extern void yyrestart(FILE* input_file);
extern int yylineno;
extern int yycolumn;
extern FILE* yyin;
extern cvil_ast_node_t* g_ast_root;

cvil_ast_node_t* cvil_parse_file(const char* filename) {
    if (!filename) return NULL;

    FILE* f = fopen(filename, "r");
    if (!f) {
        cvil_span_t span = { filename, 1, 1, 1, 1 };
        cvil_diag_error(span, "CVIL0001", "cannot open file '%s'", filename);
        return NULL;
    }

    cvil_diag_set_file(filename);
    yylineno = 1;
    yycolumn = 1;
    g_ast_root = NULL;

    yyrestart(f);
    int res = yyparse();
    fclose(f);

    if (res != 0 || g_diag_engine.error_count > 0) {
        return NULL;
    }

    return g_ast_root;
}
