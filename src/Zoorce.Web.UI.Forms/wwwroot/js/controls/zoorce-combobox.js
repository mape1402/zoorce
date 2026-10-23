(() => {
    "use strict";

    const forms = window.ZoorceForms;
    if (!forms) return;

    forms.register("Combobox", ({ root, changed, clearInvalid, invalid }) => {
        const input = root.querySelector("[data-z-combobox-input]");
        const toggle = root.querySelector("[data-z-combobox-toggle]");
        const menu = root.querySelector("[data-z-combobox-menu]");
        const hidden = root.querySelector("[data-z-form-value]");
        const empty = root.querySelector("[data-z-combobox-empty]");
        const options = [...root.querySelectorAll("[data-z-combobox-option]")];
        if (!input || !toggle || !menu || !hidden) return null;

        const allowsCustom = root.dataset.zComboboxAllowCustom === "true";
        const openOnFocus = root.dataset.zComboboxOpenOnFocus !== "false";
        let activeIndex = -1;
        const visibleOptions = () => options.filter(option => !option.hidden && !option.disabled);
        const selectedOption = () => options.find(option => option.dataset.zChoiceValue === hidden.value) ?? null;
        const setActive = index => {
            const visible = visibleOptions();
            if (!visible.length) return;
            activeIndex = Math.max(0, Math.min(visible.length - 1, index));
            visible.forEach((option, optionIndex) => option.classList.toggle("is-active", optionIndex === activeIndex));
            if (!visible[activeIndex].id) visible[activeIndex].id = `${root.dataset.zFormId}-option-${activeIndex}`;
            input.setAttribute("aria-activedescendant", visible[activeIndex].id);
        };
        const filter = (queryValue = input.value) => {
            const query = queryValue.trim().toLocaleLowerCase();
            options.forEach(option => {
                const matches = !query || `${option.dataset.zChoiceLabel ?? ""} ${option.textContent ?? ""}`.toLocaleLowerCase().includes(query);
                option.hidden = !matches;
                option.classList.remove("is-active");
            });
            const visible = visibleOptions();
            if (empty) empty.hidden = visible.length > 0;
            activeIndex = visible.length ? 0 : -1;
            if (visible.length) setActive(0);
        };
        const open = (next, showAll = false) => {
            menu.hidden = !next;
            input.setAttribute("aria-expanded", String(next));
            toggle.setAttribute("aria-expanded", String(next));
            if (!next) {
                options.forEach(option => option.classList.remove("is-active"));
                input.removeAttribute("aria-activedescendant");
                return;
            }
            filter(showAll ? "" : input.value);
            const selected = selectedOption();
            const index = visibleOptions().indexOf(selected);
            if (index >= 0) setActive(index);
        };
        const choose = option => {
            const nextValue = option?.dataset.zChoiceValue ?? input.value.trim();
            const nextLabel = option?.dataset.zChoiceLabel ?? nextValue;
            if (!nextValue) return;
            hidden.value = nextValue;
            input.value = nextLabel;
            options.forEach(candidate => {
                const selected = candidate === option;
                candidate.classList.toggle("is-selected", selected);
                candidate.setAttribute("aria-selected", String(selected));
            });
            clearInvalid(root);
            open(false);
            changed(root, "combobox", { selectedValues: option ? [nextValue] : [], isCustomValue: !option });
        };
        const restoreSelection = () => {
            const selected = selectedOption();
            input.value = selected?.dataset.zChoiceLabel ?? "";
        };
        const onDocumentClick = event => { if (!root.contains(event.target)) { open(false); if (!allowsCustom) restoreSelection(); } };
        const onKeydown = event => {
            const visible = visibleOptions();
            if (event.key === "Escape") { open(false); restoreSelection(); return; }
            if (event.key === "ArrowDown" || event.key === "ArrowUp") {
                event.preventDefault();
                if (menu.hidden) open(true);
                const current = activeIndex < 0 ? 0 : activeIndex;
                setActive(current + (event.key === "ArrowDown" ? 1 : -1));
                return;
            }
            if (event.key === "Home" && !menu.hidden) { event.preventDefault(); setActive(0); return; }
            if (event.key === "End" && !menu.hidden) { event.preventDefault(); setActive(Math.max(0, visible.length - 1)); return; }
            if (event.key === "Enter") {
                if (menu.hidden) return;
                event.preventDefault();
                const option = visibleOptions()[activeIndex];
                if (option) choose(option);
                else if (allowsCustom && input.value.trim()) choose(null);
                else invalid(root, "Choose one of the available options.");
            }
        };

        input.addEventListener("focus", () => { if (openOnFocus && !input.readOnly) open(true, true); });
        input.addEventListener("input", () => { clearInvalid(root); open(true); filter(); });
        input.addEventListener("keydown", onKeydown);
        toggle.addEventListener("click", () => { if (menu.hidden) { input.focus({ preventScroll: true }); open(true, true); } else open(false); });
        options.forEach(option => option.addEventListener("click", () => choose(option)));
        document.addEventListener("click", onDocumentClick);
        return { dispose: () => document.removeEventListener("click", onDocumentClick) };
    });
})();
