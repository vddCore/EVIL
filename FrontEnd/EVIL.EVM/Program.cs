using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine;
using Mono.Options;

namespace EVIL.EVM
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new EvmFrontEnd().Run(new[] { "-h" });
        }
    }
}