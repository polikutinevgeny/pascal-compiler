﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PascalCompiler
{
    public partial class Tokenizer : IDisposable

    {
    public Tokenizer(StreamReader reader) => _reader = reader;

    private readonly StreamReader _reader;

    public class Token : IDisposable
    {
        public TokenType Type { get; set; }
        public TokenSubType SubType { get; set; }
        public string SourceString { get; set; }
        public uint Line { get; set; }
        public uint Position { get; set; }
        private readonly string _value;

        public virtual string GetStringValue() => _value;


        public Token(TokenType type, TokenSubType subType, string sourceString, uint line, uint position)
        {
            Type = type;
            SubType = subType;
            SourceString = sourceString;
            Line = line;
            Position = position;
        }

        public Token(
            TokenType type, TokenSubType subType, string sourceString,
            uint line, uint position, string value) :
            this(type, subType, sourceString, line, position) => _value = value;

        public override string ToString() => $"{Type}({SubType}) at {Line}:{Position}";

        public void Dispose()
        {

        }
    }

    public class IntToken : Token
    {
        private readonly ulong _value;

        public override string GetStringValue() => _value.ToString();

        public IntToken(
            TokenType type, TokenSubType subType, string sourceString,
            uint line, uint position, ulong value) :
            base(type, subType, sourceString, line, position) => _value = value;

        public override string ToString() => $"{Type}({SubType}) at {Line}:{Position} == '{_value}'";
    }

    public class DoubleToken : Token
    {
        private readonly double _value;

        public override string GetStringValue() => _value.ToString(CultureInfo.InvariantCulture);

        public DoubleToken(
            TokenType type, TokenSubType subType, string sourceString,
            uint line, uint position, double value) :
            base(type, subType, sourceString, line, position) => _value = value;

        public override string ToString() => $"{Type}({SubType}) at {Line}:{Position} == '{_value}'";

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

    public class ConvertException : Exception
    {

        public uint Position { get; set; }

        public ConvertException(string message, uint position) : base(message) => Position = position;
    }

    public class ConvertOverflowException : Exception
    {

        public uint Position { get; set; }

        public ConvertOverflowException(string message, uint position) : base(message) => Position = position;

    }

    public IEnumerator<Token> GetEnumerator()
    {
        uint line = 1;
        uint pos = 0;
        var state = State.Start;
        var lexeme = "";
        while (true)
        {
            var c = Read();
            c = c != 65535 ? c : '\0';
            ++pos;
            State newState;
            try
            {
                newState = StateTable[(int) state, c];
            }
            catch (IndexOutOfRangeException)
            {
                throw new TokenizerException($"Unknown character: '{c}'(#{(uint) c})", line, pos);
            }
            lexeme += c;
            if (newState == State.UnexpectedChar)
            {
                if (c != '\0')
                {
                    throw new TokenizerException($"Unexpected character: '{c}'(#{(uint) c})", line, pos);
                }
                throw new TokenizerException("Unexpected end of file", line, pos);
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
                        if (TokenSubTypeDict.ContainsKey(lexeme.Substring(0, lexeme.Length - 1).ToLower()))
                        {
                            yield return new Token(
                                TokenType.ReservedWord,
                                TokenSubTypeDict[lexeme.Substring(0, lexeme.Length - 1).ToLower()],
                                lexeme.Substring(0, lexeme.Length - 1),
                                line, pos - (uint) lexeme.Length + 1,
                                lexeme.Substring(0, lexeme.Length - 1).ToLower());
                        }
                        else
                        {
                            yield return new Token(
                                TokenType.Identifier,
                                TokenSubType.Identifier,
                                lexeme.Substring(0, lexeme.Length - 1),
                                line, pos - (uint) lexeme.Length + 1,
                                lexeme.Substring(0, lexeme.Length - 1).ToLower());
                        }
                        lexeme = "";
                        PushBack(c);
                        --pos;
                        break;
                    case State.FloatDot:
                        yield return new IntToken(
                            TokenType.Constant,
                            TokenSubType.IntegerConstant,
                            lexeme.Substring(0, lexeme.Length - 2),
                            line, pos + 1 - (uint) lexeme.Length,
                            Convert.ToUInt64(lexeme.Substring(0, lexeme.Length - 2)));
                        pos -= 2;
                        PushBack(c);
                        PushBack(lexeme[lexeme.Length - 2]);
                        lexeme = "";
                        break;
                    case State.FloatFrac:
                    case State.FloatExpValue:
                        var provider = new NumberFormatInfo();
                        provider.NumberDecimalSeparator = ".";
                        yield return new DoubleToken(
                            TokenType.Constant,
                            TokenSubType.FloatConstant,
                            lexeme.Substring(0, lexeme.Length - 1),
                            line, pos - (uint) lexeme.Length + 1,
                            Convert.ToDouble(lexeme.Substring(0, lexeme.Length - 1), provider));
                        PushBack(c);
                        lexeme = "";
                        --pos;
                        break;
                    case State.Integer:
                    {
                        Token temp;
                        try
                        {
                            temp = new IntToken(
                                TokenType.Constant,
                                TokenSubType.IntegerConstant,
                                lexeme.Substring(0, lexeme.Length - 1),
                                line, pos - (uint) lexeme.Length + 1,
                                Convert.ToUInt64(lexeme.Substring(0, lexeme.Length - 1)));
                        }
                        catch (OverflowException)
                        {
                            throw new TokenizerException(
                                "Integer is too big", line, pos - (uint) lexeme.Length + 1);
                        }
                        yield return temp;
                    }
                        PushBack(c);
                        lexeme = "";
                        --pos;
                        break;
                    case State.PlusOperator:
                    case State.MinusOperator:
                    case State.AsteriskOperator:
                    case State.SlashOperator:
                    case State.DotOperator:
                    case State.Less:
                    case State.More:
                    case State.Operator:
                        yield return new Token(
                            TokenType.Operator,
                            TokenSubTypeDict[lexeme.Substring(0, lexeme.Length - 1)],
                            lexeme.Substring(0, lexeme.Length - 1),
                            line, pos - (uint) lexeme.Length + 1,
                            lexeme.Substring(0, lexeme.Length - 1).ToLower());
                        lexeme = "";
                        PushBack(c);
                        --pos;
                        break;
                    case State.Colon:
                    case State.Parenthesis:
                    case State.Separator:
                        yield return new Token(
                            TokenType.Separator,
                            TokenSubTypeDict[lexeme.Substring(0, lexeme.Length - 1)],
                            lexeme.Substring(0, lexeme.Length - 1),
                            line, pos - (uint) lexeme.Length + 1,
                            lexeme.Substring(0, lexeme.Length - 1).ToLower());
                        lexeme = "";
                        PushBack(c);
                        --pos;
                        break;
                    case State.StringLiteralClosed:
                    case State.StringLiteralCharBinValue:
                    case State.StringLiteralCharHexValue:
                    case State.StringLiteralCharOctValue:
                    case State.StringLiteralCharDec:
                    {
                        Token temp;
                        try
                        {
                            temp = new Token(
                                TokenType.Constant,
                                TokenSubType.StringConstant,
                                lexeme.Substring(0, lexeme.Length - 1),
                                line, pos - (uint) lexeme.Length + 1,
                                DecodeChars(lexeme.Substring(0, lexeme.Length - 1)));
                        }
                        catch (ConvertException e)
                        {
                            throw new TokenizerException(
                                "Invalid char base (This means the FSM has failed)",
                                line, pos - (uint) lexeme.Length + e.Position + 1);
                        }
                        catch (ConvertOverflowException e)
                        {
                            throw new TokenizerException(
                                "Char value is too big",
                                line, pos - (uint) lexeme.Length + e.Position + 1);
                        }
                        yield return temp;
                    }
                        lexeme = "";
                        PushBack(c);
                        --pos;
                        break;
                    case State.BinNumberValue:
                    case State.OctNumber:
                    case State.HexNumberValue:
                    {
                        Token temp;
                        try
                        {
                            temp = new IntToken(
                                TokenType.Constant,
                                TokenSubType.IntegerConstant,
                                lexeme.Substring(0, lexeme.Length - 1),
                                line, pos - (uint) lexeme.Length + 1,
                                DecodeNumber(lexeme.Substring(0, lexeme.Length - 1)));
                        }
                        catch (ConvertException e)
                        {
                            throw new TokenizerException(
                                "Invalid integer base (This means the FSM has failed)",
                                line, pos - (uint) lexeme.Length + e.Position);
                        }
                        catch (OverflowException)
                        {
                            throw new TokenizerException(
                                "Integer is too big",
                                line, pos - (uint) lexeme.Length);
                        }
                        yield return temp;
                    }
                        lexeme = "";
                        PushBack(c);
                        --pos;
                        break;
                }
            }
            else if (
                state == State.MultilineCommentNewLine &&
                (newState == State.MultilineComment ||
                 newState == State.MultilineCommentAsterisk ||
                 newState == State.MultilineCommentEnd))
            {
                ++line;
                pos = 0;
            }
            state = newState;
            if (c == '\0')
            {
                yield return new Token(TokenType.EndOfFile, TokenSubType.EndOfFile, "", line, pos, "");
                yield break;
            }
        }
    }

    private readonly Stack<char> _buffer = new Stack<char>();

    private char Read()
    {
        return _buffer.Count > 0 ? _buffer.Pop() : (char) _reader.Read();
    }

    private void PushBack(char ch) => _buffer.Push(ch);

    private static ulong DecodeNumber(string input)
    {
        switch (input[0])
        {
            case '%':
                return Convert.ToUInt64(input.Substring(1), 2);
            case '&':
                return Convert.ToUInt64(input.Substring(1), 8);
            case '$':
                return Convert.ToUInt64(input.Substring(1), 16);
            default:
                throw new ConvertException("Illegal base of integer constant", 0);
        }
    }

    private static string DecodeChars(string input)
    {
        var output = "";
        var quoted = false;
        for (var i = 0; i < input.Length; ++i)
        {
            if (input[i] == '\'')
            {
                if (quoted)
                {
                    if (i < input.Length - 1 && input[i + 1] == '\'')
                    {
                        output += '\'';
                        ++i;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    quoted = true;
                }
            }
            else if (input[i] == '#' && !quoted)
            {
                var type = input[++i];
                var pos = i++;
                var temp = char.IsDigit(type) ? type.ToString() : "";
                for (; i < input.Length && input[i] != '\'' && input[i] != '#'; ++i)
                {
                    temp += input[i];
                }
                try
                {
                    switch (type)
                    {
                        case '%':
                            output += (char) Convert.ToInt32(temp, 2);
                            break;
                        case '&':
                            output += (char) Convert.ToInt32(temp, 8);
                            break;
                        case '$':
                            output += (char) Convert.ToInt32(temp, 16);
                            break;
                        case char c when char.IsDigit(c):
                            output += (char) Convert.ToInt32(temp);
                            break;
                        default:
                            throw new ConvertException("Illegal base of char constant", (uint) pos);
                    }
                }
                catch (OverflowException)
                {
                    throw new ConvertOverflowException("Char value is too big", (uint) i);
                }
                --i;
            }
            else
            {
                output += input[i];
            }
        }
        return output;
    }

        public void Dispose()
        {
        }
    }
}