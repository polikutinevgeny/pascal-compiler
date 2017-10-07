using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            const string description = 
                "Pascal compiler by Polikutin Evgeny (FEFU, B8303a, 2017).\n" +
                "Usage: pascal-compiler filename [-t]\n" +
                "Options:\n" +
                "\t-t\tStart tokenizer.";
            if (args.Length == 0)
            {
                Console.WriteLine(description);
                Console.ReadLine();
                return;
            }
            HashSet<string> parameters = new HashSet<string>(args.Skip(1));
            if (parameters.Contains("-t"))
            {
                using (var reader = new StreamReader(args[0]))
                {
                    try
                    {
                        foreach (var token in (new Tokenizer(reader)).Tokens())
                        {
                            Console.WriteLine(token);
                        }
                    }
                    catch (Tokenizer.TokenizerException e)
                    {
                        Console.WriteLine("{0} at {1}:{2}", e.Message, e.Line, e.Position);
                    }
                }
                Console.ReadLine();
            }
        }
    }
}
