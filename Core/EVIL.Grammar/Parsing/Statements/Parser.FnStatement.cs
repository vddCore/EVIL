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
            AttributeList? attributeList = null;

            while (CurrentToken == Token.AttributeList)
            {
                attributeList = AttributeList();
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
                if (_lexer.PeekToken(1).Type == TokenType.Dot)
                {
                    if (!isLocalDefinition)
                    {
                        throw new ParserException(
                            "`loc' specifier is required for indexed function definitions.",
                            (line, col)
                        );
                    }
                    
                    Expression expression = SymbolReferenceExpression();
                    
                    while (CurrentToken.Type == TokenType.Dot)
                    {
                        expression = IndexerExpression(expression);
                    }

                    primaryTarget = expression;
                }
                else
                {
                    primaryTarget = Identifier();
                }
            }
            else if (CurrentToken.Type == TokenType.Self)
            {
                primaryTarget = SelfExpression();
                isSelfTargeting = true;
            }

            if (isSelfTargeting || CurrentToken.Type == TokenType.DoubleColon)
            {
                Match(Token.DoubleColon);
                
                if (!isLocalDefinition)
                {
                    throw new ParserException(
                        "`loc' specifier is required for targeted function definitions.",
                        (line, col)
                    );
                }
                
                secondaryIdentifier = Identifier();
            }

            ParameterList? parameterList = null;
            if (CurrentToken == Token.LParenthesis)
            {
                parameterList = ParameterList();
            }

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
                if (primaryTarget is IdentifierNode identifierNode)
                {
                    return new FnStatement(
                        identifierNode,
                        attributeList,
                        parameterList,
                        statement,
                        isLocalDefinition
                    ) { Line = line, Column = col };
                }
                else if (primaryTarget is IndexerExpression indexerExpression)
                {
                    return new FnIndexedStatement(
                        indexerExpression,
                        attributeList,
                        parameterList,
                        statement
                    ) { Line = line, Column = col };
                }
                else
                {
                    throw new ParserException(
                        $"Found an unsupported primary target node {primaryTarget!.GetType().FullName}",
                        (line, col)
                    );
                }
            }
            else
            {
                return new FnTargetedStatement(
                    primaryTarget!,
                    secondaryIdentifier,
                    attributeList,
                    parameterList,
                    statement
                ) { Line = line, Column = col };
            }
        }
    }
}