using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler
{
    public static class Parser
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

        public static Node Parse(IEnumerator<Tokenizer.Token> tokens)
        {
            tokens.MoveNext();
            var e = ParseExpr(tokens);
            if (tokens.MoveNext())
            {
                throw new ParserException("Parsing finished, tokens still left", tokens.Current.Line,
                    tokens.Current.Position);
            }
            return e;
        }

        private static ExprNode ParseExpr(IEnumerator<Tokenizer.Token> tokens)
        {
            var e = ParseTerm(tokens);
            var t = tokens.Current;
            while (t.SubType == Tokenizer.TokenSubType.Plus || t.SubType == Tokenizer.TokenSubType.Minus)
            {
                tokens.MoveNext();
                e = new BinOpNode(new List<Node>() { e, ParseTerm(tokens) }, t);
                t = tokens.Current;
            }
            return e;
        }

        private static ExprNode ParseTerm(IEnumerator<Tokenizer.Token> tokens)
        {
            var e = ParseFactor(tokens);
            var t = tokens.Current;
            while (t.SubType == Tokenizer.TokenSubType.Asterisk || t.SubType == Tokenizer.TokenSubType.Slash)
            {
                tokens.MoveNext();
                e = new BinOpNode(new List<Node>() {e, ParseFactor(tokens)}, t);
                t = tokens.Current;
            }
            return e;
        }

        private static ExprNode ParseFactor(IEnumerator<Tokenizer.Token> tokens)
        {
            var t = tokens.Current;
            tokens.MoveNext();
            switch (t.SubType)
            {
                case Tokenizer.TokenSubType.Identifier:
                    return new VarNode(null, t);
                case Tokenizer.TokenSubType.IntegerConstant:
                case Tokenizer.TokenSubType.FloatConstant:
                    return new ConstNode(null, t);
                case Tokenizer.TokenSubType.LParenthesis:
                    var e = ParseExpr(tokens);
                    Require(tokens, Tokenizer.TokenSubType.RParenthesis);
                    tokens.MoveNext();
                    return e;
            }
            throw new ParserException($"Expected identifier, constant or expression, got {t.SubType}", t.Line,
                t.Position);
        }

        private static void Require(IEnumerator<Tokenizer.Token> tokens, Tokenizer.TokenSubType type)
        {
            if (tokens.Current.SubType != type)
                throw new ParserException($"Expected {type}, got {tokens.Current.SubType}", tokens.Current.Line,
                    tokens.Current.Position);
            tokens.MoveNext();
            return;
        }
    }
}