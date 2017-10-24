using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler
{
    public class SymTable : Dictionary<string, Symbol>
    {
        public SymTable Parent { get; set; }

        public Symbol LookUp(string name)
        {
            if (ContainsKey(name))
                return this[name];
            return Parent?.LookUp(name);
        }
    }

    public abstract class Symbol
    {
        public string Name { get; set; }

        public override string ToString() => Name;
    }

    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol IntTypeSymbol = new TypeSymbol()
        {
            Name = "integer",
            Parent = null
        };

        public static readonly TypeSymbol RealTypeSymbol = new TypeSymbol()
        {
            Name = "real",
            Parent = null
        };

        public static readonly TypeSymbol CharTypeSymbol = new TypeSymbol()
        {
            Name = "char",
            Parent = null
        };

        public TypeSymbol Parent { get; private set; }

        public TypeSymbol() { }

        public TypeSymbol(TypeSymbol parent)
        {
            TypeSymbol p = parent;
            while (p.Parent != null)
                p = p.Parent;
            Parent = p;
        }

        public override string ToString()
        {
            return $"({(Name != null ? Name.ToString() + " = " : "")}({Parent?.ToString() ?? "built-in"}))";
        }
    }

    public class ArrayTypeSymbol : TypeSymbol
    {
        public TypeSymbol ElementType { get; set; }
        public List<(int Begin, int End)> Ranges { get; set; } = new List<(int Begin, int End)>();
        public ArrayTypeSymbol() { }

        public override string ToString()
        {
            return $"{(Name != null ? Name.ToString() + " = " : "")}array of {ElementType}";
        }
    }

    public class RecordTypeSymbol : TypeSymbol
    {
        public SymTable Fields;
        public RecordTypeSymbol() { }

        public override string ToString()
        {
            return $"{(Name != null ? Name.ToString() + " = " : "")}record ({string.Join(", ", Fields)})";
        }
    }

    public class ConstSymbol : Symbol
    {
        public TypeSymbol Type { get; set; }
        public Constant Value { get; set; }

        public override string ToString()
        {
            return $"{Name} : {Type} = {Value}";
        }
    }

    public class TypedConstSymbol : Symbol
    {
        public TypeSymbol Type { get; set; }
        public Constant Value { get; set; }

        public override string ToString()
        {
            return $"{Name} : {Type} = {Value}";
        }
    }

    public class VarSymbol : Symbol
    {
        public TypeSymbol Type { get; set; }
        public Constant Value { get; set; }

        public override string ToString()
        {
            return $"{Name} : {Type} {(Value != null ? $"= {Value}" : "")}";
        }
    }

    public class ParameterSymbol : VarSymbol
    {
        public ParameterModifier ParameterModifier { get; set; } = ParameterModifier.Value;

        public override string ToString()
        {
            return base.ToString() + $" modifier: {ParameterModifier}";
        }
    }

    public class ProgramSymbol : Symbol { }

    public abstract class SubprogramSymbol : Symbol
    {
        public Parameters Parameters { get; set; }
        public Block Block { get; set; }
    }

    public class ProcedureSymbol : SubprogramSymbol
    {
    }

    public class FunctionSymbol : SubprogramSymbol
    {
        public TypeSymbol ReturnType { get; set; }
    }
}
