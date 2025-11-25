using QuestPDF.Fluent;
using QuestPDF.Helpers;
using RentCarX.Application.DTOs.Car;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.Reports;
using RentCarX.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RentCarX.Application.Services.ReportingService;

public sealed class PdfReport : IReportingService
{
    private readonly ReportHelper _reportHelper;

    public PdfReport(ReportHelper helper)
    {
        _reportHelper = helper;
    }

    public async Task GenerateReport(CancellationToken ct)
    {
        var reportData = await _reportHelper.SetReport(ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("RentCarX cars sales report")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("CarId");
                            header.Cell().Text("Model");
                            header.Cell().Text("Price/Day");
                            header.Cell().Text("Total Reservations");
                            header.Cell().Text("Revenue");
                        });

                        foreach (var car in reportData.OrderBy(c => c.Model))
                        {
                            table.Cell().Text(car.CarId.ToString());
                            table.Cell().Text(car.Model);
                            table.Cell().Text(car.PricePerDay.ToString("C"));
                            table.Cell().Text(car.TotalReservations.ToString());
                            table.Cell().Text(car.Revenue.ToString("C"));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
            });
        });

        var folderPath = Path.GetTempPath();
        var filePath = Path.Combine(folderPath, $"RentCarX_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        try
        {
            Debug.WriteLine("Preparing to generate the pdf file...");
            await Task.Run(() => document.GeneratePdf(filePath));
            Debug.Assert(File.Exists(filePath), $"[DEV] File was not created at path: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DEV ERROR] Error ocurred while saving the file to pdf format: {ex.Message}");
            throw;
        }
    }
}
