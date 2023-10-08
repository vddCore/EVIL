using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(IncludeStatement includeStatement)
        {
            if (!_includeProcessors.Any())
            {
                Log.EmitWarning(
                    $"Attempt to include '{includeStatement.Path}' but no include processors found.",
                    CurrentFileName,
                    EvilMessageCode.IncludeFoundButNoIncludeProcessorsPresent,
                    includeStatement.Line,
                    includeStatement.Column
                );

                return;
            }

            foreach (var processor in _includeProcessors)
            {
                IEnumerable<Chunk> chunks;
                try
                {
                    chunks = processor(this, _script, includeStatement.Path);
                }
                catch (Exception e)
                {
                    Log.TerminateWithFatal(
                        $"Failure while processing the included filed '{includeStatement.Path}'.",
                        CurrentFileName,
                        EvilMessageCode.IncludeProcessorThrew,
                        includeStatement.Line,
                        includeStatement.Column,
                        e
                    );
                    return;
                }

                foreach (var chunk in chunks)
                {
                    _script.AddChunk(chunk, out var replacedExisting, out var conflictingChunk);

                    if (replacedExisting)
                    {
                        Log.EmitWarning(
                            $"Redefining function '{conflictingChunk.Name}' initially defined in '{conflictingChunk.DebugDatabase.DefinedInFile}' " +
                            $"with the one defined in '{includeStatement.Path}'.",
                            CurrentFileName,
                            EvilMessageCode.IncludedFileRedefinedExistingChunk,
                            includeStatement.Line,
                            includeStatement.Column
                        );
                    }
                }
            }
        }
    }
}