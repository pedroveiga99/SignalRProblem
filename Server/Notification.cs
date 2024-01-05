using Microsoft.AspNetCore.SignalR;

namespace SignalRProblem;

public class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> logger;

    public NotificationHub(
        ILogger<NotificationHub> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Notif WS connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Notif WS disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}

// Messages sent to client(s)
public interface INotificationClient
{
    Task SystemNotification(bool notifVal);
}

public class NotificationHandler : IDisposable
{
    readonly IHubContext<NotificationHub, INotificationClient> hubContext;
    readonly ILogger<NotificationHandler> logger;

    readonly Notifmanager notifManager;

    public NotificationHandler(
        ILogger<NotificationHandler> logger,
        IHubContext<NotificationHub, INotificationClient> hubContext,
        Notifmanager notifManager)
    {
        this.logger = logger;
        this.hubContext = hubContext;
        this.notifManager = notifManager;

        this.notifManager.TrueNotif += NotifManager_true;
        this.notifManager.FalseNotif += NotifManager_false;
    }

    private void NotifManager_true()
    {
        _ = hubContext.Clients.All.SystemNotification(true);
    }

    private void NotifManager_false()
    {
        _ = hubContext.Clients.All.SystemNotification(false);
    }

    public void Dispose()
    {
        notifManager.TrueNotif -= NotifManager_true;
        notifManager.FalseNotif -= NotifManager_false;
    }
}

public class Notifmanager : IDisposable
{
    readonly ILogger<Notifmanager> logger;

    public delegate void NotifState();
    public event NotifState? FalseNotif;
    public event NotifState? TrueNotif;

    public bool NotifVal { get; private set; } = false;

    bool started = false;

    public Notifmanager(ILogger<Notifmanager> logger)
    {
        this.logger = logger;
    }

    public Task Start()
    {
        started = true;
        _ = Task.Run(() =>
        {
            while (started)
            {
                Console.WriteLine("Press 'n' for notification");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.N:
                        SendNotification();
                        break;
                }
            }
        });
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        started = false;
        return Task.CompletedTask;
    }

    private void SendNotification()
    {
        if (!NotifVal)
        {
            TrueNotif?.Invoke();
        }
        else
        {
            FalseNotif?.Invoke();
        }

        NotifVal = !NotifVal;
        logger.LogWarning($"Notification {NotifVal}");
    }

    public void Dispose()
    {
    }
}

