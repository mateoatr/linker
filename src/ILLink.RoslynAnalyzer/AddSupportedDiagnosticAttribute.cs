// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace ILLink.RoslynAnalyzer
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	internal class AddSupportedDiagnosticAttribute : Attribute
	{
		public AddSupportedDiagnosticAttribute (string code, string name)
		{
			Code = code;
			Name = name;
		}

		public string Code { get; }
		public string Name { get; }
		public string Category { get; set; } = DiagnosticCategory.Trimming;
		public bool IsEnabledByDefault { get; set; } = true;
		public string? HelpLinkURI { get; set; } = null;
		public bool GenerateResourceStrings { get; set; } = true;
	}
}
