using System;
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

        public abstract void Generate(AsmCode asmCode, SymTable symTable);
    }

    public abstract class ExpressionNode : Node
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public abstract class Statement : Node
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            var left = (DesignatorNode) Childs[0];
            var right = (ExpressionNode) Childs[1];
            if (left is IdentNode id)
            {
                var v = (VarSymbol) symTable[id.Value];
                right.Generate(asmCode, symTable);
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(new AsmCmd2(AsmCmd.Cmd.Mov, new AsmMem(v.Offset, v.Type.Size), new AsmReg(AsmReg.Reg.Eax))); //Should work for ints (maybe)
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class StructStatementNode : Statement
    {
        public StructStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value,
            line, position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable) => throw new System.NotImplementedException();
    }

    public class ReadStatementNode : SimpleStatementNode
    {
        public ReadStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class WriteStatementNode : SimpleStatementNode
    {
        public WriteStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            ExpressionListNode el = (ExpressionListNode) Childs[0];
            if (el.Childs.Count == 1)
            {
                el.Childs[0].Generate(asmCode, symTable);
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(new AsmPrintf((TypeSymbol)(el.Childs[0] as ExpressionNode).Type, new AsmReg(AsmReg.Reg.Eax)));
                return;
            }
            throw new NotImplementedException();
        }
    }

    public class IfStatementNode : StructStatementNode
    {
        public IfStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ForStatementNode : StructStatementNode
    {
        public ForStatementNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DesignatorNode : ExpressionNode
    {
        public DesignatorNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class IdentNode : DesignatorNode
    {
        public IdentNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            var v = (VarSymbol)symTable[Value];
            asmCode.Add(new AsmCmd1(AsmCmd.Cmd.Push, new AsmMem(v.Offset, v.Type.Size)));
        }
    }

    public class ExpressionListNode : Node
    {
        public ExpressionListNode(List<ExpressionNode> childs, object value, uint line, uint position) : base(
            new List<Node>(childs), value, line, position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StringNode : Node
    {
        public StringNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BinOpNode : ExpressionNode
    {
        public BinOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            Childs[0].Generate(asmCode, symTable);
            Childs[1].Generate(asmCode, symTable);
            var op = AsmCmd.TokenBinIntOps[(Tokenizer.TokenSubType) Value];
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
            asmCode.Add(op, AsmReg.Reg.Eax, AsmReg.Reg.Ebx);
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
        }
    }

    public class UnOpNode : ExpressionNode
    {
        public UnOpNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            Childs[0].Generate(asmCode, symTable);
            var op = AsmCmd.TokenUnIntOps[(Tokenizer.TokenSubType)Value];
            if (op == AsmCmd.Cmd.None)
            {
                return;
            }
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(op, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
        }
    }

    public class ConstNode : ExpressionNode
    {
        public ConstNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            asmCode.Add(AsmCmd.Cmd.Push, Value.ToString());
        }
    }

    public class BreakNode : Statement
    {
        public BreakNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContinueNode : Statement
    {
        public ContinueNode(List<Node> childs, Tokenizer.Token token) : base(childs, token.Value, token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ExitNode : Statement
    {
        public ExitNode(List<Node> childs, object value, uint line, uint position) : base(childs, value, line, position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new System.NotImplementedException();
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