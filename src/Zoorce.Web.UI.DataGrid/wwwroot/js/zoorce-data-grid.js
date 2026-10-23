(() => {
    const instances = new WeakMap();
    const svgNamespace = "http://www.w3.org/2000/svg";

    const parseConfiguration = (root, selector) => {
        const source = root.querySelector(selector)?.textContent;
        if (!source) return null;
        try {
            return JSON.parse(source);
        } catch {
            root.dispatchEvent(new CustomEvent("zoorce:component-error", { detail: { message: "Invalid component configuration." } }));
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
            this.connectInvalidation();
            this.load(1, { reset: true });
        }

        bind() {
            const signal = this.abortController.signal;
            this.root.addEventListener("zoorce:grid:insert", event => this.insert(event.detail?.item), { signal });
            this.root.addEventListener("zoorce:grid:refresh", () => this.load(1, { reset: true }), { signal });
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
            try {
                const result = await window.ZoorceData.request({
                    source: this.configuration.binding?.source,
                    query: { page, pageSize: this.configuration.pageSize, search: this.search, sort: this.sort, descending: this.descending || undefined },
                    signal: request.signal
                });
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
                dispatch(this.root, "zoorce:grid:loaded", { page, append, result });
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to load records.");
                    dispatch(this.root, "zoorce:grid:error", { error });
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
            dispatch(this.root, "zoorce:grid:inserted", { item });
        }

        setState(message) {
            if (this.state) this.state.textContent = message;
        }

        connectInvalidation() {
            if (!this.configuration.binding?.invalidationScope || !window.ZoorceData?.subscribe) return;
            this.unsubscribeRealtime = window.ZoorceData.subscribe(this.configuration.binding.invalidationScope, invalidation => {
                const change = invalidation?.change ?? invalidation;
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
                dispatch(this.root, "zoorce:grid:realtime", { change });
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
    const mountComponent = root => {
        if (instances.has(root)) return;
        const configuration = parseConfiguration(root, "[data-z-grid-config]");
        if (!configuration) return;
        instances.set(root, new DataGridController(root, configuration));
    };
    const mount = (scope = document) => {
        if (scope.matches?.("[data-z-data-grid]")) mountComponent(scope);
        scope.querySelectorAll?.("[data-z-data-grid]").forEach(mountComponent);
    };
    const dispose = root => {
        const controller = instances.get(root);
        controller?.dispose();
        instances.delete(root);
    };
    window.ZoorceDataGrid = Object.freeze({ mount, dispose });
    mount();
})();
