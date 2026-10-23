(() => {
    const subscriptions = new Set();

    const subscribe = ({ hubUrl, eventName = "dataInvalidated", transport }, onInvalidation) => {
        if (!window.signalR || !hubUrl || typeof onInvalidation !== "function") return () => {};
        const builder = new window.signalR.HubConnectionBuilder();
        if (window.signalR.LogLevel?.None !== undefined) {
            builder.configureLogging(window.signalR.LogLevel.None);
        }
        const selectedTransport = transport && transport !== "Auto" ? window.signalR.HttpTransportType?.[transport] : undefined;
        const connection = selectedTransport === undefined
            ? builder.withUrl(hubUrl).withAutomaticReconnect().build()
            : builder.withUrl(hubUrl, { transport: selectedTransport }).withAutomaticReconnect().build();
        let disposed = false;
        connection.on(eventName, invalidation => {
            window.ZoorceData?.invalidate?.(invalidation);
            if (!disposed) onInvalidation(invalidation);
        });
        const start = async () => {
            try {
                await connection.start();
            } catch {
                if (!disposed) window.setTimeout(start, 2000);
            }
        };
        start();
        const dispose = () => {
            if (disposed) return;
            disposed = true;
            subscriptions.delete(dispose);
            connection.off(eventName);
            connection.stop();
        };
        subscriptions.add(dispose);
        return dispose;
    };

    window.ZoorceRealtimeSignalR = Object.freeze({ subscribe, disposeAll: () => [...subscriptions].forEach(dispose => dispose()) });
})();
