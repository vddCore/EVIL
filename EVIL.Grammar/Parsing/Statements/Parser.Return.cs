﻿using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Return()
        {
            var line = Match(TokenType.Ret);
            AstNode retNode;

            if (CurrentToken.Type == TokenType.Semicolon)
            {
                retNode = new IntegerNode(0) { Line = line };
            }
            else
            {
                retNode = AssignmentExpression();
            }
            
            return new ReturnNode(retNode) { Line = line };
        }
    }
}