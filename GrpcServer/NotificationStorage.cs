using System.Collections.Concurrent;
using GrpcGreeterProto;

namespace GrpcServer;

public class NotificationStorage
{
  #region Fields

  private readonly object m_lock = new();
  private readonly Dictionary<string, ConcurrentQueue<Notification>> m_notifications = new();

  #endregion

  public ConcurrentQueue<Notification> GetQueue(string user)
  {
    lock (m_lock)
    {
      if (!m_notifications.TryGetValue(user, out var queue))
      {
        queue = new ConcurrentQueue<Notification>();
        m_notifications.Add(user, queue);
      }

      return queue;
    }
  }
}