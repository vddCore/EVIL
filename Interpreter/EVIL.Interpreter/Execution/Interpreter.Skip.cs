﻿using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(SkipStatement skipStatement)
        {
            Environment.CallStack.Peek().LoopStackTop.SkipThisIteration = true;
        }
    }
}