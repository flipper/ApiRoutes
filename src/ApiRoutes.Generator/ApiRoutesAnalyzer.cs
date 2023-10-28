using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ApiRoutes.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiRoutesAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor throwingExceptionError = new DiagnosticDescriptor("APIROUTES0001",
        "Throwing exceptions is not allowed",
        "Throwing exceptions is not allowed", "ApiRoutes", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor allFieldsNotAnnotatedWhileFormRequest = new DiagnosticDescriptor(
        "APIROUTES0002",
        "Other properties are annotated with [FromForm] but this one isn't, so this will be ignored",
        "Other properties are annotated with [FromForm] but this one isn't, so this will be ignored", "ApiRoutes", DiagnosticSeverity.Warning, true);

    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.EnableConcurrentExecution();
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                       GeneratedCodeAnalysisFlags.ReportDiagnostics);

        analysisContext.RegisterCompilationStartAction(context =>
        {
            context.RegisterSymbolAction(symbolAnalysisContext =>
            {
                var namedTypeSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;

                if (!namedTypeSymbol.GetAttributes()
                        .Any(a => a.AttributeClass.FullName() == LibraryTypes.ApiRouteAttribute))
                {
                    return;
                }

                var properties = namedTypeSymbol.GetMembers().OfType<IPropertySymbol>().ToArray();
                
                var isFormRequest = properties.Any(p => p.GetAttributes().Any(a => a.AttributeClass.FullName() == LibraryTypes.FromFormAttribute));

                if (!isFormRequest)
                {
                    return;
                }
                
                foreach (var property in properties.Where(p => p.CanWrite() && !p.GetAttributes().Any(a => a.AttributeClass.FullName() == LibraryTypes.FromFormAttribute)))
                {
                    foreach (var location in property.Locations)
                    {
                        symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(allFieldsNotAnnotatedWhileFormRequest, location));
                    }
                }
                
            }, SymbolKind.NamedType);

            context.RegisterSymbolAction(symbolAnalysisContext =>
            {
                var namedTypeSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;

                if (namedTypeSymbol.IsAbstract)
                {
                    return;
                }

                if (!namedTypeSymbol.AllInterfaces.Any(@interface =>
                        (@interface.ConstructedFrom.FullName() == LibraryTypes.IHandlerWithoutResponse ||
                         @interface.ConstructedFrom.FullName() == LibraryTypes.IHandlerWithResponse) &&
                        @interface.TypeArguments.Length > 0))
                {
                    return;
                }

                Log.LogInfo("Handler found: " + namedTypeSymbol.FullName());

                var invokeAsync = namedTypeSymbol
                    .GetMembers()
                    .Where(m => m.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Select(m =>
                        m.ExplicitInterfaceImplementations.Length > 0
                            ? (m.ExplicitInterfaceImplementations[0], m.DeclaringSyntaxReferences)
                            : (m, m.DeclaringSyntaxReferences))
                    .FirstOrDefault(m => m.Item1.Name == "InvokeAsync");

                if (invokeAsync.Item1 != null)
                {
                    foreach (var reference in invokeAsync.DeclaringSyntaxReferences)
                    {
                        foreach (var throwToken in reference.GetSyntax().ChildNodes()
                                     .First(n => n.IsKind(SyntaxKind.Block))
                                     .DescendantNodesAndSelf().Where(t => t.IsKind(SyntaxKind.ThrowStatement)))
                        {
                            symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(throwingExceptionError,
                                throwToken.GetLocation()));
                        }
                    }
                }
            }, SymbolKind.NamedType);
        });
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(throwingExceptionError, allFieldsNotAnnotatedWhileFormRequest);
}