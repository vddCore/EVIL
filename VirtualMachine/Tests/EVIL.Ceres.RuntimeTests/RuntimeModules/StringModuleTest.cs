using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace EVIL.Ceres.RuntimeTests.RuntimeModules
{
    public class StringModuleTest : ModuleTest<StringModule>
    {
        [Test]
        public void ToChar()
        {
            EvilTestResult(
                "fn test() -> str.chr(0x65);"
            ).ShouldBe("e");
        }
        
        [Test]
        public void Explode()
        {
            var array = (Array)EvilTestResult(
                "fn test() -> str.explode('abcdef');"
            );

            array[0].ShouldBe("a");
            array[1].ShouldBe("b");
            array[2].ShouldBe("c");
            array[3].ShouldBe("d");
            array[4].ShouldBe("e");
            array[5].ShouldBe("f");
        }
        
        [Test]
        public void Split()
        {
            var array = (Array)EvilTestResult(
                "fn test() -> str.spl('abc|def|ghi', '|');"
            );

            array[0].ShouldBe("abc");
            array[1].ShouldBe("def");
            array[2].ShouldBe("ghi");
        }

        [Test]
        public void JoinEmpty()
        {
            EvilTestResult(
                "fn test() -> str.join(str.empty);"
            ).ShouldBe(string.Empty);
        }

        [Test]
        public void JoinVarious()
        {
            EvilTestResult(
                "fn test() -> str.join('|', 1, 'test', true);"
            ).ShouldBe("1|test|true");
        }

        [Test]
        public void Repeat()
        {
            EvilTestResult(
                "fn test() -> str.repeat('test', 4);"
            ).ShouldBe("testtesttesttest");
        }

        [Test]
        public void IndexOf()
        {
            EvilTestResult(
                "fn test() -> str.index_of('hello, world!', 'w');"
            ).ShouldBe(7);
        }

        [Test]
        public void LastIndexOf()
        {
            EvilTestResult(
                "fn test() -> str.last_index_of('hello hello hello hello', 'hello');"
            ).ShouldBe(18);
        }

        [Test]
        public void IsWhiteSpace()
        {
            EvilTestResult(
                "fn test() -> str.is_whitespace('');"
            ).ShouldBe(true);
        }

        [Test]
        public void LeftPad()
        {
            EvilTestResult(
                "fn test() -> str.lpad('2137', '0', 8);"
            ).ShouldBe("00002137");
        }

        [Test]
        public void RightPad()
        {
            EvilTestResult(
                "fn test() -> str.rpad('2137', '0', 8);"
            ).ShouldBe("21370000");
        }

        [Test]
        public void LeftTrim()
        {
            EvilTestResult(
                "fn test() -> str.ltrim('21372', '2');"
            ).ShouldBe("1372");
        }

        [Test]
        public void RightTrim()
        {
            EvilTestResult(
                "fn test() -> str.rtrim('21372', '2', '7');"
            ).ShouldBe("213");
        }

        [Test]
        public void Trim()
        {
            EvilTestResult(
                "fn test() -> str.trim('21372', '2');"
            ).ShouldBe("137");
        }

        [Test]
        public void UpperCase()
        {
            EvilTestResult(
                "fn test() -> str.ucase('i was lowercase. ą.');"
            ).ShouldBe("I WAS LOWERCASE. Ą.");
        }

        [Test]
        public void LowerCase()
        {
            EvilTestResult(
                "fn test() -> str.lcase('I WAS UPPERCASE ONCE. Ą.');"
            ).ShouldBe("i was uppercase once. ą.");
        }

        [Test]
        public void SubstringFrom()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 7);"
            ).ShouldBe("world");
        }

        [Test]
        public void SubstringFromWithNil()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 7, nil, true);"
            ).ShouldBe("world");
        }

        [Test]
        public void SubstringFromTo()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 3, 7, true);"
            ).ShouldBe("lo, w");
        }
        
        [Test]
        public void SubstringFromTo2()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 0, 0, true);"
            ).ShouldBe("h");
        }
        
        [Test]
        public void SubstringFromTo3()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 0, 1, true);"
            ).ShouldBe("he");
        }
        
        [Test]
        public void SubstringFromTo4()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 4, 15, true);"
            ).ShouldBe("o, world");
        }

        [Test]
        public void StartsWith()
        {
            EvilTestResult(
                "fn test() -> str.starts_with('hello, world', 'hel');"
            ).ShouldBe(true);
        }

        [Test]
        public void EndsWith()
        {
            EvilTestResult(
                "fn test() -> str.ends_with('hello, world', 'orld');"
            ).ShouldBe(true);
        }

        [Test]
        public void RegexMatch()
        {
            var array = (Array)EvilTestResult(
                "fn test() -> str.rmatch(" +
                "   'This 21 is 37 an example 00 of 12 a test string 99.'," +
                @"   '(\\w+) (\\d+)'" +
                ");"
            );

            array.Length.ShouldBe(5);
            array[0].Table!["value"].ShouldBe("This 21");
            array[1].Table!["value"].ShouldBe("is 37");
            array[2].Table!["value"].ShouldBe("example 00");
            array[3].Table!["value"].ShouldBe("of 12");
            array[4].Table!["value"].ShouldBe("string 99");
        }
    }
}