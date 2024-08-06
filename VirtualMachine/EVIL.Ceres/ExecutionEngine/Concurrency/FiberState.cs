namespace EVIL.Ceres.ExecutionEngine.Concurrency;

public enum FiberState
{
    Fresh,
    Running,
    Awaiting,
    Paused,
    Crashed,
    Finished
}