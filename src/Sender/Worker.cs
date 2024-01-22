using Microsoft.Extensions.Hosting;
using Messages;
using NServiceBus;

namespace Sender;
public class Worker(IHostApplicationLifetime lifetime, IMessageSession session) : IHostedService
{
  const int MaximumWaitInSeconds = 10;
  public DateTime LastPong { get; set; } = DateTime.MinValue;
  public DateTime LastPing { get; set; } = DateTime.MinValue;

  public int LastPingNumber { get; set; } = 0;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    return Task.Run(async () =>
    {

      Console.WriteLine("Press any key to quit");

      var running = true;

      while (running)
      {
        var utcNow = DateTime.UtcNow;

        if ((utcNow - LastPing).TotalSeconds > MaximumWaitInSeconds && (utcNow - LastPong).TotalSeconds > MaximumWaitInSeconds)
        {
          await session.Send(new Ping()
          {
            PingNumber = LastPingNumber + 1,
            Timestamp = DateTime.UtcNow
          });
          LastPing = utcNow;
        }

        if (Console.KeyAvailable)
        {
          running = false;
        }
        else
        {
          await Task.Delay(1000, lifetime.ApplicationStopping);
        }
      }
    }, cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}

public class PongHandler2(Worker worker) : IHandleMessages<Pong>
{
  public Task Handle(Pong message, IMessageHandlerContext context)
  {
    worker.LastPong = DateTime.UtcNow;
    worker.LastPing = message.OriginalTimeStamp;
    worker.LastPingNumber = Math.Max(worker.LastPingNumber, message.PingNumber);

    return Task.CompletedTask;
  }
}
