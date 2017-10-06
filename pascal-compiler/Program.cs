using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pascal_compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader reader = new StreamReader(args[0]);
            if (args[1] == "-t")
            {
                foreach(var token in Tokenizer.Tokens(reader))
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
