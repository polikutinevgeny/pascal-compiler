using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

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
                {"float", Type.Simple},
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
            switch (TypeTable[type])
            {
                case Type.Simple:
                {
                    var t = ParseConstExpr();
                    return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
                }
                case Type.Array:
                {
                    var t = ParseArrayConstant();
                    return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
                }
                case Type.Record:
                {
                    var t = ParseRecordConstant();
                    return new TypedConstantNode(new List<Node> {t}, t.Value, c.Line, c.Position);
                }
            }
            throw new InvalidOperationException($"No typed constant found at {c.Line}:{c.Position}");
        }

        // TODO: Evaluate expression at compile time
        private ConstExprNode ParseConstExpr()
        {
            var e = ParseExpression();
            return new ConstExprNode(new List<Node> {e}, e.Value, e.Line, e.Position);
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
            Require(Tokenizer.TokenSubType.Colon);
            var t = ParseType();
            if (Current.SubType == Tokenizer.TokenSubType.Equal)
            {
                var tc = ParseTypedConstant(t.Value.ToString());
                return new VarDeclNode(new List<Node> {t, tc}, c.Value, c.Line, c.Position);
            }
            return new VarDeclNode(new List<Node> {t}, c.Value, c.Line, c.Position);
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
                    TypeTable.Add(v.Value.ToString(), Type.Array);
                    break;
                case RecordTypeNode _:
                    TypeTable.Add(v.Value.ToString(), Type.Record);
                    break;
                default:
                    TypeTable.Add(v.Value.ToString(), Type.Simple);
                    break;
            }
            return new TypeDeclNode(new List<Node> { v }, c.Value, c.Line, c.Position);
        }

        private TypeNode ParseType()
        {
            var c = Current;
            if (c.SubType == Tokenizer.TokenSubType.Identifier)
            {
                if (TypeTable.ContainsKey(c.Value.ToString()))
                {
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
            return null;
        }

        private RecordTypeNode ParseRecordType()
        {
            return null;
        }

        private ProcedureDeclNode ParseProcedureDecl()
        {
            return null;
        }

        private FunctionDeclNode ParseFunctionDecl()
        {
            return null;
        }

        private CompoundStatementNode ParseCompoundStatement()
        {
            return null;
        }

        private ExpressionNode ParseExpression()
        {
            return null;
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