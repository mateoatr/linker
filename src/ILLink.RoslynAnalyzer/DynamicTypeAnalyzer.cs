// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using ILLink.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILLink.RoslynAnalyzer
{
	[DiagnosticAnalyzer (LanguageNames.CSharp)]
	public class DynamicTypeAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create (s_dynamicTypeInvocationRule);

		static readonly DiagnosticDescriptor s_dynamicTypeInvocationRule = new DiagnosticDescriptor (
			Constants.WarningCodes.IL2026,
			new LocalizableResourceString (nameof (SharedStrings.DynamicTypeInvocationTitle),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			new LocalizableResourceString (nameof (SharedStrings.DynamicTypeInvocationMessage),
				SharedStrings.ResourceManager, typeof (SharedStrings)),
			DiagnosticCategory.Trimming,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		public override void Initialize (AnalysisContext context)
		{
			context.EnableConcurrentExecution ();
			context.ConfigureGeneratedCodeAnalysis (GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.RegisterCompilationStartAction (context => {
				var compilation = context.Compilation;
				if (!context.Options.MSBuildPropertyValueIsTrue (MSBuildPropertyOptionNames.EnableTrimAnalyzer, compilation))
					return;

				context.RegisterOperationAction (operationContext => {
					var dynamicTypeInvocation = (IDynamicInvocationOperation) operationContext.Operation;
					if (operationContext.GetContainingSymbol (DiagnosticTargets.All) is ISymbol containingSymbol &&
						containingSymbol.HasAttribute (Constants.RequiresUnreferencedCodeAttribute))
						return;

					operationContext.ReportDiagnostic (Diagnostic.Create (s_dynamicTypeInvocationRule,
						dynamicTypeInvocation.Syntax.GetLocation ()));
				}, OperationKind.DynamicInvocation);
			});
		}
	}
}
