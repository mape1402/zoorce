(() => {
    "use strict";

    const instances = new WeakMap();
    const numericKinds = new Set(["NumericUpDown", "QuantityStepper", "Currency", "Percentage"]);
    const selectionKinds = new Set(["SegmentedControl", "Rating", "IconPicker", "EmojiPicker", "ChipGroup"]);

    const number = (value, fallback = 0) => {
        const parsed = Number.parseFloat(String(value ?? "").replace(/[^0-9,.-]/g, "").replace(",", "."));
        return Number.isFinite(parsed) ? parsed : fallback;
    };

    const precision = value => {
        const text = String(value ?? "");
        return text.includes(".") ? text.split(".")[1].length : 0;
    };

    const clamp = (value, minimum, maximum) => Math.min(maximum ?? Number.POSITIVE_INFINITY, Math.max(minimum ?? Number.NEGATIVE_INFINITY, value));

    const changed = (root, source = "input", extra = {}) => {
        const primary = root.querySelector("[data-z-form-value], [data-z-form-input]");
        root.dispatchEvent(new CustomEvent("zoerce:form:change", {
            bubbles: true,
            detail: {
                id: root.dataset.zFormId,
                kind: root.dataset.zFormKind,
                name: primary?.name ?? null,
                value: primary?.value ?? null,
                source,
                ...extra
            }
        }));
    };

    const invalid = (root, message) => {
        const field = root.querySelector("[data-z-form-input]");
        field?.setAttribute("aria-invalid", "true");
        root.classList.add("is-error");
        const target = root.querySelector("[data-z-form-message]");
        if (target) target.textContent = message;
        root.dispatchEvent(new CustomEvent("zoerce:form:invalid", {
            bubbles: true,
            detail: { id: root.dataset.zFormId, kind: root.dataset.zFormKind, message }
        }));
    };

    const clearInvalid = root => {
        root.classList.remove("is-error");
        root.querySelector("[data-z-form-input]")?.removeAttribute("aria-invalid");
    };

    const updateNumber = (root, direction) => {
        const input = root.querySelector("[data-z-number-input]");
        if (!input || input.disabled || input.readOnly) return;
        const minimum = root.dataset.zFormMin === "" || root.dataset.zFormMin == null ? undefined : number(root.dataset.zFormMin);
        const maximum = root.dataset.zFormMax === "" || root.dataset.zFormMax == null ? undefined : number(root.dataset.zFormMax);
        const step = number(root.dataset.zFormStep, 1);
        const current = number(input.value, minimum ?? 0);
        const next = clamp(current + (direction * step), minimum, maximum);
        input.value = next.toFixed(Math.max(precision(step), precision(input.value))).replace(/\.0+$/, "");
        input.dispatchEvent(new Event("input", { bubbles: true }));
        input.dispatchEvent(new Event("change", { bubbles: true }));
    };

    const bindNumber = root => {
        if (!numericKinds.has(root.dataset.zFormKind)) return;
        const input = root.querySelector("[data-z-number-input]");
        root.querySelector("[data-z-number-decrease]")?.addEventListener("click", () => updateNumber(root, -1));
        root.querySelector("[data-z-number-increase]")?.addEventListener("click", () => updateNumber(root, 1));
        input?.addEventListener("change", () => {
            const minimum = root.dataset.zFormMin == null ? undefined : number(root.dataset.zFormMin);
            const maximum = root.dataset.zFormMax == null ? undefined : number(root.dataset.zFormMax);
            const value = number(input.value, minimum ?? 0);
            input.value = String(clamp(value, minimum, maximum));
        });
    };

    const bindRanges = root => {
        const single = root.querySelector("[data-z-range-input]");
        if (single) {
            const output = root.querySelector("[data-z-range-output]");
            single.addEventListener("input", () => { if (output) output.textContent = single.value; changed(root, "range"); });
        }
        const start = root.querySelector("[data-z-range-start]");
        const end = root.querySelector("[data-z-range-end]");
        if (!start || !end) return;
        const output = root.querySelector("[data-z-range-output]");
        const sync = source => {
            if (number(start.value) > number(end.value)) {
                if (source === start) end.value = start.value;
                else start.value = end.value;
            }
            if (output) output.textContent = `${start.value} – ${end.value}`;
            changed(root, "range");
        };
        start.addEventListener("input", () => sync(start));
        end.addEventListener("input", () => sync(end));
    };

    const bindPassword = root => {
        const toggle = root.querySelector("[data-z-password-toggle]");
        const input = root.querySelector("input[type='password']");
        if (!toggle || !input) return;
        toggle.addEventListener("click", () => {
            const visible = input.type === "text";
            input.type = visible ? "password" : "text";
            toggle.setAttribute("aria-label", visible ? "Show password" : "Hide password");
        });
        const meter = root.querySelector("[data-z-password-meter]");
        const refresh = () => {
            const score = [input.value.length >= 8, /[A-Z]/.test(input.value), /[0-9]/.test(input.value), /[^A-Za-z0-9]/.test(input.value)].filter(Boolean).length;
            meter?.querySelectorAll("span").forEach((bar, index) => bar.classList.toggle("is-active", index < score));
        };
        input.addEventListener("input", refresh); refresh();
    };

    const bindMask = root => {
        const mask = root.dataset.zFormMask;
        const input = root.querySelector("[data-z-form-input]");
        if (!mask || !input) return;
        const accepts = (token, character) => (token === "#" ? /\d/ : token === "A" ? /[A-Za-z]/ : /[A-Za-z0-9]/).test(character);
        input.addEventListener("input", () => {
            const source = input.value.replace(/[^A-Za-z0-9]/g, "");
            let cursor = 0;
            let output = "";
            for (const token of mask) {
                if (["#", "A", "*"].includes(token)) {
                    while (cursor < source.length && !accepts(token, source[cursor])) cursor += 1;
                    if (cursor < source.length) output += source[cursor++];
                    else break;
                } else {
                    output += token;
                }
            }
            if (input.value !== output) input.value = output;
        });
    };

    const setSelectionValue = (root, value, multi = false) => {
        const hidden = root.querySelector("[data-z-form-value]");
        if (!hidden) return;
        const selected = new Set((hidden.value || "").split(",").filter(Boolean));
        if (multi) selected.has(value) ? selected.delete(value) : selected.add(value);
        else { selected.clear(); selected.add(value); }
        hidden.value = [...selected].join(",");
        root.querySelectorAll("[data-z-choice-value]").forEach(button => {
            const active = selected.has(button.dataset.zChoiceValue);
            button.classList.toggle("is-selected", active);
            button.setAttribute(button.getAttribute("role") === "radio" ? "aria-checked" : "aria-pressed", String(active));
        });
        changed(root, "choice", { selectedValues: [...selected] });
    };

    const bindSelections = root => {
        if (!selectionKinds.has(root.dataset.zFormKind)) return;
        const multi = root.dataset.zFormKind === "ChipGroup";
        root.querySelectorAll("[data-z-choice-value]").forEach(button => button.addEventListener("click", () => setSelectionValue(root, button.dataset.zChoiceValue, multi)));
    };

    const bindMultiSelect = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const summary = root.querySelector("[data-z-multi-summary]");
        const choices = [...root.querySelectorAll("[data-z-multi-choice]")];
        if (!hidden || !summary) return;
        let selected = new Set((hidden.value || "").split(",").filter(Boolean));
        const update = () => {
            hidden.value = [...selected].join(",");
            const labels = choices.filter(choice => selected.has(choice.dataset.zMultiChoice)).map(choice => choice.innerText.trim().split("\n")[0]);
            summary.textContent = labels.length ? `${labels.length} selected · ${labels.join(", ")}` : "Choose one or more";
            choices.forEach(choice => { const active = selected.has(choice.dataset.zMultiChoice); choice.classList.toggle("is-selected", active); choice.setAttribute("aria-pressed", String(active)); });
        };
        choices.forEach(choice => choice.addEventListener("click", () => { const value = choice.dataset.zMultiChoice; selected.has(value) ? selected.delete(value) : selected.add(value); update(); changed(root, "multiselect", { selectedValues: [...selected] }); }));
        update();
    };

    const bindTags = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const entry = root.querySelector("[data-z-tag-entry]");
        const list = root.querySelector("[data-z-tag-list]");
        if (!hidden || !entry || !list) return;
        let tags = (hidden.value || "").split(",").map(tag => tag.trim()).filter(Boolean);
        const render = () => {
            list.replaceChildren();
            tags.forEach(tag => {
                const chip = document.createElement("span"); chip.className = "z-chip"; chip.textContent = tag;
                const remove = document.createElement("button"); remove.type = "button"; remove.className = "z-chip-remove"; remove.textContent = "×"; remove.setAttribute("aria-label", `Remove ${tag}`);
                remove.addEventListener("click", () => { tags = tags.filter(current => current !== tag); sync(); });
                chip.append(remove); list.append(chip);
            });
        };
        const sync = () => { hidden.value = tags.join(","); render(); changed(root, "tags", { selectedValues: tags }); };
        entry.addEventListener("keydown", event => {
            if (event.key === "Enter" || event.key === ",") { event.preventDefault(); const value = entry.value.trim().replace(/,$/, ""); if (value && !tags.some(tag => tag.toLowerCase() === value.toLowerCase())) { tags.push(value); entry.value = ""; sync(); } }
            if (event.key === "Backspace" && !entry.value && tags.length) { tags.pop(); sync(); }
        });
        render();
    };

    const bindColor = root => {
        const color = root.querySelector("[data-z-color-input]");
        const text = root.querySelector("[data-z-color-text]");
        const alpha = root.querySelector("[data-z-color-alpha]");
        if (!color || !text) return;
        const initial = text.value.match(/^#[0-9a-f]{8}$/i) ? Number.parseInt(text.value.slice(7, 9), 16) : 255;
        if (alpha) alpha.value = String(Math.round((initial / 255) * 100));
        const update = value => {
            const match = String(value).match(/^#([0-9a-f]{6})([0-9a-f]{2})?$/i);
            if (!match) { invalid(root, "Use a six- or eight-digit HEX color."); return; }
            clearInvalid(root);
            const hex = `#${match[1].toUpperCase()}`;
            const opacity = alpha ? Math.round((Number(alpha.value) / 100) * 255).toString(16).padStart(2, "0").toUpperCase() : (match[2] ?? "").toUpperCase();
            color.value = hex;
            text.value = alpha && opacity !== "FF" ? `${hex}${opacity}` : hex;
            changed(root, "color", { opacity: alpha ? Number(alpha.value) : 100 });
        };
        color.addEventListener("input", () => update(color.value));
        text.addEventListener("change", () => update(text.value));
        alpha?.addEventListener("input", () => update(text.value));
        root.querySelectorAll("[data-z-color-swatch]").forEach(button => button.addEventListener("click", () => update(button.dataset.zColorSwatch)));
    };

    const bindFiles = root => {
        const input = root.querySelector("[data-z-file-input]");
        const preview = root.querySelector("[data-z-file-preview]");
        const label = root.querySelector("label");
        const isAvatar = root.classList.contains("z-file-upload--avatar");
        if (!input || !preview) return;
        const show = files => {
            preview.replaceChildren();
            label?.querySelector("[data-z-avatar-image]")?.remove();
            label?.classList.remove("has-preview");
            [...files].slice(0, isAvatar ? 1 : files.length).forEach(file => {
                if (isAvatar && file.type.startsWith("image/") && label) {
                    const image = document.createElement("img");
                    image.alt = "Selected avatar preview";
                    image.className = "z-avatar-upload-preview";
                    image.dataset.zAvatarImage = "";
                    image.src = URL.createObjectURL(file);
                    image.addEventListener("load", () => URL.revokeObjectURL(image.src));
                    label.append(image);
                    label.classList.add("has-preview");
                    return;
                }
                const row = document.createElement("span"); row.className = "z-file-name";
                if (file.type.startsWith("image/")) { const image = document.createElement("img"); image.alt = ""; image.src = URL.createObjectURL(file); image.addEventListener("load", () => URL.revokeObjectURL(image.src)); row.append(image); }
                const text = document.createElement("strong"); text.textContent = `${file.name} · ${Math.ceil(file.size / 1024)} KB`; row.append(text); preview.append(row);
            });
            root.dispatchEvent(new CustomEvent("zoerce:form:filechange", { bubbles: true, detail: { id: root.dataset.zFormId, files: [...files].map(file => ({ name: file.name, size: file.size, type: file.type })) } }));
        };
        input.addEventListener("change", () => show(input.files));
        root.addEventListener("dragover", event => { event.preventDefault(); root.classList.add("is-dragging"); });
        root.addEventListener("dragleave", () => root.classList.remove("is-dragging"));
        root.addEventListener("drop", event => { event.preventDefault(); root.classList.remove("is-dragging"); if (!input.disabled && event.dataTransfer?.files?.length) { try { input.files = event.dataTransfer.files; } catch { /* Browser forbids file assignment; preview still works. */ } show(event.dataTransfer.files); } });
    };

    const bindOtp = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const cells = [...root.querySelectorAll("[data-z-otp-cell]")];
        if (!hidden || !cells.length) return;
        const sync = () => { hidden.value = cells.map(cell => cell.value).join(""); changed(root, "otp"); };
        cells.forEach((cell, index) => {
            cell.addEventListener("input", () => { cell.value = cell.value.replace(/\D/g, "").slice(-1); if (cell.value && cells[index + 1]) cells[index + 1].focus(); sync(); });
            cell.addEventListener("keydown", event => { if (event.key === "Backspace" && !cell.value && cells[index - 1]) cells[index - 1].focus(); });
            cell.addEventListener("paste", event => { const digits = event.clipboardData?.getData("text").replace(/\D/g, "") ?? ""; if (!digits) return; event.preventDefault(); digits.slice(0, cells.length).split("").forEach((digit, offset) => { if (cells[index + offset]) cells[index + offset].value = digit; }); cells[Math.min(index + digits.length, cells.length - 1)].focus(); sync(); });
        });
    };

    const bindKeyValue = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const rows = root.querySelector("[data-z-key-value-rows]");
        const add = root.querySelector("[data-z-key-value-add]");
        if (!hidden || !rows || !add) return;
        let values;
        try { values = JSON.parse(hidden.value || "{}"); } catch { values = {}; }
        const sync = () => { const output = {}; rows.querySelectorAll("[data-z-key-value-row]").forEach(row => { const [key, value] = row.querySelectorAll("input"); if (key.value.trim()) output[key.value.trim()] = value.value; }); hidden.value = JSON.stringify(output); changed(root, "keyvalue", { value: output }); };
        const addRow = (key = "", value = "") => { const row = document.createElement("div"); row.className = "z-key-value-row"; row.dataset.zKeyValueRow = ""; row.innerHTML = '<input class="z-form-input" type="text" placeholder="Key" /><input class="z-form-input" type="text" placeholder="Value" /><button type="button" class="z-icon-button" aria-label="Remove property">×</button>'; const [keyInput, valueInput] = row.querySelectorAll("input"); keyInput.value = key; valueInput.value = value; row.querySelector("button").addEventListener("click", () => { row.remove(); sync(); }); row.querySelectorAll("input").forEach(input => input.addEventListener("input", sync)); rows.append(row); };
        Object.entries(values).forEach(([key, value]) => addRow(key, String(value))); if (!rows.children.length) addRow(); add.addEventListener("click", () => addRow()); sync();
    };

    const bindTransfer = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const source = root.querySelector("[data-z-transfer-source]");
        const target = root.querySelector("[data-z-transfer-target]");
        if (!hidden || !source || !target) return;
        const sync = () => { hidden.value = [...target.options].map(option => option.value).join(","); changed(root, "transfer", { selectedValues: [...target.options].map(option => option.value) }); };
        root.querySelector("[data-z-transfer-add]")?.addEventListener("click", () => { [...source.selectedOptions].forEach(option => target.append(option)); sync(); });
        root.querySelector("[data-z-transfer-remove]")?.addEventListener("click", () => { [...target.selectedOptions].forEach(option => source.append(option)); sync(); });
    };

    const bindInputEvents = root => {
        root.querySelectorAll("[data-z-form-input]").forEach(input => input.addEventListener("change", () => { clearInvalid(root); changed(root, "input"); }));
        root.querySelectorAll("[data-z-form-choice]").forEach(input => input.addEventListener("change", () => changed(root, "choice")));
    };

    const mount = root => {
        if (!root || instances.has(root)) return instances.get(root);
        bindInputEvents(root); bindNumber(root); bindRanges(root); bindPassword(root); bindMask(root); bindSelections(root); bindMultiSelect(root); bindTags(root); bindColor(root); bindFiles(root); bindOtp(root); bindKeyValue(root); bindTransfer(root);
        const controller = { root, dispose: () => instances.delete(root) };
        instances.set(root, controller);
        return controller;
    };

    const mountAll = (scope = document) => {
        if (scope.matches?.("[data-z-form-control]")) mount(scope);
        scope.querySelectorAll?.("[data-z-form-control]").forEach(mount);
    };

    window.ZoorceForms = Object.freeze({ mount, mountAll, dispose: root => instances.get(root)?.dispose() });
    mountAll();
})();
