using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        private int CycleCounter { get; set; } = 0;

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
                    if (CycleCounter == 0)
                    {
                        throw new ParserException("Break outside of cycle", c.Line, c.Position);
                    }
                    Next();
                    return new BreakNode(null, c);
                case Tokenizer.TokenSubType.Continue:
                    if (CycleCounter == 0)
                    {
                        throw new ParserException("Continue outside of cycle", c.Line, c.Position);
                    }
                    Next();
                    return new ContinueNode(null, c);
                case Tokenizer.TokenSubType.Exit:
                    return ParseExitStatement(symTable);
            }
            return null;
        }

        private Statement ParseExitStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Exit);
            if (Current.SubType != Tokenizer.TokenSubType.LParenthesis)
            {
                return new ExitNode(null, c.Value, c.Line, c.Position);
            }
            Next();
            if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
            {
                Next();
                return new ExitNode(null, c.Value, c.Line, c.Position);
            }
            if (CurrentReturnType == null)
            {
                throw new ParserException("Procedures cannot return a value", Current.Line, Current.Position);
            }
            var e = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.RParenthesis);
            CheckImplicitTypeCompatibility((TypeSymbol)e.Type, CurrentReturnType);
            return new ExitNode(new List<Node> {e}, c.Value, c.Line, c.Position);
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
            if (d is CallOperator co)
            {
                return new CallStatementWrapper(co);
            }
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Assign:
                case Tokenizer.TokenSubType.AsteriskAssign:
                case Tokenizer.TokenSubType.SlashAssign:
                case Tokenizer.TokenSubType.PlusAssign:
                case Tokenizer.TokenSubType.MinusAssign:
                {
                    Next();
                    var e = ParseExpression(symTable);
                    CheckImplicitTypeCompatibility((TypeSymbol)e.Type, (TypeSymbol)d.Type);
                    return new SimpleStatementNode(new List<Node> {d, e}, "Assignment statement", c.Line, c.Position);
                }
            }
            throw new ParserException("Illegal statement", c.Line, c.Position);
        }

        private DesignatorNode ParseDesignator(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            var t = symTable.LookUp(c.Value.ToString());
            switch (t)
            {
                case TypeSymbol ts:
                    return ParseCast(symTable, new IdentNode(null, c) {Type = ts});
                case ProcedureSymbol ps:
                    return ParseProcedureCall(symTable, new IdentNode(null, c) {Type = ps});
                case FunctionSymbol fs:
                    return ParseFunctionCall(symTable, new IdentNode(null, c) { Type = fs});
                case ValueSymbol vs:
                    DesignatorNode v = new IdentNode(null, c)
                    {
                        Type = vs.Type
                    };
                    switch (vs.Type)
                    {
                        case RecordTypeSymbol rt:
                            return ParseMemberAccess(symTable, v);
                        case ArrayTypeSymbol at:
                            return ParseIndex(symTable, v);
                    }
                    return v;
                default:
                    throw new ParserException("Illegal identifier", c.Line, c.Position);
            }
        }

        private DesignatorNode ParseProcedureCall(SymTable symTable, DesignatorNode procedure)
        {
            var ps = (ProcedureSymbol)procedure.Type;
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
            List<ExpressionNode> childs = new List<ExpressionNode> {ParseExpression(symTable)};
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
                return new ReadStatementNode(new List<Node> {e}, "Read", c.Line, c.Position);
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
                    return new WriteStatementNode(new List<Node> {new StringNode(null, s)}, "Write", c.Line,
                        c.Position);
                }
                var e = ParseExpressionList(symTable);
                Require(Tokenizer.TokenSubType.RParenthesis);
                return new WriteStatementNode(new List<Node> {e}, "Write", c.Line, c.Position);
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
                return new IfStatementNode(new List<Node> {i, t, e}, "If", c.Line, c.Position);
            }
            return new IfStatementNode(new List<Node> {i, t}, "If", c.Line, c.Position);
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
            ++CycleCounter;
            var s = ParseStatement(symTable);
            --CycleCounter;
            return new ForStatementNode(new List<Node> {new IdentNode(null, i), e, u, s}, "For", c.Line, c.Position);
        }

        private WhileStatementNode ParseWhileStatement(SymTable symTable)
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.While);
            var e = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.Do);
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
            Require(Tokenizer.TokenSubType.Repeat);
            ++CycleCounter;
            var s = ParseStatementList(symTable);
            --CycleCounter;
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

        private ExpressionNode ParseExpression(SymTable symTable)
        {
            ExpressionNode e = ParseSimpleExpression(symTable);
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
            if (Current.SubType == Tokenizer.TokenSubType.CharConstant)
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
                TypeSymbol type = (TypeSymbol)temp.Type;
                if (
                    t.Type is ArrayTypeSymbol || t.Type is RecordTypeSymbol ||
                    temp.Type is ArrayTypeSymbol || temp.Type is RecordTypeSymbol
                )
                {
                    throw new ParserException("Incompatible types", c.Line, c.Position);
                }
                if ((TypeSymbol)t.Type == TypeSymbol.RealTypeSymbol)
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
                TypeSymbol type = (TypeSymbol) f.Type;
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
                case Tokenizer.TokenSubType.Identifier:
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
                                case RecordTypeSymbol rt:
                                    return ParseMemberAccess(symTable, v);
                                case ArrayTypeSymbol at:
                                    return ParseIndex(symTable, v);
                            }
                            return v;
                        default:
                            throw new ParserException("Illegal identifier", c.Line, c.Position);
                    }
                case Tokenizer.TokenSubType.IntegerConstant:
                    Next();
                    return new ConstNode(null, c.Value, c.Line, c.Position)
                    {
                        Type = TypeSymbol.IntTypeSymbol
                    };
                case Tokenizer.TokenSubType.FloatConstant:
                    Next();
                    return new ConstNode(null, c.Value, c.Line, c.Position)
                    {
                        Type = TypeSymbol.RealTypeSymbol
                    };
                case Tokenizer.TokenSubType.LParenthesis:
                    Next();
                    var e = ParseExpression(symTable);
                    Require(Tokenizer.TokenSubType.RParenthesis);
                    return e;
                case Tokenizer.TokenSubType.Plus:
                case Tokenizer.TokenSubType.Minus:
                case Tokenizer.TokenSubType.Not:
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
                Type = fs.ReturnType,
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
            if (Current.SubType != Tokenizer.TokenSubType.LParenthesis)
            {
                return new ExpressionListNode(new List<ExpressionNode>(), "Expression list", Current.Line,
                    Current.Position);
            }
            Next();
            if (Current.SubType == Tokenizer.TokenSubType.RParenthesis)
            {
                Next();
                return new ExpressionListNode(new List<ExpressionNode>(), "Expression list", Current.Line,
                    Current.Position);
            }
            var t = ParseExpressionList(symTable);
            Require(Tokenizer.TokenSubType.RParenthesis);
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
            Require(Tokenizer.TokenSubType.LParenthesis);
            var t = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.RParenthesis);
            return t;
        }

        private DesignatorNode ParseIndex(SymTable symTable, DesignatorNode array)
        {
            if (Current.SubType != Tokenizer.TokenSubType.LBracket)
            {
                return array;
            }
            var ars = (ArrayTypeSymbol) array.Type;
            var p = ParseIndexParam(symTable);
            CheckImplicitTypeCompatibility((TypeSymbol) p.Type, TypeSymbol.IntTypeSymbol);
            var i = new IndexOperator(new List<Node> {array, p}, "Index", p.Line, p.Position)
            {
                Type = ars.ElementType,
            };
            switch (ars.ElementType)
            {
                case RecordTypeSymbol _:
                    return ParseMemberAccess(symTable, i);
                case ArrayTypeSymbol _:
                    return ParseIndex(symTable, i);
            }
            return i;
        }

        private ExpressionNode ParseIndexParam(SymTable symTable)
        {
            Require(Tokenizer.TokenSubType.LBracket);
            var t = ParseExpression(symTable);
            Require(Tokenizer.TokenSubType.RBracket);
            return t;
        }

        private DesignatorNode ParseMemberAccess(SymTable symTable, DesignatorNode record)
        {
            if (Current.SubType != Tokenizer.TokenSubType.Dot)
            {
                return record;
            }
            Next();
            var rt = (RecordTypeSymbol) record.Type;
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            if (rt.Fields.Contains(c.Value.ToString()))
            {
                var f = new MemberAccessOperator(new List<Node> {record, new IdentNode(null, c)}, "Member access", c.Line, c.Position)
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
            throw new ParserException("Illegal identifier", c.Line, c.Position);
        }

        private void Require(Tokenizer.TokenSubType type)
        {
            if (Current.SubType != type)
                throw new ParserException($"Expected {type}, got {Current.SubType}", Current.Line,
                    Current.Position);
            Next();
        }

        private void RequireType(ValueSymbol valueSymbol, TypeSymbol typeSymbol)
        {
            if (valueSymbol.Type != typeSymbol)
            {
                throw new ParserException("Type mismatch", Current.Line, Current.Position);
            }
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

    public static class Extension
    {
        public static IEnumerable<T> ConcatItems<T>(this IEnumerable<T> source, params T[] items)
        {
            return source.Concat(items);
        }
    }
}