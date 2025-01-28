using Microsoft.CodeAnalysis;

namespace Game.SourceGenerators;

internal abstract class ClassDataModel : BaseDataModel
{
    protected ClassDataModel(INamedTypeSymbol symbol) : base(symbol, symbol) { }
}