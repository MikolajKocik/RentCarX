using RentCarX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.HangfireWorker;

public interface IJobScheduler
{
    void SetJob(Reservation reservation);
}
