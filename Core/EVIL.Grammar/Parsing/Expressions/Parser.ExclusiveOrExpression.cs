﻿using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression ExclusiveOrExpression()
        {
            var node = AndExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseXor)
            {
                var (line, col) = Match(Token.BitwiseXor);

                node = new BinaryExpression(node, AndExpression(), BinaryOperationType.BitwiseXor)
                    { Line = line, Column = col };

                token = CurrentToken;
            }

            return node;
        }
    }
}