namespace EVIL.Ceres.Runtime.Modules;

using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

using Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

using System;
using System.IO;
using System.Linq;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

public partial class FsModule
{
    [RuntimeModuleFunction("file.exists")]
    [EvilDocFunction(
        "Checks if a file exists at the given path.",
        Returns = "`true` if a file exists at the provided path, `false` otherwise.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument(
        "path",
        "A path at which to check the existence of a file.", 
        DynamicValueType.Boolean
    )]
    private static DynamicValue FileExists(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path);
        return File.Exists(path);
    }

    [RuntimeModuleFunction("file.delete")]
    [EvilDocFunction(
        "Deletes a file at the given path.",
        Returns = "`true` if the operation succeeded, `false` otherwise.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("path", "A path specifying the file to be deleted.", DynamicValueType.String)]
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

    [RuntimeModuleFunction("file.copy")]
    [EvilDocFunction(
        "Copies a file from one path to another.",
        Returns = "`true` if the operation succeeded, `false` otherwise.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("source_path", "A path specifying the file to be copied.", DynamicValueType.String)]
    [EvilDocArgument("target_path", "A path specifying the location to which the source file should be copied.", DynamicValueType.String)]
    [EvilDocArgument(
        "overwrite_existing",
        "`true` to overwrite an existing file if found, `false` to fail when that happens.", 
        DynamicValueType.Boolean,
        DefaultValue = "false"
    )]
    private static DynamicValue FileCopy(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var sourcePath)
            .ExpectStringAt(1, out var targetPath)
            .OptionalBooleanAt(2, false, out var overwriteExisting);

        try
        {
            ClearError();
            File.Copy(sourcePath, targetPath, overwriteExisting);
            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }

    [RuntimeModuleFunction("file.move")]
    [EvilDocFunction(
        "Moves a file from one path to another.",
        Returns = "`true` if the operation succeeded, `false` otherwise.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("source_path", "A path specifying the file to be moved.", DynamicValueType.String)]
    [EvilDocArgument("target_path", "A path specifying the location to which the source file should be moved.", DynamicValueType.String)]
    [EvilDocArgument(
        "overwrite_existing",
        "`true` to overwrite an existing file if found, `false` to fail when that happens.", 
        DynamicValueType.Boolean,
        DefaultValue = "false"
    )]
    private static DynamicValue FileMove(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var sourcePath)
            .ExpectStringAt(1, out var targetPath)
            .OptionalBooleanAt(2, false, out var overwriteExisting);

        try
        {
            ClearError();
            File.Move(sourcePath, targetPath, overwriteExisting);
            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }

    [RuntimeModuleFunction("file.get_lines")]
    [EvilDocFunction(
        "Reads lines of text from a file.",
        Returns = "An Array containing the lines read from the specified file, or `nil` if the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Array
    )]
    [EvilDocArgument("path", "A path specifying the file to read the lines of text from.", DynamicValueType.String)]
    [EvilDocArgument(
        "encoding",
        "A name of the encoding to read the file as.",
        DynamicValueType.String,
        DefaultValue = "utf-8"
    )]
    private static DynamicValue FileGetLines(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path)
            .OptionalStringAt(1, "utf-8", out var encoding);

        try
        {
            ClearError();

            var lines = File.ReadLines(path, Encoding.GetEncoding(encoding)).ToArray();
            var array = new Array(lines.Length);

            for (var i = 0; i < lines.Length; i++)
                array[i] = lines[i];

            return array;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return Nil;
        }
    }

    [RuntimeModuleFunction("file.get_text")]
    [EvilDocFunction(
        "Reads a file and returns its contents as a String using the provided encoding, or `nil` if the operation fails.",
        Returns = "Contents of the read file.",
        ReturnType = DynamicValueType.String
    )]
    [EvilDocArgument("path", "A path specifying the file to read the text from.", DynamicValueType.String)]
    [EvilDocArgument(
        "encoding",
        "A name of the encoding to read the file as.",
        DynamicValueType.String,
        DefaultValue = "utf-8"
    )]
    private static DynamicValue FileGetText(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path)
            .OptionalStringAt(1, "utf-8", out var encoding);

        try
        {
            ClearError();
            return File.ReadAllText(path, Encoding.GetEncoding(encoding));
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return Nil;
        }
    }

        
    [RuntimeModuleFunction("file.open")]
    [EvilDocFunction(
        "Attempts to open a file stream given a file path and an optional file mode string.  \n\n" +
        "**File modes**  \n" +
        "> `r`  \n" +
        "> Open existing, fail if doesn't exist, read-only.\n\n" +
        "> `r+`  \n" +
        "> Open existing, fail if doesn't exist, read-write.\n\n" +
        "> `w`  \n" +
        "> Create new, overwrite if exists, write-only.\n\n" +
        "> `w+`  \n" +
        "> Create new, overwrite if exists, read-write.\n\n" +
        "> `a`  \n" +
        "> Open or create new file, write-only, seek to end.\n\n" +
        "> `a+`  \n" +
        "> Open or create new file, read-write.\n\n" +
        "> `t`  \n" +
        "> Truncate file, fail if doesn't exist, write-only.\n\n" +
        "> `rw`  \n" +
        "> Same as `a+`.\n\n" +
        "> `wr`  \n" +
        "> Same as `a+`.\n\n",
        Returns = "A NativeObject wrapping a stream handle or `nil` if the operation fails.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.NativeObject
    )]
    [EvilDocArgument("path", "A path specifying the file to open or create.", DynamicValueType.String)]
    [EvilDocArgument(
        "mode",
        "A string specifying the mode of the resulting file stream.",
        DynamicValueType.String,
        DefaultValue = "rw"
    )]
    private static DynamicValue FileOpen(Fiber _, params DynamicValue[] args)
    {
        args.ExpectStringAt(0, out var path)
            .OptionalStringAt(1, "rw", out var mode);

        try
        {
            ClearError();

            var (access, fileMode) = AccessAndModeFromString(mode);
            var stream = File.Open(path, fileMode, access, FileShare.ReadWrite);

            return FromObject(stream);
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return Nil;
        }
    }

    [RuntimeModuleFunction("file.close")]
    [EvilDocFunction(
        "Attempts to close a previously opened file stream.",
        Returns = "`true` if closed successfully, `false` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("stream", "A handle to the previously opened file stream.", DynamicValueType.NativeObject)]
    private static DynamicValue FileClose(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream);

        try
        {
            ClearError();
                
            stream.Close();
            stream.Dispose();
            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }

    [RuntimeModuleFunction("file.seek")]
    [EvilDocFunction(
        "Sets the position within the given file stream based on the given offset and origin.  \n\n" +
        "**Seek origins**  \n" +
        "> `fs.origin.BEGIN = 0`  \n" +
        "> The seek operation starts from offset 0 and moves forwards.\n\n" +
        "> `fs.origin.CURRENT = 1`  \n" +
        ">  The seek operation starts from the current position and moves forwards.\n\n" +
        "> `fs.origin.END = 2`  \n" +
        ">  The seek operation starts from the end and moves backwards.\n\n",
        Returns = "The new position within the given stream or `-1` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Number
    )]
    [EvilDocArgument("stream", "A handle to the file stream to seek through.", DynamicValueType.NativeObject)]
    [EvilDocArgument("offset", "An integer specifying the new position within the given stream.", DynamicValueType.Number)]
    [EvilDocArgument(
        "seek_origin",
        "An integer specifying the origin of the seek operation.",
        DynamicValueType.Number,
        DefaultValue = "fs.origin.BEGIN"
    )]
    private static DynamicValue FileSeek(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream)
            .ExpectIntegerAt(1, out var offset)
            .OptionalIntegerAt(2, (long)SeekOrigin.Begin, out var seekOrigin);

        try
        {
            ClearError();
            return stream.Seek(offset, (SeekOrigin)seekOrigin);
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return -1;
        }
    }
        
    [RuntimeModuleFunction("file.tell")]
    [EvilDocFunction(
        "Gets current position within the given file stream.",
        Returns = "Current position within the given file stream, or `-1` on failure.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Number
    )]
    [EvilDocArgument("stream", "A handle to an open file stream.", DynamicValueType.NativeObject)]
    private static DynamicValue FileTell(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream);

        try
        {
            ClearError();
            return stream.Position;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return -1;
        }
    }

    [RuntimeModuleFunction("file.write")]
    [EvilDocFunction(
        "Writes the provided data to the given file stream using the specified encoding.",
        Returns = "`true` if write operation was successful, `false` otherwise.  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("stream", "A handle to an open file stream.", DynamicValueType.NativeObject)]
    [EvilDocArgument("data", "An array of bytes to be written to the given file stream.", DynamicValueType.Array)]
    [EvilDocArgument(
        "offset", 
        "An integer specifying a zero-based offset in the data aray at which to begin copying bytes to the given stream.",
        DynamicValueType.Number,
        DefaultValue = "0"
    )]
    [EvilDocArgument(
        "count",
        "An integer specifying the number of bytes to be written to the given stream.",
        DynamicValueType.Number,
        DefaultValue = "#data"
    )]
    [EvilDocArgument(
        "auto_flush",
        "`true` to flush the stream immediately after writing the data, `false` to defer until the stream is closed.",
        DynamicValueType.Boolean,
        DefaultValue = "true"
    )]
    private static DynamicValue FileWrite(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream)
            .ExpectArrayAt(1, out var data)
            .OptionalIntegerAt(2, 0, out var offset)
            .OptionalIntegerAt(3, data.Length, out var count)
            .OptionalBooleanAt(4, true, out var autoFlush);

        try
        {
            ClearError();

            var bytes = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i].Type != DynamicValueType.Number)
                {
                    throw new InvalidDataException(
                        "Only byte arrays can be written to a file stream."
                    );
                }

                bytes[i] = (byte)data[i].Number;
            }

            stream.Write(bytes, (int)offset, (int)count);

            if (autoFlush)
            {
                stream.Flush();
            }

            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }
        
    [RuntimeModuleFunction("file.write_s")]
    [EvilDocFunction(
        "Writes the provided string to the given file stream using the specified encoding.",
        Returns = "`true` if the operation was successful, `false` if it failed  \n" +
                  "Check `fs.error` for failure details.",
        ReturnType = DynamicValueType.Boolean
    )]
    [EvilDocArgument("stream", "A handle to an open file stream.", DynamicValueType.NativeObject)]
    [EvilDocArgument("string", "A string to be written to the file stream.", DynamicValueType.String)]
    [EvilDocArgument("encoding", "Encoding to be used when writing the string to the stream.", DynamicValueType.String)]
    [EvilDocArgument(
        "auto_flush", 
        "`true` to flush the stream immediately after writing the data, `false` to defer until the stream is closed.",
        DynamicValueType.String,
        DefaultValue = "utf-8"
    )]
    private static DynamicValue FileWriteString(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream)
            .ExpectStringAt(1, out var str)
            .OptionalStringAt(2, "utf-8", out var encoding)
            .OptionalBooleanAt(3, true, out var autoFlush);

        try
        {
            ClearError();

            stream.Write(
                Encoding.GetEncoding(
                    encoding
                ).GetBytes(str)
            );

            if (autoFlush)
            {
                stream.Flush();
            }

            return true;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return false;
        }
    }

    [RuntimeModuleFunction("file.read")]
    [EvilDocFunction(
        "Reads bytes of data from an open stream into the provided array.",
        Returns = "The amount of bytes that has been read from the stream or `-1` on failure.  \n" +
                  "Check `fs.error` for failure details.  \n" +
                  "> **NOTE**  \n" +
                  "> The returned value may be smaller than the requested amount of data if the stream ends early.",
        ReturnType = DynamicValueType.Number
    )]
    [EvilDocArgument("stream", "A handle to an open file stream.", DynamicValueType.NativeObject)]
    [EvilDocArgument("data", "An array to read the data into.", DynamicValueType.Array)]
    [EvilDocArgument(
        "offset", 
        "An integer specifying a zero-based byte offset in the provided data array at which to begin storing the data read from the given stream.",
        DynamicValueType.Number,
        DefaultValue = "0"
    )]
    [EvilDocArgument(
        "count",
        "An integer specifying the amount of data to be read from the file stream.",
        DynamicValueType.Number,
        DefaultValue = "#data"
    )]
    private static DynamicValue FileRead(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream)
            .ExpectArrayAt(1, out var data)
            .OptionalIntegerAt(2, 0, out var offset)
            .OptionalIntegerAt(3, data.Length, out var count);

        try
        {
            ClearError();

            var length = 0;
            while (length < count)
            {
                if (offset + length >= data.Length)
                {
                    break;
                }

                var fragment = stream.ReadByte();
                if (fragment < 0)
                {
                    break;
                }

                data[offset + length] = fragment;
                length++;
            }
                
            return length;
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return -1;
        }
    }
        
    [RuntimeModuleFunction("file.read_b")]
    [EvilDocFunction(
        "Reads a single byte from an open file stream.",
        Returns = "A byte value that has been read from the provided file stream, or `-1` on either failure or end-of-stream.  \n" +
                  "Check `fs.error` for failure details and to distinguish a failure from an end-of-stream condition.",
        ReturnType = DynamicValueType.Number
    )]
    [EvilDocArgument(
        "stream", 
        "A handle to an open file stream.",
        DynamicValueType.NativeObject
    )]
    private static DynamicValue FileReadByte(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNativeObjectAt(0, out Stream stream);

        try
        {
            ClearError();
            return stream.ReadByte();
        }
        catch (Exception e)
        {
            SetError(e.Message);
            return -1;
        }
    }
}