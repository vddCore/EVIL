using EVIL.Grammar.AST.Statements.TopLevel;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(AttributeList attributeList)
        {
            foreach (var attr in attributeList.Attributes)
                Visit(attr);
        }
    }
}