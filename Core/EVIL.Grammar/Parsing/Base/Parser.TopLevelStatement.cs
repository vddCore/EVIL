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

            switch (token.Type)
            {
                case TokenType.Include:
                {
                    if (!_includeTokensAllowed)
                    {
                        throw new ParserException(
                            $"'#include' statements are only allowed at the beginning of the file.",
                            (token.Line, token.Column)
                        );
                    }
                    
                    return IncludeStatement();
                }

                case TokenType.AttributeList:
                case TokenType.Fn:
                    _includeTokensAllowed = false;
                    return FnStatement();
            }

            throw new ParserException(
                $"Expected 'fn' or '#[]', found '{token.Value}'.",
                (token.Line, token.Column)
            );
        }
    }
}