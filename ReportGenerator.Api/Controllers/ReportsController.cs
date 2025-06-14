
using Microsoft.AspNetCore.Mvc;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;

namespace ReportGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMessageQueueService _messageQueueService;

    public ReportsController(IMessageQueueService messageQueueService)
    {
        _messageQueueService = messageQueueService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] ReportRequest request)
    {
        if (string.IsNullOrEmpty(request.ReportId) || string.IsNullOrEmpty(request.WebhookUrl))
        {
            return BadRequest("ReportId and WebhookUrl are required.");
        }

        if (!Uri.TryCreate(request.WebhookUrl, UriKind.Absolute, out _))
        {
            return BadRequest("Invalid WebhookUrl format.");
        }

        //Publicando
        await _messageQueueService.PublishReportRequestAsync(request);

        //OK
        return Accepted(new { Message = $"Report {request.ReportId} queued for processing." });
    }
}