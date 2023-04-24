namespace Ceres.Runtime
{
    public class EvilRuntimeException : Exception
    {
        public EvilRuntimeException(string message)
            : base(message)
        {
        }
    }
}