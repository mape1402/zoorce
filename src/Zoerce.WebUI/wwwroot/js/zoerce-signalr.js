(() => {
    const subscriptions = new Set();

    const resolveTransport = transport => {
        if (!transport || transport === "Auto") return undefined;
        const candidate = window.signalR?.HttpTransportType?.[transport];
        return typeof candidate === "number" ? candidate : undefined;
    };

    const subscribe = ({ hubUrl, eventName, transport }, callback) => {
        let connection = null;
        let disposed = false;

        const start = async () => {
            if (!window.signalR || disposed) return;
            const builder = new window.signalR.HubConnectionBuilder();
            const selectedTransport = resolveTransport(transport);
            connection = (selectedTransport === undefined
                ? builder.withUrl(hubUrl)
                : builder.withUrl(hubUrl, { transport: selectedTransport }))
                .withAutomaticReconnect()
                .build();
            connection.on(eventName, callback);
            try {
                await connection.start();
            } catch {
                if (!disposed) window.setTimeout(start, 2000);
            }
        };

        start();
        const dispose = () => {
            disposed = true;
            subscriptions.delete(dispose);
            connection?.off(eventName, callback);
            connection?.stop();
        };
        subscriptions.add(dispose);
        return dispose;
    };

    window.ZoorceSignalR = Object.freeze({ subscribe, disposeAll: () => [...subscriptions].forEach(dispose => dispose()) });
    document.dispatchEvent(new Event("zoerce:signalr-ready"));
})();
