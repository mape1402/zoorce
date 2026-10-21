(() => {
    "use strict";

    const controllers = new WeakMap();
    const states = ["Pending", "Active", "Completed", "Attention", "Error", "Disabled"];
    const tones = ["Auto", "Neutral", "Primary", "Success", "Warning", "Danger", "Info"];
    const stateText = {
        Pending: "Pending",
        Active: "In progress",
        Completed: "Completed",
        Attention: "Needs review",
        Error: "Action required",
        Disabled: "Unavailable"
    };
    const normalizeEnum = (value, values, fallback) => Number.isInteger(value) && value >= 0 && value < values.length
        ? values[value]
        : values.find(candidate => candidate.toLowerCase() === String(value ?? "").toLowerCase()) ?? fallback;
    const normalizeState = value => normalizeEnum(value, states, "Pending");
    const normalizeTone = value => normalizeEnum(value, tones, "Auto");
    const parseConfiguration = root => {
        try { return JSON.parse(root.querySelector("[data-z-stepper-config]")?.textContent ?? "null"); }
        catch { return null; }
    };

    class StepperController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.unsubscribeRealtime = null;
            this.resizeObserver = typeof ResizeObserver === "undefined" ? null : new ResizeObserver(() => this.syncPresentation());
            this.resizeObserver?.observe(root);
            this.bind();
            this.syncPresentation();
            this.syncControls();
            if (configuration.sourceUrl) this.load();
            this.connectRealtime();
        }

        items() { return [...this.root.querySelectorAll("[data-z-stepper-step]")]; }

        currentIndex() {
            const configured = this.root.dataset.zStepperCurrent;
            const byId = this.items().findIndex(item => item.dataset.zStepId === configured);
            return byId >= 0 ? byId : 0;
        }

        itemState(item) { return normalizeState(item.dataset.zStepState); }

        rules() { return this.configuration.transitionRules ?? {}; }

        announce(message) {
            const live = this.root.querySelector("[data-z-stepper-live]");
            if (live) live.textContent = message;
        }

        bind() {
            const signal = this.abortController.signal;
            this.root.querySelectorAll("[data-z-stepper-trigger]").forEach(control => {
                if (control.tagName !== "BUTTON") return;
                control.addEventListener("click", event => {
                    event.preventDefault();
                    const item = control.closest("[data-z-stepper-step]");
                    if (item) void this.requestChange(this.items().indexOf(item), "step");
                }, { signal });
                control.addEventListener("keydown", event => this.handleKeyboard(event, control), { signal });
            });
            this.root.querySelector("[data-z-stepper-previous]")?.addEventListener("click", () => void this.requestChange(this.currentIndex() - 1, "previous"), { signal });
            this.root.querySelector("[data-z-stepper-next]")?.addEventListener("click", () => void this.requestChange(this.currentIndex() + 1, "next"), { signal });
        }

        handleKeyboard(event, control) {
            const horizontal = this.root.dataset.zStepperEffectiveOrientation === "Horizontal";
            const previous = horizontal ? "ArrowLeft" : "ArrowUp";
            const next = horizontal ? "ArrowRight" : "ArrowDown";
            const index = this.items().indexOf(control.closest("[data-z-stepper-step]"));
            let target = null;
            if (event.key === previous) target = index - 1;
            if (event.key === next) target = index + 1;
            if (event.key === "Home") target = 0;
            if (event.key === "End") target = this.items().length - 1;
            if (target === null) return;
            event.preventDefault();
            const candidate = this.items()[Math.min(this.items().length - 1, Math.max(0, target))]?.querySelector("[data-z-stepper-trigger]");
            candidate?.focus({ preventScroll: true });
        }

        canChange(targetIndex, reason) {
            const items = this.items();
            const currentIndex = this.currentIndex();
            const target = items[targetIndex];
            const current = items[currentIndex];
            const rules = this.rules();
            if (!target || targetIndex === currentIndex) return { allowed: false, message: "This step is already active." };
            if (target.dataset.zStepNavigable !== "true" || this.itemState(target) === "Disabled") return { allowed: false, message: "This step is unavailable." };
            if (targetIndex < currentIndex) {
                const backward = rules.backward ?? "Auto";
                if (backward === "Block") return { allowed: false, message: "Going back is not available in this workflow." };
                if (backward === "CompletedOnly") return this.itemState(target) === "Completed"
                    ? { allowed: true, message: "" }
                    : { allowed: false, message: "Only completed steps can be revisited." };
                if (backward === "AnyEligible" || reason === "previous") return { allowed: true, message: "" };
                return this.configuration.allowCompletedNavigation || this.itemState(target) === "Completed"
                    ? { allowed: true, message: "" }
                    : { allowed: false, message: "Completed steps cannot be revisited." };
            }
            const currentState = this.itemState(current);
            const errorRule = rules.error ?? "Block";
            const warningRule = rules.warning ?? "Allow";
            if (currentState === "Error" && errorRule === "Block") return { allowed: false, message: "Resolve the current step before continuing." };
            if (currentState === "Attention" && warningRule === "Block") return { allowed: false, message: "Resolve the warning before continuing." };
            const requiresAuthorization = (currentState === "Error" && errorRule === "RequireAuthorization") || (currentState === "Attention" && warningRule === "RequireAuthorization");
            const forward = rules.forward ?? "Auto";
            if (forward === "Block") return { allowed: false, message: "Forward progress is controlled by this workflow." };
            if (forward === "AdjacentOnly") return targetIndex === currentIndex + 1
                ? { allowed: true, message: "", requiresAuthorization }
                : { allowed: false, message: "Complete the current step before skipping ahead." };
            if (forward === "AnyEligible") return { allowed: true, message: "", requiresAuthorization };
            if (this.configuration.interaction === "Wizard") return targetIndex === currentIndex + 1
                ? { allowed: true, message: "", requiresAuthorization }
                : { allowed: false, message: "Complete the current step before skipping ahead." };
            return this.configuration.allowFutureNavigation
                ? { allowed: true, message: "", requiresAuthorization }
                : { allowed: false, message: "Future steps are not available yet." };
        }

        async requestChange(targetIndex, reason) {
            const permission = this.canChange(targetIndex, reason);
            if (!permission.allowed) {
                if (permission.message) this.announce(permission.message);
                return false;
            }
            const previousIndex = this.currentIndex();
            const items = this.items();
            const detail = {
                stepId: items[targetIndex].dataset.zStepId,
                previousStepId: items[previousIndex].dataset.zStepId,
                reason,
                isBackward: targetIndex < previousIndex,
                requiresAuthorization: Boolean(permission.requiresAuthorization)
            };
            const request = new CustomEvent("zoerce:stepper:request-change", { bubbles: true, cancelable: true, detail });
            if (!this.root.dispatchEvent(request)) return false;
            const decision = await this.authorizeTransition(detail);
            if (!decision.allowed) {
                this.announce(decision.message ?? "This transition is not authorized.");
                return false;
            }
            if (decision.snapshot) this.applySnapshot(decision.snapshot);
            else this.applyLocalTransition(targetIndex, previousIndex, reason);
            this.root.dispatchEvent(new CustomEvent("zoerce:stepper:change", { bubbles: true, detail }));
            const targetUrl = items[targetIndex].dataset.zStepTargetUrl;
            if (targetUrl) window.location.assign(targetUrl);
            return true;
        }

        applyLocalTransition(targetIndex, previousIndex, reason) {
            const items = this.items();
            const effect = this.rules().onBackward ?? "Auto";
            const resetFollowing = targetIndex < previousIndex && (effect === "ResetFollowing" || (effect === "Auto" && this.configuration.interaction === "Wizard"));
            if (resetFollowing) {
                items.slice(targetIndex + 1).forEach(item => {
                    if (this.itemState(item) !== "Disabled") this.setState(item, "Pending");
                });
                this.setState(items[targetIndex], "Active");
            } else {
                if (reason === "next" && this.itemState(items[previousIndex]) !== "Attention") this.setState(items[previousIndex], "Completed");
                if (this.itemState(items[targetIndex]) === "Pending") this.setState(items[targetIndex], "Active");
            }
            this.setCurrent(targetIndex, { focus: false });
        }

        async authorizeTransition(detail) {
            if (!this.configuration.transitionUrl) return { allowed: true };
            try {
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.transitionUrl, {
                    method: "POST",
                    credentials: "same-origin",
                    headers: { Accept: "application/json", "Content-Type": "application/json" },
                    body: JSON.stringify({ stepperId: this.root.id, ...detail })
                });
                if (!response.ok) throw new Error(`Transition failed with ${response.status}`);
                const payload = await response.json();
                const snapshot = payload?.snapshot ?? (Array.isArray(payload?.steps) ? payload : null);
                return { allowed: payload?.allowed !== false, message: payload?.message, snapshot };
            } catch (error) {
                this.root.dispatchEvent(new CustomEvent("zoerce:stepper:error", { bubbles: true, detail: { error } }));
                return { allowed: false, message: "Unable to validate this transition." };
            } finally { this.root.removeAttribute("aria-busy"); }
        }

        setState(item, state) {
            const normalized = normalizeState(state);
            item.dataset.zStepState = normalized;
            states.forEach(candidate => item.classList.remove(`is-${candidate.toLowerCase()}`));
            item.classList.add(`is-${normalized.toLowerCase()}`);
            const status = item.querySelector("[data-z-stepper-status]");
            if (status && !item.dataset.zStepOptional) status.textContent = stateText[normalized];
            const marker = item.querySelector("[data-z-stepper-marker]");
            const markerStyle = this.root.dataset.zStepperMarkerStyle;
            if (marker && markerStyle !== "Dot" && markerStyle !== "None") {
                marker.textContent = normalized === "Completed" ? "✓" : (marker.dataset.zStepperMarkerDefault ?? "");
            }
        }

        setTone(item, tone) {
            const normalized = normalizeTone(tone);
            item.dataset.zStepTone = normalized;
            tones.forEach(candidate => item.classList.remove(`tone-${candidate.toLowerCase()}`));
            item.classList.toggle("has-tone", normalized !== "Auto");
            if (normalized !== "Auto") item.classList.add(`tone-${normalized.toLowerCase()}`);
        }

        setCurrent(index, options = {}) {
            const items = this.items();
            const item = items[index];
            if (!item) return;
            this.root.dataset.zStepperCurrent = item.dataset.zStepId;
            items.forEach((candidate, candidateIndex) => {
                const current = candidateIndex === index;
                candidate.classList.toggle("is-current", current);
                const trigger = candidate.querySelector("[data-z-stepper-trigger]");
                if (trigger) {
                    if (current) trigger.setAttribute("aria-current", "step");
                    else trigger.removeAttribute("aria-current");
                    if (trigger.tagName === "BUTTON") trigger.tabIndex = current ? 0 : -1;
                }
            });
            this.syncControls();
            this.announce(`${item.querySelector("[data-z-stepper-title]")?.textContent ?? "Step"} is current.`);
            if (options.focus) item.querySelector("[data-z-stepper-trigger]")?.focus({ preventScroll: true });
        }

        syncControls() {
            const currentIndex = this.currentIndex();
            const items = this.items();
            const previous = this.root.querySelector("[data-z-stepper-previous]");
            const next = this.root.querySelector("[data-z-stepper-next]");
            if (previous) previous.disabled = currentIndex === 0;
            if (next) {
                const isFinal = currentIndex === items.length - 1;
                next.disabled = isFinal || this.itemState(items[currentIndex]) === "Error";
                next.textContent = isFinal
                    ? (next.dataset.zStepperCompleteLabel ?? "Complete")
                    : (next.dataset.zStepperNextLabel ?? "Continue");
            }
        }

        syncPresentation() {
            const configured = this.configuration.orientation ?? "Auto";
            const width = this.root.getBoundingClientRect().width;
            let orientation = configured;
            if (configured === "Auto") orientation = width < Math.max(560, this.items().length * 142) ? "Vertical" : "Horizontal";
            if (width <= 700) orientation = "Vertical";
            if (this.configuration.variant === "Sidebar") orientation = "Vertical";
            this.root.dataset.zStepperEffectiveOrientation = orientation;
            this.root.dataset.zStepperEffectiveVariant = this.configuration.variant === "Segmented" && orientation === "Vertical" ? "Rail" : this.configuration.variant;
        }

        applySnapshot(snapshot) {
            const update = snapshot?.stepper ?? snapshot;
            if (!update || typeof update !== "object") return;
            (Array.isArray(update.steps) ? update.steps : []).forEach(next => {
                const item = this.items().find(candidate => candidate.dataset.zStepId === next.id);
                if (!item) return;
                if (next.state !== undefined && next.state !== null) this.setState(item, next.state);
                if (next.tone !== undefined && next.tone !== null) this.setTone(item, next.tone);
                if (typeof next.isNavigable === "boolean") item.dataset.zStepNavigable = String(next.isNavigable);
                if (typeof next.targetUrl === "string") item.dataset.zStepTargetUrl = next.targetUrl;
                const title = item.querySelector("[data-z-stepper-title]");
                const supporting = item.querySelector("[data-z-stepper-supporting]");
                const status = item.querySelector("[data-z-stepper-status]");
                if (title && typeof next.title === "string") title.textContent = next.title;
                if (supporting && typeof next.supportingText === "string") supporting.textContent = next.supportingText;
                if (status && typeof next.statusText === "string") status.textContent = next.statusText;
                const marker = item.querySelector("[data-z-stepper-marker]");
                const markerStyle = this.root.dataset.zStepperMarkerStyle;
                if (marker && typeof next.icon === "string" && ["Auto", "Icon", "Check"].includes(markerStyle)) {
                    marker.dataset.zStepperMarkerDefault = next.icon;
                    if (this.itemState(item) !== "Completed") marker.textContent = next.icon;
                }
            });
            const requestedCurrent = update.currentStepId ?? update.currentStepID;
            const currentIndex = this.items().findIndex(item => item.dataset.zStepId === requestedCurrent);
            if (currentIndex >= 0) this.setCurrent(currentIndex);
            else this.syncControls();
        }

        async load() {
            try {
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.sourceUrl, { headers: { Accept: "application/json" }, signal: this.abortController.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                const snapshot = await response.json();
                this.applySnapshot(snapshot);
                this.root.dispatchEvent(new CustomEvent("zoerce:stepper:loaded", { bubbles: true, detail: { snapshot } }));
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.announce("Unable to refresh workflow progress.");
                    this.root.dispatchEvent(new CustomEvent("zoerce:stepper:error", { bubbles: true, detail: { error } }));
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
                this.applySnapshot(update);
                this.root.dispatchEvent(new CustomEvent("zoerce:stepper:realtime", { bubbles: true, detail: { update } }));
            });
        }

        dispose() {
            this.abortController.abort();
            this.resizeObserver?.disconnect();
            this.unsubscribeRealtime?.();
        }
    }

    const mount = (scope = document) => {
        const roots = scope.matches?.("[data-z-stepper]") ? [scope] : [...(scope.querySelectorAll?.("[data-z-stepper]") ?? [])];
        roots.forEach(root => {
            if (controllers.has(root)) return;
            const configuration = parseConfiguration(root);
            if (!configuration) return;
            controllers.set(root, new StepperController(root, configuration));
        });
    };
    const dispose = root => { controllers.get(root)?.dispose(); controllers.delete(root); };

    mount();
    window.ZoorceSteppers = Object.freeze({ mount, dispose });
})();
