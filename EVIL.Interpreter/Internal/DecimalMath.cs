using System;

// ReSharper disable once IdentifierTypo
namespace EVIL.Interpreter.Internal
{
    internal static class DecimalMath
    {
        public const decimal Pi = 3.14159265358979323846264338327950288419716939937510M;

        public const decimal Epsilon = 0.0000000000000000001M;

        private const decimal PIx2 = 6.28318530717958647692528676655900576839433879875021M;

        public const decimal E = 2.7182818284590452353602874713526624977572470936999595749M;

        private const decimal PIdiv2 = 1.570796326794896619231321691639751442098584699687552910487M;

        private const decimal PIdiv4 = 0.785398163397448309615660845819875721049292349843776455243M;

        private const decimal Einv = 0.3678794411714423215955237701614608674458111310317678M;

        private const decimal Log10Inv = 0.434294481903251827651128918916605082294397005803666566114M;

        public const decimal Zero = 0.0M;

        public const decimal One = 1.0M;

        private const decimal Half = 0.5M;

        private const int MaxIteration = 100;

        public static decimal Exp(decimal x)
        {
            var count = 0;

            if (x > One)
            {
                count = decimal.ToInt32(decimal.Truncate(x));
                x -= decimal.Truncate(x);
            }

            if (x < Zero)
            {
                count = decimal.ToInt32(decimal.Truncate(x) - 1);
                x = One + (x - decimal.Truncate(x));
            }

            var iteration = 1;
            var result = One;
            var factorial = One;
            decimal cachedResult;
            do
            {
                cachedResult = result;
                factorial *= x / iteration++;
                result += factorial;
            } while (cachedResult != result);

            if (count == 0)
                return result;
            return result * PowerN(E, count);
        }

        public static decimal Pow(decimal value, decimal power)
        {
            if (power == Zero) return One;
            if (power == One) return value;
            if (value == One) return One;

            if (value == Zero && power == Zero) return One;

            if (value == Zero)
            {
                if (power > Zero)
                {
                    return Zero;
                }

                throw new Exception("Invalid Operation: zero base and negative power");
            }

            if (power == -One) return One / value;

            var isPowerInteger = IsInteger(power);
            if (value < Zero && !isPowerInteger)
            {
                throw new Exception("Invalid Operation: negative base and non-integer power");
            }

            if (isPowerInteger && value > Zero)
            {
                int powerInt = (int)power;
                return PowerN(value, powerInt);
            }

            if (isPowerInteger && value < Zero)
            {
                int powerInt = (int)power;
                if (powerInt % 2 == 0)
                {
                    return Exp(power * Log(-value));
                }

                return -Exp(power * Log(-value));
            }

            return Exp(power * Log(value));
        }

        private static bool IsInteger(decimal value)
        {
            var longValue = (long)value;
            return Abs(value - longValue) <= Epsilon;
        }

        public static decimal PowerN(decimal value, int power)
        {
            while (true)
            {
                if (power == Zero) return One;
                if (power < Zero)
                {
                    value = One / value;
                    power = -power;
                    continue;
                }

                var q = power;
                var prod = One;
                var current = value;
                while (q > 0)
                {
                    if (q % 2 == 1)
                    {
                        // detects the 1s in the binary expression of power
                        prod = current * prod; // picks up the relevant power
                        q--;
                    }

                    current *= current; // value^i -> value^(2*i)
                    q >>= 1;
                }

                return prod;
            }
        }

        public static decimal Log10(decimal x)
        {
            return Log(x) * Log10Inv;
        }

        public static decimal Log(decimal x)
        {
            if (x <= Zero)
            {
                throw new ArgumentException("x must be greater than zero");
            }
            var count = 0;
            while (x >= One)
            {
                x *= Einv;
                count++;
            }
            while (x <= Einv)
            {
                x *= E;
                count--;
            }
            x--;
            if (x == Zero) return count;
            var result = Zero;
            var iteration = 0;
            var y = One;
            var cacheResult = result - One;
            while (cacheResult != result && iteration < MaxIteration)
            {
                iteration++;
                cacheResult = result;
                y *= -x;
                result += y / iteration;
            }
            return count - result;
        }

        public static decimal Cos(decimal x)
        {
            //truncating to  [-2*PI;2*PI]
            TruncateToPeriodicInterval(ref x);
            
            // now x in (-2pi,2pi)
            if (x >= Pi && x <= PIx2)
            {
                return -Cos(x - Pi);
            }
            if (x >= -PIx2 && x <= -Pi)
            {
                return -Cos(x + Pi);
            }

            x *= x;
            //y=1-x/2!+x^2/4!-x^3/6!...
            var xx = -x * Half;
            var y = One + xx;
            var cachedY = y - One;//init cache  with different value
            for (var i = 1; cachedY != y && i < MaxIteration; i++)
            {
                cachedY = y;
                decimal factor = i * ((i << 1) + 3) + 1; //2i^2+2i+i+1=2i^2+3i+1
                factor = -Half / factor;
                xx *= x * factor;
                y += xx;
            }
            return y;
        }

        public static decimal Tan(decimal x)
        {
            var cos = Cos(x);
            if (cos == Zero) throw new ArgumentException(nameof(x));
            //calculate sin using cos
            var sin = CalculateSinFromCos(x, cos);
            return sin / cos;
        }

        private static decimal CalculateSinFromCos(decimal x,decimal cos)
        {
            var moduleOfSin = Sqrt(One - (cos * cos));
            var sineIsPositive = IsSignOfSinePositive(x);
            if (sineIsPositive) return moduleOfSin;
            return -moduleOfSin;
        }

        public static decimal Sin(decimal x)
        {
            var cos = Cos(x);
            return CalculateSinFromCos(x, cos);
        }

        private static void TruncateToPeriodicInterval(ref decimal x)
        {
            while (x >= PIx2)
            {
                var divide = Math.Abs(decimal.ToInt32(x / PIx2));
                x -= divide * PIx2;
            }

            while (x <= -PIx2)
            {
                var divide = Math.Abs(decimal.ToInt32(x / PIx2));
                x += divide * PIx2;
            }
        }


        private static bool IsSignOfSinePositive(decimal x)
        {
            //truncating to  [-2*PI;2*PI]
            TruncateToPeriodicInterval(ref x);
            
            //now x in [-2*PI;2*PI]
            if (x >= -PIx2 && x <= -Pi) return true;
            if (x >= -Pi && x <= Zero) return false;
            if (x >= Zero && x <= Pi) return true;
            if (x >= Pi && x <= PIx2) return false;

            //will not be reached
            throw new ArgumentException(nameof(x));
        }

        public static decimal Sqrt(decimal x, decimal epsilon = Zero)
        {
            if (x < Zero) throw new OverflowException("Cannot calculate square root from a negative number");
            //initial approximation
            decimal current = (decimal)Math.Sqrt((double)x), previous;
            do
            {
                previous = current;
                if (previous == Zero) return Zero;
                current = (previous + x / previous) * Half;
            } while (Abs(previous - current) > epsilon);
            return current;
        }

        public static decimal Sinh(decimal x)
        {
            var y = Exp(x);
            var yy = One / y;
            return (y - yy) * Half;
        }

        public static decimal Cosh(decimal x)
        {
            var y = Exp(x);
            var yy = One / y;
            return (y + yy) * Half;
        }

        public static int Sign(decimal x)
        {
            return x < Zero ? -1 : (x > Zero ? 1 : 0);
        }

        public static decimal Tanh(decimal x)
        {
            var y = Exp(x);
            var yy = One / y;
            return (y - yy) / (y + yy);
        }

        public static decimal Abs(decimal x)
        {
            if (x <= Zero)
            {
                return -x;
            }
            return x;
        }

        public static decimal Asin(decimal x)
        {
            if (x > One || x < -One)
            {
                throw new ArgumentException("x must be in [-1,1]");
            }
            //known values
            if (x == Zero) return Zero;
            if (x == One) return PIdiv2;
            //asin function is odd function
            if (x < Zero) return -Asin(-x);

            //my optimize trick here

            // used a math formula to speed up :
            // asin(x)=0.5*(pi/2-asin(1-2*x*x)) 
            // if x>=0 is true

            var newX = One - 2 * x * x;

            //for calculating new value near to zero than current
            //because we gain more speed with values near to zero
            if (Abs(x) > Abs(newX))
            {
                var t = Asin(newX);
                return Half * (PIdiv2 - t);
            }
            var y = Zero;
            var result = x;
            decimal cachedResult;
            var i = 1;
            y += result;
            var xx = x * x;
            do
            {
                cachedResult = result;
                result *= xx * (One - Half / (i));
                y += result / ((i << 1) + 1);
                i++;
            } while (cachedResult != result);
            return y;
        }

        public static decimal Atan(decimal x)
        {
            if (x == Zero) return Zero;
            if (x == One) return PIdiv4;
            return Asin(x / Sqrt(One + x * x));
        }

        public static decimal Acos(decimal x)
        {
            if (x == Zero) return PIdiv2;
            if (x == One) return Zero;
            if (x < Zero) return Pi - Acos(-x);
            return PIdiv2 - Asin(x);
        }

        public static decimal Atan2(decimal y, decimal x)
        {
            if (x > Zero)
            {
                return Atan(y / x);
            }
            if (x < Zero && y >= Zero)
            {
                return Atan(y / x) + Pi;
            }
            if (x < Zero && y < Zero)
            {
                return Atan(y / x) - Pi;
            }
            if (x == Zero && y > Zero)
            {
                return PIdiv2;
            }
            if (x == Zero && y < Zero)
            {
                return -PIdiv2;
            }
            throw new ArgumentException("invalid atan2 arguments");
        }
    }
}