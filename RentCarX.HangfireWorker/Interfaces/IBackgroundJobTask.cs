using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.HangfireWorker.Interfaces;

public interface IBackgroundJobTask 
{
    Task PerformJobAsync(CancellationToken cancellationToken);
}
