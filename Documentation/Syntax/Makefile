CC ?= gcc
CFLAGS ?= -O2 -Wall -Wextra
BISON ?= bison
FLEX ?= flex

TARGET = evil_parser
BISON_SRC = evil.y
FLEX_SRC = evil.l
MAIN_SRC = main.c

TAB_C = evil.tab.c
TAB_H = evil.tab.h
LEX_C = lex.yy.c

TEST_DIR = ../../Assessment/Tests/EVIL.Assessment.Tests.VM.Language/tests
TEST_FILES = $(shell find $(TEST_DIR) -name "*.vil" | sort)
BUILTIN_FILES = $(shell find ../../EVIL/VM/EVIL.VM.Runtime/ScriptBuiltins -name "*.vil" 2>/dev/null | sort)

.PHONY: all clean test help

all: $(TARGET)

$(TAB_C) $(TAB_H): $(BISON_SRC)
	$(BISON) -d -v $(BISON_SRC)

$(LEX_C): $(FLEX_SRC) $(TAB_H)
	$(FLEX) $(FLEX_SRC)

$(TARGET): $(TAB_C) $(LEX_C) $(MAIN_SRC)
	$(CC) $(CFLAGS) -o $@ $(TAB_C) $(LEX_C) $(MAIN_SRC)

test: $(TARGET)
	@echo "Running grammar validation against EVIL Ceres language test suite..."
	@./$(TARGET) $(TEST_FILES) $(BUILTIN_FILES)

clean:
	rm -f $(TARGET) $(TAB_C) $(TAB_H) $(LEX_C) evil.output

help:
	@echo "EVIL Flex/Bison Grammar Makefile"
	@echo "  make         - Build evil_parser"
	@echo "  make test    - Build and test against all test files in EVIL.Ceres.LanguageTests"
	@echo "  make clean   - Remove generated files"
