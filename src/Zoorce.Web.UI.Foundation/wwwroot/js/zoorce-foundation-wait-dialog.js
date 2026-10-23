(() => {
    const resolve = id => document.getElementById(id);
    const setText = (root, selector, value) => {
        const element = root?.querySelector(selector);
        if (element && typeof value === "string") element.textContent = value;
    };
    const show = (id, update = {}) => {
        const root = resolve(id);
        if (!root) return false;
        setText(root, "[data-z-wait-dialog-title]", update.title);
        setText(root, "[data-z-wait-dialog-description]", update.description);
        root.hidden = false;
        root.querySelector("[role=dialog]")?.focus();
        document.dispatchEvent(new CustomEvent("zoorce:wait-dialog-shown", { detail: { id } }));
        return true;
    };
    const hide = id => {
        const root = resolve(id);
        if (!root) return false;
        root.hidden = true;
        document.dispatchEvent(new CustomEvent("zoorce:wait-dialog-hidden", { detail: { id } }));
        return true;
    };
    document.addEventListener("click", event => {
        const button = event.target.closest("[data-z-wait-dialog-cancel]");
        if (!button) return;
        button.closest("[data-z-wait-dialog]")?.dispatchEvent(new CustomEvent("zoorce:wait-dialog-cancel", { bubbles: true }));
    });
    window.ZoorceWaitDialog = Object.freeze({ show, hide });
})();
