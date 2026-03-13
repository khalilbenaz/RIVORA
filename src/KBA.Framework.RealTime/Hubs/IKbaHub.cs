namespace KBA.Framework.RealTime.Hubs;

/// <summary>
/// Interface client pour les Hubs KBA
/// </summary>
public interface IKbaHub
{
    Task ReceiveNotification(string type, object data);
}
