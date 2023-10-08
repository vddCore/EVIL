using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.Parsing;
using NUnit.Framework;

namespace EVIL.CoreTests.ParserTests
{
    public abstract class ParserTestBase
    {
        protected Parser _parser = null!;

        [SetUp]
        public virtual void Setup()
        {
            _parser = new Parser();
        }

        [TearDown]
        public virtual void Teardown()
        {
            _parser = null!;
        }

        protected ProgramNode BuildProgramTree(string source)
            => _parser.Parse(source);
    }
}