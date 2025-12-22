using RentCarX.Application.DTOs.Car;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Interfaces.Services.Reports;

public interface IReportingConfiguration
{
    Task<ConcurrentBag<ReportCarDto>> SetReport(CancellationToken ct);
}
