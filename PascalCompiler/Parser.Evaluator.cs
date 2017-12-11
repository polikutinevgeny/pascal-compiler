using System;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using static PascalCompiler.Tokenizer.TokenSubType;

namespace PascalCompiler
{
    public partial class Parser
    {
        private class EvaluatorException : Exception
        {
            public EvaluatorException(string message) : base(message)
            {
            }
        }

        private static Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>> BinaryOps { get; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>>
            {
                {Plus, (l, r) => l + r},
                {Minus, (l, r) => l - r},
                {Asterisk, (l, r) => l * r},
                {Slash, (l, r) => (double) l / (double) r},
                {Greater, (l, r) => l > r ? -1 : 0},
                {Less, (l, r) => l < r ? -1 : 0},
                {GEqual, (l, r) => l >= r ? -1 : 0},
                {LEqual, (l, r) => l <= r ? -1 : 0},
                {NEqual, (l, r) => l != r ? -1 : 0},
                {Equal, (l, r) => l == r ? -1 : 0},
                {Or, (l, r) => l | r},
                {Xor, (l, r) => l ^ r},
                {Div, (l, r) => l / r},
                {Mod, (l, r) => l % r},
                {And, (l, r) => l & r},
                {Shl, (l, r) => l << r},
                {Shr, (l, r) => l >> r}
            };

        private static Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>> UnaryOps { get; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>>
            {
                {Plus, i => i},
                {Minus, i => -i},
                {Not, i => ~i}
            };

        public static dynamic EvaluateConstExpr(Node expr, SymTable symTable)
        {
            try
            {
                switch (expr)
                {
                    case ConstNode _:
                        return expr.Value;
                    case CastOperator _:
                        return EvaluateConstExpr(expr.Childs[0], symTable);
                    case DesignatorNode _:
                        var temp = symTable.LookUp(expr.Value.ToString());
                        if (temp is ConstSymbol)
                        {
                            return ((SimpleConstant)(temp as ConstSymbol).Value).Value;
                        }
                        throw new EvaluatorException("Illegal operation");
                    case BinOpNode _:
                        return BinaryOps[(Tokenizer.TokenSubType)expr.Value](EvaluateConstExpr(expr.Childs[0], symTable),
                            EvaluateConstExpr(expr.Childs[1], symTable));
                    case UnOpNode _:
                        return UnaryOps[(Tokenizer.TokenSubType)expr.Value](EvaluateConstExpr(expr.Childs[0], symTable));
                }
            }
            catch (RuntimeBinderException)
            {
                throw new ParserException("Invalid operation in constant expression", expr.Line, expr.Position);
            }
            throw new InvalidOperationException($"Node of type {expr.GetType()} met in expression");
        }
    }
}