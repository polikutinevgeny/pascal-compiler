using System;
using System.Collections.Generic;
using System.Linq;
using static PascalCompiler.Tokenizer.TokenSubType;

namespace PascalCompiler
{
    public partial class Parser : IDisposable
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

        private int CycleCounter { get; set; }

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
            var p = ParseProgram();
            Require(EndOfFile);
            return p;
        }

        private CompoundStatementNode ParseCompoundStatement(SymTable symTable)
        {
            var c = Current;
            Require(Begin);
            var l = ParseStatementList(symTable);
            Require(End);
            return new CompoundStatementNode(null, "Compound statement", c.Line, c.Position)
            {
                Statements = l
            };
        }

        private List<Statement> ParseStatementList(SymTable symTable)
        {
            if (Current.SubType == End)
            {
                return new List<Statement>();
            }
            var statementList = new List<Statement> {ParseStatement(symTable)};
            while (Current.SubType == Semicolon)
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
                case Identifier:
                case Read:
                case Write:
                    return ParseSimpleStatement(symTable);
                case While:
                case If:
                case For:
                case Repeat:
                case Begin:
                    return ParseStructStatement(symTable);
                case Break:
                    if (CycleCounter == 0)
                    {
                        throw new ParserException("Break outside of cycle", c.Line, c.Position);
                    }
                    Next();
                    return new BreakNode(null, c);
                case Continue:
                    if (CycleCounter == 0)
                    {
                        throw new ParserException("Continue outside of cycle", c.Line, c.Position);
                    }
                    Next();
                    return new ContinueNode(null, c);
                case Exit:
                    return ParseExitStatement(symTable);
            }
            return null;
        }

        private Statement ParseExitStatement(SymTable symTable)
        {
            var c = Current;
            Require(Exit);
            if (Current.SubType != LParenthesis)
            {
                return new ExitNode(null, c.Value, c.Line, c.Position);
            }
            Next();
            if (Current.SubType == RParenthesis)
            {
                Next();
                return new ExitNode(null, c.Value, c.Line, c.Position);
            }
            if (CurrentReturnType == null)
            {
                throw new ParserException("Procedures cannot return a value", Current.Line, Current.Position);
            }
            var e = ParseExpression(symTable);
            Require(RParenthesis);
            CheckImplicitTypeCompatibility((TypeSymbol) e.Type, CurrentReturnType);
            return new ExitNode(new List<Node> {e}, c.Value, c.Line, c.Position);
        }

        private SimpleStatementNode ParseSimpleStatement(SymTable symTable)
        {
            var c = Current;
            switch (Current.SubType)
            {
                case Read:
                    return ParseReadStatement(symTable);
                case Write:
                    return ParseWriteStatement(symTable);
            }
            var d = ParseDesignator(symTable);
            if (d is CallOperator co)
            {
                return new CallStatementWrapper(co);
            }
            switch (Current.SubType)
            {
                case Assign:
                case AsteriskAssign:
                case SlashAssign:
                case PlusAssign:
                case MinusAssign:
                {
                    Next();
                    var e = ParseExpression(symTable);
                    CheckImplicitTypeCompatibility((TypeSymbol) e.Type, (TypeSymbol) d.Type);
                    return new SimpleStatementNode(new List<Node> {d, e}, "Assignment statement", c.Line, c.Position);
                }
            }
            throw new ParserException("Illegal statement", c.Line, c.Position);
        }

        private DesignatorNode ParseDesignator(SymTable symTable)
        {
            var c = Current;
            Require(Identifier);
            var t = symTable.LookUp(c.Value.ToString());
            switch (t)
            {
                case TypeSymbol ts:
                    return ParseCast(symTable, new IdentNode(null, c) {Type = ts});
                case ProcedureSymbol ps:
                    return ParseProcedureCall(symTable, new IdentNode(null, c) {Type = ps});
                case FunctionSymbol fs:
                    return ParseFunctionCall(symTable, new IdentNode(null, c) {Type = fs});
                case ValueSymbol vs:
                    DesignatorNode v = new IdentNode(null, c)
                    {
                        Type = vs.Type
                    };
                    switch (vs.Type)
                    {
                        case RecordTypeSymbol _:
                            return ParseMemberAccess(symTable, v);
                        case ArrayTypeSymbol _:
                            return ParseIndex(symTable, v);
                    }
                    return v;
                default:
                    throw new ParserException("Illegal identifier", c.Line, c.Position);
            }
        }

        private DesignatorNode ParseProcedureCall(SymTable symTable, DesignatorNode procedure)
        {
            var ps = (ProcedureSymbol) procedure.Type;
            var p = ParseActualParameters(symTable);
            CheckParameters(ps.Parameters, p);
            return new CallOperator(p.Childs, "Call", p.Line, p.Position)
            {
                Subprogram = ps
            };
        }

        private ExpressionListNode ParseExpressionList(SymTable symTable)
        {
            var c = Current;
            var childs = new List<ExpressionNode> {ParseExpression(symTable)};
            while (Current.SubType == Comma)
            {
                Next();
                childs.Add(ParseExpression(symTable));
            }
            return new ExpressionListNode(childs, "Expression list", c.Line, c.Position);
        }

        private ReadStatementNode ParseReadStatement(SymTable symTable)
        {
            var c = Current;
            Require(Read);
            if (Current.SubType != LParenthesis)
                return new ReadStatementNode(null, "Read", c.Line, c.Position);
            Next();
            var e = ParseExpressionList(symTable);
            Require(RParenthesis);
            return new ReadStatementNode(new List<Node> {e}, "Read", c.Line, c.Position);
        }

        private WriteStatementNode ParseWriteStatement(SymTable symTable)
        {
            var c = Current;
            Require(Write);
            if (Current.SubType != LParenthesis)
                return new WriteStatementNode(null, "Write", c.Line, c.Position);
            Next();
            if (Current.SubType == StringConstant)
            {
                var s = Current;
                Next();
                Require(RParenthesis);
                return new WriteStatementNode(new List<Node> {new StringNode(null, s)}, "Write", c.Line,
                    c.Position);
            }
            var e = ParseExpressionList(symTable);
            Require(RParenthesis);
            return new WriteStatementNode(new List<Node> {e}, "Write", c.Line, c.Position);
        }

        private StructStatementNode ParseStructStatement(SymTable symTable)
        {
            switch (Current.SubType)
            {
                case Begin:
                    return ParseCompoundStatement(symTable);
                case If:
                    return ParseIfStatement(symTable);
                case For:
                    return ParseForStatement(symTable);
                case While:
                    return ParseWhileStatement(symTable);
                case Repeat:
                    return ParseRepeatStatement(symTable);
            }
            throw new InvalidOperationException($"Current.Subtype was equal to {Current.SubType}");
        }

        private IfStatementNode ParseIfStatement(SymTable symTable)
        {
            var c = Current;
            Require(If);
            var i = ParseExpression(symTable);
            Require(Then);
            var t = ParseStatement(symTable);
            if (Current.SubType != Else)
                return new IfStatementNode(new List<Node> {i, t}, "If", c.Line, c.Position);
            Next();
            var e = ParseStatement(symTable);
            return new IfStatementNode(new List<Node> {i, t, e}, "If", c.Line, c.Position);
        }

        private ForStatementNode ParseForStatement(SymTable symTable)
        {
            var c = Current;
            Require(For);
            var i = Current;
            Require(Identifier);
            Require(Assign);
            var e = ParseExpression(symTable);
            Require(To);
            var u = ParseExpression(symTable);
            Require(Do);
            ++CycleCounter;
            var s = ParseStatement(symTable);
            --CycleCounter;
            return new ForStatementNode(new List<Node> {new IdentNode(null, i), e, u, s}, "For", c.Line, c.Position);
        }

        private WhileStatementNode ParseWhileStatement(SymTable symTable)
        {
            var c = Current;
            Require(While);
            var e = ParseExpression(symTable);
            Require(Do);
            ++CycleCounter;
            var s = ParseStatement(symTable);
            --CycleCounter;
            return new WhileStatementNode(new List<Node> {s}, "While", c.Line, c.Position)
            {
                Condition = e
            };
        }

        private RepeatStatementNode ParseRepeatStatement(SymTable symTable)
        {
            var c = Current;
            Require(Repeat);
            ++CycleCounter;
            var s = ParseStatementList(symTable);
            --CycleCounter;
            Require(Until);
            var e = ParseExpression(symTable);
            return new RepeatStatementNode(new List<Node>(s), "Repeat", c.Line, c.Position)
            {
                Condition = e
            };
        }

        private static HashSet<Tokenizer.TokenSubType> RelOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Less,
            Greater,
            LEqual,
            GEqual,
            NEqual,
            Equal
        };

        private static HashSet<Tokenizer.TokenSubType> AddOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Plus,
            Minus,
            Or,
            Xor
        };

        private static HashSet<Tokenizer.TokenSubType> MulOps { get; } = new HashSet<Tokenizer.TokenSubType>
        {
            Asterisk,
            Slash,
            Div,
            Mod,
            And,
            Shl,
            Shr
        };

        private ExpressionNode ParseExpression(SymTable symTable)
        {
            var e = ParseSimpleExpression(symTable);
            var c = Current;
            while (RelOps.Contains(c.SubType))
            {
                Next();
                var t = ParseSimpleExpression(symTable);
                if (
                    t.Type is ArrayTypeSymbol || t.Type is RecordTypeSymbol ||
                    e.Type is ArrayTypeSymbol || e.Type is RecordTypeSymbol
                )
                {
                    throw new ParserException("Incompatible types", c.Line, c.Position);
                }
                e = new BinOpNode(new List<Node> {e, t}, c.SubType, c.Line, c.Position)
                {
                    Type = TypeSymbol.IntTypeSymbol
                };
                c = Current;
            }
            return e;
        }

        private ExpressionNode ParseSimpleExpression(SymTable symTable)
        {
            if (Current.SubType == CharConstant)
            {
                var tmp = Current;
                Next();
                return new ConstNode(null, tmp.Value, tmp.Line, tmp.Position)
                {
                    Type = TypeSymbol.CharTypeSymbol
                };
            }
            var t = ParseTerm(symTable);
            var c = Current;
            while (AddOps.Contains(c.SubType))
            {
                Next();
                var temp = ParseTerm(symTable);
                var type = (TypeSymbol) temp.Type;
                if (
                    t.Type is ArrayTypeSymbol || t.Type is RecordTypeSymbol ||
                    temp.Type is ArrayTypeSymbol || temp.Type is RecordTypeSymbol
                )
                {
                    throw new ParserException("Incompatible types", c.Line, c.Position);
                }
                if ((TypeSymbol) t.Type == TypeSymbol.RealTypeSymbol)
                {
                    type = TypeSymbol.RealTypeSymbol;
                }
                t = new BinOpNode(new List<Node> {t, temp}, c.SubType, c.Line, c.Position)
                {
                    Type = type
                };
                c = Current;
            }
            return t;
        }

        private ExpressionNode ParseTerm(SymTable symTable)
        {
            var f = ParseFactor(symTable);
            var c = Current;
            while (MulOps.Contains(c.SubType))
            {
                Next();
                var t = ParseFactor(symTable);
                var type = (TypeSymbol) f.Type;
                if (
                    t.Type is ArrayTypeSymbol || t.Type is RecordTypeSymbol ||
                    f.Type is ArrayTypeSymbol || f.Type is RecordTypeSymbol
                )
                {
                    throw new ParserException("Incompatible types", c.Line, c.Position);
                }
                if ((TypeSymbol) t.Type == TypeSymbol.RealTypeSymbol)
                {
                    type = TypeSymbol.RealTypeSymbol;
                }
                f = new BinOpNode(new List<Node> {f, t}, c.SubType, c.Line, c.Position)
                {
                    Type = type
                };
                c = Current;
            }
            return f;
        }

        private ExpressionNode ParseFactor(SymTable symTable)
        {
            var c = Current;
            switch (c.SubType)
            {
                case Identifier:
                    Next();
                    var t = symTable.LookUp(c.Value.ToString());
                    switch (t)
                    {
                        case TypeSymbol ts:
                            return ParseCast(symTable, new IdentNode(null, c) {Type = ts});
                        case FunctionSymbol fs:
                            return ParseFunctionCall(symTable, new IdentNode(null, c) {Type = fs});
                        case ValueSymbol vs:
                            DesignatorNode v = new IdentNode(null, c)
                            {
                                Type = vs.Type
                            };
                            switch (vs.Type)
                            {
                                case RecordTypeSymbol _:
                                    return ParseMemberAccess(symTable, v);
                                case ArrayTypeSymbol _:
                                    return ParseIndex(symTable, v);
                            }
                            return v;
                        default:
                            throw new ParserException("Illegal identifier", c.Line, c.Position);
                    }
                case IntegerConstant:
                    Next();
                    return new ConstNode(null, c.Value, c.Line, c.Position)
                    {
                        Type = TypeSymbol.IntTypeSymbol
                    };
                case FloatConstant:
                    Next();
                    return new ConstNode(null, c.Value, c.Line, c.Position)
                    {
                        Type = TypeSymbol.RealTypeSymbol
                    };
                case LParenthesis:
                    Next();
                    var e = ParseExpression(symTable);
                    Require(RParenthesis);
                    return e;
                case Plus:
                case Minus:
                case Not:
                    Next();
                    var tmp = ParseFactor(symTable);
                    if (tmp.Type == TypeSymbol.IntTypeSymbol)
                    {
                        return new UnOpNode(new List<Node> {tmp}, c.SubType, c.Line, c.Position)
                        {
                            Type = tmp.Type
                        };
                    }
                    throw new ParserException("Illegal operation", c.Line, c.Position);
            }
            throw new ParserException($"Unexpected token {c.SubType}", c.Line, c.Position);
        }

        private DesignatorNode ParseFunctionCall(SymTable symTable, DesignatorNode function)
        {
            var fs = (FunctionSymbol) function.Type;
            var p = ParseActualParameters(symTable);
            CheckParameters(fs.Parameters, p);
            var f = new CallOperator(p.Childs, "Call", p.Line, p.Position)
            {
                Subprogram = fs,
                Type = fs.ReturnType
            };
            switch (fs.ReturnType)
            {
                case RecordTypeSymbol _:
                    return ParseMemberAccess(symTable, f);
                case ArrayTypeSymbol _:
                    return ParseIndex(symTable, f);
            }
            return f;
        }

        private ExpressionListNode ParseActualParameters(SymTable symTable)
        {
            if (Current.SubType != LParenthesis)
            {
                return new ExpressionListNode(new List<ExpressionNode>(), "Expression list", Current.Line,
                    Current.Position);
            }
            Next();
            if (Current.SubType == RParenthesis)
            {
                Next();
                return new ExpressionListNode(new List<ExpressionNode>(), "Expression list", Current.Line,
                    Current.Position);
            }
            var t = ParseExpressionList(symTable);
            Require(RParenthesis);
            return t;
        }

        private DesignatorNode ParseCast(SymTable symTable, DesignatorNode type)
        {
            var ts = (TypeSymbol) type.Type;
            var p = ParseCastParam(symTable);
            CheckExplicitTypeCompatibility((TypeSymbol) p.Type, ts);
            var c = new CastOperator(new List<Node> {p}, "Cast", p.Line, p.Position)
            {
                Type = ts
            };
            switch (ts)
            {
                case RecordTypeSymbol _:
                    return ParseMemberAccess(symTable, c);
                case ArrayTypeSymbol _:
                    return ParseIndex(symTable, c);
            }
            return c;
        }

        private ExpressionNode ParseCastParam(SymTable symTable)
        {
            Require(LParenthesis);
            var t = ParseExpression(symTable);
            Require(RParenthesis);
            return t;
        }

        private DesignatorNode ParseIndex(SymTable symTable, DesignatorNode array)
        {
            while (true)
            {
                if (Current.SubType != LBracket)
                {
                    return array;
                }
                var ars = (ArrayTypeSymbol) array.Type;
                var p = ParseIndexParam(symTable);
                CheckImplicitTypeCompatibility((TypeSymbol) p.Type, TypeSymbol.IntTypeSymbol);
                var i = new IndexOperator(new List<Node> {array, p}, "Index", p.Line, p.Position)
                {
                    Type = ars.ElementType
                };
                switch (ars.ElementType)
                {
                    case RecordTypeSymbol _:
                        return ParseMemberAccess(symTable, i);
                    case ArrayTypeSymbol _:
                        array = i;
                        continue;
                }
                return i;
            }
        }

        private ExpressionNode ParseIndexParam(SymTable symTable)
        {
            Require(LBracket);
            var t = ParseExpression(symTable);
            Require(RBracket);
            return t;
        }

        private DesignatorNode ParseMemberAccess(SymTable symTable, DesignatorNode record)
        {
            if (Current.SubType != Dot)
            {
                return record;
            }
            Next();
            var rt = (RecordTypeSymbol) record.Type;
            var c = Current;
            Require(Identifier);
            if (!rt.Fields.Contains(c.Value.ToString()))
                throw new ParserException("Illegal identifier", c.Line, c.Position);
            var f = new MemberAccessOperator(new List<Node> {record, new IdentNode(null, c)}, "Member access", c.Line,
                c.Position)
            {
                Type = ((VarSymbol) rt.Fields[c.Value.ToString()]).Type
            };
            switch (f.Type)
            {
                case RecordTypeSymbol _:
                    return ParseMemberAccess(symTable, f);
                case ArrayTypeSymbol _:
                    return ParseIndex(symTable, f);
            }
            return f;
        }

        private void Require(Tokenizer.TokenSubType type)
        {
            if (Current.SubType != type)
                throw new ParserException($"Expected {type}, got {Current.SubType}", Current.Line,
                    Current.Position);
            Next();
        }

        private void CheckParameters(Parameters parameters, ExpressionListNode expressionList)
        {
            if (parameters.Count != expressionList.Childs.Count)
            {
                throw new ParserException("Parameter count mismatch", Current.Line, Current.Position);
            }
            foreach (var pair in parameters.Zip(expressionList.Childs,
                (symbol, node) => (symbol.Type, ((ExpressionNode) node).Type)))
            {
                if (pair.Item1 != pair.Item2)
                {
                    throw new ParserException("Parameter type mismatch", Current.Line, Current.Position);
                }
            }
        }

        private void CheckImplicitTypeCompatibility(TypeSymbol from, TypeSymbol to)
        {
            if (
                from == to ||
                from == TypeSymbol.IntTypeSymbol && to == TypeSymbol.RealTypeSymbol ||
                from is ArrayTypeSymbol atf && to is ArrayTypeSymbol att && atf.ElementType == att.ElementType &&
                atf.Range.Begin == att.Range.Begin && atf.Range.End == att.Range.End
            )
            {
                return;
            }
            throw new ParserException("Incompatible types", Current.Line, Current.Position);
        }

        private void CheckExplicitTypeCompatibility(TypeSymbol from, TypeSymbol to)
        {
            if (
                from == to ||
                from == TypeSymbol.CharTypeSymbol &&
                (to == TypeSymbol.IntTypeSymbol || to == TypeSymbol.RealTypeSymbol) ||
                from == TypeSymbol.IntTypeSymbol && to == TypeSymbol.RealTypeSymbol ||
                from is ArrayTypeSymbol atf && to is ArrayTypeSymbol att && atf.ElementType == att.ElementType &&
                atf.Length == att.Length
            )
            {
                return;
            }
            throw new ParserException("Incompatible types", Current.Line, Current.Position);
        }

        public void Dispose()
        {
        }
    }
}