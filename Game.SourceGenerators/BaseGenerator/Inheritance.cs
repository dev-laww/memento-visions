using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

#nullable enable
namespace Game.SourceGenerators.BaseGenerator;

public abstract class Inheritance : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a pipeline that provides BOTH syntax and semantic information
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax,
                transform: (ctx, _) => GetClassWithBaseType(ctx)
            )
            .Where(static c => c.HasValue)
            .Select(static (c, _) => c!.Value)
            .Collect();

        context.RegisterSourceOutput(
            classDeclarations,
            GenerateSource
        );
    }

    // Tuple to hold both syntax and symbol
    private (INamedTypeSymbol Symbol, ClassDeclarationSyntax Syntax)? GetClassWithBaseType(
        GeneratorSyntaxContext context
    )
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        // Check if the class inherits from the specified base class
        if (symbol?.BaseType?.ToDisplayString() == BaseClassName) return (symbol, classDecl);

        return null;
    }

    private void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<(INamedTypeSymbol Symbol, ClassDeclarationSyntax Syntax)> classes
    )
    {
        foreach (var (symbol, syntax) in classes)
        {
            var source = GenerateCode(symbol, syntax);
            context.AddSource(
                $"{symbol.ToDisplayString()}.g.cs",
                SourceText.From(source, Encoding.UTF8)
            );
        }
    }

    protected abstract string BaseClassName { get; }
    protected abstract string GenerateCode(INamedTypeSymbol symbol, ClassDeclarationSyntax classDecl);
}