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
        public void Acos()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.acos(0.13);").Number, 1),
                Is.EqualTo(1.4)
            );
        }

        [Test]  
        public void Acosh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.acosh(100);").Number, 1),
                Is.EqualTo(5.3)
            );
        }

        [Test]
        public void Asin()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.asin(0.13);").Number, 1),
                Is.EqualTo(0.1)
            );
        }
        
        [Test]
        public void Asinh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.asinh(0.20);").Number, 1),
                Is.EqualTo(0.2)
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