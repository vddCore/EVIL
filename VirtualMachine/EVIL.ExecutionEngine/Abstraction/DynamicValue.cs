using System.Text;

namespace EVIL.ExecutionEngine.Abstraction
{
    public class DynamicValue
    {
        private string _string;
        private double _number;

        public static DynamicValue Zero { get; } = new(0) { IsReadOnly = true };
        
        public bool IsReadOnly { get; set; }
        public DynamicValueType Type { get; set; }

        public string String
        {
            get
            {
                if (Type != DynamicValueType.String)
                    throw new UnexpectedTypeException($"Attempted to use a {Type} as String.");

                return _string;
            }
            set
            {
                if (IsReadOnly)
                    return;

                _string = value;
                Type = DynamicValueType.String;
            }
        }

        public double Number
        {
            get
            {
                if (Type != DynamicValueType.Number)
                    throw new UnexpectedTypeException($"Attempted to use a {Type} as Number");

                return _number;
            }
            set
            {
                if (IsReadOnly)
                    return;

                _number = value;
                Type = DynamicValueType.Number;
            }
        }

        public DynamicValue(double num)
            => Number = num;

        public DynamicValue(string str)
            => String = str;
    }
}