// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ILLink.RoslynAnalyzer
{
	public static class Constants
	{
		public static class WarningCodes
		{
			public const string IL2026 = nameof (IL2026);
			public const string IL2046 = nameof (IL2046);
			public const string IL3000 = nameof (IL3000);
			public const string IL3001 = nameof (IL3001);
			public const string IL3002 = nameof (IL3002);
			public const string IL3003 = nameof (IL3003);
		}

		public const string RequiresUnreferencedCodeAttribute = nameof (RequiresUnreferencedCodeAttribute);
		public const string FullyQualifiedRequiresUnreferencedCodeAttribute = "System.Diagnostics.CodeAnalysis." + RequiresUnreferencedCodeAttribute;
		public const string RequiresAssemblyFilesAttribute = nameof (RequiresAssemblyFilesAttribute);
		public const string RequiresAssemblyFilesAttributeFullyQualifiedName = "System.Diagnostics.CodeAnalysis." + RequiresAssemblyFilesAttribute;
	}
}
