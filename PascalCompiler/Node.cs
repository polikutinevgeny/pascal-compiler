using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    public abstract class Node
    {
        protected Node(List<Node> childs, Tokenizer.Token token)
        {
            Childs = childs;
            Token = token;
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
                indent += "│ ";
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