using System.Collections.Generic;

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
    }

    public class Parameters : List<ParameterSymbol> { }

    public enum ParameterModifier
    {
        Value,
        Var,
        Const
    }
}
