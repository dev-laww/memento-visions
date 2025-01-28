using Microsoft.CodeAnalysis;

namespace Game.SourceGenerators;

internal abstract class MemberDataModel : BaseDataModel
{
    protected MemberDataModel(ISymbol symbol) : base(symbol, symbol.ContainingType) { }
}