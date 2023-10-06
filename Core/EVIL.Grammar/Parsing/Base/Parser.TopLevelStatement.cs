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
                case TokenType.Fn:
                    return FnStatement();
            }

            throw new ParserException(
                $"Expected 'fn' or '#[]', found '{token.Value}'.",
                (token.Line, token.Column)
            );
        }
    }
}