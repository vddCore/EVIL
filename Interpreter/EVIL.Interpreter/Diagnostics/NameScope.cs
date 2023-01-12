using System;
using System.Collections.Generic;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Diagnostics
{
    [Serializable]
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
            if (HasMember(identifier))
            {
                Members[identifier] = dynValue;
            }
            else
            {
                Members.Add(identifier, dynValue);
            }

            return dynValue;
        }

        public bool FindAndUndefine(string identifier)
        {
            var current = this;

            while (current != null)
            {
                if (current.HasMember(identifier))
                {
                    current.Members.Remove(identifier);
                    return true;
                }
                
                current = current.ParentScope;
            }

            return false;
        }

        public DynValue FindInScope(string identifier)
        {
            var current = this;

            while (current != null)
            {
                if (current.HasMember(identifier))
                    return current.Members[identifier];

                current = current.ParentScope;
            }
            
            return null;
        }
    }
}