(() => {
    const sources = new Map();
    const listeners = new Map();

    const normalizeScope = scope => typeof scope === "string" && scope.trim() ? scope.trim() : null;

    const notify = invalidation => {
        const scope = normalizeScope(invalidation?.scope || invalidation?.Scope);
        if (!scope) return;
        for (const listener of listeners.get(scope) || []) {
            listener(invalidation);
        }
        document.dispatchEvent(new CustomEvent("zoorce:data-invalidated", { detail: invalidation }));
    };

    const httpSource = {
        async load({ source, query, signal }) {
            const url = new URL(source, window.location.origin);
            Object.entries(query || {}).forEach(([key, value]) => {
                if (value !== undefined && value !== null && value !== "") url.searchParams.set(key, String(value));
            });
            const response = await fetch(url, { headers: { Accept: "application/json" }, signal });
            if (!response.ok) throw new Error(`Data source returned ${response.status}.`);
            return response.json();
        }
    };

    const resolve = source => sources.get(source) || (typeof source === "string" && /^(\/|https?:)/i.test(source) ? httpSource : null);

    const request = async request => {
        const adapter = resolve(request?.source);
        if (!adapter || typeof adapter.load !== "function") {
            throw new Error(`No Zoorce data source is registered for '${request?.source || "unknown"}'.`);
        }
        return adapter.load(request);
    };

    const register = (key, adapter) => {
        if (typeof key !== "string" || !key.trim()) throw new Error("A data source key is required.");
        if (!adapter || typeof adapter.load !== "function") throw new Error("A Zoorce data source must expose load(request).");
        sources.set(key.trim(), adapter);
        return () => sources.delete(key.trim());
    };

    const subscribe = (scope, listener) => {
        const normalizedScope = normalizeScope(scope);
        if (!normalizedScope || typeof listener !== "function") return () => {};
        const scoped = listeners.get(normalizedScope) || new Set();
        scoped.add(listener);
        listeners.set(normalizedScope, scoped);
        return () => {
            scoped.delete(listener);
            if (scoped.size === 0) listeners.delete(normalizedScope);
        };
    };

    window.ZoorceData = Object.freeze({ register, request, subscribe, invalidate: notify });
})();
