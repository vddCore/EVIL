using System.IO;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;
using EVIL.RT;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("fs")]
    public class FileSystemLibrary
    {
        [ClrFunction("get_lines", RuntimeAlias = "fs.get_lines")]
        public static DynamicValue GetLines(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var filePath = args[0].String;

            try
            {
                var table = new Table();

                using (var sr = new StreamReader(filePath))
                {
                    var data = sr.ReadToEnd().Split('\n');

                    for (var i = 0; i < data.Length; i++)
                    {
                        table.SetByNumber(i, new DynamicValue(data[i]));
                    }
                }

                return new DynamicValue(table);
            }
            catch
            {
                return DynamicValue.Zero;
            }
        }

        [ClrFunction("fex", RuntimeAlias = "fs.fex")]
        public static DynamicValue FileExists(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var filePath = args[0].String;

            return new DynamicValue(File.Exists(filePath));
        }

        [ClrFunction("dex", RuntimeAlias = "fs.dex")]
        public static DynamicValue DirectoryExists(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var filePath = args[0].String;

            return new DynamicValue(Directory.Exists(filePath));
        }
    }
}