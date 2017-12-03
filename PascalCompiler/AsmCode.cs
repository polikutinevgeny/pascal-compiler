using System;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler
{
    public class AsmCode
    {
        public List<AsmCmd> Commands { get; } = new List<AsmCmd>();
        public Dictionary<object, string> ConstDictionary { get; } = new Dictionary<object, string>();
        public Stack<LoopStatement> LoopStack { get; } = new Stack<LoopStatement>();
        public Stack<SubprogramSymbol> SubprogramStack { get; } = new Stack<SubprogramSymbol>();

        public long CurrentID = 0;

        public AsmCode(PascalProgram program)
        {
            GenerateSubprogramHeader(program.Block);
            var mainLabel = new AsmLabel("main");
            Add(mainLabel);
            program.Block.Generate(this);
            Add(new AsmSpecial("exit"));
            GenerateSubprograms(program.Block);
            Add(new AsmSpecial($"END {mainLabel.ToArgString()}"));
        }

        private void GenerateSubprogramHeader(Block block, string prefix = "")
        {
            foreach (var symbol in block.SymTable.Values)
            {
                if (symbol is SubprogramSymbol subprogram)
                {
                    subprogram.Label = new AsmLabel($"{prefix}@{subprogram.Name}");
                    subprogram.ParameterOffset = -12; // [ebp+8] - parent frame pointer
                    foreach (var parameter in subprogram.Parameters.AsEnumerable())
                    {
                        parameter.Offset = subprogram.ParameterOffset;
                        subprogram.ParameterOffset -= parameter.Type.Size;
                    }
                    subprogram.ParameterOffset +=
                        subprogram.Parameters.Count == 0 ? 4 : subprogram.Parameters.Last().Type.Size;
                    GenerateSubprogramHeader(subprogram.Block, $"{prefix}@{subprogram.Name}");
                }
            }
        }

        private void GenerateSubprograms(Block block)
        {
            foreach (var symbol in block.SymTable.Values)
            {
                if (symbol is SubprogramSymbol subprogram)
                {
                    Add(subprogram.Label);
                    SubprogramStack.Push(subprogram);
                    subprogram.Block.Generate(this);
                    Add(AsmCmd.Cmd.Ret, $"{-subprogram.ParameterOffset - 4}");
                    GenerateSubprograms(subprogram.Block);
                    SubprogramStack.Pop();
                }
            }
        }

        public void Add(AsmCmd.Cmd cmd)
        {
            Add(new AsmCmd0(cmd));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg)
        {
            Add(new AsmCmd1(cmd, new AsmReg(reg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg1, AsmReg.Reg reg2)
        {
            Add(new AsmCmd2(cmd, new AsmReg(reg1), new AsmReg(reg2)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg, int arg)
        {
            Add(new AsmCmd2(cmd, new AsmReg(reg), new AsmImm(arg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg, object arg)
        {
            Add(new AsmCmd2(cmd, new AsmReg(reg), new AsmImm(arg)));
        }

        public void Add(AsmCmd.Cmd cmd, string arg)
        {
            Add(new AsmCmd1(cmd, new AsmImm(arg)));
        }

        public void Add(AsmCmd cmd)
        {
            Commands.Add(cmd);
        }

        private readonly string _preamble =
            ".686p\n" +
            "include \\masm32\\include\\masm32rt.inc\n" +
            ".mmx\n" +
            ".xmm\n";

        private readonly string _codePreamble = ".code\n";

        private readonly string _constPreamble = ".const\n";

        public override string ToString()
        {
            string consts = _constPreamble;
            foreach (var pair in ConstDictionary)
            {
                consts += pair.Value;
                switch (pair.Key)
                {
                    case double d:
                        consts += $" dq {HexConverter.DoubleToHexString(d)}h\n";
                        break;
                    case string s:
                        consts += $" db {string.Join(", ", s.Select(Convert.ToInt32))}, 0\n";
                        break;
                }
            }
            return _preamble + consts + _codePreamble + string.Join("\n", Commands.Select(x => x.ToString()));
        }

        public void Add(AsmCmd.Cmd cmd, string arg, AsmReg.Reg reg)
        {
            Add(new AsmCmd2(cmd, new AsmImm(arg), new AsmReg(reg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg, AsmOffset arg)
        {
            Add(new AsmCmd2(cmd, new AsmReg(reg), arg));
        }

        public void Add(AsmCmd.Cmd cmd, AsmOffset arg, AsmReg.Reg reg)
        {
            Add(new AsmCmd2(cmd, arg, new AsmReg(reg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmOffset arg)
        {
            Add(new AsmCmd1(cmd, arg));
        }

        public void Add(string label)
        {
            Add(new AsmLabel(label));
        }

        public void Add(long labelId)
        {
            Add(new AsmLabel(labelId));
        }

        public void Add(AsmCmd.Cmd cmd, AsmLabel label)
        {
            Add(new AsmJump(cmd, label));
        }
    }

    public abstract class AsmCmd
    {
        public enum Cmd
        {
            None,
            Pop,
            Push,
            Invoke,
            Call,
            Add,
            Sub,
            Imul,
            Mul,
            Idiv,
            Div,
            Movsd,
            Addsd,
            Subsd,
            Mulsd,
            Divsd,
            Mov,
            Ret,
            Neg,
            Pxor,

            // ReSharper disable once InconsistentNaming
            Cvtsi2sd,

            // ReSharper disable once InconsistentNaming
            Cvttsd2si,
            Not,
            Movsx,
            Cdq,
            Shl,
            Shr,
            Cmp,
            And,
            Or,
            Xor,
            Sete,
            Setge,
            Setg,
            Setle,
            Setl,
            Setne,
            Cmpsd,
            Cmpeqsd,
            Cmpltsd,
            Cmplesd,
            Cmpneqsd,
            Cmpnltsd,
            Cmpnlesd,
            Movd,
            Test,
            Jne,
            Jmp,
            Lea,
            Inc,
            Jnge,
            Jnz,
            Je,
            Jle,
            Exit,
            Leave
        }

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenBinIntOps =
            new Dictionary<Tokenizer.TokenSubType, Cmd>
            {
                {Tokenizer.TokenSubType.Plus, Cmd.Add},
                {Tokenizer.TokenSubType.Minus, Cmd.Sub},
                {Tokenizer.TokenSubType.Asterisk, Cmd.Imul},
                {Tokenizer.TokenSubType.Div, Cmd.Idiv},
                {Tokenizer.TokenSubType.Mod, Cmd.Idiv},
                {Tokenizer.TokenSubType.Shl, Cmd.Shl},
                {Tokenizer.TokenSubType.Shr, Cmd.Shr},
                {Tokenizer.TokenSubType.Less, Cmd.Cmp},
                {Tokenizer.TokenSubType.Greater, Cmd.Cmp},
                {Tokenizer.TokenSubType.LEqual, Cmd.Cmp},
                {Tokenizer.TokenSubType.GEqual, Cmd.Cmp},
                {Tokenizer.TokenSubType.Equal, Cmd.Cmp},
                {Tokenizer.TokenSubType.NEqual, Cmd.Cmp},
                {Tokenizer.TokenSubType.Xor, Cmd.Xor},
                {Tokenizer.TokenSubType.And, Cmd.And},
                {Tokenizer.TokenSubType.Or, Cmd.Or}
            };

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenCmpIntOps =
            new Dictionary<Tokenizer.TokenSubType, Cmd>
            {
                {Tokenizer.TokenSubType.Less, Cmd.Setge},
                {Tokenizer.TokenSubType.Greater, Cmd.Setle},
                {Tokenizer.TokenSubType.LEqual, Cmd.Setg},
                {Tokenizer.TokenSubType.GEqual, Cmd.Setl},
                {Tokenizer.TokenSubType.Equal, Cmd.Setne},
                {Tokenizer.TokenSubType.NEqual, Cmd.Sete},
            };

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenUnIntOps =
            new Dictionary<Tokenizer.TokenSubType, Cmd>
            {
                {Tokenizer.TokenSubType.Plus, Cmd.None},
                {Tokenizer.TokenSubType.Not, Cmd.Not},
                {Tokenizer.TokenSubType.Minus, Cmd.Neg},
            };

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenBinRealOps =
            new Dictionary<Tokenizer.TokenSubType, Cmd>
            {
                {Tokenizer.TokenSubType.Plus, Cmd.Addsd},
                {Tokenizer.TokenSubType.Minus, Cmd.Subsd},
                {Tokenizer.TokenSubType.Asterisk, Cmd.Mulsd},
                {Tokenizer.TokenSubType.Slash, Cmd.Divsd},
                {Tokenizer.TokenSubType.Less, Cmd.Cmpltsd},
                {Tokenizer.TokenSubType.Greater, Cmd.Cmpnlesd},
                {Tokenizer.TokenSubType.LEqual, Cmd.Cmplesd},
                {Tokenizer.TokenSubType.GEqual, Cmd.Cmpnltsd},
                {Tokenizer.TokenSubType.Equal, Cmd.Cmpeqsd},
                {Tokenizer.TokenSubType.NEqual, Cmd.Cmpneqsd}
            };

        public Cmd Command { get; set; }
    }

    public class AsmSpecial : AsmCmd
    {
        public string Value { get; set; }

        public AsmSpecial(string value) => Value = value;

        public override string ToString() => Value;
    }

    public class AsmCmd0 : AsmCmd
    {
        public AsmCmd0(Cmd cmd) => Command = cmd;

        public override string ToString()
        {
            return $"{Command}";
        }
    }

    public class AsmCmd1 : AsmCmd
    {
        public AsmArg Arg { get; set; }

        public AsmCmd1(Cmd cmd, AsmArg arg)
        {
            Command = cmd;
            Arg = arg;
        }

        public override string ToString()
        {
            return $"{Command} {Arg}";
        }
    }

    public class AsmCmd2 : AsmCmd
    {
        public AsmArg Arg1 { get; set; }
        public AsmArg Arg2 { get; set; }

        public AsmCmd2(Cmd cmd, AsmArg arg1, AsmArg arg2)
        {
            Command = cmd;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public override string ToString()
        {
            return $"{Command} {Arg1}, {Arg2}";
        }
    }

    public abstract class AsmArg
    {
    }

    public class AsmReg : AsmArg
    {
        public enum Reg
        {
            Eax,
            Ebx,
            Ecx,
            Edx,
            Esi,
            Edi,
            Ebp,
            Esp,
            Xmm0,
            Xmm1,
            Xmm2,
            Xmm3,
            Xmm4,
            Xmm5,
            Xmm6,
            Xmm7,
            Al,
            Bl,
            Cl,
        }

        public Reg Value { get; set; }

        public AsmReg(Reg reg) => Value = reg;

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AsmImm : AsmArg
    {
        public object Value { get; set; }

        public AsmImm(object value) => Value = value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public abstract class AsmMem : AsmArg
    {
        protected AsmMem(int offset, int size, AsmReg.Reg start)
        {
            this.Offset = offset;
            this.Size = size;
            this.Start = new AsmReg(start);
        }

        public int Offset { get; set; }
        public int Size { get; set; }
        public AsmReg Start { get; set; }

        public static Dictionary<int, string> SizeType { get; } = new Dictionary<int, string>
        {
            {0, ""},
            {1, "byte"},
            {2, "word"},
            {4, "dword"},
            {8, "qword"},
        };
    }

    public class AsmOffset : AsmMem // Offset over register ([reg-offset])
    {
        public AsmOffset(int offset, int size) : base(offset, size, AsmReg.Reg.Ebp)
        {
            Offset = offset;
            Size = size;
        }

        public AsmOffset(int offset, int size, AsmReg.Reg start) : base(offset, size, start)
        {
        }

        public override string ToString()
        {
            return
                $"{(SizeType[Size] != "" ? $"{SizeType[Size]} ptr" : "")} [{Start}{(Offset > 0 ? $"-{Offset}" : $"+{-Offset}")}]";
        }
    }

    public class AsmArrayAddr : AsmMem // [base+size*index-offset]
    {
        public AsmArrayAddr(int offset, int size, AsmReg.Reg start, AsmReg.Reg index) : base(offset, size, start) =>
            this.Index = new AsmReg(index);

        public AsmReg Index { get; set; }

        public override string ToString()
        {
            return
                $"{(SizeType[Size] != "" ? $"{SizeType[Size]} ptr" : "")} [{Start}+{Size}*{Index}{(Offset > 0 ? $"-{Offset}" : $"+{-Offset}")}]";
        }
    }

    public class AsmLabel : AsmCmd
    {
        public AsmLabel(string name) => this.Name = name;

        public AsmLabel(long id) => this.Name = $"Label{id}";

        public string Name { get; set; }

        public override string ToString()
        {
            return $"__@label_{Name}:";
        }

        public string ToArgString()
        {
            return $"__@label_{Name}";
        }
    }

    public class AsmJump : AsmCmd
    {
        public AsmLabel Arg { get; set; }

        public AsmJump(Cmd cmd, AsmLabel arg)
        {
            Command = cmd;
            Arg = arg;
        }

        public override string ToString()
        {
            return $"{Command} {Arg.ToArgString()}";
        }
    }
}