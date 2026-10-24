(() => {
  const iso = date => date ? [date.getFullYear(), String(date.getMonth() + 1).padStart(2, "0"), String(date.getDate()).padStart(2, "0")].join("-") : "";
  const parse = value => value ? new Date(value + "T12:00:00") : null;
  const same = (left, right) => left && right && iso(left) === iso(right);
  const clampMonth = (date, offset) => new Date(date.getFullYear(), date.getMonth() + offset, 1);

  document.querySelectorAll("[data-z-date-picker]").forEach(root => {
    const mode = root.dataset.zDatePickerMode;
    const panel = root.querySelector("[data-z-date-picker-panel]");
    const trigger = root.querySelector("[data-z-date-picker-trigger]");
    const triggers = root.querySelectorAll("[data-z-date-picker-trigger]");
    const days = root.querySelector("[data-z-date-picker-days]");
    const weekdays = root.querySelector("[data-z-date-picker-weekdays]");
    const heading = root.querySelector("[data-z-date-picker-month]");
    const locale = root.dataset.zDatePickerLocale || undefined;
    const minimum = parse(root.dataset.zDatePickerMin);
    const maximum = parse(root.dataset.zDatePickerMax);
    const unavailable = new Set((root.dataset.zDatePickerDisabled || "").split(",").filter(Boolean));
    const readOnly = root.dataset.zDatePickerReadonly === "true";
    const value = root.querySelector("[data-z-date-picker-value]");
    const start = root.querySelector("[data-z-date-picker-start]");
    const end = root.querySelector("[data-z-date-picker-end]");
    let single = parse(value?.value);
    let rangeStart = parse(start?.value);
    let rangeEnd = parse(end?.value);
    let cursor = single || rangeStart || new Date();
    cursor = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
    const firstDay = Number(root.dataset.zDatePickerFirstDay || 1);

    const dateText = date => date ? new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(date) : "";
    const update = () => {
      const display = mode === "single" ? dateText(single) : [dateText(rangeStart), dateText(rangeEnd)].filter(Boolean).join(" – ");
      trigger.value = display;
      if (value) value.value = iso(single);
      if (start) start.value = iso(rangeStart);
      if (end) end.value = iso(rangeEnd);
      root.dispatchEvent(new CustomEvent("zoorce:date-picker:change", { bubbles: true, detail: { value: iso(single), start: iso(rangeStart), end: iso(rangeEnd) } }));
    };
    const unavailableDate = date => minimum && date < minimum || maximum && date > maximum || unavailable.has(iso(date));
    const render = () => {
      heading.textContent = new Intl.DateTimeFormat(locale, { month: "long", year: "numeric" }).format(cursor);
      weekdays.innerHTML = "";
      days.innerHTML = "";
      for (let offset = 0; offset < 7; offset++) {
        const day = (firstDay + offset) % 7;
        const label = new Intl.DateTimeFormat(locale, { weekday: "short" }).format(new Date(2024, 0, 7 + day));
        const item = document.createElement("span");
        item.textContent = label;
        weekdays.append(item);
      }
      const first = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
      const leading = (first.getDay() - firstDay + 7) % 7;
      for (let index = 0; index < 42; index++) {
        const date = new Date(cursor.getFullYear(), cursor.getMonth(), index - leading + 1);
        const button = document.createElement("button");
        button.type = "button";
        button.dataset.zDatePickerDate = iso(date);
        button.textContent = String(date.getDate());
        button.setAttribute("role", "gridcell");
        button.setAttribute("aria-label", dateText(date));
        button.disabled = unavailableDate(date) || readOnly;
        if (date.getMonth() !== cursor.getMonth()) button.dataset.zDateOutside = "";
        if (same(date, single)) button.dataset.zDateSelected = "";
        if (same(date, rangeStart)) button.dataset.zDateRangeStart = "";
        if (same(date, rangeEnd)) button.dataset.zDateRangeEnd = "";
        if (rangeStart && rangeEnd && date > rangeStart && date < rangeEnd) button.dataset.zDateInRange = "";
        days.append(button);
      }
    };
    const close = restore => {
      panel.hidden = true;
      triggers.forEach(item => item.setAttribute("aria-expanded", "false"));
      if (restore) trigger.focus();
    };
    const open = () => {
      if (readOnly || trigger.disabled) return;
      panel.hidden = false;
      triggers.forEach(item => item.setAttribute("aria-expanded", "true"));
      render();
      panel.querySelector("[data-z-date-picker-date]:not(:disabled)")?.focus();
    };
    triggers.forEach(item => item.addEventListener("click", () => panel.hidden ? open() : close(true)));
    root.querySelector("[data-z-date-picker-previous]").addEventListener("click", () => { cursor = clampMonth(cursor, -1); render(); });
    root.querySelector("[data-z-date-picker-next]").addEventListener("click", () => { cursor = clampMonth(cursor, 1); render(); });
    root.querySelector("[data-z-date-picker-today]").addEventListener("click", () => { cursor = new Date(new Date().getFullYear(), new Date().getMonth(), 1); render(); });
    root.querySelector("[data-z-date-picker-clear]").addEventListener("click", () => {
      single = rangeStart = rangeEnd = null;
      update(); render();
    });
    days.addEventListener("click", event => {
      const button = event.target.closest("[data-z-date-picker-date]");
      if (!button || button.disabled) return;
      const chosen = parse(button.dataset.zDatePickerDate);
      if (mode === "single") { single = chosen; update(); close(true); return; }
      if (!rangeStart || rangeEnd) { rangeStart = chosen; rangeEnd = null; }
      else if (chosen < rangeStart) { rangeEnd = rangeStart; rangeStart = chosen; }
      else { rangeEnd = chosen; }
      update(); render();
      if (rangeEnd) close(true);
    });
    panel.addEventListener("keydown", event => {
      const focused = event.target.closest("[data-z-date-picker-date]");
      if (event.key === "Escape") { event.preventDefault(); close(true); return; }
      if (!focused) return;
      const current = parse(focused.dataset.zDatePickerDate);
      const move = { ArrowLeft: -1, ArrowRight: 1, ArrowUp: -7, ArrowDown: 7, Home: -((current.getDay() - firstDay + 7) % 7), End: 6 - ((current.getDay() - firstDay + 7) % 7) }[event.key];
      if (move !== undefined) {
        event.preventDefault();
        const next = new Date(current); next.setDate(next.getDate() + move);
        cursor = new Date(next.getFullYear(), next.getMonth(), 1); render();
        days.querySelector('[data-z-date-picker-date="' + iso(next) + '"]:not(:disabled)')?.focus();
      } else if (event.key === "PageUp" || event.key === "PageDown") {
        event.preventDefault(); cursor = clampMonth(cursor, event.key === "PageUp" ? -1 : 1); render();
      }
    });
    document.addEventListener("pointerdown", event => { if (!root.contains(event.target) && !panel.hidden) close(false); });
    render();
  });
})();
