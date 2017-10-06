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
            public TokenType Type;
            public String SourceString;
            public uint Line;
            public uint Position;

            public Token(TokenType type, string sourceString, uint line, uint position)
            {
                Type = type;
                SourceString = sourceString;
                Line = line;
                Position = position;
            }
        }

        public class IntToken : Token
        {
            public ulong Value;

            public IntToken(TokenType type, string sourceString, uint line, uint position, ulong value) : base(type, sourceString, line, position) => Value = value;
        }

        public class StringToken : Token
        {
            public String Value;

            public StringToken(TokenType type, String sourceString, uint line, uint position, String value) : base(type, sourceString, line, position) => Value = value;
        }

        public class TokenizerException : Exception
        {
            public uint Line;
            public uint Position;

            public TokenizerException(string message, uint line, uint position) : base(message)
            {
                Line = line;
                Position = position;
            }
        }

        public System.Collections.Generic.IEnumerable<Token> Tokens()
        {
            uint line = 1;
            uint pos = 1;
            State state = State.Stop;
            while (!reader.EndOfStream && buffer.Count > 0)
            {
                char c = Read();
                ++pos;
                try
                {
                    state = StateTable[(int)state, c];
                }
                catch(IndexOutOfRangeException)
                {
                    throw new TokenizerException(String.Format("Unknown character: '{1}'", c), line, pos);
                }
                switch(state)
                {
                    default:
                        throw new TokenizerException(String.Format("Unexpected character: '{1}'", c), line, pos);
                }
            }
            yield break;
        }

        private Stack<Char> buffer = new Stack<char>();

        private char Read() => buffer.Count > 0 ? buffer.Pop() : (char)reader.Read();

        private void PushBack(char ch) => buffer.Push(ch);
    }
}
