(() => {
  document.querySelectorAll("[data-z-split-pane]").forEach(root => {
    const divider = root.querySelector("[data-z-split-divider]");
    const collapse = root.querySelector("[data-z-split-collapse]");
    const vertical = () => root.dataset.zSplitOrientation === "vertical" || matchMedia("(max-width: 42rem)").matches;
    const minPrimary = Number(root.dataset.zSplitMinPrimary);
    const minSecondary = Number(root.dataset.zSplitMinSecondary);
    const key = root.dataset.zSplitPersistence;
    let last = Number.parseFloat(getComputedStyle(root).getPropertyValue("--z-split-primary")) || 50;
    const set = value => {
      const size = Math.max(minPrimary, Math.min(100 - minSecondary, value));
      root.style.setProperty("--z-split-primary", size + "%");
      divider.setAttribute("aria-valuenow", String(Math.round(size)));
      if (key) localStorage.setItem(key, String(size));
      root.dispatchEvent(new CustomEvent("zoorce:split-pane:resize", { bubbles: true, detail: { primaryPercent: size } }));
    };
    if (key) { const stored = Number(localStorage.getItem(key)); if (Number.isFinite(stored)) set(stored); }
    const eventPercent = event => {
      const rect = root.getBoundingClientRect();
      return vertical() ? (event.clientY - rect.top) / rect.height * 100 : (event.clientX - rect.left) / rect.width * 100;
    };
    divider.addEventListener("pointerdown", event => {
      event.preventDefault(); divider.setPointerCapture(event.pointerId);
      const move = moveEvent => set(eventPercent(moveEvent));
      const end = endEvent => { divider.releasePointerCapture?.(endEvent.pointerId); divider.removeEventListener("pointermove", move); divider.removeEventListener("pointerup", end); };
      divider.addEventListener("pointermove", move); divider.addEventListener("pointerup", end);
    });
    divider.addEventListener("keydown", event => {
      const step = event.shiftKey ? 10 : 2;
      const plus = vertical() ? "ArrowDown" : "ArrowRight";
      const minus = vertical() ? "ArrowUp" : "ArrowLeft";
      if (event.key === plus) { event.preventDefault(); set(last = Number.parseFloat(getComputedStyle(root).getPropertyValue("--z-split-primary")) + step); }
      if (event.key === minus) { event.preventDefault(); set(last = Number.parseFloat(getComputedStyle(root).getPropertyValue("--z-split-primary")) - step); }
      if (event.key === "Home") { event.preventDefault(); set(minPrimary); }
      if (event.key === "End") { event.preventDefault(); set(100 - minSecondary); }
    });
    divider.addEventListener("dblclick", () => { set(50); });
    collapse?.addEventListener("click", () => {
      const current = Number.parseFloat(getComputedStyle(root).getPropertyValue("--z-split-primary"));
      if (current <= minPrimary + .1) set(last || 50); else { last = current; set(minPrimary); }
    });
  });
})();
