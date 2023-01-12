using System.Collections.Generic;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public class RuntimeConstPool
    {
        private Dictionary<int, DynamicValue> _constants = new();

        public IReadOnlyDictionary<int, DynamicValue> Constants => _constants;

        public RuntimeConstPool(ConstPool constPool)
        {
            for (var i = 0; i < constPool.Count; i++)
            {
                var str = constPool.GetStringConstant(i);

                if (str != null)
                {
                    _constants.Add(
                        i,
                        new DynamicValue(str)
                            { IsReadOnly = true }
                    );
                }
                else
                {
                    _constants.Add(
                        i,
                        new DynamicValue(constPool.GetNumberConstant(i)!.Value)
                            { IsReadOnly = true }
                    );
                }
            }
        }

        public DynamicValue FetchConst(int id)
            => new(_constants[id], false);
    }
}