using System;
using System.Collections.Generic;
using System.Linq;

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
                if (right.Type == TypeSymbol.IntTypeSymbol || right.Type == TypeSymbol.CharTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                    asmCode.Add(AsmCmd.Cmd.Mov, new AsmMem(v.Offset, v.Type.Size), AsmReg.Reg.Eax);
                }
                else if (right.Type == TypeSymbol.RealTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm0, new AsmMem(0, v.Type.Size, AsmReg.Reg.Esp));
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, v.Type.Size);
                    asmCode.Add(AsmCmd.Cmd.Movsd, new AsmMem(v.Offset, v.Type.Size), AsmReg.Reg.Xmm0);
                }
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
                var type = (TypeSymbol) (el.Childs[0] as ExpressionNode).Type;
                asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Eax, AsmReg.Reg.Esp);
                asmCode.Add(new AsmPrintf(type, new AsmMem(0, type.Size, AsmReg.Reg.Eax)));
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, type.Size);
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
            var v = (VarSymbol) symTable[Value];
            if (v.Type == TypeSymbol.IntTypeSymbol || v.Type == TypeSymbol.CharTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Push, new AsmMem(v.Offset, v.Type.Size));
            }
            else if (v.Type == TypeSymbol.RealTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Push, new AsmMem(v.Offset - v.Type.Size / 2, v.Type.Size / 2));
                asmCode.Add(AsmCmd.Cmd.Push, new AsmMem(v.Offset, v.Type.Size / 2));
            }
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
            var val = (Tokenizer.TokenSubType) Value;
            if (Parser.RelOps.Contains(val))
            {
                var t = (TypeSymbol) ((ExpressionNode) Childs[0]).Type;
                if (t == TypeSymbol.IntTypeSymbol || t == TypeSymbol.CharTypeSymbol) // Easy mode: char is 4 byte
                {
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                    asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, AsmReg.Reg.Ebx);
                    var cmpop = AsmCmd.TokenCmpIntOps[val];
                    asmCode.Add(cmpop, AsmReg.Reg.Al);
                    asmCode.Add(AsmCmd.Cmd.Movsx, AsmReg.Reg.Eax, AsmReg.Reg.Al);
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Eax, 1);
                    asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
                    return;
                }
                if (t == TypeSymbol.RealTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm1, new AsmMem(0, 8, AsmReg.Reg.Esp));
                    asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                    asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm0, new AsmMem(0, 8, AsmReg.Reg.Esp));
                    asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                    var cmpop = AsmCmd.TokenBinRealOps[val];
                    asmCode.Add(cmpop, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 4);
                    asmCode.Add(AsmCmd.Cmd.Movd, new AsmMem(0, 4, AsmReg.Reg.Esp), AsmReg.Reg.Xmm0);
                    return;
                }
                throw new InvalidOperationException("Non-scalar compared");
            }
            if (Type == TypeSymbol.IntTypeSymbol)
            {
                var op = AsmCmd.TokenBinIntOps[val];
                switch (op)
                {
                    case AsmCmd.Cmd.Add:
                    case AsmCmd.Cmd.Sub:
                    case AsmCmd.Cmd.Or:
                    case AsmCmd.Cmd.Xor:
                    case AsmCmd.Cmd.And:
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                        asmCode.Add(op, AsmReg.Reg.Eax, AsmReg.Reg.Ebx);
                        asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
                        return;
                    case AsmCmd.Cmd.Imul:
                    case AsmCmd.Cmd.Idiv:
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                        if (op == AsmCmd.Cmd.Idiv)
                        {
                            asmCode.Add(AsmCmd.Cmd.Cdq);
                        }
                        asmCode.Add(op, AsmReg.Reg.Ebx);
                        asmCode.Add(AsmCmd.Cmd.Push,
                            val == Tokenizer.TokenSubType.Mod ? AsmReg.Reg.Edx : AsmReg.Reg.Eax);
                        return;
                    case AsmCmd.Cmd.Shl:
                    case AsmCmd.Cmd.Shr:
                        asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Ecx, AsmReg.Reg.Ebx);
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ecx);
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                        asmCode.Add(op, AsmReg.Reg.Eax, AsmReg.Reg.Cl);
                        asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Ebx, AsmReg.Reg.Ecx);
                        asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
                        return;
                }
            }
            else if (Type == TypeSymbol.RealTypeSymbol)
            {
                var op = AsmCmd.TokenBinRealOps[val];
                asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm1, new AsmMem(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm0, new AsmMem(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(op, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                asmCode.Add(AsmCmd.Cmd.Movsd, new AsmMem(0, 8, AsmReg.Reg.Esp), AsmReg.Reg.Xmm0);
                return;
            }
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
            var opName = (Tokenizer.TokenSubType) Value;
            if ((Childs[0] as ExpressionNode).Type == TypeSymbol.IntTypeSymbol)
            {
                var op = AsmCmd.TokenUnIntOps[opName];
                if (op == AsmCmd.Cmd.None)
                {
                    return;
                }
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(op, AsmReg.Reg.Eax);
                asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
            }
            else if ((Childs[0] as ExpressionNode).Type == TypeSymbol.RealTypeSymbol)
            {
                if (opName == Tokenizer.TokenSubType.Plus)
                {
                    return;
                }
                asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm0, new AsmMem(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                var neg = HexConverter.DoubleToHexString(-0.0);
                asmCode.Add(AsmCmd.Cmd.Push, neg.Item1 + "h");
                asmCode.Add(AsmCmd.Cmd.Push, neg.Item2 + "h");
                asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm1, new AsmMem(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Pxor, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                asmCode.Add(AsmCmd.Cmd.Movsd, new AsmMem(0, 8, AsmReg.Reg.Esp), AsmReg.Reg.Xmm0);
            }
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
            if (Type == TypeSymbol.IntTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Push, Value.ToString());
            }
            else if (Type == TypeSymbol.CharTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Push, Convert.ToInt32(Value).ToString());
            }
            else if (Type == TypeSymbol.RealTypeSymbol)
            {
                var r = HexConverter.DoubleToHexString((double) Value);
                asmCode.Add(AsmCmd.Cmd.Push, r.Item1 + "h");
                asmCode.Add(AsmCmd.Cmd.Push, r.Item2 + "h");
            }
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
            var child = (ExpressionNode) Childs[0];
            var ct = (TypeSymbol) child.Type;
            var pt = (TypeSymbol) Type;
            child.Generate(asmCode, symTable);
            if (ct == TypeSymbol.IntTypeSymbol && pt == TypeSymbol.RealTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Cvtsi2sd, AsmReg.Reg.Xmm0, new AsmMem(0, 4, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 4);
                asmCode.Add(AsmCmd.Cmd.Movsd, new AsmMem(0, 8, AsmReg.Reg.Esp), AsmReg.Reg.Xmm0);
            }
            if (ct == TypeSymbol.RealTypeSymbol && pt == TypeSymbol.IntTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Cvttsd2si, AsmReg.Reg.Eax, new AsmMem(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 4);
                asmCode.Add(AsmCmd.Cmd.Mov, new AsmMem(0, 4, AsmReg.Reg.Esp), AsmReg.Reg.Eax);
            }
            if (ct == TypeSymbol.CharTypeSymbol && pt == TypeSymbol.IntTypeSymbol)
            {
                // Char is 4 bytes for now
            }
            if (ct == TypeSymbol.IntTypeSymbol && pt == TypeSymbol.CharTypeSymbol)
            {
                // Char is 4 bytes for now
            }
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new NotImplementedException();
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

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            throw new NotImplementedException();
        }
    }

    public static class HexConverter
    {
        public static (string, string) DoubleToHexString(double inp)
        {
            byte[] bytes = BitConverter.GetBytes(inp);
            string res = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
            return (new string(res.Take(8).ToArray()), new string (res.Skip(8).ToArray()));
        }
    }
}