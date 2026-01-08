using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace RentCarX.Presentation.Observability.Prometheus;

public static class ApplicationMetrics
{
    public static readonly Meter Meter = new("RentCarX");
    public static readonly ActivitySource ActivitySource = new("RentCarX");

    public static readonly Counter<long> ReservationCreated =
        Meter.CreateCounter<long>("reservations_created_total");

    public static readonly Counter<long> PaymentProcessed =
        Meter.CreateCounter<long>("payments_processed_total");

    public static readonly Counter<long> ReservationCancelled =
        Meter.CreateCounter<long>("reservations_cancelled_total");

    public static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>("request_duration_ms");

}
