using ClosedXML.Excel;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.Reports;
using System.Diagnostics;

namespace RentCarX.Application.Services.ReportingService;

public sealed class ExcelReport : IReportingService
{
    private readonly IReportingConfiguration _reportHelper;

    public ExcelReport(IReportingConfiguration helper)
    {
        _reportHelper = helper;
    }

    public DocumentReport DocumentReport => DocumentReport.Xlsx;

    public async Task<byte[]> GenerateReport(CancellationToken ct)
    {
        var reportData = await _reportHelper.SetReport(ct);

        using(var workbook = new XLWorkbook())
        {
            IXLWorksheet worksheet = workbook.Worksheets.Add($"RentCarX Sales Report");

            // adding headers
            worksheet.Cell("A1").Value = "CarId";
            worksheet.Cell("B1").Value = "Model";
            worksheet.Cell("C1").Value = "Price/Day";
            worksheet.Cell("D1").Value = "Total Reservations";
            worksheet.Cell("E1").Value = "Revenue";

            // heading styles
            var range = worksheet.Range("A1:E1");
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            range.Style.Font.FontColor = XLColor.White;

            int currentRow = 2;

            // data
            foreach (var car in reportData)
            {
                worksheet.Cell(currentRow, 1).Value = car.CarId.ToString(); 
                worksheet.Cell(currentRow, 2).Value = car.Model;
                worksheet.Cell(currentRow, 3).Value = car.PricePerDay;
                worksheet.Cell(currentRow, 4).Value = car.TotalReservations;
                worksheet.Cell(currentRow, 5).Value = car.Revenue;

                currentRow++;
            }

            // adjusting the column width
            worksheet.Columns().AdjustToContents();

            Debug.WriteLine("Preparing a report file to save as xlsx format");

            // safe file
            var folderPath = Path.GetTempPath(); 
            var filePath = Path.Combine(folderPath, $"RentCarX_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            Debug.WriteLine($"[DEV] Attempt to save the file: {filePath}");
            try
            {
                await Task.Run(() => workbook.SaveAs(filePath));
                Debug.Assert(File.Exists(filePath), $"[DEV] File was not created at path: {filePath}");
                return File.ReadAllBytesAsync(filePath).Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEV ERROR] Error ocurred while saving the file to xlsx format: {ex.Message}");
                throw;
            }
        }
    }
}
