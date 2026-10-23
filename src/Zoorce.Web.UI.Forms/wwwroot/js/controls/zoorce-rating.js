(() => {
    "use strict";

    const forms = window.ZoorceForms;
    if (!forms) return;

    forms.register("Rating", ({ root, changed, clearInvalid }) => {
        const hidden = root.querySelector("[data-z-form-value]");
        const options = [...root.querySelectorAll("[data-z-rating-option]")];
        if (!hidden || !options.length) return null;

        const isCumulative = root.dataset.zRatingMode !== "Single";
        const allowsClear = root.dataset.zRatingAllowClear === "true";
        const selectedIndex = () => options.findIndex(option => option.dataset.zRatingValue === hidden.value);
        const render = (index, preview = -1) => {
            options.forEach((option, optionIndex) => {
                const selected = optionIndex === index;
                const paintedIndex = preview >= 0 ? preview : index;
                const filled = isCumulative ? optionIndex <= paintedIndex : optionIndex === paintedIndex;
                option.classList.toggle("is-selected", selected);
                option.classList.toggle("is-filled", preview < 0 && filled);
                option.classList.toggle("is-preview", preview >= 0 && filled);
                option.setAttribute("aria-checked", String(selected));
                option.tabIndex = selected || (index < 0 && optionIndex === 0) ? 0 : -1;
            });
        };
        const select = (index, source = "rating") => {
            const previous = selectedIndex();
            const next = allowsClear && previous === index ? -1 : index;
            hidden.value = next >= 0 ? options[next].dataset.zRatingValue : "";
            render(next);
            clearInvalid(root);
            changed(root, source, { rating: next >= 0 ? next + 1 : null, selectedValues: next >= 0 ? [hidden.value] : [] });
        };
        const onKeydown = event => {
            const current = selectedIndex();
            let target = current;
            if (event.key === "ArrowRight" || event.key === "ArrowUp") target = Math.min(options.length - 1, Math.max(0, current + 1));
            else if (event.key === "ArrowLeft" || event.key === "ArrowDown") target = Math.max(0, current - 1);
            else if (event.key === "Home") target = 0;
            else if (event.key === "End") target = options.length - 1;
            else if (event.key === "Enter" || event.key === " ") { event.preventDefault(); select(current < 0 ? 0 : current, "rating-keyboard"); return; }
            else return;
            event.preventDefault();
            select(target, "rating-keyboard");
            options[target]?.focus({ preventScroll: true });
        };

        options.forEach((option, index) => {
            option.addEventListener("click", () => select(index));
            option.addEventListener("mouseenter", () => render(selectedIndex(), index));
            option.addEventListener("keydown", onKeydown);
        });
        root.querySelector("[data-z-rating]")?.addEventListener("mouseleave", () => render(selectedIndex()));
        render(selectedIndex());
        return null;
    });
})();
