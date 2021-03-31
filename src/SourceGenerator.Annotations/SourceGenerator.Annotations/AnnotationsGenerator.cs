using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

namespace SourceGenerator.Annotations
{
    [Generator]
    public class AnnotationsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not MethodSyntaxReceiver receiver)
                return;

            var parameters = receiver.GetCandidateMethods()
                .SelectMany(m => GetMethodSymbol(m, context).Parameters);

            var rule = new DiagnosticDescriptor(
                "test",
                "Range argument",
                "Argument '{0}' has a '{1}' attribute",
                "Info",
                DiagnosticSeverity.Info,
                true);

            foreach (var param in parameters)
            {
                if (!param.GetAttributes().Any(IsAttribute<RangeAttribute>))
                    continue;

                var diagnostic = Diagnostic.Create(
                    rule,
                    param.Locations.FirstOrDefault(),
                    param.Name,
                    nameof(RangeAttribute));
            }
        }

        private bool IsAttribute<TAttribute>(AttributeData attribute)
            where TAttribute : Attribute
        {
            return attribute.AttributeClass.Name.Equals(typeof(TAttribute).Name);
        }

        private static IMethodSymbol GetMethodSymbol(
            MethodDeclarationSyntax method,
            GeneratorExecutionContext context)
        {
            SemanticModel model = context.Compilation.GetSemanticModel(method.SyntaxTree);
            return model.GetDeclaredSymbol(method);
        }
    }

    /// <summary>
    /// Syntax Receiver to collect methods tagget with the <see cref="kopeFunctionAttribute"/>
    /// </summary>
    public class MethodSyntaxReceiver : ISyntaxReceiver
    {
        private readonly List<MethodDeclarationSyntax> candidateMethods = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Only interested in methods
            if (syntaxNode is MethodDeclarationSyntax methodDeclaration)
                this.candidateMethods.Add(methodDeclaration);
        }

        public IReadOnlyCollection<MethodDeclarationSyntax> GetCandidateMethods()
            => new ReadOnlyCollection<MethodDeclarationSyntax>(this.candidateMethods);
    }
}