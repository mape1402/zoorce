(() => {
    const root = document.documentElement;
    const shell = document.querySelector("[data-z-shell]");
    const toastStack = document.querySelector("[data-z-toast-stack]");
    const dialogLayer = document.querySelector("[data-z-dialog-layer]");
    const commandLayer = document.querySelector("[data-z-command-layer]");
    const commandInput = document.querySelector("[data-z-command-input]");
    let restoreFocus = null;
    let pendingConfirmation = null;

    const mode = () => root.dataset.zoerceTheme === "light" ? "light" : "dark";
    const updateThemeControls = () => {
        const dark = mode() === "dark";
        document.querySelectorAll("[data-z-theme-label]").forEach(label => label.textContent = dark ? "Light" : "Dark");
        document.querySelectorAll("[data-z-theme-toggle]").forEach(button => {
            button.setAttribute("aria-pressed", String(dark));
            button.querySelector(".z-theme-icon").textContent = dark ? "☼" : "◐";
        });
    };
    const setTheme = value => {
        root.dataset.zoerceTheme = value === "light" ? "light" : "dark";
        localStorage.setItem("zoerce-theme", root.dataset.zoerceTheme);
        updateThemeControls();
        document.dispatchEvent(new CustomEvent("zoerce:theme-change", { detail: { mode: root.dataset.zoerceTheme } }));
    };
    updateThemeControls();

    document.querySelectorAll("[data-z-theme-toggle]").forEach(button => {
        button.addEventListener("click", () => setTheme(mode() === "dark" ? "light" : "dark"));
    });

    document.querySelectorAll("[data-z-sidebar-toggle]").forEach(button => {
        button.addEventListener("click", () => {
            if (!shell) return;
            const open = shell.dataset.sidebarOpen !== "true";
            shell.dataset.sidebarOpen = String(open);
            localStorage.setItem("zoerce-sidebar-open", String(open));
        });
    });

    document.querySelectorAll("[data-z-nav-group]").forEach(button => {
        button.addEventListener("click", () => {
            const expanded = button.getAttribute("aria-expanded") === "true";
            button.setAttribute("aria-expanded", String(!expanded));
        });
    });

    const removeToast = toast => {
        if (!toast || toast.classList.contains("is-leaving")) return;
        toast.classList.add("is-leaving");
        window.setTimeout(() => toast.remove(), 180);
    };
    const wireToast = toast => {
        const timeout = Number.parseInt(toast.dataset.zTimeout ?? "5200", 10);
        let timer = Number.isFinite(timeout) && timeout > 0 ? window.setTimeout(() => removeToast(toast), timeout) : 0;
        toast.querySelector("[data-z-toast-dismiss]")?.addEventListener("click", () => {
            if (timer) window.clearTimeout(timer);
            removeToast(toast);
        });
        toast.addEventListener("mouseenter", () => {
            if (timer) window.clearTimeout(timer);
            timer = 0;
        });
        toast.addEventListener("mouseleave", () => {
            if (!timer && timeout > 0) timer = window.setTimeout(() => removeToast(toast), 1300);
        });
    };
    document.querySelectorAll("[data-z-toast-item]").forEach(wireToast);

    const toast = (title, message, kind = "success", timeout = 5200) => {
        if (!toastStack) return;
        const item = document.createElement("article");
        item.className = `z-toast z-toast-${kind}`;
        item.dataset.zToastItem = "";
        item.dataset.zTimeout = String(timeout);
        item.setAttribute("role", "status");
        const symbol = document.createElement("span");
        symbol.className = "z-toast-symbol";
        symbol.setAttribute("aria-hidden", "true");
        const copy = document.createElement("div");
        const heading = document.createElement("strong");
        heading.textContent = title;
        const body = document.createElement("p");
        body.textContent = message;
        copy.append(heading, body);
        const dismiss = document.createElement("button");
        dismiss.className = "z-toast-dismiss";
        dismiss.type = "button";
        dismiss.dataset.zToastDismiss = "";
        dismiss.setAttribute("aria-label", "Dismiss notification");
        dismiss.textContent = "×";
        item.append(symbol, copy, dismiss);
        toastStack.prepend(item);
        wireToast(item);
    };
    window.Zoorce = {
        toast,
        success: (title, message) => toast(title, message, "success"),
        info: (title, message) => toast(title, message, "information"),
        warning: (title, message) => toast(title, message, "warning"),
        danger: (title, message) => toast(title, message, "danger"),
        setTheme
    };

    document.querySelectorAll("[data-z-toast]").forEach(button => {
        button.addEventListener("click", () => toast(
            button.dataset.zToastTitle ?? "Component ready",
            button.dataset.zToast ?? "Your notification was delivered.",
            button.dataset.zToastKind ?? "success"));
    });

    const closeDialog = () => {
        if (!dialogLayer) return;
        dialogLayer.hidden = true;
        pendingConfirmation = null;
        restoreFocus?.focus();
        restoreFocus = null;
    };
    document.querySelectorAll("[data-z-confirm]").forEach(button => {
        button.addEventListener("click", () => {
            if (!dialogLayer) return;
            restoreFocus = document.activeElement;
            dialogLayer.querySelector("[data-z-dialog-message]").textContent = button.dataset.zConfirm ?? "Are you sure?";
            dialogLayer.querySelector("[data-z-dialog-confirm]").textContent = button.dataset.zConfirmLabel ?? "Confirm";
            pendingConfirmation = () => toast(
                button.dataset.zConfirmSuccessTitle ?? "Action confirmed",
                button.dataset.zConfirmSuccess ?? "The requested action has been completed.",
                button.dataset.zConfirmKind ?? "success");
            dialogLayer.hidden = false;
            dialogLayer.querySelector("[data-z-dialog-confirm]").focus();
        });
    });
    dialogLayer?.querySelector("[data-z-dialog-cancel]")?.addEventListener("click", closeDialog);
    dialogLayer?.querySelector("[data-z-dialog-confirm]")?.addEventListener("click", () => {
        pendingConfirmation?.();
        closeDialog();
    });
    dialogLayer?.addEventListener("click", event => {
        if (event.target === dialogLayer) closeDialog();
    });

    const closeCommand = () => {
        if (!commandLayer) return;
        commandLayer.hidden = true;
        restoreFocus?.focus();
        restoreFocus = null;
    };
    const openCommand = () => {
        if (!commandLayer) return;
        restoreFocus = document.activeElement;
        commandLayer.hidden = false;
        if (commandInput) {
            commandInput.value = "";
            document.querySelectorAll("[data-z-command-item]").forEach(item => item.hidden = false);
            commandInput.focus();
        }
    };
    document.querySelectorAll("[data-z-command-open]").forEach(button => button.addEventListener("click", openCommand));
    commandLayer?.addEventListener("click", event => {
        if (event.target === commandLayer) closeCommand();
    });
    commandInput?.addEventListener("input", () => {
        const query = commandInput.value.trim().toLowerCase();
        document.querySelectorAll("[data-z-command-item]").forEach(item => {
            item.hidden = !item.textContent.toLowerCase().includes(query);
        });
    });

    const closeLayer = layer => {
        layer.hidden = true;
        restoreFocus?.focus();
        restoreFocus = null;
    };
    document.querySelectorAll("[data-z-modal-open], [data-z-drawer-open]").forEach(button => {
        button.addEventListener("click", () => {
            const layer = document.querySelector(button.dataset.zModalOpen ?? button.dataset.zDrawerOpen);
            if (!layer) return;
            restoreFocus = document.activeElement;
            layer.hidden = false;
            layer.querySelector("[data-z-modal-focus]")?.focus();
        });
    });
    document.querySelectorAll("[data-z-modal-close], [data-z-drawer-close]").forEach(button => {
        button.addEventListener("click", () => closeLayer(button.closest("[data-z-modal-layer], [data-z-drawer-layer]")));
    });
    document.querySelectorAll("[data-z-modal-layer], [data-z-drawer-layer]").forEach(layer => {
        layer.addEventListener("click", event => {
            if (event.target === layer) closeLayer(layer);
        });
    });

    document.querySelectorAll("[data-z-tabs]").forEach(tabs => {
        const buttons = [...tabs.querySelectorAll("[role=tab]")];
        const panels = [...tabs.parentElement.querySelectorAll("[role=tabpanel]")];
        buttons.forEach(button => button.addEventListener("click", () => {
            buttons.forEach(candidate => candidate.setAttribute("aria-selected", String(candidate === button)));
            panels.forEach(panel => panel.hidden = panel.id !== button.getAttribute("aria-controls"));
        }));
    });

    document.querySelectorAll("[data-z-filter-input]").forEach(input => {
        input.addEventListener("input", () => {
            const query = input.value.trim().toLowerCase();
            document.querySelectorAll(input.dataset.zFilterInput).forEach(row => {
                row.hidden = !row.textContent.toLowerCase().includes(query);
            });
        });
    });
    document.querySelectorAll("[data-z-filter-chip]").forEach(chip => {
        chip.addEventListener("click", () => chip.classList.toggle("is-active"));
    });
    document.querySelectorAll("[data-z-copy]").forEach(button => {
        button.addEventListener("click", async () => {
            const target = document.querySelector(button.dataset.zCopy);
            if (!target) return;
            await navigator.clipboard.writeText(target.textContent.trim());
            const previous = button.textContent;
            button.textContent = "Copied";
            window.setTimeout(() => button.textContent = previous, 1400);
        });
    });

    document.addEventListener("keydown", event => {
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
            event.preventDefault();
            openCommand();
        }
        if (event.key === "Escape") {
            if (commandLayer && !commandLayer.hidden) closeCommand();
            if (dialogLayer && !dialogLayer.hidden) closeDialog();
            document.querySelectorAll("[data-z-modal-layer], [data-z-drawer-layer]").forEach(layer => {
                if (!layer.hidden) closeLayer(layer);
            });
        }
    });
})();
