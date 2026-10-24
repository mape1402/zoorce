(() => {
  const parse = value => {
    if (!/^\d{2}:\d{2}$/.test(value || "")) return null;
    const [hours, minutes] = value.split(":").map(Number);
    return hours < 24 && minutes < 60 ? { hours, minutes } : null;
  };
  const minutesOfDay = value => value ? value.hours * 60 + value.minutes : null;
  const iso = value => value ? `${String(value.hours).padStart(2, "0")}:${String(value.minutes).padStart(2, "0")}` : "";

  document.querySelectorAll("[data-z-time-picker]").forEach(root => {
    const panel = root.querySelector("[data-z-time-picker-panel]");
    const trigger = root.querySelector("input[data-z-time-picker-trigger]");
    const triggers = root.querySelectorAll("[data-z-time-picker-trigger]");
    const value = root.querySelector("[data-z-time-picker-value]");
    const hours = root.querySelector("[data-z-time-picker-hours]");
    const minutes = root.querySelector("[data-z-time-picker-minutes]");
    const preview = root.querySelector("[data-z-time-picker-preview]");
    const locale = root.dataset.zTimePickerLocale || undefined;
    const format = root.dataset.zTimePickerFormat;
    const twelveHour = format === "twelvehour" || format === "locale" && new Intl.DateTimeFormat(locale, { hour: "numeric" }).resolvedOptions().hour12;
    const step = Number(root.dataset.zTimePickerMinuteStep || 5);
    const minimum = minutesOfDay(parse(root.dataset.zTimePickerMin));
    const maximum = minutesOfDay(parse(root.dataset.zTimePickerMax));
    const readOnly = root.dataset.zTimePickerReadonly === "true";
    let selected = parse(value.value);
    let draft = selected || { hours: new Date().getHours(), minutes: Math.floor(new Date().getMinutes() / step) * step };

    const display = time => {
      if (!time) return "";
      const date = new Date(2000, 0, 1, time.hours, time.minutes);
      return new Intl.DateTimeFormat(locale, { hour: twelveHour ? "numeric" : "2-digit", minute: "2-digit", hour12: twelveHour }).format(date);
    };
    const isAvailable = time => {
      const candidate = minutesOfDay(time);
      return (minimum === null || candidate >= minimum) && (maximum === null || candidate <= maximum);
    };
    const commit = () => {
      selected = { ...draft };
      value.value = iso(selected);
      trigger.value = display(selected);
      preview.textContent = display(selected);
      root.dispatchEvent(new CustomEvent("zoorce:time-picker:change", { bubbles: true, detail: { value: value.value } }));
    };
    const option = (label, selectedOption, available, dataset) => {
      const button = document.createElement("button");
      button.type = "button";
      button.textContent = label;
      Object.entries(dataset).forEach(([key, current]) => button.dataset[key] = String(current));
      button.classList.toggle("is-selected", selectedOption);
      button.setAttribute("aria-pressed", String(selectedOption));
      button.disabled = !available || readOnly;
      return button;
    };
    const render = () => {
      preview.textContent = display(draft);
      hours.replaceChildren();
      const values = twelveHour ? Array.from({ length: 12 }, (_, index) => index + 1) : Array.from({ length: 24 }, (_, index) => index);
      values.forEach(hour => {
        const actualHour = twelveHour ? (draft.hours >= 12 ? hour % 12 + 12 : hour % 12) : hour;
        const available = Array.from({ length: 60 / step }, (_, index) => index * step).some(minute => isAvailable({ hours: actualHour, minutes: minute }));
        hours.append(option(String(hour).padStart(2, "0"), (twelveHour ? draft.hours % 12 || 12 : draft.hours) === hour, available, { zTimePickerHour: hour }));
      });
      minutes.replaceChildren();
      for (let minute = 0; minute < 60; minute += step) {
        minutes.append(option(String(minute).padStart(2, "0"), draft.minutes === minute, isAvailable({ hours: draft.hours, minutes: minute }), { zTimePickerMinute: minute }));
      }
      root.querySelectorAll("[data-z-time-picker-period]").forEach(button => {
        const selectedPeriod = draft.hours >= 12 ? "pm" : "am";
        const active = button.dataset.zTimePickerPeriod === selectedPeriod;
        button.classList.toggle("is-selected", active);
        button.setAttribute("aria-pressed", String(active));
        button.disabled = readOnly;
      });
    };
    const close = restore => {
      panel.hidden = true;
      triggers.forEach(item => item.setAttribute("aria-expanded", "false"));
      if (restore) trigger.focus();
    };
    const open = () => {
      if (readOnly || trigger.disabled) return;
      draft = selected ? { ...selected } : { hours: new Date().getHours(), minutes: Math.floor(new Date().getMinutes() / step) * step };
      panel.hidden = false;
      triggers.forEach(item => item.setAttribute("aria-expanded", "true"));
      render();
      panel.querySelector(".is-selected:not(:disabled)")?.focus();
    };

    triggers.forEach(item => {
      item.addEventListener("click", () => panel.hidden ? open() : close(true));
      item.addEventListener("keydown", event => {
        if (["Enter", " ", "ArrowDown"].includes(event.key) && panel.hidden) { event.preventDefault(); open(); }
        if (event.key === "Escape" && !panel.hidden) { event.preventDefault(); close(true); }
      });
    });
    panel.addEventListener("click", event => {
      const hour = event.target.closest("[data-z-time-picker-hour]");
      const minute = event.target.closest("[data-z-time-picker-minute]");
      const period = event.target.closest("[data-z-time-picker-period]");
      if (hour && !hour.disabled) {
        const selectedHour = Number(hour.dataset.zTimePickerHour);
        draft.hours = twelveHour ? (draft.hours >= 12 ? selectedHour % 12 + 12 : selectedHour % 12) : selectedHour;
        if (!isAvailable(draft)) draft.minutes = Array.from({ length: 60 / step }, (_, index) => index * step).find(current => isAvailable({ hours: draft.hours, minutes: current })) ?? draft.minutes;
        render();
      } else if (minute && !minute.disabled) {
        draft.minutes = Number(minute.dataset.zTimePickerMinute);
        render();
      } else if (period && !period.disabled) {
        draft.hours = draft.hours % 12 + (period.dataset.zTimePickerPeriod === "pm" ? 12 : 0);
        if (isAvailable(draft)) render();
      } else if (event.target.closest("[data-z-time-picker-clear]")) {
        selected = null; value.value = ""; trigger.value = ""; root.dispatchEvent(new CustomEvent("zoorce:time-picker:change", { bubbles: true, detail: { value: "" } })); close(true);
      } else if (event.target.closest("[data-z-time-picker-done]")) { commit(); close(true); }
      else if (event.target.closest("[data-z-time-picker-close]")) close(true);
    });
    panel.addEventListener("keydown", event => { if (event.key === "Escape") { event.preventDefault(); close(true); } });
    document.addEventListener("pointerdown", event => { if (!root.contains(event.target) && !panel.hidden) close(false); });
    trigger.value = display(selected);
  });
})();
