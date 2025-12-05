using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Interfaces.Services.Reports;

public interface IReportingService
{
    Task GenerateReport(CancellationToken ct);
}
