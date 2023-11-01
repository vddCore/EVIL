using System;
using System.IO;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
        [RuntimeModuleFunction("dir.exists", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue DirectoryExists(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);
            return Directory.Exists(path);
        }

        [RuntimeModuleFunction("dir.create", ReturnType = DynamicValueType.Table)]
        private static DynamicValue DirectoryCreate(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                var info = Directory.CreateDirectory(path);

                ClearError();
                return new Table
                {
                    ["name"] = info.Name,
                    ["path"] = info.FullName
                };
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("dir.delete", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue DirectoryDelete(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);

            try
            {
                Directory.Delete(path, true);
                
                ClearError();
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }

        [RuntimeModuleFunction("dir.get_files", ReturnType = DynamicValueType.Array)]
        private static DynamicValue DirectoryGetFiles(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path)
                .OptionalStringAt(1, "*", out var searchPattern);

            try
            {
                var files = Directory.GetFiles(path, searchPattern);
                var array = new Array(files.Length);

                for (var i = 0; i < files.Length; i++)
                    array[i] = files[i].Replace('\\', '/');

                ClearError();
                return array;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("dir.get_dirs", ReturnType = DynamicValueType.Array)]
        private static DynamicValue DirectoryGetDirectories(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path)
                .OptionalStringAt(1, "*", out var searchPattern);

            try
            {
                var dirs = Directory.GetDirectories(path, searchPattern);
                var array = new Array(dirs.Length);

                for (var i = 0; i < dirs.Length; i++)
                    array[i] = dirs[i].Replace('\\', '/');

                ClearError();
                return array;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("dir.copy", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue DirectoryCopy(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var sourcePath)
                .ExpectStringAt(1, out var targetPath);

            try
            {
                var sourceDirectoryInfo = new DirectoryInfo(sourcePath);
                var targetDirectoryInfo = new DirectoryInfo(targetPath);

                CopyDirectoryRecursively(sourceDirectoryInfo, targetDirectoryInfo);

                ClearError();
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }

        [RuntimeModuleFunction("dir.move", ReturnType = DynamicValueType.Boolean)]
        private static DynamicValue DirectoryMove(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var sourcePath)
                .ExpectStringAt(1, out var targetPath);

            try
            {
                Directory.Move(sourcePath, targetPath);

                ClearError();
                return true;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return false;
            }
        }
    }
}