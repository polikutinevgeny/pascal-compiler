using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.Mode == "tokenize")
                {
                    try
                    {
                        using (var reader = new StreamReader(options.InputFile))
                        using (var writer = new StreamWriter(options.OutputFile))
                        {
                            writer.WriteLine("{0, -5}|{1, -5}|{2, -12}|{3, -25}|{4, -35}|{0, -50}", "Line", "Pos", "Type", "Subtype", "Source", "Value (if apllicable)");
                            writer.WriteLine(new String('-', 142));
                            try
                            {
                                foreach (var t in (new Tokenizer(reader)).Tokens())
                                {
                                    writer.Write("{0, -5}|{1, -5}|{2, -12}|{3, -25}|{4, -35}|", t.Line, t.Position, t.Type, t.SubType, t.SourceString);
                                    if (t is Tokenizer.IntToken)
                                    {
                                        writer.Write("{0, -50}", (t as Tokenizer.IntToken).Value);
                                    }
                                    if (t is Tokenizer.DoubleToken)
                                    {
                                        writer.Write("{0, -50}", (t as Tokenizer.DoubleToken).Value);
                                    }
                                    if (t is Tokenizer.StringToken)
                                    {
                                        writer.Write("{0, -50}", (t as Tokenizer.StringToken).Value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"));
                                    }
                                    writer.WriteLine();
                                }
                            }
                            catch (Tokenizer.TokenizerException e)
                            {
                                writer.WriteLine("{0} at {1}:{2}", e.Message, e.Line, e.Position);
                            }
                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine($"{e.FileName} not found");
                    }
                }
            }
        }
    }
}
