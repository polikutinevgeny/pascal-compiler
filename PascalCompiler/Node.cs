using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    public abstract class Node
    {
        public Node(List<Node> childs, object value, uint line, uint position)
        {
            Childs = childs;
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

    public class ConstExprNode : ExpressionNode
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

    public class ArrayConstantNode : TypedConstantNode
    {
        public ArrayConstantNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RecordConstantNode : TypedConstantNode
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

    public class SubrangeNode : Node
    {
        public SubrangeNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
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

    public class VarNode : Node
    {
        public VarNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class FieldNode : Node
    {
        public FieldNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class RecordFieldListNode : Node
    {
        public RecordFieldListNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class FieldDeclNode : Node
    {
        public FieldDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class IdentListNode : Node
    {
        public IdentListNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ProcedureDeclNode : Node
    {
        public ProcedureDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ProcedureHeadingNode : Node
    {
        public ProcedureHeadingNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class FormalParametersNode : Node
    {
        public FormalParametersNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class FormalParamNode : Node
    {
        public FormalParamNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ParameterNode : FormalParamNode
    {
        public ParameterNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class VarParameterNode : FormalParamNode
    {
        public VarParameterNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstParameterNode : FormalParamNode
    {
        public ConstParameterNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ParamNode : Node
    {
        public ParamNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class FunctionDeclNode : Node
    {
        public FunctionDeclNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class FunctionHeadingNode : Node
    {
        public FunctionHeadingNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class CompoundStatementNode : StructStatementNode
    {
        public CompoundStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class StatementListNode : Node
    {
        public StatementListNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class StatementNode : Node
    {
        public StatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class SimpleStatementNode : StatementNode
    {
        public SimpleStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class StructStatementNode : StatementNode
    {
        public StructStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ReadStatementNode : SimpleStatementNode
    {
        public ReadStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class WriteStatementNode : SimpleStatementNode
    {
        public WriteStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class IfStatementNode : StructStatementNode
    {
        public IfStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ForStatementNode : StructStatementNode
    {
        public ForStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class WhileStatementNode : StructStatementNode
    {
        public WhileStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RepeatStatementNode : StructStatementNode
    {
        public RepeatStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class DesignatorNode : Node
    {
        public DesignatorNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class IdentNode : SimpleExpressionNode
    {
        public IdentNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line, token.Position)
        {
        }
    }

    public class ExpressionListNode : Node
    {
        public ExpressionListNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class StringNode : Node
    {
        public StringNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class SimpleExpressionNode : Node
    {
        public SimpleExpressionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class BinOpNode : SimpleExpressionNode
    {
        public BinOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class UnOpNode : SimpleExpressionNode
    {
        public UnOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstNode : Node
    {
        public ConstNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class CallNode : Node
    {
        public CallNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class BreakNode : StatementNode
    {
        public BreakNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line, token.Position)
        {
        }
    }

    public class ContinueNode : StatementNode
    {
        public ContinueNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line, token.Position)
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
            for (var i = 0; i < node?.Childs?.Count; ++i)
            {
                PrintTree(writer, node.Childs[i], indent, i == node.Childs.Count - 1);
            }
        }
    }
}