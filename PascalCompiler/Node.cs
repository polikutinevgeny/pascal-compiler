using System.Collections.Generic;

namespace PascalCompiler
{
    public abstract class Node
    {
        protected Node(List<Node> childs, object value, uint line, uint position)
        {
            Childs = childs;
            Value = value;
            Line = line;
            Position = position;
        }

        protected Node(List<Node> childs, Tokenizer.Token token) : this(childs, token.Value, token.Line, token.Position)
        {
        }

        public List<Node> Childs { get; set; }
        public object Value { get; set; }
        public uint Line { get; set; }
        public uint Position { get; set; }

        public override string ToString()
        {
            return Value?.ToString() ?? GetType().Name;
        }
    }

    public class ExpressionNode : Node
    {
        public Symbol Type { get; set; }

        public ExpressionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class CompoundStatementNode : StructStatementNode
    {
        public List<Statement> Statements { get; set; }

        public CompoundStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }
    }

    public class Statement : Node
    {
        public Statement(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class SimpleStatementNode : Statement
    {
        public SimpleStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }
    }

    public class StructStatementNode : Statement
    {
        public StructStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }
    }

    public class ReadStatementNode : SimpleStatementNode
    {
        public ReadStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class WriteStatementNode : SimpleStatementNode
    {
        public WriteStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class IfStatementNode : StructStatementNode
    {
        public IfStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class ForStatementNode : StructStatementNode
    {
        public ForStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class WhileStatementNode : StructStatementNode
    {
        public Node Condition { get; set; }

        public WhileStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class RepeatStatementNode : StructStatementNode
    {
        public Node Condition { get; set; }

        public RepeatStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }
    }

    public class DesignatorNode : ExpressionNode
    {
        public DesignatorNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class IdentNode : DesignatorNode
    {
        public IdentNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }
    }

    public class ExpressionListNode : Node
    {
        public ExpressionListNode(List<ExpressionNode> childs, object value, uint line, uint position) : base(
            new List<Node>(childs), value, line, position)
        {
        }
    }

    public class StringNode : Node
    {
        public StringNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class BinOpNode : ExpressionNode
    {
        public BinOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class UnOpNode : ExpressionNode
    {
        public UnOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class ConstNode : ExpressionNode
    {
        public ConstNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }
    }

    public class BreakNode : Statement
    {
        public BreakNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }
    }

    public class ContinueNode : Statement
    {
        public ContinueNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }
    }

    public class ExitNode : Statement
    {
        public ExitNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class CallOperator : DesignatorNode
    {
        public SubprogramSymbol Subprogram { get; set; }

        public CallOperator(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override string ToString()
        {
            return $"{Subprogram.Name} call";
        }
    }

    public class CallStatementWrapper : SimpleStatementNode
    {
        public CallStatementWrapper(CallOperator callOperator) : base(new List<Node> {callOperator}, callOperator.Value,
            callOperator.Line, callOperator.Position)
        {
        }

        public override string ToString()
        {
            return "Call statement wrapper";
        }
    }

    public class CastOperator : DesignatorNode
    {
        public CastOperator(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override string ToString()
        {
            return $"{Type.Name} cast";
        }
    }

    public class IndexOperator : DesignatorNode
    {

        public IndexOperator(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override string ToString()
        {
            return "Index";
        }
    }

    public class MemberAccessOperator : DesignatorNode
    {

        public MemberAccessOperator(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }

        public override string ToString()
        {
            return "Member access";
        }
    }
}