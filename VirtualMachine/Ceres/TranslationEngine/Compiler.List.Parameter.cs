using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ParameterList parameterList)
        {
            for (var i = 0; i < parameterList.Parameters.Count; i++)
            {
                var parameter = parameterList.Parameters[i];
                var parameterId = Chunk.AllocateParameter();

                try
                {
                    _currentScope.DefineParameter(
                        parameter.Identifier.Name,
                        parameterId,
                        parameter.ReadWrite,
                        parameter.Line,
                        parameter.Column
                    );

                    Chunk.DebugDatabase.SetParameterName(
                        parameterId,
                        parameter.Identifier.Name,
                        parameter.ReadWrite
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        dse.Message,
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        Line,
                        Column,
                        dse
                    );
                }

                if (parameter.Initializer != null)
                {
                    Chunk.InitializeParameter(
                        parameterId,
                        ExtractConstantValueFrom(parameter.Initializer)
                    );
                }
            }
        }
    }
}