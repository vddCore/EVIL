using System;
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
        
        [RuntimeModuleFunction("print")]
        [EvilDocFunction(
            "Writes string representations of the provided values, separated by the tabulator character, to the standard output stream.",
            Returns = "Length of the printed string, including the tabulator characters, if any.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument(
            "...",
            "__At least 1__ value to be printed.",
            CanBeNil = true
        )]
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
        
        [RuntimeModuleFunction("println")]
        [EvilDocFunction(
            "Writes string representations of the provided values, separated by the tabulator character, followed by the platform's new line sequence, to the standard output stream.",
            Returns = "Length of the printed string, including the tabulator characters, if any, and the new line sequence.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument(
            "...",
            "An arbitrary (can be none) amount of values to be printed.",
            CanBeNil = true
        )]
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