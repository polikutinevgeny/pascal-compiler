using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public partial class Parser
    {
        public class EvaluatorException : Exception
        {
            public EvaluatorException(string message) : base(message)
            {
            }
        }

        private Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>> BinaryOps { get; set; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic, dynamic>>
            {
                {Tokenizer.TokenSubType.Plus, (l, r) => l + r},
                {Tokenizer.TokenSubType.Minus, (l, r) => l - r},
                {Tokenizer.TokenSubType.Asterisk, (l, r) => l * r},
                {Tokenizer.TokenSubType.Slash, (l, r) => (double) l / (double) r},
                {Tokenizer.TokenSubType.Greater, (l, r) => Convert.ToInt32(l > r)},
                {Tokenizer.TokenSubType.Less, (l, r) => Convert.ToInt32(l < r)},
                {Tokenizer.TokenSubType.GEqual, (l, r) => Convert.ToInt32(l >= r)},
                {Tokenizer.TokenSubType.LEqual, (l, r) => Convert.ToInt32(l <= r)},
                {Tokenizer.TokenSubType.NEqual, (l, r) => Convert.ToInt32(l != r)},
                {Tokenizer.TokenSubType.Equal, (l, r) => Convert.ToInt32(l == r)},
                {
                    Tokenizer.TokenSubType.Or, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l | r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Xor, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l ^ r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Div, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l / r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Mod, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l % r;
                    }
                },
                {
                    Tokenizer.TokenSubType.And, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l & r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Shl, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l << r;
                    }
                },
                {
                    Tokenizer.TokenSubType.Shr, (l, r) =>
                    {
                        if (l is double || r is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return l >> r;
                    }
                }
            };

        private Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>> UnaryOps { get; set; } =
            new Dictionary<Tokenizer.TokenSubType, Func<dynamic, dynamic>>
            {
                {Tokenizer.TokenSubType.Plus, i => i},
                {Tokenizer.TokenSubType.Minus, i => -i},
                {
                    Tokenizer.TokenSubType.Not, i =>
                    {
                        if (i.GetType() is double)
                        {
                            throw new EvaluatorException("Illegal operation");
                        }
                        return ~i;
                    }
                }
            };

        private dynamic EvaluateConstExpr(Node expr, SymTable symTable)
        {
            switch (expr)
            {
                case ConstNode _:
                    return expr.Value;
                case DesignatorNode _:
                    var temp = symTable.LookUp(expr.Value.ToString());
                    if (temp is ConstSymbol)
                    {
                        return ((SimpleConstant) (temp as ConstSymbol).Value).Value;
                    }
                    throw new EvaluatorException("Illegal operation");
                case BinOpNode _:
                    return BinaryOps[(Tokenizer.TokenSubType) expr.Value](EvaluateConstExpr(expr.Childs[0], symTable),
                        EvaluateConstExpr(expr.Childs[1], symTable));
                case UnOpNode _:
                    return UnaryOps[(Tokenizer.TokenSubType) expr.Value](EvaluateConstExpr(expr.Childs[0], symTable));
            }
            throw new InvalidOperationException($"Node of type {expr.GetType()} met in expression");
        }
    }
}