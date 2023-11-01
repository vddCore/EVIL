using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public partial class FsModuleTest
    {
        [Test]
        public void FileExists()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.exists('{tmpPath}');"
            );
            
            File.Delete(tmpPath);
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
        }

        [Test]
        public void FileDelete()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.delete('{tmpPath}');"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            File.Exists(tmpPath).ShouldBe(false);
        }
        
        [Test]
        public void FileCopy()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var tmpPath2 = tmpPath + "2";
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.copy(" +
                $"  '{tmpPath}'," +
                $"  '{tmpPath2}'" +
                $");"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            
            File.Exists(tmpPath).ShouldBe(true);
            File.Exists(tmpPath2).ShouldBe(true);

            File.Delete(tmpPath);
            File.Delete(tmpPath2);
        }
        
        [Test]
        public void FileMove()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var tmpPath2 = tmpPath + "2";
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.move(" +
                $"  '{tmpPath}'," +
                $"  '{tmpPath2}'" +
                $");"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            
            File.Exists(tmpPath).ShouldBe(false);
            File.Exists(tmpPath2).ShouldBe(true);

            File.Delete(tmpPath2);
        }

        [Test]
        public void FileLines()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            using (var sw = new StreamWriter(tmpPath))
            {
                sw.WriteLine("line 1");
                sw.WriteLine("line 2");
                sw.WriteLine("line 3");
            }

            var result = EvilTestResult(
                $"fn test() -> fs.file.get_lines('{tmpPath}');"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            
            var array = result.Array!;
            array.Length.ShouldBe(3);
            
            for (var i = 0; i < array.Length; i++)
            {
                array[i].ShouldBe($"line {i+1}");
            }
            
            File.Delete(tmpPath);
        }

        [Test]
        public void FileOpen()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            File.Create(tmpPath).Dispose();
            
            var result = EvilTestResult(
                $"fn test() -> fs.file.open('{tmpPath}');"
            );

            result.Type.ShouldBe(DynamicValueType.NativeObject);
            result.NativeObject.ShouldBeAssignableTo<Stream>();

            ((Stream)result.NativeObject)!.Dispose();

            File.Delete(tmpPath);
        }
        
        [Test]
        public void FileClose()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            File.Create(tmpPath).Dispose();
            
            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}');" +
                $"  fs.file.close(stream);" +
                $"  ret stream;" +
                $"}}"
            );

            result.Type.ShouldBe(DynamicValueType.NativeObject);
            result.NativeObject.ShouldBeAssignableTo<Stream>();

            Should.Throw<ObjectDisposedException>(() =>
            {
                _ = ((Stream)result.NativeObject)!.Read(new byte[4]);
            });

            File.Delete(tmpPath);
        }
        
        [Test]
        public void FileSeek()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var stream = File.Create(tmpPath);
            stream.Write(Encoding.UTF8.GetBytes("hello world uwu"));
            stream.Dispose();
            
            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}');" +
                $"  val retval = fs.file.seek(stream, 8, fs.origin.BEGIN);" +
                $"  ret array() {{ stream, retval }};" +
                $"}}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var array = result.Array!;
            array.Length.ShouldBe(2);
            array[0].Type.ShouldBe(DynamicValueType.NativeObject);
            array[0].NativeObject.ShouldBeAssignableTo<Stream>();
            var innerStream = (Stream)array[0].NativeObject!;
            array[1].Type.ShouldBe(DynamicValueType.Number);
            var position = array[1].Number;

            innerStream.Position.ShouldBe(8);
            position.ShouldBe(8);

            innerStream.Dispose();
            
            File.Delete(tmpPath);
        }
        
        [Test]
        public void FileTell()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var stream = File.Create(tmpPath);
            stream.Write(Encoding.UTF8.GetBytes("hello world uwu"));
            stream.Dispose();
            
            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}');" +
                $"  fs.file.seek(stream, 8, fs.origin.BEGIN);" +
                $"  val retval = fs.file.tell(stream);" +
                $"  fs.file.close(stream);" +
                $"  ret retval;" +
                $"}}"
            );

            result.Type.ShouldBe(DynamicValueType.Number);
            result.ShouldBe(8);
            
            File.Delete(tmpPath);
        }

        [Test]
        public void FileWrite()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}', 'w+');" +
                $"  val result = fs.file.write(" +
                $"      stream, " +
                $"      array() {{ 0x21, 0x37, 0x42, 0x69 }}" +
                $"  );" +
                $"" +
                $"  ret array() {{ result, stream }};" +
                $"}}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var array = result.Array!;
            array.Length.ShouldBe(2);

            array[0].Type.ShouldBe(DynamicValueType.Boolean);
            array[0].ShouldBe(true);

            array[1].Type.ShouldBe(DynamicValueType.NativeObject);
            array[1].NativeObject.ShouldBeAssignableTo<Stream>();
            
            ((Stream)array[1].NativeObject!).Dispose();

            var bytes = File.ReadAllBytes(tmpPath);
            bytes[0].ShouldBe((byte)0x21);
            bytes[1].ShouldBe((byte)0x37);
            bytes[2].ShouldBe((byte)0x42);
            bytes[3].ShouldBe((byte)0x69);
            
            File.Delete(tmpPath);
        }

        [Test]
        public void FileWriteString()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}', 'w+');" +
                $"  val result = fs.file.write_s(stream, 'hello world uwu');" +
                $"  fs.file.close(stream);" +
                $"  ret result;" +
                $"}}"
            );

            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);

            var text = File.ReadAllText(tmpPath);
            text.ShouldBe("hello world uwu");

            File.Delete(tmpPath);
        }

        [Test]
        public void FileRead()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var stream = File.OpenWrite(tmpPath);
            stream.Write(Encoding.UTF8.GetBytes("hello world"));
            stream.Dispose();
            
            var result = EvilTestResult(
                $"fn test() {{" +
                $"  val stream = fs.file.open('{tmpPath}', 'r');" +
                $"  val arr = array(16); " +
                $"  val read_cnt = fs.file.read(stream, arr);" +
                $"  fs.file.close(stream);" +
                $"  ret array() {{ read_cnt, arr }};" +
                $"}}"
            );
            
            result.Type.ShouldBe(DynamicValueType.Array);
            var array = result.Array!;
            array.Length.ShouldBe(2);
            
            array[0].Type.ShouldBe(DynamicValueType.Number);
            array[0].ShouldBe(11);

            array[1].Type.ShouldBe(DynamicValueType.Array);
            var innerArray = array[1].Array!;
            innerArray.Length.ShouldBe(16);

            var bytes = new byte[(int)array[0].Number];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)innerArray[i].Number;
            }

            var str = Encoding.UTF8.GetString(bytes);
            str.ShouldBe("hello world");

            File.Delete(tmpPath);
        }
    }
}