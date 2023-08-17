using System;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class MathModuleTest : ModuleTest<MathModule>
    {
        [Test]
        public void Abs()
        {
            EvilTestResult(
                "fn test() -> math.abs(-2137);"
            ).ShouldBe(2137);
        }

        [Test]
        public void Acos()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.acos(0.13);"
                ).Number, 1
            ).ShouldBe(1.4);
        }

        [Test]
        public void Acosh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.acosh(100);"
                ).Number, 1
            ).ShouldBe(5.3);
        }

        [Test]
        public void Asin()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.asin(0.13);"
                ).Number, 1
            ).ShouldBe(0.1);
        }

        [Test]
        public void Asinh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.asinh(0.20);"
                ).Number, 1
            ).ShouldBe(0.2);
        }

        [Test]
        public void Atan()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.atan(2137);"
                ).Number, 1
            ).ShouldBe(1.6);
        }

        [Test]
        public void Atanh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.atanh(0.2137);"
                ).Number, 1
            ).ShouldBe(0.2);
        }

        [Test]
        public void Atan2()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.atan2(21, 37);"
                ).Number, 1
            ).ShouldBe(0.5);
        }

        [Test]
        public void Cbrt()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.cbrt(27);"
                ).Number, 1
            ).ShouldBe(3);
        }

        [Test]
        public void Ceil()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.ceil(13.34);"
                ).Number, 1
            ).ShouldBe(14);
        }

        [Test]
        public void Clamp()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.clamp(10, 11, 13);"
                ).Number, 1
            ).ShouldBe(11);
        }

        [Test]
        public void Cos()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.cos(math.pi/6);"
                ).Number, 1
            ).ShouldBe(0.9);
        }

        [Test]
        public void Cosh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.cosh(math.pi/6);"
                ).Number, 1
            ).ShouldBe(1.1);
        }

        [Test]
        public void Exp()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.exp(3);"
                ).Number, 1
            ).ShouldBe(20.1);
        }

        [Test]
        public void Floor()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.floor(21.37);"
                ).Number, 1
            ).ShouldBe(21);
        }

        [Test]
        public void Lerp()
        {
            var t = EvilTestResult(
                "fn test() -> {" +
                "  math.round(math.lerp(0, 100, 0.0), 0)," +
                "  math.round(math.lerp(0, 100, 0.2), 0)," +
                "  math.round(math.lerp(0, 100, 0.4), 0)," +
                "  math.round(math.lerp(0, 100, 0.6), 0)," +
                "  math.round(math.lerp(0, 100, 0.8), 0)," +
                "  math.round(math.lerp(0, 100, 1.0), 0) " +
                "};"
            ).Table!;

            t[0].ShouldBe(0);
            t[1].ShouldBe(20);
            t[2].ShouldBe(40);
            t[3].ShouldBe(60);
            t[4].ShouldBe(80);
            t[5].ShouldBe(100);
        }

        [Test]
        public void Log()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.log(27, 3);"
                ).Number, 1
            ).ShouldBe(3);
        }

        [Test]
        public void Log2()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.log2(64);"
                ).Number, 1
            ).ShouldBe(6);
        }

        [Test]
        public void Log10()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.log10(100);"
                ).Number, 1
            ).ShouldBe(2);
        }

        [Test]
        public void Max()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.max(100, 1000);"
                ).Number, 1
            ).ShouldBe(1000);
        }

        [Test]
        public void Min()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.min(100, 1000);"
                ).Number, 1
            ).ShouldBe(100);
        }

        [Test]
        public void Pow()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.pow(4, 4);"
                ).Number, 1
            ).ShouldBe(256);
        }

        [Test]
        public void Round()
        {
            var t = EvilTestResult(
                "fn test() -> { " +
                "  math.round(3.6662137, 0)," +
                "  math.round(3.6662137, 1)" +
                "};"
            ).Table!;

            t[0].ShouldBe(4);
            t[1].ShouldBe(3.7);
        }

        [Test]
        public void Sin()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.sin(math.pi/6);"
                ).Number, 1
            ).ShouldBe(0.5);
        }

        [Test]
        public void SinCos()
        {
            var t = EvilTestResult("fn test() -> math.sincos(math.pi/6);").Table!;
            Math.Round(t["sin"].Number, 1).ShouldBe(0.5);
            Math.Round(t["cos"].Number, 1).ShouldBe(0.9);
        }

        [Test]
        public void Sinh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.sinh(math.pi/8);"
                ).Number, 1
            ).ShouldBe(0.4);
        }

        [Test]
        public void Sqrt()
        {
            EvilTestResult(
                "fn test() -> math.sqrt(400);"
            ).ShouldBe(20);
        }

        [Test]
        public void Tan()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.tan(math.pi/6);"
                ).Number, 1
            ).ShouldBe(0.6);
        }

        [Test]
        public void Tanh()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.tanh(math.pi/9.5);"
                ).Number, 1
            ).ShouldBe(0.3);
        }

        [Test]
        public void Trunc()
        {
            EvilTestResult(
                "fn test() -> math.trunc(13333.411321443134213);"
            ).ShouldBe(13333);
        }

        [Test]
        public void Rad2Deg()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.rad2deg(1.1);"
                ).Number, 1
            ).ShouldBe(63);
        }

        [Test]
        public void Deg2Rad()
        {
            Math.Round(
                EvilTestResult(
                    "fn test() -> math.deg2rad(21.37);"
                ).Number, 1
            ).ShouldBe(0.4);
        }
    }
}