using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private EachStatement EachLoop()
        {
            var (line, col) = Match(Token.Each);

            Match(Token.LParenthesis);
            var definitions = VariableDefinition();

            Match(Token.Colon);
            var tableNode = AssignmentExpression();

            Match(Token.RParenthesis);

            var statements = LoopDescent(() => Statement());

            return new EachStatement(definitions, tableNode, statements)
                { Line = line, Column = col };
        }
    }
}