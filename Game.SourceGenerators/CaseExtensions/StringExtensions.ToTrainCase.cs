using System;

namespace Game.CaseExtensions;

public static partial class StringExtensions
{
    public static string ToTrainCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '-',
            (s, disableFrontDelimeter) =>
                disableFrontDelimeter ? [char.ToUpperInvariant(s)] : ['-', char.ToUpperInvariant(s)]);
    }
}