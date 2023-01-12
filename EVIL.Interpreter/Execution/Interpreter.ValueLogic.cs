using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {        
        public override DynValue Visit(NumberNode numberNode)
        {
            return new(numberNode.Value);
        }

        public override DynValue Visit(StringNode stringNode)
        {
            return new(stringNode.Value);
        }

        public override DynValue Visit(TableNode tableNode)
        {
            var tbl = new Table();
            var value = new DynValue(tbl);

            _currentThisContextStack.Push(value);
            if (tableNode.Keyed)
            {
                for (var i = 0; i < tableNode.Initializers.Count; i++)
                {
                    var node = (AssignmentNode)tableNode.Initializers[i];
                    var key = Visit(node.Left);

                    if (tbl.ContainsKey(tbl.GetKeyByDynValue(key)))
                    {
                        throw new RuntimeException(
                            $"Attempt to initialize a table with the same key '{key.AsString().String}' twice.",
                            Environment,
                            node.Line
                        );
                    }
                    
                    tbl[key] = Visit(node.Right);
                }
            }
            else
            {
                for (var i = 0; i < tableNode.Initializers.Count; i++)
                {
                    tbl[i] = Visit(tableNode.Initializers[i]);
                }
            }
            _currentThisContextStack.Pop();

            return value;
        }
    }
}