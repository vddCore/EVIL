using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public sealed class StringModule : RuntimeModule
    {
        public override string FullyQualifiedName => "str";

        [RuntimeModuleGetter("empty")]
        private static DynamicValue EmptyString(DynamicValue _)
            => string.Empty;

        [RuntimeModuleFunction("chr")]
        private static DynamicValue ToCharacter(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAt(0, out var num);

            if (num < 0 || num >= ushort.MaxValue)
            {
                throw new EvilRuntimeException(
                    "Expected a number in range of 0-65535"
                );
            }

            return ((char)num).ToString();
        }

        [RuntimeModuleFunction("bytes")]
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
        private static DynamicValue Explode(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var str);

            var chars = str.ToCharArray();
            var array = new Array(chars.Length);
            
            for (var i = 0; i < chars.Length; i++)
            {
                array[i] = chars[i];
            }

            return array;
        }
        
        [RuntimeModuleFunction("spl")]
        private static DynamicValue Split(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var src)
                .ExpectStringAt(1, out var delim);

            var segments = src.Split(delim);
            var array = new Array(segments.Length);

            for (var i = 0; i < segments.Length; i++)
            {
                array[i] = segments[i];
            }

            return array;
        }

        [RuntimeModuleFunction("join")]
        private static DynamicValue Join(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out var delim);

            return string.Join(delim, args.Skip(1).Select(x => x.ConvertToString().String!));
        }

        [RuntimeModuleFunction("rep")]
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
        private static DynamicValue IndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var haystack)
                .ExpectStringAt(1, out var needle);
            
            return haystack.IndexOf(needle, StringComparison.InvariantCulture);
        }
        
        [RuntimeModuleFunction("last_index_of")]
        private static DynamicValue LastIndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var haystack)
                .ExpectStringAt(1, out var needle);
            
            return haystack.LastIndexOf(needle, StringComparison.InvariantCulture);
        }

        [RuntimeModuleFunction("is_empty")]
        private static DynamicValue IsEmpty(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var value);

            return string.IsNullOrEmpty(value);
        }
        
        [RuntimeModuleFunction("is_whitespace")]
        private static DynamicValue IsWhiteSpace(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var value);

            return string.IsNullOrWhiteSpace(value);
        }

        [RuntimeModuleFunction("lpad")]
        private static DynamicValue LeftPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var source)
                .ExpectCharAt(1, out var pad)
                .ExpectIntegerAt(2, out var totalWidth);

            return source.PadLeft((int)totalWidth, pad);
        }
        
        [RuntimeModuleFunction("rpad")]
        private static DynamicValue RightPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var source)
                .ExpectCharAt(1, out var pad)
                .ExpectIntegerAt(2, out var totalWidth);

            return source.PadRight((int)totalWidth, pad);
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
                .ExpectAtMost(3)
                .ExpectStringAt(0, out var source)
                .ExpectIntegerAt(1, out var startIndex)
                .OptionalIntegerAt(2, defaultValue: -1, out var endIndex);

            if (endIndex > 0)
            {
                if (startIndex > endIndex)
                    return Nil;

                try
                {
                    return source.Substring(
                        (int)startIndex,
                        (int)(source.Length - endIndex)
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
                    { "groups", new Table() }
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
                    
                    matchTable["groups"].Table!.Add(
                        matchTable["groups"].Table!.Length,
                        groupTable
                    );
                }

                array[i] = matchTable;
            }

            return array;
        }
    }
}