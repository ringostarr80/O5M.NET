using System;
using System.Diagnostics;
using System.Linq;
using System.IO;

using CommandLine;
using OSMDataPrimitives.Xml;
using O5M;

namespace CLITest
{
	class MainClass
	{
		private static CLIOptions _cliOptions = new CLIOptions();

		public static CLIOptions CLIOptions { get { return _cliOptions; } }

		public static void Main(string[] args)
		{
			var parser = new Parser(with => { with.EnableDashDash = true; with.HelpWriter = Console.Out; });
			var helpRequestedError = false;
			var parserResult = parser.ParseArguments<CLIOptions>(args)
				.WithParsed<CLIOptions>(opts =>
				{
					_cliOptions = opts;
				})
				.WithNotParsed(errs =>
				{
					foreach (var err in errs)
					{
						if (err is HelpRequestedError)
						{
							helpRequestedError = true;
							break;
						}
					}
				});
			if (helpRequestedError)
			{
				return;
			}

			using (var o5mReader = new O5MReader(_cliOptions.InputFilename))
			{
				var nodeCounter = 0L;
				var wayCounter = 0L;
				var relationCounter = 0L;
				var nodesStarted = false;
				var waysStarted = false;
				var relationsStarted = false;
				var stopWatchNodes = new Stopwatch();
				var stopWatchWays = new Stopwatch();
				var stopWatchRelations = new Stopwatch();
				var nodesPerSecond = 0.0;
				var waysPerSecond = 0.0;
				var relationsPerSecond = 0.0;

				O5MWriter o5mWriter = null;
				if (_cliOptions.OutputFilename != string.Empty)
				{
					if (File.Exists(_cliOptions.OutputFilename))
					{
						File.Delete(_cliOptions.OutputFilename);
					}
					o5mWriter = new O5MWriter(_cliOptions.OutputFilename);
				}

				Console.WriteLine("Header: " + o5mReader.Header);
				if (o5mReader.FileTimestamp.HasValue)
				{
					Console.WriteLine("FileTimestamp: {0}", o5mReader.FileTimestamp.Value);
					o5mWriter?.WriteTimestamp(o5mReader.FileTimestamp.Value);
				}
				if (Math.Abs(o5mReader.LatitudeMin) > double.Epsilon && Math.Abs(o5mReader.LatitudeMax) > double.Epsilon &&
				  Math.Abs(o5mReader.LongitudeMin) > double.Epsilon && Math.Abs(o5mReader.LongitudeMax) > double.Epsilon)
				{
					var boundings = "Boundings:\n";
					boundings += "    LatitudeMin: " + o5mReader.LatitudeMin + "\n";
					boundings += "    LatitudeMax: " + o5mReader.LatitudeMax + "\n";
					boundings += "    LongitudeMin: " + o5mReader.LongitudeMin + "\n";
					boundings += "    LongitudeMax: " + o5mReader.LongitudeMax + "\n";
					Console.WriteLine(boundings);
					o5mWriter?.WriteBoundings(latitudeMin: o5mReader.LatitudeMin,
												latitudeMax: o5mReader.LatitudeMax,
												longitudeMin: o5mReader.LongitudeMin,
												longitudeMax: o5mReader.LongitudeMax);
				}

				var outputStatistics = new Action(() =>
				{
					Console.Write("\rnodes: {0:N0} (nodes/sec = {1:N0}); ways: {2:N0} (ways/sec = {3:N0}); relations: {4:N0} (relations/sec = {5:N0})", nodeCounter, nodesPerSecond, wayCounter, waysPerSecond, relationCounter, relationsPerSecond);
				});

#if DEBUG
				o5mReader.FoundNodeRaw = (node, rawData, debugInfos) =>
				{
					if (!nodesStarted)
					{
						stopWatchNodes.Start();
						nodesStarted = true;
					}
					nodeCounter++;
					if (o5mWriter != null)
					{
						byte[] writtenData = null;
						o5mWriter.WriteElement(node, out writtenData);
						if (!rawData.SequenceEqual(writtenData))
						{
							Console.WriteLine("read data (" + rawData.Length + ") differs from written data (" + writtenData.Length + ").");
							Console.WriteLine("node: " + node.ToXmlString());
							var maxBytes = Math.Max(rawData.Length, writtenData.Length);
							for (var i = 0; i < maxBytes; i++)
							{
								var originalByte = (i < rawData.Length) ? string.Format("{0:X2}", rawData[i]) : "--";
								var writtenByte = (i < writtenData.Length) ? string.Format("{0:X2}", writtenData[i]) : "--";
								if (originalByte != writtenByte)
								{
									Console.ForegroundColor = ConsoleColor.Red;
								}
								Console.WriteLine("{0} - {1}", originalByte, writtenByte);
								Console.ResetColor();
							}

							o5mReader.Stop();
						}
					}

					if (nodeCounter % 10000 == 0)
					{
						nodesPerSecond = nodeCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
				o5mReader.FoundWayRaw = (way, rawData, debugInfos) =>
				{
					if (!waysStarted)
					{
						stopWatchNodes.Stop();
						nodesPerSecond = nodeCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						stopWatchWays.Start();
						waysStarted = true;
					}
					wayCounter++;
					if (o5mWriter != null)
					{
						byte[] writtenData = null;
						o5mWriter.WriteElement(way, out writtenData);
						if (!rawData.SequenceEqual(writtenData))
						{
							Console.WriteLine("read data (" + rawData.Length + ") differs from written data (" + writtenData.Length + ").");
							Console.WriteLine("way: " + way.ToXmlString());
							var maxBytes = Math.Max(rawData.Length, writtenData.Length);
							for (var i = 0; i < maxBytes; i++)
							{
								var originalByte = (i < rawData.Length) ? string.Format("{0:X2}", rawData[i]) : "--";
								var writtenByte = (i < writtenData.Length) ? string.Format("{0:X2}", writtenData[i]) : "--";
								if (originalByte != writtenByte)
								{
									Console.ForegroundColor = ConsoleColor.Red;
								}
								Console.WriteLine("{0} - {1}", originalByte, writtenByte);
								Console.ResetColor();
							}

							o5mReader.Stop();
						}
					}

					if (wayCounter % 1000 == 0)
					{
						waysPerSecond = wayCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
				o5mReader.FoundRelationRaw = (relation, rawData, debugInfos) =>
				{
					if (!relationsStarted)
					{
						stopWatchWays.Stop();
						waysPerSecond = wayCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						stopWatchRelations.Start();
						relationsStarted = true;
					}

					relationCounter++;
					if (o5mWriter != null)
					{
						byte[] writtenData = null;
						o5mWriter.WriteElement(relation, out writtenData);
						if (!rawData.SequenceEqual(writtenData))
						{
							Console.WriteLine("read data (" + rawData.Length + ") differs from written data (" + writtenData.Length + ").");
							Console.WriteLine("relation: " + relation.ToXmlString());
							var maxBytes = Math.Max(rawData.Length, writtenData.Length);
							for (var i = 0; i < maxBytes; i++)
							{
								var originalByte = (i < rawData.Length) ? string.Format("{0:X2}", rawData[i]) : "--";
								var writtenByte = (i < writtenData.Length) ? string.Format("{0:X2}", writtenData[i]) : "--";
								var additionalDebugInfos = string.Empty;
								if (debugInfos.InfoExistsForBytePosition((uint)(i + 1)))
								{
									additionalDebugInfos = " => " + debugInfos[(uint)(i + 1)];
								}
								if (originalByte != writtenByte)
								{
									Console.ForegroundColor = ConsoleColor.Red;
								}
								Console.WriteLine("{0} - {1}{2}", originalByte, writtenByte, additionalDebugInfos);
								Console.ResetColor();
							}

							o5mReader.Stop();
						}
					}

					if (relationCounter % 1000 == 0)
					{
						relationsPerSecond = relationCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
#else
				o5mReader.FoundNode = (node) => {
					if(!nodesStarted) {
						stopWatchNodes.Start();
						nodesStarted = true;
					}
					nodeCounter++;
					o5mWriter?.WriteElement(node);

					if(nodeCounter % 10000 == 0) {
						nodesPerSecond = nodeCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
				o5mReader.FoundWay = (way) => {
					if(!waysStarted) {
						stopWatchNodes.Stop();
						nodesPerSecond = nodeCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						stopWatchWays.Start();
						waysStarted = true;
					}
					wayCounter++;
					o5mWriter?.WriteElement(way);

					if(wayCounter % 1000 == 0) {
						waysPerSecond = wayCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
				o5mReader.FoundRelation = (relation) => {
					if(!relationsStarted) {
						stopWatchWays.Stop();
						waysPerSecond = wayCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						stopWatchRelations.Start();
						relationsStarted = true;
					}
					relationCounter++;
					o5mWriter?.WriteElement(relation);

					if(relationCounter % 1000 == 0) {
						relationsPerSecond = relationCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);
						outputStatistics();
					}
				};
#endif

				o5mReader.SkipNodes = _cliOptions.SkipNodes;
				o5mReader.SkipWays = _cliOptions.SkipWays;
				o5mReader.SkipRelations = _cliOptions.SkipRelations;
				o5mReader.Start();
				stopWatchRelations.Stop();
				relationsPerSecond = relationCounter / (stopWatchNodes.ElapsedMilliseconds / 1000.0);

				outputStatistics();
				Console.WriteLine();

				o5mWriter?.Dispose();
			}
		}
	}
}
