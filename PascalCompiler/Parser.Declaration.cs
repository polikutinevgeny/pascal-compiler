using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PascalCompiler
{
    public partial class Parser
    {
        private TypeSymbol CurrentReturnType { get; set; } = null;

        private PascalProgram ParseProgram()
        {
            var c = Current;
            string name = "";
            if (c.SubType == Tokenizer.TokenSubType.Program)
            {
                Next();
                var n = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                name = n.Value.ToString();
                Require(Tokenizer.TokenSubType.Semicolon);
            }
            SymTable st = new SymTable()
            {
                {TypeSymbol.IntTypeSymbol.Name, TypeSymbol.IntTypeSymbol},
                {TypeSymbol.RealTypeSymbol.Name, TypeSymbol.RealTypeSymbol},
                {TypeSymbol.CharTypeSymbol.Name, TypeSymbol.CharTypeSymbol},
            };
            if (st.LookUp(name) != null)
            {
                throw new ParserException("Illegal program name", c.Line, c.Position);
            }
            ProgramSymbol pn = null;
            if (name != "")
            {
                pn = new ProgramSymbol() {Name = name};
                st.Add(name, pn);
            }
            else
            {
                pn = new ProgramSymbol() {Name = ""};
            }
            var b = ParseBlock(st);
            Require(Tokenizer.TokenSubType.Dot);
            return new PascalProgram() {Block = b, Name = pn};
        }

        private Block ParseBlock(SymTable symTable)
        {
            if (Current.SubType != Tokenizer.TokenSubType.Begin)
            {
                ParseDeclSection(symTable);
                var c = ParseCompoundStatement(symTable);
                return new Block() {StatementList = c.Statements, SymTable = symTable};
            }
            else
            {
                var c = ParseCompoundStatement(symTable);
                return new Block() {StatementList = c.Statements, SymTable = symTable};
            }
        }

        private void ParseDeclSection(SymTable symTable)
        {
            var c = Current;
            while (true)
            {
                switch (Current.SubType)
                {
                    case Tokenizer.TokenSubType.Const:
                        ParseConstSection(symTable);
                        break;
                    case Tokenizer.TokenSubType.Var:
                        ParseVarSection(symTable);
                        break;
                    case Tokenizer.TokenSubType.Type:
                        ParseTypeSection(symTable);
                        break;
                    case Tokenizer.TokenSubType.Procedure:
                        ParseProcedureDecl(symTable);
                        break;
                    case Tokenizer.TokenSubType.Function:
                        ParseFunctionDecl(symTable);
                        break;
                    default:
                        return;
                }
            }
        }

        private void ParseConstSection(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.Const);
            var c = Current;
            bool f = true;
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                ParseConstantDecl(symTable);
                Require(Tokenizer.TokenSubType.Semicolon);
                f = false;
            }
            if (f)
            {
                throw new ParserException("Empty constant section", c.Line, c.Position);
            }
        }

        private void ParseConstantDecl(SymTable symTable)
        {
            var c = Current;
            Next();
            if (Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                Next();
                var ce = ParseConstExpr(symTable);
                if (symTable.Contains(c.Value.ToString()))
                {
                    throw new ParserException($"Duplicate identifier {c.Value}", c.Line, c.Position);
                }
                symTable.Add(c.Value.ToString(),
                    new ConstSymbol()
                    {
                        Name = c.Value.ToString(),
                        Type = ce.Type,
                        Value = new SimpleConstant() {Type = ce.Type, Value = ce.Value}
                    });
                return;
            }
            if (Current.SubType == Tokenizer.TokenSubType.Colon)
            {
                Next();
                var t = ParseType(symTable);
                Require(Tokenizer.TokenSubType.Equal);
                var tc = ParseTypedConstant(symTable, t);
                CheckImplicitTypeCompatibility(tc.Type, t);
                symTable.Add(c.Value.ToString(),
                    new TypedConstSymbol()
                    {
                        Name = c.Value.ToString(),
                        Type = t,
                        Value = new SimpleConstant() {Type = t, Value = tc}
                    });
                return;
            }
            throw new ParserException($"Expected '=' or ':', got {Current.SubType}.", Current.Line, Current.Position);
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
                return new SimpleConstant() {Value = t, Type = type};
            }
            catch (EvaluatorException ex)
            {
                throw new ParserException(ex.Message, Current.Line, Current.Position);
            }
        }

        private ArrayConstant ParseArrayConstant(SymTable symTable, ArrayTypeSymbol arrayType)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            List<Constant> arrayElem =
                new List<Constant> {ParseTypedConstant(symTable, arrayType.ElementType)};
            while (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
            {
                Require(Tokenizer.TokenSubType.Comma);
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
            Require(Tokenizer.TokenSubType.RParenthesis);
            return new ArrayConstant() {Elements = arrayElem, Type = arrayType};
        }

        private RecordConstant ParseRecordConstant(SymTable symTable, RecordTypeSymbol recordType)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            Dictionary<VarSymbol, Constant> recordElem = new Dictionary<VarSymbol, Constant>();
            for (var index = 0; Current.SubType == Tokenizer.TokenSubType.Identifier && index < recordType.Fields.Count; ++index)
            {
                var i = Current;
                if (((VarSymbol) recordType.Fields[index]).Name == i.Value.ToString())
                {
                    Next();
                    Require(Tokenizer.TokenSubType.Colon);
                    var v = ParseTypedConstant(symTable, ((VarSymbol)recordType.Fields[index]).Type);
                    CheckImplicitTypeCompatibility(v.Type, ((VarSymbol)recordType.Fields[index]).Type);
                    recordElem.Add((VarSymbol)recordType.Fields[index], v);
                    if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
                    {
                        break;
                    }
                    Require(Tokenizer.TokenSubType.Semicolon);
                }
                else
                {
                    throw new ParserException("Invalid identifier", Current.Line, Current.Position);
                }
            }
            Require(Tokenizer.TokenSubType.RParenthesis);
            return new RecordConstant() {Type = recordType, Values = recordElem};
        }

        private void ParseVarSection(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.Var);
            var c = Current;
            ParseVarDecl(symTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                ParseVarDecl(symTable);
                Require(Tokenizer.TokenSubType.Semicolon);
            }
        }

        private void ParseVarDecl(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            var tmp = Current;
            List<string> idents = new List<string> {c.Value.ToString()};
            while (tmp.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                tmp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                idents.Add(tmp.Value.ToString());
                tmp = Current;
            }
            Require(Tokenizer.TokenSubType.Colon);
            var t = ParseType(symTable);
            if (idents.Count == 1 && Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                Next();
                var v = ParseTypedConstant(symTable, t);
                CheckImplicitTypeCompatibility(v.Type, t);
                symTable.Add(idents[0], new VarSymbol() {Name = idents[0], Type = t, Value = v});
                return;
            }
            if (Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                throw new ParserException("Only 1 variable can be initialized", Current.Line, Current.Position);
            }
            foreach (var i in idents)
            {
                symTable.Add(i, new VarSymbol() {Name = i, Type = t, Value = null});
            }
        }

        private void ParseTypeSection(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.Type);
            var c = Current;
            ParseTypeDecl(symTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                ParseTypeDecl(symTable);
                Require(Tokenizer.TokenSubType.Semicolon);
            }
        }

        private void ParseTypeDecl(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Equal);
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
            if (c.SubType == Tokenizer.TokenSubType.Identifier)
            {
                Next();
                var t = symTable.LookUp(c.Value.ToString());
                if (!(t is TypeSymbol))
                {
                    throw new ParserException("Error in type definition", c.Line, c.Position);
                }
                return (TypeSymbol) t;
            }
            if (c.SubType == Tokenizer.TokenSubType.Array)
            {
                return ParseArrayType(symTable);
            }
            if (c.SubType == Tokenizer.TokenSubType.Record)
            {
                return ParseRecordType(symTable);
            }
            throw new ParserException($"Expected type, found '{c.SubType}'", c.Line, c.Position);
        }

        private ArrayTypeSymbol ParseArrayType(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Array);
            Require(Tokenizer.TokenSubType.LBracket);
            var range = ParseSubrange(symTable);
            Require(Tokenizer.TokenSubType.RBracket);
            Require(Tokenizer.TokenSubType.Of);
            var t = ParseType(symTable);
            return new ArrayTypeSymbol() {Name = "#array", ElementType = t, Range = range};
        }

        private (int Left, int Right) ParseSubrange(SymTable symTable)
        {
            var c = Current;
            var l = ParseConstExpr(symTable).Value;
            Require(Tokenizer.TokenSubType.Range);
            var r = ParseConstExpr(symTable).Value;
            if (!(l is int && r is int))
            {
                throw new ParserException("Only integer values can be used in array range", c.Line, c.Position);
            }
            return (Left: (int) l, Right: (int) r);
        }

        private RecordTypeSymbol ParseRecordType(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Record);
            SymTable recSymTable = new SymTable() {Parent = null};
            ParseRecordFieldList(symTable, recSymTable);
            Require(Tokenizer.TokenSubType.End);
            return new RecordTypeSymbol() {Name = "#record", Fields = recSymTable};
        }

        private void ParseRecordFieldList(SymTable parentSymTable, SymTable recSymTable)
        {
            var c = Current;
            ParseFieldDecl(parentSymTable, recSymTable);
            if (Current.SubType == Tokenizer.TokenSubType.End)
            {
                return;
            }
            Require(Tokenizer.TokenSubType.Semicolon);
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                ParseFieldDecl(parentSymTable, recSymTable);
                if (Current.SubType == Tokenizer.TokenSubType.End)
                {
                    break;
                }
                Require(Tokenizer.TokenSubType.Semicolon);
            }
        }

        private void ParseFieldDecl(SymTable parentSymTable, SymTable recSymTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<string> idents = new List<string> {c.Value.ToString()};
            var t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                t = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                idents.Add(t.Value.ToString());
            }
            Require(Tokenizer.TokenSubType.Colon);
            var tp = ParseType(parentSymTable);
            foreach (var i in idents)
            {
                if (recSymTable.Contains(i))
                {
                    throw new ParserException($"Field {i} already declared", c.Line, c.Position);
                }
                recSymTable.Add(i, new VarSymbol() {Name = i, Type = tp, Value = null});
            }
        }

        private void ParseProcedureDecl(SymTable symTable)
        {
            var c = Current;
            SymTable procSymTable = new SymTable() {Parent = symTable};
            var h = ParseProcedureHeading(procSymTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            if (symTable.Contains(h.Name))
            {
                throw new ParserException($"Duplicate identifier {h.Name}", c.Line, c.Position);
            }
            var p = new ProcedureSymbol() {Name = h.Name, Parameters = h.Parameters};
            symTable.Add(h.Name, p);
            var backup = CurrentReturnType;
            CurrentReturnType = null;
            var b = ParseBlock(procSymTable);
            CurrentReturnType = backup;
            p.Block = b;
            Require(Tokenizer.TokenSubType.Semicolon);
        }

        private (string Name, Parameters Parameters) ParseProcedureHeading(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Procedure);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                var p = ParseFormalParameters(symTable);
                return (Name: i.Value.ToString(), Parameters: p);
            }
            return (Name: i.Value.ToString(), Parameters: new Parameters());
        }

        private Parameters ParseFormalParameters(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            Parameters parameters = new Parameters();
            if (Current.SubType == Tokenizer.TokenSubType.Identifier ||
                Current.SubType == Tokenizer.TokenSubType.Var ||
                Current.SubType == Tokenizer.TokenSubType.Const)
            {
                parameters.AddRange(ParseFormalParam(symTable));
            }
            if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
            {
                Next();
                return parameters;
            }
            Require(Tokenizer.TokenSubType.Semicolon);
            while (
                Current.SubType == Tokenizer.TokenSubType.Identifier ||
                Current.SubType == Tokenizer.TokenSubType.Var ||
                Current.SubType == Tokenizer.TokenSubType.Const)
            {
                parameters.AddRange(ParseFormalParam(symTable));
                if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
                {
                    Next();
                    return parameters;
                }
                Require(Tokenizer.TokenSubType.Semicolon);
            }
            Require(Tokenizer.TokenSubType.RParenthesis);
            return parameters;
        }

        private Parameters ParseFormalParam(SymTable symTable)
        {
            var c = Current;
            switch (c.SubType)
            {
                case Tokenizer.TokenSubType.Var:
                    return ParseVarParameter(symTable);
                case Tokenizer.TokenSubType.Const:
                    return ParseConstParameter(symTable);
                case Tokenizer.TokenSubType.Identifier:
                    return ParseParameter(symTable);
            }
            throw new ParserException($"Expected var, const or identifier, got {c.SubType}", c.Line, c.Position);
        }

        private Parameters ParseParameter(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<string> idents = new List<string> {c.Value.ToString()};
            while (Current.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                var tmp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                idents.Add(tmp.Value.ToString());
            }
            Require(Tokenizer.TokenSubType.Colon);
            var t = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            var tp = symTable.LookUp(t.Value.ToString());
            if (!(tp is TypeSymbol))
            {
                throw new ParserException("Illegal type declaration", t.Line, t.Position);
            }
            if (idents.Count == 1 && Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                Require(Tokenizer.TokenSubType.Equal);
                var v = ParseConstExpr(symTable);
                var s = new ParameterSymbol()
                {
                    Name = idents[0],
                    ParameterModifier = ParameterModifier.Value,
                    Type = (TypeSymbol) tp,
                    Value = v
                };
                symTable.Add(idents[0], s);
                return new Parameters {s};
            }
            if (Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                throw new ParserException("Only one parameter can have default value", Current.Line, Current.Position);
            }
            var p = new Parameters();
            foreach (var i in idents)
            {
                var s = new ParameterSymbol()
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
            Require(Tokenizer.TokenSubType.Var);
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<string> idents = new List<string> {c.Value.ToString()};
            while (Current.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                var tmp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                idents.Add(tmp.Value.ToString());
            }
            Require(Tokenizer.TokenSubType.Colon);
            var t = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            var tp = symTable.LookUp(t.Value.ToString());
            if (!(tp is TypeSymbol))
            {
                throw new ParserException("Illegal type declaration", t.Line, t.Position);
            }
            var p = new Parameters();
            foreach (var i in idents)
            {
                var s = new ParameterSymbol()
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
            Require(Tokenizer.TokenSubType.Const);
            var p = ParseParameter(symTable);
            foreach (ParameterSymbol ps in p)
            {
                ps.ParameterModifier = ParameterModifier.Const;
            }
            return p;
        }

        private void ParseFunctionDecl(SymTable symTable)
        {
            var c = Current;
            SymTable procSymTable = new SymTable() {Parent = symTable};
            var h = ParseFunctionHeading(procSymTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            if (symTable.Contains(h.Name))
            {
                throw new ParserException("Duplicate identifier {h.Name}", c.Line, c.Position);
            }
            var f = new FunctionSymbol()
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
            Require(Tokenizer.TokenSubType.Semicolon);
            f.Block = b;
        }

        private (string Name, Parameters Parameters, TypeSymbol ReturnType) ParseFunctionHeading(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Function);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                var p = ParseFormalParameters(symTable);
                Require(Tokenizer.TokenSubType.Colon);
                var rt = ParseType(symTable);
                return (Name: i.Value.ToString(), Parameters: p, ReturnType: rt);
            }
            Require(Tokenizer.TokenSubType.Colon);
            var ret = ParseType(symTable);
            return (Name: i.Value.ToString(), Parameters: new Parameters(), ReturnType: ret);
        }
    }
}