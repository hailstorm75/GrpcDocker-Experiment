using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGreeterProto;

namespace GrpcServer.Services;

public class GreeterService : Greeter.GreeterBase
{
  #region Fields

  private static readonly Empty EMPTY = new Empty();

  private readonly ILogger<GreeterService> m_logger;
  private readonly NotificationStorage m_storage;

  #endregion

  public GreeterService(ILogger<GreeterService> logger, NotificationStorage storage)
  {
    m_logger = logger;
    m_storage = storage;
  }

  /// <inheritdoc />
  public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    => Task.FromResult(new HelloReply
    {
      Message = "Hello " + request.Name
    });

  /// <inheritdoc />
  public override Task<Empty> SendNotification(Notification request, ServerCallContext context)
  {
    var queue = m_storage.GetQueue(request.To);

    if (!context.CancellationToken.IsCancellationRequested)
    {
      queue.Enqueue(request);
      m_logger.LogInformation("Sender {From} sent notification to {To}", request.From, request.To);
    }
    else
      m_logger.LogInformation("Sender {From} cancelled notification sending", request.From);

    return Task.FromResult(EMPTY);
  }

  /// <inheritdoc />
  public override async Task ListenToNotifications(RequesterId request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
  {
    m_logger.LogInformation("Listener {Id} connected", request.Id);

    while (!context.CancellationToken.IsCancellationRequested)
    {
      var queue = m_storage.GetQueue(request.Id);
      if (queue.TryDequeue(out var notification))
      {
        await responseStream.WriteAsync(notification).ConfigureAwait(false);
        m_logger.LogInformation("Sending notification to {Id}", request.Id);
      }
      else
        await Task.Delay(1000);
    }

    m_logger.LogInformation("Listener {Id} disconnected", request.Id);
  }
}