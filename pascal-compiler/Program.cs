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
            StreamReader reader = new StreamReader(args[0]);
            Tokenizer tokenizer = new Tokenizer(reader);
            if (args[1] == "-t")
            {
                foreach(var token in tokenizer.Tokens())
                {
                    Console.Write(token.SourceString);
                }   
            }
            Console.ReadLine();
            reader.Close();
            reader.Dispose();
        }
    }
}
