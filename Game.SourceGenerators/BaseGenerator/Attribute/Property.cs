using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Game.SourceGenerators.BaseGenerator.Attribute;

public abstract class Property<TAttribute> : Member<TAttribute, PropertyDeclarationSyntax>
    where TAttribute : System.Attribute
{
    protected abstract (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        IPropertySymbol symbol,
        AttributeData attribute
    );

    protected sealed override (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute
    ) => GenerateCode(compilation, node, (IPropertySymbol)symbol, attribute);
}