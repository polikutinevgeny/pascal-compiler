using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PascalCompiler;

namespace PascalCompiler
{
    public abstract class Node
    {
        protected Node(List<Node> childs, Tokenizer.Token token)
        {
            this.Childs = childs;
            this.Token = token;
        }

        public List<Node> Childs { get; set; }
        public Tokenizer.Token Token { get; }

        public override string ToString()
        {
            return Token.GetStringValue();
        }
    }

    public static class TreePrinter
    {
//        public static void PrintTree(StreamWriter writer, Node node, int level)
//        {
//            writer.Write(new String('\t', level));
//            writer.WriteLine(node);
//            foreach (var n in node.Childs ?? Enumerable.Empty<Node>())
//            {
//                PrintTree(writer, n, level + 1);
//            }
//        }
        public static void PrintTree(StreamWriter writer, Node node, string indent, bool last)
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
                indent += "| ";
            }
            writer.WriteLine(node);
            for (var i = 0; i < node.Childs?.Count; ++i)
            {
                PrintTree(writer, node.Childs[i], indent, i == node.Childs.Count - 1);
            }
        }
    }

    public class ExprNode : Node
    {
        public ExprNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class VarNode : ExprNode
    {
        public VarNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class ConstNode : ExprNode
    {
        public ConstNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class BinOpNode : ExprNode
    {
        public BinOpNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }

    public class UnOpNode : ExprNode
    {
        public UnOpNode(List<Node> childs, Tokenizer.Token token) : base(childs, token)
        {
        }
    }
}