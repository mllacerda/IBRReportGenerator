using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ReportGenerator.Worker.Services;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ReportGenerator.Tests;

public class WebhookServiceTests
{
    private readonly Mock<ILogger<WebhookService>> _loggerMock;
    private readonly WebhookService _service;
    private const string TestWebhookUrl = "https://test.com/webhook";

    public WebhookServiceTests()
    {
        _loggerMock = new Mock<ILogger<WebhookService>>();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://test.com") };
        _service = new WebhookService(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task SendWebhookAsync_SuccessfulRequest_LogsSuccess()
    {
        // Arrange
        var reportId = TestConstants.ReportIdTest;
        var pdfBytes = TestConstants.PdfBytesTest;
        var message = "Success";

        // Act
        await _service.SendWebhookAsync(TestWebhookUrl, reportId, pdfBytes, true, message);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Information, $"Webhook sent to {TestWebhookUrl} for report {reportId}", Times.Once());
    }

    [Fact]
    public async Task SendWebhookAsync_FailedRequest_LogsError()
    {
        // Arrange
        var reportId = TestConstants.ReportIdTest;
        var pdfBytes = TestConstants.PdfBytesTest;
        var message = "Success";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ThrowsAsync(new HttpRequestException("Failed"));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(TestConstants.UrlTest) };
        var service = new WebhookService(httpClient, _loggerMock.Object);

        // Act
        await service.SendWebhookAsync(TestWebhookUrl, reportId, pdfBytes, true, message);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Failed to send webhook to {TestWebhookUrl} for report {reportId}", Times.Once());
    }
}