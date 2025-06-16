using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Extensions;
using System.Data.Common;
using System.Text;
using System.Text.Json;

namespace ReportGenerator.Worker.Services;

public class ReportGeneratorService : IReportGeneratorService
{
    private record FormatResult(string Text = null, byte[] ImageBytes = null)
    {
        public bool IsImage => ImageBytes != null;
    }

    public byte[] GenerateReportPdf(ReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                // Header
                page.Header()
                    .Text($"Report: {request.ReportId}")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                // Content
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text("Report Parameters").FontSize(16).Bold();

                        if (request.Parameters == null)
                        {
                            column.Item().Text("No parameters provided");
                        }
                        else if (request.Parameters is string base64String && base64String.IsBase64Image())
                        {
                            try
                            {
                                var imageBytes = base64String.ToImageBytes();
                                column.Item().Image(imageBytes).FitWidth();
                            }
                            catch (Exception ex)
                            {
                                column.Item().Text($"Error rendering image: {ex.Message}");
                            }
                        }
                        else if (request.Parameters is IDictionary<string, object> dict)
                        {
                            RenderDictionary(column, dict, indent: 0);
                        }
                        else if (request.Parameters is IEnumerable<object> list && !(request.Parameters is string))
                        {
                            RenderList(column, list, indent: 0);
                        }
                        else if (request.Parameters is DateTime dt)
                        {
                            column.Item().Text($"Date: {dt:yyyy-MM-dd HH:mm:ss}");
                        }
                        else
                        {
                            try
                            {
                                column.Item().Text($"Parameters: {JsonSerializer.Serialize(request.Parameters)}");
                            }
                            catch
                            {
                                column.Item().Text("Invalid or non-serializable parameters");
                            }
                        }
                    });

                // Footer
                page.Footer()
                    .AlignCenter()
                    .Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private void RenderDictionary(ColumnDescriptor column, IDictionary<string, object> dict, int indent)
    {
        foreach (var param in dict)
        {
            var formatResult = FormatValue(param.Value, indent + 1);
            if (formatResult.IsImage)
            {
                try
                {
                    column.Item().Text($"{new string(' ', indent * 2)}{param.Key}:");
                    column.Item().PaddingLeft((indent + 1) * 10).Image(formatResult.ImageBytes).FitWidth();
                }
                catch (Exception ex)
                {
                    column.Item().Text($"{new string(' ', indent * 2)}{param.Key}: Error rendering image: {ex.Message}");
                }
            }
            else if (param.Value is IDictionary<string, object> nestedDict)
            {
                column.Item().Text($"{new string(' ', indent * 2)}{param.Key}:");
                RenderDictionary(column, nestedDict, indent + 1);
            }
            else if (param.Value is IEnumerable<object> nestedList && !(param.Value is string))
            {
                column.Item().Text($"{new string(' ', indent * 2)}{param.Key}:");
                RenderList(column, nestedList, indent + 1);
            }
            else
            {
                column.Item().Text($"{new string(' ', indent * 2)}{param.Key}: {formatResult.Text}");
            }
        }
    }

    private void RenderList(ColumnDescriptor column, IEnumerable<object> list, int indent)
    {
        int index = 1;
        foreach (var item in list)
        {
            var formatResult = FormatValue(item, indent + 1);
            if (formatResult.IsImage)
            {
                try
                {
                    column.Item().Text($"{new string(' ', indent * 2)}Item {index++}:");
                    column.Item().PaddingLeft((indent + 1) * 10).Image(formatResult.ImageBytes).FitWidth();
                }
                catch (Exception ex)
                {
                    column.Item().Text($"{new string(' ', indent * 2)}Item {index++}: Error rendering image: {ex.Message}");
                }
            }
            else if (item is IDictionary<string, object> nestedDict)
            {
                column.Item().Text($"{new string(' ', indent * 2)}Item {index++}:");
                RenderDictionary(column, nestedDict, indent + 1);
            }
            else if (item is IEnumerable<object> nestedList && !(item is string))
            {
                column.Item().Text($"{new string(' ', indent * 2)}Item {index++}:");
                RenderList(column, nestedList, indent + 1);
            }
            else
            {
                column.Item().Text($"{new string(' ', indent * 2)}Item {index++}: {formatResult.Text}");
            }
        }
    }

    private FormatResult FormatValue(object value, int indent = 0)
    {
        if (value == null)
            return new FormatResult("null");

        if (value is string s)
        {
            if (s.IsBase64Image())
            {
                try
                {
                    return new FormatResult(ImageBytes: s.ToImageBytes());
                }
                catch
                {
                    return new FormatResult($"Invalid Base64 image: {s[..Math.Min(20, s.Length)]}...");
                }
            }
            return new FormatResult(s);
        }

        if (value is DateTime dt)
            return new FormatResult(dt.ToString("yyyy-MM-dd HH:mm:ss"));

        if (value is IDictionary<string, object> || value is IEnumerable<object> && !(value is string))
        {
            return new FormatResult(""); // Rendering handled by RenderDictionary or RenderList
        }

        try
        {
            return new FormatResult(JsonSerializer.Serialize(value));
        }
        catch
        {
            return new FormatResult(value.ToString());
        }
    }
}
