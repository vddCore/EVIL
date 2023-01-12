using System.IO;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EvilProgram = EVIL.Grammar.AST.Nodes.Program;

namespace EVIL.CVIL.Services
{
    public class ParserService : Service
    {
        private readonly Lexer _lexer = new();
        private readonly Parser _parser;

        public bool AllowTopLevelStatements { get; set; } = true;

        public ParserService()
        {
            _parser = new Parser(_lexer);
        }
        
        public EvilProgram Parse(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                _lexer.LoadSource(sr.ReadToEnd());
            }

            return _parser.Parse(AllowTopLevelStatements);
        }
    }
}