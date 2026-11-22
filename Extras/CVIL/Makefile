CC ?= gcc
CFLAGS ?= -O2 -Wall -Wextra -Iinclude -Isrc/frontend -DHAVE_LIBGC -g
LDFLAGS = -lgc -lm
AR ?= ar

SRC_RUNTIME = src/runtime/cvil_runtime.c
SRC_FRONTEND = src/frontend/ast.c src/frontend/parser.c src/frontend/evil.tab.c src/frontend/lex.yy.c src/frontend/import_resolver.c
SRC_ANALYSIS = src/analysis/diagnostics.c src/analysis/scope.c
SRC_CODEGEN = src/codegen/codegen.c
SRC_DRIVER = src/driver/main.c

OBJS = $(SRC_RUNTIME:.c=.o) \
       src/frontend/ast.o \
       src/frontend/parser.o \
       src/frontend/evil.tab.o \
       src/frontend/lex.yy.o \
       src/frontend/import_resolver.o \
       $(SRC_ANALYSIS:.c=.o) \
       $(SRC_CODEGEN:.c=.o) \
       $(SRC_DRIVER:.c=.o)

all: bin/cvil lib/libcvil.a

bin/cvil: $(OBJS) | bin
	$(CC) $(OBJS) $(LDFLAGS) -o $@

lib/libcvil.a: src/runtime/cvil_runtime.o | lib
	$(AR) rcs $@ $^

src/frontend/evil.tab.c src/frontend/evil.tab.h: src/frontend/evil.y
	bison -d -o src/frontend/evil.tab.c src/frontend/evil.y

src/frontend/lex.yy.c: src/frontend/evil.l src/frontend/evil.tab.h
	flex -o src/frontend/lex.yy.c src/frontend/evil.l

src/frontend/evil.tab.o: src/frontend/evil.tab.c
	$(CC) $(CFLAGS) -Wno-unused-function -Wno-unused-variable -c $< -o $@

src/frontend/lex.yy.o: src/frontend/lex.yy.c
	$(CC) $(CFLAGS) -Wno-unused-function -Wno-unused-variable -c $< -o $@

%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

bin:
	mkdir -p bin

lib:
	mkdir -p lib

clean:
	rm -rf bin lib $(OBJS) src/frontend/evil.tab.* src/frontend/lex.yy.c

test: bin/cvil lib/libcvil.a
	@echo "=========================================================="
	@echo "Running CVIL Language Tests against Ceres Test Suite"
	@echo "=========================================================="
	@bash -c '\
	TEST_DIR="../../VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests"; \
	TOTAL=0; PASSED=0; FAILED=0; \
	for test_file in $$TEST_DIR/*.vil; do \
		echo "--- Running test file: $$(basename $$test_file) ---"; \
		./bin/cvil --test "$$test_file"; \
		if [ $$? -eq 0 ]; then \
			PASSED=$$((PASSED + 1)); \
		else \
			FAILED=$$((FAILED + 1)); \
		fi; \
		TOTAL=$$((TOTAL + 1)); \
	done; \
	echo "=========================================================="; \
	echo "Language Test Summary: $$PASSED / $$TOTAL files passed ($$FAILED failed)"; \
	echo "=========================================================="; \
	[ $$FAILED -eq 0 ]'

.PHONY: all clean test
