using Microsoft.CodeAnalysis;

namespace ApiRoutes.Generator;

public static class SemanticHelper
{
    /// <summary>
    /// This will return a full name if the type symbol is a IArrayTypeSymbol or INamedTypeSymbol. 
    /// Otherwise it will return the symbol's name without a namespace.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static string? TryFullName(this ITypeSymbol? symbol)
    {
        if (symbol == null)
            return null;

        if (symbol is IArrayTypeSymbol ats)
            return $"{ats.ElementType.TryFullName()}[{(new string(',', ats.Rank - 1))}]";

        if (symbol is INamedTypeSymbol nts)
            return FullName(nts);

        return symbol.Name;
    }

    /// <summary>
    /// This will attempt to get the full name of the array, including its namespace and rank.
    /// If the ElementType is not a INamedTypeSymbol, then the full name may not be available.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static string? TryFullName(this IArrayTypeSymbol? symbol)
    {
        if (symbol == null)
            return null;

        return $"{symbol.ElementType.TryFullName()}[{(new string(',', symbol.Rank - 1))}]";
    }

    /// <summary>
    /// Returns the full name of a method, including any type parameters. It does not inlcude parameters or the return type.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static string FullName(this IMethodSymbol symbol)
    {
        var suffix = "";
        if (symbol.Arity > 0)
        {
            suffix = CollectTypeArguments(symbol.TypeArguments);
        }

        return symbol.Name + suffix;
    }

    /// <summary>
    /// This is used to append a `?` to type name.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    static string NullableToken(this ITypeSymbol symbol)
    {
        if (symbol.IsValueType || symbol.NullableAnnotation != NullableAnnotation.Annotated)
            return "";
        return "?";
    }

    /// <summary>
    /// This will return a full name of a type, including the namespace and type arguments.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static string? FullName(this INamedTypeSymbol? symbol)
    {
        if (symbol == null)
            return null;

        var prefix = FullNamespace(symbol);
        var suffix = "";
        if (symbol.Arity > 0)
        {
            suffix = CollectTypeArguments(symbol.TypeArguments);
        }

        if (prefix != "")
            return prefix + "." + symbol.Name + suffix + symbol.NullableToken();
        return symbol.Name + suffix + symbol.NullableToken();
    }

    public static string? SafeFullName(this INamedTypeSymbol? symbol)
    {
        return FullName(symbol)?.Replace(".", "_").Replace(":", "_");
    }

    static string CollectTypeArguments(IReadOnlyList<ITypeSymbol> typeArguments)
    {
        var output = new List<string>();
        for (var i = 0; i < typeArguments.Count; i++)
        {
            switch (typeArguments[i])
            {
                case INamedTypeSymbol nts:
                    output.Add(FullName(nts)!);
                    break;
                case ITypeParameterSymbol tps:
                    output.Add(tps.Name + tps.NullableToken());
                    break;
                default:
                    throw new NotSupportedException(
                        $"Cannot generate type name from type argument {typeArguments[i].GetType().FullName}");
            }
        }

        return "<" + string.Join(", ", output) + ">";
    }


    /// <summary>
    /// Returns the full namespace for a sumbol.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <param name="includeGlobal">adds global:: prefix</param>
    /// <returns></returns>
    public static string FullNamespace(this ISymbol symbol, bool includeGlobal = true)
    {
        var parts = new Stack<string>();
        var iterator = (symbol as INamespaceSymbol) ?? symbol.ContainingNamespace;
        while (iterator != null)
        {
            if (!string.IsNullOrEmpty(iterator.Name))
                parts.Push(iterator.Name);
            iterator = iterator.ContainingNamespace;
        }

        return includeGlobal ? "global::" + string.Join(".", parts) : string.Join(".", parts);
    }

    /// <summary>
    /// Returns a list of scalar properties with setters.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static IEnumerable<IPropertySymbol> WritableScalarProperties(this INamedTypeSymbol symbol)
    {
        return symbol.GetMembers().OfType<IPropertySymbol>().Where(p => p.CanWrite() && !p.HasParameters());
    }

    /// <summary>
    /// Returns true if the property has a getter.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static bool CanRead(this IPropertySymbol symbol) => symbol.GetMethod != null;

    /// <summary>
    /// Returns true if the property has a setter.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static bool CanWrite(this IPropertySymbol symbol) => symbol.SetMethod != null;

    /// <summary>
    /// Returns true if the property has any parameters.
    /// </summary>
    /// <param name="symbol">The symbol being examined.</param>
    /// <returns></returns>
    public static bool HasParameters(this IPropertySymbol symbol) => symbol.Parameters.Any();
}