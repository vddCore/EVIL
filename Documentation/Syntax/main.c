/*
 * Test driver and CLI validator for the EVIL Flex/Bison parser.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

extern int yyparse(void);
extern void yyrestart(FILE *input_file);
extern int yylineno;
extern int yycolumn;

char *current_filename = NULL;

static void print_usage(const char *prog) {
    printf("Usage: %s [options] <file1.vil> [file2.vil ...]\n", prog);
    printf("Options:\n");
    printf("  -q, --quiet     Only print summary and errors\n");
    printf("  -h, --help      Display this help message\n");
    printf("If no files are given or '-' is specified, reads from standard input.\n");
}

int main(int argc, char **argv) {
    int quiet = 0;
    int first_file_idx = 1;

    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-q") == 0 || strcmp(argv[i], "--quiet") == 0) {
            quiet = 1;
        } else if (strcmp(argv[i], "-h") == 0 || strcmp(argv[i], "--help") == 0) {
            print_usage(argv[0]);
            return 0;
        } else if (argv[i][0] != '-' || strcmp(argv[i], "-") == 0) {
            first_file_idx = i;
            break;
        }
    }

    /* Standard input mode */
    if (argc <= first_file_idx || (argc == 2 && strcmp(argv[1], "-") == 0)) {
        current_filename = "<stdin>";
        yylineno = 1;
        yycolumn = 1;
        yyrestart(stdin);
        int res = yyparse();
        if (res == 0) {
            if (!quiet) printf("[PASS] <stdin>\n");
            return 0;
        } else {
            fprintf(stderr, "[FAIL] <stdin>\n");
            return 1;
        }
    }

    int total = 0;
    int failed = 0;

    for (int i = first_file_idx; i < argc; i++) {
        if (strcmp(argv[i], "-q") == 0 || strcmp(argv[i], "--quiet") == 0) {
            continue;
        }

        current_filename = argv[i];
        FILE *f = fopen(current_filename, "r");
        if (!f) {
            fprintf(stderr, "Error: Could not open file '%s'\n", current_filename);
            failed++;
            total++;
            continue;
        }

        yylineno = 1;
        yycolumn = 1;
        yyrestart(f);

        int result = yyparse();
        fclose(f);
        total++;

        if (result == 0) {
            if (!quiet) {
                printf("[PASS] %s\n", current_filename);
            }
        } else {
            printf("[FAIL] %s\n", current_filename);
            failed++;
        }
    }

    printf("\nTotal: %d, Passed: %d, Failed: %d\n", total, total - failed, failed);
    return failed == 0 ? 0 : 1;
}
