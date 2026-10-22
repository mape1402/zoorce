(() => {
    "use strict";

    const controllers = new WeakMap();
    const states = ["Default", "Highlighted", "Pending", "Completed", "Error"];
    const tones = ["Auto", "Neutral", "Primary", "Info", "Success", "Warning", "Danger"];
    const placements = ["Auto", "Left", "Right", "Above", "Below"];
    const overflowModes = ["Auto", "Scroll", "Stack"];
    const enumValue = (value, values, fallback) => Number.isInteger(value) && value >= 0 && value < values.length
        ? values[value]
        : values.find(candidate => candidate.toLowerCase() === String(value ?? "").toLowerCase()) ?? fallback;
    const safeText = value => typeof value === "string" ? value.trim() : "";
    const parseConfiguration = root => {
        try { return JSON.parse(root.querySelector("[data-z-timeline-config]")?.textContent ?? "null") ?? {}; }
        catch { return {}; }
    };
    const formatTime = (value, agenda) => {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";
        return new Intl.DateTimeFormat(undefined, agenda
            ? { hour: "2-digit", minute: "2-digit" }
            : { month: "short", day: "numeric", year: "numeric" }).format(date);
    };
    const relativeTime = value => {
        const difference = Math.max(0, Date.now() - new Date(value).getTime());
        const minutes = Math.floor(difference / 60000);
        if (minutes < 1) return "Just now";
        if (minutes < 60) return `${minutes}m ago`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        return `${Math.floor(hours / 24)}d ago`;
    };

    class TimelineController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.unsubscribeRealtime = null;
            this.viewport = root.querySelector("[data-z-timeline-scroll-viewport]");
            this.list = root.querySelector("[data-z-timeline-list]");
            this.resizeObserver = typeof ResizeObserver === "undefined" ? null : new ResizeObserver(() => this.syncPresentation());
            this.resizeObserver?.observe(root);
            this.resizeObserver?.observe(this.list);
            this.bind();
            this.syncPresentation();
            if (configuration.sourceUrl) void this.load();
            this.connectRealtime();
        }

        items() { return [...this.root.querySelectorAll("[data-z-timeline-item]")]; }

        selectedItem() { return this.items().find(item => item.dataset.zTimelineItemId === this.root.dataset.zTimelineSelected) ?? null; }

        announce(message) {
            const live = this.root.querySelector("[data-z-timeline-live]");
            if (live) live.textContent = message;
        }

        bind() {
            const signal = this.abortController.signal;
            this.viewport?.addEventListener("scroll", () => this.syncOverflowControls(), { signal });
            this.viewport?.addEventListener("wheel", event => this.handleWheel(event), { signal, passive: false });
            this.root.addEventListener("click", event => {
                const scrollControl = event.target.closest("[data-z-timeline-scroll]");
                if (scrollControl && this.root.contains(scrollControl)) {
                    event.preventDefault();
                    this.scrollBy(scrollControl.dataset.zTimelineScroll === "previous" ? -1 : 1);
                    return;
                }
                const action = event.target.closest("[data-z-timeline-action]");
                if (action && this.root.contains(action)) {
                    const item = action.closest("[data-z-timeline-item]");
                    if (!item) return;
                    if (action.tagName === "A") return;
                    event.preventDefault();
                    this.dispatchAction(item, action.dataset.zTimelineActionId ?? "");
                    return;
                }
                const select = event.target.closest("[data-z-timeline-select]");
                if (select && this.root.contains(select)) {
                    event.preventDefault();
                    this.select(select.closest("[data-z-timeline-item]"), true);
                }
            }, { signal });
            this.root.addEventListener("keydown", event => this.handleKeyboard(event), { signal });
        }

        handleKeyboard(event) {
            if (event.target === this.viewport && this.root.dataset.zTimelineOverflowing === "true") {
                const direction = event.key === "ArrowRight" ? 1 : event.key === "ArrowLeft" ? -1 : 0;
                if (direction) {
                    event.preventDefault();
                    this.scrollBy(direction);
                    return;
                }
                if (event.key === "Home" || event.key === "End") {
                    event.preventDefault();
                    this.viewport?.scrollTo({ left: event.key === "Home" ? 0 : this.viewport.scrollWidth, behavior: "smooth" });
                    return;
                }
            }
            const select = event.target.closest("[data-z-timeline-select]");
            if (!select || !this.root.contains(select)) return;
            const items = this.items().filter(item => item.querySelector("[data-z-timeline-select]"));
            const index = items.indexOf(select.closest("[data-z-timeline-item]"));
            let target = null;
            const horizontal = this.root.dataset.zTimelineEffectiveOrientation === "Horizontal";
            if (event.key === (horizontal ? "ArrowRight" : "ArrowDown")) target = index + 1;
            if (event.key === (horizontal ? "ArrowLeft" : "ArrowUp")) target = index - 1;
            if (event.key === "Home") target = 0;
            if (event.key === "End") target = items.length - 1;
            if (target === null) return;
            event.preventDefault();
            items[Math.max(0, Math.min(items.length - 1, target))]?.querySelector("[data-z-timeline-select]")?.focus({ preventScroll: true });
        }

        dispatchAction(item, actionId) {
            const detail = { timelineId: this.root.id, itemId: item.dataset.zTimelineItemId, actionId };
            this.root.dispatchEvent(new CustomEvent("zoerce:timeline:action", { bubbles: true, detail }));
        }

        select(item, focus) {
            if (!item || item.dataset.zTimelineSelectable === "false") return;
            const id = item.dataset.zTimelineItemId;
            if (!id) return;
            const request = new CustomEvent("zoerce:timeline:request-select", { bubbles: true, cancelable: true, detail: { timelineId: this.root.id, itemId: id } });
            if (!this.root.dispatchEvent(request)) return;
            this.root.dataset.zTimelineSelected = id;
            this.items().forEach(candidate => {
                const selected = candidate === item;
                candidate.classList.toggle("is-selected", selected);
                const button = candidate.querySelector("[data-z-timeline-select]");
                if (button) {
                    button.setAttribute("aria-pressed", String(selected));
                    button.tabIndex = selected ? 0 : -1;
                }
            });
            this.announce(`${item.querySelector("[data-z-timeline-title]")?.textContent ?? "Event"} selected.`);
            this.root.dispatchEvent(new CustomEvent("zoerce:timeline:select", { bubbles: true, detail: { timelineId: this.root.id, itemId: id } }));
            this.revealItem(item);
            if (focus) item.querySelector("[data-z-timeline-select]")?.focus({ preventScroll: true });
        }

        syncPresentation() {
            const configured = this.configuration.orientation ?? "Auto";
            const horizontalVariant = ["Milestones", "Presentation"].includes(this.configuration.variant);
            const overflowMode = enumValue(this.configuration.overflowMode, overflowModes, "Auto");
            let orientation = configured === "Auto" ? (horizontalVariant ? "Horizontal" : "Vertical") : enumValue(configured, ["Auto", "Vertical", "Horizontal"], "Auto");
            if (orientation === "Auto") orientation = horizontalVariant ? "Horizontal" : "Vertical";

            this.root.dataset.zTimelineEffectiveOrientation = orientation;
            if (orientation === "Horizontal" && overflowMode === "Stack" && this.hasHorizontalOverflow()) {
                orientation = "Vertical";
                this.root.dataset.zTimelineEffectiveOrientation = orientation;
            }

            this.root.dataset.zTimelineOverflowing = String(orientation === "Horizontal" && this.hasHorizontalOverflow());
            this.syncSegments();
            this.syncOverflowControls();
        }

        hasHorizontalOverflow() {
            if (!this.viewport || !this.list) return false;
            return this.list.scrollWidth > this.viewport.clientWidth + 1;
        }

        syncSegments() {
            const items = this.items();
            items.forEach((item, index) => {
                item.dataset.zTimelineSegment = items.length === 1 ? "only" : index === 0 ? "start" : index === items.length - 1 ? "end" : "middle";
            });
        }

        syncOverflowControls() {
            const viewport = this.viewport;
            if (!viewport) return;
            const overflowing = this.root.dataset.zTimelineOverflowing === "true";
            const visible = overflowing && this.configuration.showOverflowControls !== false;
            const maximum = Math.max(0, viewport.scrollWidth - viewport.clientWidth);
            const atStart = viewport.scrollLeft <= 1;
            const atEnd = viewport.scrollLeft >= maximum - 1;

            this.root.querySelectorAll("[data-z-timeline-scroll]").forEach(control => {
                const previous = control.dataset.zTimelineScroll === "previous";
                control.hidden = !visible || (previous ? atStart : atEnd);
                control.disabled = !visible;
            });
            viewport.tabIndex = visible ? 0 : -1;
            viewport.setAttribute("aria-label", visible ? `${this.root.getAttribute("aria-label") ?? "Timeline"}. Scrollable timeline events.` : this.root.getAttribute("aria-label") ?? "Timeline");
        }

        handleWheel(event) {
            const viewport = this.viewport;
            if (!viewport || this.root.dataset.zTimelineOverflowing !== "true") return;
            const delta = Math.abs(event.deltaX) > Math.abs(event.deltaY) ? event.deltaX : event.deltaY;
            if (!delta) return;
            const maximum = Math.max(0, viewport.scrollWidth - viewport.clientWidth);
            const next = Math.max(0, Math.min(maximum, viewport.scrollLeft + delta));
            if (Math.abs(next - viewport.scrollLeft) < .5) return;
            event.preventDefault();
            viewport.scrollLeft = next;
        }

        scrollBy(direction) {
            const viewport = this.viewport;
            if (!viewport || this.root.dataset.zTimelineOverflowing !== "true") return;
            viewport.scrollBy({ left: direction * Math.max(180, Math.floor(viewport.clientWidth * .72)), behavior: "smooth" });
            window.requestAnimationFrame(() => this.syncOverflowControls());
        }

        revealItem(item) {
            const viewport = this.viewport;
            if (!viewport || this.root.dataset.zTimelineOverflowing !== "true") return;
            const maximum = Math.max(0, viewport.scrollWidth - viewport.clientWidth);
            const target = Math.max(0, Math.min(maximum, item.offsetLeft - Math.max(0, (viewport.clientWidth - item.offsetWidth) / 2)));
            viewport.scrollTo({ left: target, behavior: "smooth" });
            window.requestAnimationFrame(() => this.syncOverflowControls());
        }

        normalizeItem(update) {
            if (!update || typeof update !== "object") return null;
            const id = safeText(update.id);
            if (!id) return null;
            return {
                ...update,
                id,
                title: safeText(update.title) || "Event",
                description: safeText(update.description),
                icon: safeText(update.icon),
                timestamp: update.timestamp ?? new Date().toISOString(),
                tone: enumValue(update.tone, tones, "Auto"),
                state: enumValue(update.state, states, "Default"),
                placement: enumValue(update.placement, placements, "Auto")
            };
        }

        applyItemData(element, data) {
            element.dataset.zTimelineTimestamp = data.timestamp;
            element.dataset.zTimelineTone = data.tone;
            element.dataset.zTimelineState = data.state;
            element.dataset.zTimelinePlacement = data.placement;
            states.forEach(state => element.classList.remove(`is-${state.toLowerCase()}`));
            tones.forEach(tone => element.classList.remove(`tone-${tone.toLowerCase()}`));
            placements.forEach(placement => element.classList.remove(`placement-${placement.toLowerCase()}`));
            element.classList.add(`is-${data.state.toLowerCase()}`, `tone-${data.tone.toLowerCase()}`, `placement-${data.placement.toLowerCase()}`);
            element.classList.toggle("is-unread", data.isRead === false);
            element.classList.toggle("is-pinned", data.isPinned === true);
            const title = element.querySelector("[data-z-timeline-title]");
            if (title) title.textContent = data.title;
            const description = element.querySelector("[data-z-timeline-description]");
            if (description && data.description) description.textContent = data.description;
            if (description && !data.description) description.remove();
            const marker = element.querySelector("[data-z-timeline-marker]");
            if (marker && data.icon) marker.textContent = data.icon;
            const time = element.querySelector("[data-z-timeline-time]");
            if (time) {
                time.dateTime = new Date(data.timestamp).toISOString();
                time.textContent = formatTime(data.timestamp, this.configuration.variant === "Agenda");
            }
            const relative = element.querySelector("[data-z-timeline-relative]");
            if (relative) relative.textContent = relativeTime(data.timestamp);
        }

        createItem(data) {
            const item = document.createElement("li");
            item.className = "z-timeline-item";
            item.dataset.zTimelineItem = "";
            item.dataset.zTimelineItemId = data.id;
            item.dataset.zTimelineSegment = "middle";
            item.dataset.zTimelineSelectable = String(data.isSelectable !== false);
            item.innerHTML = `<div class="z-timeline-rail" aria-hidden="true"><span class="z-timeline-marker" data-z-timeline-marker></span></div><div class="z-timeline-content"><time class="z-timeline-time" data-z-timeline-time></time><article class="z-timeline-card" data-z-timeline-card><div class="z-timeline-card-heading"><div><h3 data-z-timeline-title></h3><small class="z-timeline-relative" data-z-timeline-relative></small></div></div><p data-z-timeline-description></p><div class="z-timeline-meta" data-z-timeline-meta></div></article></div>`;
            if (this.configuration.interaction === "Select" || this.configuration.interaction === "Command") {
                const select = document.createElement("button");
                select.type = "button";
                select.className = "z-timeline-select";
                select.dataset.zTimelineSelect = "";
                select.setAttribute("aria-label", `Select ${data.title}`);
                select.setAttribute("aria-pressed", "false");
                select.tabIndex = -1;
                item.append(select);
            }
            this.applyItemData(item, data);
            this.syncSegments();
            return item;
        }

        upsert(data, options = {}) {
            const item = this.normalizeItem(data);
            if (!item) return false;
            const existing = this.items().find(candidate => candidate.dataset.zTimelineItemId === item.id);
            if (existing) {
                this.applyItemData(existing, item);
                return false;
            }
            const list = this.root.querySelector("[data-z-timeline-list]");
            const empty = this.root.querySelector("[data-z-timeline-empty]");
            empty?.remove();
            const element = this.createItem(item);
            const addFirst = this.configuration.sortAscending === false;
            list?.insertAdjacentElement(addFirst ? "afterbegin" : "beforeend", element);
            this.enforceMaxItems();
            this.syncPresentation();
            if (options.announce !== false) this.announce(`New event: ${item.title}.`);
            if (this.configuration.followLatest && options.follow !== false) element.scrollIntoView({ block: "nearest", behavior: "smooth" });
            return true;
        }

        remove(id) {
            const item = this.items().find(candidate => candidate.dataset.zTimelineItemId === id);
            if (!item) return;
            const wasSelected = item.classList.contains("is-selected");
            item.remove();
            if (wasSelected) {
                this.root.dataset.zTimelineSelected = "";
                this.items()[0] && this.select(this.items()[0], false);
            }
            if (!this.items().length) {
                const empty = document.createElement("li");
                empty.className = "z-timeline-empty";
                empty.dataset.zTimelineEmpty = "";
                empty.textContent = "No events to display.";
                this.root.querySelector("[data-z-timeline-list]")?.append(empty);
            }
            this.syncPresentation();
        }

        enforceMaxItems() {
            const maximum = Number(this.configuration.maxItems);
            if (!Number.isFinite(maximum) || maximum < 1) return;
            const items = this.items();
            while (items.length > maximum) (this.configuration.sortAscending === false ? items.pop() : items.shift())?.remove();
        }

        applySnapshot(snapshot, options = {}) {
            const payload = snapshot?.timeline ?? snapshot;
            if (!payload) return;
            if (payload.operation === "remove" && payload.id) {
                this.remove(payload.id);
                return;
            }
            const updates = Array.isArray(payload) ? payload : (Array.isArray(payload.items) ? payload.items : [payload.item ?? payload]);
            if ((payload.replace === true || this.configuration.refreshMode === "Replace") && Array.isArray(payload.items)) this.items().forEach(item => item.remove());
            let additions = 0;
            updates.forEach(update => { if (this.upsert(update, { announce: false, follow: false })) additions += 1; });
            if (typeof payload.selectedItemId === "string") this.select(this.items().find(item => item.dataset.zTimelineItemId === payload.selectedItemId), false);
            if (additions) this.announce(`${additions} new ${additions === 1 ? "event" : "events"} received.`);
            this.syncPresentation();
            this.root.dispatchEvent(new CustomEvent("zoerce:timeline:items-changed", { bubbles: true, detail: { timelineId: this.root.id, additions } }));
        }

        async load() {
            try {
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.sourceUrl, { headers: { Accept: "application/json" }, signal: this.abortController.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                this.applySnapshot(await response.json(), { source: "rest" });
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.announce("Unable to refresh timeline events.");
                    this.root.dispatchEvent(new CustomEvent("zoerce:timeline:error", { bubbles: true, detail: { error } }));
                }
            } finally { this.root.removeAttribute("aria-busy"); }
        }

        connectRealtime() {
            if (!this.configuration.realtimeHubUrl || !this.configuration.realtimeEventName) return;
            if (!window.ZoorceSignalR?.subscribe) {
                document.addEventListener("zoerce:signalr-ready", () => this.connectRealtime(), { once: true, signal: this.abortController.signal });
                return;
            }
            this.unsubscribeRealtime = window.ZoorceSignalR.subscribe({ hubUrl: this.configuration.realtimeHubUrl, eventName: this.configuration.realtimeEventName, transport: this.configuration.realtimeTransport }, update => {
                this.applySnapshot(update, { source: "realtime" });
                this.root.dispatchEvent(new CustomEvent("zoerce:timeline:realtime", { bubbles: true, detail: { timelineId: this.root.id, update } }));
            });
        }

        dispose() {
            this.abortController.abort();
            this.resizeObserver?.disconnect();
            this.unsubscribeRealtime?.();
        }
    }

    const initialize = root => {
        if (!(root instanceof HTMLElement) || controllers.has(root)) return;
        controllers.set(root, new TimelineController(root, parseConfiguration(root)));
    };
    const dispose = root => {
        const controller = controllers.get(root);
        controller?.dispose();
        controllers.delete(root);
    };
    const initializeAll = scope => (scope ?? document).querySelectorAll?.("[data-z-timeline]").forEach(initialize);

    window.ZoorceTimeline = { initialize, initializeAll, dispose };
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", () => initializeAll());
    else initializeAll();
})();
