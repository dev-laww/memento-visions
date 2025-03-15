
using System;
using static Game.UI.Screens.Loading;

namespace Game.Utils.Extensions;

public static class TransitionExtensions
{
    public static string ToAnimation(this Transition transition) => transition switch
    {
        Transition.Fade => "fade",
        _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, null)
    };
}