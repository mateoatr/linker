// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.TypeForwarding.Dependencies;

namespace Mono.Linker.Tests.Cases.TypeForwarding
{
	[SkipUnresolved (true)]
	[SetupLinkerDefaultAction ("link")]

	[SetupCompileBefore ("BaseAttribute.dll", new[] { "Dependencies/BaseAttribute.cs" }, defines: new[] { "INCLUDE_ATTRIBUTE" })]
	[SetupCompileBefore ("CustomAttribute.dll", new[] { "Dependencies/CustomAttribute.cs" },
		references: new[] { "BaseAttribute.dll" }, defines: new[] { "INCLUDE_ATTRIBUTE" })]
	[SetupCompileBefore ("Library.dll", new[] { "Dependencies/Library.cs" }, references: new[] { "BaseAttribute.dll", "CustomAttribute.dll" })]

	[SetupCompileAfter ("Attributes.dll", new[] { "Dependencies/BothAttributes.cs" }, references: new[] { "BaseAttribute.dll" })]
	[SetupCompileAfter ("Enum.dll", new[] { "Dependencies/MyEnum.cs" })]
	[SetupCompileAfter ("BaseAttribute.dll", new[] { "Dependencies/EnumForwarder.cs" }, references: new[] { "Enum.dll" })]

	[KeptAssembly ("BaseAttribute.dll")]
	class CustomAttributeArgumentIsForwarded
	{
		static void Main ()
		{
			Library.KeepCustomAttribute ();
		}
	}
}
