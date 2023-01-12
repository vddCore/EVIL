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
            Match(Token.EOF);

            return programNode;
        }

        private T FunctionDescent<T>(Func<T> func) where T : AstNode
        {
            _functionDescent++;
            var ret = func();
            _functionDescent--;

            return ret;
        }

        private T LoopDescent<T>(Func<T> func) where T : AstNode
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

        private int Match(Token token)
        {
            var line = Lexer.State.Line;

            if (!CurrentToken.Equals(token))
            {
                var expected = $"'{token.Value}'";
                if (string.IsNullOrEmpty(token.Value))
                {
                    expected = WithArticle(token.Type.ToString());
                }

                var actual = $"'{CurrentToken.Value}'";
                if(string.IsNullOrEmpty(CurrentToken.Value))
                {
                    actual = WithArticle(CurrentToken.Type.ToString());
                }

                throw new ParserException(
                    $"Expected {expected}, got {actual}.",
                    Lexer.State
                );
            }

            Lexer.NextToken();

            return line;
        }

        private string WithArticle(string word)
        {
            var result = word.ToLower();
            
            return "aeiou".Contains(result[0])
                ? $"an {result}"
                : $"a {result}";
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