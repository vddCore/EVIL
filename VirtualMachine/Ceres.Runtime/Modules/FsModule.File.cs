using System;
using System.IO;
using System.Linq;
using System.Text;
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
        [RuntimeModuleFunction("file.exists")]
        private static DynamicValue FileExists(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path);
            return File.Exists(path);
        }

        [RuntimeModuleFunction("file.delete")]
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

        [RuntimeModuleFunction("file.move")]
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

        [RuntimeModuleFunction("file.get_lines")]
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
                return Nil;
            }
        }

        [RuntimeModuleFunction("file.open")]
        private static DynamicValue FileOpen(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var path)
                .OptionalStringAt(1, "rw", out var modeString);

            try
            {
                ClearError();

                var (access, mode) = AccessAndModeFromString(modeString);
                var stream = File.Open(path, mode, access, FileShare.ReadWrite);

                return FromObject(stream);
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return Nil;
            }
        }

        [RuntimeModuleFunction("file.close")]
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
        private static DynamicValue FileWrite(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNativeObjectAt(0, out Stream stream)
                .ExpectArrayAt(1, out var array)
                .OptionalIntegerAt(2, 0, out var offset)
                .OptionalIntegerAt(3, array.Length, out var count)
                .OptionalBooleanAt(4, true, out var autoFlush);

            try
            {
                ClearError();

                var bytes = new byte[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i].Type != DynamicValueType.Number)
                    {
                        throw new InvalidDataException(
                            "Only byte arrays can be written to a file stream."
                        );
                    }

                    bytes[i] = (byte)array[i].Number;
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
        private static DynamicValue FileWriteString(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNativeObjectAt(0, out Stream stream)
                .ExpectStringAt(1, out var str)
                .OptionalStringAt(2, "utf-8", out var encodingName)
                .OptionalBooleanAt(3, true, out var autoFlush);

            try
            {
                ClearError();

                var encoding = Encoding.GetEncoding(encodingName);
                stream.Write(encoding.GetBytes(str));

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
        private static DynamicValue FileRead(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNativeObjectAt(0, out Stream stream)
                .ExpectArrayAt(1, out var array)
                .OptionalIntegerAt(2, 0, out var offset)
                .OptionalIntegerAt(3, array.Length, out var count);

            try
            {
                ClearError();

                var bytes = new byte[array.Length];
                var readCount = stream.Read(bytes, (int)offset, (int)count);

                for (var i = 0; i < readCount; i++)
                {
                    array[i] = bytes[i];
                }

                return readCount;
            }
            catch (Exception e)
            {
                SetError(e.Message);
                return -1;
            }
        }
        
        [RuntimeModuleFunction("file.read_b")]
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
}