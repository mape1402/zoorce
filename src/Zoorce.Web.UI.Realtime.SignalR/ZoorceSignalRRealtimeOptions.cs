namespace Zoorce.Web.UI.Realtime.SignalR;

/// <summary>
/// Configures one optional SignalR subscription that forwards invalidations to Zoorce data components.
/// </summary>
public sealed record ZoorceSignalRRealtimeOptions
{
    /// <summary>Gets the absolute or host-relative SignalR hub URL.</summary>
    public string HubUrl { get; init; } = string.Empty;

    /// <summary>Gets the hub event carrying <c>ZoorceDataInvalidation</c> payloads.</summary>
    public string EventName { get; init; } = "dataInvalidated";

    /// <summary>Gets whether the SignalR client automatically reconnects after a transport loss.</summary>
    public bool AutomaticReconnect { get; init; } = true;

    /// <summary>Returns a validated configuration.</summary>
    public ZoorceSignalRRealtimeOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(HubUrl))
        {
            throw new ArgumentException("A SignalR hub URL is required.", nameof(HubUrl));
        }

        if (string.IsNullOrWhiteSpace(EventName))
        {
            throw new ArgumentException("A SignalR event name is required.", nameof(EventName));
        }

        return this with { HubUrl = HubUrl.Trim(), EventName = EventName.Trim() };
    }
}
