using System.Linq.Expressions;
using System.Reflection;
using ApiRoutes.Utilities;
using FluentValidation.Internal;

namespace ApiRoutes;

/// <summary>
/// Convert property names to camelCase as Asp.Net Core does 
/// https://github.com/FluentValidation/FluentValidation/issues/226
/// </summary>
public static class CamelCasePropertyNameResolver
{
    public static string? ResolvePropertyName(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        var s = DefaultPropertyNameResolver(type, memberInfo, expression);
        return string.IsNullOrEmpty(s) ? null : StringUtility.ToCamelCase(s);
    }

    private static string? DefaultPropertyNameResolver(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        if (expression != null)
        {
            var chain = PropertyChain.FromExpression(expression);
            if (chain.Count > 0)
            {
                return chain.ToString();
            }
        }

        if (memberInfo != null)
        {
            return memberInfo.Name;
        }

        return null;
    }
}