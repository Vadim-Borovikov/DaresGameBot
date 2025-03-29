using JetBrains.Annotations;

namespace DaresGameBot.Cpu;

[PublicAPI]
public sealed class Result
{
    [UsedImplicitly]
    public double MillisecondsTotal { get; set; }

    [UsedImplicitly]
    public double MillisecondsSinceLastSnapshot { get; set; }

    public Result(double millisecondsTotal, double millisecondsSinceLastSnapshot)
    {
        MillisecondsTotal = millisecondsTotal;
        MillisecondsSinceLastSnapshot = millisecondsSinceLastSnapshot;
    }
}