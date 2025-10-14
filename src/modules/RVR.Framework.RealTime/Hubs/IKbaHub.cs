namespace RVR.Framework.RealTime.Hubs;

/// <summary>
/// Interface client pour les Hubs RVR
/// </summary>
public interface IKbaHub
{
    Task ReceiveNotification(string type, object data);
}
