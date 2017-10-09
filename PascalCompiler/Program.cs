using System;
using System.IO;
using CommandLine;

namespace PascalCompiler
{
    internal class Program
    {
        // TODO: Remove python code, consolidate switch statement, add more error types, add error tests.
        // TODO: Remove doubling code in convert fucntions
        private static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options)) return;
            if (options.Mode == "tokenize")
            {
                try
                {
                    using (var reader = new StreamReader(options.InputFile))
                    using (var writer = new StreamWriter(options.OutputFile))
                    {
                        writer.WriteLine(
                            $"{"Line",-5}|{"Pos",-5}|{"Type",-12}|{"Subtype",-25}|{"Value",-35}|{"Source",-50}");
                        writer.WriteLine(new string('-', 142));
                        try
                        {
                            foreach (Tokenizer.Token t in new Tokenizer(reader).Tokens())
                            {
                                writer.Write(
                                    "{0, -5}|{1, -5}|{2, -12}|{3, -25}|{4, -35}|{5, -50}", 
                                    t.Line, 
                                    t.Position, 
                                    t.Type, 
                                    t.SubType, 
                                    t.GetStringValue().
                                        Replace("\n", "\\n").
                                        Replace("\r", "\\r").
                                        Replace("\t", "\\t"), 
                                    t.SourceString);
                                writer.WriteLine();
                            }
                        }
                        catch (Tokenizer.TokenizerException e)
                        {
                            writer.WriteLine($"{e.Message} at {e.Line}:{e.Position}");
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
