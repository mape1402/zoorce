(() => {
  'use strict';
  const emit = (root, name, detail = {}) => root.dispatchEvent(new CustomEvent(`zoorce:${name}`, { bubbles: true, detail: { id: root.id, ...detail } }));
  const focusable = root => [...root.querySelectorAll('a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])')].filter(item => !item.hidden);

  document.querySelectorAll('[data-z-component-drawer]').forEach(root => {
    if (root.dataset.zDrawerBound) return;
    root.dataset.zDrawerBound = 'true';
    const surface = root.querySelector('[data-z-drawer-focus]');
    let opener = null;
    const open = source => { opener = source instanceof HTMLElement ? source : document.activeElement; root.hidden = false; surface?.focus(); emit(root, 'drawer:open'); };
    const close = () => { root.hidden = true; emit(root, 'drawer:close'); opener?.focus?.(); };
    root.open = open; root.close = close;
    root.querySelectorAll('[data-z-drawer-close]').forEach(button => button.addEventListener('click', close));
    root.addEventListener('click', event => { if (event.target === root && root.dataset.zDrawerBackdrop === 'true') close(); });
    root.addEventListener('keydown', event => {
      if (event.key === 'Escape') { event.preventDefault(); close(); }
      if (event.key !== 'Tab' || !surface) return;
      const items = focusable(surface); if (!items.length) { event.preventDefault(); return; }
      if (event.shiftKey && document.activeElement === items[0]) { event.preventDefault(); items.at(-1)?.focus(); }
      if (!event.shiftKey && document.activeElement === items.at(-1)) { event.preventDefault(); items[0]?.focus(); }
    });
  });
  document.querySelectorAll('[data-z-drawer-open]').forEach(trigger => trigger.addEventListener('click', event => {
    const selector = trigger.dataset.zDrawerOpen; if (!selector) return;
    let drawer; try { drawer = document.querySelector(selector); } catch { return; }
    if (drawer?.matches('[data-z-component-drawer]')) { event.stopImmediatePropagation(); drawer.open?.(trigger); }
  }, true));

  document.querySelectorAll('[data-z-notification-center]').forEach(root => {
    if (root.dataset.zNotificationBound) return;
    root.dataset.zNotificationBound = 'true';
    const trigger = root.querySelector('[data-z-notification-trigger]'); const panel = root.querySelector('[data-z-notification-panel]');
    const items = () => [...root.querySelectorAll('[data-z-notification-item]')];
    const update = () => { const unread = items().filter(item => item.dataset.zNotificationRead !== 'true').length; const badge = root.querySelector('[data-z-notification-count]'); if (badge) { badge.textContent = unread; badge.hidden = unread === 0; } const empty = root.querySelector('[data-z-notification-empty]'); const filter = root.querySelector('[data-z-notification-filter].is-active')?.dataset.zNotificationFilter ?? 'all'; empty.hidden = items().some(item => !item.hidden && (filter !== 'unread' || item.dataset.zNotificationRead !== 'true')); };
    const open = next => { panel.hidden = !next; trigger.setAttribute('aria-expanded', String(next)); if (next) root.querySelector('[data-z-notification-filter].is-active')?.focus(); };
    trigger?.addEventListener('click', () => open(panel.hidden));
    root.querySelectorAll('[data-z-notification-filter]').forEach(button => button.addEventListener('click', () => { root.querySelectorAll('[data-z-notification-filter]').forEach(candidate => { const active = candidate === button; candidate.classList.toggle('is-active', active); candidate.setAttribute('aria-pressed', String(active)); }); const unread = button.dataset.zNotificationFilter === 'unread'; items().forEach(item => item.hidden = unread && item.dataset.zNotificationRead === 'true'); update(); }));
    root.querySelector('[data-z-notification-read-all]')?.addEventListener('click', () => { items().forEach(item => { item.dataset.zNotificationRead = 'true'; item.classList.remove('is-unread'); item.classList.add('is-read'); }); update(); emit(root, 'notification:read-all'); });
    root.addEventListener('click', event => { const item = event.target.closest('[data-z-notification-item]'); if (!item) return; if (item.dataset.zNotificationRead !== 'true') { item.dataset.zNotificationRead = 'true'; item.classList.remove('is-unread'); item.classList.add('is-read'); update(); emit(root, 'notification:read', { notificationId: item.dataset.zNotificationId }); } const action = event.target.closest('[data-z-notification-action]'); if (action) emit(root, 'notification:action', { notificationId: item.dataset.zNotificationId }); });
    document.addEventListener('pointerdown', event => { if (!root.contains(event.target)) open(false); }); root.addEventListener('keydown', event => { if (event.key === 'Escape') { open(false); trigger?.focus(); } }); update();
  });
})();
