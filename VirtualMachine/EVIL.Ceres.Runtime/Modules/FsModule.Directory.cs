namespace EVIL.Ceres.Runtime.Modules;

using Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

using System;
using System.IO;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

public partial class FsModule
{
    [RuntimeModuleFunction("dir.exists")]
    [EvilDocFunction(
        "Checks if a directory exists at the given path.",
        Returns = "`true` if directory exists, `false` otherwise.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("path", "A path at which to check the existence of a directory.", DynamicValueType.String)]
    private static DynamicValue DirectoryExists(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path);
        return Directory.Exists(path);
    }

    [RuntimeModuleFunction("dir.create")]
    [EvilDocFunction(
        "Creates a directory and all subdirectories at a given path if they don't already exist.", 
        Returns = "A Table containing the last directory information or `nil` if it was a failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Table
    )]
    [EvilDocArgument("path", "A path to create the directory at.", DynamicValueType.String)]
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
            return DynamicValue.Nil;
        }
    }

    [RuntimeModuleFunction("dir.delete")]
    [EvilDocFunction(
        "Deletes a directory and all of its contents recursively __without confirmation__.",
        Returns = "`true` if the operation was successful, `false` otherwise.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("path", "A path specifying the directory to be deleted.", DynamicValueType.String)]
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

    [RuntimeModuleFunction("dir.get_files")]
    [EvilDocFunction(
        "Attempts to retrieve a list of files present in the directory specified by the given path.",
        Returns = "An Array containing the list of full paths to files in the specified directory, or `nil` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Array
    )]
    [EvilDocArgument("path", "A path specifying the directory to list the files of.", DynamicValueType.String)]
    [EvilDocArgument(
        "search_pattern",
        "An pattern to match the paths with while listing the files. Wildcard '*' is supported.",
        DynamicValueType.String,
        DefaultValue = "*"
    )]
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
            return DynamicValue.Nil;
        }
    }

    [RuntimeModuleFunction("dir.get_dirs")]
    [EvilDocFunction(
        "Attempts to retrieve a list of sub-directories present in the directory specified by the given path.",
        Returns = "An Array containing the list of full paths to sub-directories in the specified directory, or `nil` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Array
    )]
    [EvilDocArgument("path", "A path specifying the directory to list the sub-directories of.", DynamicValueType.String)]
    [EvilDocArgument(
        "search_pattern",
        "A pattern to match the paths with while listing the sub-directories. Wildcard '*' is supported.",
        DynamicValueType.String,
        DefaultValue = "*"
    )]
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
            return DynamicValue.Nil;
        }
    }

    [RuntimeModuleFunction("dir.copy")]
    [EvilDocFunction(
        "Attempts to recursively copy a directory from the source path into the target path.",
        Returns = "`true` on operation success, `false` when the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("source_path", "A path specifying the directory whose contents of are to be copied.", DynamicValueType.String)]
    [EvilDocArgument("target_path", "A path specifying the directory to copy the contents of source directory into.", DynamicValueType.String)]
    [EvilDocArgument(
        "overwrite_existing", 
        "`true` to overwrite existing files if found, `false` to fail if a file exists at the target path.",
        DynamicValueType.Boolean,
        DefaultValue = "false"
    )]
    private static DynamicValue DirectoryCopy(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var sourcePath)
            .ExpectStringAt(1, out var targetPath)
            .OptionalBooleanAt(2, false, out var overwriteExisting);

        try
        {
            var sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            var targetDirectoryInfo = new DirectoryInfo(targetPath);

            CopyDirectoryRecursively(sourceDirectoryInfo, targetDirectoryInfo, overwriteExisting);

            ClearError();
            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }

    [RuntimeModuleFunction("dir.move")]
    [EvilDocFunction(
        "Attempts to move a directory from the source path to the target path.",
        Returns = "`true` on operation success, `false` when the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("source_path", "A path specifying the directory to be moved.", DynamicValueType.String)]
    [EvilDocArgument("target_path", "A path to which the directory will be moved.", DynamicValueType.String)]
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