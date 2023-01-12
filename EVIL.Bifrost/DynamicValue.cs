namespace EVIL.Bifrost
{
    public class DynamicValue
    {
        public enum Type : byte
        {
            NaN,
            Number,
            String,
            Table,
            Function,
            ClrFunction
        }

        private double _raw;

        public DynamicValue(double value)
        {
            _raw = value;
        }

        public DynamicValue(ulong index)
        {
        }

        public ulong AsRawBits()
        {
            unsafe
            {
                fixed (double* data = &_raw)
                {
                    var longptr = (ulong*)data;
                    return *longptr;
                }
            }
        }

        public Type GetDynamicType()
        {
            if (!double.IsNaN(_raw))
                return Type.Number;

            var bits = AsRawBits();

            var typeBits = (byte)((bits & 0x0007000000000000) >> 48);
            return (Type)typeBits;
        }
    }
}