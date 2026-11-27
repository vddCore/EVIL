namespace EVIL.VM.ExecutionEngine.Concurrency;

public enum FiberState
{
    Fresh,
    Running,
    Awaiting,
    Paused,
    Crashed,
    Finished
}