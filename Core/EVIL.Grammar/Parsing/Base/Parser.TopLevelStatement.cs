using EVIL.Grammar.AST.Base;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private bool _includeTokensAllowed = true;

        private TopLevelStatement TopLevelStatement()
        {
            var token = CurrentToken;
            TopLevelStatement node;

            switch (token.Type)
            {
                case TokenType.Include when !_includeTokensAllowed:
                    throw new ParserException(
                        $"'#include' statements are only allowed at the beginning of the file.",
                        (token.Line, token.Column)
                    );

                case TokenType.Include:
                    node = IncludeStatement();
                    break;

                case TokenType.AttributeList:
                case TokenType.Fn:
                    _includeTokensAllowed = false;
                    node = FnStatement();
                    break;

                default:
                    throw new ParserException(
                        $"Expected 'fn' or '#[]', found '{token.Value}'.",
                        (token.Line, token.Column)
                    );
            }

            return node;
        }
    }
}