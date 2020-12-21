﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace ILLink.RoslynAnalyzer
{
	static class ISymbolExtensions
	{
		internal static bool HasAttribute (this ISymbol symbol, string attributeName)
		{
			return symbol.GetAttributes ().Where (a => a?.AttributeClass?.Name == attributeName).Count () > 0;
		}
	}
}
