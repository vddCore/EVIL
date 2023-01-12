using System.Collections.Generic;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;

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
                    );
                }
                else
                {
                    _constants.Add(
                        i, 
                        new DynamicValue(constPool.GetNumberConstant(i)!.Value)
                     );
                }
            }
        }

        public DynamicValue FetchConst(int id)
            => _constants[id];
    }
}