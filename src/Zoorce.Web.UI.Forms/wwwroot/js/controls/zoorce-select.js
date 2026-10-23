(() => {
    "use strict";

    const forms = window.ZoorceForms;
    if (!forms) return;

    forms.register("Select", ({ root, changed, clearInvalid }) => {
        const trigger = root.querySelector("[data-z-select-trigger]");
        const menu = root.querySelector("[data-z-select-menu]");
        const value = root.querySelector("[data-z-form-value]");
        const label = root.querySelector("[data-z-select-value]");
        const options = [...root.querySelectorAll("[data-z-select-option]")];
        const clear = root.querySelector("[data-z-select-clear]");
        if (!trigger || !menu || !value || !label) return null;

        let activeIndex = -1;
        const interactiveOptions = () => [...menu.querySelectorAll("[data-z-select-option], [data-z-select-clear]")].filter(option => !option.disabled);
        const selectedOption = () => options.find(option => option.dataset.zChoiceValue === value.value) ?? null;
        const setActive = index => {
            const items = interactiveOptions();
            if (!items.length) return;
            activeIndex = Math.max(0, Math.min(items.length - 1, index));
            items.forEach((item, itemIndex) => item.classList.toggle("is-active", itemIndex === activeIndex));
            if (!items[activeIndex].id) items[activeIndex].id = `${root.dataset.zFormId}-option-${activeIndex}`;
            trigger.setAttribute("aria-activedescendant", items[activeIndex].id);
        };
        const open = next => {
            menu.hidden = !next;
            trigger.setAttribute("aria-expanded", String(next));
            if (!next) {
                menu.querySelectorAll(".is-active").forEach(item => item.classList.remove("is-active"));
                trigger.removeAttribute("aria-activedescendant");
                return;
            }
            const selected = selectedOption();
            const index = interactiveOptions().indexOf(selected);
            setActive(index >= 0 ? index : 0);
        };
        const select = option => {
            value.value = option?.dataset.zChoiceValue ?? "";
            options.forEach(candidate => {
                const selected = candidate === option;
                candidate.classList.toggle("is-selected", selected);
                candidate.setAttribute("aria-selected", String(selected));
            });
            label.textContent = option?.dataset.zChoiceLabel ?? root.dataset.zPlaceholder ?? "Select an option";
            label.classList.toggle("is-placeholder", !option);
            clearInvalid(root);
            open(false);
            changed(root, "select", { selectedValues: option ? [option.dataset.zChoiceValue] : [] });
            trigger.focus({ preventScroll: true });
        };
        const onDocumentClick = event => { if (!root.contains(event.target)) open(false); };
        const onKeydown = event => {
            const items = interactiveOptions();
            if (!items.length || trigger.disabled) return;
            if (event.key === "Escape") { open(false); return; }
            if (["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key)) {
                event.preventDefault();
                if (menu.hidden) open(true);
                const delta = event.key === "ArrowDown" ? 1 : event.key === "ArrowUp" ? -1 : 0;
                const next = event.key === "Home" ? 0 : event.key === "End" ? items.length - 1 : activeIndex + delta;
                setActive(next);
                return;
            }
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                if (menu.hidden) open(true);
                else {
                    const active = items[activeIndex];
                    if (active === clear) select(null);
                    else if (active) select(active);
                }
            }
        };

        trigger.addEventListener("click", () => open(menu.hidden));
        trigger.addEventListener("keydown", onKeydown);
        options.forEach(option => option.addEventListener("click", () => select(option)));
        clear?.addEventListener("click", () => select(null));
        document.addEventListener("click", onDocumentClick);
        return { dispose: () => document.removeEventListener("click", onDocumentClick) };
    });
})();
