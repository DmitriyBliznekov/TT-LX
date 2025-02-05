﻿using Microsoft.AspNetCore.Mvc;
using Server.BackgroundServices;

namespace Server.Controllers;

[ApiController]
public class SignalRController : ControllerBase
{
    private readonly SendRowBackgroundService _sendRowService;

    public SignalRController(SendRowBackgroundService sendRowService) 
    { 
        _sendRowService = sendRowService;
    }

    [HttpGet("/activate_signalr")]
    public async Task<ActionResult> StartSendRow()
    {
        await _sendRowService.StartAsync(CancellationToken.None);

        return Ok();
    }

    [HttpGet("/deactivate_signalr")]
    public async Task<ActionResult> StopSendRow()
    {
        await _sendRowService.StopAsync(CancellationToken.None);

        return Ok();
    }
}