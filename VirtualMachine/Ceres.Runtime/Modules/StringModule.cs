using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public sealed class StringModule : RuntimeModule
    {
        public override string FullyQualifiedName => "str";

        [RuntimeModuleGetter("empty")]
        [EvilDocProperty(EvilDocPropertyMode.Get, "An empty string.", ReturnType = DynamicValueType.String)]
        private static DynamicValue EmptyString(DynamicValue _)
            => string.Empty;

        [RuntimeModuleFunction("chr")]
        [EvilDocFunction(
            "Converts a character code into its String representation.",
            Returns = "The converted character code expressed as a String, or `nil` on error.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("char_code", "A character code to be converted into a String.", DynamicValueType.String)]
        private static DynamicValue ToCharacter(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAt(0, out var charCode);

            if (charCode < 0 || charCode >= ushort.MaxValue)
            {
                return Nil;
            }

            return ((char)charCode).ToString();
        }
        
        [RuntimeModuleFunction("code")]
        private static DynamicValue ToCharacterCode(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectCharAt(0, out var character);

            return (ushort)character;
        }

        [RuntimeModuleFunction("replace")]
        private static DynamicValue Replace(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var str)
                .ExpectStringAt(1, out var toReplace)
                .ExpectStringAt(2, out var with);

            return str.Replace(toReplace, with);
        }

        [RuntimeModuleFunction("bytes")]
        [EvilDocFunction(
            "Converts a String to its byte representation in the specified encoding.",
            Returns = "An Array of bytes representing the provided String in the specified encoding.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("str", "A String that is to be converted.", DynamicValueType.String)]
        [EvilDocArgument(
            "encoding",
            "Encoding to be used when converting the String to bytes.", 
            DynamicValueType.String,
            DefaultValue = "utf-8"
        )]
        private static DynamicValue ToBytes(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var str)
                .OptionalStringAt(1, "utf-8", out var encoding);

            var bytes = Encoding.GetEncoding(encoding).GetBytes(str);
            var array = new Array(bytes.Length);

            for (var i = 0; i < bytes.Length; i++)
            {
                array[i] = bytes[i];
            }

            return array;
        }

        [RuntimeModuleFunction("explode")]
        [EvilDocFunction(
            "Puts each character of the provided String into an Array.",
            Returns = "An Array containing every character of the provided String.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("str", "A String to explode.", DynamicValueType.String)]
        private static DynamicValue Explode(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var str);

            var array = Array.FromString(str);

            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == "💣")
                {
                    array[i] = "💥";
                }
            }

            return array;
        }
        
        [RuntimeModuleFunction("spl")]
        [EvilDocFunction(
            "Splits the provided String into segments by the given separator.",
            Returns = "Segments of the provided String after splitting by the given separator.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("str", "A String to split.", DynamicValueType.String)]
        [EvilDocArgument("delim", "A String to split `str` by.", DynamicValueType.String)]
        private static DynamicValue Split(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectStringAt(1, out var separator);

            var segments = str.Split(separator);
            var array = new Array(segments.Length);

            for (var i = 0; i < segments.Length; i++)
            {
                array[i] = segments[i];
            }

            return array;
        }

        [RuntimeModuleFunction("join")]
        [EvilDocFunction(
            "Concatenates the provided values into a String using the provided separator. " +
            "The values provided will be converted to their String representations.",
            Returns = "A concatenated String with the given separator inserted between each provided value.",
            ReturnType = DynamicValueType.String,
            IsVariadic = true
        )]
        [EvilDocArgument("separator", "A separator to be inserted between each given value during concatenation.", DynamicValueType.String)]
        [EvilDocArgument("...", "An arbitrary amount of values to be concatenated into a String.")]
        private static DynamicValue Join(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var separator);

            return string.Join(separator, args.Skip(1).Select(x => x.ConvertToString().String!));
        }

        [RuntimeModuleFunction("repeat")]
        [EvilDocFunction(
            "Repeats the provided String `count` times.",
            Returns = "A String containing `str` repeated `count` times.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be repeated.", DynamicValueType.String)]
        [EvilDocArgument("count", "An integer specifying the amount of times to repeat `str`.", DynamicValueType.Number)]
        private static DynamicValue Repeat(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectIntegerAt(1, out var count);

            var sb = new StringBuilder();
            for (var i = 0; i < (int)count; i++) 
                sb.Append(str);

            return sb.ToString();
        }

        [RuntimeModuleFunction("index_of")]
        [EvilDocFunction(
            "Finds the zero-based starting index of the first occurrence of `needle` in `haystack`.",
            Returns = "The starting index of the first occurrence of `needle` in `haystack`, or `-1` if not found.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("haystack", "A String to be searched through.", DynamicValueType.String)]
        [EvilDocArgument("needle", "A String to seek for.", DynamicValueType.String)]
        private static DynamicValue IndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var haystack)
                .ExpectStringAt(1, out var needle);
            
            return haystack.IndexOf(needle, StringComparison.InvariantCulture);
        }
        
        [RuntimeModuleFunction("last_index_of")]
        [EvilDocFunction(
            "Finds the zero-based starting index of the last occurrence of `needle` in `haystack`.",
            Returns = "The starting index of the last occurrence of `needle` in `haystack`, or `-1` if not found.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("haystack", "A String to be searched through.", DynamicValueType.String)]
        [EvilDocArgument("needle", "A String to seek for.", DynamicValueType.String)]
        private static DynamicValue LastIndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var haystack)
                .ExpectStringAt(1, out var needle);
            
            return haystack.LastIndexOf(needle, StringComparison.InvariantCulture);
        }

        [RuntimeModuleFunction("is_whitespace")]
        [EvilDocFunction(
            "Checks if the provided String consists only of whitespace charracters.",
            Returns = "`true` if the provided String consists only of whitespace characterrs, `false` otherwise.",
            ReturnType = DynamicValueType.Boolean)]
        [EvilDocArgument("str", "A String to be checked.", DynamicValueType.String)]
        private static DynamicValue IsWhiteSpace(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var str);

            return string.IsNullOrWhiteSpace(str);
        }

        [RuntimeModuleFunction("lpad")]
        [EvilDocFunction(
            "Pads a shorter `str` so that its length matches `total_width` by appeding `padding_char` to its left side.",
            Returns = "A String padded in the way described above.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be padded.", DynamicValueType.String)]
        [EvilDocArgument("padding_char", "A character to be used for padding.", DynamicValueType.String)]
        [EvilDocArgument("total_width", "Total length of the string to be matched.", DynamicValueType.Number)]
        private static DynamicValue LeftPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var str)
                .ExpectCharAt(1, out var paddingChar)
                .ExpectIntegerAt(2, out var totalWidth);

            return str.PadLeft((int)totalWidth, paddingChar);
        }
        
        [RuntimeModuleFunction("rpad")]
        [EvilDocFunction(
            "Pads a shorter `str` so that its length matches `total_width` by appeding `padding_char` to its right side.",
            Returns = "A String padded in the way described above.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be padded.", DynamicValueType.String)]
        [EvilDocArgument("padding_char", "A character to be used for padding.", DynamicValueType.String)]
        [EvilDocArgument("total_width", "Total length of the string to be matched.", DynamicValueType.Number)]
        private static DynamicValue RightPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var str)
                .ExpectCharAt(1, out var pad)
                .ExpectIntegerAt(2, out var totalWidth);

            return str.PadRight((int)totalWidth, pad);
        }
        
        [RuntimeModuleFunction("trim")]
        private static DynamicValue Trim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var source);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return source.Trim(chars.ToArray());
        }
        
        [RuntimeModuleFunction("ltrim")]
        private static DynamicValue LeftTrim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var source);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return source.TrimStart(chars.ToArray());
        }
        
        [RuntimeModuleFunction("rtrim")]
        private static DynamicValue RightTrim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var source);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return source.TrimEnd(chars.ToArray());
        }
        
        [RuntimeModuleFunction("ucase")]
        private static DynamicValue UpperCase(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var source);

            return source.ToUpper(CultureInfo.InvariantCulture);
        }
        
        [RuntimeModuleFunction("lcase")]
        private static DynamicValue LowerCase(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var source);

            return source.ToLower(CultureInfo.InvariantCulture);
        }

        [RuntimeModuleFunction("sub")]
        private static DynamicValue Substring(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(4)
                .ExpectStringAt(0, out var source)
                .ExpectIntegerAt(1, out var startIndex)
                .OptionalIntegerAt(2, defaultValue: -1, out var endIndex)
                .OptionalBooleanAt(3, defaultValue: false, out var endInclusive);

            if (endIndex >= 0)
            {
                if (startIndex > endIndex)
                    return Nil;

                try
                {
                    var end = (int)(endIndex - startIndex + 1);

                    if (!endInclusive)
                    {
                        end--;
                    }
                    
                    if (end >= source.Length)
                    {
                        end = source.Length - (int)startIndex;
                    }
                    
                    return source.Substring(
                        (int)startIndex,
                        end
                    );
                }
                catch
                {
                    return Nil;
                }
            }
            else
            {
                try
                {
                    return source.Substring((int)startIndex);
                }
                catch
                {
                    return Nil;
                }
            }
        }

        [RuntimeModuleFunction("starts_with")]
        private static DynamicValue StartsWith(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var source)
                .ExpectStringAt(1, out var start);

            return source.StartsWith(start);
        }

        [RuntimeModuleFunction("ends_with")]
        private static DynamicValue EndsWith(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var source)
                .ExpectStringAt(1, out var end);

            return source.EndsWith(end);
        }
        
        [RuntimeModuleFunction("rmatch")]
        private static DynamicValue RegexMatch(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var source)
                .ExpectStringAt(1, out var regex);

            if (!Regex.IsMatch(source, regex))
                return Nil;

            var matches = Regex.Matches(source, regex);
            var array = new Array(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                
                var matchTable = new Table
                {
                    { "name", match.Name },
                    { "value", match.Value },
                    { "starts_at", match.Index },
                    { "length", match.Length },
                    { "groups", new Array(0) }
                };
                
                foreach (Group group in match.Groups)
                {
                    var groupTable = new Table
                    {
                        { "name", group.Name },
                        { "value", group.Value },
                        { "starts_at", group.Index },
                        { "length", group.Length }
                    };
                    
                    matchTable["groups"].Array!.Push(groupTable);
                }

                array[i] = matchTable;
            }

            return array;
        }

        [RuntimeModuleFunction("md5")]
        private static DynamicValue Md5(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var text)
                .OptionalStringAt(1, "utf-8", out var encoding);

            var bytes = Encoding.GetEncoding(encoding).GetBytes(text);
            var hash = MD5.HashData(bytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}