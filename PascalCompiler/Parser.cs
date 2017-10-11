using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Parser(IEnumerator<Tokenizer.Token> tokenizer) => this._tokenizer = tokenizer;

        public Node Parse()
        {
            Next();
            var e = ParseExpr();
            if (Current.SubType != Tokenizer.TokenSubType.EndOfFile)
            {
                throw new ParserException("Parsing finished, tokens still left", Current.Line, Current.Position);
            }
            return e;
        }

        private ExprNode ParseExpr()
        {
            var e = ParseTerm();
            var t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Plus || t.SubType == Tokenizer.TokenSubType.Minus)
            {
                Next();
                e = new BinOpNode(new List<Node>() {e, ParseTerm()}, t);
                t = Current;
            }
            return e;
        }

        private ExprNode ParseTerm()
        {
            var e = ParseFactor();
            var t = Current;
            while (t.SubType == Tokenizer.TokenSubType.Asterisk || t.SubType == Tokenizer.TokenSubType.Slash)
            {
                Next();
                e = new BinOpNode(new List<Node>() {e, ParseFactor()}, t);
                t = Current;
            }
            return e;
        }

        private ExprNode ParseFactor()
        {
            var t = Current;
            Next();
            switch (t.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                    return new VarNode(null, t);
                case Tokenizer.TokenSubType.IntegerConstant:
                case Tokenizer.TokenSubType.FloatConstant:
                    return new ConstNode(null, t);
                case Tokenizer.TokenSubType.LParenthesis:
                    var e = ParseExpr();
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