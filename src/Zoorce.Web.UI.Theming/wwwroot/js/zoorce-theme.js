(() => {
    const apply = theme => {
        if (!theme || typeof theme !== "object") return;
        const root = document.documentElement;
        if (typeof theme.name === "string" && theme.name.trim()) {
            root.dataset.zoorceTheme = theme.name.trim();
        }
        const tokens = theme.tokens && typeof theme.tokens === "object" ? theme.tokens : {};
        Object.entries(tokens).forEach(([name, value]) => {
            if (name.startsWith("--") && typeof value === "string" && value.trim()) {
                root.style.setProperty(name, value.trim());
            }
        });
        document.dispatchEvent(new CustomEvent("zoorce:theme-changed", { detail: theme }));
    };

    window.ZoorceTheme = Object.freeze({ apply });
})();
