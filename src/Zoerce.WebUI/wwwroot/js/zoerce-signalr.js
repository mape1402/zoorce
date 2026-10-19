(() => {
    const subscriptions = new Set();

    const subscribe = ({ hubUrl, eventName }, callback) => {
        let connection = null;
        let disposed = false;

        const start = async () => {
            if (!window.signalR || disposed) return;
            connection = new window.signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
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
