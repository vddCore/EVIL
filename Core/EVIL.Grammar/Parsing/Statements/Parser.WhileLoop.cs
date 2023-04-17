using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private WhileStatement WhileLoop()
        {
            var (line, col) = Match(Token.While);
            Match(Token.LParenthesis);
            var condition = AssignmentExpression();
            Match(Token.RParenthesis);

            var statement = LoopDescent(() => Statement());
            
            return new WhileStatement(condition, statement) 
                { Line = line, Column = col };
        }
    }
}
