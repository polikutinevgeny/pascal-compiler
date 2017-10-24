using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PascalCompiler
{
    public class Parser : IDisposable
    {
        public class ParserException : Exception
        {
            public uint Line { get; set; }
            public uint Position { get; set; }

            public ParserException(string message, uint line, uint position) : base(message)
            {
                Line = line;
                Position = position;
            }
        }

        public class EvaluatorException : Exception
        {
            public EvaluatorException(string message) : base(message)
            {
            }
        }

        private Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>> BinaryOps { get; set; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>>
            {
                {Tokenizer.TokenSubType.Plus, (l, r) => l + r},
                {Tokenizer.TokenSubType.Minus, (l, r) => l - r},
                {Tokenizer.TokenSubType.Asterisk, (l, r) => l * r},
                {Tokenizer.TokenSubType.Slash, (l, r) => (double) l / (double) r},
                {Tokenizer.TokenSubType.Greater, (l, r) => (int) (l > r)},
                {Tokenizer.TokenSubType.Less, (l, r) => (int) (l < r)},
                {Tokenizer.TokenSubType.GEqual, (l, r) => (int) (l >= r)},
                {Tokenizer.TokenSubType.LEqual, (l, r) => (int) (l <= r)},
                {Tokenizer.TokenSubType.NEqual, (l, r) => (int) (l != r)},
                {Tokenizer.TokenSubType.Equal, (l, r) => (long) (l == r)},
                {
                    Tokenizer.TokenSubType.Or, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l | r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Xor, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l ^ r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Div, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l / r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Mod, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l % r;
                    }
                },
                {
                    Tokenizer.TokenSubType.And, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l & r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Shl, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l << r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Shr, (l, r) =>
                    {
                        if (l.GetType() is double || r.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l >> r;
                    }
                },
            };

        private Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>> UnaryOps { get; set; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>>
            {
                {Tokenizer.TokenSubType.Plus, i => i},
                {Tokenizer.TokenSubType.Minus, i => -i},
                {
                    Tokenizer.TokenSubType.Not, i =>
                    {
                        if (i.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return ~i;
                    }
                },
            };

        private readonly IEnumerator<Tokenizer.Token> _tokenizer;

        private void Next() => _tokenizer.MoveNext();

        private Tokenizer.Token Current => _tokenizer.Current;

        public Parser(IEnumerator<Tokenizer.Token> tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public PascalProgram Parse()
        {
            Next();
            PascalProgram p = ParseProgram();
            Require(Tokenizer.TokenSubType.EndOfFile);
            return p;
        }

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
                pn = new ProgramSymbol() { Name = name };
                st.Add(name, pn);
            }
            else
            {
                pn = new ProgramSymbol() { Name = "" };
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
                if (symTable.ContainsKey(c.Value.ToString()))
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
            return new SimpleConstant() {Value = EvaluateConstExpr(e, symTable), Type = null};
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
            Require(Tokenizer.TokenSubType.RParenthesis);
            return new ArrayConstant() {Elements = arrayElem, Type = arrayType};
        }

        private RecordConstant ParseRecordConstant(SymTable symTable, RecordTypeSymbol recordType)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            Dictionary<VarSymbol, Constant> recordElem = new Dictionary<VarSymbol, Constant>();
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                var i = Current;
                var f = recordType.Fields.LookUp(i.Value.ToString());
                if (f == null)
                {
                    throw new ParserException($"Unknown identifier {i.Value}", i.Line, i.Position);
                }
                if (recordElem.ContainsKey((VarSymbol) f))
                {
                    throw new ParserException($"Field {i.Value} already initialized", i.Line, i.Position);
                }
                Next();
                Require(Tokenizer.TokenSubType.Colon);
                var v = ParseTypedConstant(symTable, ((VarSymbol) f).Type);
                recordElem.Add((VarSymbol) f, v);
                if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
                {
                    break;
                }
                Require(Tokenizer.TokenSubType.Semicolon);
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
//                Require(Tokenizer.TokenSubType.Semicolon);
            }
        }

        private void ParseTypeDecl(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Equal);
            var t = ParseType(symTable);
//            Next();
            t.Name = c.Value.ToString();
            switch (t)
            {
                case ArrayTypeSymbol at:
                    symTable.Add(at.Name, at);
                    break;
                case RecordTypeSymbol rt:
                    symTable.Add(rt.Name, rt);
                    break;
                default:
                    symTable.Add(t.Name, t);
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
            var rangeList = new List<(int Left, int Right)> {ParseSubrange(symTable)};
            while (Current.SubType != Tokenizer.TokenSubType.RBracket)
            {
                Require(Tokenizer.TokenSubType.Comma);
                rangeList.Add(ParseSubrange(symTable));
            }
            Require(Tokenizer.TokenSubType.RBracket);
            Require(Tokenizer.TokenSubType.Of);
            var t = ParseType(symTable);
            return new ArrayTypeSymbol() {Name = null, ElementType = t, Ranges = rangeList};
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
            return new RecordTypeSymbol() {Name = null, Fields = recSymTable};
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
                if (recSymTable.ContainsKey(i))
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
            var b = ParseBlock(procSymTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            symTable.Add(h.Name, new ProcedureSymbol() {Name = h.Name, Block = b, Parameters = h.Parameters});
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
                var s = new ParameterSymbol() {Name = idents[0], ParameterModifier = ParameterModifier.Value, Type = (TypeSymbol)tp, Value = v};
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
                    Type = (TypeSymbol)tp,
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
                    Type = (TypeSymbol)tp
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
            var b = ParseBlock(procSymTable);
            Require(Tokenizer.TokenSubType.Semicolon);
            symTable.Add(h.Name,
                new FunctionSymbol() {Name = h.Name, Block = b, Parameters = h.Parameters, ReturnType = h.ReturnType});
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

        private CompoundStatementNode ParseCompoundStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Begin);
            var l = ParseStatementList(symTable);
            Require(Tokenizer.TokenSubType.End);
            return new CompoundStatementNode(null, "Compound statement", c.Line, c.Position)
            {
                Statements = l
            };
        }

        private List<Statement> ParseStatementList(SymTable symTable)
        {
            var c = Current;
            if (Current.SubType == Tokenizer.TokenSubType.End)
            {
                return new List<Statement>();
            }
            List<Statement> statementList = new List<Statement> {ParseStatement(symTable)};
            while (Current.SubType == Tokenizer.TokenSubType.Semicolon)
            {
                Next();
                var t = ParseStatement(symTable);
                if (t != null)
                {
                    statementList.Add(t);
                }
            }
            return statementList;
        }

        private Statement ParseStatement(SymTable symTable)
        {
            var c = Current;
            switch (c.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                case Tokenizer.TokenSubType.Read:
                case Tokenizer.TokenSubType.Write:
                    return ParseSimpleStatement(symTable);
                case Tokenizer.TokenSubType.While:
                case Tokenizer.TokenSubType.If:
                case Tokenizer.TokenSubType.For:
                case Tokenizer.TokenSubType.Repeat:
                case Tokenizer.TokenSubType.Begin:
                    return ParseStructStatement(symTable);
                case Tokenizer.TokenSubType.Break:
                    Next();
                    return new BreakNode(null, c);
                case Tokenizer.TokenSubType.Continue:
                    Next();
                    return new ContinueNode(null, c);
            }
            return null;
        }

        private SimpleStatementNode ParseSimpleStatement(SymTable symTable)
        {
            var c = Current;
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Read:
                    return ParseReadStatement(symTable);
                case Tokenizer.TokenSubType.Write:
                    return ParseWriteStatement(symTable);
            }
            var d = ParseDesignator(symTable);
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.LParenthesis:
                    {
                        Next();
                        if (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
                        {
                            var e = ParseExpressionList(symTable);
                            Require(Tokenizer.TokenSubType.RParenthesis);
                            return new SimpleStatementNode(new List<Node> { d, e }, "Call in statement", c.Line, c.Position);
                        }
                        Next();
                        return new SimpleStatementNode(new List<Node> { d }, "Call in statement", c.Line, c.Position);
                    }
                case Tokenizer.TokenSubType.Assign:
                case Tokenizer.TokenSubType.AsteriskAssign:
                case Tokenizer.TokenSubType.SlashAssign:
                case Tokenizer.TokenSubType.PlusAssign:
                case Tokenizer.TokenSubType.MinusAssign:
                    {
                        Next();
                        var e = ParseExpression(symTable);
                        return new SimpleStatementNode(new List<Node> { d, e }, "Assignment statement", c.Line, c.Position);
                    }
            }
            return new SimpleStatementNode(new List<Node> { d }, "Call in statement", c.Line, c.Position);
        }

        private DesignatorNode ParseDesignator(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node> { new IdentNode(null, c) };
            while (Current.SubType == Tokenizer.TokenSubType.Dot)
            {
                Next();
                var i = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                childs.Add(new IdentNode(null, i));
            }
            if (Current.SubType == Tokenizer.TokenSubType.LBracket)
            {
                Next();
                if (Current.SubType != Tokenizer.TokenSubType.RBracket)
                {
                    childs.Add(ParseExpressionList(symTable));
                    Require(Tokenizer.TokenSubType.RBracket);
                }
                else
                {
                    Next();
                }
            }
            return new DesignatorNode(childs, "Designator", c.Line, c.Position);
        }

        private ExpressionListNode ParseExpressionList(SymTable symTable)
        {
            var c = Current;
            List<Node> childs = new List<Node> { ParseExpression(symTable) };
            while (Current.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                childs.Add(ParseExpression(symTable));
            }
            return new ExpressionListNode(childs, "Expression list", c.Line, c.Position);
        }

        private ReadStatementNode ParseReadStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Read);
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                Next();
                var e = ParseExpressionList(symTable);
                Require(Tokenizer.TokenSubType.RParenthesis);
                return new ReadStatementNode(new List<Node> { e }, "Read", c.Line, c.Position);
            }
            return new ReadStatementNode(null, "Read", c.Line, c.Position);
        }

        private WriteStatementNode ParseWriteStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Write);
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                Next();
                if (Current.SubType == Tokenizer.TokenSubType.StringConstant)
                {
                    var s = Current;
                    Next();
                    Require(Tokenizer.TokenSubType.RParenthesis);
                    return new WriteStatementNode(new List<Node> { new StringNode(null, s) }, "Write", c.Line,
                        c.Position);
                }
                var e = ParseExpressionList(symTable);
                Require(Tokenizer.TokenSubType.RParenthesis);
                return new WriteStatementNode(new List<Node> { e }, "Write", c.Line, c.Position);
            }
            return new WriteStatementNode(null, "Write", c.Line, c.Position);
        }

        private StructStatementNode ParseStructStatement(SymTable symTable)
        {
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Begin:
                    return ParseCompoundStatement(symTable);
                case Tokenizer.TokenSubType.If:
                    return ParseIfStatement(symTable);
                case Tokenizer.TokenSubType.For:
                    return ParseForStatement(symTable);
                case Tokenizer.TokenSubType.While:
                    return ParseWhileStatement(symTable);
                case Tokenizer.TokenSubType.Repeat:
                    return ParseRepeatStatement(symTable);
            }
            throw new InvalidOperationException($"Current.Subtype was equal to {Current.SubType}");
        }

        private IfStatementNode ParseIfStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.If);
            var i = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.Then);
            var t = ParseStatement(symTable);
            if (Current.SubType == Tokenizer.TokenSubType.Else)
            {
                Next();
                var e = ParseStatement(symTable);
                return new IfStatementNode(new List<Node> { i, t, e }, "If", c.Line, c.Position);
            }
            return new IfStatementNode(new List<Node> { i, t }, "If", c.Line, c.Position);
        }

        private ForStatementNode ParseForStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.For);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Assign);
            var e = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.To);
            var u = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.Do);
            var s = ParseStatement(symTable);
            return new ForStatementNode(new List<Node> { new IdentNode(null, i), e, u, s }, "For", c.Line, c.Position);
        }

        private WhileStatementNode ParseWhileStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.While);
            var e = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.Do);
            var s = ParseStatement(symTable);
            return new WhileStatementNode(new List<Node> {s}, "While", c.Line, c.Position)
            {
                Condition = e
            };
        }

        private RepeatStatementNode ParseRepeatStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Repeat);
            var s = ParseStatementList(symTable);
            Require(Tokenizer.TokenSubType.Until);
            var e = ParseExpression(symTable);
            return new RepeatStatementNode(new List<Node>(s), "Repeat", c.Line, c.Position)
            {
                Condition = e
            };
        }

        private static HashSet<Tokenizer.TokenSubType> RelOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Tokenizer.TokenSubType.Less,
            Tokenizer.TokenSubType.Greater,
            Tokenizer.TokenSubType.LEqual,
            Tokenizer.TokenSubType.GEqual,
            Tokenizer.TokenSubType.NEqual,
            Tokenizer.TokenSubType.Equal,
        };

        private static HashSet<Tokenizer.TokenSubType> AddOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Tokenizer.TokenSubType.Plus,
            Tokenizer.TokenSubType.Minus,
            Tokenizer.TokenSubType.Or,
            Tokenizer.TokenSubType.Xor,
        };

        private static HashSet<Tokenizer.TokenSubType> MulOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Tokenizer.TokenSubType.Asterisk,
            Tokenizer.TokenSubType.Slash,
            Tokenizer.TokenSubType.Div,
            Tokenizer.TokenSubType.Mod,
            Tokenizer.TokenSubType.And,
            Tokenizer.TokenSubType.Shl,
            Tokenizer.TokenSubType.Shr,
        };

        private Node ParseExpression(SymTable symTable)
        {
            Node e = ParseSimpleExpression(symTable);
            var c = Current;
            while (RelOps.Contains(c.SubType))
            {
                Next();
                e = new BinOpNode(new List<Node> { e, ParseSimpleExpression(symTable) }, c.SubType, c.Line, c.Position);
                c = Current;
            }
            return e;
        }

        private Node ParseSimpleExpression(SymTable symTable)
        {
            if (Current.SubType == Tokenizer.TokenSubType.CharConstant)
            {
                var tmp = Current;
                Next();
                return new ConstNode(null, tmp.Value, tmp.Line, tmp.Position);
            }
            Node t = ParseTerm(symTable);
            var c = Current;
            while (AddOps.Contains(c.SubType))
            {
                Next();
                t = new BinOpNode(new List<Node> { t, ParseTerm(symTable) }, c.SubType, c.Line, c.Position);
                c = Current;
            }
            return t;
        }

        private Node ParseTerm(SymTable symTable)
        {
            Node f = ParseFactor(symTable);
            var c = Current;
            while (MulOps.Contains(c.SubType))
            {
                Next();
                f = new BinOpNode(new List<Node> { f, ParseFactor(symTable) }, c.SubType, c.Line, c.Position);
                c = Current;
            }
            return f;
        }

        private Node ParseFactor(SymTable symTable)
        {
            var c = Current;
            switch (c.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                    var d = ParseDesignator(symTable);
                    if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
                    {
                        Next();
                        if (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
                        {
                            var ex = ParseExpressionList(symTable);
                            Require(Tokenizer.TokenSubType.RParenthesis);
                            return new CallNode(new List<Node> { d, ex }, "Call in expression", c.Line, c.Position);
                        }
                        Next();
                        return new CallNode(new List<Node> { d }, "Call in expression", c.Line, c.Position);
                    }
                    return d;
                case Tokenizer.TokenSubType.IntegerConstant:
                case Tokenizer.TokenSubType.FloatConstant:
                    Next();
                    return new ConstNode(null, c.Value, c.Line, c.Position);
                case Tokenizer.TokenSubType.LParenthesis:
                    Next();
                    Node e = ParseExpression(symTable);
                    Require(Tokenizer.TokenSubType.RParenthesis);
                    return e;
                case Tokenizer.TokenSubType.Plus:
                case Tokenizer.TokenSubType.Minus:
                case Tokenizer.TokenSubType.Not:
                    Next();
                    return new UnOpNode(new List<Node> { ParseFactor(symTable) }, c.SubType, c.Line, c.Position);
            }
            throw new ParserException($"Unexpected token {c.SubType}", c.Line, c.Position);
        }

        private dynamic EvaluateConstExpr(Node expr, SymTable symTable)
        {
            if (expr is ConstNode)
            {
                return expr.Value;
            }
            if (expr is DesignatorNode)
            {
                var temp = symTable.LookUp(expr.Childs[0].Value.ToString());
                if (expr.Childs.Count == 1 && (temp is ConstSymbol))
                {
                    return ((SimpleConstant) (temp as ConstSymbol).Value).Value;
                }
                throw new EvaluatorException("Illegal operation");
            }
            if (expr is BinOpNode)
            {
                return BinaryOps[(Tokenizer.TokenSubType)expr.Value](EvaluateConstExpr(expr.Childs[0], symTable),
                    EvaluateConstExpr(expr.Childs[1], symTable));
            }
            if (expr is UnOpNode)
            {
                return UnaryOps[(Tokenizer.TokenSubType)expr.Value](EvaluateConstExpr(expr.Childs[0], symTable));
            }
            throw new InvalidOperationException($"Node of type {expr.GetType()} met in expression");
        }

        private void Require(Tokenizer.TokenSubType type)
        {
            if (Current.SubType != type)
                throw new ParserException($"Expected {type}, got {Current.SubType}", Current.Line,
                    Current.Position);
            Next();
        }

        public void Dispose()
        {
        }
    }
}