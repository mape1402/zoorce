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
    const hourList = root.querySelector("[data-z-time-picker-hour-list]");
    const minuteList = root.querySelector("[data-z-time-picker-minute-list]");
    const periodButtons = root.querySelectorAll("[data-z-time-picker-period]");
    const preview = root.querySelector("[data-z-time-picker-preview]");
    const error = root.querySelector("[data-z-time-picker-error]");
    const locale = root.dataset.zTimePickerLocale || undefined;
    const format = root.dataset.zTimePickerFormat;
    const twelveHour = format === "twelvehour" || format === "locale" && new Intl.DateTimeFormat(locale, { hour: "numeric" }).resolvedOptions().hour12;
    const minimum = minutesOfDay(parse(root.dataset.zTimePickerMin));
    const maximum = minutesOfDay(parse(root.dataset.zTimePickerMax));
    const readOnly = root.dataset.zTimePickerReadonly === "true";
    let selected = parse(value.value);
    let draft = selected ? { ...selected } : null;

    const display = time => {
      if (!time) return "";
      const date = new Date(2000, 0, 1, time.hours, time.minutes);
      return new Intl.DateTimeFormat(locale, { hour: twelveHour ? "numeric" : "2-digit", minute: "2-digit", hour12: twelveHour }).format(date);
    };
    const isAvailable = time => {
      const candidate = minutesOfDay(time);
      return (minimum === null || candidate >= minimum) && (maximum === null || candidate <= maximum);
    };
    const firstAvailable = () => {
      for (let hour = 0; hour < 24; hour += 1) {
        for (let minute = 0; minute < 60; minute += 1) {
          if (isAvailable({ hours: hour, minutes: minute })) return { hours: hour, minutes: minute };
        }
      }
      return { hours: 0, minutes: 0 };
    };
    const createOption = (label, optionValue, selectedOption) => {
      const option = document.createElement("option");
      option.textContent = label;
      option.value = String(optionValue);
      option.selected = selectedOption;
      option.disabled = readOnly;
      return option;
    };
    const periodFor = hour => hour >= 12 ? "pm" : "am";
    const hourFor = (displayHour, period) => twelveHour ? displayHour % 12 + (period === "pm" ? 12 : 0) : displayHour;
    const keepPanelVisible = () => requestAnimationFrame(() => panel.scrollIntoView({ block: "nearest" }));
    const render = () => {
      if (!draft) draft = firstAvailable();
      const period = periodFor(draft.hours);
      const visibleHour = twelveHour ? draft.hours % 12 || 12 : draft.hours;
      const hourValues = twelveHour ? Array.from({ length: 12 }, (_, index) => index + 1) : Array.from({ length: 24 }, (_, index) => index);
      hourList.replaceChildren(...hourValues.map(hour => createOption(String(hour).padStart(2, "0"), hour, hour === visibleHour)));
      minuteList.replaceChildren(...Array.from({ length: 60 }, (_, minute) => createOption(String(minute).padStart(2, "0"), minute, minute === draft.minutes)));
      periodButtons.forEach(button => {
        const selectedPeriod = button.dataset.zTimePickerPeriod === period;
        button.classList.toggle("is-selected", selectedPeriod);
        button.setAttribute("aria-pressed", String(selectedPeriod));
        button.disabled = readOnly;
      });
      preview.textContent = display(draft);
    };
    const commit = () => {
      if (!isAvailable(draft)) {
        error.hidden = false;
        keepPanelVisible();
        return false;
      }
      selected = { ...draft };
      value.value = iso(selected);
      trigger.value = display(selected);
      preview.textContent = display(selected);
      root.dispatchEvent(new CustomEvent("zoorce:time-picker:change", { bubbles: true, detail: { value: value.value } }));
      return true;
    };
    const close = restore => {
      panel.hidden = true;
      triggers.forEach(item => item.setAttribute("aria-expanded", "false"));
      if (restore) trigger.focus();
    };
    const open = () => {
      if (readOnly || trigger.disabled) return;
      draft = selected ? { ...selected } : firstAvailable();
      error.hidden = true;
      panel.hidden = false;
      triggers.forEach(item => item.setAttribute("aria-expanded", "true"));
      render();
      hourList.focus();
      keepPanelVisible();
    };

    triggers.forEach(item => {
      item.addEventListener("click", () => panel.hidden ? open() : close(true));
      item.addEventListener("keydown", event => {
        if (["Enter", " ", "ArrowDown"].includes(event.key) && panel.hidden) { event.preventDefault(); open(); }
        if (event.key === "Escape" && !panel.hidden) { event.preventDefault(); close(true); }
      });
    });
    hourList.addEventListener("change", () => {
      const displayHour = Number(hourList.value);
      draft.hours = hourFor(displayHour, periodFor(draft.hours));
      error.hidden = true;
      render();
    });
    minuteList.addEventListener("change", () => {
      const minute = Number(minuteList.value);
      draft.minutes = minute;
      error.hidden = true;
      render();
    });
    periodButtons.forEach(button => {
      button.addEventListener("click", () => {
        draft.hours = hourFor(twelveHour ? draft.hours % 12 || 12 : draft.hours, button.dataset.zTimePickerPeriod);
        error.hidden = true;
        render();
      });
    });
    panel.addEventListener("click", event => {
      if (event.target.closest("[data-z-time-picker-clear]")) {
        selected = null;
        value.value = "";
        trigger.value = "";
        root.dispatchEvent(new CustomEvent("zoorce:time-picker:change", { bubbles: true, detail: { value: "" } }));
        close(true);
      } else if (event.target.closest("[data-z-time-picker-done]")) {
        if (commit()) close(true);
      } else if (event.target.closest("[data-z-time-picker-close]")) close(true);
    });
    panel.addEventListener("keydown", event => { if (event.key === "Escape") { event.preventDefault(); close(true); } });
    document.addEventListener("pointerdown", event => { if (!root.contains(event.target) && !panel.hidden) close(false); });
    trigger.value = display(selected);
  });
})();
