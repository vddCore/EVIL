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
                        if (_env.GlobalScope.HasMember(identifier))
                        {
                            current = _env.GlobalScope;
                            break;
                        }
                        throw new RuntimeException($"'{identifier}' was not found in current scope.", _env, null);
                    }

                    current = current.ParentScope;
                    continue;
                }
                break;
            }

            current.Members.Remove(identifier);
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

            if (_env.GlobalScope.HasMember(identifier))
                return _env.GlobalScope.Members[identifier];

            return null;
        }
    }
}