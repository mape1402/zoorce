(() => {
  'use strict';
  const emit = (root, name, detail = {}) => root.dispatchEvent(new CustomEvent(`zoorce:navigation:${name}`, { bubbles: true, detail: { id: root.id, ...detail } }));
  const enabled = (items) => [...items].filter(item => !item.disabled && item.getAttribute('aria-disabled') !== 'true');
  const moveFocus = (items, current, direction) => {
    const available = enabled(items);
    if (!available.length) return;
    const index = Math.max(0, available.indexOf(current));
    available[(index + direction + available.length) % available.length].focus();
  };

  document.querySelectorAll('[data-z-accordion]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    const triggers = () => root.querySelectorAll('[data-z-accordion-trigger]');
    const setExpanded = (trigger, expanded) => {
      trigger.setAttribute('aria-expanded', String(expanded));
      const panel = document.getElementById(trigger.getAttribute('aria-controls'));
      if (panel) panel.hidden = !expanded;
      emit(root, 'accordion-change', { itemId: trigger.id, expanded });
    };
    root.addEventListener('click', event => {
      const trigger = event.target.closest('[data-z-accordion-trigger]');
      if (!trigger || trigger.disabled) return;
      const next = trigger.getAttribute('aria-expanded') !== 'true';
      if (next && root.dataset.zAccordionMultiple !== 'true') triggers().forEach(item => { if (item !== trigger) setExpanded(item, false); });
      setExpanded(trigger, next);
    });
    root.addEventListener('keydown', event => {
      const current = event.target.closest('[data-z-accordion-trigger]');
      if (!current) return;
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') { event.preventDefault(); moveFocus(triggers(), current, event.key === 'ArrowDown' ? 1 : -1); }
      if (event.key === 'Home' || event.key === 'End') { event.preventDefault(); const items = enabled(triggers()); items[event.key === 'Home' ? 0 : items.length - 1]?.focus(); }
    });
  });

  document.querySelectorAll('[data-z-tabs]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    const tabs = () => root.querySelectorAll('[data-z-tab]');
    const select = (tab, focus = false) => {
      if (!tab || tab.disabled) return;
      tabs().forEach(item => {
        const selected = item === tab;
        item.setAttribute('aria-selected', String(selected));
        item.tabIndex = selected ? 0 : -1;
        const panel = root.querySelector(`[data-z-tab-panel="${CSS.escape(item.dataset.zTab)}"]`);
        if (panel) panel.hidden = !selected;
      });
      if (focus) tab.focus();
      emit(root, 'tab-change', { value: tab.dataset.zTab });
    };
    root.addEventListener('click', event => select(event.target.closest('[data-z-tab]')));
    root.addEventListener('keydown', event => {
      const current = event.target.closest('[data-z-tab]');
      if (!current) return;
      if (event.key === 'ArrowRight' || event.key === 'ArrowDown') { event.preventDefault(); const items = enabled(tabs()); select(items[(items.indexOf(current) + 1) % items.length], true); }
      if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') { event.preventDefault(); const items = enabled(tabs()); select(items[(items.indexOf(current) - 1 + items.length) % items.length], true); }
      if (event.key === 'Home' || event.key === 'End') { event.preventDefault(); const items = enabled(tabs()); select(items[event.key === 'Home' ? 0 : items.length - 1], true); }
    });
  });

  document.querySelectorAll('[data-z-tree-view]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    const values = () => root.querySelectorAll('[data-z-tree-value]');
    const expand = button => {
      const node = button?.closest('[role=treeitem]'); const group = node?.querySelector(':scope > [role=group]');
      if (!node || !group) return;
      group.hidden = !group.hidden; node.setAttribute('aria-expanded', String(!group.hidden)); button.classList.toggle('is-expanded', !group.hidden);
      emit(root, 'tree-expand', { value: node.querySelector('[data-z-tree-value]')?.dataset.zTreeValue, expanded: !group.hidden });
    };
    const select = option => {
      if (!option || option.disabled) return;
      if (root.dataset.zTreeMultiple !== 'true') values().forEach(item => { item.classList.remove('is-selected'); item.closest('[role=treeitem]')?.setAttribute('aria-selected', 'false'); });
      const selected = root.dataset.zTreeMultiple === 'true' ? !option.classList.contains('is-selected') : true;
      option.classList.toggle('is-selected', selected); option.closest('[role=treeitem]')?.setAttribute('aria-selected', String(selected));
      emit(root, 'tree-select', { value: option.dataset.zTreeValue, selected });
    };
    root.addEventListener('click', event => { const expander = event.target.closest('[data-z-tree-expand]'); if (expander) expand(expander); else select(event.target.closest('[data-z-tree-value]')); });
    root.addEventListener('keydown', event => {
      const current = event.target.closest('[data-z-tree-value]');
      if (!current) return;
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') { event.preventDefault(); moveFocus(values(), current, event.key === 'ArrowDown' ? 1 : -1); }
      if (event.key === 'ArrowRight') { event.preventDefault(); const expander = current.parentElement?.querySelector('[data-z-tree-expand]'); if (expander && current.parentElement.querySelector('[role=group]')?.hidden) expand(expander); else current.parentElement?.querySelector('[role=group] [data-z-tree-value]')?.focus(); }
      if (event.key === 'ArrowLeft') { event.preventDefault(); const node = current.closest('[role=treeitem]'); const expander = node?.querySelector(':scope > [data-z-tree-expand]'); if (expander && !node.querySelector(':scope > [role=group]')?.hidden) expand(expander); else node?.parentElement?.closest('[role=treeitem]')?.querySelector(':scope > [data-z-tree-value]')?.focus(); }
      if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); select(current); }
    });
  });

  const closeMenuBars = (except) => document.querySelectorAll('[data-z-menu-bar]').forEach(root => { if (root === except) return; root.querySelectorAll('[data-z-menubar-trigger]').forEach(trigger => trigger.setAttribute('aria-expanded', 'false')); root.querySelectorAll('[data-z-menubar-panel]').forEach(panel => panel.hidden = true); });
  document.querySelectorAll('[data-z-menu-bar]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    const open = (trigger, next) => { const panel = trigger?.parentElement?.querySelector('[data-z-menubar-panel]'); if (!trigger || !panel) return; if (next) closeMenuBars(root); trigger.setAttribute('aria-expanded', String(next)); panel.hidden = !next; if (next) enabled(panel.querySelectorAll('[role=menuitem]'))[0]?.focus(); };
    root.addEventListener('click', event => { const trigger = event.target.closest('[data-z-menubar-trigger]'); if (trigger) open(trigger, trigger.getAttribute('aria-expanded') !== 'true'); const item = event.target.closest('[data-z-menubar-value]'); if (item && !item.disabled) { emit(root, 'menu-select', { value: item.dataset.zMenubarValue }); closeMenuBars(); } });
    root.addEventListener('keydown', event => { if (event.key === 'Escape') { const openTrigger = root.querySelector('[data-z-menubar-trigger][aria-expanded=true]'); closeMenuBars(); openTrigger?.focus(); } });
  });
  document.addEventListener('pointerdown', event => { if (!event.target.closest('[data-z-menu-bar]')) closeMenuBars(); });

  document.querySelectorAll('[data-z-context-menu]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    let targets = [];
    try { targets = [...document.querySelectorAll(root.dataset.zContextTarget)]; } catch { return; }
    const close = () => { root.hidden = true; };
    targets.forEach(target => target.addEventListener('contextmenu', event => { event.preventDefault(); root.hidden = false; root.style.left = `${Math.max(8, Math.min(event.clientX, window.innerWidth - root.offsetWidth - 8))}px`; root.style.top = `${Math.max(8, Math.min(event.clientY, window.innerHeight - root.offsetHeight - 8))}px`; enabled(root.querySelectorAll('[role=menuitem]'))[0]?.focus(); emit(root, 'context-open', { target }); }));
    root.addEventListener('click', event => { const item = event.target.closest('[data-z-context-value]'); if (!item || item.disabled) return; emit(root, 'context-select', { value: item.dataset.zContextValue }); close(); });
    root.addEventListener('keydown', event => { if (event.key === 'Escape') close(); });
    document.addEventListener('pointerdown', event => { if (!event.target.closest('[data-z-context-menu]')) close(); });
  });

  document.querySelectorAll('[data-z-toolbar]').forEach(root => {
    if (root.dataset.zNavigationBound) return;
    root.dataset.zNavigationBound = 'true';
    root.addEventListener('click', event => { const action = event.target.closest('[data-z-toolbar-value]'); if (action && !action.disabled) emit(root, 'toolbar-select', { value: action.dataset.zToolbarValue }); });
  });
})();
