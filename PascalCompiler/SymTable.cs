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
            return LookUpLevel(name)?.Item1;
        }

        public (Symbol, int)? LookUpLevel(string name, int level = 0)
        {
            return Contains(name) ? ((Symbol)this[name], level) : Parent?.LookUpLevel(name, level + 1);
        }

        public void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Symbol
    {
        public string Name { get; set; }

        public override string ToString() => Name;

        public abstract void Generate(AsmCode code);
    }

    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol IntTypeSymbol = new TypeSymbol
        {
            Name = "integer",
            _size = 4,
        };

        public static readonly TypeSymbol RealTypeSymbol = new TypeSymbol
        {
            Name = "real",
            _size = 8,
        };

        public static readonly TypeSymbol CharTypeSymbol = new TypeSymbol
        {
            Name = "char",
            _size = 1,
        };

        protected TypeSymbol() { }

        private int _size;

        public override string ToString()
        {
            return $"{Name}";
        }

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }

        public virtual int Size => _size;
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

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }

        public override int Size => ElementType.Size * Length;
    }

    public class RecordTypeSymbol : TypeSymbol
    {
        private SymTable _fields;
        public SymTable Fields
        {
            get => _fields;
            set
            {
                _fields = value;
                SetOffsets();
            }
        }

        public override string ToString()
        {
            return "record";
        }

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }

        public override int Size => Fields.Values.Cast<VarSymbol>().Sum(field => field.Type.Size);

        public void SetOffsets()
        {
            var recOffset = 0;
            foreach (VarSymbol field in Fields.Values)
            {
                field.Offset = recOffset;
                recOffset -= field.Type.Size;
            }
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

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public class ConstSymbol : ValueSymbol
    {
        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public class TypedConstSymbol : ValueSymbol
    {
        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public class VarSymbol : ValueSymbol
    {
        public int Offset { get; set; }
    }

    public class ParameterSymbol : VarSymbol
    {
        public ParameterModifier ParameterModifier { get; set; } = ParameterModifier.Value;

        public override string ToString()
        {
            return $"{ParameterModifier} {base.ToString()}";
        }

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgramSymbol : Symbol
    {
        public override string ToString()
        {
            return $"program {Name}";
        }

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class SubprogramSymbol : Symbol
    {
        public Parameters Parameters { get; set; }
        public Block Block { get; set; }
        public AsmLabel Label { get; set; }
        public int ParameterOffset { get; set; }
        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }

    public class ProcedureSymbol : SubprogramSymbol
    {
        public override string ToString()
        {
            return
                $"procedure {Name}({string.Join(", ", Parameters.Select(t => (t.ParameterModifier, t.Name, t.Type.Name)))})";
        }

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
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

        public override void Generate(AsmCode code)
        {
            throw new NotImplementedException();
        }
    }
}