// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Mono.Linker.Steps;
using Mono.Linker.Tests.TestCases;
using Mono.Linker.Tests.TestCasesRunner;
using Perfolizer.Mathematics.OutlierDetection;

namespace Mono.Linker.Tests.Benchmarks
{
	public class StepColumn : IColumn
	{
		public string Id { get; }
		public string ColumnName => "Linker Step";
		public bool AlwaysShow => true;
		public ColumnCategory Category => ColumnCategory.Custom;
		public int PriorityInCategory => 0;
		public bool IsNumeric => false;
		public UnitType UnitType => UnitType.Dimensionless;
		public string Legend => $"Custom '{nameof (StepColumn)}' column.";
		public bool IsAvailable (BenchmarkDotNet.Reports.Summary summary) => true;
		public bool IsDefault (BenchmarkDotNet.Reports.Summary summary, BenchmarkCase benchmarkCase) => false;

		public string GetValue (BenchmarkDotNet.Reports.Summary summary, BenchmarkCase benchmarkCase, BenchmarkDotNet.Reports.SummaryStyle style)
			=> this.GetValue (summary, benchmarkCase);

		public string GetValue (BenchmarkDotNet.Reports.Summary summary, BenchmarkCase benchmarkCase)
		{
			var passedArg = benchmarkCase.Parameters.Items.Single ();
			Type stepType = (passedArg.Value as IStep).GetType ();
			return stepType.Name;
		}
	}

	[Config (typeof (Config))]
	[Outliers (OutlierMode.DontRemove)]
	public class Benchmarks
	{
		Queue<string> LinkerArguments;
		BenchmarkDriver Driver;

		public Benchmarks ()
		{
			LinkerArguments = new Queue<string> ();
		}

		public IEnumerable<object> Foo ()
		{
			Driver = new BenchmarkDriver (GetLinkerArguments ());
			Driver.SetupContext ();
			return  Driver.Context.Pipeline.GetSteps ().Select (t => (object) t).ToArray ();
		}

		[Benchmark]
		[ArgumentsSource (nameof (Foo))]
		public void ProcessStep (IStep step) => Driver.ProcessStep (step);

		public static Queue<string> GetLinkerArguments ()
		{
			var linkerArguments = new Queue<string> ();
			using (var reader = File.OpenText (@"D:\tests\consoleApp\link.rsp"))
				Linker.Driver.ParseResponseFile (reader, linkerArguments);

			return linkerArguments;
		}

		class Config : ManualConfig
		{
			class StepsOrder : IOrderer
			{
				public bool SeparateLogicalGroups => true;

				public IEnumerable<BenchmarkCase> GetExecutionOrder (ImmutableArray<BenchmarkCase> benchmarksCase)
				{
					return
						from benchmark in benchmarksCase
						orderby benchmark.Job.Id descending,
							benchmark.Descriptor.WorkloadMethodDisplayInfo
						select benchmark;
				}

				public string GetHighlightGroupKey (BenchmarkCase benchmarkCase) => benchmarkCase.Descriptor.WorkloadMethodDisplayInfo;

				public string GetLogicalGroupKey (
					ImmutableArray<BenchmarkCase> allBenchmarksCases,
					BenchmarkCase benchmarkCase) => benchmarkCase.Job.Id;

				public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder (
					IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups) => logicalGroups.OrderBy (lg => lg.Key);

				public IEnumerable<BenchmarkCase> GetSummaryOrder (
					ImmutableArray<BenchmarkCase> benchmarksCases,
					BenchmarkDotNet.Reports.Summary summary) => benchmarksCases.OrderBy (bc => bc.Descriptor.WorkloadMethodDisplayInfo);
			}

			public Config ()
			{
				int totalIterations = 3;
				var jobs = new List<Job> ();
				foreach (var iteration in Enumerable.Range (0, totalIterations)) {
					var job = new Job ((iteration + 1).ToString ());
					job.Run.RunStrategy = RunStrategy.ColdStart;
					job.Run.IterationCount = 1;
					job.Run.WarmupCount = 0;
					jobs.Add (job);
				}

				Orderer = new StepsOrder ();
				AddJob (jobs.ToArray ());
				AddLogger (BenchmarkDotNet.Loggers.ConsoleLogger.Default);
				AddColumn (
					new StepColumn (),
					StatisticColumn.Mean,
					StatisticColumn.Min,
					StatisticColumn.Max);
				AddExporter (HtmlExporter.Default);
				UnionRule = ConfigUnionRule.AlwaysUseLocal;
			}
		}

		public class BenchmarkDriver : Driver
		{
			public BenchmarkDriver (Queue<string> arguments)
				: base (arguments)
			{
			}

			public LinkContext Context => context;

			public void ProcessStep (IStep step)
			{
				step.Process (context);
				context.Pipeline.RemoveCurrentStep ();
			}

			public void ProcessCurrentStep ()
			{
				var currStep = context.Pipeline.GetCurrentStep ();
				if (currStep is null)
					return;

				currStep.Process (context);
				context.Pipeline.RemoveCurrentStep ();
			}

			public void SetupContext () => base.SetupContext ();
		}
	}

	class RunBenchmarks
	{
		static void Main (string[] args)
		{
			var summary = BenchmarkRunner.Run<Benchmarks> (args: args);
			Console.WriteLine (summary);
		}
	}
}
