namespace EVIL.VirtualMachine.ExecutionEngine.Concurrency;

public enum FiberState
{
    Fresh,
    Running,
    Awaiting,
    Paused,
    Crashed,
    Finished
}