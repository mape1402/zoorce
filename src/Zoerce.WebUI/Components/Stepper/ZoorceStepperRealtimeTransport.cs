namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines the SignalR transport preferred by a stepper instance.</summary>
public enum ZoorceStepperRealtimeTransport
{
    /// <summary>Lets the SignalR client negotiate the best transport.</summary>
    Auto,

    /// <summary>Uses WebSockets when the host and browser support them.</summary>
    WebSockets,

    /// <summary>Uses server-sent events for server-to-client updates.</summary>
    ServerSentEvents,

    /// <summary>Uses HTTP long polling where persistent browser connections are unavailable.</summary>
    LongPolling
}
