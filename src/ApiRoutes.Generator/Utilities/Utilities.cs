using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiRoutes.Generator;

public static class Utilities
{
    public static bool TryGetException(Compilation compilation,
        ObjectCreationExpressionSyntax objectCreationExpressionSyntax, out Tuple<INamedTypeSymbol, HttpStatusCode> exception)
    {
        var genericNameNode = objectCreationExpressionSyntax.ChildNodes()
            .FirstOrDefault(n => n.IsKind(SyntaxKind.GenericName));

        if (genericNameNode is GenericNameSyntax genericNameSyntax)
        {
            var correctSemanticModel = compilation.GetSemanticModel(genericNameSyntax.SyntaxTree);
            var symbol = ModelExtensions.GetSymbolInfo(correctSemanticModel, genericNameSyntax);

            bool IsRequestException(INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.BaseType == null)
                {
                    return false;
                }

                if (typeSymbol.BaseType.FullName() == LibraryTypes.RequestException)
                {
                    return true;
                }

                return IsRequestException(typeSymbol.BaseType);
            }

            if (symbol.Symbol is INamedTypeSymbol namedTypeSymbol && IsRequestException(namedTypeSymbol))
            {
                foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
                {
                    var baseConstructor = syntaxReference.GetSyntax().DescendantNodesAndSelf()
                        .FirstOrDefault(n =>
                            n.IsKind(SyntaxKind.BaseConstructorInitializer));

                    if (baseConstructor is ConstructorInitializerSyntax
                        baseConstructorInitializerSyntax)
                    {
                        var statusCode = baseConstructorInitializerSyntax
                            .ArgumentList.Arguments[0];


                        if (statusCode.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax && Enum.TryParse<HttpStatusCode>(memberAccessExpressionSyntax.Name.ToString(), out var httpStatusCode))
                        {
                            exception = new (namedTypeSymbol, httpStatusCode);
                            return true;
                        }
                    }
                }
            }
        }

        exception = null!;
        return false;
    }


    public static Tuple<INamedTypeSymbol, HttpStatusCode>? FindRequestExceptionInSymbol(Compilation compilation, ISymbol symbol)
    {
        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            foreach (var n in syntaxReference.GetSyntax().ChildNodes().First(n => n.IsKind(SyntaxKind.Block))
                         .DescendantNodesAndSelf())
            {
                if (n is ObjectCreationExpressionSyntax
                        objectCreationExpressionSyntax &&
                    TryGetException(compilation, objectCreationExpressionSyntax, out var exception))
                {
                    return exception;
                }

                if (n is InvocationExpressionSyntax invocation)
                {
                    var correctSemanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);
                    var otherSymbol = correctSemanticModel.GetSymbolInfo(invocation);

                    if (otherSymbol.Symbol != null)
                    {
                        return FindRequestExceptionInSymbol(compilation, otherSymbol.Symbol);
                    }
                }
            }
        }

        return null;
    }
}