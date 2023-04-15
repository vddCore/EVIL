using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private DoWhileStatement DoWhileLoop()
        {
            var (line, col) = Match(Token.Do);

            var statements = LoopDescent(() => Statement());
            
            Match(Token.While);
            Match(Token.LParenthesis);
            var conditionExpression = AssignmentExpression();
            Match(Token.RParenthesis);

            return new DoWhileStatement(conditionExpression, statements) 
                { Line = line, Column = col };
        }
    }
}