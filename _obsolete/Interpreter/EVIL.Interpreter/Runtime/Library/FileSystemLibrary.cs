using System.IO;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("fs")]
    public class FileSystemLibrary
    {
        [ClrFunction("get_lines")]
        public static DynValue GetLines(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var filePath = args[0].String;

            try
            {
                var table = new Table();

                using (var sr = new StreamReader(filePath))
                {
                    var data = sr.ReadToEnd().Split('\n');

                    for (var i = 0; i < data.Length; i++)
                    {
                        table[i] = new DynValue(data[i]);
                    }
                }

                return new DynValue(table);
            }
            catch
            {
                return DynValue.Zero;
            }
        }

        [ClrFunction("file_exists")]
        public static DynValue FileExists(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var filePath = args[0].String;

            return new DynValue(File.Exists(filePath));
        }

        [ClrFunction("dir_exists")]
        public static DynValue DirectoryExists(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var filePath = args[0].String;

            return new DynValue(Directory.Exists(filePath));
        }
    }
}