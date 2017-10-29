using System;
using System.Collections.Specialized;
using System.Linq;

namespace PascalCompiler
{
    public class SymTable : OrderedDictionary
    {
        public SymTable Parent { private get; set; }

        public Symbol LookUp(string name)
        {
            if (Contains(name))
                return (Symbol) this[name];
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
        public static readonly TypeSymbol IntTypeSymbol = new TypeSymbol
        {
            Name = "integer"
        };

        public static readonly TypeSymbol RealTypeSymbol = new TypeSymbol
        {
            Name = "real"
        };

        public static readonly TypeSymbol CharTypeSymbol = new TypeSymbol
        {
            Name = "char"
        };

        public override string ToString()
        {
            return $"{Name}";
        }
    }

    public class ArrayTypeSymbol : TypeSymbol
    {
        public TypeSymbol ElementType { get; set; }
        public (int Begin, int End) Range { get; set; }
        public int Length => Range.End - Range.Begin + 1;

        public override string ToString()
        {
            return $"[{Range.Begin}..{Range.End}]: array of {ElementType}";
        }
    }

    public class RecordTypeSymbol : TypeSymbol
    {
        public SymTable Fields;

        public override string ToString()
        {
            return "record";
        }
    }

    public abstract class ValueSymbol : Symbol
    {
        public TypeSymbol Type { get; set; }
        public Constant Value { get; set; }

        public override string ToString()
        {
            return $"{Type.Name}{(Value != null ? $" = {Value}" : "")}";
        }
    }

    public class ConstSymbol : ValueSymbol
    {
    }

    public class TypedConstSymbol : ValueSymbol
    {
    }

    public class VarSymbol : ValueSymbol
    {
    }

    public class ParameterSymbol : VarSymbol
    {
        public ParameterModifier ParameterModifier { get; set; } = ParameterModifier.Value;

        public override string ToString()
        {
            return $"{ParameterModifier} {base.ToString()}";
        }
    }

    public class ProgramSymbol : Symbol
    {
        public override string ToString()
        {
            return $"program {Name}";
        }
    }

    public abstract class SubprogramSymbol : Symbol
    {
        public Parameters Parameters { get; set; }
        public Block Block { get; set; }
    }

    public class ProcedureSymbol : SubprogramSymbol
    {
        public override string ToString()
        {
            return
                $"procedure {Name}({string.Join(", ", Parameters.Select(t => (t.ParameterModifier, t.Name, t.Type.Name)))})";
        }
    }

    public class FunctionSymbol : SubprogramSymbol
    {
        public TypeSymbol ReturnType { get; set; }

        public override string ToString()
        {
            return
                $"function {Name}({string.Join(", ", Parameters.Select(t => (t.ParameterModifier, t.Name, t.Type.Name)))}): {ReturnType.Name}";
        }
    }
}