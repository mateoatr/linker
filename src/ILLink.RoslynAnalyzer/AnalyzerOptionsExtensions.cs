// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILLink.RoslynAnalyzer
{
	internal static class AnalyzerOptionsExtensions
	{
		private static Dictionary<string, string> s_cachedOptions = new Dictionary<string, string> ();

		public static string? GetMSBuildPropertyValue (
			this AnalyzerOptions options,
			string optionName,
			Compilation compilation)
		{
			// MSBuild property values should be set at compilation level, and cannot have different values per-tree.
			// So, we default to first syntax tree.
			var tree = compilation.SyntaxTrees.FirstOrDefault ();
			if (tree is null) {
				return null;
			}

			if (s_cachedOptions.TryGetValue (optionName, out var cachedOption)) {
				return cachedOption;
			}

			s_cachedOptions = ComputeCategorizedAnalyzerConfigOptions () ?? s_cachedOptions;
			return s_cachedOptions.TryGetValue ($"build_property.{optionName}", out string optionValue) ? optionValue :
				options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue (
					$"build_property.{optionName}", out var value)
				? value : null;

			Dictionary<string, string>? ComputeCategorizedAnalyzerConfigOptions ()
			{
				foreach (var additionalFile in options.AdditionalFiles) {
					var fileName = Path.GetFileName (additionalFile.Path);
					if (fileName.Equals (".editorconfig", StringComparison.OrdinalIgnoreCase)) {
						var text = additionalFile.GetText ();
						if (text is null)
							return null;

						return EditorConfigParser.ParseEditorConfig (text);
					}
				}

				return null;
			}
		}
	}
}
