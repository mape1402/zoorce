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
        const rows = [...panel.querySelectorAll(input.dataset.zFilterInput)];
        const chips = [...panel.querySelectorAll("[data-z-filter-chip]")];
        const applyFilter = () => {
            const query = input.value.trim().toLowerCase();
            const status = chips.find(chip => chip.classList.contains("is-active"))?.dataset.zFilterChip ?? "all";
            rows.forEach(row => {
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
            this.search = "";
            this.sort = null;
            this.descending = false;
            this.searchTimer = 0;
            this.body = root.querySelector("[data-z-grid-body]");
            this.empty = root.querySelector("[data-z-grid-empty]");
            this.summary = root.querySelector("[data-z-grid-summary]");
            this.state = root.querySelector("[data-z-grid-state]");
            this.pagination = root.querySelector("[data-z-grid-pagination]");
            this.previous = root.querySelector("[data-z-grid-previous]");
            this.next = root.querySelector("[data-z-grid-next]");
            this.loadMore = root.querySelector("[data-z-grid-load-more]");
            this.sentinel = root.querySelector("[data-z-grid-sentinel]");
            this.bind();
            this.connectRealtime();
            this.load(1);
        }

        bind() {
            const signal = this.abortController.signal;
            this.root.querySelector("[data-z-grid-search]")?.addEventListener("input", event => {
                const value = event.currentTarget.value.trim();
                window.clearTimeout(this.searchTimer);
                this.searchTimer = window.setTimeout(() => {
                    this.search = value;
                    this.load(1);
                }, 180);
            }, { signal });
            this.root.querySelectorAll("[data-z-grid-sort]").forEach(button => button.addEventListener("click", () => {
                const field = button.dataset.zGridSort;
                this.descending = this.sort === field ? !this.descending : false;
                this.sort = field;
                this.root.querySelectorAll("[data-z-grid-sort]").forEach(candidate => candidate.setAttribute("aria-sort", candidate.dataset.zGridSort === field ? this.descending ? "descending" : "ascending" : "none"));
                this.load(1);
            }, { signal }));
            this.previous?.addEventListener("click", () => this.load(Math.max(1, this.page - 1)), { signal });
            this.next?.addEventListener("click", () => this.load(this.page + 1), { signal });
            this.loadMore?.addEventListener("click", () => this.load(this.page + 1, true), { signal });

            if (this.configuration.loadMode === "InfiniteScroll" && "IntersectionObserver" in window && this.sentinel) {
                this.observer = new IntersectionObserver(entries => {
                    if (entries.some(entry => entry.isIntersecting) && this.hasMore && !this.requestController) this.load(this.page + 1, true);
                }, { rootMargin: "180px" });
                this.observer.observe(this.sentinel);
            }
        }

        async load(page, append = false) {
            if (this.requestController) this.requestController.abort();
            this.requestController = new AbortController();
            this.root.setAttribute("aria-busy", "true");
            this.setState("Loading…");
            const url = new URL(this.configuration.sourceUrl, window.location.origin);
            url.searchParams.set("page", String(page));
            url.searchParams.set("pageSize", String(this.configuration.pageSize));
            if (this.search) url.searchParams.set("search", this.search);
            if (this.sort) url.searchParams.set("sort", this.sort);
            if (this.descending) url.searchParams.set("descending", "true");

            try {
                const response = await fetch(url, { headers: { Accept: "application/json" }, signal: this.requestController.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                const result = await response.json();
                this.render(result.items ?? [], append);
                this.page = page;
                this.hasMore = Boolean(result.hasMore);
                this.updateControls(result.totalCount);
                this.setState("");
                dispatch(this.root, "zoerce:grid:loaded", { page, append, result });
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to load records.");
                    dispatch(this.root, "zoerce:grid:error", { error });
                }
            } finally {
                this.root.removeAttribute("aria-busy");
                this.requestController = null;
            }
        }

        render(items, append) {
            if (!append) this.body.replaceChildren();
            const fragment = document.createDocumentFragment();
            items.forEach(item => {
                const row = document.createElement("tr");
                this.configuration.columns.forEach(column => {
                    const cell = document.createElement("td");
                    cell.textContent = displayValue(item[column.field]);
                    row.append(cell);
                });
                fragment.append(row);
            });
            this.body.append(fragment);
            if (this.configuration.virtualize && this.body.rows.length > this.configuration.pageSize * 6) {
                const excess = this.body.rows.length - this.configuration.pageSize * 6;
                for (let index = 0; index < excess; index += 1) this.body.deleteRow(0);
            }
            this.empty.hidden = this.body.rows.length > 0;
        }

        updateControls(totalCount) {
            const infinite = this.configuration.loadMode === "InfiniteScroll";
            this.pagination.hidden = infinite;
            this.sentinel.hidden = !infinite || !this.hasMore;
            this.loadMore.hidden = !infinite || !this.hasMore || Boolean(this.observer);
            this.previous.disabled = this.page <= 1;
            this.next.disabled = !this.hasMore;
            const loaded = this.body.rows.length;
            this.summary.textContent = totalCount === null || totalCount === undefined
                ? `Showing ${loaded} records`
                : `Showing ${loaded} of ${totalCount} records`;
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
                if (change?.kind === "remove" || change?.kind === "upsert" || change?.kind === "invalidate") this.load(1);
                dispatch(this.root, "zoerce:grid:realtime", { change });
            });
        }

        dispose() {
            window.clearTimeout(this.searchTimer);
            this.abortController.abort();
            this.requestController?.abort();
            this.observer?.disconnect();
            this.unsubscribeRealtime?.();
        }
    }

    const normalizeSeries = series => Array.isArray(series) ? series.filter(item => Array.isArray(item?.points)) : [];
    const createSvgElement = name => document.createElementNS(svgNamespace, name);

    class ChartController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.canvas = root.querySelector("[data-z-chart-canvas]");
            this.state = root.querySelector("[data-z-chart-state]");
            this.render(normalizeSeries(configuration.series));
            this.connectRealtime();
            if (configuration.sourceUrl) this.load();
        }

        async load() {
            try {
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
            }
        }

        render(series) {
            if (!this.canvas) return;
            this.canvas.replaceChildren();
            const values = series.flatMap(item => item.points.map(point => Number(point.value))).filter(Number.isFinite);
            if (!values.length) return;
            const minimum = Math.min(...values);
            const maximum = Math.max(...values);
            const range = maximum - minimum || 1;
            const grid = createSvgElement("path");
            grid.setAttribute("class", "z-native-chart-grid");
            grid.setAttribute("d", "M0 20H100M0 50H100M0 80H100");
            this.canvas.append(grid);
            if (this.configuration.kind === "Donut") {
                this.renderDonut(series, values);
                return;
            }
            series.forEach((item, seriesIndex) => {
                const points = item.points.map((point, index) => {
                    const x = item.points.length <= 1 ? 50 : index * 100 / (item.points.length - 1);
                    const y = 92 - ((Number(point.value) - minimum) / range) * 76;
                    return `${x},${y}`;
                }).join(" ");
                if (this.configuration.kind === "Bar") {
                    this.renderBars(item.points, minimum, range, seriesIndex, series.length);
                    return;
                }
                if (this.configuration.kind === "Area" && seriesIndex === 0) {
                    const fill = createSvgElement("polygon");
                    fill.setAttribute("class", "z-native-chart-area");
                    fill.setAttribute("points", `0,92 ${points} 100,92`);
                    this.canvas.append(fill);
                }
                const line = createSvgElement("polyline");
                line.setAttribute("class", seriesIndex === 0 ? "z-native-chart-line" : "z-native-chart-line-secondary");
                line.setAttribute("points", points);
                this.canvas.append(line);
            });
        }

        renderBars(points, minimum, range, seriesIndex, seriesCount) {
            const width = 76 / Math.max(points.length, 1) / Math.max(seriesCount, 1);
            points.forEach((point, index) => {
                const value = Number(point.value);
                const height = Math.max(2, ((value - minimum) / range) * 76);
                const bar = createSvgElement("rect");
                bar.setAttribute("class", seriesIndex === 0 ? "z-native-chart-bar" : "z-native-chart-bar-secondary");
                bar.setAttribute("x", String(12 + index * (76 / points.length) + seriesIndex * width));
                bar.setAttribute("y", String(92 - height));
                bar.setAttribute("width", String(Math.max(1.5, width - 1)));
                bar.setAttribute("height", String(height));
                bar.setAttribute("rx", "1");
                this.canvas.append(bar);
            });
        }

        renderDonut(series, values) {
            const total = values.reduce((sum, value) => sum + value, 0) || 1;
            let offset = 25;
            values.forEach((value, index) => {
                const circle = createSvgElement("circle");
                const share = value / total * 100;
                circle.setAttribute("class", index === 0 ? "z-native-chart-donut" : "z-native-chart-donut-secondary");
                circle.setAttribute("cx", "50"); circle.setAttribute("cy", "50"); circle.setAttribute("r", "34");
                circle.setAttribute("fill", "none"); circle.setAttribute("pathLength", "100");
                circle.setAttribute("stroke-dasharray", `${Math.max(0, share - 1)} ${101 - share}`);
                circle.setAttribute("stroke-dashoffset", String(-offset));
                offset += share;
                this.canvas.append(circle);
            });
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
