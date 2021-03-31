// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Linker.Tests.Cases.TypeForwarding.Dependencies
{
	public abstract partial class BaseAttribute : Attribute
	{
		protected BaseAttribute (MyEnum e)
		{
		}
	}

	[AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
	public sealed partial class CustomAttributeAttribute : BaseAttribute
	{
		public CustomAttributeAttribute (MyEnum e) :
			base (e)
		{
		}
	}
}