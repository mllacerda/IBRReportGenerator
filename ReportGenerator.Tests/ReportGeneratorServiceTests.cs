using QuestPDF.Infrastructure;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Services;
using System.Text.Json;
using Xunit;

namespace ReportGenerator.Tests;

public class ReportGeneratorServiceTests
{
    private readonly ReportGeneratorService _service;

    public ReportGeneratorServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _service = new ReportGeneratorService();
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithDictionary_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new Dictionary<string, object> { { "key1", "value1" }, { "key2", 42 } }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF should have content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithComplexDictionary_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new Dictionary<string, object>
            {
                { "name", "João Silva" },
                { "age", 30 },
                { "isActive", true },
                { "balance", 1234.56 },
                { "createdAt", "2025-06-16T12:00:00Z" },
                { "tags", new List<string> { "urgent", "priority" } }
            }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "PDF should have significant content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithNestedDictionary_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new Dictionary<string, object>
            {
                {
                    "customer", new Dictionary<string, object>
                    {
                        { "id", "CUST123" },
                        { "name", "Maria Oliveira" },
                        { "email", "maria@example.com" }
                    }
                },
                {
                    "order", new Dictionary<string, object>
                    {
                        { "orderId", 456 },
                        { "total", 789.99 },
                        { "items", new List<string> { "item1", "item2" } }
                    }
                },
                { "status", "pending" }
            }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "PDF should have significant content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithLargeDictionary_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new Dictionary<string, object>
            {
                { "projectName", "Sistema ReportGen" },
                { "version", "1.0.0" },
                { "owner", "Equipe Dev" },
                { "startDate", "2025-01-01" },
                { "endDate", "2025-12-31" },
                { "budget", 50000.00 },
                { "progress", 75 },
                { "priority", "high" },
                { "teamMembers", new List<string> { "Alice", "Bob", "Charlie" } },
                { "notes", "Projeto em fase de testes finais" },
                { "approved", true },
                { "lastUpdated", "2025-06-16T10:00:00Z" }
            }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "PDF should have significant content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithNestedDictionaryAndImage_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new Dictionary<string, object>
            {
                {
                    "client", new Dictionary<string, object>
                    {
                        { "id", "CLI123" },
                        { "name", "Joao Oliveira" },
                        { "email", "joao@example.com" }
                    }
                },
                {
                    "order", new Dictionary<string, object>
                    {
                        { "orderId", 456 },
                        { "total", 789.99 },
                        { "items", new List<string> { "item1", "item2" } },
                        {
                            "customer", new Dictionary<string, object>
                            {
                                { "id", "CUST123" },
                                { "name", "Maria Oliveira" },
                                { "email", "maria@example.com" },
                                { "image", TestConstants.Base64ImageTest }
                            }
                        }
                    }
                },
                { "status", "paused" }
            }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "PDF should have significant content");
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithObject_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = new { Name = "Test", Age = 30 }
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    [Fact]
    public void GenerateReportPdf_ValidRequestWithBase64Image_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = TestConstants.Base64ImageTest
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF should have content");
    }

    [Fact]
    public void GenerateReportPdf_NullParameters_ReturnsPdfBytes()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = null
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    [Fact]
    public void GenerateReportPdf_JsonElementParameters_ReturnsPdfBytes()
    {
        // Arrange
        var json = @"{""key1"": ""value1"", ""key2"": 42}";
        var parameters = JsonSerializer.Deserialize<object>(json);
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = parameters
        };

        // Act
        var pdfBytes = _service.GenerateReportPdf(request);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF should have content");
    }
}