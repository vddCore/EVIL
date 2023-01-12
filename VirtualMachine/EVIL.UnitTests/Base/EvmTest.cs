using EVIL.ExecutionEngine;
using NUnit.Framework;

namespace EVIL.UnitTests.Base
{
    public class EvmTest
    {
        protected EVM EVM { get; private set; }

        [SetUp]
        public void SetUp()
        {
            EVM = new EVM();
        }
    }
}