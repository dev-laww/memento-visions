using System;
using System.Linq;
using System.Text;

namespace Game.CaseExtensions;

public static partial class StringExtensions
{
    private static readonly char[] Delimiters = { ' ', '-', '_' };

    private static string SymbolsPipe(
        string source,
        char mainDelimeter,
        Func<char, bool, char[]> newWordSymbolHandler)
    {
        var builder = new StringBuilder();

        bool nextSymbolStartsNewWord = true;
        bool disableFrontDelimeter = true;
        for (var i = 0; i < source.Length; i++)
        {
            var symbol = source[i];
            if (Delimiters.Contains(symbol))
            {
                if (symbol == mainDelimeter)
                {
                    builder.Append(symbol);
                    disableFrontDelimeter = true;
                }

                nextSymbolStartsNewWord = true;
            }
            else if (!char.IsLetter(symbol))
            {
                builder.Append(symbol);
                disableFrontDelimeter = true;
                nextSymbolStartsNewWord = true;
            }
            else
            {
                if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                {
                    builder.Append(newWordSymbolHandler(symbol, disableFrontDelimeter));
                    disableFrontDelimeter = false;
                    nextSymbolStartsNewWord = false;
                }
                else
                {
                    builder.Append(symbol);
                }
            }
        }

        return builder.ToString();
    }
}