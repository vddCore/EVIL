using System.Collections.Generic;
using EVIL.Compiler.Collections;

namespace EVIL.Compiler
{
    public class Prototype
    {
        private int _nextStringConstantId;

        public CodeGenerator CodeGenerator { get; }

        public TwoWayDictionary<string, int> Locals { get; } = new();
        public TwoWayDictionary<string, int> StringConstants { get; } = new();
        
        public IReadOnlyList<byte> Instructions => CodeGenerator.Instructions;

        public Prototype()
        {
            CodeGenerator = new CodeGenerator(this);
        }
        
        public int AddConstantString(string constant)
        {
            if (StringConstants.Forward.ContainsKey(constant))
                return StringConstants.GetByKey(constant);
            
            StringConstants.Add(constant, _nextStringConstantId);
            return _nextStringConstantId++;
        }

        public string GetConstantById(int id)
        {
            if (!StringConstants.Reverse.ContainsKey(id))
                return null;

            return StringConstants.GetByValue(id);
        }
    }
}