﻿using System;

namespace EVIL.Interpreter.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ClrFunctionAttribute : Attribute
    {
        public string Name { get; }

        public ClrFunctionAttribute(string name)
            => Name = name;
    }
}