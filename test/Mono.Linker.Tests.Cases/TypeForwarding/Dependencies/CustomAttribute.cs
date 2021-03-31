// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Linker.Tests.Cases.TypeForwarding.Dependencies
{
#if INCLUDE_ATTRIBUTE
	[AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
	public sealed class CustomAttributeAttribute : BaseAttribute
	{
		public CustomAttributeAttribute (MyEnum e) :
			base (e)
		{
		}
	}
#endif
}