namespace EVIL.Ceres.Runtime.Modules;

using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

using Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

using System;
using System.IO;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

public partial class FsModule
{
    [RuntimeModuleGetter("path.sep")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "A platform-specific character used to separate path strings.",
        ReturnType = DynamicValueType.String
    )]
    private static DynamicValue GetPathSeparator(DynamicValue _)
        => Path.PathSeparator;

    [RuntimeModuleGetter("path.alt_sep")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "An alternate platform-specific character used to separate path strings.",
        ReturnType = DynamicValueType.String
    )]
    private static DynamicValue GetAltPathSeparator(DynamicValue _)
        => Path.AltDirectorySeparatorChar;

    [RuntimeModuleGetter("path.bad_path_chars")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "An array containing characters that are not allowed in paths.",
        ReturnType = DynamicValueType.Array
    )]
    private static DynamicValue GetRestrictedPathChars(DynamicValue _)
    {
        var invalidPathChars = Path.GetInvalidPathChars();
        var array = new ExecutionEngine.Collections.Array(invalidPathChars.Length);

        for (var i = 0; i < invalidPathChars.Length; i++)
            array[i] = invalidPathChars[i];

        return array;
    }

    [RuntimeModuleGetter("path.bad_fname_chars")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "An array containing characters that are not allowed in file names.",
        ReturnType = DynamicValueType.Array
    )]
    private static DynamicValue GetRestrictedNameChars(DynamicValue _)
    {
        var invalidNameChars = Path.GetInvalidFileNameChars();
        var array = new Array(invalidNameChars.Length);

        for (var i = 0; i < invalidNameChars.Length; i++)
            array[i] = invalidNameChars[i];

        return array;
    }
        
    [RuntimeModuleGetter("path.temp_dir")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "The path of the current user's temporary directory, or `nil` on failure.  \n" +
        "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.String
    )]
    private static DynamicValue PathGetTempDirPath(DynamicValue _)
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
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "Each indexing attempt of this property will return a randomly generated name for a file system entity.",
        ReturnType = DynamicValueType.String
    )]
    private static DynamicValue PathGetRandomFileName(DynamicValue _)
        => Path.GetRandomFileName();

    [RuntimeModuleFunction("path.cmb")]
    [EvilDocFunction(
        "Combines the provided strings into a path.",
        Returns = "The combined paths, or `nil` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Number,
        IsVariadic = true
    )]
    [EvilDocArgument(
        "...",
        "___At least 2___ strings to be combined into a path.",
        DynamicValueType.String
    )]
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
    [EvilDocFunction(
        "Returns the file name with or without extension from the specified path.",
        Returns = "The string after the last directory separator in the provided path.  \n" +
                  "Will return an empty string if the last character is a directory separator.  \n" +
                  "Will return `nil` on failure - check `fs.error` for failure details.",
        ReturnType = DynamicValueType.String
    )]
    [EvilDocArgument(
        "without_extension",
        "`true` to get the file name without extension, `false` to preserve it.",
        DynamicValueType.Boolean,
        DefaultValue = "false"
    )]
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
    [EvilDocFunction(
        "Attempts to retrieve the path leading up to the parent directory of the last file system entity described by the given path.",
        Returns = "The parent directory of the last file system entity (e.g. `/var/lib/file` -> `/var/lib`), or `nil on failure.",
        ReturnType = DynamicValueType.String
    )]
    [EvilDocArgument("path", "A path to be transformed.", DynamicValueType.String)]
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
    [EvilDocFunction(
        "Checks if the given path leads to a valid file system entity.",
        Returns = "`true` if the given path leads to any valid file system entity. `false` otherwise, or when the operation fails.  \n" +
                  "`fs.error` is not set by this function, even on failure.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("path", "The path to be checked.")]
    private static DynamicValue PathExists(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path);

        ClearError();
        return Path.Exists(path);
    }
        
    [RuntimeModuleFunction("path.get_ext")]
    [EvilDocFunction(
        "Attempts to extract the extension part of the given path.",
        Returns = "The extension part of a given path, an empty string if no extension is present, or `nil` when the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.String)]
    [EvilDocArgument("path", "A path from which to extract the extension part.", DynamicValueType.String)]
    [EvilDocArgument(
        "with_dot",
        "`true` to include the dot with the extension, `false` to trim it away.",
        DynamicValueType.Boolean,
        DefaultValue = "true"
    )]
    private static DynamicValue PathGetExtension(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path)
            .OptionalBooleanAt(1, true, out var withDot);

        try
        {
            ClearError();
                
            var ext = Path.GetExtension(path);

            if (!withDot)
            {
                ext = ext.TrimStart('.');
            }

            return ext;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return Nil;
        }
    }
        
    [RuntimeModuleFunction("path.has_ext")]
    [EvilDocFunction(
        "Checks if the provided path contains extension information.",
        Returns = "`true` if the path ends with a `.` followed by at least 1 character, `false` if not and `nil` when the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("path", "A path to be checked.", DynamicValueType.String)]
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
    [EvilDocFunction(
        "Changes the extension of the given path.",
        Returns = "The modified path. If the path has no extension it will be appended.  \n" +
                  "Will return `nil` when the operation fails. Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.String
    )]
    [EvilDocArgument("path", "The path to be modified.", DynamicValueType.String)]
    [EvilDocArgument(
        "new_ext",
        "An extension (either dot-prefixed or not) to be applied to the given path. " +
        "Specify `nil` to remove an existing path extension instead.",
        DynamicValueType.String,
        CanBeNil = true
    )]
    private static DynamicValue PathChangeExtension(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path)
            .ExpectStringAt(1, out var newExt, allowNil: true); 

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
        
    [RuntimeModuleFunction("path.get_full")]
    [EvilDocFunction(
        "Attempts to retrieve an absolute path for the given path string.",
        Returns = "The fully qualified location of the provided path, e.g. `/var/lib/file.txt`, or `nil` when the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.String
    )]
    [EvilDocArgument("path", "A file or directory path of which to retrieve an absolute path for.", DynamicValueType.String)]
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