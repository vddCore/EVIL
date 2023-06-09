using Ceres.ExecutionEngine.Collections;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class StringModuleTest : ModuleTest<StringModule>
    {
        [Test]
        public void Explode()
        {
            var t = (Table)EvilTestResult(
                "fn test() -> str.explode('abcdef');"
            );

            t[0].ShouldBe("a");
            t[1].ShouldBe("b");
            t[2].ShouldBe("c");
            t[3].ShouldBe("d");
            t[4].ShouldBe("e");
            t[5].ShouldBe("f");
        }
        
        [Test]
        public void Split()
        {
            var t = (Table)EvilTestResult(
                "fn test() -> str.spl('abc|def|ghi', '|');"
            );

            t[0].ShouldBe("abc");
            t[1].ShouldBe("def");
            t[2].ShouldBe("ghi");
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
                "fn test() -> str.rep('test', 4);"
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
        public void IsEmpty()
        {
            EvilTestResult(
                "fn test() -> str.is_empty('');"
            ).ShouldBe(true);
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
                "fn test() -> str.sub('hello, world', 7, nil);"
            ).ShouldBe("world");
        }

        [Test]
        public void SubstringFromTo()
        {
            EvilTestResult(
                "fn test() -> str.sub('hello, world', 3, 7);"
            ).ShouldBe("lo, w");
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
            var t = EvilTestResult(
                "fn test() -> str.rmatch(" +
                "   'This 21 is 37 an example 00 of 12 a test string 99.'," +
                @"   '(\\w+) (\\d+)'" +
                ");"
            ).Table!;

            t.Length.ShouldBe(5);
            t[0].Table!["value"].ShouldBe("This 21");
            t[1].Table!["value"].ShouldBe("is 37");
            t[2].Table!["value"].ShouldBe("example 00");
            t[3].Table!["value"].ShouldBe("of 12");
            t[4].Table!["value"].ShouldBe("string 99");
        }
    }
}