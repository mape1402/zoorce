(() => {
  const dispatch = (root, type, detail) => root.dispatchEvent(new CustomEvent(type, { bubbles: true, detail }));
  const pageButton = page => {
    const item = document.createElement("button");
    item.type = "button";
    item.dataset.zPaginationPage = String(page);
    item.textContent = String(page);
    return item;
  };
  const ellipsis = () => {
    const item = document.createElement("span");
    item.setAttribute("aria-hidden", "true");
    item.textContent = "…";
    return item;
  };

  document.querySelectorAll("[data-z-breadcrumb]").forEach(root => {
    const toggle = root.querySelector("[data-z-breadcrumb-toggle]");
    const menu = root.querySelector("[data-z-breadcrumb-menu]");
    if (!toggle || !menu) return;
    const close = () => { menu.hidden = true; toggle.setAttribute("aria-expanded", "false"); };
    toggle.addEventListener("click", () => {
      const open = menu.hidden;
      menu.hidden = !open;
      toggle.setAttribute("aria-expanded", String(open));
    });
    document.addEventListener("pointerdown", event => { if (!root.contains(event.target)) close(); });
    root.addEventListener("keydown", event => { if (event.key === "Escape") { close(); toggle.focus(); } });
  });

  document.querySelectorAll("[data-z-pagination]").forEach(root => {
    const pages = root.querySelector("[data-z-pagination-pages]");
    const previous = root.querySelector(".z-pagination-control:first-child");
    const next = root.querySelector(".z-pagination-control:nth-of-type(2)");
    const jump = root.querySelector("[data-z-pagination-jump]");
    const size = root.querySelector("[data-z-pagination-page-size]");
    const total = Number(root.dataset.zTotalPages);
    const windowSize = Number(root.dataset.zWindowSize);
    const render = page => {
      const half = Math.floor(windowSize / 2);
      const first = Math.max(1, Math.min(page - half, total - windowSize + 1));
      const last = Math.min(total, first + windowSize - 1);
      pages.replaceChildren();
      const add = value => {
        const item = pageButton(value);
        if (value === page) item.setAttribute("aria-current", "page");
        pages.append(item);
      };
      if (first > 1) {
        add(1);
        if (first > 2) pages.append(ellipsis());
      }
      for (let value = first; value <= last; value++) add(value);
      if (last < total) {
        if (last < total - 1) pages.append(ellipsis());
        add(total);
      }
      previous.disabled = page === 1;
      previous.dataset.zPaginationPage = String(Math.max(1, page - 1));
      next.disabled = page === total;
      next.dataset.zPaginationPage = String(Math.min(total, page + 1));
      if (jump) jump.value = String(page);
    };
    const change = page => {
      if (!Number.isInteger(page) || page < 1 || page > total) return;
      dispatch(root, "zoorce:pagination:change", { page, pageSize: Number(size?.value || 0) || undefined });
      if (root.dataset.zControlled === "true") return;
      root.dataset.zCurrentPage = String(page);
      render(page);
    };
    root.addEventListener("click", event => {
      const target = event.target.closest("[data-z-pagination-page]");
      if (target?.disabled) return;
      if (target) change(Number(target.dataset.zPaginationPage));
    });
    jump?.addEventListener("change", () => change(Math.max(1, Math.min(total, Number(jump.value)))));
    jump?.addEventListener("keydown", event => { if (event.key === "Enter") { event.preventDefault(); jump.dispatchEvent(new Event("change")); } });
    size?.addEventListener("change", () => dispatch(root, "zoorce:pagination:change", { page: 1, pageSize: Number(size.value) }));
    render(Number(root.dataset.zCurrentPage));
  });
})();
