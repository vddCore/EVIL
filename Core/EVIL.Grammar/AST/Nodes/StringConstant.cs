namespace EVIL.Grammar.AST.Nodes
{
    public class StringConstant : Expression
    {
        public string Value { get; }

        public StringConstant(string value)
        {
            Value = value;
        }
    }
}