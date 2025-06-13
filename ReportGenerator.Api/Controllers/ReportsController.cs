using Microsoft.AspNetCore.Mvc;
using ReportGenerator.Domain.Models;

namespace ReportGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
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

        // TODO: Enviar a solicitação para o RabbitMQ
        return Accepted(new { Message = $"Report {request.ReportId} queued for processing." });
    }
}