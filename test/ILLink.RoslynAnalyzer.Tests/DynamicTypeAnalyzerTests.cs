// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = ILLink.RoslynAnalyzer.Tests.CSharpAnalyzerVerifier<
	ILLink.RoslynAnalyzer.DynamicTypeAnalyzer>;

namespace ILLink.RoslynAnalyzer.Tests
{
	public class DynamicTypeAnalyzerTests
	{
		static async Task VerifyDynamicTypeAnalyzer (string source, params DiagnosticResult[] expected) =>
			await VerifyCS.VerifyAnalyzerAsync (source, TestCaseUtils.UseMSBuildProperties (MSBuildPropertyOptionNames.EnableTrimAnalyzer), null, expected);

		[Fact]
		public Task DynamicTypeInvokation ()
		{
			var source = @"
using System;

class C
{
	static void M0 ()
	{
		dynamic dynamicField = ""Some string"";
		Console.WriteLine (dynamicField);
	}

	static void M1 ()
	{
		MethodWithDynamicArgDoNothing (0);
		MethodWithDynamicArgDoNothing (""Some string"");
		MethodWithDynamicArg(-1);
	}

	static void MethodWithDynamicArgDoNothing (dynamic arg)
	{
	}

	static void MethodWithDynamicArg (dynamic arg)
	{
		arg.MethodWithDynamicArg (arg);
	}
}";

			return VerifyDynamicTypeAnalyzer (source,
				// (10,3): warning IL2026: Invoking members on dynamic types is not trimming safe. Types or member might have been removed by the trimmer.
				VerifyCS.Diagnostic ().WithSpan (9, 3, 9, 35),
				// (26,3): warning IL2026: Invoking members on dynamic types is not trimming safe. Types or member might have been removed by the trimmer.
				VerifyCS.Diagnostic ().WithSpan (25, 3, 25, 33));
		}
	}
}
