using System.Collections.Generic;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    public delegate IEnumerable<Chunk> IncludeProcessor(
        Compiler compiler,
        Script script,
        string includePath
    );
}