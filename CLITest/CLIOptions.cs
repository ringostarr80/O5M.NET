using CommandLine;
using CommandLine.Text;

namespace CLITest
{
	public class CLIOptions
	{
		[Option('a', "skip-nodes", Required = false, HelpText = "skip all nodes.")]
		public bool SkipNodes { get; set; }

		[Option('b', "skip-ways", Required = false, HelpText = "skip all ways.")]
		public bool SkipWays { get; set; }

		[Option('c', "skip-relations", Required = false, HelpText = "skip all relations.")]
		public bool SkipRelations { get; set; }

		[Option('i', "input-filename", Required = true, HelpText = "the o5m-filename to read.")]
		public string InputFilename { get; set; }

		[Option('o', "output-filename", Required = false, HelpText = "the o5m-filename to write.")]
		public string OutputFilename { get; set; }

		public CLIOptions()
		{
			if (string.IsNullOrWhiteSpace(this.InputFilename))
			{
				this.InputFilename = "";
			}
			if (string.IsNullOrWhiteSpace(this.OutputFilename))
			{
				this.OutputFilename = "";
			}
		}
	}
}
