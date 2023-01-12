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

        public bool HasMember(string identifier)
            => Members.ContainsKey(identifier);
        
        public DynValue Set(string identifier, DynValue dynValue)
        {
            if (Members.ContainsKey(identifier))
            {
                Members[identifier] = dynValue;
            }
            else
            {
                Members.Add(identifier, dynValue);
            }

            return dynValue;
        }

        public void UnSetInChain(string identifier)
        {
            var current = this;

            while (current != null)
            {
                if (!current.Members.ContainsKey(identifier))
                {
                    if (current.ParentScope == null)
                    {
                        throw new RuntimeException($"'{identifier}' was not found in current scope.", null);
                    }
                    
                    current = current.ParentScope;
                    continue;
                }

                current.Members.Remove(identifier);
                break;
            }
        }

        public DynValue FindInScopeChain(string identifier)
        {
            var current = this;

            while (current != null)
            {
                if (current.Members.ContainsKey(identifier))
                    return current.Members[identifier];
                
                current = current.ParentScope;
            }

            return null;
        }
    }
}