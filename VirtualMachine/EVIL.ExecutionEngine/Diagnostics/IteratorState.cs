using System.Collections.Generic;
using System.Linq;
using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine.Diagnostics
{
    public class IteratorState
    {
        public Table Table { get; }
        public int Pointer { get; private set; }

        public KeyValuePair<DynamicValue, DynamicValue> CurrentPair { get; private set; }

        public IteratorState(Table table)
        {
            Table = table;

            if (Table.Entries.Count > 0)
            {
                CurrentPair = Table.Entries.ElementAt(0);
            }
        }

        public bool MoveNext()
        {
            if (Pointer < Table.Entries.Count)
            {
                CurrentPair = Table.Entries.ElementAt(Pointer++);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}