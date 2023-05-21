using EVIL.Grammar.AST.Base;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private TopLevelStatement TopLevelStatement()
        {
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.AttributeList:
                    return AttributeList();

                case TokenType.Fn:
                    return FunctionDefinition();
            }

            throw new ParserException(
                $"Expected 'fn' or '#[]', found '{token.Value}'.",
                (_lexer.State.TokenStartColumn, _lexer.State.TokenStartLine)
            );
        }
    }
}