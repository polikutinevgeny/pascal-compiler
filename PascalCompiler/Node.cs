using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;

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

        protected Node(List<Node> childs, Tokenizer.Token token) : this(
            childs,
            token.Value,
            token.Line,
            token.Position)
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

        public ExpressionNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }
    }

    public class CompoundStatementNode : StructStatementNode
    {
        public List<Statement> Statements { get; set; }

        public CompoundStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            if (Statements == null) return;
            foreach (var child in Statements)
            {
                child.Generate(asmCode, symTable);
            }
        }
    }

    public abstract class Statement : Node
    {
        public Statement(List<Node> childs, object value, uint line, uint position) :
            base(
                childs,
                value,
                line,
                position)
        {
        }
    }

    public class SimpleStatementNode : Statement
    {
        public SimpleStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            var left = (DesignatorNode) Childs[0];
            var right = (ExpressionNode) Childs[1];
            right.Generate(asmCode, symTable);
            left.GenerateLValue(asmCode, symTable);
            if (right.Type == TypeSymbol.IntTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    new AsmOffset(0, 4, AsmReg.Reg.Eax),
                    AsmReg.Reg.Ebx);
            }
            else if (right.Type == TypeSymbol.CharTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    new AsmOffset(0, 1, AsmReg.Reg.Eax),
                    AsmReg.Reg.Bl);
            }
            else if (right.Type == TypeSymbol.RealTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm0,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Eax),
                    AsmReg.Reg.Xmm0);
            }
            else if (right.Type is ArrayTypeSymbol || right.Type is RecordTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                for (int i = 0; i < ((TypeSymbol) right.Type).Size; i += 4)
                {
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                    asmCode.Add(
                        AsmCmd.Cmd.Mov,
                        new AsmOffset(-i, 4, AsmReg.Reg.Eax),
                        AsmReg.Reg.Ebx);
                }
            }
        }
    }

    public abstract class StructStatementNode : Statement
    {
        protected StructStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }
    }

    public class ReadStatementNode : SimpleStatementNode
    {
        public ReadStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
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
        public WriteStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        private static readonly Dictionary<TypeSymbol, string> _fmtDictionary =
            new Dictionary<TypeSymbol, string>
            {
                {TypeSymbol.CharTypeSymbol, "%c"},
                {TypeSymbol.IntTypeSymbol, "%d"},
                {TypeSymbol.RealTypeSymbol, "%f"},
            };

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            int stackSize = 4;
            if (Childs[0] is ExpressionListNode el)
            {
                foreach (var node in el.Childs.AsEnumerable().Reverse())
                {
                    var child = (ExpressionNode) node;
                    child.Generate(asmCode, symTable);
                    stackSize += ((TypeSymbol) child.Type).Size;
                }
                string fmt = string.Join(
                    " ",
                    el.Childs.Cast<ExpressionNode>().
                        Select(i => _fmtDictionary[(TypeSymbol) i.Type]));
                string val = $"__@string{asmCode.CurrentID++}";
                if (!asmCode.ConstDictionary.ContainsKey(fmt))
                {
                    asmCode.ConstDictionary[fmt] = val;
                }
                asmCode.Add(AsmCmd.Cmd.Push, $"offset {asmCode.ConstDictionary[fmt]}");
            }
            else if (Childs[0] is StringNode sn)
            {
                sn.GenerateLValue(asmCode, symTable);
            }
//            asmCode.Add(AsmCmd.Cmd.And, AsmReg.Reg.Esp, -4);
            asmCode.Add(AsmCmd.Cmd.Call, "crt_printf");
            asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, stackSize);
        }
    }

    public class IfStatementNode : StructStatementNode
    {
        public IfStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            long id = asmCode.CurrentID++;
            var elseLabel = new AsmLabel($"Condtion{id}Else");
            var endLabel = new AsmLabel($"Condtion{id}End");
            Childs[0].Generate(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, 0);
            asmCode.Add(AsmCmd.Cmd.Je, elseLabel);
            Childs[1].Generate(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Jmp, endLabel);
            asmCode.Add(elseLabel);
            if (Childs.Count > 2)
            {
                Childs[2].Generate(asmCode, symTable);
            }
            asmCode.Add(endLabel);
        }
    }

    public abstract class LoopStatement : StructStatementNode
    {
        protected LoopStatement(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public AsmLabel StartLabel { get; protected set; }
        public AsmLabel EndLabel { get; protected set; }
    }

    public class ForStatementNode : LoopStatement
    {
        public ForStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            long id = asmCode.CurrentID++;
            StartLabel = new AsmLabel($"Cycle{id}Start");
            EndLabel = new AsmLabel($"Cycle{id}End");
            AsmLabel bodyLabel = new AsmLabel($"Cycle{id}Body");
            var v = (VarSymbol) symTable.LookUp(Childs[0].Value.ToString());
            Childs[1].Generate(asmCode, symTable); // Initial value
            ((IdentNode) Childs[0]).GenerateLValue(asmCode, symTable); // Cycle counter
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);

            asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Ebx, 1);

            asmCode.Add(
                AsmCmd.Cmd.Mov,
                new AsmOffset(0, v.Type.Size, AsmReg.Reg.Eax),
                AsmReg.Reg.Ebx);
            // Value initialized
            asmCode.Add(AsmCmd.Cmd.Jmp, StartLabel);
            asmCode.Add(bodyLabel);
            asmCode.LoopStack.Push(this);
            Childs[3].Generate(asmCode, symTable); // Cycle body
            asmCode.LoopStack.Pop();

            asmCode.Add(StartLabel);

            ((IdentNode) Childs[0]).GenerateLValue(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Inc, new AsmOffset(0, v.Type.Size, AsmReg.Reg.Eax));


            Childs[2].Generate(asmCode, symTable); // Cycle counter target

            ((IdentNode) Childs[0]).GenerateLValue(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);

            asmCode.Add(
                AsmCmd.Cmd.Mov,
                AsmReg.Reg.Eax,
                new AsmOffset(0, v.Type.Size, AsmReg.Reg.Eax));
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
            asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, AsmReg.Reg.Ebx);
            asmCode.Add(AsmCmd.Cmd.Jle, bodyLabel);
            asmCode.Add(EndLabel);
        }
    }

    public class WhileStatementNode : LoopStatement
    {
        public ExpressionNode Condition { get; set; }

        public WhileStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) :
            base(childs, value, line, position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            long id = asmCode.CurrentID++;
            StartLabel = new AsmLabel($"Cycle{id}Start");
            EndLabel = new AsmLabel($"Cycle{id}End");
            AsmLabel bodyLabel = new AsmLabel($"Cycle{id}Body");
            asmCode.Add(AsmCmd.Cmd.Jmp, StartLabel);
            asmCode.Add(bodyLabel);
            asmCode.LoopStack.Push(this);
            Childs[0].Generate(asmCode, symTable);
            asmCode.LoopStack.Pop();
            asmCode.Add(StartLabel);
            Condition.Generate(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, 0);
            asmCode.Add(AsmCmd.Cmd.Jne, bodyLabel);
            asmCode.Add(EndLabel);
        }
    }

    public class RepeatStatementNode : LoopStatement
    {
        public ExpressionNode Condition { get; set; }

        public RepeatStatementNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            long id = asmCode.CurrentID++;
            StartLabel = new AsmLabel($"Cycle{id}Start");
            EndLabel = new AsmLabel($"Cycle{id}End");
            asmCode.Add(StartLabel);
            asmCode.LoopStack.Push(this);
            foreach (var child in Childs)
            {
                child.Generate(asmCode, symTable);
            }
            asmCode.LoopStack.Pop();
            Condition.Generate(asmCode, symTable);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, 0);
            asmCode.Add(AsmCmd.Cmd.Je, StartLabel);
            asmCode.Add(EndLabel);
        }
    }

    public abstract class DesignatorNode : ExpressionNode
    {
        protected DesignatorNode(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            var v = (TypeSymbol) Type;
            if (v == TypeSymbol.IntTypeSymbol)
            {
                asmCode.Add(AsmCmd.Cmd.Push, new AsmOffset(0, 4, AsmReg.Reg.Eax));
            }
            else if (v == TypeSymbol.CharTypeSymbol)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Movsx,
                    AsmReg.Reg.Eax,
                    new AsmOffset(0, 1, AsmReg.Reg.Eax));
                asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
            }
            else if (v == TypeSymbol.RealTypeSymbol)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm0,
                    new AsmOffset(0, 8, AsmReg.Reg.Eax));
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 8);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp),
                    AsmReg.Reg.Xmm0);
            }
            else if (v is ArrayTypeSymbol || v is RecordTypeSymbol)
            {
                for (int i = v.Size - 4; i >= 0; i -= 4)
                {
                    asmCode.Add(AsmCmd.Cmd.Push, new AsmOffset(-i, 4, AsmReg.Reg.Eax));
                }
            }
        }

        public abstract void GenerateLValue(AsmCode asmCode, SymTable symTable);
    }

    public class IdentNode : DesignatorNode
    {
        public IdentNode(List<Node> childs, Tokenizer.Token token) : base(
            childs,
            token.Value,
            token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            GenerateLValue(asmCode, symTable);
            base.Generate(asmCode, symTable);
        }

        public override void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            var t = symTable.LookUpLevel(Value.ToString());
            VarSymbol v = (VarSymbol) t?.Item1;
            int level = t.Value.Item2;
            asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Eax, AsmReg.Reg.Ebp);
            while (level-- > 0)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    AsmReg.Reg.Eax,
                    new AsmOffset(-8, 4, AsmReg.Reg.Eax));
            }
            asmCode.Add(
                AsmCmd.Cmd.Lea,
                AsmReg.Reg.Eax,
                new AsmOffset(v.Offset, 0, AsmReg.Reg.Eax));
            if (v is ParameterSymbol ps &&
                ps.ParameterModifier != ParameterModifier.Value)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    AsmReg.Reg.Eax,
                    new AsmOffset(0, 4, AsmReg.Reg.Eax));
            }
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
        }
    }

    public class ExpressionListNode : Node
    {
        public ExpressionListNode(
            List<ExpressionNode> childs,
            object value,
            uint line,
            uint position) : base(
            new List<Node>(childs),
            value,
            line,
            position)
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

        public void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            string val = $"__@string{asmCode.CurrentID++}";
            if (!asmCode.ConstDictionary.ContainsKey(Value))
            {
                asmCode.ConstDictionary[Value] = val;
            }
            asmCode.Add(AsmCmd.Cmd.Push, $"offset {asmCode.ConstDictionary[Value]}");
        }
    }

    public class BinOpNode : ExpressionNode
    {
        public BinOpNode(List<Node> childs, object value, uint line, uint position) :
            base(
                childs,
                value,
                line,
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
                if (t == TypeSymbol.IntTypeSymbol || t == TypeSymbol.CharTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
                    asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                    asmCode.Add(AsmCmd.Cmd.Cmp, AsmReg.Reg.Eax, AsmReg.Reg.Ebx);
                    asmCode.Add(AsmCmd.TokenCmpIntOps[val], AsmReg.Reg.Al);
                    asmCode.Add(AsmCmd.Cmd.Movsx, AsmReg.Reg.Eax, AsmReg.Reg.Al);
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Eax, 1);
                    asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
                    return;
                }
                if (t == TypeSymbol.RealTypeSymbol)
                {
                    asmCode.Add(
                        AsmCmd.Cmd.Movsd,
                        AsmReg.Reg.Xmm1,
                        new AsmOffset(0, 8, AsmReg.Reg.Esp));
                    asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                    asmCode.Add(
                        AsmCmd.Cmd.Movsd,
                        AsmReg.Reg.Xmm0,
                        new AsmOffset(0, 8, AsmReg.Reg.Esp));
                    asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                    var cmpop = AsmCmd.TokenBinRealOps[val];
                    asmCode.Add(cmpop, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 4);
                    asmCode.Add(
                        AsmCmd.Cmd.Movd,
                        new AsmOffset(0, 4, AsmReg.Reg.Esp),
                        AsmReg.Reg.Xmm0);
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
                        asmCode.Add(
                            AsmCmd.Cmd.Push,
                            val == Tokenizer.TokenSubType.Mod
                                ? AsmReg.Reg.Edx
                                : AsmReg.Reg.Eax);
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
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm1,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm0,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(op, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp),
                    AsmReg.Reg.Xmm0);
                return;
            }
        }
    }

    public class UnOpNode : ExpressionNode
    {
        public UnOpNode(List<Node> childs, object value, uint line, uint position) : base(
            childs,
            value,
            line,
            position)
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
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm0,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 8);
                string neg = $"__@real{HexConverter.DoubleToHexString(-0.0)}";
                asmCode.ConstDictionary[-0.0] = neg;
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 8);
                asmCode.Add(AsmCmd.Cmd.Movsd, AsmReg.Reg.Xmm1, neg);
                asmCode.Add(AsmCmd.Cmd.Pxor, AsmReg.Reg.Xmm0, AsmReg.Reg.Xmm1);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp),
                    AsmReg.Reg.Xmm0);
            }
        }
    }

    public class ConstNode : ExpressionNode
    {
        public ConstNode(List<Node> childs, object value, uint line, uint position) :
            base(
                childs,
                value,
                line,
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
                string val = $"__@real{HexConverter.DoubleToHexString((double) Value)}";
                if (!asmCode.ConstDictionary.ContainsKey(Value))
                {
                    asmCode.ConstDictionary[Value] = val;
                }
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 8);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    AsmReg.Reg.Xmm0,
                    asmCode.ConstDictionary[Value]);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp),
                    AsmReg.Reg.Xmm0);
            }
        }
    }

    public class BreakNode : Statement
    {
        public BreakNode(List<Node> childs, Tokenizer.Token token) : base(
            childs,
            token.Value,
            token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            asmCode.Add(AsmCmd.Cmd.Jmp, asmCode.LoopStack.Peek().EndLabel);
        }
    }

    public class ContinueNode : Statement
    {
        public ContinueNode(List<Node> childs, Tokenizer.Token token) : base(
            childs,
            token.Value,
            token.Line,
            token.Position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            asmCode.Add(AsmCmd.Cmd.Jmp, asmCode.LoopStack.Peek().StartLabel);
        }
    }

    public class ExitNode : Statement
    {
        public ExitNode(List<Node> childs, object value, uint line, uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            if (asmCode.SubprogramStack.Count == 0)
            {
                asmCode.Add(AsmCmd.Cmd.Exit);
                return;
            }
            var sp = asmCode.SubprogramStack.Peek();
            switch (sp)
            {
                case FunctionSymbol fs:
                    Childs[0].Generate(asmCode, symTable);
                    if (fs.ReturnType == TypeSymbol.IntTypeSymbol ||
                        fs.ReturnType == TypeSymbol.CharTypeSymbol)
                    {
                        asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
                    }
                    else if (fs.ReturnType == TypeSymbol.RealTypeSymbol)
                    {
                        asmCode.Add(
                            AsmCmd.Cmd.Movsd,
                            AsmReg.Reg.Xmm0,
                            new AsmOffset(0, 8, AsmReg.Reg.Esp));
                        asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 8);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
            }
            asmCode.Add(AsmCmd.Cmd.Leave);
            asmCode.Add(AsmCmd.Cmd.Ret, $"{-sp.ParameterOffset - 4}");
        }
    }

    public class CallOperator : DesignatorNode
    {
        public SubprogramSymbol Subprogram { get; set; }

        public CallOperator(List<Node> childs, object value, uint line, uint position) :
            base(
                childs,
                value,
                line,
                position)
        {
        }

        public override string ToString()
        {
            return $"{Subprogram.Name} call";
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            if (Subprogram is FunctionSymbol fs &&
                (fs.ReturnType is RecordTypeSymbol || fs.ReturnType is ArrayTypeSymbol))
            {
                throw new NotImplementedException();
            }
            foreach (var prm in Childs.AsEnumerable().
                Reverse().
                Zip(
                    Subprogram.Parameters.AsEnumerable().Reverse(),
                    (node, symbol) => (node, symbol)))
            {
                if (prm.symbol.ParameterModifier == ParameterModifier.Value)
                {
                    prm.node.Generate(asmCode, symTable);
                }
                else
                {
                    ((DesignatorNode) prm.node).GenerateLValue(asmCode, symTable);
                }
            }
            if (asmCode.SubprogramStack.Count > 0 &&
                asmCode.SubprogramStack.Peek() == Subprogram)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    AsmReg.Reg.Eax,
                    new AsmOffset(-8, 4, AsmReg.Reg.Ebp));
                asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
            }
            else
            {
                asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Ebp);
            }
            asmCode.Add(AsmCmd.Cmd.Call, Subprogram.Label.ToArgString());
            if (Subprogram is FunctionSymbol functionSymbol)
            {
                if (functionSymbol.ReturnType == TypeSymbol.IntTypeSymbol ||
                    functionSymbol.ReturnType == TypeSymbol.CharTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
                }
                if (functionSymbol.ReturnType == TypeSymbol.RealTypeSymbol)
                {
                    asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 8);
                    asmCode.Add(
                        AsmCmd.Cmd.Movsd,
                        new AsmOffset(0, 8, AsmReg.Reg.Esp),
                        AsmReg.Reg.Xmm0);
                }
            }
        }

        public override void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            throw new NotImplementedException();
        }
    }

    public class CallStatementWrapper : SimpleStatementNode
    {
        public CallStatementWrapper(CallOperator callOperator) : base(
            new List<Node> {callOperator},
            callOperator.Value,
            callOperator.Line,
            callOperator.Position)
        {
        }

        public override string ToString()
        {
            return "Call statement wrapper";
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            Childs[0].Generate(asmCode, symTable);
        }
    }

    public class CastOperator : DesignatorNode
    {
        public CastOperator(List<Node> childs, object value, uint line, uint position) :
            base(
                childs,
                value,
                line,
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
                asmCode.Add(
                    AsmCmd.Cmd.Cvtsi2sd,
                    AsmReg.Reg.Xmm0,
                    new AsmOffset(0, 4, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, 4);
                asmCode.Add(
                    AsmCmd.Cmd.Movsd,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp),
                    AsmReg.Reg.Xmm0);
            }
            if (ct == TypeSymbol.RealTypeSymbol && pt == TypeSymbol.IntTypeSymbol)
            {
                asmCode.Add(
                    AsmCmd.Cmd.Cvttsd2si,
                    AsmReg.Reg.Eax,
                    new AsmOffset(0, 8, AsmReg.Reg.Esp));
                asmCode.Add(AsmCmd.Cmd.Add, AsmReg.Reg.Esp, 4);
                asmCode.Add(
                    AsmCmd.Cmd.Mov,
                    new AsmOffset(0, 4, AsmReg.Reg.Esp),
                    AsmReg.Reg.Eax);
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

        public override void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            throw new NotImplementedException();
        }
    }

    public class IndexOperator : DesignatorNode
    {
        public IndexOperator(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override string ToString()
        {
            return "Index";
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            GenerateLValue(asmCode, symTable);
            base.Generate(asmCode, symTable);
        }

        public override void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            ((DesignatorNode) Childs[0]).GenerateLValue(asmCode, symTable);
            Childs[1].Generate(asmCode, symTable);
            var t = (ArrayTypeSymbol) ((DesignatorNode) Childs[0]).Type;
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(AsmCmd.Cmd.Imul, AsmReg.Reg.Eax, t.ElementType.Size);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
            asmCode.Add(
                AsmCmd.Cmd.Lea,
                AsmReg.Reg.Eax,
                new AsmArrayAddr(
                    t.Range.Begin * t.ElementType.Size,
                    1,
                    AsmReg.Reg.Ebx,
                    AsmReg.Reg.Eax));
//            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
//            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebx);
//            asmCode.Add(AsmCmd.Cmd.Lea, AsmReg.Reg.Eax,
//                new AsmArrayAddr(t.Range.Begin * t.ElementType.Size, t.ElementType.AddressSize, AsmReg.Reg.Eax, AsmReg.Reg.Ebx));
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
        }
    }

    public class MemberAccessOperator : DesignatorNode
    {
        public MemberAccessOperator(
            List<Node> childs,
            object value,
            uint line,
            uint position) : base(
            childs,
            value,
            line,
            position)
        {
        }

        public override string ToString()
        {
            return "Member access";
        }

        public override void Generate(AsmCode asmCode, SymTable symTable)
        {
            GenerateLValue(asmCode, symTable);
            base.Generate(asmCode, symTable);
        }

        public override void GenerateLValue(AsmCode asmCode, SymTable symTable)
        {
            ((DesignatorNode) Childs[0]).GenerateLValue(asmCode, symTable);
            var t = (RecordTypeSymbol) ((DesignatorNode) Childs[0]).Type;
            var f = ((VarSymbol) t.Fields[Childs[1].Value]);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Eax);
            asmCode.Add(
                AsmCmd.Cmd.Lea,
                AsmReg.Reg.Eax,
                new AsmOffset(f.Offset, 0, AsmReg.Reg.Eax));
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Eax);
        }
    }

    public static class HexConverter
    {
        public static string DoubleToHexString(double inp)
        {
            byte[] bytes = BitConverter.GetBytes(inp);
            string res = BitConverter.ToString(bytes.Reverse().ToArray()).
                Replace("-", "");
            return $"0{res}";
        }
    }
}