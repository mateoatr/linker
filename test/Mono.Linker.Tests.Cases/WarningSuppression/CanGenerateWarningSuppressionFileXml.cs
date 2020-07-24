﻿using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.WarningSuppression.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Mono.Linker.Tests.Cases.WarningSuppression
{
	[SetupLinkerCoreAction ("skip")]
	[SetupCompileBefore ("library.dll", new[] { "Dependencies/TriggerWarnings_Lib.cs" })]
	[KeptAssembly ("library.dll")]
	[SetupLinkerAction ("link", "library.dll")]
	[SetupLinkerArgument ("--verbose")]
	[SetupLinkerArgument ("--generate-warning-suppressions", "xml")]
	public class CanGenerateWarningSuppressionFileXml
	{
		public static void Main ()
		{
			TriggerWarnings_Lib.Main ();
		}
	}
}
