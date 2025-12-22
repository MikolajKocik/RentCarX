using RentCarX.Application.Services.ReportingService;

namespace RentCarX.Application.Interfaces.Services.Reports;

public interface IReportingService
{   
    public DocumentReport DocumentReport { get; }
    Task<byte[]> GenerateReport(CancellationToken ct);
}
