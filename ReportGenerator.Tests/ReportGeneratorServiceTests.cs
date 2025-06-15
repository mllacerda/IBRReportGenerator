using QuestPDF.Infrastructure;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Services;
using Xunit;

namespace ReportGenerator.Tests;

public class ReportGeneratorServiceTests
{
    [Fact]
    public void GenerateReportPdf_ValidRequestWithDictionary_ReturnsPdfBytes()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var service = new ReportGeneratorService();
        var request = new ReportRequest
        {
            ReportId = "123",
            WebhookUrl = "https://example.com/webhook",
            Parameters = new Dictionary<string, object> { { "key1", "value1" }, { "key2", 42 } }
        };

        // Act
        var pdfBytes = service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF should have content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithObject_ReturnsPdfBytes()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var service = new ReportGeneratorService();
        var request = new ReportRequest
        {
            ReportId = "123",
            WebhookUrl = "https://example.com/webhook",
            Parameters = new { Name = "Test", Age = 30 }
        };

        // Act
        var pdfBytes = service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithBase64Image_ReturnsPdfBytes()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var service = new ReportGeneratorService();
        
        var base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8cX3QAAAABJRU5ErkJggg==";
        var request = new ReportRequest
        {
            ReportId = "123",
            WebhookUrl = "https://example.com/webhook",
            Parameters = base64Image
        };

        // Act
        var pdfBytes = service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF should have content");
    }

    [Fact]
    public void GenerateReportPdf_NullParameters_ReturnsPdfBytes()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var service = new ReportGeneratorService();
        var request = new ReportRequest
        {
            ReportId = "123",
            WebhookUrl = "https://example.com/webhook",
            Parameters = null
        };

        // Act
        var pdfBytes = service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }
}