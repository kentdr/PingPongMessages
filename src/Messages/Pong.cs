namespace Messages;

public class Pong : IMessage
{
  public int PingNumber { get; init; }
  public DateTime OriginalTimeStamp { get; init; }
}
