// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Linker.Tests.Cases.TypeForwarding.Dependencies
{
#if INCLUDE_ATTRIBUTE
	public abstract class BaseAttribute : Attribute
	{
		public BaseAttribute (MyEnum e)
		{
		}
	}

	[System.Flags]
	public enum MyEnum
	{
		One = 1,
		Two = 2,
		Three = 3,
	};
#endif
}
