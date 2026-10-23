(() => {
  const emit = (root, name, detail = {}) => root.dispatchEvent(new CustomEvent("zoorce:" + name, { bubbles: true, detail: { id: root.id, ...detail } }));
  const focusables = root => [...root.querySelectorAll('button:not([disabled]),a[href],[tabindex]:not([tabindex="-1"])')];
  const closeMenus = except => document.querySelectorAll('[data-z-dropdown-menu]').forEach(root => {
    if (root !== except) root.querySelector('[data-z-menu-popover]')?.setAttribute('hidden', '');
    root.querySelector('[data-z-menu-trigger]')?.setAttribute('aria-expanded', 'false');
  });

  document.querySelectorAll('[data-z-dropdown-menu]').forEach(root => {
    const trigger = root.querySelector('[data-z-menu-trigger]');
    const popover = root.querySelector('[data-z-menu-popover]');
    const open = next => {
      popover.hidden = !next;
      trigger.setAttribute('aria-expanded', String(next));
      if (next) {
        closeMenus(root);
        focusables(popover)[0]?.focus();
      }
    };
    trigger.addEventListener('click', () => open(popover.hidden));
    root.addEventListener('click', event => {
      const submenuTrigger = event.target.closest('[data-z-menu-submenu-trigger]');
      if (submenuTrigger && !submenuTrigger.disabled) {
        const panel = submenuTrigger.parentElement?.querySelector('[data-z-menu-submenu-panel]');
        if (panel) {
          const next = panel.hidden;
          root.querySelectorAll('[data-z-menu-submenu-panel]').forEach(other => { if (other !== panel) other.hidden = true; });
          root.querySelectorAll('[data-z-menu-submenu-trigger]').forEach(other => { if (other !== submenuTrigger) other.setAttribute('aria-expanded', 'false'); });
          panel.hidden = !next;
          submenuTrigger.setAttribute('aria-expanded', String(next));
          if (next) focusables(panel)[0]?.focus();
        }
        return;
      }
      const item = event.target.closest('[data-z-menu-value]');
      if (!item || item.hasAttribute('disabled')) return;
      emit(root, 'menu:select', { value: item.dataset.zMenuValue, text: item.textContent.trim() });
      if (item.tagName !== 'A') open(false);
    });
    root.addEventListener('keydown', event => {
      if (event.key === 'Escape') {
        open(false);
        trigger.focus();
      }
      if (event.key === 'ArrowDown' && event.target === trigger) {
        event.preventDefault();
        open(true);
      }
    });
  });
  document.addEventListener('pointerdown', event => {
    if (!event.target.closest('[data-z-dropdown-menu]')) closeMenus();
  });

  document.querySelectorAll('[data-z-speed-dial]').forEach(root => {
    const trigger = root.querySelector('[data-z-speed-trigger]');
    const actions = root.querySelector('[data-z-speed-actions]');
    const open = next => {
      actions.hidden = !next;
      trigger.setAttribute('aria-expanded', String(next));
      root.classList.toggle('is-open', next);
      if (next) focusables(actions)[0]?.focus();
    };
    trigger.addEventListener('click', () => open(actions.hidden));
    root.addEventListener('click', event => {
      const item = event.target.closest('[data-z-speed-value]');
      if (!item) return;
      emit(root, 'speed-dial:select', { value: item.dataset.zSpeedValue });
      open(false);
    });
    root.addEventListener('keydown', event => {
      if (event.key === 'Escape') {
        open(false);
        trigger.focus();
      }
    });
  });
  document.addEventListener('pointerdown', event => {
    if (!event.target.closest('[data-z-speed-dial]')) document.querySelectorAll('[data-z-speed-dial]').forEach(root => {
      const actions = root.querySelector('[data-z-speed-actions]');
      if (!actions) return;
      actions.hidden = true;
      root.querySelector('[data-z-speed-trigger]')?.setAttribute('aria-expanded', 'false');
      root.classList.remove('is-open');
    });
  });

  document.querySelectorAll('[data-z-message-dismiss]').forEach(button => button.addEventListener('click', () => {
    const root = button.closest('[data-z-message]');
    root.hidden = true;
    emit(root, 'message:dismiss');
  }));
  document.querySelectorAll('[data-z-message-action]').forEach(button => button.addEventListener('click', () => {
    const root = button.closest('[data-z-message]');
    if (root) emit(root, 'message:action', { text: button.textContent.trim() });
  }));
  document.querySelectorAll('[data-z-fab]').forEach(button => button.addEventListener('click', () => emit(button, 'fab:activate')));
  document.querySelectorAll('[data-z-progress-button]').forEach(button => button.addEventListener('click', () => emit(button, 'progress-button:activate')));
  document.querySelectorAll('[data-z-card]').forEach(card => {
    const activate = () => emit(card, 'card:activate');
    card.addEventListener('click', activate);
    card.addEventListener('keydown', event => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); activate(); } });
  });

  document.querySelectorAll('[data-z-component-dialog]').forEach(root => {
    const dialog = root.querySelector('[data-z-dialog-focus]');
    let opener = null;
    const open = (source = document.activeElement) => {
      opener = source instanceof HTMLElement ? source : null;
      root.hidden = false;
      dialog.focus();
      emit(root, 'dialog:open');
    };
    const close = () => {
      root.hidden = true;
      emit(root, 'dialog:close');
      opener?.focus();
    };
    root.open = open;
    root.close = close;
    root.querySelectorAll('[data-z-dialog-close]').forEach(button => button.addEventListener('click', close));
    root.addEventListener('click', event => {
      if (event.target === root && root.dataset.zDialogBackdrop === 'true') close();
    });
    root.addEventListener('keydown', event => {
      if (event.key === 'Escape') close();
      if (event.key === 'Tab') {
        const items = focusables(dialog);
        if (!items.length) { event.preventDefault(); return; }
        const first = items[0];
        const last = items[items.length - 1];
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
        if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
      }
    });
  });
  document.querySelectorAll('[data-z-dialog-open]').forEach(trigger => trigger.addEventListener('click', () => {
    const selector = trigger.dataset.zDialogOpen;
    if (!selector) return;
    let dialog = null;
    try { dialog = document.querySelector(selector); } catch { return; }
    dialog?.open?.(trigger);
  }));
})();
