using System;

namespace Game.CaseExtensions;

public static partial class StringExtensions
{
    public static string ToKebabCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '-',
            (s, disableFrontDelimeter) => disableFrontDelimeter
                ? [char.ToLowerInvariant(s)]
                : ['-', char.ToLowerInvariant(s)]);
    }
}