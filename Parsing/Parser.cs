using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private bool IsInsideFunctionDefinition { get; set; }
        
        public Scanner Scanner { get; private set; }

        public void LoadSource(string source)
        {
            Scanner = new Scanner();
            Scanner.LoadSource(source);
        }

        public RootNode Parse()
        {
            var rootNode = new RootNode();
            rootNode.Children.AddRange(RootStatementList());

            return rootNode;
        }

        private List<AstNode> RootStatementList()
        {
            var statementList = new List<AstNode>();

            if (Scanner.State.CurrentToken == null)
                throw new ParserException("Internal error: scanner is in invalid state (current token is null?).");

            while (Scanner.State.CurrentToken.Type != TokenType.EOF)
                statementList.Add(Statement());

            return statementList;
        }

        private List<AstNode> LoopStatementList()
        {
            var statementList = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace)
            {
                if (Scanner.State.CurrentToken.Type == TokenType.EOF)
                    throw new ParserException("Unexpected EOF in a loop block.", Scanner.State);

                statementList.Add(Statement());
            }

            return statementList;
        }

        private List<AstNode> ConditionStatementList()
        {
            var statementList = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace &&
                   Scanner.State.CurrentToken.Type != TokenType.Else &&
                   Scanner.State.CurrentToken.Type != TokenType.Elif)
            {
                if (Scanner.State.CurrentToken.Type == TokenType.EOF)
                    throw new ParserException("Unexpected EOF in condition block.", Scanner.State);

                statementList.Add(Statement());
            }

            return statementList;
        }

        private List<AstNode> FunctionStatementList()
        {
            var statementList = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace)
            {
                if (Scanner.State.CurrentToken.Type == TokenType.EOF)
                    throw new ParserException("Unexpected EOF in function definition.", Scanner.State);

                statementList.Add(Statement());
            }

            return statementList;
        }

        private int Match(TokenType tokenType)
        {
            var line = Scanner.State.Line;

            if (Scanner.State.CurrentToken.Type != tokenType)
                throw new ParserException($"Expected '{Token.StringRepresentation(tokenType)}', got '{Scanner.State.CurrentToken.Value}'.", Scanner.State);

            Scanner.NextToken();

            return line;
        }
    }
}