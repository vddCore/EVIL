using System;
using System.Collections.Generic;
using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private uint _functionDescent;
        private uint _loopDescent;

        public Lexer Lexer { get; private set; }
        public Token CurrentToken => Lexer.State.CurrentToken;

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
        }

        public ProgramNode Parse()
        {
            var programNode = Program();
            Match(TokenType.EOF);

            return programNode;
        }

        private T FunctionDescent<T>(Func<T> func) where T: AstNode
        {
            _functionDescent++;
            var ret = func();
            _functionDescent--;

            return ret;
        }

        private T LoopDescent<T>(Func<T> func) where T: AstNode
        {
            _loopDescent++;
            var ret = func();
            _loopDescent--;

            return ret;
        }

        private ProgramNode Program()
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
            {
                statementList.Add(Statement());
            }

            return new ProgramNode(statementList);
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