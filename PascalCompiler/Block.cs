﻿using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler
{
    public class PascalProgram
    {
        public ProgramSymbol Name { get; set; }
        public Block Block { get; set; }
    }

    public class Block
    {
        public SymTable SymTable { get; set; }
        public List<Statement> StatementList { get; set; }

        public void Generate(AsmCode asmCode)
        {
            asmCode.Add(AsmCmd.Cmd.Push, AsmReg.Reg.Ebp);
            asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Ebp, AsmReg.Reg.Esp);
            var curOffset = 0;
            foreach (string s in SymTable.Keys)
            {
                var t = SymTable[s];
                if (t.GetType() == typeof(VarSymbol)) // Must be EXACTLY var (cannot be parameters)
                {
                    var v = (VarSymbol) t;
//                    if (v.Type is RecordTypeSymbol r)
//                    {
//                        r.SetOffsets();
//                    }
                    curOffset += v.Type.Size;
                    v.Offset = curOffset;
                }
//                if (t is RecordTypeSymbol r)
//                {
//                    var recOffset = 0;
//                    foreach (VarSymbol field in r.Fields.Values)
//                    {
//                        recOffset -= field.Type.Size;
//                        field.Offset = recOffset;
//                    }
//                }
            }
            asmCode.Add(AsmCmd.Cmd.Sub, AsmReg.Reg.Esp, curOffset.ToString());
            // TODO: Generate initial values
//            foreach (string s in SymTable.Keys)  // Must be EXACTLY var (cannot be parameters)
//            {
//                var t = SymTable[s];
//                if (t.GetType() == typeof(VarSymbol))
//                {
//                    var v = (VarSymbol)t;
//                    
//                }
//            }
            foreach (Statement st in StatementList)
            {
                st.Generate(asmCode, SymTable);
            }
            asmCode.Add(AsmCmd.Cmd.Mov, AsmReg.Reg.Esp, AsmReg.Reg.Ebp);
            asmCode.Add(AsmCmd.Cmd.Pop, AsmReg.Reg.Ebp);
        }
    }

    public class Parameters : List<ParameterSymbol> { }

    public enum ParameterModifier
    {
        Value,
        Var,
        Const
    }
}
