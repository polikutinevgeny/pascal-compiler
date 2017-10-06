using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pascal_compiler
{
    public static class Tokenizer
    {
        public enum TokenType
        {
            End,
        }

        public class Token
        {
            public TokenType Type;
            public String SourceString;

            public Token(TokenType type, string sourceString)
            {
                Type = type;
                SourceString = sourceString;
            }
        }

        public class IntToken : Token
        {
            public long Value;

            public IntToken(TokenType type, string sourceString, long value) : base(type, sourceString)
            {
                Value = value;
            }
        }

        public class UIntToken : Token
        {
            public ulong Value;

            public UIntToken(TokenType type, string sourceString, ulong value) : base(type, sourceString)
            {
                Value = value;
            }
        }

        public class StringToken : Token
        {
            public String Value;

            public StringToken(TokenType type, String sourceString, String value) : base(type, sourceString)
            {
                Value = value;
            }
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

        static public System.Collections.Generic.IEnumerable<Token> Tokens(StreamReader stream)
        {
            uint line = 1;
            uint pos = 1;
            while (!stream.EndOfStream)
            {
                char c = (char)stream.Peek();
                switch (c)
                {
                    default:
                        throw new TokenizerException("Unknown character", line, pos);
                }
            }
            yield break;
        }
    }
}
