using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Game.SourceGenerators.BaseGenerator.Attribute;

public abstract class Field<TAttribute> : Member<TAttribute, FieldDeclarationSyntax> where TAttribute : System.Attribute
{
    protected abstract (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        IFieldSymbol symbol,
        AttributeData attribute
    );

    protected sealed override (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute
    ) => GenerateCode(compilation, node, (IFieldSymbol)symbol, attribute);

    protected override SyntaxNode Node(FieldDeclarationSyntax node) => node.Declaration.Variables.Single();
}