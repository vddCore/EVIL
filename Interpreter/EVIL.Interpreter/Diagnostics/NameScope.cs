using System.Collections.Generic;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Execution;

namespace EVIL.Interpreter.Diagnostics
{
    public class NameScope
    {
        private readonly Environment _env;

        public Dictionary<string, DynValue> Members { get; } = new();
        public NameScope ParentScope { get; }

        public NameScope(Environment env, NameScope parentScope)
        {
            _env = env;

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

        public void FindAndUndefine(string identifier)
        {
            var current = this;

            while (current != null)
            {
                if (current.HasMember(identifier))
                {
                    current.Members.Remove(identifier);
                    return;
                }
                
                current = current.ParentScope;
            }


            throw new RuntimeException($"'{identifier}' was not found in current scope.", _env, null);
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