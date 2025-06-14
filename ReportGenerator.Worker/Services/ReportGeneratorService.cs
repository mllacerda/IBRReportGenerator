using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportGenerator.Domain.Models;
using System;

namespace ReportGenerator.Worker.Services;

public class ReportGeneratorService
{
    public byte[] GenerateReportPdf(ReportRequest request)
    {
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
                        foreach (var param in request.Parameters)
                        {
                            column.Item().Text($"{param.Key}: {param.Value}");
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