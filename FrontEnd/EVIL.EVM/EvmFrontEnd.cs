using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.Runtime;
using Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using Mono.Options;

namespace EVIL.EVM
{
    public partial class EvmFrontEnd
    {
        private static Compiler _compiler = new();
        private static CeresVM _vm = new();

        private static OptionSet _options = new()
        {
            { "h|help", "display this message and quit.", (h) => _displayHelpAndQuit = h != null },
            { "v|version", "display compiler and VM version information.", (v) => _displayVersionAndQuit = v != null },
            { "c|compile-only", "don't run the source file; compile it to an executable instead.", (c) => _compileOnly = c != null },
            { "o|output=", "output the compiled script to the provided one, default is 'a'.", (o) => _outputName = o },
            { "I|include-dir=", "add include directory to the list of search paths.", (I) => _includeSearchPaths.Add(I) },
            { "d|disassemble", "disassemble the executed script.", (d) => _disassembleCompiledScript = d != null },
        };

        private static bool _displayHelpAndQuit = false;
        private static bool _displayVersionAndQuit = false;
        private static bool _compileOnly = false;
        private static string _outputName = "a";
        private static HashSet<string> _includeSearchPaths = new();
        private static bool _disassembleCompiledScript = false;

        public void Run(string[] args)
        {
            var filePaths = InitializeOptions(args);

            if (_displayHelpAndQuit)
            {
                Terminate(writeHelp: true);
            }

            if (_displayVersionAndQuit)
            {
                Terminate(BuildVersionBanner());
            }

            if (!filePaths.Any())
            {
                Terminate(
                    "fatal: no input files",
                    exitCode: ExitCode.NoInputFiles
                );
            }

            _compiler.RegisterIncludeProcessor(ProcessIncludeFile);
        }

        private List<string> InitializeOptions(string[] args)
        {
            List<string> fileNames = new();

            try
            {
                var extra = _options.Parse(args);

                if (extra != null)
                {
                    fileNames.AddRange(extra);
                }
            }
            catch (OptionException oe)
            {
                Terminate(
                    $"fatal: {oe.Message}. run with `--help' to see usage instructions",
                    exitCode: ExitCode.GenericError
                );
            }

            return fileNames;
        }

        private static IEnumerable<Chunk> ProcessIncludeFile(
            Compiler requestingCompiler,
            Script scriptBeingBuilt,
            string includedFilePath)
        {
            foreach (var path in _includeSearchPaths)
            {
            }

            return new List<Chunk>();
        }
    }
}