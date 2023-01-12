namespace EVIL.Grammar.AST.Nodes
{
    public class StringConstant : ConstantExpression
    {
        public string Value { get; }

        public StringConstant(string value)
        {
            Value = value;
        }
    }
}