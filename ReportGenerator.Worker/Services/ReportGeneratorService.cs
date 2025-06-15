using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Extensions;
using System.Text.Json;

namespace ReportGenerator.Worker.Services;

public class ReportGeneratorService: IReportGeneratorService
{
    public byte[] GenerateReportPdf(ReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return Document.Create(container =>
        {
            //Cria PDF
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                // Cabeçalho
                page.Header()
                    .Text($"Relatório: {request.ReportId}")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                // Conteúdo
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text("Parâmetros do Relatório").FontSize(16).Bold();

                        if (request.Parameters == null)
                        {
                            column.Item().Text(string.Empty);
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
                        else if (request.Parameters is Dictionary<string, object> dict)
                        {
                            foreach (var param in dict)
                            {
                                column.Item().Text($"{param.Key}: {param.Value ?? "null"}");
                            }
                        }
                        else if (request.Parameters is IEnumerable<object> list)
                        {
                            int index = 1;
                            foreach (var item in list)
                            {
                                column.Item().Text($"Item {index++}: {item ?? "null"}");
                            }
                        }
                        else if (request.Parameters is DateTime dt)
                        {
                            column.Item().Text($"Date: {dt:yyyy-MM-dd HH:mm:ss}");
                        }
                        else
                        {
                            column.Item().Text($"Parameters: {JsonSerializer.Serialize(request.Parameters)}");
                        }

                    });

                // Rodapé
                page.Footer()
                    .AlignCenter()
                    .Text($"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }
}