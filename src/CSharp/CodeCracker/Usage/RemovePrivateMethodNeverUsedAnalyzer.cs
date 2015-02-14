﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace CodeCracker.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RemovePrivateMethodNeverUsedAnalyzer : DiagnosticAnalyzer
    {

        internal const string Title = "Unused method";
        internal const string Message = "Method '{0}' is not used.";
        internal const string Category = SupportedCategories.Usage;
        const string Description = "When a method declares  does not use it might bring incorrect conclusions.";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
             DiagnosticId.RemovePrivateMethodNeverUsed.ToDiagnosticId(),
            Title,
            Message,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            helpLink: HelpLink.ForDiagnostic(DiagnosticId.RemovePrivateMethodNeverUsed));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(a => a.ValueText == SyntaxFactory.Token(SyntaxKind.PrivateKeyword).ValueText)) return;


            if (IsMethodUsed(methodDeclaration)) return;

            if (methodDeclaration.Modifiers.Any(SyntaxKind.ExternKeyword)) return;

            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }

        private bool IsMethodUsed(MethodDeclarationSyntax methodTarget)
        {
            var classDeclaration = (ClassDeclarationSyntax)methodTarget.Parent;

            return (from invocation in classDeclaration.SyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>()
                    where ((IdentifierNameSyntax)invocation.Expression).Identifier.ValueText.Equals(methodTarget.Identifier.ValueText)
                    select invocation).Any();
        }
    }
}