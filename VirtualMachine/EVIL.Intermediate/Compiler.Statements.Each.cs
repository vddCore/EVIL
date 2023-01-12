using System.Linq;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(EachStatement eachStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (eachStatement.Initialization.Definitions.Count == 1)
            {
                var valueLocal = eachStatement.Initialization.Definitions.ElementAt(0);

                if (valueLocal.Value != null)
                {
                    throw new CompilerException(
                        "An each-loop iterator variable cannot have an initializer.",
                        valueLocal.Value.Line,
                        valueLocal.Value.Column
                    );
                }

                EnterScope();
                {
                    var localScope = ScopeStack.Peek();
                    var valueLocalSym = localScope.DefineLocal(valueLocal.Key);
                    var iterStatement = CurrentChunk.DefineLabel();

                    Visit(eachStatement.Iterable);
                    cg.Emit(OpCode.EACH);
                    cg.Emit(OpCode.JUMP, iterStatement);
                    var loopStart = CurrentChunk.DefineLabel(cg.IP);
                    cg.Emit(OpCode.STL, valueLocalSym.Id);
                    Visit(eachStatement.Body);
                    CurrentChunk.UpdateLabel(iterStatement, cg.IP);
                    cg.Emit(OpCode.ITER, 0);
                    cg.Emit(OpCode.TJMP, loopStart);
                }
                LeaveScope();
            }
            else if (eachStatement.Initialization.Definitions.Count == 2)
            {
                var keyLocal = eachStatement.Initialization.Definitions.ElementAt(0);
                var valueLocal = eachStatement.Initialization.Definitions.ElementAt(1);

                if (keyLocal.Value != null)
                {
                    throw new CompilerException(
                        "An each-loop iterator variable cannot have an initializer.",
                        valueLocal.Value.Line,
                        valueLocal.Value.Column
                    );
                }

                if (valueLocal.Value != null)
                {
                    throw new CompilerException(
                        "An each-loop iterator variable cannot have an initializer.",
                        valueLocal.Value.Line,
                        valueLocal.Value.Column
                    );
                }

                EnterScope();
                {
                    var localScope = ScopeStack.Peek();
                    var keyLocalSym = localScope.DefineLocal(keyLocal.Key);
                    var valueLocalSym = localScope.DefineLocal(valueLocal.Key);

                    var iterStatement = CurrentChunk.DefineLabel();

                    Visit(eachStatement.Iterable);
                    cg.Emit(OpCode.EACH);
                    cg.Emit(OpCode.JUMP, iterStatement);
                    var loopStart = CurrentChunk.DefineLabel(cg.IP);
                    cg.Emit(OpCode.STL, keyLocalSym.Id);
                    cg.Emit(OpCode.STL, valueLocalSym.Id);
                    Visit(eachStatement.Body);
                    CurrentChunk.UpdateLabel(iterStatement, cg.IP);
                    cg.Emit(OpCode.ITER, 1);
                    cg.Emit(OpCode.TJMP, loopStart);
                }
                LeaveScope();
            }
            else
            {
                throw new CompilerException(
                    "Invalid each-loop initialization section.",
                    CurrentLine,
                    CurrentColumn
                );
            }
        }
    }
}