using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private readonly Lexer _lexer = new();
        
        private uint _functionDescent;
        private uint _loopDescent;

        public LexerState CurrentState => _lexer.State;
        public Token CurrentToken => CurrentState.CurrentToken;

        public Program Parse(string source)
        {
            _lexer.LoadSource(source);

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

        private (int, int) Match(Token token)
        {
            var (line, column) = (
                _lexer.State.TokenStartLine,
                _lexer.State.TokenStartColumn
            );

            if (!CurrentToken.Equals(token))
            {
                var expected = $"'{token.Value}'";
                if (string.IsNullOrEmpty(token.Value))
                {
                    expected = WithArticle(token.Type.ToString());
                }

                var actual = $"'{CurrentToken.Value}'";
                if (string.IsNullOrEmpty(CurrentToken.Value))
                {
                    actual = WithArticle(CurrentToken.Type.ToString());
                }

                throw new ParserException(
                    $"Expected {expected}, got {actual}.",
                    (line, column)
                );
            }

            _lexer.NextToken();
            return (line, column);
        }

        private string WithArticle(string word)
        {
            var result = word.ToLower();

            return "aeiou".Contains(result[0])
                ? $"an {result}"
                : $"a {result}";
        }
    }
}