(() => {
    "use strict";

    const instances = new WeakMap();
    const componentBindings = new Map();
    const numericKinds = new Set(["NumericUpDown", "QuantityStepper", "Currency", "Percentage"]);
    const selectionKinds = new Set(["SegmentedControl", "IconPicker", "EmojiPicker", "ChipGroup"]);

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
        root.dispatchEvent(new CustomEvent("zoorce:form:change", {
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
        root.querySelectorAll("[data-z-form-input]").forEach(field => field.setAttribute("aria-invalid", "true"));
        root.querySelector("[data-z-otp-input]")?.setAttribute("aria-invalid", "true");
        root.classList.add("is-error");
        const target = root.querySelector("[data-z-form-message]");
        if (target) target.textContent = message;
        root.dispatchEvent(new CustomEvent("zoorce:form:invalid", {
            bubbles: true,
            detail: { id: root.dataset.zFormId, kind: root.dataset.zFormKind, message }
        }));
    };

    const clearInvalid = root => {
        root.classList.remove("is-error");
        root.querySelectorAll("[data-z-form-input]").forEach(field => field.removeAttribute("aria-invalid"));
        root.querySelector("[data-z-otp-input]")?.removeAttribute("aria-invalid");
    };

    const bindComponentExtension = root => {
        const kind = root.dataset.zFormKind;
        const binder = componentBindings.get(kind);
        if (!binder || root.dataset.zFormExtensionBound === kind) return null;
        root.dataset.zFormExtensionBound = kind;
        return binder({ root, changed, clearInvalid, invalid });
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
        const surface = root.querySelector("[data-z-file-upload]");
        const input = root.querySelector("[data-z-file-input]");
        const preview = root.querySelector("[data-z-file-preview]");
        const feedback = root.querySelector("[data-z-file-feedback]");
        const avatarPreview = root.querySelector("[data-z-avatar-preview]");
        const avatarStatus = root.querySelector("[data-z-avatar-status]");
        const avatarEdit = root.querySelector("[data-z-avatar-edit]");
        const removeAll = root.querySelector("[data-z-file-remove-all]");
        const uploadTrigger = root.querySelector("[data-z-file-upload-trigger]");
        if (!surface || !input || !preview) return null;

        const isAvatar = surface.classList.contains("z-file-upload--avatar");
        const legacyAvatarDropzone = isAvatar && !avatarPreview ? surface.querySelector("label") : null;
        const acceptsImages = (input.accept || "").split(",").some(rule => rule.trim().toLowerCase().startsWith("image/"));
        const isImageUpload = surface.classList.contains("z-file-upload--images") || acceptsImages || isAvatar;
        const allowsMultiple = input.multiple && !isAvatar;
        const maximumFiles = Number.parseInt(surface.dataset.zFileMaxFiles || "", 10) || (isAvatar ? 1 : Number.POSITIVE_INFINITY);
        const maximumFileSize = Number.parseInt(surface.dataset.zFileMaxSize || "", 10) || Number.POSITIVE_INFINITY;
        const maximumTotalSize = Number.parseInt(surface.dataset.zFileMaxTotal || "", 10) || Number.POSITIVE_INFINITY;
        const uploadUrl = surface.dataset.zFileUploadUrl || "";
        const uploadBehavior = surface.dataset.zFileUploadBehavior || "Deferred";
        const objectUrls = new Set();
        const removedExisting = new Set();
        let serial = 0;
        let existing = [];
        let selected = [];

        try {
            const initial = JSON.parse(surface.dataset.zFileInitial || "[]");
            existing = Array.isArray(initial) ? initial.map(item => ({
                id: String(item.id ?? item.Id ?? `existing-${++serial}`),
                name: String(item.name ?? item.Name ?? "File"),
                type: String(item.contentType ?? item.ContentType ?? ""),
                size: Number(item.sizeBytes ?? item.SizeBytes ?? 0) || 0,
                url: item.url ?? item.Url ?? null,
                status: "uploaded",
                progress: 100,
                existing: true
            })) : [];
        } catch {
            existing = [];
        }

        const fileKey = file => `${file.name.toLowerCase()}-${file.size}-${file.lastModified}`;
        const itemKey = item => item.file ? fileKey(item.file) : `existing-${item.id}`;
        const formatBytes = value => {
            if (!value) return "";
            if (value < 1024) return `${value} B`;
            if (value < 1024 * 1024) return `${Math.ceil(value / 1024)} KB`;
            return `${(value / (1024 * 1024)).toFixed(value >= 10 * 1024 * 1024 ? 0 : 1)} MB`;
        };
        const isImage = item => (item.type || "").toLowerCase().startsWith("image/") || /\.(avif|gif|jpe?g|png|svg|webp)$/i.test(item.name || "");
        const accepted = file => {
            const rules = (input.accept || "").split(",").map(rule => rule.trim().toLowerCase()).filter(Boolean);
            if (!rules.length) return true;
            const type = (file.type || "").toLowerCase();
            const name = (file.name || "").toLowerCase();
            return rules.some(rule => rule.endsWith("/*") ? type.startsWith(rule.slice(0, -1)) : rule.startsWith(".") ? name.endsWith(rule) : type === rule);
        };
        const emit = (name, detail) => root.dispatchEvent(new CustomEvent(name, { bubbles: true, detail: { id: root.dataset.zFormId, ...detail } }));
        const emitChange = () => {
            const files = selected.map(item => ({ name: item.name, size: item.size, type: item.type, status: item.status, progress: item.progress }));
            emit("zoorce:form:filechange", { files, existingFiles: existing.map(item => ({ id: item.id, name: item.name, size: item.size, type: item.type })), removedFileIds: [...removedExisting] });
            changed(root, "files", { files, removedFileIds: [...removedExisting] });
        };
        const setFeedback = (messages = [], tone = "") => {
            if (!feedback) return;
            feedback.replaceChildren();
            feedback.className = `z-file-upload-feedback${tone ? ` is-${tone}` : ""}`;
            messages.filter(Boolean).forEach(message => {
                const item = document.createElement("span");
                item.textContent = message;
                feedback.append(item);
            });
        };
        const syncNativeInput = () => {
            try {
                const transfer = new DataTransfer();
                selected.forEach(item => transfer.items.add(item.file));
                input.files = transfer.files;
            } catch {
                // Browsers without DataTransfer keep their native selection; visual removal remains available.
            }
        };
        const release = item => {
            if (item?.url && objectUrls.has(item.url)) {
                URL.revokeObjectURL(item.url);
                objectUrls.delete(item.url);
            }
        };
        const createItem = file => {
            const url = isImage({ type: file.type, name: file.name }) ? URL.createObjectURL(file) : null;
            if (url) objectUrls.add(url);
            return { id: `selected-${Date.now()}-${++serial}`, name: file.name, type: file.type, size: file.size, url, file, status: "selected", progress: 0, existing: false, xhr: null };
        };
        const allItems = () => [...existing, ...selected];
        const renderAvatar = items => {
            const item = items[0];
            if (avatarPreview) {
                avatarPreview.replaceChildren();
                if (item && item.url && isImage(item)) {
                    const image = document.createElement("img");
                    image.src = item.url;
                    image.alt = item.name ? `Avatar: ${item.name}` : "Selected avatar";
                    avatarPreview.append(image);
                    avatarPreview.classList.add("has-image");
                    avatarPreview.disabled = true;
                    avatarPreview.setAttribute("aria-label", "Avatar selected. Use Edit to replace the image.");
                    avatarPreview.title = "Use Edit to replace the avatar";
                } else {
                    const icon = document.createElement("span");
                    icon.className = "z-avatar-picker-empty-icon";
                    icon.setAttribute("aria-hidden", "true");
                    icon.textContent = "↑";
                    const copy = document.createElement("span");
                    copy.className = "z-avatar-picker-empty-copy";
                    copy.textContent = "Add image";
                    avatarPreview.append(icon, copy);
                    avatarPreview.classList.remove("has-image");
                    avatarPreview.disabled = input.disabled;
                    avatarPreview.setAttribute("aria-label", "Add avatar image");
                    avatarPreview.title = "Add avatar image";
                }
                if (avatarStatus) {
                    avatarStatus.textContent = item ? `${item.name} selected.` : "No avatar selected.";
                    avatarPreview.append(avatarStatus);
                }
                if (avatarEdit) {
                    avatarEdit.toggleAttribute("hidden", !item);
                    avatarEdit.disabled = input.disabled;
                }
                return;
            }
            if (!legacyAvatarDropzone) return;
            legacyAvatarDropzone.querySelector(".z-avatar-upload-preview")?.remove();
            if (item && item.url && isImage(item)) {
                const image = document.createElement("img");
                image.className = "z-avatar-upload-preview";
                image.src = item.url;
                image.alt = item.name ? `Avatar: ${item.name}` : "Selected avatar";
                legacyAvatarDropzone.append(image);
                legacyAvatarDropzone.classList.add("has-preview");
            } else {
                legacyAvatarDropzone.classList.remove("has-preview");
            }
        };
        const createRemove = item => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "z-file-remove";
            button.setAttribute("aria-label", `Remove ${item.name}`);
            button.title = `Remove ${item.name}`;
            const icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            icon.setAttribute("viewBox", "0 0 24 24");
            icon.setAttribute("aria-hidden", "true");
            icon.setAttribute("focusable", "false");
            const firstStroke = document.createElementNS("http://www.w3.org/2000/svg", "path");
            firstStroke.setAttribute("d", "M7 7 17 17");
            const secondStroke = document.createElementNS("http://www.w3.org/2000/svg", "path");
            secondStroke.setAttribute("d", "M17 7 7 17");
            icon.append(firstStroke, secondStroke);
            button.append(icon);
            button.disabled = input.disabled;
            button.addEventListener("click", () => remove(item));
            return button;
        };
        const createProgress = item => {
            if (item.status !== "uploading" && item.status !== "error" && item.status !== "uploaded") return null;
            const wrap = document.createElement("div");
            wrap.className = "z-file-progress";
            const bar = document.createElement("span");
            bar.style.setProperty("--z-file-progress", `${Math.min(100, Math.max(0, item.progress || 0))}%`);
            const label = document.createElement("small");
            label.textContent = item.status === "error" ? "Upload failed" : item.status === "uploaded" ? "Uploaded" : `${item.progress || 0}% uploaded`;
            wrap.append(bar, label);
            return wrap;
        };
        const createCard = item => {
            const card = document.createElement("article");
            card.className = `z-file-item is-${item.status}${item.existing ? " is-existing" : ""}`;
            card.dataset.zFileItem = item.id;
            if (isImage(item) && item.url) {
                const image = document.createElement("img");
                image.className = "z-file-thumbnail";
                image.src = item.url;
                image.alt = "";
                card.append(image);
            } else {
                const icon = document.createElement("span");
                icon.className = "z-file-kind";
                icon.setAttribute("aria-hidden", "true");
                icon.textContent = isImage(item) ? "▧" : "↗";
                card.append(icon);
            }
            const copy = document.createElement("div");
            copy.className = "z-file-copy";
            const name = document.createElement("strong");
            name.title = item.name;
            name.textContent = item.name;
            const meta = document.createElement("small");
            meta.textContent = [formatBytes(item.size), item.existing ? "Saved" : item.status === "selected" ? "Ready" : null].filter(Boolean).join(" · ");
            copy.append(name, meta);
            const progress = createProgress(item);
            if (progress) copy.append(progress);
            card.append(copy, createRemove(item));
            return card;
        };
        const renderCarousel = items => {
            const carousel = document.createElement("div");
            carousel.className = "z-file-carousel";
            const track = document.createElement("div");
            track.className = "z-file-carousel-track";
            track.tabIndex = 0;
            track.setAttribute("role", "list");
            track.setAttribute("aria-label", "Selected images");
            items.forEach(item => {
                const slide = document.createElement("article");
                slide.className = `z-file-image-slide is-${item.status}${item.existing ? " is-existing" : ""}`;
                slide.setAttribute("role", "listitem");
                slide.setAttribute("aria-label", item.name);
                if (item.url) {
                    const image = document.createElement("img");
                    image.src = item.url;
                    image.alt = item.name;
                    slide.append(image);
                }
                const name = document.createElement("span");
                name.className = "z-file-image-name";
                name.title = item.name;
                name.textContent = item.name;
                slide.append(name, createRemove(item));
                track.append(slide);
            });
            carousel.append(track);
            return carousel;
        };
        const render = () => {
            const items = allItems();
            preview.replaceChildren();
            surface.classList.toggle("has-files", items.length > 0);
            removeAll?.toggleAttribute("hidden", items.length === 0);
            if (uploadTrigger) {
                const canUpload = selected.some(item => item.status === "selected" || item.status === "error");
                uploadTrigger.hidden = !canUpload;
                uploadTrigger.disabled = input.disabled || !canUpload;
            }
            if (isAvatar) {
                renderAvatar(items);
                return;
            }
            if (!items.length) return;
            if (isImageUpload && (allowsMultiple || items.length > 1)) {
                preview.append(renderCarousel(items));
                return;
            }
            const list = document.createElement("div");
            list.className = "z-file-list";
            items.forEach(item => list.append(createCard(item)));
            preview.append(list);
        };
        const remove = item => {
            if (item.xhr) item.xhr.abort();
            if (item.existing) {
                existing = existing.filter(current => current !== item);
                removedExisting.add(item.id);
            } else {
                selected = selected.filter(current => current !== item);
                release(item);
            }
            syncNativeInput();
            setFeedback();
            render();
            emitChange();
        };
        const upload = item => new Promise(resolve => {
            if (!uploadUrl || !item?.file || item.status === "uploading" || item.status === "uploaded") { resolve(); return; }
            item.status = "uploading";
            item.progress = 0;
            render();
            const request = new XMLHttpRequest();
            item.xhr = request;
            request.open("POST", uploadUrl, true);
            request.responseType = "json";
            request.upload.addEventListener("progress", event => {
                if (!event.lengthComputable) return;
                item.progress = Math.round((event.loaded / event.total) * 100);
                render();
                emit("zoorce:form:uploadprogress", { file: { name: item.name, size: item.size, type: item.type }, progress: item.progress });
            });
            request.addEventListener("load", () => {
                item.xhr = null;
                if (request.status >= 200 && request.status < 300) {
                    item.status = "uploaded";
                    item.progress = 100;
                    setFeedback([`${item.name} uploaded.`], "success");
                    emit("zoorce:form:uploadcomplete", { file: { name: item.name, size: item.size, type: item.type }, response: request.response });
                } else {
                    item.status = "error";
                    setFeedback([`${item.name} could not be uploaded.`], "error");
                    emit("zoorce:form:uploaderror", { file: { name: item.name, size: item.size, type: item.type }, status: request.status });
                }
                render();
                resolve();
            });
            request.addEventListener("error", () => {
                item.xhr = null;
                item.status = "error";
                setFeedback([`${item.name} could not be uploaded.`], "error");
                emit("zoorce:form:uploaderror", { file: { name: item.name, size: item.size, type: item.type }, status: 0 });
                render();
                resolve();
            });
            request.addEventListener("abort", () => { item.xhr = null; resolve(); });
            const payload = new FormData();
            payload.append(input.name || root.dataset.zFormId || "file", item.file, item.name);
            request.send(payload);
        });
        const uploadSelected = items => Promise.all(items.filter(item => item.status === "selected" || item.status === "error").map(upload));
        const addFiles = incoming => {
            if (input.disabled) return;
            const files = [...(incoming || [])];
            if (!files.length) return;
            const messages = [];
            const added = [];
            const used = new Set(selected.map(itemKey));
            let totalSize = allowsMultiple ? allItems().reduce((total, item) => total + (item.size || 0), 0) : 0;
            files.forEach(file => {
                if (!accepted(file)) { messages.push(`${file.name} does not match the accepted format.`); return; }
                if (file.size > maximumFileSize) { messages.push(`${file.name} exceeds the file-size limit.`); return; }
                if (used.has(fileKey(file))) { messages.push(`${file.name} is already selected.`); return; }
                if (!allowsMultiple && added.length) { messages.push("This control accepts one file only."); return; }
                if (allowsMultiple && allItems().length + added.length >= maximumFiles) { messages.push(`You can select up to ${maximumFiles} file${maximumFiles === 1 ? "" : "s"}.`); return; }
                if (totalSize + file.size > maximumTotalSize) { messages.push("The selected files exceed the total-size limit."); return; }
                const item = createItem(file);
                used.add(fileKey(file));
                totalSize += file.size;
                added.push(item);
            });
            if (!allowsMultiple && added.length) {
                selected.forEach(release);
                existing.forEach(item => removedExisting.add(item.id));
                existing = [];
                selected = [added[0]];
            } else {
                selected.push(...added);
            }
            syncNativeInput();
            setFeedback(messages, messages.length ? "error" : "");
            render();
            emitChange();
            if (uploadBehavior === "Automatic" && uploadUrl && added.length) void uploadSelected(added);
        };

        input.addEventListener("change", () => addFiles(input.files));
        root.querySelectorAll("[data-z-file-picker]").forEach(trigger => {
            if (trigger.tagName === "LABEL") return;
            trigger.addEventListener("click", () => {
                if (!input.disabled) input.click();
            });
        });
        surface.addEventListener("dragover", event => { if (!input.disabled) { event.preventDefault(); surface.classList.add("is-dragging"); } });
        surface.addEventListener("dragleave", event => { if (!surface.contains(event.relatedTarget)) surface.classList.remove("is-dragging"); });
        surface.addEventListener("drop", event => { event.preventDefault(); surface.classList.remove("is-dragging"); addFiles(event.dataTransfer?.files); });
        removeAll?.addEventListener("click", () => [...allItems()].forEach(remove));
        uploadTrigger?.addEventListener("click", () => void uploadSelected(selected));
        render();
        return { dispose: () => { selected.forEach(item => { item.xhr?.abort(); release(item); }); } };
    };

    const bindOtp = root => {
        const hidden = root.querySelector("[data-z-form-value]");
        const cells = [...root.querySelectorAll("[data-z-otp-cell]")];
        const status = root.querySelector("[data-z-otp-status]");
        if (!hidden || !cells.length) return;

        const characterSet = root.dataset.zOtpCharacterSet || "Numeric";
        const textTransform = root.dataset.zOtpTextTransform || "Uppercase";
        const accepts = character => characterSet === "Numeric" ? /[0-9]/.test(character) : characterSet === "Alphabetic" ? /[A-Za-z]/.test(character) : /[A-Za-z0-9]/.test(character);
        const normalize = value => [...String(value ?? "")]
            .filter(accepts)
            .map(character => textTransform === "Uppercase" ? character.toUpperCase() : textTransform === "Lowercase" ? character.toLowerCase() : character)
            .join("");
        const isEditable = cell => !cell.disabled && !cell.readOnly;
        let wasComplete = cells.every(cell => cell.value.length === 1);

        const sync = (source = "otp", notify = true) => {
            const value = cells.map(cell => cell.value).join("");
            const complete = cells.every(cell => cell.value.length === 1);
            hidden.value = value;
            root.classList.toggle("is-complete", complete);
            root.dataset.zOtpComplete = String(complete);
            if (status) status.textContent = complete ? "Code complete." : `${value.length} of ${cells.length} characters entered.`;
            if (notify) {
                clearInvalid(root);
                changed(root, source, { complete });
            }
            if (notify && complete && !wasComplete) {
                root.dispatchEvent(new CustomEvent("zoorce:form:complete", {
                    bubbles: true,
                    detail: { id: root.dataset.zFormId, kind: root.dataset.zFormKind, name: hidden.name, value, source }
                }));
            }
            wasComplete = complete;
        };

        const focus = index => {
            const target = cells[Math.max(0, Math.min(index, cells.length - 1))];
            if (target && !target.disabled) target.focus();
        };

        const write = (startIndex, rawValue, source = "otp") => {
            const characters = normalize(rawValue);
            const current = cells[startIndex];
            if (!current || !isEditable(current)) return;

            if (!characters) {
                current.value = "";
                sync(source);
                return;
            }

            let written = 0;
            for (const character of characters) {
                const target = cells[startIndex + written];
                if (!target || !isEditable(target)) break;
                target.value = character;
                written += 1;
            }
            focus(startIndex + written);
            sync(source);
        };

        cells.forEach((cell, index) => {
            cell.value = normalize(cell.value).slice(0, 1);
            cell.addEventListener("focus", () => cell.select());
            cell.addEventListener("input", () => write(index, cell.value));
            cell.addEventListener("paste", event => {
                event.preventDefault();
                write(index, event.clipboardData?.getData("text") ?? "", "otp-paste");
            });
            cell.addEventListener("keydown", event => {
                if (!isEditable(cell)) return;
                if (event.key === "Backspace") {
                    event.preventDefault();
                    if (cell.value) {
                        cell.value = "";
                    } else if (cells[index - 1] && isEditable(cells[index - 1])) {
                        cells[index - 1].value = "";
                        focus(index - 1);
                    }
                    sync("otp-keyboard");
                    return;
                }
                if (event.key === "Delete") {
                    event.preventDefault();
                    cell.value = "";
                    sync("otp-keyboard");
                    return;
                }
                if (event.key === "ArrowLeft") { event.preventDefault(); focus(index - 1); return; }
                if (event.key === "ArrowRight") { event.preventDefault(); focus(index + 1); return; }
                if (event.key === "Home") { event.preventDefault(); focus(0); return; }
                if (event.key === "End") { event.preventDefault(); focus(cells.length - 1); return; }
            });
        });

        sync("otp", false);
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
        root.querySelectorAll("[data-z-form-input]:not([data-z-otp-cell])").forEach(input => input.addEventListener("change", () => { clearInvalid(root); changed(root, "input"); }));
        root.querySelectorAll("[data-z-form-choice]").forEach(input => input.addEventListener("change", () => changed(root, "choice")));
    };

    const mount = root => {
        if (!root || instances.has(root)) return instances.get(root);
        bindInputEvents(root); bindNumber(root); bindRanges(root); bindPassword(root); bindMask(root); bindSelections(root); bindMultiSelect(root); bindTags(root); bindColor(root); const fileController = bindFiles(root); bindOtp(root); bindKeyValue(root); bindTransfer(root);
        const extensions = [];
        const controller = { root, extensions, dispose: () => { fileController?.dispose(); extensions.forEach(extension => extension?.dispose?.()); instances.delete(root); } };
        instances.set(root, controller);
        const extension = bindComponentExtension(root);
        if (extension) extensions.push(extension);
        return controller;
    };

    const mountAll = (scope = document) => {
        if (scope.matches?.("[data-z-form-control]")) mount(scope);
        scope.querySelectorAll?.("[data-z-form-control]").forEach(mount);
    };

    const register = (kind, binder) => {
        if (!kind || typeof binder !== "function") return;
        componentBindings.set(kind, binder);
        document.querySelectorAll(`[data-z-form-control][data-z-form-kind="${CSS.escape(kind)}"]`).forEach(root => {
            const controller = mount(root);
            const extension = bindComponentExtension(root);
            if (extension) controller.extensions?.push(extension);
        });
    };

    window.ZoorceForms = Object.freeze({ mount, mountAll, register, dispose: root => instances.get(root)?.dispose() });
    document.addEventListener("click", event => {
        const remove = event.target.closest("[data-z-chip-remove]");
        if (!remove || remove.disabled) return;
        const chip = remove.closest("[data-z-chip]");
        if (!chip) return;
        chip.dispatchEvent(new CustomEvent("zoorce:chip:remove", {
            bubbles: true,
            detail: { text: chip.querySelector("span:not(.z-chip-icon)")?.textContent?.trim() ?? "" }
        }));
        chip.remove();
    });
    mountAll();
})();
