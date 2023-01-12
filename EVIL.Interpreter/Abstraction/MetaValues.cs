using System.Collections.Generic;

namespace EVIL.Interpreter.Abstraction
{
    public class MetaValues
    {
        private Dictionary<string, DynValue> _metaValues = new();

        public DynValue this[string key]
        {
            get
            {
                if (!_metaValues.ContainsKey(key))
                {
                    _metaValues.Add(key, DynValue.Zero);
                }

                return _metaValues[key];
            }

            set
            {
                if (!_metaValues.ContainsKey(key))
                {
                    _metaValues.Add(key, value);
                }
                else
                {
                    _metaValues[key] = value;
                }
            }
        }
    }
}