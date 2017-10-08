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
            State state = State.Start;
            string lexeme = "";
            while (true)
            {
                char c = Read();
                if (c == '\0')
                {
                    yield break;
                }
                c = c != 65535 ? c : '\0';
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
                lexeme += c;
                if (newState == State.UnexpectedChar)
                {
                    throw new TokenizerException($"Unknown character: '{c}'(#{(uint)c})", line, pos);
                }
                if (newState == State.Start)
                {
                    switch (state)
                    {
                        case State.Comment:
                        case State.MultilineCommentEnd:
                            PushBack(c);
                            --pos;
                            lexeme = "";
                            break;
                        case State.Start:
                            lexeme = "";
                            break;
                        case State.NewLine:
                            ++line;
                            pos = 0;
                            lexeme = "";
                            PushBack(c);
                            break;
                        case State.Identifier:
                            yield return new Token(TokenType.Identifier, TokenSubType.Identifier, lexeme.Substring(0, lexeme.Length - 1), line, pos);
                            lexeme = "";
                            PushBack(c);
                            --pos;
                            break;
                        case State.FloatDot:
                            pos -= 2;
                            yield return new IntToken(TokenType.Constant, TokenSubType.IntegerConstant, lexeme.Substring(0, lexeme.Length - 2), line, pos + 1, Convert.ToUInt64(lexeme.Substring(0, lexeme.Length - 2)));
                            PushBack(c);
                            PushBack(lexeme[lexeme.Length - 2]);
                            lexeme = "";
                            break;
                        case State.Integer:
                            yield return new IntToken(TokenType.Constant, TokenSubType.IntegerConstant, lexeme.Substring(0, lexeme.Length - 1), line, pos, Convert.ToUInt64(lexeme.Substring(0, lexeme.Length - 1)));
                            PushBack(c);
                            lexeme = "";
                            --pos;
                            break;
                        case State.Operator:
                            yield return new Token(TokenType.Operator, TokenSubType.Operator, lexeme.Substring(0, lexeme.Length - 1), line, pos);
                            lexeme = "";
                            PushBack(c);
                            --pos;
                            break;
                        case State.Separator:
                            yield return new Token(TokenType.Separator, TokenSubType.Operator, lexeme.Substring(0, lexeme.Length - 1), line, pos);
                            lexeme = "";
                            PushBack(c);
                            --pos;
                            break;
                    }
                }
                else if (state == State.MultilineCommentNewLine && (newState == State.MultilineComment || newState == State.MultilineCommentAsterisk || newState == State.MultilineCommentEnd))
                {
                    ++line;
                    pos = 0;
                }
                state = newState;
            }
        }

        private Stack<Char> buffer = new Stack<char>();

        private char Read()
        {
            return buffer.Count > 0 ? buffer.Pop() : (char)reader.Read();
        }

        private void PushBack(char ch) => buffer.Push(ch);
    }
}
