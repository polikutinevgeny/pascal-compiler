using System;
using System.Collections.Generic;
using static PascalCompiler.Tokenizer.TokenSubType;

namespace PascalCompiler
{
    public partial class Parser
    {
        private TypeSymbol CurrentReturnType { get; set; }

        private PascalProgram ParseProgram()
        {
            var c = Current;
            string name = "";
            if (c.SubType == Tokenizer.TokenSubType.Program)
            {
                Next();
                var n = Current;
                Require(Identifier);
                name = n.Value.ToString();
                Require(Semicolon);
            }
            SymTable st = new SymTable
            {
                {TypeSymbol.IntTypeSymbol.Name, TypeSymbol.IntTypeSymbol},
                {TypeSymbol.RealTypeSymbol.Name, TypeSymbol.RealTypeSymbol},
                {TypeSymbol.CharTypeSymbol.Name, TypeSymbol.CharTypeSymbol}
            };
            if (st.LookUp(name) != null)
            {
                throw new ParserException("Illegal program name", c.Line, c.Position);
            }
            ProgramSymbol pn;
            if (name != "")
            {
                pn = new ProgramSymbol {Name = name};
                st.Add(name, pn);
            }
            else
            {
                pn = new ProgramSymbol {Name = ""};
            }
            var b = ParseBlock(st);
            Require(Dot);
            return new PascalProgram {Block = b, Name = pn};
        }

        private Block ParseBlock(SymTable symTable)
        {
            if (Current.SubType != Begin)
            {
                ParseDeclSection(symTable);
            }
            return new Block {StatementList = ParseCompoundStatement(symTable).Statements, SymTable = symTable};
        }

        private void ParseDeclSection(SymTable symTable)
        {
            while (true)
            {
                switch (Current.SubType)
                {
                    case Const:
                        ParseConstSection(symTable);
                        break;
                    case Var:
                        ParseVarSection(symTable);
                        break;
                    case Tokenizer.TokenSubType.Type:
                        ParseTypeSection(symTable);
                        break;
                    case Procedure:
                        ParseProcedureDecl(symTable);
                        break;
                    case Function:
                        ParseFunctionDecl(symTable);
                        break;
                    default:
                        return;
                }
            }
        }

        private void ParseConstSection(SymTable symTable)
        {
            Require(Const);
            ParseConstantDecl(symTable);
            Require(Semicolon);
            while (Current.SubType == Identifier)
            {
                ParseConstantDecl(symTable);
                Require(Semicolon);
            }
        }

        private void ParseConstantDecl(SymTable symTable)
        {
            var c = Current;
            Next();
            if (Current.SubType == Equal)
            {
                Next();
                var ce = ParseConstExpr(symTable);
                if (symTable.Contains(c.Value.ToString()))
                {
                    throw new ParserException($"Duplicate identifier {c.Value}", c.Line, c.Position);
                }
                symTable.Add(c.Value.ToString(),
                    new ConstSymbol
                    {
                        Name = c.Value.ToString(),
                        Type = ce.Type,
                        Value = new SimpleConstant {Type = ce.Type, Value = ce.Value}
                    });
                return;
            }
            if (Current.SubType != Colon)
                throw new ParserException($"Expected '=' or ':', got {Current.SubType}.", Current.Line,
                    Current.Position);
            Next();
            var t = ParseType(symTable);
            Require(Equal);
            var tc = ParseTypedConstant(symTable, t);
            CheckImplicitTypeCompatibility(tc.Type, t);
            symTable.Add(c.Value.ToString(),
                new TypedConstSymbol
                {
                    Name = c.Value.ToString(),
                    Type = t,
                    Value = new SimpleConstant {Type = t, Value = tc}
                });
        }

        private Constant ParseTypedConstant(SymTable symTable, TypeSymbol t)
        {
            switch (t)
            {
                case ArrayTypeSymbol at:
                    return ParseArrayConstant(symTable, at);
                case RecordTypeSymbol rt:
                    return ParseRecordConstant(symTable, rt);
                default:
                    return ParseConstExpr(symTable);
            }
        }

        private SimpleConstant ParseConstExpr(SymTable symTable)
        {
            var e = ParseExpression(symTable);
            try
            {
                var t = EvaluateConstExpr(e, symTable);
                TypeSymbol type;
                switch (t)
                {
                    case int _:
                        type = TypeSymbol.IntTypeSymbol;
                        break;
                    case double _:
                        type = TypeSymbol.RealTypeSymbol;
                        break;
                    case char _:
                        type = TypeSymbol.CharTypeSymbol;
                        break;
                    default:
                        throw new InvalidOperationException("Const type is invalid");
                }
                return new SimpleConstant {Value = t, Type = type};
            }
            catch (EvaluatorException ex)
            {
                throw new ParserException(ex.Message, Current.Line, Current.Position);
            }
        }

        private ArrayConstant ParseArrayConstant(SymTable symTable, ArrayTypeSymbol arrayType)
        {
            Require(LParenthesis);
            List<Constant> arrayElem =
                new List<Constant> {ParseTypedConstant(symTable, arrayType.ElementType)};
            while (Current.SubType != RParenthesis)
            {
                Require(Comma);
                arrayElem.Add(ParseTypedConstant(symTable, arrayType.ElementType));
            }
            foreach (var el in arrayElem)
            {
                CheckImplicitTypeCompatibility(el.Type, arrayType.ElementType);
            }
            if (arrayType.Length != arrayElem.Count)
            {
                throw new ParserException("Invalid array length", Current.Line, Current.Position);
            }
            Require(RParenthesis);
            return new ArrayConstant {Elements = arrayElem, Type = arrayType};
        }

        private RecordConstant ParseRecordConstant(SymTable symTable, RecordTypeSymbol recordType)
        {
            Require(LParenthesis);
            Dictionary<VarSymbol, Constant> recordElem = new Dictionary<VarSymbol, Constant>();
            for (var index = 0;
                Current.SubType == Identifier && index < recordType.Fields.Count;
                ++index)
            {
                var i = Current;
                if (((VarSymbol) recordType.Fields[index]).Name != i.Value.ToString())
                {
                    throw new ParserException("Invalid identifier", Current.Line, Current.Position);
                }
                Next();
                Require(Colon);
                var v = ParseTypedConstant(symTable, ((VarSymbol) recordType.Fields[index]).Type);
                CheckImplicitTypeCompatibility(v.Type, ((VarSymbol) recordType.Fields[index]).Type);
                recordElem.Add((VarSymbol) recordType.Fields[index], v);
                if (Current.SubType == RParenthesis)
                {
                    break;
                }
                Require(Semicolon);
            }
            Require(RParenthesis);
            return new RecordConstant {Type = recordType, Values = recordElem};
        }

        private void ParseVarSection(SymTable symTable)
        {
            Require(Var);
            ParseVarDecl(symTable);
            Require(Semicolon);
            while (Current.SubType == Identifier)
            {
                ParseVarDecl(symTable);
                Require(Semicolon);
            }
        }

        private void ParseVarDecl(SymTable symTable)
        {
            var c = Current;
            Require(Identifier);
            var tmp = Current;
            List<string> idents = new List<string> {c.Value.ToString()};
            while (tmp.SubType == Comma)
            {
                Next();
                tmp = Current;
                Require(Identifier);
                idents.Add(tmp.Value.ToString());
                tmp = Current;
            }
            Require(Colon);
            var t = ParseType(symTable);
            if (idents.Count == 1 && Current.SubType == Equal)
            {
                Next();
                var v = ParseTypedConstant(symTable, t);
                CheckImplicitTypeCompatibility(v.Type, t);
                symTable.Add(idents[0], new VarSymbol {Name = idents[0], Type = t, Value = v});
                return;
            }
            if (Current.SubType == Equal)
            {
                throw new ParserException("Only 1 variable can be initialized", Current.Line, Current.Position);
            }
            foreach (var i in idents)
            {
                symTable.Add(i, new VarSymbol {Name = i, Type = t, Value = null});
            }
        }

        private void ParseTypeSection(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.Type);
            ParseTypeDecl(symTable);
            Require(Semicolon);
            while (Current.SubType == Identifier)
            {
                ParseTypeDecl(symTable);
                Require(Semicolon);
            }
        }

        private void ParseTypeDecl(SymTable symTable)
        {
            var c = Current;
            Require(Identifier);
            Require(Equal);
            var t = ParseType(symTable);
            switch (t)
            {
                case ArrayTypeSymbol at:
                    at.Name = c.Value.ToString();
                    symTable.Add(at.Name, at);
                    break;
                case RecordTypeSymbol rt:
                    rt.Name = c.Value.ToString();
                    symTable.Add(rt.Name, rt);
                    break;
                default:
                    symTable.Add(c.Value.ToString(), t);
                    break;
            }
        }

        private TypeSymbol ParseType(SymTable symTable)
        {
            var c = Current;
            switch (c.SubType)
            {
                case Identifier:
                    Next();
                    var t = symTable.LookUp(c.Value.ToString());
                    if (!(t is TypeSymbol))
                    {
                        throw new ParserException("Error in type definition", c.Line, c.Position);
                    }
                    return (TypeSymbol) t;
                case Tokenizer.TokenSubType.Array:
                    return ParseArrayType(symTable);
                case Record:
                    return ParseRecordType(symTable);
            }
            throw new ParserException($"Expected type, found '{c.SubType}'", c.Line, c.Position);
        }

        private ArrayTypeSymbol ParseArrayType(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.Array);
            Require(LBracket);
            var range = ParseSubrange(symTable);
            Require(RBracket);
            Require(Of);
            var t = ParseType(symTable);
            return new ArrayTypeSymbol {Name = "#array", ElementType = t, Range = range};
        }

        private (int Left, int Right) ParseSubrange(SymTable symTable)
        {
            var c = Current;
            var l = ParseConstExpr(symTable).Value;
            Require(Range);
            var r = ParseConstExpr(symTable).Value;
            if (!(l is int && r is int))
            {
                throw new ParserException("Only integer values can be used in array range", c.Line, c.Position);
            }
            return (Left: (int) l, Right: (int) r);
        }

        private RecordTypeSymbol ParseRecordType(SymTable symTable)
        {
            Require(Record);
            SymTable recSymTable = new SymTable {Parent = null};
            ParseRecordFieldList(symTable, recSymTable);
            Require(End);
            return new RecordTypeSymbol {Name = "#record", Fields = recSymTable};
        }

        private void ParseRecordFieldList(SymTable parentSymTable, SymTable recSymTable)
        {
            ParseFieldDecl(parentSymTable, recSymTable);
            if (Current.SubType == End)
            {
                return;
            }
            Require(Semicolon);
            while (Current.SubType == Identifier)
            {
                ParseFieldDecl(parentSymTable, recSymTable);
                if (Current.SubType == End)
                {
                    break;
                }
                Require(Semicolon);
            }
        }

        private void ParseFieldDecl(SymTable parentSymTable, SymTable recSymTable)
        {
            var c = Current;
            Require(Identifier);
            List<string> idents = new List<string> {c.Value.ToString()};
            var t = Current;
            while (t.SubType == Comma)
            {
                Next();
                t = Current;
                Require(Identifier);
                idents.Add(t.Value.ToString());
            }
            Require(Colon);
            var tp = ParseType(parentSymTable);
            foreach (var i in idents)
            {
                if (recSymTable.Contains(i))
                {
                    throw new ParserException($"Field {i} already declared", c.Line, c.Position);
                }
                recSymTable.Add(i, new VarSymbol {Name = i, Type = tp, Value = null});
            }
        }

        private void ParseProcedureDecl(SymTable symTable)
        {
            var c = Current;
            SymTable procSymTable = new SymTable {Parent = symTable};
            var h = ParseProcedureHeading(procSymTable);
            Require(Semicolon);
            if (symTable.Contains(h.Name))
            {
                throw new ParserException($"Duplicate identifier {h.Name}", c.Line, c.Position);
            }
            var p = new ProcedureSymbol {Name = h.Name, Parameters = h.Parameters};
            symTable.Add(h.Name, p);
            var backup = CurrentReturnType;
            CurrentReturnType = null;
            var b = ParseBlock(procSymTable);
            CurrentReturnType = backup;
            p.Block = b;
            Require(Semicolon);
        }

        private (string Name, Parameters Parameters) ParseProcedureHeading(SymTable symTable)
        {
            Require(Procedure);
            var i = Current;
            Require(Identifier);
            if (Current.SubType != LParenthesis)
                return (Name: i.Value.ToString(), Parameters: new Parameters());
            var p = ParseFormalParameters(symTable);
            return (Name: i.Value.ToString(), Parameters: p);
        }

        private Parameters ParseFormalParameters(SymTable symTable)
        {
            Require(LParenthesis);
            Parameters parameters = new Parameters();
            while (true)
            {
                switch (Current.SubType)
                {
                    case Var:
                        parameters.AddRange(ParseVarParameter(symTable));
                        break;
                    case Const:
                        parameters.AddRange(ParseConstParameter(symTable));
                        break;
                    case Identifier:
                        parameters.AddRange(ParseValueParameter(symTable));
                        break;
                    case RParenthesis:
                        Next();
                        return parameters;
                    default:
                        throw new ParserException($"Unexpected token {Current.SubType}", Current.Line,
                            Current.Position);
                }
                if (Current.SubType == RParenthesis)
                {
                    continue;
                }
                Require(Semicolon);
            }
        }

        private Parameters ParseValueParameter(SymTable symTable)
        {
            List<string> idents = new List<string>();
            while (Current.SubType == Identifier)
            {
                idents.Add(Current.Value.ToString());
                Next();
                if (Current.SubType != Comma)
                {
                    break;
                }
                Next();
            }
            Require(Colon);
            var t = Current;
            Require(Identifier);
            var tp = symTable.LookUp(t.Value.ToString());
            if (!(tp is TypeSymbol))
            {
                throw new ParserException("Illegal type declaration", t.Line, t.Position);
            }
            if (Current.SubType == Equal)
            {
                if (idents.Count != 1)
                    throw new ParserException("Only one parameter can have default value", Current.Line,
                        Current.Position);
                Require(Equal);
                var v = ParseConstExpr(symTable);
                var s = new ParameterSymbol
                {
                    Name = idents[0],
                    ParameterModifier = ParameterModifier.Value,
                    Type = (TypeSymbol)tp,
                    Value = v
                };
                symTable.Add(idents[0], s);
                return new Parameters { s };
            }
            var p = new Parameters();
            foreach (var i in idents)
            {
                var s = new ParameterSymbol
                {
                    Name = i,
                    ParameterModifier = ParameterModifier.Value,
                    Type = (TypeSymbol) tp,
                    Value = null
                };
                p.Add(s);
                symTable.Add(i, s);
            }
            return p;
        }

        private Parameters ParseVarParameter(SymTable symTable)
        {
            Require(Var);
            List<string> idents = new List<string>();
            while (Current.SubType == Identifier)
            {
                idents.Add(Current.Value.ToString());
                Next();
                if (Current.SubType != Comma)
                {
                    break;
                }
                Next();
            }
            if (idents.Count == 0)
                throw new ParserException("Empty parameter list", Current.Line, Current.Position);
            Require(Colon);
            var t = Current;
            Require(Identifier);
            var tp = symTable.LookUp(t.Value.ToString());
            if (!(tp is TypeSymbol))
            {
                throw new ParserException("Illegal type declaration", t.Line, t.Position);
            }
            var p = new Parameters();
            foreach (var i in idents)
            {
                var s = new ParameterSymbol
                {
                    Name = i,
                    ParameterModifier = ParameterModifier.Var,
                    Type = (TypeSymbol) tp
                };
                p.Add(s);
                symTable.Add(i, s);
            }
            return p;
        }

        private Parameters ParseConstParameter(SymTable symTable)
        {
            Require(Const);
            var p = ParseValueParameter(symTable);
            foreach (ParameterSymbol ps in p)
            {
                ps.ParameterModifier = ParameterModifier.Const;
            }
            return p;
        }

        private void ParseFunctionDecl(SymTable symTable)
        {
            var c = Current;
            SymTable procSymTable = new SymTable {Parent = symTable};
            var h = ParseFunctionHeading(procSymTable);
            Require(Semicolon);
            if (symTable.Contains(h.Name))
            {
                throw new ParserException("Duplicate identifier {h.Name}", c.Line, c.Position);
            }
            var f = new FunctionSymbol
            {
                Name = h.Name,
                Parameters = h.Parameters,
                ReturnType = h.ReturnType
            };
            symTable.Add(h.Name, f);
            var backup = CurrentReturnType;
            CurrentReturnType = f.ReturnType;
            var b = ParseBlock(procSymTable);
            CurrentReturnType = backup;
            Require(Semicolon);
            f.Block = b;
        }

        private (string Name, Parameters Parameters, TypeSymbol ReturnType) ParseFunctionHeading(SymTable symTable)
        {
            Require(Function);
            var i = Current;
            Require(Identifier);
            if (Current.SubType == LParenthesis)
            {
                var p = ParseFormalParameters(symTable);
                Require(Colon);
                var rt = ParseType(symTable);
                return (Name: i.Value.ToString(), Parameters: p, ReturnType: rt);
            }
            Require(Colon);
            var ret = ParseType(symTable);
            return (Name: i.Value.ToString(), Parameters: new Parameters(), ReturnType: ret);
        }
    }
}