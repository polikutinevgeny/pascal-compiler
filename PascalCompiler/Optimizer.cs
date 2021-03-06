﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PascalCompiler
{
    public static class Optimizer
    {
        private static Dictionary<Type, TypeSymbol> TypeDictionary { get; } =
            new Dictionary<Type, TypeSymbol>
            {
                {typeof(int), TypeSymbol.IntTypeSymbol},
                {typeof(double), TypeSymbol.RealTypeSymbol},
                {typeof(char), TypeSymbol.CharTypeSymbol},
            };

        public static void Optimize(PascalProgram program)
        {
            OptimizeBlock(program.Block);
        }

        private static List<Node> OptimizeStatements(List<Node> list, SymTable symTable)
        {
            for (var i = 0; i < list.Count; i++)
            {
                FindAndOptimizeExpressions(list[i], symTable);
                switch (list[i])
                {
                    case IfStatementNode ifs:
                        OptimizeStatements(ifs.Childs, symTable);
                        list[i] = OptimizeIf(ifs, symTable);
                        break;
                    case RepeatStatementNode rs:
                        OptimizeStatements(rs.Childs, symTable);
                        list[i] = OptimizeRepeat(rs, symTable);
                        break;
                    case WhileStatementNode ws:
                        OptimizeStatements(ws.Childs, symTable);
                        list[i] = OptimizeWhile(ws, symTable);
                        break;
                    case ForStatementNode fs:
                        OptimizeStatements(fs.Childs, symTable);
                        list[i] = OptimizeFor(fs, symTable);
                        break;
                    case CompoundStatementNode csn:
                        csn.Statements = OptimizeStatements(csn.Statements, symTable);
                        break;
                }
            }
            return list;
        }

        private static List<Statement> OptimizeStatements(
            List<Statement> statements, SymTable symTable)
        {
            return OptimizeStatements(statements.Cast<Node>().ToList(), symTable).
                Cast<Statement>().
                ToList();
        }

        private static void OptimizeBlock(Block block)
        {
            block.StatementList = OptimizeStatements(block.StatementList, block.SymTable);
            foreach (Symbol sym in block.SymTable.Values)
            {
                if (sym is SubprogramSymbol ss)
                {
                    OptimizeBlock(ss.Block);
                }
            }
        }

        private static StructStatementNode OptimizeIf(
            IfStatementNode ifNode, SymTable symTable)
        {
            if (!(ifNode.Childs[0] is ConstNode cn)) return ifNode;
            return new CompoundStatementNode(
                null, "Compound statement", ifNode.Line, ifNode.Position)
            {
                Statements = new List<Statement>
                {
                    (Statement) ((int) cn.Value == 0 ? ifNode.Childs[2]
                        : ifNode.Childs[1])
                }
            };
        }

        private static StructStatementNode OptimizeRepeat(
            RepeatStatementNode repeatNode, SymTable symTable)
        {
            if (!(repeatNode.Condition is ConstNode cn)) return repeatNode;
            if ((int) cn.Value == -1)
            {
                return new CompoundStatementNode(
                    null, "Compound statement", repeatNode.Line,
                    repeatNode.Position)
                {
                    Statements = repeatNode.Childs.Cast<Statement>().ToList()
                };
            }
            return repeatNode;
        }

        private static StructStatementNode OptimizeWhile(
            WhileStatementNode whileNode, SymTable symTable)
        {
            if (!(whileNode.Condition is ConstNode cn)) return whileNode;
            if ((int) cn.Value == 0)
            {
                return new CompoundStatementNode(
                    null, "Compound statement", whileNode.Line,
                    whileNode.Position);
            }
            return whileNode;
        }

        private static StructStatementNode OptimizeFor(
            ForStatementNode forNode, SymTable symTable)
        {
            if (!(forNode.Childs[1] is ConstNode start) ||
                !(forNode.Childs[2] is ConstNode end))
                return forNode;
            if ((int) start.Value > (int) end.Value)
            {
                return new CompoundStatementNode(
                    null, "Compound statement", forNode.Line,
                    forNode.Position);
            }
            return forNode;
        }

        private static void FindAndOptimizeExpressions(Node start, SymTable symTable)
        {
            switch (start)
            {
                case RepeatStatementNode rn:
                    rn.Condition = OptimizeExpression(
                        (ExpressionNode) rn.Condition, symTable);
                    break;
                case WhileStatementNode wn:
                    wn.Condition = OptimizeExpression(
                        (ExpressionNode) wn.Condition, symTable);
                    break;
                case CompoundStatementNode csn:
                    foreach (var statement in csn.Statements)
                    {
                        FindAndOptimizeExpressions(statement, symTable);
                    }
                    break;
            }
            for (var i = 0; i < start.Childs?.Count; i++)
            {
                if (start.Childs[i] is ExpressionNode en)
                {
                    start.Childs[i] = OptimizeExpression(en, symTable);
                }
                else
                {
                    FindAndOptimizeExpressions(start.Childs[i], symTable);
                }
            }
        }

        private static ExpressionNode OptimizeExpression(
            ExpressionNode exprNode,
            SymTable symTable)
        {
            ExpressionNode res = exprNode;
            switch (exprNode)
            {
                case UnOpNode unOp:
                    unOp.Childs[0] = OptimizeExpression(
                        (ExpressionNode) unOp.Childs[0], symTable);
                    res = unOp;
                    if (unOp.Childs[0] is ConstNode)
                    {
                        object value = Parser.EvaluateConstExpr(unOp, symTable);
                        res = new ConstNode(null, value, unOp.Line, unOp.Position)
                        {
                            Type = TypeDictionary[value.GetType()]
                        };
                    }
                    break;
                case BinOpNode binOp:
                    binOp.Childs[0] = OptimizeExpression(
                        (ExpressionNode) binOp.Childs[0], symTable);
                    binOp.Childs[1] = OptimizeExpression(
                        (ExpressionNode) binOp.Childs[1], symTable);
                    res = binOp;
                    if (binOp.Childs[0] is ConstNode && binOp.Childs[1] is ConstNode)
                    {
                        object value = Parser.EvaluateConstExpr(binOp, symTable);
                        res = new ConstNode(null, value, binOp.Line, binOp.Position)
                        {
                            Type = TypeDictionary[value.GetType()]
                        };
                    }
                    break;
            }
            return res;
        }
    }
}