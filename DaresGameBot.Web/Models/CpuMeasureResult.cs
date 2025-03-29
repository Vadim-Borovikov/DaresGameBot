using DaresGameBot.Cpu;
using JetBrains.Annotations;

namespace DaresGameBot.Web.Models;

[PublicAPI]
public sealed class CpuMeasureResult
{
    [UsedImplicitly]
    public Result Bot { get; set; }

    [UsedImplicitly]
    public Result WebApp { get; set; }

    public CpuMeasureResult(Result bot, Result webApp)
    {
        Bot = bot;
        WebApp = webApp;
    }
}