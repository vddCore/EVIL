namespace EVIL.Grammar.AST.Nodes
{
    public class NumberExpression : Expression
    {
        public double Value { get; }

        public NumberExpression(double value)
        {
            Value = value;
        }
    }
}