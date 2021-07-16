// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace ILLink.RoslynAnalyzer
{
	[Flags]
	public enum DiagnosticTargets
	{
		MethodOrConstructor = 0x0001,
		Property = 0x0002,
		Field = 0x0004,
		Event = 0x0008,
		All = MethodOrConstructor | Property | Field | Event
	}
}
