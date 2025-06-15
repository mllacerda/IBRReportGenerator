using System.Threading.Tasks;

namespace ReportGenerator.Worker.Services;

public interface IWebhookService
{
    Task SendWebhookAsync(string webhookUrl, string reportId, byte[] pdfBytes, bool success, string message);
}