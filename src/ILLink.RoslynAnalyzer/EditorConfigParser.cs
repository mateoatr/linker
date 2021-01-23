// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ILLink.RoslynAnalyzer
{
	// Copied from https://github.com/dotnet/roslyn-analyzers/blob/14fd5efc7204b29f613ca28abb7d139f208bfb21/src/Utilities/Compiler/Options/EditorConfigParser.cs
	/// <summary>
	/// Parses a given .editorconfig source text.
	/// </summary>
	internal static class EditorConfigParser
	{
		// Matches EditorConfig property such as "indent_style = space", see http://editorconfig.org for details
		private static readonly Regex s_propertyMatcher = new (@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$", RegexOptions.Compiled);

		private static readonly StringComparer s_keyComparer = CaseInsensitiveComparison.Comparer;

		/// <summary>
		/// A set of keys that are reserved for special interpretation for the editorconfig specification.
		/// All values corresponding to reserved keys in a (key,value) property pair are always lowercased
		/// during parsing.
		/// </summary>
		/// <remarks>
		/// This list was retrieved from https://editorconfig.org/
		/// at 2021-01-22 15:32:31Z. New keys may be added to this list in newer versions, but old ones will
		/// not be removed.
		/// </remarks>
		private static readonly ImmutableHashSet<string> s_reservedKeys
			= ImmutableHashSet.CreateRange (s_keyComparer, new[] {
				"indent_style",
				"indent_size",
				"tab_width",
				"end_of_line",
				"charset",
				"trim_trailing_whitespace",
				"insert_final_newline",
				"root"
			});

		/// <summary>
		/// A set of values that are reserved for special use for the editorconfig specification
		/// and will always be lower-cased by the parser.
		/// </summary>
		private static readonly ImmutableHashSet<string> s_reservedValues
			= ImmutableHashSet.CreateRange (s_keyComparer, new[] { "unset" });

		internal static Dictionary<string, string> ParseEditorConfig (SourceText text)
		{
			var parsedOptions = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

			foreach (var textLine in text.Lines) {
				var line = textLine.ToString ();
				if (string.IsNullOrWhiteSpace (line) || IsComment (line)) {
					continue;
				}

				var propMatches = s_propertyMatcher.Matches (line);
				if (propMatches.Count > 0 && propMatches[0].Groups.Count > 1) {
					var key = propMatches[0].Groups[1].Value;
					var value = propMatches[0].Groups[2].Value;

					Debug.Assert (!string.IsNullOrEmpty (key));
					Debug.Assert (key == key.Trim ());
					Debug.Assert (value == value.Trim ());

					key = CaseInsensitiveComparison.ToLower (key);
					if (s_reservedKeys.Contains (key) || s_reservedValues.Contains (value)) {
						value = CaseInsensitiveComparison.ToLower (value);
					}

					parsedOptions[key] = value;
					continue;
				}
			}

			return parsedOptions;
		}

		static bool IsComment (string line)
		{
			foreach (char c in line) {
				if (!char.IsWhiteSpace (c)) {
					return c is '#' or ';';
				}
			}

			return false;
		}
	}
}
