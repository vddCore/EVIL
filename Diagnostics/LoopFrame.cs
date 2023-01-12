namespace EVIL.Diagnostics
{
    public class LoopFrame
    {
        public bool BreakLoop { get; private set; }
        public bool SkipThisIteration { get; set; }

        public void Break()
        {
            BreakLoop = true;
        }
    }
}