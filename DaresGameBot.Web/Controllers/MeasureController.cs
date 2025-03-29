using DaresGameBot.Cpu;
using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DaresGameBot.Web.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class MeasureController : ControllerBase
{
    public MeasureController(Cpu.Timer cpuTimer) => _cpuTimer = cpuTimer;

    [HttpGet("snapshot")]
    public IActionResult Snapshot([FromServices] Bot bot)
    {
        Result botSnapshot = bot.CpuTimer.Snapshot();
        Result webappSnapshot = _cpuTimer.Snapshot();
        CpuMeasureResult result = new(botSnapshot, webappSnapshot);
        return Ok(result);
    }

    private readonly Cpu.Timer _cpuTimer;
}