using EVIL.Execution;

namespace EVIL.RuntimeLibrary.Base
{
    public abstract class ClrPackage
    {
        public abstract void Register(Environment env, Interpreter interpreter);

        public virtual void Reset(Environment env, Interpreter interpreter)
        {
        }
    }
}
