using System.Collections.Generic;
using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public Lexer Lexer { get; private set; }
        public Token CurrentToken => Lexer.State.CurrentToken;
        
        public Parser(Lexer lexer)
        {
            Lexer = lexer;
        }

        public ProgramNode Parse()
        {
            var programNode = new ProgramNode(RootStatementList());
            Match(TokenType.EOF);
            return programNode;
        }

        private List<AstNode> RootStatementList()
        {
            var statementList = new List<AstNode>();

            if (CurrentToken == null)
            {
                throw new ParserException(
                    "Internal error: scanner is in invalid state (current token is null?).",
                    Lexer.State
                );
            }
            
            while (CurrentToken.Type != TokenType.EOF)
                statementList.Add(Statement());
            
            return statementList;
        }

        private List<AstNode> ConditionStatementList()
        {
            var statementList = new List<AstNode>();

            while (CurrentToken.Type != TokenType.RBrace &&
                   CurrentToken.Type != TokenType.Else &&
                   CurrentToken.Type != TokenType.Elif)
            {
                if (CurrentToken.Type == TokenType.EOF)
                {
                    throw new ParserException(
                        "Unexpected EOF in condition block.", 
                        Lexer.State
                    );
                }

                statementList.Add(Statement());
            }

            return statementList;
        }

        private int Match(TokenType tokenType)
        {
            var line = Lexer.State.Line;

            if (CurrentToken.Type != tokenType)
            {
                throw new ParserException(
                    $"Expected '{Token.StringRepresentation(tokenType)}', got '{CurrentToken.Value}'.", 
                    Lexer.State
                );
            }

            Lexer.NextToken();

            return line;
        }

        private void DisallowPrevious(params TokenType[] types)
        {
            if (types.Contains(Lexer.State.PreviousToken.Type))
            {
                throw new ParserException(
                    "Disallowed token sequence.", 
                    Lexer.State
                );
            }
        }
    }
}