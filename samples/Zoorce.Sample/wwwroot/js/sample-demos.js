(() => {
    "use strict";

    const notify = (title, message, kind = "success") => window.Zoorce?.[kind]?.(title, message);
    const number = value => Number.parseFloat(String(value).replaceAll(",", ""));

    // A host opts into its transport adapter. The sample deliberately proves the
    // transport-neutral invalidation path below, so it never assumes SignalR is present.
    if (document.documentElement.dataset.zoorceRealtime === "enabled" && window.ZoorceRealtimeSignalR && !window.__zoorceSampleRealtimeConnected) {
        window.__zoorceSampleRealtimeConnected = true;
        window.ZoorceRealtimeSignalR.subscribe({ hubUrl: "/hubs/component-activity", transport: "LongPolling" }, () => {});
    }

    document.querySelectorAll("[data-z-live-demo]").forEach(button => {
        button.addEventListener("click", async () => {
            if (button.disabled) return;
            const endpoint = button.dataset.zLiveEndpoint;
            if (!endpoint) return;
            const original = button.textContent;
            button.disabled = true;
            button.textContent = "Publishing…";
            try {
                const response = await fetch(endpoint, { method: "POST", headers: { Accept: "application/json" } });
                if (!response.ok) throw new Error(`The sample endpoint returned ${response.status}.`);
                const item = await response.json();
                // The initiating browser can update immediately. SignalR forwards the same invalidation to peers.
                window.ZoorceData?.invalidate?.({
                    scope: "components",
                    strategy: "Patch",
                    change: { kind: "Upsert", key: item.id, item }
                });
                window.ZoorceData?.invalidate?.({ scope: "adoption", strategy: "Replace" });
                notify("Live change published", "The affected component roots refreshed without rebuilding the page.", "success");
            } catch {
                notify("Live change unavailable", "The local sample endpoint could not be reached.", "warning");
            } finally {
                button.disabled = false;
                button.textContent = original;
            }
        });
    });

    const dashboardRanges = {
        "30d": { total: "18,492", trend: "↗ 24.8%", caption: "Shipped work across the last 30 days", path: "M0 148 C55 140 70 102 118 115 S188 139 222 93 S296 47 338 82 S396 120 433 71 S508 53 545 41 S610 61 650 16", fill: "M0 148 C55 140 70 102 118 115 S188 139 222 93 S296 47 338 82 S396 120 433 71 S508 53 545 41 S610 61 650 16 V180 H0 Z" },
        "90d": { total: "53,108", trend: "↗ 18.6%", caption: "Shipped work across the last 90 days", path: "M0 141 C47 110 84 137 123 109 S200 88 238 104 S319 64 357 86 S433 103 477 59 S550 41 595 55 S625 38 650 29", fill: "M0 141 C47 110 84 137 123 109 S200 88 238 104 S319 64 357 86 S433 103 477 59 S550 41 595 55 S625 38 650 29 V180 H0 Z" },
        Year: { total: "201,884", trend: "↗ 31.2%", caption: "Shipped work across the last 12 months", path: "M0 156 C52 142 82 125 122 134 S188 104 231 117 S290 78 337 89 S402 55 445 76 S515 37 555 52 S613 18 650 26", fill: "M0 156 C52 142 82 125 122 134 S188 104 231 117 S290 78 337 89 S402 55 445 76 S515 37 555 52 S613 18 650 26 V180 H0 Z" }
    };

    const setDashboardRange = (group, value) => {
        const card = group.closest(".z-chart-card");
        const detail = dashboardRanges[value];
        if (!card || !detail) return;
        const total = card.querySelector("[data-z-demo-chart-total], .z-chart-total");
        const caption = card.querySelector("[data-z-demo-chart-caption], .z-card-caption");
        const line = card.querySelector("[data-z-demo-chart-line], .z-chart-line");
        const fill = card.querySelector("[data-z-demo-chart-fill], .z-chart-fill");
        if (total) total.firstChild.textContent = `${detail.total} `;
        if (total?.querySelector("span")) total.querySelector("span").textContent = detail.trend;
        if (caption) caption.textContent = detail.caption;
        if (line) line.setAttribute("d", detail.path);
        if (fill) fill.setAttribute("d", detail.fill);
    };

    document.querySelectorAll("[data-z-demo-range], #dashboard .z-chart-card [data-z-selection-group]").forEach(group => {
        group.querySelectorAll("button").forEach(button => button.addEventListener("click", () => setDashboardRange(group, button.dataset.zDemoRangeValue ?? button.textContent.trim())));
    });

    document.querySelectorAll("[data-z-demo-dashboard-refresh]").forEach(button => button.addEventListener("click", () => {
        const dashboard = button.closest("[data-z-demo-dashboard]");
        if (!dashboard || button.disabled) return;
        const original = button.textContent;
        button.disabled = true;
        button.textContent = "Refreshing…";
        dashboard.setAttribute("aria-busy", "true");
        window.setTimeout(() => {
            const values = ["13", "79%", "03", "99.99"];
            dashboard.querySelectorAll("[data-z-demo-metric]").forEach((metric, index) => metric.textContent = values[index] ?? metric.textContent);
            dashboard.removeAttribute("aria-busy");
            button.disabled = false;
            button.textContent = original;
            notify("Workspace refreshed", "Metrics and the delivery snapshot were refreshed locally.", "info");
        }, 360);
    }));

    const bindFormDemo = form => {
        if (form.dataset.zDemoBound === "true") return;
        form.dataset.zDemoBound = "true";
        const name = form.querySelector("[data-z-demo-name]");
        let status = form.querySelector("[data-z-demo-form-status]");
        const save = form.querySelector("[data-z-demo-form-save]");
        if (!status && save) {
            status = document.createElement("p");
            status.className = "z-card-caption";
            status.dataset.zDemoFormStatus = "";
            status.setAttribute("aria-live", "polite");
            save.closest(".z-page-actions")?.after(status);
        }
        const writeStatus = value => {
            if (status) status.textContent = value;
        };
        save?.addEventListener("click", () => {
            const valid = Boolean(name?.value.trim());
            name?.classList.toggle("is-invalid", !valid);
            if (!valid) {
                writeStatus("A component name is required before the local draft can be saved.");
                name?.focus();
                notify("Draft needs a name", "Add a component name and try again.", "warning");
                return;
            }
            writeStatus(`Draft “${name.value.trim()}” saved locally. No network request was made.`);
            notify("Draft saved", "The form validated its local state and saved the demonstration draft.");
        });
        form.addEventListener("reset", () => window.setTimeout(() => {
            name?.classList.remove("is-invalid");
            writeStatus("The form was restored to its initial sample values.");
        }));
    };

    document.querySelectorAll("[data-z-demo-form]").forEach(bindFormDemo);
    document.querySelectorAll("#forms form:not([data-z-demo-form])").forEach(form => {
        const name = form.querySelector("#component-name");
        const save = form.querySelector(".z-page-actions .z-button-primary");
        if (!name || !save) return;
        name.dataset.zDemoName = "";
        save.dataset.zDemoFormSave = "";
        bindFormDemo(form);
    });

    document.querySelectorAll(".z-insight-stack .z-button:not([disabled]):not([data-z-demo-form-save])").forEach(button => {
        if (button.closest("[data-z-demo-experiment]")) return;
        button.addEventListener("click", () => notify("Button pressed", `${button.textContent.trim()} is available in the current visual treatment.`, "info"));
    });

    const bindExperimentDemo = button => {
        if (button.dataset.zDemoBound === "true") return;
        button.dataset.zDemoBound = "true";
        button.dataset.zDemoExperiment = "";
        button.addEventListener("click", () => {
        const card = button.closest(".z-empty-state");
        if (!card || card.dataset.zDemoCreated === "true") return;
        card.dataset.zDemoCreated = "true";
        card.querySelector("h2, h3").textContent = "Experiment branch created";
        card.querySelector("p").textContent = "A local branch is ready for comparison. This keeps the empty-state transition visible in the sample.";
        button.textContent = "Experiment ready";
        button.disabled = true;
        notify("Experiment created", "The empty state transitioned into a local experiment state.");
        });
    };
    document.querySelectorAll("[data-z-demo-experiment], #forms .z-empty-state .z-button").forEach(bindExperimentDemo);

    const chartPulse = root => {
        let configuration;
        try { configuration = JSON.parse(root.querySelector("[data-z-chart-config]")?.textContent ?? "{}"); } catch { return; }
        const pulse = Number(root.dataset.zDemoPulse ?? "0") + 1;
        root.dataset.zDemoPulse = String(pulse);
        const series = (configuration.series ?? []).map((entry, seriesIndex) => ({
            ...entry,
            points: (entry.points ?? []).map((point, pointIndex) => {
                const current = number(point.value);
                const direction = (pulse + seriesIndex + pointIndex) % 2 ? 1 : -1;
                const change = Math.max(1, Math.round(Math.max(Math.abs(current), 8) * .035)) * direction;
                return { ...point, value: Math.max(0, current + change) };
            })
        }));
        root.dispatchEvent(new CustomEvent("zoorce:chart:replace", { detail: { series } }));
    };

    document.querySelectorAll("[data-z-demo-chart-update]").forEach(button => button.addEventListener("click", () => {
        const scope = button.closest(".z-chart-gallery-page") ?? button.closest("[data-z-demo-chart-gallery]") ?? document;
        scope.querySelectorAll("[data-z-chart]").forEach(chartPulse);
        const status = scope.querySelector("[data-z-demo-chart-status]");
        if (status) status.textContent = "Local sample data updated just now.";
        notify("Charts updated", "Each chart replaced only its own local data series.", "info");
    }));

    let nextDemoRecord = 1;
    const addDemoRecord = target => {
        const item = {
            id: `sample-${String(nextDemoRecord).padStart(3, "0")}`,
            name: `Draft component ${nextDemoRecord}`,
            category: "Feedback",
            theme: "Dark / Light",
            status: "Preview",
            adoption: String(64 + nextDemoRecord)
        };
        nextDemoRecord += 1;
        String(target.dataset.zDemoGridInsert ?? "").split(",").map(value => value.trim()).filter(Boolean).forEach(id => {
            document.getElementById(id)?.dispatchEvent(new CustomEvent("zoorce:grid:insert", { detail: { item } }));
        });
        notify("Component added", `“${item.name}” was inserted into the local grid state.`);
    };
    document.querySelectorAll("[data-z-demo-grid-insert]").forEach(button => button.addEventListener("click", () => addDemoRecord(button)));

    document.querySelectorAll("[data-z-static-catalogue], #data-grid .z-panel").forEach(catalogue => {
        if (catalogue.dataset.zStaticBound === "true") return;
        catalogue.dataset.zStaticBound = "true";
        const body = catalogue.querySelector("tbody");
        const summary = catalogue.querySelector("[data-z-static-summary], .z-pagination > span");
        const records = [
            ["CS", "App shell", "Layout", "Stable"], ["DG", "Data grid", "Data", "Stable"], ["CP", "Command palette", "Navigation", "Preview"], ["TS", "Toast service", "Feedback", "Updated"],
            ["ST", "Workflow stepper", "Navigation", "Preview"], ["CH", "Native charts", "Data", "Stable"], ["MD", "Markdown reader", "Content", "Stable"], ["FM", "Form controls", "Feedback", "Updated"],
            ["FD", "Feedback overlays", "Feedback", "Stable"], ["NT", "Navigation tree", "Navigation", "Preview"], ["TH", "Theme tokens", "Foundation", "Stable"], ["UP", "Upload surface", "Forms", "Updated"]
        ];
        let page = 1;
        let sortDirection = "none";
        const render = () => {
            const ordered = sortDirection === "none"
                ? [...records]
                : [...records].sort((left, right) => sortDirection === "descending" ? right[1].localeCompare(left[1]) : left[1].localeCompare(right[1]));
            const totalPages = Math.ceil(ordered.length / 4);
            page = Math.min(page, totalPages);
            const slice = ordered.slice((page - 1) * 4, page * 4);
            body?.replaceChildren(...slice.map(([mark, name, category, status]) => {
                const row = document.createElement("tr");
                row.dataset.zFilterRow = "";
                row.dataset.zFilterStatus = status === "Stable" ? "stable" : status === "Preview" ? "preview" : "updated";
                const statusClass = status === "Stable" ? "success" : status === "Preview" ? "warning" : "info";
                row.innerHTML = `<td><span class="z-cell-main"><span class="z-cell-mark">${mark}</span>${name}</span></td><td>${category}</td><td>Dark / Light</td><td><span class="z-status z-status-${statusClass}">${status}</span></td><td><span class="z-row-actions"><button type="button" data-z-static-inspect="${name}" aria-label="Inspect ${name}">⌕</button><button type="button" data-z-static-edit="${name}" aria-label="Edit ${name}">⋯</button></span></td>`;
                return row;
            }));
            if (summary) summary.textContent = `Showing ${(page - 1) * 4 + 1}–${Math.min(page * 4, ordered.length)} of ${ordered.length} components`;
            catalogue.querySelectorAll("[data-z-static-page], .z-pagination-actions button").forEach(button => {
                const selected = Number(button.dataset.zStaticPage ?? button.textContent.trim()) === page;
                button.classList.toggle("is-current", selected);
                button.setAttribute("aria-current", selected ? "page" : "false");
                if (button.textContent.trim() === "←") button.disabled = page === 1;
                if (button.textContent.trim() === "→") button.disabled = page === totalPages;
            });
            catalogue.querySelector("[data-z-filter-input]")?.dispatchEvent(new Event("input", { bubbles: true }));
        };
        catalogue.addEventListener("click", event => {
            const target = event.target.closest("button");
            if (!target) return;
            if (target.matches("[data-z-static-page]")) { page = Number(target.dataset.zStaticPage); render(); return; }
            if (target.matches("[data-z-static-next]")) { page = Math.min(page + 1, Math.ceil(records.length / 4)); render(); return; }
            if (target.matches("[data-z-static-previous]")) { page = Math.max(page - 1, 1); render(); return; }
            if (target.closest("th") || target.matches("[data-z-static-sort]")) {
                sortDirection = sortDirection === "ascending" ? "descending" : "ascending";
                render();
                notify("Catalogue sorted", `Component names are now ${sortDirection}.`, "info");
                return;
            }
            if (target.closest(".z-pagination-actions")) {
                const label = target.textContent.trim();
                if (label === "←") page = Math.max(page - 1, 1);
                else if (label === "→") page = Math.min(page + 1, Math.ceil(records.length / 4));
                else if (/^\d+$/.test(label) && Number(label) <= Math.ceil(records.length / 4)) page = Number(label);
                else return;
                render();
                return;
            }
            if (target.matches("[data-z-static-inspect]")) { notify("Component inspected", `${target.dataset.zStaticInspect} is ready for review.`, "info"); return; }
            if (target.matches("[data-z-static-edit]")) { notify("Edit draft opened", `${target.dataset.zStaticEdit} was prepared for local editing.`, "info"); }
        });
        document.querySelectorAll("#data-grid .z-section-heading .z-button, [data-z-static-catalogue-insert]").forEach(button => button.addEventListener("click", () => {
            records.unshift(["NW", `New component ${records.length + 1}`, "Feedback", "Preview"]);
            page = 1;
            render();
            notify("Component added", "A local component record was added to the catalogue.");
        }));
        render();
    });
})();
