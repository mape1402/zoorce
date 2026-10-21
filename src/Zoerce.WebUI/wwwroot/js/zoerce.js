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

    const desktopSidebarQuery = window.matchMedia("(min-width: 851px)");
    const canCollapseSidebar = () => shell?.dataset.sidebarMode === "collapsible";
    const persistsSidebar = () => shell?.dataset.sidebarPersist === "true";
    let sidebarCollapsed = shell?.dataset.sidebarDefault === "collapsed";
    let mobileSidebarOpen = false;

    if (canCollapseSidebar() && persistsSidebar()) {
        try {
            const savedState = localStorage.getItem("zoerce-sidebar-collapsed");
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
                localStorage.setItem("zoerce-sidebar-collapsed", String(sidebarCollapsed));
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

    document.querySelectorAll("[data-z-live-demo]").forEach(button => {
        button.addEventListener("click", async () => {
            const endpoint = button.dataset.zLiveEndpoint;
            if (!endpoint || button.disabled) return;
            const previous = button.textContent;
            button.disabled = true;
            button.textContent = "Sending…";
            try {
                const response = await fetch(endpoint, { method: "POST", headers: { Accept: "application/json" } });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                toast("Live event sent", "The components received one deliberate update.", "information");
            } catch {
                toast("Live event unavailable", "The sample host could not send its demonstration update.", "danger");
            } finally {
                button.disabled = false;
                button.textContent = previous;
            }
        });
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

(() => {
    const instances = new WeakMap();
    const svgNamespace = "http://www.w3.org/2000/svg";

    const parseConfiguration = (root, selector) => {
        const source = root.querySelector(selector)?.textContent;
        if (!source) return null;
        try {
            return JSON.parse(source);
        } catch {
            root.dispatchEvent(new CustomEvent("zoerce:component-error", { detail: { message: "Invalid component configuration." } }));
            return null;
        }
    };

    const dispatch = (root, name, detail = {}) => root.dispatchEvent(new CustomEvent(name, { bubbles: true, detail }));
    const displayValue = value => value === null || value === undefined ? "" : String(value);

    class DataGridController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.requestController = null;
            this.page = 1;
            this.hasMore = false;
            this.totalCount = null;
            this.items = [];
            this.pendingUpdates = 0;
            this.hasUserScrolled = false;
            this.renderFrame = 0;
            this.lastWindow = "";
            this.rowHeight = 48;
            this.search = "";
            this.sort = null;
            this.descending = false;
            this.searchTimer = 0;
            this.body = root.querySelector("[data-z-grid-body]");
            this.viewport = root.querySelector("[data-z-grid-viewport]");
            this.empty = root.querySelector("[data-z-grid-empty]");
            this.summary = root.querySelector("[data-z-grid-summary]");
            this.state = root.querySelector("[data-z-grid-state]");
            this.pagination = root.querySelector("[data-z-grid-pagination]");
            this.previous = root.querySelector("[data-z-grid-previous]");
            this.next = root.querySelector("[data-z-grid-next]");
            this.loadMore = root.querySelector("[data-z-grid-load-more]");
            this.sentinel = root.querySelector("[data-z-grid-sentinel]");
            this.updateNoticeButton = root.querySelector("[data-z-grid-update-notice]");
            this.bind();
            this.connectRealtime();
            this.load(1, { reset: true });
        }

        bind() {
            const signal = this.abortController.signal;
            this.root.addEventListener("zoerce:grid:insert", event => this.insert(event.detail?.item), { signal });
            this.root.addEventListener("zoerce:grid:refresh", () => this.load(1, { reset: true }), { signal });
            this.root.querySelector("[data-z-grid-search]")?.addEventListener("input", event => {
                const value = event.currentTarget.value.trim();
                window.clearTimeout(this.searchTimer);
                this.searchTimer = window.setTimeout(() => {
                    this.search = value;
                    this.load(1, { reset: true });
                }, 180);
            }, { signal });
            this.root.querySelectorAll("[data-z-grid-sort]").forEach(button => button.addEventListener("click", () => {
                const field = button.dataset.zGridSort;
                this.descending = this.sort === field ? !this.descending : false;
                this.sort = field;
                this.root.querySelectorAll("[data-z-grid-sort]").forEach(candidate => candidate.setAttribute("aria-sort", candidate.dataset.zGridSort === field ? this.descending ? "descending" : "ascending" : "none"));
                this.load(1, { reset: true });
            }, { signal }));
            this.previous?.addEventListener("click", () => this.load(Math.max(1, this.page - 1), { reset: true }), { signal });
            this.next?.addEventListener("click", () => this.load(this.page + 1, { reset: true }), { signal });
            this.loadMore?.addEventListener("click", () => this.requestNext(), { signal });
            this.updateNoticeButton?.addEventListener("click", () => {
                this.pendingUpdates = 0;
                this.updateNoticeButton.hidden = true;
                this.load(1, { reset: true });
            }, { signal });

            if (this.configuration.loadMode === "InfiniteScroll" && this.viewport) {
                this.viewport.addEventListener("scroll", () => this.onViewportScroll(), { passive: true, signal });
            }

            if (this.configuration.loadMode === "InfiniteScroll" && "IntersectionObserver" in window && this.sentinel) {
                this.observer = new IntersectionObserver(entries => {
                    if (this.hasUserScrolled && entries.some(entry => entry.isIntersecting)) this.requestNext();
                }, {
                    root: this.viewport ?? null,
                    rootMargin: `0px 0px ${Number(this.configuration.prefetchThreshold ?? 144)}px 0px`
                });
                this.observer.observe(this.sentinel);
            }
        }

        onViewportScroll() {
            this.hasUserScrolled = true;
            if (this.configuration.virtualize && !this.renderFrame) {
                this.renderFrame = window.requestAnimationFrame(() => {
                    this.renderFrame = 0;
                    this.renderRows();
                });
            }
            if (!this.viewport) return;
            const remaining = this.viewport.scrollHeight - this.viewport.scrollTop - this.viewport.clientHeight;
            if (remaining <= Number(this.configuration.prefetchThreshold ?? 144)) this.requestNext();
        }

        requestNext() {
            if (this.configuration.loadMode !== "InfiniteScroll" || !this.hasMore || this.requestController) return;
            this.load(this.page + 1, { append: true });
        }

        async load(page, { append = false, reset = false } = {}) {
            if (this.requestController) return;
            this.requestController = new AbortController();
            const request = this.requestController;
            this.root.setAttribute("aria-busy", "true");
            this.setState("Loading…");
            const url = new URL(this.configuration.sourceUrl, window.location.origin);
            url.searchParams.set("page", String(page));
            url.searchParams.set("pageSize", String(this.configuration.pageSize));
            if (this.search) url.searchParams.set("search", this.search);
            if (this.sort) url.searchParams.set("sort", this.sort);
            if (this.descending) url.searchParams.set("descending", "true");

            try {
                const response = await fetch(url, { headers: { Accept: "application/json" }, signal: request.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                const result = await response.json();
                const received = Array.isArray(result.items) ? result.items : [];
                if (append) {
                    const keys = new Set(this.items.map(item => displayValue(item[this.configuration.rowKeyField])));
                    this.items.push(...received.filter(item => !keys.has(displayValue(item[this.configuration.rowKeyField]))));
                } else {
                    this.items = received;
                    if (reset && this.viewport) this.viewport.scrollTop = 0;
                }
                this.page = page;
                this.hasMore = Boolean(result.hasMore);
                this.totalCount = Number.isFinite(Number(result.totalCount)) ? Number(result.totalCount) : null;
                this.renderRows(true);
                this.updateControls();
                this.setState("");
                dispatch(this.root, "zoerce:grid:loaded", { page, append, result });
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to load records.");
                    dispatch(this.root, "zoerce:grid:error", { error });
                }
            } finally {
                this.root.removeAttribute("aria-busy");
                if (this.requestController === request) this.requestController = null;
            }
        }

        createSpacer(height) {
            const row = document.createElement("tr");
            row.className = "z-grid-virtual-spacer";
            row.setAttribute("aria-hidden", "true");
            const cell = document.createElement("td");
            cell.colSpan = this.configuration.columns.length;
            cell.style.height = `${Math.max(0, height)}px`;
            row.append(cell);
            return row;
        }

        createRow(item) {
            const row = document.createElement("tr");
            row.dataset.zGridKey = displayValue(item[this.configuration.rowKeyField]);
            this.configuration.columns.forEach(column => {
                const cell = document.createElement("td");
                cell.dataset.zGridField = column.field;
                cell.textContent = displayValue(item[column.field]);
                row.append(cell);
            });
            return row;
        }

        renderRows(force = false) {
            if (!this.body) return;
            const virtual = Boolean(this.configuration.virtualize) && this.configuration.loadMode === "InfiniteScroll" && this.viewport;
            let start = 0;
            let end = this.items.length;
            if (virtual) {
                const overscan = Number(this.configuration.virtualizationOverscan ?? 8);
                const visibleTop = Math.max(0, this.viewport.scrollTop - 44);
                start = Math.max(0, Math.floor(visibleTop / this.rowHeight) - overscan);
                end = Math.min(this.items.length, Math.ceil((visibleTop + this.viewport.clientHeight) / this.rowHeight) + overscan);
            }
            const signature = `${start}:${end}:${this.items.length}`;
            if (!force && signature === this.lastWindow) return;
            this.lastWindow = signature;
            const fragment = document.createDocumentFragment();
            if (virtual && start > 0) fragment.append(this.createSpacer(start * this.rowHeight));
            this.items.slice(start, end).forEach(item => fragment.append(this.createRow(item)));
            if (virtual && end < this.items.length) fragment.append(this.createSpacer((this.items.length - end) * this.rowHeight));
            this.body.replaceChildren(fragment);
            if (this.empty) this.empty.hidden = this.items.length > 0;
        }

        updateControls() {
            const infinite = this.configuration.loadMode === "InfiniteScroll";
            if (this.pagination) this.pagination.hidden = infinite;
            if (this.sentinel) this.sentinel.hidden = !infinite || !this.hasMore;
            if (this.loadMore) this.loadMore.hidden = !infinite || !this.hasMore;
            if (this.previous) this.previous.disabled = this.page <= 1;
            if (this.next) this.next.disabled = !this.hasMore;
            if (this.summary) {
                const visible = this.items.length;
                this.summary.textContent = this.totalCount === null
                    ? `Showing ${visible} records`
                    : infinite ? `Showing ${visible} loaded of ${this.totalCount}` : `Showing ${visible} of ${this.totalCount}`;
            }
        }

        renderUpdateNotice() {
            if (!this.updateNoticeButton) return;
            this.updateNoticeButton.hidden = this.pendingUpdates === 0;
            this.updateNoticeButton.textContent = `${this.pendingUpdates} update${this.pendingUpdates === 1 ? "" : "s"} available · Refresh`;
        }

        patch(change) {
            const item = change?.item;
            const key = displayValue(change?.key ?? item?.[this.configuration.rowKeyField]);
            const index = this.items.findIndex(candidate => displayValue(candidate[this.configuration.rowKeyField]) === key);
            if (index < 0 || !item) return false;
            this.items[index] = item;
            this.body?.querySelector(`tr[data-z-grid-key="${CSS.escape(key)}"]`)?.querySelectorAll("[data-z-grid-field]").forEach(cell => {
                cell.textContent = displayValue(item[cell.dataset.zGridField]);
            });
            return true;
        }

        insert(item) {
            const key = displayValue(item?.[this.configuration.rowKeyField]);
            if (!key) return;
            const existing = this.items.findIndex(candidate => displayValue(candidate[this.configuration.rowKeyField]) === key);
            if (existing >= 0) this.items.splice(existing, 1);
            this.items.unshift(item);
            if (this.totalCount !== null) this.totalCount += existing < 0 ? 1 : 0;
            this.renderRows(true);
            this.updateControls();
            this.setState("A local sample record was added.");
            dispatch(this.root, "zoerce:grid:inserted", { item });
        }

        setState(message) {
            if (this.state) this.state.textContent = message;
        }

        connectRealtime() {
            if (!this.configuration.realtimeHubUrl || !this.configuration.realtimeEventName) return;
            if (!window.ZoorceSignalR?.subscribe) {
                document.addEventListener("zoerce:signalr-ready", () => this.connectRealtime(), { once: true, signal: this.abortController.signal });
                return;
            }
            this.unsubscribeRealtime = window.ZoorceSignalR.subscribe({ hubUrl: this.configuration.realtimeHubUrl, eventName: this.configuration.realtimeEventName }, change => {
                const behavior = this.configuration.realtimeBehavior ?? "Badge";
                if (behavior === "Refresh") {
                    this.load(1, { reset: true });
                } else if (behavior === "Patch" && this.patch(change)) {
                    this.setState("Live row updated.");
                } else {
                    this.pendingUpdates += 1;
                    this.renderUpdateNotice();
                    this.setState(behavior === "Manual" ? "Live changes are available." : "New data is available.");
                }
                dispatch(this.root, "zoerce:grid:realtime", { change });
            });
        }

        dispose() {
            window.clearTimeout(this.searchTimer);
            this.abortController.abort();
            this.requestController?.abort();
            this.observer?.disconnect();
            if (this.renderFrame) window.cancelAnimationFrame(this.renderFrame);
            this.unsubscribeRealtime?.();
        }
    }

    const normalizeSeries = series => Array.isArray(series) ? series.filter(item => Array.isArray(item?.points)) : [];
    const createSvgElement = name => document.createElementNS(svgNamespace, name);
    const chartNumber = value => Number.isFinite(Number(value)) ? Number(value) : 0;
    const formatChartValue = value => Number.isFinite(value)
        ? new Intl.NumberFormat(undefined, { maximumFractionDigits: 1 }).format(value)
        : "—";
    const niceNumber = value => {
        const exponent = Math.floor(Math.log10(Math.max(value, 1)));
        const fraction = value / (10 ** exponent);
        const niceFraction = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
        return niceFraction * (10 ** exponent);
    };
    const createTicks = (minimum, maximum, count = 4) => {
        const step = niceNumber((maximum - minimum || 1) / count);
        const start = Math.floor(minimum / step) * step;
        const end = Math.ceil(maximum / step) * step;
        const ticks = [];
        for (let value = start; value <= end + step / 100; value += step) ticks.push(Number(value.toFixed(8)));
        return ticks;
    };
    const appendSvgText = (parent, x, y, value, className, anchor = "start") => {
        const text = createSvgElement("text");
        text.setAttribute("class", className);
        text.setAttribute("x", String(x));
        text.setAttribute("y", String(y));
        text.setAttribute("text-anchor", anchor);
        text.textContent = value;
        parent.append(text);
        return text;
    };

    class ChartController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.canvas = root.querySelector("[data-z-chart-canvas]");
            this.state = root.querySelector("[data-z-chart-state]");
            this.metric = root.querySelector("[data-z-chart-metric]");
            this.delta = root.querySelector("[data-z-chart-delta]");
            this.legend = root.querySelector("[data-z-chart-legend]");
            this.series = normalizeSeries(configuration.series);
            this.renderFrame = 0;
            this.initializeCanvas();
            this.render(this.series);
            this.connectRealtime();
            if (configuration.sourceUrl) this.load();
            if ("ResizeObserver" in window && this.canvas) {
                this.resizeObserver = new ResizeObserver(() => this.scheduleRender());
                this.resizeObserver.observe(this.canvas);
            } else {
                window.addEventListener("resize", () => this.scheduleRender(), { signal: this.abortController.signal });
            }
        }

        initializeCanvas() {
            if (!this.canvas) return;
            const title = this.canvas.querySelector("title") ?? createSvgElement("title");
            const description = this.canvas.querySelector("desc") ?? createSvgElement("desc");
            this.layers = {
                definitions: createSvgElement("defs"),
                grid: createSvgElement("g"),
                axis: createSvgElement("g"),
                data: createSvgElement("g"),
                markers: createSvgElement("g"),
                annotation: createSvgElement("g")
            };
            this.layers.grid.setAttribute("class", "z-native-chart-grid-layer");
            this.layers.axis.setAttribute("class", "z-native-chart-axis");
            this.canvas.replaceChildren(title, description, this.layers.definitions, this.layers.grid, this.layers.axis, this.layers.data, this.layers.markers, this.layers.annotation);
        }

        scheduleRender() {
            if (this.renderFrame) return;
            this.renderFrame = window.requestAnimationFrame(() => {
                this.renderFrame = 0;
                this.render(this.series);
            });
        }

        clearLayers() {
            Object.values(this.layers ?? {}).forEach(layer => layer.replaceChildren());
        }

        async load() {
            try {
                this.root.setAttribute("aria-busy", "true");
                this.setState("Refreshing…");
                const response = await fetch(this.configuration.sourceUrl, { headers: { Accept: "application/json" }, signal: this.abortController.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                const body = await response.json();
                this.render(normalizeSeries(body.series ?? body));
                this.setState("");
                dispatch(this.root, "zoerce:chart:loaded", { series: body.series ?? body });
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to refresh chart.");
                    dispatch(this.root, "zoerce:chart:error", { error });
                }
            } finally {
                this.root.removeAttribute("aria-busy");
            }
        }

        render(series) {
            if (!this.canvas || !this.layers) return;
            this.series = series;
            this.clearLayers();
            const values = series.flatMap(item => item.points.map(point => Number(point.value))).filter(Number.isFinite);
            if (!values.length) return;
            this.updateMetadata(series);
            const box = this.canvas.getBoundingClientRect();
            const width = Math.max(280, Math.round(box.width || 600));
            const height = Math.max(220, Math.round(box.height || 270));
            this.canvas.setAttribute("viewBox", `0 0 ${width} ${height}`);
            this.canvas.setAttribute("preserveAspectRatio", "none");
            const minimum = Math.min(...values);
            const maximum = Math.max(...values);
            if (this.configuration.kind === "Donut") {
                this.renderDonut(series, width, height);
                return;
            }
            const isBar = this.configuration.kind === "Bar";
            const padding = (maximum - minimum || Math.max(Math.abs(maximum) * .12, 1)) * .12;
            const provisionalLowerBound = isBar ? Math.min(0, minimum) : minimum - padding;
            const provisionalUpperBound = isBar ? Math.max(0, maximum) : maximum + padding;
            const ticks = createTicks(provisionalLowerBound, provisionalUpperBound);
            const lowerBound = ticks[0];
            const upperBound = ticks.at(-1);
            const range = upperBound - lowerBound || 1;
            const plot = { left: Math.min(52, width * .15), right: width - 18, top: 18, bottom: height - 34 };
            const toY = value => plot.bottom - ((value - lowerBound) / range) * (plot.bottom - plot.top);
            const toX = (index, count) => count <= 1 ? (plot.left + plot.right) / 2 : plot.left + index * (plot.right - plot.left) / (count - 1);
            this.renderAxes(series[0]?.points ?? [], ticks, plot, toX, toY);
            if (isBar) {
                this.renderBars(series, plot, toX, toY);
                return;
            }
            this.renderLines(series, plot, toX, toY);
        }

        updateMetadata(series) {
            const primary = series[0];
            const points = primary?.points ?? [];
            const latest = chartNumber(points.at(-1)?.value);
            const previous = points.length > 1 ? chartNumber(points.at(-2)?.value) : null;
            const suffix = this.configuration.valueSuffix ?? "";
            if (this.metric) this.metric.textContent = points.length ? `${formatChartValue(latest)}${suffix}` : "—";
            if (this.delta) {
                const difference = previous === null ? null : latest - previous;
                this.delta.textContent = difference === null ? "Awaiting data" : `${difference >= 0 ? "+" : ""}${formatChartValue(difference)}${suffix} vs. previous`;
                this.delta.classList.toggle("is-positive", difference > 0);
                this.delta.classList.toggle("is-negative", difference < 0);
            }
            if (this.legend) {
                const items = series.map((item, index) => {
                    const entry = document.createElement("span");
                    const swatch = document.createElement("i");
                    swatch.dataset.zChartSeries = String(index);
                    swatch.setAttribute("aria-hidden", "true");
                    entry.append(swatch, document.createTextNode(item.name || `Series ${index + 1}`));
                    return entry;
                });
                this.legend.replaceChildren(...items);
            }
        }

        renderAxes(points, ticks, plot, toX, toY) {
            const gridLayer = this.layers.grid;
            const axisLayer = this.layers.axis;
            for (const value of ticks) {
                const y = toY(value);
                const grid = createSvgElement("line");
                grid.setAttribute("class", "z-native-chart-grid");
                grid.setAttribute("x1", String(plot.left)); grid.setAttribute("x2", String(plot.right));
                grid.setAttribute("y1", String(y)); grid.setAttribute("y2", String(y));
                gridLayer.append(grid);
                appendSvgText(axisLayer, plot.left - 10, y + 3.5, formatChartValue(value), "z-native-chart-axis-label", "end");
            }
            const interval = Math.max(1, Math.ceil(points.length / 6));
            points.forEach((point, index) => {
                if (index % interval !== 0 && index !== points.length - 1) return;
                appendSvgText(axisLayer, toX(index, points.length), plot.bottom + 20, point.label ?? "", "z-native-chart-axis-label", "middle");
            });
        }

        renderLines(series, plot, toX, toY) {
            const suffix = (this.root.id || "chart").replace(/[^a-z0-9_-]/gi, "");
            if (this.configuration.kind === "Area") {
                const gradient = createSvgElement("linearGradient");
                gradient.setAttribute("id", `z-chart-area-${suffix}`);
                gradient.setAttribute("x1", "0"); gradient.setAttribute("x2", "0");
                gradient.setAttribute("y1", "0"); gradient.setAttribute("y2", "1");
                const start = createSvgElement("stop"); start.setAttribute("offset", "0%"); start.setAttribute("stop-color", "var(--z-data-primary)"); start.setAttribute("stop-opacity", ".32");
                const end = createSvgElement("stop"); end.setAttribute("offset", "100%"); end.setAttribute("stop-color", "var(--z-data-primary)"); end.setAttribute("stop-opacity", "0");
                gradient.append(start, end); this.layers.definitions.append(gradient);
            }
            series.forEach((item, seriesIndex) => {
                const points = item.points.map((point, index) => ({ x: toX(index, item.points.length), y: toY(chartNumber(point.value)), point }));
                if (!points.length) return;
                const pathData = points.map((point, index) => `${index === 0 ? "M" : "L"}${point.x} ${point.y}`).join(" ");
                if (this.configuration.kind === "Area" && seriesIndex === 0) {
                    const area = createSvgElement("path");
                    area.setAttribute("class", "z-native-chart-area");
                    area.setAttribute("fill", `url(#z-chart-area-${suffix})`);
                    area.setAttribute("d", `${pathData} L${points.at(-1).x} ${plot.bottom} L${points[0].x} ${plot.bottom} Z`);
                    this.layers.data.append(area);
                }
                const line = createSvgElement("path");
                line.setAttribute("class", seriesIndex === 0 ? "z-native-chart-line" : "z-native-chart-line-secondary");
                line.setAttribute("d", pathData);
                const title = createSvgElement("title");
                title.textContent = `${item.name}: ${points.map(({ point }) => `${point.label} ${formatChartValue(chartNumber(point.value))}`).join(", ")}`;
                line.append(title); this.layers.data.append(line);
                const finalPoint = points.at(-1);
                const marker = createSvgElement("circle");
                marker.setAttribute("class", seriesIndex === 0 ? "z-native-chart-marker" : "z-native-chart-marker-secondary");
                marker.setAttribute("cx", String(finalPoint.x)); marker.setAttribute("cy", String(finalPoint.y)); marker.setAttribute("r", seriesIndex === 0 ? "4" : "3");
                marker.append(createSvgElement("title")); marker.lastChild.textContent = `${item.name}, ${finalPoint.point.label}: ${formatChartValue(chartNumber(finalPoint.point.value))}`;
                this.layers.markers.append(marker);
            });
        }

        renderBars(series, plot, toX, toY) {
            const labels = series[0]?.points ?? [];
            const clusterWidth = (plot.right - plot.left) / Math.max(labels.length, 1);
            const gap = Math.min(5, clusterWidth * .09);
            const barWidth = Math.max(4, (clusterWidth * .72 - gap * Math.max(series.length - 1, 0)) / Math.max(series.length, 1));
            const baseline = toY(0);
            series.forEach((item, seriesIndex) => {
                item.points.forEach((point, index) => {
                    const valueY = toY(chartNumber(point.value));
                    const height = Math.abs(baseline - valueY);
                    const bar = createSvgElement("rect");
                    bar.setAttribute("class", seriesIndex === 0 ? "z-native-chart-bar" : "z-native-chart-bar-secondary");
                    bar.setAttribute("x", String(toX(index, labels.length) - (barWidth * series.length + gap * Math.max(series.length - 1, 0)) / 2 + seriesIndex * (barWidth + gap)));
                    bar.setAttribute("y", String(Math.min(valueY, baseline)));
                    bar.setAttribute("width", String(barWidth));
                    bar.setAttribute("height", String(Math.max(height, 1)));
                    bar.setAttribute("rx", "3");
                    const title = createSvgElement("title"); title.textContent = `${item.name}, ${point.label}: ${formatChartValue(chartNumber(point.value))}`;
                    bar.append(title); this.layers.data.append(bar);
                });
            });
        }

        renderDonut(series, width, height) {
            const leadingPoint = series[0]?.points.at(-1);
            const leadingValue = Math.min(100, Math.max(0, chartNumber(leadingPoint?.value)));
            const slices = [
                { name: series[0]?.name ?? "Adoption", value: leadingValue },
                { name: "Remaining", value: 100 - leadingValue }
            ];
            const centerX = width / 2; const centerY = height / 2; const radius = Math.max(42, Math.min(width, height) * .28);
            const track = createSvgElement("circle");
            track.setAttribute("class", "z-native-chart-donut-track"); track.setAttribute("cx", String(centerX)); track.setAttribute("cy", String(centerY)); track.setAttribute("r", String(radius)); track.setAttribute("fill", "none");
            this.layers.data.append(track);
            let offset = 0;
            slices.forEach((slice, index) => {
                const value = slice.value;
                const circle = createSvgElement("circle");
                const share = value;
                circle.setAttribute("class", index === 0 ? "z-native-chart-donut" : "z-native-chart-donut-secondary");
                circle.setAttribute("cx", String(centerX)); circle.setAttribute("cy", String(centerY)); circle.setAttribute("r", String(radius));
                circle.setAttribute("fill", "none"); circle.setAttribute("pathLength", "100");
                circle.setAttribute("stroke-dasharray", `${Math.max(0, share - 1.5)} ${101.5 - share}`);
                circle.setAttribute("stroke-dashoffset", String(-offset));
                circle.setAttribute("transform", `rotate(-90 ${centerX} ${centerY})`);
                offset += share;
                const title = createSvgElement("title"); title.textContent = `${slice.name}: ${formatChartValue(value)} (${formatChartValue(share)}%)`;
                circle.append(title); this.layers.data.append(circle);
            });
            const leadingSlice = slices[0];
            appendSvgText(this.layers.annotation, centerX, centerY - 2, leadingSlice ? `${formatChartValue(leadingSlice.value)}${this.configuration.valueSuffix ?? "%"}` : "—", "z-native-chart-donut-value", "middle");
            appendSvgText(this.layers.annotation, centerX, centerY + 17, leadingSlice?.name ?? "No data", "z-native-chart-donut-label", "middle");
        }

        setState(message) {
            if (this.state) this.state.textContent = message;
        }

        connectRealtime() {
            if (!this.configuration.realtimeHubUrl || !this.configuration.realtimeEventName) return;
            if (!window.ZoorceSignalR?.subscribe) {
                document.addEventListener("zoerce:signalr-ready", () => this.connectRealtime(), { once: true, signal: this.abortController.signal });
                return;
            }
            this.unsubscribeRealtime = window.ZoorceSignalR.subscribe({ hubUrl: this.configuration.realtimeHubUrl, eventName: this.configuration.realtimeEventName }, update => {
                this.render(normalizeSeries(update?.series ?? update));
                dispatch(this.root, "zoerce:chart:realtime", { update });
            });
        }

        dispose() {
            this.abortController.abort();
            this.resizeObserver?.disconnect();
            if (this.renderFrame) window.cancelAnimationFrame(this.renderFrame);
            this.unsubscribeRealtime?.();
        }
    }

    const mountComponent = root => {
        if (instances.has(root)) return;
        const configuration = root.matches("[data-z-data-grid]")
            ? parseConfiguration(root, "[data-z-grid-config]")
            : root.matches("[data-z-chart]")
                ? parseConfiguration(root, "[data-z-chart-config]")
                : null;
        if (!configuration) return;
        const controller = root.matches("[data-z-data-grid]")
            ? new DataGridController(root, configuration)
            : root.matches("[data-z-chart]") ? new ChartController(root, configuration) : null;
        if (controller) instances.set(root, controller);
    };

    const mount = (scope = document) => {
        if (scope.matches?.("[data-z-data-grid], [data-z-chart]")) mountComponent(scope);
        scope.querySelectorAll?.("[data-z-data-grid], [data-z-chart]").forEach(mountComponent);
    };

    const dispose = root => {
        const controller = instances.get(root);
        controller?.dispose();
        instances.delete(root);
    };

    window.ZoorceComponents = Object.freeze({ mount, dispose });
    mount();
})();
