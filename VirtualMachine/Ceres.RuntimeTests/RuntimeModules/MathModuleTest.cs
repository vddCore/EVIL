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
        public void Atan()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.atan(2137);").Number, 1),
                Is.EqualTo(1.6)
            );
        }

        [Test]
        public void Atanh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.atanh(0.2137);").Number, 1),
                Is.EqualTo(0.2)
            );
        }

        [Test]
        public void Atan2()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.atan2(21, 37);").Number, 1),
                Is.EqualTo(0.5)
            );
        }

        [Test]
        public void Cbrt()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.cbrt(27);").Number, 1),
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Ceil()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.ceil(13.34);").Number, 1),
                Is.EqualTo(14)
            );
        }

        [Test]
        public void Clamp()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.clamp(10, 11, 13);").Number, 1),
                Is.EqualTo(11)
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
        public void Cosh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.cosh(math.pi/6);").Number, 1),
                Is.EqualTo(1.1)
            );
        }

        [Test]
        public void Exp()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.exp(3);").Number, 1),
                Is.EqualTo(20.1)
            );
        }

        [Test]
        public void Floor()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.floor(21.37);").Number, 1),
                Is.EqualTo(21)
            );
        }

        [Test]
        public void Log()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.log(27, 3);").Number, 1),
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Log2()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.log2(64);").Number, 1),
                Is.EqualTo(6)
            );
        }

        [Test]
        public void Log10()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.log10(100);").Number, 1),
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Max()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.max(100, 1000);").Number, 1),
                Is.EqualTo(1000)
            );
        }

        [Test]
        public void Min()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.min(100, 1000);").Number, 1),
                Is.EqualTo(100)
            );
        }

        [Test]
        public void Pow()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.pow(4, 4);").Number, 1),
                Is.EqualTo(256)
            );
        }

        [Test]
        public void Round()
        {
            var t = RunEvilCode(
                "fn test() -> { " +
                "  math.round(3.6662137, 0)," +
                "  math.round(3.6662137, 1)" +
                "};"
            ).Table!;
            
            Assert.That(t[0].Number, Is.EqualTo(4));
            Assert.That(t[1].Number, Is.EqualTo(3.7));
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
        public void Sinh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.sinh(math.pi/8);").Number, 1),
                Is.EqualTo(0.4)
            );
        }

        [Test]
        public void Sqrt()
        {
            Assert.That(
                RunEvilCode("fn test() -> math.sqrt(400);"),
                Is.EqualTo((DynamicValue)20)
            );
        }
        
        [Test]
        public void Tan()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.tan(math.pi/6);").Number, 1),
                Is.EqualTo(0.6)
            );
        }
        
        [Test]
        public void Tanh()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.tanh(math.pi/9.5);").Number, 1),
                Is.EqualTo(0.3)
            );
        }
        
        [Test]
        public void Trunc()
        {
            Assert.That(
                Math.Round(RunEvilCode("fn test() -> math.trunc(13333.411321443134213);").Number, 1),
                Is.EqualTo(13333)
            );
        }
    }
}