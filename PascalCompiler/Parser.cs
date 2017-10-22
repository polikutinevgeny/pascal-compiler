using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        private enum Type
        {
            Simple,
            Record,
            Array,
        }

        private Dictionary<string, Type> TypeTable { get; set; }

        private readonly IEnumerator<Tokenizer.Token> _tokenizer;

        private void Next() => _tokenizer.MoveNext();

        private Tokenizer.Token Current => _tokenizer.Current;

        public Parser(IEnumerator<Tokenizer.Token> tokenizer)
        {
            _tokenizer = tokenizer;
            TypeTable = new Dictionary<string, Type>
            {
                {"integer", Type.Simple},
                {"real", Type.Simple},
                {"char", Type.Simple},
            };
        }

        public Node Parse()
        {
            Next();
            ProgramNode p = ParseProgram();
            Require(Tokenizer.TokenSubType.EndOfFile);
            return p;
        }

        private ProgramNode ParseProgram()
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
            var b = ParseBlock();
            Require(Tokenizer.TokenSubType.Dot);
            return new ProgramNode(new List<Node> {b}, $"Program '{name}'", c.Line, c.Position);
        }

        private BlockNode ParseBlock()
        {
            if (Current.SubType != Tokenizer.TokenSubType.Begin)
            {
                var d = ParseDeclSection();
                var c = ParseCompoundStatement();
                return new BlockNode(new List<Node> {d, c}, "Block", d.Line, d.Position);
            }
            else
            {
                var c = ParseCompoundStatement();
                return new BlockNode(new List<Node> {c}, "Block", c.Line, c.Position);
            }
        }

        private DeclSectionNode ParseDeclSection()
        {
            List<Node> declList = new List<Node>();
            var c = Current;
            while (true)
            {
                switch (Current.SubType)
                {
                    case Tokenizer.TokenSubType.Const:
                        declList.Add(ParseConstSection());
                        break;
                    case Tokenizer.TokenSubType.Var:
                        declList.Add(ParseVarSection());
                        break;
                    case Tokenizer.TokenSubType.Type:
                        declList.Add(ParseTypeSection());
                        break;
                    case Tokenizer.TokenSubType.Procedure:
                        declList.Add(ParseProcedureDecl());
                        break;
                    case Tokenizer.TokenSubType.Function:
                        declList.Add(ParseFunctionDecl());
                        break;
                    default:
                        return new DeclSectionNode(declList, "Declarations", c.Line, c.Position);
                }
            }
        }

        private ConstSectionNode ParseConstSection()
        {
            Require(Tokenizer.TokenSubType.Const);
            var c = Current;
            List<Node> declList = new List<Node>();
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                declList.Add(ParseConstantDecl());
                Require(Tokenizer.TokenSubType.Semicolon);
            }
            if (declList.Count == 0)
            {
                throw new ParserException("Empty constant section", c.Line, c.Position);
            }
            return new ConstSectionNode(declList, "Constant section", c.Line, c.Position);
        }

        private ConstantDeclNode ParseConstantDecl()
        {
            var c = Current;
            Next();
            if (Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                Next();
                var ce = ParseConstExpr();
                return new ConstantDeclNode(new List<Node> {ce}, c.Value, c.Line, c.Position);
            }
            if (Current.SubType == Tokenizer.TokenSubType.Colon)
            {
                Next();
                var t = ParseType();
                Require(Tokenizer.TokenSubType.Equal);
                var tc = ParseTypedConstant(t.Value.ToString());
                return new ConstantDeclNode(new List<Node> {t, tc}, c.Value, c.Line, c.Position);
            }
            throw new ParserException($"Expected '=' or ':', got {Current.SubType}.", Current.Line, Current.Position);
        }

        private TypedConstantNode ParseTypedConstant(string type)
        {
            var c = Current;
            if (type == "Array" || TypeTable[type] == Type.Array) // array may be anonymous
            {
                var t = ParseArrayConstant();
                return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
            }
            else if (type == "Record" || TypeTable[type] == Type.Record) // record may be anonymous
            {
                var t = ParseRecordConstant();
                return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
            }
            else if (TypeTable[type] == Type.Simple)
            {
                var t = ParseConstExpr();
                return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
            }
            throw new InvalidOperationException($"No typed constant found at {c.Line}:{c.Position}");
        }

        // TODO: Evaluate expression at compile time
        private ConstExprNode ParseConstExpr()
        {
            var e = ParseExpression();
            return new ConstExprNode(e.Childs, e.Value, e.Line, e.Position);
        }

        private ArrayConstantNode ParseArrayConstant()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            List<Node> arrayElem = new List<Node> {ParseConstExpr()};
            while (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
            {
                Require(Tokenizer.TokenSubType.Comma);
                arrayElem.Add(ParseConstExpr());
            }
            Require(Tokenizer.TokenSubType.RParenthesis);
            return new ArrayConstantNode(arrayElem, "Array constant", c.Line, c.Position);
        }

        private RecordConstantNode ParseRecordConstant()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            List<Node> recordElem = new List<Node> {ParseRecordFieldConstant()};
            while (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
            {
                Require(Tokenizer.TokenSubType.Semicolon);
                recordElem.Add(ParseRecordFieldConstant());
            }
            return new RecordConstantNode(recordElem, "Record constant", c.Line, c.Position);
        }

        private RecordFieldConstantNode ParseRecordFieldConstant()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Colon);
            var v = ParseConstExpr();
            return new RecordFieldConstantNode(new List<Node> {v}, c.Value, c.Line, c.Position);
        }

        private VarSectionNode ParseVarSection()
        {
            Require(Tokenizer.TokenSubType.Var);
            var c = Current;
            List<Node> declList = new List<Node>();
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                declList.Add(ParseVarDecl());
                Require(Tokenizer.TokenSubType.Semicolon);
            }
            if (declList.Count == 0)
            {
                throw new ParserException("Empty var section", c.Line, c.Position);
            }
            return new VarSectionNode(declList, "Var section", c.Line, c.Position);
        }

        private VarDeclNode ParseVarDecl()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            var tmp = Current;
            List<Node> childs = new List<Node> {new VarNode(null, c)};
            while (tmp.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                tmp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                childs.Add(new VarNode(null, tmp));
                tmp = Current;
            }
            Require(Tokenizer.TokenSubType.Colon);
            childs.Insert(0, ParseType());
            if (childs.Count == 1 && Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                childs.Add(ParseTypedConstant(childs[0].Value.ToString()));
                return new VarDeclNode(childs, c.Value, c.Line, c.Position);
            }
            return new VarDeclNode(childs, "Variable declaration", c.Line, c.Position);
        }

        private TypeSectionNode ParseTypeSection()
        {
            Require(Tokenizer.TokenSubType.Type);
            var c = Current;
            List<Node> declList = new List<Node>();
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                declList.Add(ParseTypeDecl());
                Require(Tokenizer.TokenSubType.Semicolon);
            }
            if (declList.Count == 0)
            {
                throw new ParserException("Empty type section", c.Line, c.Position);
            }
            return new TypeSectionNode(declList, "Type section", c.Line, c.Position);
        }

        private TypeDeclNode ParseTypeDecl()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Equal);
            var v = ParseType();
            switch (v)
            {
                case ArrayTypeNode _:
                    TypeTable.Add(c.Value.ToString(), Type.Array);
                    break;
                case RecordTypeNode _:
                    TypeTable.Add(c.Value.ToString(), Type.Record);
                    break;
                default:
                    TypeTable.Add(c.Value.ToString(), Type.Simple);
                    break;
            }
            return new TypeDeclNode(new List<Node> {v}, c.Value, c.Line, c.Position);
        }

        private TypeNode ParseType()
        {
            var c = Current;
            if (c.SubType == Tokenizer.TokenSubType.Identifier)
            {
                if (TypeTable.ContainsKey(c.Value.ToString()))
                {
                    Next();
                    return new TypeNode(null, c.Value, c.Line, c.Position);
                }
                throw new ParserException($"Identifier not found '{c.Value}'", c.Line, c.Position);
            }
            if (c.SubType == Tokenizer.TokenSubType.Array)
            {
                return ParseArrayType();
            }
            if (c.SubType == Tokenizer.TokenSubType.Record)
            {
                return ParseRecordType();
            }
            throw new ParserException($"Expected type, found '{c.SubType}'", c.Line, c.Position);
        }

        private ArrayTypeNode ParseArrayType()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Array);
            Require(Tokenizer.TokenSubType.LBracket);
            List<Node> childs = new List<Node> {ParseSubrange()};
            while (Current.SubType != Tokenizer.TokenSubType.RBracket)
            {
                Require(Tokenizer.TokenSubType.Comma);
                childs.Add(ParseSubrange());
            }
            Require(Tokenizer.TokenSubType.RBracket);
            Require(Tokenizer.TokenSubType.Of);
            childs.Insert(0, ParseType());
            return new ArrayTypeNode(childs, "Array", c.Line, c.Position);
        }

        private SubrangeNode ParseSubrange()
        {
            var c = Current;
            var l = ParseConstExpr();
            Require(Tokenizer.TokenSubType.Range);
            var r = ParseConstExpr();
            return new SubrangeNode(new List<Node> {l, r}, "Range", c.Line, c.Position);
        }

        private RecordTypeNode ParseRecordType()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Record);
            var f = ParseRecordFieldList();
            Require(Tokenizer.TokenSubType.End);
            return new RecordTypeNode(new List<Node> {f}, "Record", c.Line, c.Position);
        }

        private RecordFieldListNode ParseRecordFieldList()
        {
            var c = Current;
            List<Node> childs = new List<Node> {ParseFieldDecl()};
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                Require(Tokenizer.TokenSubType.Semicolon);
                childs.Add(ParseFieldDecl());
            }
            if (Current.SubType == Tokenizer.TokenSubType.Semicolon)
            {
                Next();
            }
            return new RecordFieldListNode(childs, "Field list", c.Line, c.Position);
        }

        private FieldDeclNode ParseFieldDecl()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node> {new FieldNode(null, c)};
            var t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                t = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                childs.Add(new FieldNode(null, t));
            }
            Require(Tokenizer.TokenSubType.Colon);
            childs.Insert(0, ParseType());
            return new FieldDeclNode(childs, "Field declaration", c.Line, c.Position);
        }

        private ProcedureDeclNode ParseProcedureDecl()
        {
            var c = Current;
            var h = ParseProcedureHeading();
            Require(Tokenizer.TokenSubType.Semicolon);
            var b = ParseBlock();
            Require(Tokenizer.TokenSubType.Semicolon);
            return new ProcedureDeclNode(new List<Node> {h, b}, "Procedure", c.Line, c.Position);
        }

        private ProcedureHeadingNode ParseProcedureHeading()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Procedure);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node>();
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                childs.Add(ParseFormalParameters());
            }
            return new ProcedureHeadingNode(childs.Count > 0 ? childs : null, $"Procedure heading: {i.Value}", c.Line,
                c.Position);
        }

        private FormalParametersNode ParseFormalParameters()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.LParenthesis);
            List<Node> parameters = new List<Node>();
            if (Current.SubType == Tokenizer.TokenSubType.Identifier ||
                Current.SubType == Tokenizer.TokenSubType.Var ||
                Current.SubType == Tokenizer.TokenSubType.Const)
            {
                parameters.Add(ParseFormalParam());
            }
            while (
                Current.SubType == Tokenizer.TokenSubType.Identifier ||
                Current.SubType == Tokenizer.TokenSubType.Var ||
                Current.SubType == Tokenizer.TokenSubType.Const)
            {
                Require(Tokenizer.TokenSubType.Semicolon);
                parameters.Add(ParseFormalParam());
            }
            Require(Tokenizer.TokenSubType.RParenthesis);
            return new FormalParametersNode(parameters.Count > 0 ? parameters : null, "Formal parameters", c.Line,
                c.Position);
        }

        private FormalParamNode ParseFormalParam()
        {
            var c = Current;
            switch (c.SubType)
            {
                case Tokenizer.TokenSubType.Var:
                    return ParseVarParameter();
                case Tokenizer.TokenSubType.Const:
                    return ParseConstParameter();
                case Tokenizer.TokenSubType.Identifier:
                    return ParseParameter();
            }
            throw new ParserException($"Expected var, const or identifier, got {c.SubType}", c.Line, c.Position);
        }

        private ParameterNode ParseParameter()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node> {new ParamNode(null, c)};
            while (Current.SubType == Tokenizer.TokenSubType.Identifier)
            {
                Require(Tokenizer.TokenSubType.Comma);
                childs.Add(new ParamNode(null, Current));
            }
            Require(Tokenizer.TokenSubType.Colon);
            if (Current.SubType == Tokenizer.TokenSubType.Array)
            {
                var p = Current;
                Require(Tokenizer.TokenSubType.Array);
                Require(Tokenizer.TokenSubType.Of);
                var tp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                childs.Insert(0,
                    new ArrayTypeNode(new List<Node> {new TypeNode(null, tp.Value, tp.Line, tp.Position)}, "Array",
                        p.Line, p.Position));
                return new ParameterNode(childs, "Parameter", c.Line, c.Position);
            }
            var t = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            childs.Insert(0, new TypeNode(null, t.Value, t.Line, t.Position));
            if (childs.Count == 1 && Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                Require(Tokenizer.TokenSubType.Equal);
                childs.Add(ParseConstExpr());
                return new ParameterNode(childs, "Parameter", c.Line, c.Position);
            }
            return new ParameterNode(childs, "Parameter", c.Line, c.Position);
        }

        private VarParameterNode ParseVarParameter()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Var);
            var tmp = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node> {new ParamNode(null, tmp)};
            while (Current.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                tmp = Current;
                Next();
                childs.Add(new ParamNode(null, tmp));
                tmp = Current;
            }
            Require(Tokenizer.TokenSubType.Colon);
            if (Current.SubType == Tokenizer.TokenSubType.Array)
            {
                var p = Current;
                Require(Tokenizer.TokenSubType.Array);
                Require(Tokenizer.TokenSubType.Of);
                var tp = Current;
                Require(Tokenizer.TokenSubType.Identifier);
                childs.Insert(0,
                    new ArrayTypeNode(new List<Node> {new TypeNode(null, tp.Value, tp.Line, tp.Position)}, "Array",
                        p.Line, p.Position));
                return new VarParameterNode(childs, " Var Parameter", c.Line, c.Position);
            }
            var t = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            childs.Insert(0, new TypeNode(null, t.Value, t.Line, t.Position));
            return new VarParameterNode(childs, "Var Parameter", c.Line, c.Position);
        }

        private ConstParameterNode ParseConstParameter()
        {
            Require(Tokenizer.TokenSubType.Const);
            var p = ParseParameter();
            return new ConstParameterNode(p.Childs, p.Value, p.Line, p.Position);
        }

        private FunctionDeclNode ParseFunctionDecl()
        {
            var c = Current;
            var h = ParseFunctionHeading();
            Require(Tokenizer.TokenSubType.Semicolon);
            var b = ParseBlock();
            Require(Tokenizer.TokenSubType.Semicolon);
            return new FunctionDeclNode(new List<Node> {h, b}, "Function", c.Line, c.Position);
        }

        private FunctionHeadingNode ParseFunctionHeading()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Function);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node>();
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                childs.Add(ParseFormalParameters());
            }
            Require(Tokenizer.TokenSubType.Colon);
            var t = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            childs.Add(new TypeNode(null, t.Value, t.Line, t.Position));
            return new FunctionHeadingNode(childs, $"Function heading: {i.Value}", c.Line,
                c.Position);
        }

        private CompoundStatementNode ParseCompoundStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Begin);
            var l = ParseStatementList();
            Require(Tokenizer.TokenSubType.End);
            return new CompoundStatementNode(new List<Node> {l}, "Compound statement", c.Line, c.Position);
        }

        private StatementListNode ParseStatementList()
        {
            var c = Current;
            if (Current.SubType == Tokenizer.TokenSubType.End)
            {
                return new StatementListNode(null, "Statement list", c.Line, c.Position);
            }
            List<Node> childs = new List<Node> {ParseStatement()};
            while (Current.SubType == Tokenizer.TokenSubType.Semicolon)
            {
                Next();
                var t = ParseStatement();
                if (t != null)
                {
                    childs.Add(t);
                }
            }
            return new StatementListNode(childs, "Statement list", c.Line, c.Position);
        }

        private StatementNode ParseStatement()
        {
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                case Tokenizer.TokenSubType.Read:
                case Tokenizer.TokenSubType.Write:
                    return ParseSimpleStatement();
                case Tokenizer.TokenSubType.While:
                case Tokenizer.TokenSubType.If:
                case Tokenizer.TokenSubType.For:
                case Tokenizer.TokenSubType.Repeat:
                case Tokenizer.TokenSubType.Begin:
                    return ParseStructStatement();
            }
            return null;
        }

        private SimpleStatementNode ParseSimpleStatement()
        {
            var c = Current;
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Read:
                    return ParseReadStatement();
                case Tokenizer.TokenSubType.Write:
                    return ParseWriteStatement();
            }
            var d = ParseDesignator();
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.LParenthesis:
                {
                    Next();
                    if (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
                    {
                        var e = ParseExpressionList();
                        Require(Tokenizer.TokenSubType.RParenthesis);
                        return new SimpleStatementNode(new List<Node> {d, e}, "Call in statement", c.Line, c.Position);
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
                    var e = ParseExpression();
                    return new SimpleStatementNode(new List<Node> {d, e}, "Assignment statement", c.Line, c.Position);
                }
            }
            return new SimpleStatementNode(new List<Node> { d }, "Call in statement", c.Line, c.Position);
        }

        private DesignatorNode ParseDesignator()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            List<Node> childs = new List<Node> {new IdentNode(null, c)};
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
                    childs.Add(ParseExpressionList());
                    Require(Tokenizer.TokenSubType.RBracket);
                }
                else
                {
                    Next();
                }
            }
            return new DesignatorNode(childs, "Designator", c.Line, c.Position);
        }

        private ExpressionListNode ParseExpressionList()
        {
            var c = Current;
            List<Node> childs = new List<Node> {ParseExpression()};
            while (Current.SubType == Tokenizer.TokenSubType.Comma)
            {
                Next();
                childs.Add(ParseExpression());
            }
            return new ExpressionListNode(childs, "Expression list", c.Line, c.Position);
        }

        private ReadStatementNode ParseReadStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Read);
            if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
            {
                Next();
                var e = ParseExpressionList();
                Require(Tokenizer.TokenSubType.RParenthesis);
                return new ReadStatementNode(new List<Node> {e}, "Read", c.Line, c.Position);
            }
            return new ReadStatementNode(null, "Read", c.Line, c.Position);
        }

        private WriteStatementNode ParseWriteStatement()
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
                    return new WriteStatementNode(new List<Node> {new StringNode(null, s)}, "Write", c.Line,
                        c.Position);
                }
                var e = ParseExpressionList();
                Require(Tokenizer.TokenSubType.RParenthesis);
                return new WriteStatementNode(new List<Node> {e}, "Write", c.Line, c.Position);
            }
            return new WriteStatementNode(null, "Write", c.Line, c.Position);
        }

        private StructStatementNode ParseStructStatement()
        {
            switch (Current.SubType)
            {
                case Tokenizer.TokenSubType.Begin:
                    return ParseCompoundStatement();
                case Tokenizer.TokenSubType.If:
                    return ParseIfStatement();
                case Tokenizer.TokenSubType.For:
                    return ParseForStatement();
                case Tokenizer.TokenSubType.While:
                    return ParseWhileStatement();
                case Tokenizer.TokenSubType.Repeat:
                    return ParseRepeatStatement();
            }
            throw new InvalidOperationException($"Current.Subtype was equal to {Current.SubType}");
        }

        private IfStatementNode ParseIfStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.If);
            var i = ParseExpression();
            Require(Tokenizer.TokenSubType.Then);
            var t = ParseStatement();
            if (Current.SubType == Tokenizer.TokenSubType.Else)
            {
                Next();
                var e = ParseStatement();
                return new IfStatementNode(new List<Node> {i, t, e}, "If", c.Line, c.Position);
            }
            return new IfStatementNode(new List<Node> {i, t}, "If", c.Line, c.Position);
        }

        private ForStatementNode ParseForStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.For);
            var i = Current;
            Require(Tokenizer.TokenSubType.Identifier);
            Require(Tokenizer.TokenSubType.Assign);
            var e = ParseExpression();
            Require(Tokenizer.TokenSubType.To);
            var u = ParseExpression();
            Require(Tokenizer.TokenSubType.Do);
            var s = ParseStatement();
            return new ForStatementNode(new List<Node> {new IdentNode(null, i), e, u, s}, "For", c.Line, c.Position);
        }

        private WhileStatementNode ParseWhileStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.While);
            var e = ParseExpression();
            Require(Tokenizer.TokenSubType.Do);
            var s = ParseStatement();
            return new WhileStatementNode(new List<Node> {e, s}, "While", c.Line, c.Position);
        }

        private RepeatStatementNode ParseRepeatStatement()
        {
            var c = Current;
            Require(Tokenizer.TokenSubType.Repeat);
            var s = ParseStatementList();
            Require(Tokenizer.TokenSubType.Until);
            var e = ParseExpression();
            return new RepeatStatementNode(new List<Node> {s, e}, "Repeat", c.Line, c.Position);
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

        private Node ParseExpression()
        {
            Node e = ParseSimpleExpression();
            var c = Current;
            while (RelOps.Contains(c.SubType))
            {
                Next();
                e = new BinOpNode(new List<Node> {e, ParseSimpleExpression()}, c.Value, c.Line, c.Position);
                c = Current;
            }
            return e;
        }

        private Node ParseSimpleExpression()
        {
            Node t = ParseTerm();
            var c = Current;
            while (AddOps.Contains(c.SubType))
            {
                Next();
                t = new BinOpNode(new List<Node> {t, ParseTerm()}, c.Value, c.Line, c.Position);
                c = Current;
            }
            return t;
        }

        private Node ParseTerm()
        {
            Node f = ParseFactor();
            var c = Current;
            while (MulOps.Contains(c.SubType))
            {
                Next();
                f = new BinOpNode(new List<Node> {f, ParseFactor()}, c.Value, c.Line, c.Position);
                c = Current;
            }
            return f;
        }

        private Node ParseFactor()
        {
            var c = Current;
            switch (c.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                    var d = ParseDesignator();
                    if (Current.SubType == Tokenizer.TokenSubType.LParenthesis)
                    {
                        Next();
                        if (Current.SubType != Tokenizer.TokenSubType.RParenthesis)
                        {
                            var ex = ParseExpressionList();
                            Require(Tokenizer.TokenSubType.RParenthesis);
                            return new CallNode(new List<Node> {d, ex}, "Call in expression", c.Line, c.Position);
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
                    Node e = ParseExpression();
                    Require(Tokenizer.TokenSubType.RParenthesis);
                    return e;
                case Tokenizer.TokenSubType.Plus:
                case Tokenizer.TokenSubType.Minus:
                case Tokenizer.TokenSubType.Not:
                    Next();
                    return new UnOpNode(new List<Node> {ParseFactor()}, c.Value, c.Line, c.Position);
            }
            throw new ParserException($"Unexpected token {c.SubType}", c.Line, c.Position);
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