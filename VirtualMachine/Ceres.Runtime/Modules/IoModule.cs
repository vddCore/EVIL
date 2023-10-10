﻿using System;
using System.Linq;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public class IoModule : RuntimeModule
    {
        public override string FullyQualifiedName => "io";
        
        [RuntimeModuleFunction("print", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1);
            
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            return str.Length;
        }
        
        [RuntimeModuleFunction("println", ReturnType = DynamicValueType.Number)]
        private static DynamicValue PrintLine(Fiber _, params DynamicValue[] args)
        {
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            Console.Write(Environment.NewLine);
            
            return str.Length + Environment.NewLine.Length;
        }
    }
}