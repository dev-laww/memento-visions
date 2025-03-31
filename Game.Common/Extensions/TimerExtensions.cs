using Timer = Godot.Timer;

namespace Game.Common.Extensions;

public static class TimerExtensions
{
    public static void Pause(this Timer timer)
    {
        if (timer.IsStopped()) return;

        timer.Paused = true;
    }

    public static void Resume(this Timer timer)
    {
        if (!timer.Paused) return;

        timer.Paused = false;
    }
}