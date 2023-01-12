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
        private bool _allowTopLevelStatements;

        public Lexer Lexer { get; private set; }
        public Token CurrentToken => Lexer.State.CurrentToken;

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
        }

        public Program Parse(bool allowTopLevelStatements)
        {
            _allowTopLevelStatements = allowTopLevelStatements;            
            
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

        private List<string> ParseParameters()
        {
            Match(Token.LParenthesis);
            var parameters = new List<string>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                parameters.Add(CurrentToken.Value);
                Match(Token.Identifier);

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(Token.Comma);
            }
            Match(Token.RParenthesis);

            return parameters;
        }

        private T LoopDescent<T>(Func<T> func) where T : AstNode
        {
            _loopDescent++;
            var ret = func();
            _loopDescent--;

            return ret;
        }

        private Program Program()
        {
            var statementList = new List<Statement>();

            if (CurrentToken == Token.Empty)
            {
                throw new ParserException(
                    "Internal error: lexer is in an invalid state (current token is empty?).",
                    (-1, -1)
                );
            }

            while (CurrentToken.Type != TokenType.EOF)
            {
                statementList.Add(Statement());
            }

            return new Program(statementList);
        }

        private (int, int) Match(Token token)
        {
            var (line, column) = (Lexer.State.Line, Lexer.State.Column);

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

            Lexer.NextToken();
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