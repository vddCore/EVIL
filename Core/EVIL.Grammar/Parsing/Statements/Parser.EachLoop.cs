using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private EachStatement EachLoop()
        {
            var (line, col) = Match(Token.Each);
            Match(Token.LParenthesis);
            
            Match(Token.Var);
            var keyIdentifier = Identifier();
            IdentifierNode? valueIdentifier = null;
            
            if (CurrentToken == Token.Comma)
            {
                Match(Token.Comma);
                valueIdentifier = Identifier();
            }
            
            Match(Token.Colon);
            
            var iterable = AssignmentExpression();
            
            Match(Token.RParenthesis);
            
            var statement = LoopDescent(() => Statement());

            return new EachStatement(
                keyIdentifier,
                valueIdentifier,
                iterable,
                statement
            ) { Line = line, Column = col };
        }
    }
}