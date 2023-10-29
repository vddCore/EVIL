using System;
using System.IO;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule : RuntimeModule
    {
        [RuntimeModuleGetter("path.sep", ReturnType = DynamicValueType.String)]
        private static DynamicValue GetPathSeparator(DynamicValue key)
            => Path.PathSeparator;

        [RuntimeModuleGetter("path.alt_sep", ReturnType = DynamicValueType.String)]
        private static DynamicValue GetAltPathSeparator(DynamicValue key)
            => Path.AltDirectorySeparatorChar;

        [RuntimeModuleGetter("path.bad_path_chars", ReturnType = DynamicValueType.Array)]
        private static DynamicValue GetRestrictedPathChars(DynamicValue key)
        {
            var invalidPathChars = Path.GetInvalidPathChars();
            var array = new Array(invalidPathChars.Length);

            for (var i = 0; i < invalidPathChars.Length; i++)
                array[i] = invalidPathChars[i];

            return array;
        }

        [RuntimeModuleGetter("path.bad_name_chars", ReturnType = DynamicValueType.Array)]
        private static DynamicValue GetRestrictedNameChars(DynamicValue key)
        {
            var invalidNameChars = Path.GetInvalidFileNameChars();
            var array = new Array(invalidNameChars.Length);

            for (var i = 0; i < invalidNameChars.Length; i++)
                array[i] = invalidNameChars[i];

            return array;
        }
        
        [RuntimeModuleFunction("path.cmb", ReturnType = DynamicValueType.Array)]
        private static DynamicValue PathCombine(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2);

            var pathSegments = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                args.ExpectStringAt(i, out pathSegments[i]);
            }

            try
            {
                return Path.Combine(pathSegments).Replace('\\', '/');
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
    }
}