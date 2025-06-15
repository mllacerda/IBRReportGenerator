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
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = TestConstants.DictionaryTwoValuesTest
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
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = TestConstants.ObjectTest
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

        var base64Image = TestConstants.Base64ImageTest;
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
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
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = null
        };

        // Act
        var pdfBytes = service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }
}