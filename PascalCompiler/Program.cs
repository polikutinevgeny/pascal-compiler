using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PascalCompiler
{
    internal static class Program
    {
        // TODO: Remove python code, consolidate switch statement, add more error types, add error tests.
        // TODO: Remove doubling code in convert fucntions
        // TODO: Not ASCII symbols?
        // TODO: Token value boxing.
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
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
                                foreach (var t in new Tokenizer(reader))
                                {
                                    if (t.Type == Tokenizer.TokenType.EndOfFile)
                                    {
                                        break;
                                    }
                                    writer.Write(
                                        "{0, -5}|{1, -5}|{2, -12}|{3, -25}|{4, -35}|{5, -50}",
                                        t.Line,
                                        t.Position,
                                        t.Type,
                                        t.SubType,
                                        t.Value.ToString().Replace("\n", "\\n").Replace("\r", "\\r")
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
                                using (var tokenizer = new Tokenizer(reader))
                                using (var tokenStream = tokenizer.GetEnumerator())
                                using (var parser = new Parser(tokenStream))
                                {
                                    var p = parser.Parse();
                                    TreePrinter.PrintProgram(writer, p);
                                }
                            }
                            catch (Tokenizer.TokenizerException e)
                            {
                                writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            catch (Parser.ParserException e)
                            {
                                writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            break;
                        case "generate":
                            try
                            {
                                using (var tokenizer = new Tokenizer(reader))
                                using (var tokenStream = tokenizer.GetEnumerator())
                                using (var parser = new Parser(tokenStream))
                                {
                                    var p = parser.Parse();
                                    var asm = new AsmCode(p);
                                    writer.WriteLine(asm);
                                }
                            }
                            catch (Tokenizer.TokenizerException e)
                            {
                                writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
                            }
                            catch (Parser.ParserException e)
                            {
                                writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
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