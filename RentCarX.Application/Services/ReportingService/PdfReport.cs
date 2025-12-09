using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentCarX.Application.Interfaces.Services.Reports;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;

namespace RentCarX.Application.Services.ReportingService;

public class ReservationsDocumentModel
{
    public string Title { get; set; } = string.Empty;
    public List<Reservation> Reservations { get; set; } = new();
}

public class PdfReport : IReportingService
{
    private readonly IReservationRepository _reservationRepository;

    public PdfReport(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
        // Set the QuestPDF license type
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // strategy pattern
    public DocumentReport DocumentReport => DocumentReport.Pdf;

    public async Task<byte[]> GenerateReport(CancellationToken ct)
    {
        var reservations = await _reservationRepository.GetAll()
            .Include(r => r.Car)
            .Include(r => r.User)
            .ToListAsync(ct);

        var model = new ReservationsDocumentModel
        {
            Title = "Reservations Report",
            Reservations = reservations
        };

        var document = new ReservationsDocument(model);

        // Generate the PDF document in memory
        byte[] pdfBytes = await Task.Run(() => document.GeneratePdf(), ct);

        return pdfBytes;
    }
}

public class ReservationsDocument : IDocument
{
    public ReservationsDocumentModel Model { get; }

    public ReservationsDocument(ReservationsDocumentModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(Model.Title).Style(titleStyle);
                column.Item().Text(DateTime.Now.ToString("g"));
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(20);
            column.Item().Element(ComposeTable);
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            // columns
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            // header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("User");
                header.Cell().Element(CellStyle).Text("Car");
                header.Cell().Element(CellStyle).Text("Start Date");
                header.Cell().Element(CellStyle).Text("End Date");
                header.Cell().Element(CellStyle).Text("Total Cost");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // reservations
            foreach (var reservation in Model.Reservations)
            {
                table.Cell().Element(CellStyle).Text(reservation.User?.Email ?? "N/A");
                table.Cell().Element(CellStyle).Text($"{reservation.Car?.Brand} {reservation.Car?.Model}");
                table.Cell().Element(CellStyle).Text(reservation.StartDate.ToString("d"));
                table.Cell().Element(CellStyle).Text(reservation.EndDate.ToString("d"));
                table.Cell().Element(CellStyle).Text($"{reservation.TotalCost:C}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }
}