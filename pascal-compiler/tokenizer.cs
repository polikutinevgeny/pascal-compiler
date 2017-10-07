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

        public enum TokenType
        {
            Identifier,
            Integer,
            Float,
            String,
            Operator,
            Separator,
            ReservedWord,
            Directive,
        }

        public enum TokenSubType
        {
            None,
            And,
            Array,
            Begin,
            Case,
            Const,
            Div,
            Do,
            Downto,
            Else,
            End,
            File,
            For,
            Function,
            Goto,
            If,
            In,
            Label,
            Mod,
            Nil,
            Not,
            Of,
            Or,
            Packed,
            Procedure,
            Program,
            Record,
            Repeat,
            Set,
            Then,
            To,
            Type,
            Until,
            Var,
            While,
            With,
            Forward,
            Plus,
            Minus,
            Asterisk,
            Slash,
            Equal,
            Less,
            Greater,
            LBracket,
            RBracket,
            Dot,
            Comma,
            Colon,
            Semicolon,
            Caret,
            LParenthesis,
            RParenthesis,
            NEqual,
            EqLess,
            EqGreater,
            Assign,
            Range,
        }

        public class Token
        {
            public TokenType Type { get; set; }
            public String SourceString { get; set; }
            public uint Line { get; set; }
            public uint Position { get; set; }

            public Token(TokenType type, string sourceString, uint line, uint position)
            {
                Type = type;
                SourceString = sourceString;
                Line = line;
                Position = position;
            }

            public override string ToString() => $"|{Line,-5}|{Position,-5}|{Type,-20}|{SourceString,-30}|";
        }

        public class IntToken : Token
        {
            public ulong Value { get; set; }

            public IntToken(TokenType type, string sourceString, uint line, uint position, ulong value) : base(type, sourceString, line, position) => Value = value;

            public override string ToString() => $"|{Line,-5}|{Position,-5}|{Type,-20}|{SourceString,-30}|{Value,-30}";
        }

        public class StringToken : Token
        {
            public String Value { get; set; }

            public StringToken(TokenType type, String sourceString, uint line, uint position, String value) : base(type, sourceString, line, position) => Value = value;

            public override string ToString() => $"|{Line,-5}|{Position,-5}|{Type,-20}|{SourceString,-30}|{Value,-30}";
        }

        public class DoubleToken : Token
        {
            public double Value { get; set; }

            public DoubleToken(TokenType type, string sourceString, uint line, uint position, double value) : base(type, sourceString, line, position) => Value = value;

            public override string ToString() => $"|{Line,-5}|{Position,-5}|{Type,-20}|{SourceString,-30}|{Value,-30}";
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
                        yield return new Token(TokenType.Identifier, current, line, pos);
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
