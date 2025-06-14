using System.Net.Http.Json;
using System.Text;

namespace ReportGenerator.Worker.Services;

public class WebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendWebhookAsync(string webhookUrl, string reportId, byte[] pdfBytes, bool success, string message)
    {
        var payload = new
        {
            ReportId = reportId,
            Status = success ? "Success" : "Error",
            PdfBase64 = Convert.ToBase64String(pdfBytes),
            Message = message
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Webhook sent to {WebhookUrl} for report {ReportId}", webhookUrl, reportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook to {WebhookUrl} for report {ReportId}", webhookUrl, reportId);
        }
    }
}