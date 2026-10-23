(() => {
    const root = document.documentElement;
    const shell = document.querySelector("[data-z-shell]");
    const sidebar = document.querySelector("[data-z-sidebar]");
    const sidebarBackdrop = document.querySelector("[data-z-sidebar-backdrop]");
    const toastStack = document.querySelector("[data-z-toast-stack]");
    const dialogLayer = document.querySelector("[data-z-dialog-layer]");
    const commandLayer = document.querySelector("[data-z-command-layer]");
    const commandInput = document.querySelector("[data-z-command-input]");
    let restoreFocus = null;
    let pendingConfirmation = null;

    const mode = () => root.dataset.zoorceTheme === "light" ? "light" : "dark";
    const updateThemeControls = () => {
        const dark = mode() === "dark";
        document.querySelectorAll("[data-z-theme-label]").forEach(label => label.textContent = dark ? "Light" : "Dark");
        document.querySelectorAll("[data-z-theme-toggle]").forEach(button => {
            button.setAttribute("aria-pressed", String(dark));
            button.querySelector(".z-theme-icon").textContent = dark ? "☼" : "◐";
        });
    };
    const setTheme = value => {
        root.dataset.zoorceTheme = value === "light" ? "light" : "dark";
        localStorage.setItem("zoorce-theme", root.dataset.zoorceTheme);
        updateThemeControls();
        document.dispatchEvent(new CustomEvent("zoorce:theme-change", { detail: { mode: root.dataset.zoorceTheme } }));
    };
    updateThemeControls();

    const desktopSidebarQuery = window.matchMedia("(min-width: 851px)");
    const canCollapseSidebar = () => shell?.dataset.sidebarMode === "collapsible";
    const persistsSidebar = () => shell?.dataset.sidebarPersist === "true";
    let sidebarCollapsed = shell?.dataset.sidebarDefault === "collapsed";
    let mobileSidebarOpen = false;

    if (canCollapseSidebar() && persistsSidebar()) {
        try {
            const savedState = localStorage.getItem("zoorce-sidebar-collapsed");
            if (savedState === "true" || savedState === "false") sidebarCollapsed = savedState === "true";
        } catch {
            // A blocked storage area should not prevent the shell from rendering.
        }
    }

    const updateSidebarControls = () => {
        if (!shell) return;
        const desktop = desktopSidebarQuery.matches;
        if (!canCollapseSidebar()) sidebarCollapsed = false;
        shell.dataset.sidebarCollapsed = String(sidebarCollapsed);
        shell.dataset.sidebarOpen = String(mobileSidebarOpen);
        const sidebarIsHidden = desktop ? sidebarCollapsed : !mobileSidebarOpen;
        if (sidebar) {
            sidebar.toggleAttribute("inert", sidebarIsHidden);
            sidebar.setAttribute("aria-hidden", String(sidebarIsHidden));
        }
        if (sidebarBackdrop) sidebarBackdrop.hidden = desktop || !mobileSidebarOpen;
        document.querySelectorAll("[data-z-sidebar-toggle]").forEach(button => {
            const expanded = desktop ? !sidebarCollapsed : mobileSidebarOpen;
            button.setAttribute("aria-expanded", String(expanded));
            button.setAttribute("aria-label", desktop
                ? expanded ? "Collapse navigation" : "Expand navigation"
                : mobileSidebarOpen ? "Close navigation" : "Open navigation");
        });
    };
    const setSidebarCollapsed = (collapsed, persist = true) => {
        if (!canCollapseSidebar()) return;
        sidebarCollapsed = Boolean(collapsed);
        if (persist && persistsSidebar()) {
            try {
                localStorage.setItem("zoorce-sidebar-collapsed", String(sidebarCollapsed));
            } catch {
                // Keep the current-page preference when persistent storage is unavailable.
            }
        }
        updateSidebarControls();
    };
    const setMobileSidebar = open => {
        mobileSidebarOpen = Boolean(open);
        updateSidebarControls();
    };
    const toggleSidebar = () => {
        if (desktopSidebarQuery.matches) {
            setSidebarCollapsed(!sidebarCollapsed);
        } else {
            setMobileSidebar(!mobileSidebarOpen);
        }
    };
    updateSidebarControls();

    document.querySelectorAll("[data-z-theme-toggle]").forEach(button => {
        button.addEventListener("click", () => setTheme(mode() === "dark" ? "light" : "dark"));
    });

    document.querySelectorAll("[data-z-sidebar-toggle]").forEach(button => {
        button.addEventListener("click", toggleSidebar);
    });
    document.querySelectorAll("[data-z-sidebar-close]").forEach(button => button.addEventListener("click", () => setMobileSidebar(false)));
    sidebarBackdrop?.addEventListener("click", () => setMobileSidebar(false));
    document.querySelectorAll(".z-sidebar a").forEach(link => link.addEventListener("click", () => {
        if (!desktopSidebarQuery.matches) setMobileSidebar(false);
    }));
    desktopSidebarQuery.addEventListener("change", () => {
        if (desktopSidebarQuery.matches) mobileSidebarOpen = false;
        updateSidebarControls();
    });

    const setGroupExpanded = (button, expanded) => button.setAttribute("aria-expanded", String(expanded));
    const visibleNavigationControls = () => [...document.querySelectorAll("[data-z-nav-group], [data-z-nav-link]")]
        .filter(control => control.getClientRects().length > 0);

    document.querySelectorAll("[data-z-nav-group]").forEach(button => {
        button.addEventListener("click", () => {
            const expanded = button.getAttribute("aria-expanded") === "true";
            setGroupExpanded(button, !expanded);
        });
        button.addEventListener("keydown", event => {
            const expanded = button.getAttribute("aria-expanded") === "true";
            if (event.key === "ArrowRight" && !expanded) {
                event.preventDefault();
                setGroupExpanded(button, true);
                return;
            }
            if (event.key === "ArrowLeft") {
                event.preventDefault();
                if (expanded) {
                    setGroupExpanded(button, false);
                } else {
                    button.closest(".z-nav-group-items")?.previousElementSibling?.focus();
                }
                return;
            }
            if (event.key === "Home" || event.key === "End") {
                event.preventDefault();
                const controls = visibleNavigationControls();
                controls[event.key === "Home" ? 0 : controls.length - 1]?.focus();
            }
        });
    });

    const syncHashNavigation = () => {
        const currentTarget = `${window.location.pathname}${window.location.hash}`;
        document.querySelectorAll("[data-z-nav-link][data-z-nav-match='hash']").forEach(link => {
            const active = (link.dataset.zNavTargets ?? "").split("|").some(target => {
                if (!target.includes("#")) return false;
                return target.startsWith("#")
                    ? window.location.hash === target
                    : currentTarget === target;
            });
            link.classList.toggle("is-active", active);
            if (active) link.setAttribute("aria-current", "page");
            else link.removeAttribute("aria-current");
        });
        document.querySelectorAll("[data-z-nav-group]").forEach(button => {
            const group = document.getElementById(button.getAttribute("aria-controls") ?? "");
            if (group?.querySelector(".z-nav-link.is-active")) setGroupExpanded(button, true);
        });
    };
    syncHashNavigation();
    window.addEventListener("hashchange", syncHashNavigation);

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
        if (!layer) return;
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

    document.querySelectorAll("[data-z-selection-group]").forEach(group => {
        const buttons = [...group.querySelectorAll("button")];
        buttons.forEach(button => button.addEventListener("click", () => {
            buttons.forEach(candidate => {
                const selected = candidate === button;
                candidate.classList.toggle("is-active", selected);
                candidate.setAttribute("aria-pressed", String(selected));
            });
        }));
    });

    document.querySelectorAll("[data-z-density-preview]").forEach(button => {
        button.addEventListener("click", () => {
            const density = button.dataset.zDensityPreview;
            if (shell && (density === "comfortable" || density === "compact" || density === "dense")) {
                shell.dataset.navigationDensity = density;
            }
        });
    });

    document.querySelectorAll("[data-z-filter-input]").forEach(input => {
        const panel = input.closest(".z-panel") ?? document;
        const rows = () => [...panel.querySelectorAll(input.dataset.zFilterInput)];
        const chips = [...panel.querySelectorAll("[data-z-filter-chip]")];
        const applyFilter = () => {
            const query = input.value.trim().toLowerCase();
            const status = chips.find(chip => chip.classList.contains("is-active"))?.dataset.zFilterChip ?? "all";
            rows().forEach(row => {
                const matchesQuery = row.textContent.toLowerCase().includes(query);
                const matchesStatus = status === "all" || row.dataset.zFilterStatus === status;
                row.hidden = !matchesQuery || !matchesStatus;
            });
        };
        input.addEventListener("input", applyFilter);
        chips.forEach(chip => chip.addEventListener("click", () => {
            chips.forEach(candidate => {
                const selected = candidate === chip;
                candidate.classList.toggle("is-active", selected);
                candidate.setAttribute("aria-pressed", String(selected));
            });
            applyFilter();
        }));
    });
    document.querySelectorAll("[data-z-copy]").forEach(button => {
        button.addEventListener("click", async () => {
            const target = document.querySelector(button.dataset.zCopy);
            if (!target) return;
            try {
                await navigator.clipboard.writeText(target.textContent.trim());
                const previous = button.textContent;
                button.textContent = "Copied";
                window.setTimeout(() => button.textContent = previous, 1400);
            } catch {
                toast("Copy unavailable", "Your browser blocked clipboard access.", "danger");
            }
        });
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Tab") {
            const activeLayer = [...document.querySelectorAll("[data-z-dialog-layer], [data-z-command-layer], [data-z-modal-layer], [data-z-drawer-layer]")]
                .find(layer => !layer.hidden);
            if (activeLayer) {
                const focusable = [...activeLayer.querySelectorAll("a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex='-1'])")];
                const first = focusable[0];
                const last = focusable.at(-1);
                if (!first || !last) {
                    event.preventDefault();
                } else if (event.shiftKey && document.activeElement === first) {
                    event.preventDefault();
                    last.focus();
                } else if (!event.shiftKey && document.activeElement === last) {
                    event.preventDefault();
                    first.focus();
                }
            }
        }
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
            event.preventDefault();
            openCommand();
        }
        if (event.key === "Escape") {
            if (!desktopSidebarQuery.matches && mobileSidebarOpen) {
                setMobileSidebar(false);
                return;
            }
            if (commandLayer && !commandLayer.hidden) closeCommand();
            if (dialogLayer && !dialogLayer.hidden) closeDialog();
            document.querySelectorAll("[data-z-modal-layer], [data-z-drawer-layer]").forEach(layer => {
                if (!layer.hidden) closeLayer(layer);
            });
        }
    });
})();
