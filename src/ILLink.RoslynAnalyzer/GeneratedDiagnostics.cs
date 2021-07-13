// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ILLink.RoslynAnalyzer
{
	internal static class GeneratedDiagnostics
	{
		static readonly Dictionary<Type, List<string>> supportedDiagnosticsOnType = new Dictionary<Type, List<string>> ();
		static readonly Dictionary<string, DiagnosticDescriptor> generatedDiagnostics = new Dictionary<string, DiagnosticDescriptor> ();

		public static DiagnosticDescriptor GetDiagnostic(string diagnosticCode)
		{
			if (!generatedDiagnostics.TryGetValue (diagnosticCode, out var diagnostic))
				throw new ArgumentException ("TODO");

			return diagnostic;
		}

		public static DiagnosticDescriptor[] GetSupportedDiagnosticsOnType (Type type)
		{
			var supportedDiagnostics = new List<DiagnosticDescriptor> ();
			foreach (var diagnostic in supportedDiagnosticsOnType[type]) {
				if (!generatedDiagnostics.TryGetValue (diagnostic, out var supportedDiagnostic))
					throw new ArgumentException ("TODO");
				
				supportedDiagnostics.Add (supportedDiagnostic);
			}

			return supportedDiagnostics.ToArray ();
		}
	}
}
