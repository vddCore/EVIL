using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ExitStatement Exit()
        {
            var (line, col) = Match(Token.Exit);
            return new ExitStatement { Line = line, Column = col };
        }
    }
}
