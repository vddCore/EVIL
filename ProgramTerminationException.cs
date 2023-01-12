using System;

namespace EVIL
{
    public class ProgramTerminationException : Exception
    {
        public ProgramTerminationException(string message) : base(message)
        {

        }
    }
}
