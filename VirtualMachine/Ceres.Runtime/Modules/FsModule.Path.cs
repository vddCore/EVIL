using System;
using System.IO;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
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