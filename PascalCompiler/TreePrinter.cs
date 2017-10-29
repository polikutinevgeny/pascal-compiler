using System.IO;
using System.Linq;

namespace PascalCompiler
{
    public static class TreePrinter
    {
        public static void PrintProgram(StreamWriter writer, PascalProgram program)
        {
            writer.WriteLine($"program {program.Name.Name}");
            PrintBlock(writer, program.Block, "");
        }

        public static void PrintBlock(StreamWriter writer, Block block, string indent)
        {
            foreach (var key in block.SymTable.Keys)
            {
                switch (block.SymTable[key])
                {
                    case ProcedureSymbol pc:
                        writer.WriteLine(indent + new string('━', 40));
                        writer.WriteLine(indent + indent + $"  {pc}");
                        PrintBlock(writer, pc.Block, indent + "  ");
                        writer.WriteLine(indent + new string('━', 40));
                        continue;
                    case FunctionSymbol fc:
                        writer.WriteLine(indent + new string('━', 40));
                        writer.WriteLine(indent + indent + $"  {fc}");
                        PrintBlock(writer, fc.Block, indent + "  ");
                        writer.WriteLine(indent + new string('━', 40));
                        continue;
                }
                writer.WriteLine(indent + indent + $"  {key} = {block.SymTable[key]};");
            }
            foreach (var st in block.StatementList.Take(block.StatementList.Count - 1))
            {
                PrintNode(writer, st, indent, false);
            }
            if (block.StatementList.Count > 0)
            {
                PrintNode(writer, block.StatementList.Last(), indent, true);
            }
        }

        public static void PrintNode(StreamWriter writer, Node node, string indent, bool last)
        {
            writer.Write(indent);
            if (last)
            {
                writer.Write("└─");
                indent += "  ";
            }
            else
            {
                writer.Write("├─");
                indent += "│ ";
            }
            switch (node)
            {
                case WhileStatementNode wsn:
                    writer.WriteLine("while");
                    PrintNode(writer, wsn.Condition, indent, false);
                    break;
                case CompoundStatementNode csn:
                    writer.WriteLine("compound statement");
                    for (var i = 0; i < csn.Statements.Count; ++i)
                    {
                        PrintNode(writer, csn.Statements[i], indent, i == csn.Statements.Count - 1);
                    }
                    break;
                case RepeatStatementNode rsn:
                    writer.WriteLine("repeat");
                    PrintNode(writer, rsn.Condition, indent, false);
                    break;
                default:
                    writer.WriteLine(node);
                    break;
            }
            for (var i = 0; i < node?.Childs?.Count; ++i)
            {
                PrintNode(writer, node.Childs[i], indent, i == node.Childs.Count - 1);
            }
        }
    }
}