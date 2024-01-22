using NServiceBus;
using Messages;

namespace Sender;
public class MainPongHandler : IHandleMessages<Pong>
{
  public async Task Handle(Pong message, IMessageHandlerContext context)
  {
    var utcNow = DateTime.UtcNow;

    Console.WriteLine($"Reply to ping {message.PingNumber} received in {(utcNow - message.OriginalTimeStamp).TotalMilliseconds:0}ms ");

    await Task.Delay(1000, context.CancellationToken);

    await context.Send(new Ping
    {
      PingNumber = message.PingNumber + 1,
      Timestamp = utcNow
    });
  }
}
