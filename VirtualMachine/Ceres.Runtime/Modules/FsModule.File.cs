using System;
using System.IO;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
        [RuntimeModuleFunction("file.exists", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue FileExists(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);
            return File.Exists(path);
        }

        [RuntimeModuleFunction("file.delete", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue FileDelete(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }

        [RuntimeModuleFunction("file.copy", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue FileCopy(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var sourcePath)
                .ExpectStringAt(1, out var targetPath);

            try
            {
                ClearError();
                File.Copy(sourcePath, targetPath, true);
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }
        
        [RuntimeModuleFunction("file.move", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue FileMove(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var sourcePath)
                .ExpectStringAt(1, out var targetPath);

            try
            {
                ClearError();
                File.Move(sourcePath, targetPath, true);
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }

        [RuntimeModuleFunction("file.get_lines", ReturnType = DynamicValueType.Array)]
        private static DynamicValue FileGetLines(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                
                var lines = File.ReadLines(path, Encoding.UTF8).ToArray();
                var array = new Array(lines.Length);

                for (var i = 0; i < lines.Length; i++)
                    array[i] = lines[i];

                return array;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }
    }
}