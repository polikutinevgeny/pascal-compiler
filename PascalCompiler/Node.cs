using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    public abstract class Node
    {
        public Node(List<Node> childs, object value, uint line, uint position)
        {
            Value = value;
            Line = line;
            Position = position;
        }

        public Node(List<Node> childs, Tokenizer.Token token) : this(childs, token.Value, token.Line, token.Position)
        {
            
        }

        public List<Node> Childs { get; set; }
        public object Value { get; set; }
        public uint Line { get; set; }
        public uint Position { get; set; }

        public override string ToString()
        {
            return Value?.ToString() ?? this.GetType().Name;
        }
    }

    public class ProgramNode : Node
    {
        public ProgramNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class BlockNode : Node
    {
        public BlockNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class DeclSectionNode : Node
    {
        public DeclSectionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstSectionNode : Node
    {
        public ConstSectionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstantDeclNode : Node
    {
        public string Type { get; set; }
        public ConstantDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class TypedConstantNode : Node
    {
        public TypedConstantNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstExprNode : Node
    {
        public ConstExprNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ExpressionNode : Node
    {
        public ExpressionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ArrayConstantNode : Node
    {
        public ArrayConstantNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RecordConstantNode : Node
    {
        public RecordConstantNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RecordFieldConstantNode : Node
    {
        public RecordFieldConstantNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class TypeSectionNode : Node
    {
        public TypeSectionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class TypeDeclNode : Node
    {
        public TypeDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class TypeNode : Node
    {
        public TypeNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ArrayTypeNode : TypeNode
    {
        public ArrayTypeNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RecordTypeNode : TypeNode
    {
        public RecordTypeNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class VarSectionNode : Node
    {
        public VarSectionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class VarDeclNode : Node
    {
        public VarDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ProcedureDeclNode : Node
    {
        public ProcedureDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class FunctionDeclNode : Node
    {
        public FunctionDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class CompoundStatementNode : Node
    {
        public CompoundStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public static class TreePrinter
    {
        public static void PrintTree(StreamWriter writer, Node node, string indent, bool last)
        {
            writer.Write(indent);
            if (last)
            {
                writer.Write("└─");
                indent += "  ";
            }
            else
            {
                writer.Write("├─");
                indent += "│ ";
            }
            writer.WriteLine(node);
            for (var i = 0; i < node.Childs?.Count; ++i)
            {
                PrintTree(writer, node.Childs[i], indent, i == node.Childs.Count - 1);
            }
        }
    }
}