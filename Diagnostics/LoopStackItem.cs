namespace EVIL.Diagnostics
{
    public class LoopStackItem
    {
        public bool BreakLoop { get; private set; }
        public bool SkipThisIteration { get; set; }

        public void Break()
        {
            BreakLoop = true;
        }
    }
}