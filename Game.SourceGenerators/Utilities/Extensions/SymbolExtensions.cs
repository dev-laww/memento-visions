﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Game.SourceGenerators;

public static class SymbolExtensions
{
    public static string NamespaceOrNull(this ISymbol symbol)
        => symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : string.Join(".", symbol.ContainingNamespace.ConstituentNamespaces);

    public static (string NamespaceDeclaration, string NamespaceClosure, string NamespaceIndent)
        GetNamespaceDeclaration(this ISymbol symbol, string indent = "    ")
    {
        var ns = symbol.NamespaceOrNull();
        return ns is null
            ? (null, null, null)
            : ($"namespace {ns}\n{{\n", "}\n", indent);
    }

    public static INamedTypeSymbol OuterType(this ISymbol symbol)
        => symbol.ContainingType?.OuterType() ?? symbol as INamedTypeSymbol;

    public static string ClassDef(this INamedTypeSymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public static string GeneratePartialClass(
        this INamedTypeSymbol symbol,
        IEnumerable<string> content = null,
        IEnumerable<string> usings = null,
        IEnumerable<string> attributes = null
    )
    {
        var (nsOpen, nsClose, nsIndent) = symbol.GetNamespaceDeclaration();

        return $$"""
                 {{(usings is null ? "" : string.Join("\n", usings.Select(u => $"using {u};")))}}

                 {{nsOpen?.Trim()}}
                 {{nsIndent}}{{(attributes is null ? "" : string.Join("\n", attributes.Select(a => $"[{a}]")))}}
                 {{nsIndent}}partial class {{symbol.ClassDef()}}
                 {{nsIndent}}{
                 {{nsIndent}}    {{string.Join($"\n{nsIndent}    ", content ?? [])}}
                 {{nsIndent}}}
                 {{nsClose?.Trim()}}

                 """.TrimStart();
    }

    public static bool InheritsFrom(this ITypeSymbol symbol, string type)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == type)
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }
}