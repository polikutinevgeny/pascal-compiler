using System;
using System.Collections.Generic;

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

        private readonly IEnumerator<Tokenizer.Token> _tokenizer;

        private void Next() => _tokenizer.MoveNext();

        private Tokenizer.Token Current => _tokenizer.Current;

        public Parser(IEnumerator<Tokenizer.Token> tokenizer) => _tokenizer = tokenizer;

        public Node Parse()
        {
            Next();
            ExprNode e = ParseExpr();
            Require(Tokenizer.TokenSubType.EndOfFile);
            return e;
        }

        private ProgramNode ParseProgram()
        {
            Tokenizer.Token t = Current;
            if (t.SubType == Tokenizer.TokenSubType.Program)
            {
                Next();
                Tokenizer.Token n = Current;
                if (n.SubType == Tokenizer.TokenSubType.Identifier)
                {
                    Require(Tokenizer.TokenSubType.Semicolon);
                    var b = ParseBlock();
                    Require(Tokenizer.TokenSubType.Dot);
                    return new ProgramNode(new List<Node> {b}, t, n);
                }
                throw new ParserException($"Expected identifier, got {n.SubType}", n.Line, n.Position);
            }
            throw new ParserException($"Expected program, got {t.SubType}", t.Line, t.Position);
        }

        private BlockNode ParseBlock()
        {
            var decl = ParseDeclarationPart();
            var stat = ParseStatementPart();
            return new BlockNode(new List<Node>{decl, stat}, null);
        }

        private DeclarationPartNode ParseDeclarationPart()
        {
            var t = Current;
            List<Node> declList = new List<Node>();
            while (true)
            {
                switch (t.SubType)
                {
                    case Tokenizer.TokenSubType.Const:
                        declList.Add(ParseConstDecl());
                        break;
                    case Tokenizer.TokenSubType.Var:
                        declList.Add(ParseVarDecl());
                        break;
                    case Tokenizer.TokenSubType.Type:
                        declList.Add(ParseTypeDecl());
                        break;
                    case Tokenizer.TokenSubType.Procedure:
                        declList.Add(ParseProcDecl());
                        break;
                    case Tokenizer.TokenSubType.Function:
                        declList.Add(ParseFuncDecl());
                        break;
                    default:
                        return new DeclarationPartNode(declList.Count > 0 ? declList : null, null);
                }
            }
        }

        private ConstDeclNode ParseConstDecl()
        {
            var t = Current;

            return null;
        }

        private VarDeclNode ParseVarDecl()
        {
            return null;
        }

        private TypeDeclNode ParseTypeDecl()
        {
            return null;
        }

        private ProcDeclNode ParseProcDecl()
        {
            return null;
        }

        private FuncDeclNode ParseFuncDecl()
        {
            return null;
        }

        private StatementPartNode ParseStatementPart()
        {
            return null;
        }

        private ExprNode ParseExpr()
        {
            ExprNode e = ParseTerm();
            Tokenizer.Token t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Plus || t.SubType == Tokenizer.TokenSubType.Minus)
            {
                Next();
                e = new BinOpNode(new List<Node> {e, ParseTerm()}, t);
                t = Current;
            }
            return e;
        }

        private ExprNode ParseTerm()
        {
            ExprNode e = ParseFactor();
            Tokenizer.Token t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Asterisk || t.SubType == Tokenizer.TokenSubType.Slash)
            {
                Next();
                e = new BinOpNode(new List<Node> {e, ParseFactor()}, t);
                t = Current;
            }
            return e;
        }

        private ExprNode ParseFactor()
        {
            Tokenizer.Token t = Current;
            Next();
            switch (t.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                    return new VarNode(null, t);
                case Tokenizer.TokenSubType.IntegerConstant:
                case Tokenizer.TokenSubType.FloatConstant:
                    return new ConstNode(null, t);
                case Tokenizer.TokenSubType.LParenthesis:
                    ExprNode e = ParseExpr();
                    Require(Tokenizer.TokenSubType.RParenthesis);
                    return e;
                default:
                    throw new ParserException($"Expected identifier, constant or expression, got {t.SubType}", t.Line,
                        t.Position);
            }
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