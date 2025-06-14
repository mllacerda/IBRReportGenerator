using System.Net.Http.Json;
using System.Text;

namespace ReportGenerator.Worker.Services;

public class WebhookService
{
    private readonly HttpClient _httpClient;

    public WebhookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
            Console.WriteLine($"Webhook sent to {webhookUrl} for report {reportId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send webhook to {webhookUrl}: {ex.Message}");
        }
    }
}