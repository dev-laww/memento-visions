﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Game.SourceGenerators.BaseGenerator.Attribute;

public abstract class Member<TAttribute, TDeclarationSyntax> : IIncrementalGenerator
    where TAttribute : System.Attribute
    where TDeclarationSyntax : MemberDeclarationSyntax
{
    private static readonly string attributeType = typeof(TAttribute).Name;

    private static readonly string attributeName = Regex.Replace(
        attributeType,
        "Attribute$",
        "",
        RegexOptions.Compiled
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(IsSyntaxTarget, GetSyntaxTarget);
        var compilationProvider = context.CompilationProvider.Combine(syntaxProvider.Collect());
        context.RegisterSourceOutput(compilationProvider, (c, s) => OnExecute(s.Right, s.Left, c));
        return;

        static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
        {
            return node is TDeclarationSyntax type && HasAttributeType();

            bool HasAttributeType()
            {
                return type.AttributeLists.Count is not 0 && type.AttributeLists
                    .SelectMany(attributeList => attributeList.Attributes)
                    .Any(attribute => attribute.Name.ToString() == attributeName);
            }
        }

        static TDeclarationSyntax GetSyntaxTarget(GeneratorSyntaxContext context, CancellationToken _)
            => (TDeclarationSyntax)context.Node;

        void OnExecute(
            ImmutableArray<TDeclarationSyntax> nodes,
            Compilation compilation,
            SourceProductionContext context
        )
        {
            try
            {
                foreach (var node in nodes.Distinct())
                {
                    if (context.CancellationToken.IsCancellationRequested)
                        return;

                    var model = compilation.GetSemanticModel(node.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(Node(node));
                    var attribute = symbol?
                        .GetAttributes()
                        .SingleOrDefault(x => x.AttributeClass?.Name == attributeType);
                    if (attribute is null) continue;

                    var (generatedCode, error) = _GenerateCode(compilation, node, symbol, attribute);

                    if (generatedCode is null)
                    {
                        var descriptor = new DiagnosticDescriptor(
                            error.Id ?? attributeName,
                            error.Title,
                            error.Message,
                            error.Category ?? "Usage", DiagnosticSeverity.Error,
                            true
                        );

                        if (attribute.ApplicationSyntaxReference is null) continue;


                        var diagnostic = Diagnostic.Create(
                            descriptor,
                            attribute.ApplicationSyntaxReference.GetSyntax().GetLocation()
                        );
                        context.ReportDiagnostic(diagnostic);

                        continue;
                    }

                    context.AddSource(GenerateFilename(symbol), generatedCode);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }

    protected abstract (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute
    );

    private (string GeneratedCode, DiagnosticDetail Error) _GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute
    )
    {
        try
        {
            return GenerateCode(compilation, node, symbol, attribute);
        }
        catch (Exception e)
        {
            Log.Error(e);
            return (null, InternalError(e));
        }

        static DiagnosticDetail InternalError(Exception e) => new() { Title = "Internal Error", Message = e.Message };
    }

    protected virtual string GenerateFilename(ISymbol symbol) =>
        $"{string.Join("_", $"{symbol}".Split(Path.GetInvalidFileNameChars()))}.g.cs";

    protected virtual SyntaxNode Node(TDeclarationSyntax node) => node;
}