using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Jobs;

/// <summary>
/// A background service that processes a queue of car reservation requests.
/// </summary>
/// <remarks>This service continuously monitors a queue of car reservation IDs and processes them asynchronously.
/// It uses a semaphore to signal when new items are added to the queue. If an error occurs during the processing of a
/// reservation, the item is re-enqueued for retry.</remarks>
public sealed class ReservationQueueWorker : BackgroundService
{
    private readonly ConcurrentQueue<Guid> _reservationQueue;
    private readonly SemaphoreSlim _signal = new(0);

    public ReservationQueueWorker(ConcurrentQueue<Guid> reservationQueue)
    {
        _reservationQueue = reservationQueue;
    }

    public void Enqueue(Guid carId)
    {
        _reservationQueue.Enqueue(carId);
        _signal.Release();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            while(_reservationQueue.TryDequeue(out Guid carId))
            {
                try
                {
                    // telemetry todo
                    Console.WriteLine($"Processing vehicleId: {carId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {carId}: {ex.Message}");
                    _reservationQueue.Enqueue(carId);
                    _signal.Release();
                }
            }
        }
    }
}

