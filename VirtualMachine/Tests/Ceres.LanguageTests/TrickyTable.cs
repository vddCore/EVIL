using System;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.LanguageTests
{
    public class TrickyTable : Table
    {
        private int _indexCount;
        
        protected override DynamicValue OnIndex(DynamicValue key)
        {
            if (_indexCount++ < 3)
            {
                return DynamicValue.Nil;
            }
            
            if (key.Type == DynamicValue.DynamicValueType.String && key.String == "d20")
            {
                return new(Random.Shared.Next(0, 21));
            }

            return base.OnIndex(key);
        }

        protected override (DynamicValue Key, DynamicValue Value) OnSet(DynamicValue key, DynamicValue value)
        {
            if (key.Type == DynamicValue.DynamicValueType.String && key.String == "d20")
                return (DynamicValue.Nil, DynamicValue.Nil);

            return base.OnSet(key, value);
        }

        protected override bool OnContains(DynamicValue key)
        {
            if (key.Type == DynamicValue.DynamicValueType.String && key.String == "d20")
            {
                return false;
            }

            return base.OnContains(key);
        }
    }
}