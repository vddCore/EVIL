using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Statement FnStatement()
        {
            var attributes = new List<AttributeNode>();

            while (CurrentToken == Token.AttributeList)
            {
                attributes.AddRange(AttributeList().Attributes);
            }

            var (line, col) = (-1, -1);
            var isLocalDefinition = false;
            if (CurrentToken == Token.Loc)
            {
                (line, col) = Match(Token.Loc);
                isLocalDefinition = true;
                
                Match(Token.Fn);
            }
            else
            {
                (line, col) = Match(Token.Fn);
            }

            if (_functionDescent > 0 && !isLocalDefinition)
            {
                throw new ParserException(
                    "Global function definitions may only appear outside of any other functions.",
                    (line, col)
                );
            }

            var isSelfTargeting = false;
            AstNode? primaryTarget = null;            
            IdentifierNode? secondaryIdentifier = null;

            if (CurrentToken.Type == TokenType.Identifier)
            {
                primaryTarget = Identifier();
            }
            else if (CurrentToken.Type == TokenType.Self)
            {
                var (l, c) = Match(Token.Self);
                primaryTarget = new SelfExpression { Line = l, Column = c };
                isSelfTargeting = true;
            }

            if (isSelfTargeting || CurrentToken.Type == TokenType.DoubleColon)
            {
                Match(Token.DoubleColon);
                
                if (!isLocalDefinition)
                {
                    throw new ParserException(
                        "`loc' specifier is required for targeted definitions.",
                        (line, col)
                    );
                }
                
                secondaryIdentifier = Identifier();
            }
            
            var parameterList = ParameterList();

            Statement statement;
            if (CurrentToken == Token.LBrace)
            {
                statement = FunctionDescent(BlockStatement);
            }
            else if (CurrentToken == Token.RightArrow)
            {
                statement = FunctionDescent(ExpressionBodyStatement);
                Match(Token.Semicolon);
            }
            else
            {
                throw new ParserException(
                    $"Expected '{{' or '->', found '{CurrentToken.Value}',",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            if (secondaryIdentifier == null)
            {
                return new FnStatement(
                    (IdentifierNode)primaryTarget!,
                    parameterList,
                    statement,
                    attributes,
                    isLocalDefinition
                ) { Line = line, Column = col };
            }
            else
            {
                return new FnTargetedStatement(
                    primaryTarget,
                    secondaryIdentifier,
                    parameterList,
                    statement,
                    attributes
                ) { Line = line, Column = col };
            }
        }
    }
}