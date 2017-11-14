﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler
{
    public class AsmCode
    {
        public List<AsmCmd> Commands { get; } = new List<AsmCmd>();


        public AsmCode(PascalProgram program)
        {
            //Generate subprograms here
            Add(new AsmSpecial("main PROC"));
            program.Block.Generate(this);
            Add(new AsmSpecial("exit"));
            Add(new AsmSpecial("main ENDP"));
            Add(new AsmSpecial("END main"));
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
            ".xmm\n" +
            ".code\n";

        public override string ToString()
        {
            return _preamble + string.Join("\n", Commands.Select(x => x.ToString()));
        }

        public void Add(AsmCmd.Cmd cmd, string arg, AsmReg.Reg reg)
        {
            Add(new AsmCmd2(cmd, new AsmImm(arg), new AsmReg(reg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmReg.Reg reg, AsmMem arg)
        {
            Add(new AsmCmd2(cmd, new AsmReg(reg), arg));
        }

        public void Add(AsmCmd.Cmd cmd, AsmMem arg, AsmReg.Reg reg)
        {
            Add(new AsmCmd2(cmd, arg, new AsmReg(reg)));
        }

        public void Add(AsmCmd.Cmd cmd, AsmMem arg)
        {
            Add(new AsmCmd1(cmd, arg));
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
            Cvtsi2sd,
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
            Movd
        }

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenBinIntOps= new Dictionary<Tokenizer.TokenSubType, Cmd>
        {
            {Tokenizer.TokenSubType.Plus, Cmd.Add},
            {Tokenizer.TokenSubType.Minus, Cmd.Sub },
            {Tokenizer.TokenSubType.Asterisk, Cmd.Imul },
            {Tokenizer.TokenSubType.Div, Cmd.Idiv },
            {Tokenizer.TokenSubType.Mod, Cmd.Idiv },
            {Tokenizer.TokenSubType.Shl, Cmd.Shl },
            {Tokenizer.TokenSubType.Shr, Cmd.Shr },
            {Tokenizer.TokenSubType.Less, Cmd.Cmp },
            {Tokenizer.TokenSubType.Greater, Cmd.Cmp },
            {Tokenizer.TokenSubType.LEqual, Cmd.Cmp },
            {Tokenizer.TokenSubType.GEqual, Cmd.Cmp },
            {Tokenizer.TokenSubType.Equal, Cmd.Cmp },
            {Tokenizer.TokenSubType.NEqual, Cmd.Cmp },
            {Tokenizer.TokenSubType.Xor, Cmd.Xor },
            {Tokenizer.TokenSubType.And, Cmd.And },
            {Tokenizer.TokenSubType.Or, Cmd.Or }
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

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenUnIntOps = new Dictionary<Tokenizer.TokenSubType, Cmd>
        {
            {Tokenizer.TokenSubType.Plus, Cmd.None},
            {Tokenizer.TokenSubType.Not,  Cmd.Not},
            {Tokenizer.TokenSubType.Minus, Cmd.Neg },
        };

        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenBinRealOps = new Dictionary<Tokenizer.TokenSubType, Cmd>
        {
            {Tokenizer.TokenSubType.Plus, Cmd.Addsd},
            {Tokenizer.TokenSubType.Minus, Cmd.Subsd },
            {Tokenizer.TokenSubType.Asterisk, Cmd.Mulsd },
            {Tokenizer.TokenSubType.Slash, Cmd.Divsd },
            {Tokenizer.TokenSubType.Less, Cmd.Cmpltsd },
            {Tokenizer.TokenSubType.Greater, Cmd.Cmpnlesd },
            {Tokenizer.TokenSubType.LEqual, Cmd.Cmplesd },
            {Tokenizer.TokenSubType.GEqual, Cmd.Cmpnltsd },
            {Tokenizer.TokenSubType.Equal, Cmd.Cmpeqsd },
            {Tokenizer.TokenSubType.NEqual, Cmd.Cmpneqsd }
        };

//        public static readonly Dictionary<Tokenizer.TokenSubType, Cmd> TokenCmpRealOps =
//            new Dictionary<Tokenizer.TokenSubType, Cmd>
//            {
//                {Tokenizer.TokenSubType.Less, Cmd.Setl},
//                {Tokenizer.TokenSubType.Greater, Cmd.Setg},
//                {Tokenizer.TokenSubType.LEqual, Cmd.Setle},
//                {Tokenizer.TokenSubType.GEqual, Cmd.Setge},
//                {Tokenizer.TokenSubType.Equal, Cmd.Sete},
//                {Tokenizer.TokenSubType.NEqual, Cmd.Setne},
//            };

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

    public class AsmCmd1: AsmCmd
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

    public class AsmPrintf : AsmCmd
    {
        public TypeSymbol Type { get; set; }
        public AsmArg Arg { get; set; }

        private static Dictionary<TypeSymbol, string> FormatStrings = new Dictionary<TypeSymbol, string>
        {
            {TypeSymbol.IntTypeSymbol, "%d" },
            {TypeSymbol.RealTypeSymbol, "%f" },
            {TypeSymbol.CharTypeSymbol, "%c" }
        };

        public AsmPrintf(TypeSymbol type, AsmArg arg)
        {
            Type = type;
            Arg = arg;
        }

        public override string ToString()
        {
            return $"printf(\"{FormatStrings[Type]}\", {Arg})";
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

    public class AsmMem : AsmArg // Offset over EBP
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public AsmArg Start { get; set; } = new AsmReg(AsmReg.Reg.Ebp);

        public static Dictionary<int, string> SizeType { get; } = new Dictionary<int, string>
        {
            {1, "byte" },
            {2, "word" },
            {4, "dword" },
            {8, "qword" },
        };

        public AsmMem(int offset, int size)
        {
            Offset = offset;
            Size = size;
        }

        public AsmMem(int offset, int size, AsmArg start) : this(offset, size) => this.Start = start;

        public AsmMem(int offset, int size, AsmReg.Reg start) : this(offset, size) => this.Start = new AsmReg(start);

        public override string ToString()
        {
            return $"{SizeType[Size]} ptr [{Start}{(Offset != 0 ? (-Offset).ToString() : "")}]";
        }
    }
}
