using System;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Modules;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class MathModuleTest : ModuleTest<MathModule>
    {
        [Test]
        public void Abs()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.abs(-2137);").Number, 1),
                Is.EqualTo(2137)
            );
        }
        
        [Test]
        public void Cos()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.cos(math.pi/6);").Number, 1),
                Is.EqualTo(0.9)
            );
        }
        
        [Test]
        public void Sin()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.sin(math.pi/6);").Number, 1),
                Is.EqualTo(0.5)
            );
        }

        [Test]
        public void SinCos()
        {
            var ret = RunEvilCode("fn test() -> math.sincos(math.pi/6);").Table!;
            
            Assert.That(Math.Round(ret["sin"].Number, 1), Is.EqualTo(0.5));
            Assert.That(Math.Round(ret["cos"].Number, 1), Is.EqualTo(0.9));
        }
        
        [Test]
        public void Sqrt()
        {
            Assert.That(
                RunEvilCode("fn test() -> math.sqrt(400);"),
                Is.EqualTo((DynamicValue)20)
            );
        }
    }
}