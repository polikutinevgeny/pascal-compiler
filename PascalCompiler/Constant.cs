using System.Collections.Generic;

namespace PascalCompiler
{
    public abstract class Constant
    {
        public TypeSymbol Type { get; set; }
    }

    public class SimpleConstant : Constant
    {
        public object Value { get; set; }

        public override string ToString() => $"{Value}";
    }

    public abstract class StructConstant : Constant
    {

    }

    public class ArrayConstant : StructConstant
    {
        public List<Constant> Elements { get; set; }

        public override string ToString() => $"({string.Join(", ", Elements)})";
    }

    public class RecordConstant : StructConstant
    {
        public Dictionary<VarSymbol, Constant> Values;

        public override string ToString() => $"({string.Join(", ", Values)})";
    }
}
