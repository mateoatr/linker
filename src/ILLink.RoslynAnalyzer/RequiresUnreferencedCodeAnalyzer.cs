// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using ILLink.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILLink.RoslynAnalyzer
{
	[DiagnosticAnalyzer (LanguageNames.CSharp)]
	public sealed class RequiresUnreferencedCodeAnalyzer : RequiresAnalyzerBase
	{
		public static readonly DiagnosticDescriptor RequiresUnreferencedCodeRule = new DiagnosticDescriptor (
			Constants.WarningCodes.IL2026,
			new LocalizableResourceString (nameof (SharedStrings.RequiresUnreferencedCodeTitle),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			new LocalizableResourceString (nameof (SharedStrings.RequiresUnreferencedCodeMessage),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			DiagnosticCategory.Trimming,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		public static readonly DiagnosticDescriptor DynamicTypeInvocationRule = new DiagnosticDescriptor (
			Constants.WarningCodes.IL2026,
			new LocalizableResourceString (nameof (SharedStrings.DynamicTypeInvocationTitle),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			new LocalizableResourceString (nameof (SharedStrings.DynamicTypeInvocationMessage),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			DiagnosticCategory.Trimming,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		static readonly DiagnosticDescriptor s_requiresAttributeMismatch = new DiagnosticDescriptor (
			Constants.WarningCodes.IL2046,
			new LocalizableResourceString (nameof (SharedStrings.RequiresAttributeMismatchTitle),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			new LocalizableResourceString (nameof (SharedStrings.RequiresAttributeMismatchMessage),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			DiagnosticCategory.Trimming,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		static readonly Action<OperationAnalysisContext> s_dynamicTypeInvocation = operationContext => {
			if (operationContext.GetContainingSymbol (DiagnosticTargets.All) is ISymbol containingSymbol &&
				containingSymbol.HasAttribute (Constants.RequiresUnreferencedCodeAttribute))
				return;

			operationContext.ReportDiagnostic (Diagnostic.Create (DynamicTypeInvocationRule,
				operationContext.Operation.Syntax.GetLocation ()));
		};

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create (RequiresUnreferencedCodeRule, s_requiresAttributeMismatch, DynamicTypeInvocationRule);

		private protected override string RequiresAttributeName => Constants.RequiresUnreferencedCodeAttribute;

		private protected override string RequiresAttributeFullyQualifiedName => Constants.FullyQualifiedRequiresUnreferencedCodeAttribute;

		private protected override DiagnosticTargets AnalyzerDiagnosticTargets => DiagnosticTargets.MethodOrConstructor;

		private protected override DiagnosticDescriptor RequiresDiagnosticRule => RequiresUnreferencedCodeRule;

		private protected override DiagnosticDescriptor RequiresAttributeMismatch => s_requiresAttributeMismatch;

		private protected override ImmutableArray<(Action<OperationAnalysisContext> Action, OperationKind[] OperationKind)> ExtraOperationActions =>
			ImmutableArray.Create ((s_dynamicTypeInvocation, new OperationKind[] { OperationKind.DynamicInvocation }));

		protected override bool IsAnalyzerEnabled (AnalyzerOptions options, Compilation compilation) =>
			options.IsMSBuildPropertyValueTrue (MSBuildPropertyOptionNames.EnableTrimAnalyzer, compilation);

		protected override bool VerifyAttributeArguments (AttributeData attribute) =>
			attribute.ConstructorArguments.Length >= 1 && attribute.ConstructorArguments[0] is { Type: { SpecialType: SpecialType.System_String } } ctorArg;

		protected override string GetMessageFromAttribute (AttributeData? requiresAttribute)
		{
			var message = (string) requiresAttribute!.ConstructorArguments[0].Value!;
			return MessageFormat.FormatRequiresAttributeMessageArg (message);
		}
	}
}
