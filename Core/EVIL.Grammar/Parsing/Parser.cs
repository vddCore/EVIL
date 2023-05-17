using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private uint _functionDescent;
        private uint _loopDescent;

        public Lexer Lexer { get; private set; } = new();
        public Token CurrentToken => Lexer.State.CurrentToken;

        public Program Parse(string source)
        {
            Lexer.LoadSource(source);

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

        private ParameterList ParseParameters()
        {
            var (line, col) = Match(Token.LParenthesis);
            var parameters = new List<ParameterNode>();
            var hasInitializers = false;
            
            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                var parameterName = CurrentToken.Value!;
                ConstantExpression? initializer = null;

                var (pline, pcol) = Match(Token.Identifier);

                if (CurrentToken == Token.Assign)
                {
                    Match(Token.Assign);
                    initializer = Constant();
                    hasInitializers = true;
                }
                else
                {
                    if (hasInitializers)
                    {
                        throw new ParserException(
                            "Uninitialized parameters must precede default parameters.",
                            (pline, pcol)
                        );
                    }
                }

                parameters.Add(
                    new ParameterNode(parameterName, initializer)
                        { Line = pline, Column = pcol }
                );

                if (CurrentToken == Token.RParenthesis)
                    break;

                Match(Token.Comma);
            }
            Match(Token.RParenthesis);

            return new ParameterList(parameters)
                { Line = line, Column = col };
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
                if (CurrentToken.Type != TokenType.Fn && CurrentToken.Type != TokenType.AttributeList)
                {
                    throw new ParserException($"Expected 'fn' or an attribute, found: '{CurrentToken}'");
                }

                statementList.Add(Statement());
            }

            return new Program(statementList);
        }

        private ExpressionBodyStatement ExpressionBody()
        {
            Match(Token.RightArrow);
            var stmt = new ExpressionBodyStatement(AssignmentExpression());
            Match(Token.Semicolon);

            return stmt;
        }

        private (int, int) Match(Token token)
        {
            var (line, column) = (
                Lexer.State.TokenStartLine,
                Lexer.State.TokenStartColumn
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