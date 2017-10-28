﻿using System.Collections.Generic;

namespace PascalCompiler
{
    public abstract class Node
    {
        public Node() {}

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

    public class ExpressionNode : Node
    {
        public TypeSymbol Type { get; set; }
        public ExpressionNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class CompoundStatementNode : StructStatementNode
    {
        public List<Statement> Statements { get; set; }
        public CompoundStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class Statement : Node
    {
        public Statement(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class SimpleStatementNode : Statement
    {
        public SimpleStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class StructStatementNode : Statement
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
        public Node Condition { get; set; }
        public WhileStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class RepeatStatementNode : StructStatementNode
    {
        public Node Condition { get; set; }
        public RepeatStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }
    }

    public class DesignatorNode : Node
    {
        public TypeSymbol Type { get; set; }
        public List<ExpressionNode> Indexes { get; set; }
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

    public class BreakNode : Statement
    {
        public BreakNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line, token.Position)
        {
        }
    }

    public class ContinueNode : Statement
    {
        public ContinueNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line, token.Position)
        {
        }
    }
}