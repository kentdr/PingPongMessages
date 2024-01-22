using NServiceBus;

namespace Messages;

public class Ping : IMessage
{
  public int PingNumber {get; init;}
  public DateTime Timestamp {get; init;}
}
