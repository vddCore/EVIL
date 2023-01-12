using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ExtraArgumentsExpression FunctionArguments()
        {
            if (_functionDescent == 0)
            {
                throw new ParserException("Unexpected '...' operator outside of a function.");
            }

            var line = Match(Token.ExtraArguments);
            return new ExtraArgumentsExpression() { Line = line };
        }
    }
}