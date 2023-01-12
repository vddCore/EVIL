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

        public double Double => _raw;

        public ulong Pointer
        {
            get
            {
                unsafe
                {
                    fixed (double* ptr = &_raw)
                    {
                        return *(ulong*)ptr & 0x0000FFFFFFFFFFFF;
                    }
                }
            }
        }

        public DynamicValue(double value)
        {
            _raw = value;
        }

        public DynamicValue(Type type, ulong index)
        {
            unsafe
            {
                Box(type, &index);
                _raw = *(double*)&index;
            }
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

        public void LoadRawBits(ulong bits)
        {
            unsafe
            {
                _raw = *(double*)&bits;
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

        private unsafe void Box(Type t, ulong* u)
        {
            *u |= 0x0008000000000000;
            *u |= (ulong)t << 48;
            *u |= 0x7FFF000000000000;
        }
    }
}