namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private EachStatement EachStatement()
    {
        var (line, col) = Match(Token.Each);
        Match(Token.LParenthesis);

        if (CurrentToken == Token.Val)
        {
            throw new ParserException(
                "Each-loop variables must be 'rw'.",
                (_lexer.State.Line, _lexer.State.Column)
            );
        }
            
        Match(Token.Rw);
        Match(Token.Val);
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