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
                throw new ParserException("Unexpected function arguments operator outside of a function.");
            }

            Match(Token.ExtraArguments);
            return new ExtraArgumentsExpression();
        }
    }
}