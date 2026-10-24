(() => {
    "use strict";

    const workflow = window.ZoorceWorkflow;
    const controllers = new WeakMap();
    const parseConfiguration = root => {
        try { return JSON.parse(root.querySelector("[data-z-wizard-config]")?.textContent ?? "null"); }
        catch { return null; }
    };
    const selectorEscape = value => window.CSS?.escape ? window.CSS.escape(value) : String(value).replace(/[^a-zA-Z0-9_-]/g, "\\$&");

    class WizardController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = configuration;
            this.abortController = new AbortController();
            this.pending = false;
            this.bind();
            this.sync();
        }

        items() { return [...this.root.querySelectorAll("[data-z-wizard-step]")]; }
        panels() { return [...this.root.querySelectorAll("[data-z-wizard-panel]")]; }
        currentIndex() {
            const index = this.items().findIndex(item => item.dataset.zStepId === this.root.dataset.zWizardCurrent);
            return index >= 0 ? index : 0;
        }
        currentItem() { return this.items()[this.currentIndex()]; }
        currentStepId() { return this.currentItem()?.dataset.zStepId ?? ""; }
        state(item) { return workflow.normalizeState(item?.dataset.zStepState); }
        rules() { return this.configuration.transitionRules ?? {}; }
        announce(message) {
            const live = this.root.querySelector("[data-z-wizard-live]");
            if (live) live.textContent = message;
        }

        bind() {
            const signal = this.abortController.signal;
            this.root.querySelectorAll("[data-z-wizard-step-trigger]").forEach(button => {
                button.addEventListener("click", () => {
                    const item = button.closest("[data-z-wizard-step]");
                    if (item) void this.requestChange(this.items().indexOf(item), "step");
                }, { signal });
                button.addEventListener("keydown", event => this.handleKeyboard(event, button), { signal });
            });
            this.root.querySelector("[data-z-wizard-previous]")?.addEventListener("click", () => void this.requestChange(this.currentIndex() - 1, "previous"), { signal });
            this.root.querySelector("[data-z-wizard-next]")?.addEventListener("click", () => {
                const current = this.currentIndex();
                if (current === this.items().length - 1) void this.requestFinish();
                else void this.requestChange(current + 1, "next");
            }, { signal });
            this.root.querySelector("[data-z-wizard-save]")?.addEventListener("click", () => void this.requestStandaloneAction("save"), { signal });
            this.root.querySelector("[data-z-wizard-cancel]")?.addEventListener("click", () => void this.requestStandaloneAction("cancel"), { signal });
        }

        handleKeyboard(event, button) {
            const horizontal = this.configuration.layout === "Top";
            const previous = horizontal ? "ArrowLeft" : "ArrowUp";
            const next = horizontal ? "ArrowRight" : "ArrowDown";
            const index = this.items().indexOf(button.closest("[data-z-wizard-step]"));
            let target = null;
            if (event.key === previous) target = index - 1;
            if (event.key === next) target = index + 1;
            if (event.key === "Home") target = 0;
            if (event.key === "End") target = this.items().length - 1;
            if (target === null) return;
            event.preventDefault();
            this.items()[Math.min(this.items().length - 1, Math.max(0, target))]?.querySelector("[data-z-wizard-step-trigger]")?.focus({ preventScroll: true });
        }

        canChange(targetIndex, reason) {
            return workflow.evaluateTransition({
                items: this.items().map(item => ({ state: this.state(item), navigable: item.dataset.zStepNavigable === "true" })),
                currentIndex: this.currentIndex(),
                targetIndex,
                reason,
                interaction: "Wizard",
                allowCompletedNavigation: this.configuration.allowCompletedNavigation,
                allowFutureNavigation: this.configuration.allowFutureNavigation,
                transitionRules: this.rules()
            });
        }

        async requestChange(targetIndex, reason) {
            if (this.pending) return false;
            const previousIndex = this.currentIndex();
            // A rail click is still a forward/backward transition. Normalize it to
            // the footer action so host validation and local state updates cannot
            // bypass the rules applied by Continue and Back.
            const action = targetIndex > previousIndex ? "next" : targetIndex < previousIndex ? "previous" : reason;
            const permission = this.canChange(targetIndex, action);
            if (!permission.allowed) {
                this.announce(permission.message);
                return false;
            }

            const targetId = this.items()[targetIndex].dataset.zStepId;
            const detail = this.createDetail(action, targetId, targetIndex < previousIndex, permission.requiresAuthorization);
            if (!this.dispatchRequest(detail, "change")) return false;

            if (this.configuration.executionMode === "Manual") return true;
            if (!detail.isBackward && !this.validateCurrentForm()) return false;
            const decision = await this.authorize(detail);
            if (!decision.allowed) return false;
            if (decision.snapshot) this.applySnapshot(decision.snapshot);
            else this.applyLocalTransition(targetIndex, previousIndex, action);
            this.emitChange(detail, true);
            return true;
        }

        async requestStandaloneAction(action) {
            if (this.pending) return false;
            const detail = this.createDetail(action, null, false, false);
            if (!this.dispatchRequest(detail, action)) return false;
            if (this.configuration.executionMode === "Manual") return true;
            if (action === "finish" && !this.validateCurrentForm()) return false;
            const decision = await this.authorize(detail);
            if (!decision.allowed) return false;
            if (decision.snapshot) this.applySnapshot(decision.snapshot);
            this.root.dispatchEvent(new CustomEvent(`zoorce:wizard:${action}`, { bubbles: true, detail: { ...detail, decision } }));
            if (decision.message) this.announce(decision.message);
            return true;
        }

        requestFinish() { return this.requestStandaloneAction("finish"); }

        createDetail(action, targetStepId, isBackward, requiresAuthorization) {
            return {
                wizardId: this.root.id,
                action,
                currentStepId: this.currentStepId(),
                targetStepId,
                isBackward: Boolean(isBackward),
                requiresAuthorization: Boolean(requiresAuthorization)
            };
        }

        dispatchRequest(detail, specific) {
            const actionRequest = new CustomEvent("zoorce:wizard:request-action", { bubbles: true, cancelable: true, detail });
            if (!this.root.dispatchEvent(actionRequest)) return false;
            const specificRequest = new CustomEvent(`zoorce:wizard:request-${specific}`, { bubbles: true, cancelable: true, detail });
            return this.root.dispatchEvent(specificRequest);
        }

        validateCurrentForm() {
            const form = this.currentForm();
            if (!form || typeof form.reportValidity !== "function" || form.reportValidity()) return true;
            this.announce("Resolve the highlighted fields before continuing.");
            return false;
        }

        currentForm() {
            const formId = this.currentItem()?.dataset.zStepFormId;
            return formId ? document.getElementById(formId) : null;
        }

        collectCurrentValues() {
            const form = this.currentForm();
            if (!form) return null;
            const values = {};
            new FormData(form).forEach((value, name) => {
                const text = typeof value === "string" ? value : value.name;
                if (!values[name]) values[name] = [];
                values[name].push(text);
            });
            return values;
        }

        async authorize(detail) {
            try {
                this.pending = true;
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.transitionUrl, {
                    method: "POST",
                    credentials: "same-origin",
                    headers: { Accept: "application/json", "Content-Type": "application/json" },
                    body: JSON.stringify({ ...detail, values: this.collectCurrentValues() })
                });
                const payload = await response.json().catch(() => ({}));
                const decision = {
                    allowed: response.ok && payload?.allowed !== false,
                    message: payload?.message,
                    snapshot: payload?.snapshot ?? null,
                    fieldErrors: payload?.fieldErrors ?? payload?.errors ?? null
                };
                if (!decision.allowed) {
                    this.showFieldErrors(decision.fieldErrors);
                    this.announce(decision.message ?? "This action is not authorized.");
                } else {
                    this.clearFieldErrors();
                }
                return decision;
            } catch (error) {
                this.root.dispatchEvent(new CustomEvent("zoorce:wizard:error", { bubbles: true, detail: { error } }));
                this.announce("Unable to validate this action.");
                return { allowed: false };
            } finally {
                this.pending = false;
                this.root.removeAttribute("aria-busy");
            }
        }

        commit(targetStepId, reason = "next") {
            const targetIndex = this.items().findIndex(item => item.dataset.zStepId === targetStepId);
            const permission = this.canChange(targetIndex, reason);
            if (!permission.allowed) {
                this.announce(permission.message);
                return false;
            }
            const detail = this.createDetail(reason, targetStepId, targetIndex < this.currentIndex(), permission.requiresAuthorization);
            this.applyLocalTransition(targetIndex, this.currentIndex(), reason);
            this.emitChange(detail, false);
            return true;
        }

        goTo(targetStepId) { return this.commit(targetStepId, "step"); }

        complete(message = "The wizard is complete.") {
            const current = this.currentItem();
            if (current) this.setState(current, "Completed");
            this.sync();
            this.announce(message);
            this.root.dispatchEvent(new CustomEvent("zoorce:wizard:finish", { bubbles: true, detail: this.createDetail("finish", null, false, false) }));
        }

        applyLocalTransition(targetIndex, previousIndex, reason) {
            const items = this.items();
            const plan = workflow.getLocalTransitionPlan({ currentIndex: previousIndex, targetIndex, reason, transitionRules: this.rules(), interaction: "Wizard" });
            if (plan.resetFollowing) {
                items.slice(targetIndex + 1).forEach(item => {
                    if (this.state(item) !== "Disabled") this.setState(item, "Pending");
                });
                this.setState(items[targetIndex], "Active");
            } else {
                if (plan.completePrevious && this.state(items[previousIndex]) !== "Attention") this.setState(items[previousIndex], "Completed");
                if (this.state(items[targetIndex]) === "Pending") this.setState(items[targetIndex], "Active");
            }
            this.setCurrent(targetIndex, true);
        }

        emitChange(detail, hostManaged) {
            this.root.dispatchEvent(new CustomEvent("zoorce:wizard:change", { bubbles: true, detail: { ...detail, hostManaged } }));
        }

        setCurrent(index, focus) {
            const item = this.items()[index];
            if (!item) return;
            this.root.dataset.zWizardCurrent = item.dataset.zStepId;
            this.sync();
            const title = item.querySelector("[data-z-wizard-step-title]")?.textContent ?? "Step";
            this.announce(`${title} is current. Step ${index + 1} of ${this.items().length}.`);
            if (focus) {
                const panel = this.panelFor(item.dataset.zStepId);
                const focusTarget = panel?.querySelector("[data-z-wizard-panel-focus]")
                    ?? panel?.querySelector("input:not([type='hidden']):not(:disabled), select:not(:disabled), textarea:not(:disabled), button:not(:disabled)")
                    ?? panel;
                focusTarget?.focus({ preventScroll: true });
            }
        }

        sync() {
            const index = this.currentIndex();
            const items = this.items();
            const current = items[index];
            items.forEach((item, itemIndex) => {
                const isCurrent = itemIndex === index;
                item.classList.toggle("is-current", isCurrent);
                const trigger = item.querySelector("[data-z-wizard-step-trigger]");
                if (trigger) {
                    if (isCurrent) trigger.setAttribute("aria-current", "step");
                    else trigger.removeAttribute("aria-current");
                    trigger.tabIndex = isCurrent ? 0 : -1;
                    const disabled = this.state(item) === "Disabled" || item.dataset.zStepNavigable !== "true";
                    trigger.setAttribute("aria-disabled", String(disabled));
                    trigger.disabled = disabled;
                }
            });
            this.panels().forEach(panel => { panel.hidden = panel.dataset.zStepId !== current?.dataset.zStepId; });
            const previous = this.root.querySelector("[data-z-wizard-previous]");
            if (previous) previous.disabled = index === 0;
            const next = this.root.querySelector("[data-z-wizard-next]");
            if (next && current) {
                const final = index === items.length - 1;
                const label = final
                    ? current.dataset.zStepFinishLabel || next.dataset.zWizardFinishLabel || "Finish"
                    : current.dataset.zStepNextLabel || next.dataset.zWizardNextLabel || "Continue";
                next.textContent = label;
                next.disabled = this.state(current) === "Error";
            }
            const save = this.root.querySelector("[data-z-wizard-save]");
            if (save && current) {
                const showSave = current.dataset.zStepShowSaveDraft === "true" || current.dataset.zStepShowSaveDraft === "" && this.configuration.showSaveDraft;
                save.hidden = !showSave;
            }
        }

        panelFor(stepId) { return this.panels().find(panel => panel.dataset.zStepId === stepId); }

        setState(item, state) {
            const normalized = workflow.normalizeState(state);
            item.dataset.zStepState = normalized;
            workflow.states.forEach(candidate => item.classList.remove(`is-${candidate.toLowerCase()}`));
            item.classList.add(`is-${normalized.toLowerCase()}`);
            const status = item.querySelector("[data-z-wizard-step-status]");
            if (status && item.dataset.zStepOptional !== "true") status.textContent = workflow.stateText[normalized];
            const marker = item.querySelector("[data-z-wizard-step-marker]");
            if (marker) marker.textContent = normalized === "Completed" ? "✓" : marker.dataset.zWizardMarkerDefault ?? "";
        }

        setTone(item, tone) {
            const normalized = workflow.normalizeTone(tone);
            item.dataset.zStepTone = normalized;
            workflow.tones.forEach(candidate => item.classList.remove(`tone-${candidate.toLowerCase()}`));
            item.classList.toggle("has-tone", normalized !== "Auto");
            if (normalized !== "Auto") item.classList.add(`tone-${normalized.toLowerCase()}`);
        }

        applySnapshot(snapshot) {
            if (!snapshot || typeof snapshot !== "object") return;
            (snapshot.steps ?? []).forEach(next => {
                const item = this.items().find(candidate => candidate.dataset.zStepId === next.id);
                if (!item) return;
                if (next.state !== undefined && next.state !== null) this.setState(item, next.state);
                if (next.tone !== undefined && next.tone !== null) this.setTone(item, next.tone);
                if (typeof next.isNavigable === "boolean") item.dataset.zStepNavigable = String(next.isNavigable);
                const title = item.querySelector("[data-z-wizard-step-title]");
                const supporting = item.querySelector("[data-z-wizard-step-supporting]");
                const status = item.querySelector("[data-z-wizard-step-status]");
                if (title && typeof next.title === "string") title.textContent = next.title;
                if (supporting && typeof next.supportingText === "string") supporting.textContent = next.supportingText;
                if (status && typeof next.statusText === "string") status.textContent = next.statusText;
            });
            const requested = snapshot.currentStepId;
            const index = this.items().findIndex(item => item.dataset.zStepId === requested);
            if (index >= 0) this.setCurrent(index, true);
            else this.sync();
        }

        clearFieldErrors() {
            const panel = this.panelFor(this.currentStepId());
            panel?.querySelectorAll("[data-z-wizard-field-error]").forEach(error => error.remove());
            panel?.querySelectorAll("[aria-invalid='true']").forEach(field => field.removeAttribute("aria-invalid"));
            const summary = panel?.querySelector("[data-z-wizard-field-errors]");
            if (summary) summary.replaceChildren();
        }

        showFieldErrors(errors) {
            this.clearFieldErrors();
            if (!errors || typeof errors !== "object") return;
            const panel = this.panelFor(this.currentStepId());
            if (!panel) return;
            const summary = panel.querySelector("[data-z-wizard-field-errors]");
            let firstInvalid = null;
            Object.entries(errors).forEach(([name, messages]) => {
                const field = panel.querySelector(`[name="${selectorEscape(name)}"], #${selectorEscape(name)}`);
                const text = Array.isArray(messages) ? messages.join(" ") : String(messages ?? "");
                const message = document.createElement("p");
                message.dataset.zWizardFieldError = "";
                message.className = "z-wizard-field-error";
                message.textContent = text;
                if (field) {
                    field.setAttribute("aria-invalid", "true");
                    field.insertAdjacentElement("afterend", message);
                    firstInvalid ??= field;
                } else summary?.append(message);
            });
            firstInvalid?.focus({ preventScroll: true });
        }

        dispose() { this.abortController.abort(); }
    }

    const mount = (scope = document) => {
        const roots = scope.matches?.("[data-z-wizard]") ? [scope] : [...(scope.querySelectorAll?.("[data-z-wizard]") ?? [])];
        roots.forEach(root => {
            if (controllers.has(root)) return;
            const configuration = parseConfiguration(root);
            if (configuration && workflow) controllers.set(root, new WizardController(root, configuration));
        });
    };
    const get = id => {
        const root = typeof id === "string" ? document.getElementById(id) : id;
        return root ? controllers.get(root) ?? null : null;
    };
    const dispose = root => { controllers.get(root)?.dispose(); controllers.delete(root); };

    mount();
    window.ZoorceWizards = Object.freeze({ mount, get, dispose });
})();
