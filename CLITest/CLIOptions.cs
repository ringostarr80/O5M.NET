using CommandLine;
using CommandLine.Text;

namespace CLITest
{
	public class CLIOptions
	{
		[Option('a', "skip-nodes", DefaultValue = false, Required = false, HelpText = "skip all nodes.")]
		public bool SkipNodes { get; set; }

		[Option('b', "skip-ways", DefaultValue = false, Required = false, HelpText = "skip all ways.")]
		public bool SkipWays { get; set; }

		[Option('c', "skip-relations", DefaultValue = false, Required = false, HelpText = "skip all relations.")]
		public bool SkipRelations { get; set; }

		[Option('i', "input-filename", DefaultValue = "", Required = true, HelpText = "the o5m-filename to read.")]
		public string InputFilename { get; set; }

		[Option('o', "output-filename", DefaultValue = "", Required = false, HelpText = "the o5m-filename to write.")]
		public string OutputFilename { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			var help = new HelpText {
				AddDashesToOption = true,
				AdditionalNewLineAfterOption = true,
				Copyright = new CopyrightInfo("locr GmbH", 2015, 2016)
			};
			help.AddOptions(this);

			return help;
		}
	}
}
