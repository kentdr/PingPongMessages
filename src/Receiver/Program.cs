using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NServiceBus;

namespace Receiver;

static class Program
{
  public static void Main(string[] args)
  {
    CreateHostBuilder(args).Build().Run();
  }

  static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
        .UseConsoleLifetime()
        .ConfigureLogging(logging =>
        {
          logging.AddConsole();
        })
        .UseNServiceBus(ctx =>
        {
          var endpointConfiguration = new EndpointConfiguration("PingPongMessages.Receiver");

          // RabbitMQ Transport: https://docs.particular.net/transports/rabbitmq/
          var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672";
          var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), rabbitMqConnectionString);
          var routing = endpointConfiguration.UseTransport(transport);

          // Message serialization
          endpointConfiguration.UseSerialization<SystemJsonSerializer>();

          endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
          endpointConfiguration.AuditProcessedMessagesTo("audit");

          // Installers are useful in development. Consider disabling in production.
          // https://docs.particular.net/nservicebus/operations/installers
          endpointConfiguration.EnableInstallers();

          var recoverability = endpointConfiguration.Recoverability();
          recoverability.Delayed(d => d.NumberOfRetries(0));
          recoverability.Immediate(i => i.NumberOfRetries(0));

          return endpointConfiguration;
        });
  }

  static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
  {
    // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
    // https://docs.particular.net/nservicebus/hosting/critical-errors
    // and consider setting up service recovery
    // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
    try
    {
      await context.Stop(cancellationToken);
    }
    finally
    {
      FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
    }
  }

  static void FailFast(string message, Exception exception)
  {
    try
    {
      // TODO: decide what kind of last resort logging is necessary
      // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
      // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
    }
    finally
    {
      Environment.FailFast(message, exception);
    }
  }
}
