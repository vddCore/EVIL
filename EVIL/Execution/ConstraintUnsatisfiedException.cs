using System;

namespace EVIL.Execution
{
    public class ConstraintUnsatisfiedException : Exception
    {
        public Constraint Constraint { get; }

        public ConstraintUnsatisfiedException(string message, Constraint constraint)
            : base(message)
        {
            Constraint = constraint;
        }
    }
}