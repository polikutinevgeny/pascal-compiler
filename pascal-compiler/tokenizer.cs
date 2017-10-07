using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PascalCompiler
{
    public partial class Tokenizer
    {
        public Tokenizer(StreamReader reader) => this.reader = reader;

        private StreamReader reader;

        public class Token
        {
            public TokenType Type { get; set; }
            public TokenSubType SubType { get; set; }
            public String SourceString { get; set; }
            public uint Line { get; set; }
            public uint Position { get; set; }

            public Token(TokenType type, TokenSubType subType, string sourceString, uint line, uint position)
            {
                Type = type;
                SubType = subType;
                SourceString = sourceString;
                Line = line;
                Position = position;
            }

            public override string ToString() => $"|{Line,-5}|{Position,-5}|{Type,-20} ({SubType, -15})|{SourceString,-30}|";
        }

        public class IntToken : Token
        {
            public ulong Value { get; set; }

            public IntToken(TokenType type, TokenSubType subType, string sourceString, uint line, uint position, ulong value) : base(type, subType, sourceString, line, position) => Value = value;

            public override string ToString() => base.ToString() + $"{Value,-30}";
        }

        public class StringToken : Token
        {
            public String Value { get; set; }

            public StringToken(TokenType type, TokenSubType subType, String sourceString, uint line, uint position, String value) : base(type, subType, sourceString, line, position) => Value = value;

            public override string ToString() => base.ToString() + $"{Value,-30}";
        }

        public class DoubleToken : Token
        {
            public double Value { get; set; }

            public DoubleToken(TokenType type, TokenSubType subType, string sourceString, uint line, uint position, double value) : base(type, subType, sourceString, line, position) => Value = value;

            public override string ToString() => base.ToString() + $"{Value,-30}";
        }

        public class TokenizerException : Exception
        {
            public uint Line { get; set; }
            public uint Position { get; set; }

            public TokenizerException(string message, uint line, uint position) : base(message)
            {
                Line = line;
                Position = position;
            }
        }

        public System.Collections.Generic.IEnumerable<Token> Tokens()
        {
            uint line = 1;
            uint pos = 0;
            State state = State.Stop;
            string current = "";
            while (!reader.EndOfStream || buffer.Count > 0)
            {
                char c = Read();
                ++pos;
                State newState;
                try
                {
                    newState = StateTable[(int)state, c];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new TokenizerException($"Unknown character: '{c}'(#{(uint)c})", line, pos);
                }
                switch (newState)
                {
                    case State.Identifier:
                        current += c;
                        break;
                    case State.Stop:
                        yield return new Token(TokenType.Identifier, TokenSubType.Identifier, current, line, pos);
                        current = "";
                        break;
                    case State.NewLine:
                        ++line;
                        pos = 0;
                        break;
                    default:
                        throw new TokenizerException($"Unexpected character: '{c}'(#{(uint)c})", line, pos);
                }
                state = newState;
            }
            yield break;
        }

        private Stack<Char> buffer = new Stack<char>();

        private char Read() => buffer.Count > 0 ? buffer.Pop() : (char)reader.Read();

        private void PushBack(char ch) => buffer.Push(ch);
    }
}
