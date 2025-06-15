using ReportGenerator.Domain.Models;

namespace ReportGenerator.Worker.Services;

public interface IReportGeneratorService
{
    byte[] GenerateReportPdf(ReportRequest request);
}