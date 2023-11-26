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
        [EvilDocFunction(
            "Converts a character to its 16-bit Unicode character code representation.",
            Returns = "A 16-bit Unicode character code.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument("chr", "A single-character string whose character code to retrieve.", DynamicValueType.String)]
        private static DynamicValue ToCharacterCode(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectCharAt(0, out var chr);

            return (ushort)chr;
        }

        [RuntimeModuleFunction("replace")]
        [EvilDocFunction(
            "Replaces a provided String within the provided String with another String.",
            Returns = "The modified string.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be modified.", DynamicValueType.String)]
        [EvilDocArgument("to_replace", "A String to look for in order to replace.", DynamicValueType.String)]
        [EvilDocArgument("with", "A String to serve as a replacement.", DynamicValueType.String)]
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
        [EvilDocFunction(
            "Removes the provided characters from both ends of the given String.",
            Returns = "A String with the provided characters removed from both ends of the given String.",
            ReturnType = DynamicValueType.String,
            IsVariadic = true
        )]
        [EvilDocArgument("str", "A String to be trimmed.", DynamicValueType.String)]
        [EvilDocArgument("...", "Zero or more single-character Strings. If none are provided, whitespace are assumed.", DynamicValueType.String)]
        private static DynamicValue Trim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var str);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return str.Trim(chars.ToArray());
        }
        
        [RuntimeModuleFunction("ltrim")]
        [EvilDocFunction(
            "Removes the provided characters from the start of the given String.",
            Returns = "A String with the provided characters removed from the start of the given String.",
            ReturnType = DynamicValueType.String,
            IsVariadic = true
        )]
        [EvilDocArgument("str", "A String to be trimmed.", DynamicValueType.String)]
        [EvilDocArgument("...", "__At least one__ single-character String.", DynamicValueType.String)]
        private static DynamicValue LeftTrim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var str);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return str.TrimStart(chars.ToArray());
        }
        
        [RuntimeModuleFunction("rtrim")]
        [EvilDocFunction(
            "Removes the provided characters from the end of the given String.",
            Returns = "A String with the provided characters removed from the end of the given String.",
            ReturnType = DynamicValueType.String,
            IsVariadic = true
        )]
        [EvilDocArgument("str", "A String to be trimmed.", DynamicValueType.String)]
        [EvilDocArgument("...", "__At least one__ single-character String.", DynamicValueType.String)]
        private static DynamicValue RightTrim(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var str);

            var chars = new char[args.Length - 1];
            if (args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    args.ExpectCharAt(i, out chars[i - 1]);
                }
            }

            return str.TrimEnd(chars.ToArray());
        }
        
        [RuntimeModuleFunction("ucase")]
        [EvilDocFunction(
            "Converts all characters in the provided String to upper case and returns the result.",
            Returns = "The upper-case version of the provided String.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be converted.", DynamicValueType.String)]
        private static DynamicValue UpperCase(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var str);

            return str.ToUpper(CultureInfo.InvariantCulture);
        }
        
        [RuntimeModuleFunction("lcase")]
        [EvilDocFunction(
            "Converts all characters in the provided String to lower case and returns the result.",
            Returns = "The lower-case version of the provided String.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to be converted.", DynamicValueType.String)]
        private static DynamicValue LowerCase(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var source);

            return source.ToLower(CultureInfo.InvariantCulture);
        }

        [RuntimeModuleFunction("sub")]
        [EvilDocFunction(
            "Finds a portion of the provided String using a provided start index and optionally an end index.",
            Returns = "A portion of the provided String using the provided indices, or `nil` on failure.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("str", "A String to search for the substring in.", DynamicValueType.String)]
        [EvilDocArgument("start_index", "Starting index of the substring within the provided String.", DynamicValueType.Number)]
        [EvilDocArgument(
            "end_index",
            "Ending index of the substring within the provided String.", 
            DynamicValueType.Number,
            DefaultValue = "-1"
        )]
        [EvilDocArgument(
            "end_inclusive",
            "Specifies whether the ending index is inclusive (i.e. the character at it should be included in the returned substring) or not.", 
            DynamicValueType.Boolean, 
            DefaultValue = "false"
        )]
        private static DynamicValue Substring(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(4)
                .ExpectStringAt(0, out var str)
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
                    
                    if (end >= str.Length)
                    {
                        end = str.Length - (int)startIndex;
                    }
                    
                    return str.Substring(
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
                    return str.Substring((int)startIndex);
                }
                catch
                {
                    return Nil;
                }
            }
        }

        [RuntimeModuleFunction("starts_with")]
        [EvilDocFunction(
            "Checks if the provided String starts with the other provided String.",
            Returns = "A Boolean value indicating whether the provided String starts with the other provided String.",
            ReturnType = DynamicValueType.Boolean
        )]
        [EvilDocArgument("str", "A String to check.", DynamicValueType.String)]
        [EvilDocArgument("start", "A String to be matched with the start of `str`.", DynamicValueType.String)]
        private static DynamicValue StartsWith(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectStringAt(1, out var start);

            return str.StartsWith(start);
        }

        [RuntimeModuleFunction("ends_with")]
        [EvilDocFunction(
            "Checks if the provided String ends with the other provided String.",
            Returns = "A Boolean value indicating whether the provided String ends with the other provided String.",
            ReturnType = DynamicValueType.Boolean
        )]
        [EvilDocArgument("str", "A String to check.", DynamicValueType.String)]
        [EvilDocArgument("start", "A String to be matched with the end of `str`.", DynamicValueType.String)]
        private static DynamicValue EndsWith(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectStringAt(1, out var end);

            return str.EndsWith(end);
        }
        
        [RuntimeModuleFunction("rmatch")]
        [EvilDocFunction(
            "Matches the provided String against a regular expression.",
            Returns = "An Array containing matches for the provided regex, assuming any have been found, or `nil` on failure.  \n" +
                      "" +
                      "If the returned Array is not empty, it will contain one or more tables structured as follows:  \n" +
                      "```\n" +
                      "{\n" +
                      "  success: Boolean\n" +
                      "  name: String\n" +
                      "  value: String\n" +
                      "  starts_at: Number\n" +
                      "  length: Number\n" +
                      "  groups: Array\n" +
                      "}\n" +
                      "```\n" +
                      "" +
                      "If the match contains any groups, the `groups` Array will contain one or more tables structured as follows:  \n" +
                      "```\n" +
                      "{\n" +
                      "  success: Boolean\n" +
                      "  name: String\n" +
                      "  value: String\n" +
                      "  starts_at: Number\n" +
                      "  length: Number\n" +
                      "}\n" +
                      "```\n",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("str", "A String to be matched.", DynamicValueType.String)]
        [EvilDocArgument("regex", "A String containing the regular expression to match `str` against.", DynamicValueType.String)]
        private static DynamicValue RegexMatch(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectStringAt(1, out var regex);

            if (!Regex.IsMatch(str, regex))
                return Nil;

            var matches = Regex.Matches(str, regex);
            var array = new Array(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                
                var matchTable = new Table()
                {
                    { "success", match.Success },
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
                    
                    groupTable.Freeze(true);
                    matchTable["groups"].Array!.Push(groupTable);
                }

                matchTable.Freeze(true);
                array[i] = matchTable;
            }

            return array;
        }

        [RuntimeModuleFunction("md5")]
        [EvilDocFunction(
            "Calculates a checksum of the provided String, applying the provided encoding.",
            Returns = "A String containing the computed checksum of the provided String.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument("text", "A String whose checksum to compute.", DynamicValueType.String)]
        [EvilDocArgument(
            "encoding",
            "Name of the encoding to be used during computation.",
            DynamicValueType.String,
            DefaultValue = "utf-8"
        )]
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