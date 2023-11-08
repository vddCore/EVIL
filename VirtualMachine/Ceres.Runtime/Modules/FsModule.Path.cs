using System;
using System.IO;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
        [RuntimeModuleGetter("path.sep")]
        private static DynamicValue GetPathSeparator(DynamicValue key)
            => Path.PathSeparator;

        [RuntimeModuleGetter("path.alt_sep")]
        private static DynamicValue GetAltPathSeparator(DynamicValue key)
            => Path.AltDirectorySeparatorChar;

        [RuntimeModuleGetter("path.bad_path_chars")]
        private static DynamicValue GetRestrictedPathChars(DynamicValue key)
        {
            var invalidPathChars = Path.GetInvalidPathChars();
            var array = new Array(invalidPathChars.Length);

            for (var i = 0; i < invalidPathChars.Length; i++)
                array[i] = invalidPathChars[i];

            return array;
        }

        [RuntimeModuleGetter("path.bad_fname_chars")]
        private static DynamicValue GetRestrictedNameChars(DynamicValue key)
        {
            var invalidNameChars = Path.GetInvalidFileNameChars();
            var array = new Array(invalidNameChars.Length);

            for (var i = 0; i < invalidNameChars.Length; i++)
                array[i] = invalidNameChars[i];

            return array;
        }
        
        [RuntimeModuleGetter("path.temp_dir")]
        private static DynamicValue PathGetTempDirPath(DynamicValue key)
        {
            try
            {
                ClearError();
                return Path.GetTempPath();
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleGetter("path.rand_fname")]
        private static DynamicValue PathGetRandomFileName(DynamicValue key)
            => Path.GetRandomFileName();

        [RuntimeModuleFunction("path.cmb")]
        private static DynamicValue PathCombine(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2);

            var pathSegments = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                args.ExpectStringAt(i, out pathSegments[i]);
            }

            try
            {
                ClearError();
                return Path.Combine(pathSegments).Replace('\\', '/');
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("path.get_fname")]
        private static DynamicValue PathGetFileName(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path)
                .OptionalBooleanAt(1, false, out var withoutExtension);

            try
            {
                ClearError();

                if (withoutExtension)
                {
                    return Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    return Path.GetFileName(path);
                }
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("path.get_dname")]
        private static DynamicValue PathGetDirectoryName(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.GetDirectoryName(path)!.Replace('\\', '/');
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("path.exists")]
        private static DynamicValue PathExists(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.Exists(path);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
        
        [RuntimeModuleFunction("path.get_ext")]
        private static DynamicValue PathGetExtension(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.GetExtension(path);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
        
        [RuntimeModuleFunction("path.has_ext")]
        private static DynamicValue PathHasExtension(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.HasExtension(path);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
        
        [RuntimeModuleFunction("path.chg_ext")]
        private static DynamicValue PathChangeExtension(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path)
                .ExpectStringAt(1, out var newExt);

            try
            {
                ClearError();
                return Path.ChangeExtension(path, newExt);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
        
        [RuntimeModuleFunction("path.rm_ext")]
        private static DynamicValue PathRemoveExtension(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.ChangeExtension(path, null);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
        
        [RuntimeModuleFunction("path.get_full")]
        private static DynamicValue PathGetFullPath(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                ClearError();
                return Path.GetFullPath(path);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }
    }
}