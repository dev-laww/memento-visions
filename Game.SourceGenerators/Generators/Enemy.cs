using Game.SourceGenerators.BaseGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Game.SourceGenerators.Generators;

[Generator]
public class Enemy : Inheritance
{
    protected override string BaseClassName => "Game.Entities.Enemies.Enemy";

    protected override string GenerateCode(INamedTypeSymbol symbol, ClassDeclarationSyntax classDecl) =>
        symbol.GeneratePartialClass(
            usings: ["Godot"],
            attributes: ["Tool", "Icon(\"res://assets/icons/enemy.svg\")"]
        );
}