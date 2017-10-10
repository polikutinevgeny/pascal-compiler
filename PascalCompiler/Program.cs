using System;
using System.IO;

namespace PascalCompiler
{
    internal static class Program
    {
        // TODO: Remove python code, consolidate switch statement, add more error types, add error tests.
        // TODO: Remove doubling code in convert fucntions
        // TODO: Not ASCII symbols?
        private static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options)) return;
            try
            {
                using (var reader = new StreamReader(options.InputFile))
                using (var writer = new StreamWriter(options.OutputFile))
                {
                    switch (options.Mode)
                    {
                        case "tokenize":
                            writer.WriteLine(
                                $"{"Line",-5}|{"Pos",-5}|{"Type",-12}|{"Subtype",-25}|{"Value",-35}|{"Source",-50}");
                            writer.WriteLine(new string('-', 142));
                            try
                            {
                                foreach (Tokenizer.Token t in new Tokenizer(reader))
                                {
                                    writer.Write(
                                        "{0, -5}|{1, -5}|{2, -12}|{3, -25}|{4, -35}|{5, -50}",
                                        t.Line,
                                        t.Position,
                                        t.Type,
                                        t.SubType,
                                        t.GetStringValue().Replace("\n", "\\n").Replace("\r", "\\r")
                                            .Replace("\t", "\\t"),
                                        t.SourceString);
                                    writer.WriteLine();
                                }
                            }
                            catch (Tokenizer.TokenizerException e)
                            {
                                writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            break;
                        case "parse":
                            try
                            {
                                using (var tokenizer = (new Tokenizer(reader)))
                                using (var tokenStream = tokenizer.GetEnumerator())
                                {
                                    var n = Parser.Parse(tokenStream);
                                    TreePrinter.PrintTree(writer, n, 0);
                                }
                            }
                            catch (Tokenizer.TokenizerException e)
                            {
                                Console.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            catch (Parser.ParserException e)
                            {
                                Console.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            break;
                        default:
                            Console.WriteLine($"Mode {options.Mode} not found.");
                            break;
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