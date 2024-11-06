using DelaunatorSharp;

namespace Game.Generation;

public class DelaunayPoint : IPoint
{
    public double X { get; set; }
    public double Y { get; set; }

    public DelaunayPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}