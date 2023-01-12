using System.Collections.Generic;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Diagnostics
{
    public class NameScope
    {
        public Dictionary<string, DynValue> Members { get; } = new();
        public NameScope ParentScope { get; }

        public NameScope(NameScope parentScope)
        {
            ParentScope = parentScope;
        }

        public bool HasMember(string name)
            => Members.ContainsKey(name);
        
        public DynValue Set(string name, DynValue dynValue)
        {
            if (Members.ContainsKey(name))
            {
                Members[name] = dynValue;
            }
            else
            {
                Members.Add(name, dynValue);
            }

            return dynValue;
        }

        public void UnSet(string name)
        {
            if (!Members.ContainsKey(name))
            {
                throw new RuntimeException($"'{name}' was not found in current scope.", null);
            }

            Members.Remove(name);
        }

        public DynValue FindInScopeChain(string name)
        {
            var current = this;

            while (current != null)
            {
                if (current.Members.ContainsKey(name))
                    return current.Members[name];
                
                current = current.ParentScope;
            }

            return null;
        }
    }
}