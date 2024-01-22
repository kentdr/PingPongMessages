using Messages;

using NServiceBus;

namespace Receiver;
public class PingHandler : IHandleMessages<Ping>
{
  public async Task Handle(Ping message, IMessageHandlerContext context)
  {
    DateTime now = DateTime.UtcNow;

    Console.WriteLine($"Ping {message.PingNumber} received in {(now - message.Timestamp).TotalMilliseconds:0}ms");

    if(Random.Shared.Next(100) < 10)
    {
      throw new Exception("Simulated exception");
    }

    await Task.Delay(1000, context.CancellationToken);

    await context.Reply(new Pong
    {
      OriginalTimeStamp = message.Timestamp,
      PingNumber = message.PingNumber
    });
  }
}
