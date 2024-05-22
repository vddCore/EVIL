using System;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static EVIL.Ceres.Runtime.EvilDocPropertyMode;

namespace EVIL.Ceres.Runtime.Modules
{
    public sealed class MathModule : RuntimeModule
    {       
        public override string FullyQualifiedName => "math";

        private static readonly Table _randTable = new PropertyTable()
        {
            { "i32", (_) => Random.Shared.Next() },
            { "i64", (_) => Random.Shared.NextInt64() },
            { "f32", (_) => Random.Shared.NextSingle() },
            { "f64", (_) => Random.Shared.NextDouble() }
        }.Freeze();

        [RuntimeModuleGetter("max_val")]
        [EvilDocProperty(
            Get,
            "Gets the highest possible value a Number type can hold.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue MaxValue(DynamicValue _)
            => double.MaxValue;
        
        [RuntimeModuleGetter("min_val")]
        [EvilDocProperty(
            Get,
            "Gets the lowest possible value a Number type can hold.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue MinValue(DynamicValue _)
            => double.MaxValue;
        
        [RuntimeModuleGetter("r")]
        [EvilDocProperty(
            Get,
            "Gets a random value of the specified type.  \n" +
            "> `.i32`  \n" +
            "> Gets a random signed 32-bit integer.  \n\n" +
            "" +
            "> `.i64`  \n" +
            "> Gets a random signed 64-bit integer.  \n\n" +
            "" +
            "> `.f32`  \n" +
            "> Gets a random 32-bit floating point number.  \n\n" +
            "" +
            ">`.f64`  \n" +
            "> Gets a random 64-bit floating point number.  \n\n",
            ReturnType = DynamicValueType.Table
        )]
        private static DynamicValue Rand(DynamicValue _)
            => _randTable;

        [RuntimeModuleGetter("pi")]
        [EvilDocProperty(
            Get,
            "Gets the value of the π constant.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue Pi(DynamicValue _)
            => Math.PI;
        
        [RuntimeModuleGetter("e")]
        [EvilDocProperty(
            Get,
            "Gets the value of the e constant.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue E(DynamicValue _)
            => Math.E;

        [RuntimeModuleGetter("tau")]
        [EvilDocProperty(
            Get,
            "Gets the value of the τ constant.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue Tau(DynamicValue _)
            => Math.Tau;

        [RuntimeModuleFunction("abs")]
        [EvilDocFunction(
            "Calculates the absolute value of a Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose absolute value to calculate.", DynamicValueType.Number)]
        private static DynamicValue Abs(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Abs(value);
        }

        [RuntimeModuleFunction("acos")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose cosine is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A cosine whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Acos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Acos(value);
        }

        [RuntimeModuleFunction("acosh")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose hyperbolic cosine is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A hyperbolic cosine whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Acosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Acosh(value);
        }

        [RuntimeModuleFunction("asin")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose sine is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A sine whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Asin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Asin(value);
        }

        [RuntimeModuleFunction("asinh")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose hyperbolic sine is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A hyperbolic sine whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Asinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Asinh(value);
        }

        [RuntimeModuleFunction("atan")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose tangent is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A tangent whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Atan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Atan(value);
        }

        [RuntimeModuleFunction("atanh")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose hyperbolic tangent is the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A hyperbolic tangent whose angle to calculate.", DynamicValueType.Number)]
        private static DynamicValue Atanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Atanh(value);
        }

        [RuntimeModuleFunction("atan2")]
        [EvilDocFunction(
            "Calculates the angle (radians) whose tangent is the quotient of two provided coordinates.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("x", "The X-component of the coordinate.", DynamicValueType.Number)]
        [EvilDocArgument("y", "The Y-component of the coordinate.", DynamicValueType.Number)]
        private static DynamicValue Atan2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var y);

            return Math.Atan2(y, x);
        }

        [RuntimeModuleFunction("cbrt")]
        [EvilDocFunction(
            "Calculates the cube root of the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose cube root to calculate.", DynamicValueType.Number)]
        private static DynamicValue Cbrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cbrt(value);
        }

        [RuntimeModuleFunction("ceil")]
        [EvilDocFunction(
            "Calculates the smallest integer that's greater than or equal to the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose ceiling to calculate.", DynamicValueType.Number)]
        private static DynamicValue Ceiling(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Ceiling(value);
        }

        [RuntimeModuleFunction("clamp")]
        [EvilDocFunction(
            "Clamps the provided value to the _inclusive_ range specified by `min` and `max` parameters.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value to be clamped.", DynamicValueType.Number)]
        [EvilDocArgument("min", "A lower inclusive bound of the clamping operation. ", DynamicValueType.Number)]
        [EvilDocArgument("max", "An upper inclusive bound of the clamping operation. ", DynamicValueType.Number)]
        private static DynamicValue Clamp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var min)
                .ExpectNumberAt(2, out var max);

            return Math.Clamp(value, min, max);
        }

        [RuntimeModuleFunction("cos")]
        [EvilDocFunction(
            "Calculates the cosine of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose cosine to calculate.", DynamicValueType.Number)]
        private static DynamicValue Cos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cos(value);
        }

        [RuntimeModuleFunction("cosh")]
        [EvilDocFunction(
            "Calculates the hyperbolic cosine of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose hyperbolic cosine to calculate.", DynamicValueType.Number)]
        private static DynamicValue Cosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cosh(value);
        }

        [RuntimeModuleFunction("exp")]
        [EvilDocFunction(
            "Calculates `math.e` raised to the given power.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A power to which `math.e` should be raised.", DynamicValueType.Number)]
        private static DynamicValue Exp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Exp(value);
        }

        [RuntimeModuleFunction("floor")]
        [EvilDocFunction(
            "Calculates the largest integer that's less than or equal to the specified Number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose floor to calculate.", DynamicValueType.Number)]
        private static DynamicValue Floor(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Floor(value);
        }

        [RuntimeModuleFunction("lerp")]
        [EvilDocFunction(
            "Calculates a value linearly interpolated between two values for the given interpolant.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("a", "Start value, returned when `t == 0`.", DynamicValueType.Number)]
        [EvilDocArgument("b", "End value, returned when `t == 1`.", DynamicValueType.Number)]
        [EvilDocArgument("t", "Position on the line between `a` and `b`. The interpolant is expressed as a number in range of [0.0 - 1.0].", DynamicValueType.Number)]
        private static DynamicValue Lerp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b)
                .ExpectNumberAt(2, out var t);

            t = Math.Clamp(t, 0.0, 1.0);
            return a * (1 - t) + b * t;
        }

        [RuntimeModuleFunction("log")]
        [EvilDocFunction(
            "Calculates the logarithm of a specified number in a specified base.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose logarithm to calculate.", DynamicValueType.Number)]
        [EvilDocArgument("base", "Base of the logarithm", DynamicValueType.Number)]
        private static DynamicValue Log(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var @base);

            return Math.Log(value, @base);
        }

        [RuntimeModuleFunction("log2")]
        [EvilDocFunction(
            "Calculates the base 2 logarithm of a specified number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose logarithm to calculate.", DynamicValueType.Number)]
        private static DynamicValue Log2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Log2(value);
        }

        [RuntimeModuleFunction("log10")]
        [EvilDocFunction(
            "Calculates the base 10 logarithm of a specified number.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A value whose logarithm to calculate.", DynamicValueType.Number)]
        private static DynamicValue Log10(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Log10(value);
        }

        [RuntimeModuleFunction("max")]
        [EvilDocFunction(
            "Returns the larger of the provided values.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("a", "First value to be checked.", DynamicValueType.Number)]
        [EvilDocArgument("a", "Second value to be checked.", DynamicValueType.Number)]
        private static DynamicValue Max(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);

            return Math.Max(a, b);
        }

        [RuntimeModuleFunction("min")]
        [EvilDocFunction(
            "Returns the smaller of the provided values.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("a", "First value to be checked.", DynamicValueType.Number)]
        [EvilDocArgument("a", "Second value to be checked.", DynamicValueType.Number)]
        private static DynamicValue Min(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);

            return Math.Min(a, b);
        }

        [RuntimeModuleFunction("pow")]
        [EvilDocFunction(
            "Calculates `x` raised to the power of `y`.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("x", "Base of the power.", DynamicValueType.Number)]
        [EvilDocArgument("y", "Exponent of the power.", DynamicValueType.Number)]
        private static DynamicValue Pow(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var y);

            return Math.Pow(x, y);
        }

        [RuntimeModuleFunction("round")]
        [EvilDocFunction(
            "Rounds `x` to the specified number of decimal digits.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("x", "Number to be rounded.", DynamicValueType.Number)]
        [EvilDocArgument("digits", "A number specifying the number of digits to round `x` to.", DynamicValueType.Number)]
        private static DynamicValue Round(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var x)
                .ExpectIntegerAt(1, out var digits);

            return Math.Round(x, (int)digits);
        }

        [RuntimeModuleFunction("sign")]
        [EvilDocFunction(
            "Returns a value denoting the sign of the specified value.",
            Returns = "`-1` if the value was negative, `0` if the value was 0, `1` if the value was positive.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "The value whose sign is to be evaluated.", DynamicValueType.Number)]
        private static DynamicValue Sign(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sign(value);
        }

        [RuntimeModuleFunction("sin")]
        [EvilDocFunction(
            "Calculates the sine of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose sine to calculate.", DynamicValueType.Number)]
        private static DynamicValue Sin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sin(value);
        }

        [RuntimeModuleFunction("sincos")]
        [EvilDocFunction(
            "Calculates sine and cosine of the given angle.",
            Returns = "A Table containing `sin` and `cos` fields with respective calclation results.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("value", "An angle (radians) whose sine and cosine to calculate.", DynamicValueType.Number)]
        private static DynamicValue SinCos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            var result = Math.SinCos(value);

            return new Table
            {
                { "sin", result.Sin },
                { "cos", result.Cos }
            };
        }

        [RuntimeModuleFunction("sinh")]
        [EvilDocFunction(
            "Calculates the hyperbolic sine of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose hyperbolic sine to calculate.", DynamicValueType.Number)]
        private static DynamicValue Sinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sinh(value);
        }

        [RuntimeModuleFunction("sqrt")]
        [EvilDocFunction(
            "Calculates the square root of the given value.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A number whose square root to calculate.", DynamicValueType.Number)]
        private static DynamicValue Sqrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sqrt(value);
        }

        [RuntimeModuleFunction("tan")]
        [EvilDocFunction(
            "Calculates the tangent of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose tangent to calculate.", DynamicValueType.Number)]
        private static DynamicValue Tan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Tan(value);
        }

        [RuntimeModuleFunction("tanh")]
        [EvilDocFunction(
            "Calculates the hyperbolic tangent of the given angle.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "An angle (radians) whose hyperbolic tangent to calculate.", DynamicValueType.Number)]
        private static DynamicValue Tanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Tanh(value);
        }

        [RuntimeModuleFunction("trunc")]
        [EvilDocFunction(
            "Truncates the specified number to its integral part.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("value", "A number that is to be truncated.", DynamicValueType.Number)]
        private static DynamicValue Trunc(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Truncate(value);
        }

        [RuntimeModuleFunction("rad2deg")]
        [EvilDocFunction(
            "Converts an angle expressed in radians to an angle expressed in degrees.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("radians", "The angle expressed in radians to be converted to degrees.", DynamicValueType.Number)]
        private static DynamicValue Rad2Deg(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var radians);
            
            return radians * (180.0 / Math.PI);
        }

        [RuntimeModuleFunction("deg2rad")]
        [EvilDocFunction(
            "Converts an angle expressed in degrees to an angle expressed in radians.",
            Returns = null,
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("degrees", "The angle expressed in degrees to be converted to radians.", DynamicValueType.Number)]
        private static DynamicValue Deg2Rad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var degrees);
            
            return degrees * (Math.PI / 180.0);
        }
    }
}