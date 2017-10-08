using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace PascalCompiler
{
    class Options
    {
        [Option('m', "mode", Required = true,
          HelpText = "Compiler mode (tokenize)")]
        public string Mode { get; set; }

        [Option('i', "input", Required = true,
          HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true,
          HelpText = "Output file to be written to.")]
        public string OutputFile { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption(HelpText = "Display this help text.")]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}