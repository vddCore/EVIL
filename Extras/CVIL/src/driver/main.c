#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/wait.h>
#include <libgen.h>
#include <time.h>
#include "cvil.h"

static void print_usage(const char* prog) {
    printf("CVIL - EVIL to Native C Transpiler & Toolchain\n");
    printf("Usage: %s [options] <input.vil>\n\n", prog);
    printf("Options:\n");
    printf("  -o <file>        Specify output binary or C file\n");
    printf("  -c, --emit-c     Only emit C code without compiling to binary\n");
    printf("  -r, --run        Execute binary immediately after compilation\n");
    printf("  --test           Compile and execute test suite (runs #[test] functions)\n");
    printf("  -O0, -O1, -O2, -O3  Set C compiler optimization level (default: -O2)\n");
    printf("  -v, --verbose    Print verbose compilation steps\n");
    printf("  --no-color       Disable colored diagnostic output\n");
    printf("  --json           Emit diagnostics in JSON format\n");
    printf("  -h, --help       Display this help message\n");
}

/* Get directory of current executable */
static char* get_executable_dir(void) {
    char buf[1024];
    ssize_t len = readlink("/proc/self/exe", buf, sizeof(buf) - 1);
    if (len != -1) {
        buf[len] = '\0';
        return strdup(dirname(buf));
    }
    return strdup(".");
}

int cvil_compile_c_to_binary(const char* c_filename, const char* out_binary, const cvil_options_t* opts) {
    char* exe_dir = get_executable_dir();
    char inc_dir[1024];
    char runtime_obj[1024];
    char runtime_lib[1024];

    /* Look for include and runtime relative to binary or source tree */
    snprintf(inc_dir, sizeof(inc_dir), "%s/../include", exe_dir);
    snprintf(runtime_obj, sizeof(runtime_obj), "%s/../src/runtime/cvil_runtime.o", exe_dir);
    snprintf(runtime_lib, sizeof(runtime_lib), "%s/../lib/libcvil.a", exe_dir);

    /* Fallback checks if running in-tree */
    if (access(runtime_obj, R_OK) != 0) {
        snprintf(runtime_obj, sizeof(runtime_obj), "Extras/CVIL/src/runtime/cvil_runtime.o");
    }
    if (access(inc_dir, R_OK) != 0) {
        snprintf(inc_dir, sizeof(inc_dir), "Extras/CVIL/include");
    }

    char cmd[4096];
    const char* opt_flag = "-O2";
    if (opts->opt_level == 0) opt_flag = "-O0";
    else if (opts->opt_level == 1) opt_flag = "-O1";
    else if (opts->opt_level == 3) opt_flag = "-O3";

    if (access(runtime_lib, R_OK) == 0) {
        snprintf(cmd, sizeof(cmd), "gcc %s -I%s -Iinclude \"%s\" \"%s\" -DHAVE_LIBGC -lgc -lm -o \"%s\"",
                 opt_flag, inc_dir, c_filename, runtime_lib, out_binary);
    } else {
        snprintf(cmd, sizeof(cmd), "gcc %s -I%s -Iinclude \"%s\" \"%s\" -DHAVE_LIBGC -lgc -lm -o \"%s\"",
                 opt_flag, inc_dir, c_filename, runtime_obj, out_binary);
    }

    if (opts->verbose) {
        printf("[CVIL CC] %s\n", cmd);
    }

    free(exe_dir);
    int res = system(cmd);
    if (res != 0) {
        fprintf(stderr, "error: host C compiler (gcc) failed with exit code %d\n", res);
        return 1;
    }
    return 0;
}

int main(int argc, char** argv) {
    cvil_options_t opts = {
        .input_file = NULL,
        .output_file = NULL,
        .emit_c_only = false,
        .run_immediately = false,
        .test_mode = false,
        .verbose = false,
        .optimize = true,
        .opt_level = 2,
        .enable_colors = true,
        .json_diagnostics = false
    };

    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-h") == 0 || strcmp(argv[i], "--help") == 0) {
            print_usage(argv[0]);
            return 0;
        } else if (strcmp(argv[i], "-v") == 0 || strcmp(argv[i], "--verbose") == 0) {
            opts.verbose = true;
        } else if (strcmp(argv[i], "-c") == 0 || strcmp(argv[i], "--emit-c") == 0) {
            opts.emit_c_only = true;
        } else if (strcmp(argv[i], "-r") == 0 || strcmp(argv[i], "--run") == 0) {
            opts.run_immediately = true;
        } else if (strcmp(argv[i], "--test") == 0) {
            opts.test_mode = true;
            opts.run_immediately = true;
        } else if (strcmp(argv[i], "--no-color") == 0) {
            opts.enable_colors = false;
        } else if (strcmp(argv[i], "--json") == 0) {
            opts.json_diagnostics = true;
        } else if (strcmp(argv[i], "-O0") == 0) {
            opts.opt_level = 0;
        } else if (strcmp(argv[i], "-O1") == 0) {
            opts.opt_level = 1;
        } else if (strcmp(argv[i], "-O2") == 0) {
            opts.opt_level = 2;
        } else if (strcmp(argv[i], "-O3") == 0) {
            opts.opt_level = 3;
        } else if (strcmp(argv[i], "-o") == 0) {
            if (++i < argc) {
                opts.output_file = argv[i];
            } else {
                fprintf(stderr, "error: -o requires an argument\n");
                return 1;
            }
        } else if (argv[i][0] == '-') {
            fprintf(stderr, "error: unrecognized option '%s'\n", argv[i]);
            print_usage(argv[0]);
            return 1;
        } else {
            if (!opts.input_file) {
                opts.input_file = argv[i];
            } else {
                fprintf(stderr, "error: multiple input files specified: '%s' and '%s'\n", opts.input_file, argv[i]);
                return 1;
            }
        }
    }

    if (!opts.input_file) {
        fprintf(stderr, "error: no input .vil file specified\n");
        print_usage(argv[0]);
        return 1;
    }

    /* Initialize diagnostic engine */
    cvil_diag_init(opts.enable_colors, opts.json_diagnostics);

    if (opts.verbose) {
        printf("[CVIL] Parsing '%s'...\n", opts.input_file);
    }

    /* 1. Frontend: Lex and Parse */
    cvil_ast_node_t* root = cvil_parse_file(opts.input_file);
    if (!root) {
        if (opts.verbose) fprintf(stderr, "[CVIL] Parse failed.\n");
        return 1;
    }

    /* 1b. Import Resolution (dofile & require) */
    root = cvil_resolve_imports(root, opts.input_file);
    if (!root) {
        if (opts.verbose) fprintf(stderr, "[CVIL] Import resolution failed.\n");
        return 1;
    }

    /* 2. Semantic Analysis */
    if (opts.verbose) {
        printf("[CVIL] Performing semantic and scope analysis...\n");
    }
    if (!cvil_analyze_semantics(root)) {
        if (opts.verbose) fprintf(stderr, "[CVIL] Semantic analysis failed with errors.\n");
        return 1;
    }

    /* 3. Determine C output path */
    char c_filepath[1024];
    char bin_filepath[1024];

    if (opts.emit_c_only) {
        if (opts.output_file) {
            snprintf(c_filepath, sizeof(c_filepath), "%s", opts.output_file);
        } else {
            snprintf(c_filepath, sizeof(c_filepath), "%s.c", opts.input_file);
        }
    } else {
        snprintf(c_filepath, sizeof(c_filepath), "/tmp/cvil_%d_%ld.c", getpid(), (long)time(NULL));
        if (opts.output_file) {
            snprintf(bin_filepath, sizeof(bin_filepath), "%s", opts.output_file);
        } else if (opts.test_mode) {
            snprintf(bin_filepath, sizeof(bin_filepath), "/tmp/cvil_test_%d_%ld.bin", getpid(), (long)time(NULL));
        } else {
            /* Replace or append extension */
            const char* dot = strrchr(opts.input_file, '.');
            if (dot) {
                size_t base_len = dot - opts.input_file;
                snprintf(bin_filepath, sizeof(bin_filepath), "%.*s", (int)base_len, opts.input_file);
            } else {
                snprintf(bin_filepath, sizeof(bin_filepath), "%s.bin", opts.input_file);
            }
        }
    }

    if (opts.verbose) {
        printf("[CVIL] Generating C code to '%s'...\n", c_filepath);
    }

    FILE* c_out = fopen(c_filepath, "w");
    if (!c_out) {
        fprintf(stderr, "error: cannot open output C file '%s' for writing\n", c_filepath);
        return 1;
    }

    /* 4. Code Generation */
    if (!cvil_generate_c(root, c_out, opts.input_file, opts.test_mode)) {
        fclose(c_out);
        fprintf(stderr, "error: C code generation failed\n");
        return 1;
    }
    fclose(c_out);

    if (opts.emit_c_only) {
        if (opts.verbose) {
            printf("[CVIL] Done. Emitted C source at '%s'\n", c_filepath);
        }
        return 0;
    }

    /* 5. Compile C to Native Binary */
    if (opts.verbose) {
        printf("[CVIL] Compiling C code to native binary '%s'...\n", bin_filepath);
    }
    int cc_res = cvil_compile_c_to_binary(c_filepath, bin_filepath, &opts);
    if (!opts.verbose) {
        /* Remove temp C file */
        unlink(c_filepath);
    }
    if (cc_res != 0) {
        return cc_res;
    }

    /* 6. Execute binary if requested */
    if (opts.run_immediately || opts.test_mode) {
        if (opts.verbose) {
            printf("[CVIL] Executing '%s'...\n", bin_filepath);
        }
        int exec_res = system(bin_filepath);
        if (opts.test_mode && !opts.output_file) {
            unlink(bin_filepath);
        }
        if (WIFEXITED(exec_res)) {
            return WEXITSTATUS(exec_res);
        }
        return 1;
    }

    return 0;
}
