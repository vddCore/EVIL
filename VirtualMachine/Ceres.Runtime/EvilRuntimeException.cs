﻿using System;

namespace Ceres.Runtime
{
    public class EvilRuntimeException : Exception
    {
        public EvilRuntimeException(string message)
            : base(message)
        {
        }
        
        public EvilRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}