(() => {
  const remoteRequests = new WeakMap();
  const remoteTimers = new WeakMap();
  const fire = (root, kind, value, label) => {
    const field = root.querySelector('[data-z-choice-value], [data-z-mention-value]');
    const detail = { id: root.dataset.zFormId || field?.id || root.id, kind: root.dataset.zFormKind || kind, name: field?.name || null, value, label, source: kind };
    root.dispatchEvent(new CustomEvent("zoorce:" + kind + ":change", { bubbles: true, detail }));
    root.dispatchEvent(new CustomEvent("zoorce:form:change", { bubbles: true, detail }));
    field?.dispatchEvent(new Event('change', { bubbles: true }));
  };
  const contains = (value, query) => !query || value.toLocaleLowerCase().includes(query.toLocaleLowerCase());
  const toItems = body => Array.isArray(body) ? body : body?.items || body?.results || body?.data || [];
  const addQuery = (source, query) => {
    const url = new URL(source, window.location.href);
    url.searchParams.set('q', query);
    return url.toString();
  };
  const appendRemoteOptions = (root, selector, items, multiColumn = false) => {
    const container = root.querySelector(selector);
    if (!container) return;
    container.querySelectorAll('[data-z-remote-option]').forEach(option => option.remove());
    items.forEach(item => {
      const scalar = typeof item === 'string' || typeof item === 'number' ? String(item) : null;
      const value = String(scalar ?? item?.value ?? item?.id ?? item?.key ?? '').trim();
      const label = String(scalar ?? item?.label ?? item?.text ?? item?.name ?? value).trim();
      if (!value || !label) return;
      const option = document.createElement('button');
      option.type = 'button';
      option.role = 'option';
      option.disabled = Boolean(item?.disabled);
      option.dataset.zChoiceOption = '';
      option.dataset.zRemoteOption = '';
      option.dataset.zChoiceValue = value;
      option.dataset.zChoiceLabel = label;
      if (selector.includes('mention')) option.dataset.zMentionOption = '';
      option.className = multiColumn ? 'z-multi-column-row' : 'z-choice-option';
      if (multiColumn) {
        const values = item?.values && typeof item.values === 'object' ? Object.values(item.values) : [label];
        values.forEach(valuePart => { const cell = document.createElement('span'); cell.textContent = String(valuePart ?? ''); option.append(cell); });
      } else {
        const copy = document.createElement('span'); copy.className = 'z-choice-option-copy';
        const strong = document.createElement('strong'); strong.textContent = label; copy.append(strong);
        if (item?.description) { const description = document.createElement('small'); description.textContent = String(item.description); copy.append(description); }
        option.append(copy);
      }
      container.append(option);
    });
  };
  const requestRemote = async (root, query, selector, multiColumn = false) => {
    const source = root.dataset.zSource;
    if (!source) return;
    remoteRequests.get(root)?.abort();
    const controller = new AbortController();
    remoteRequests.set(root, controller);
    try {
      const response = await fetch(addQuery(source, query), { headers: { Accept: 'application/json' }, signal: controller.signal });
      if (!response.ok) throw new Error('Suggestion request failed.');
      appendRemoteOptions(root, selector, toItems(await response.json()), multiColumn);
      filter(root, query);
    } catch (error) {
      if (error?.name !== 'AbortError') root.dispatchEvent(new CustomEvent('zoorce:form:lookup-error', { bubbles: true, detail: { id: root.dataset.zFormId || root.id, source } }));
    }
  };
  const queueRemote = (root, query, selector, multiColumn = false) => {
    if (!root.dataset.zSource) return;
    window.clearTimeout(remoteTimers.get(root));
    const delay = Number.parseInt(root.dataset.zDebounce || '220', 10);
    remoteTimers.set(root, window.setTimeout(() => requestRemote(root, query, selector, multiColumn), Number.isFinite(delay) ? Math.max(0, delay) : 220));
  };
  const setExpanded = (root, expanded) => root.querySelectorAll('[data-z-choice-toggle], [data-z-choice-input][aria-expanded]').forEach(control => control.setAttribute('aria-expanded', String(expanded)));
  const closeOthers = root => document.querySelectorAll('[data-z-autocomplete],[data-z-multi-dropdown],[data-z-dropdown-tree],[data-z-multi-column-combobox]').forEach(other => {
    if (other === root) return;
    other.querySelector('[data-z-choice-menu]')?.setAttribute('hidden', '');
    setExpanded(other, false);
  });
  const open = (root, next) => {
    const menu = root.querySelector('[data-z-choice-menu]');
    const toggle = root.querySelector('[data-z-choice-toggle]');
    if (!menu) return;
    menu.hidden = !next;
    setExpanded(root, next);
    if (next) closeOthers(root);
  };
  const filter = (root, query) => {
    const options = [...root.querySelectorAll('[data-z-choice-option]')];
    let count = 0;
    options.forEach(option => {
      const match = contains(option.textContent || '', query);
      option.hidden = !match;
      if (match) count += 1;
    });
    const empty = root.querySelector('[data-z-choice-empty]');
    if (empty) empty.hidden = count > 0;
  };
  const selectedValues = root => (root.querySelector('[data-z-choice-value]')?.value || '').split(',').filter(Boolean);
  const setValue = (root, values, labels, kind) => {
    const value = root.querySelector('[data-z-choice-value]');
    value.value = values.join(',');
    root.querySelectorAll('[data-z-choice-option]').forEach(option => {
      const selected = values.includes(option.dataset.zChoiceValue);
      option.classList.toggle('is-selected', selected);
      option.setAttribute('aria-selected', String(selected));
    });
    const summary = root.querySelector('[data-z-multi-summary], [data-z-tree-summary]');
    if (summary) summary.textContent = labels.length ? labels.join(', ') : summary.dataset.zPlaceholder || 'Choose options';
    fire(root, kind, value.value, labels.join(', '));
  };

  const setupChoicePopup = (root, kind, multiple) => {
    const input = root.querySelector('[data-z-choice-input]');
    const toggle = root.querySelector('[data-z-choice-toggle]');
    const value = root.querySelector('[data-z-choice-value]');
    const select = option => {
      const optionValue = option.dataset.zChoiceValue;
      const optionLabel = option.dataset.zChoiceLabel || option.textContent.trim();
      if (multiple) {
        const values = selectedValues(root);
        const index = values.indexOf(optionValue);
        if (index >= 0) values.splice(index, 1); else values.push(optionValue);
        const labels = values.map(current => root.querySelector('[data-z-choice-option][data-z-choice-value="' + CSS.escape(current) + '"]')?.dataset.zChoiceLabel || current);
        setValue(root, values, labels, kind);
      } else {
        value.value = optionValue;
        if (input) input.value = optionLabel;
        fire(root, kind, optionValue, optionLabel);
        open(root, false);
      }
    };
    toggle?.addEventListener('click', () => {
      const next = root.querySelector('[data-z-choice-menu]').hidden;
      if (next && input) filter(root, '');
      open(root, next);
      if (next && input) input.focus();
    });
    input?.addEventListener('focus', () => {
      filter(root, input.value);
      open(root, true);
    });
    input?.addEventListener('input', () => {
      filter(root, input.value);
      open(root, true);
      const minimum = Number.parseInt(root.dataset.zMinimum || '0', 10);
      if (!root.dataset.zSource || input.value.length < (Number.isFinite(minimum) ? minimum : 0)) return;
      queueRemote(root, input.value, '[data-z-choice-menu]', root.matches('[data-z-multi-column-combobox]'));
    });
    root.addEventListener('click', event => {
      const option = event.target.closest('[data-z-choice-option]');
      if (option && !option.disabled) select(option);
    });
    root.addEventListener('keydown', event => {
      const menu = root.querySelector('[data-z-choice-menu]');
      const visible = [...root.querySelectorAll('[data-z-choice-option]:not([hidden]):not([disabled])')];
      if (event.key === 'Escape') {
        open(root, false);
        toggle?.focus();
      }
      if (event.key === 'ArrowDown' && visible.length) {
        event.preventDefault();
        open(root, true);
        visible[0].focus();
      }
      if ((event.key === 'Enter' || event.key === ' ') && event.target.matches('[data-z-choice-option]')) {
        event.preventDefault();
        select(event.target);
      }
      if (menu && event.key === 'ArrowDown' && event.target.matches('[data-z-choice-option]')) {
        event.preventDefault();
        const index = visible.indexOf(event.target);
        visible[(index + 1) % visible.length]?.focus();
      }
      if (menu && event.key === 'ArrowUp' && event.target.matches('[data-z-choice-option]')) {
        event.preventDefault();
        const index = visible.indexOf(event.target);
        visible[(index - 1 + visible.length) % visible.length]?.focus();
      }
    });
  };

  document.querySelectorAll('[data-z-autocomplete]').forEach(root => setupChoicePopup(root, 'autocomplete', false));
  document.querySelectorAll('[data-z-multi-column-combobox]').forEach(root => setupChoicePopup(root, 'multi-column-combobox', false));
  document.querySelectorAll('[data-z-multi-dropdown]').forEach(root => {
    const summary = root.querySelector('[data-z-multi-summary]');
    summary.dataset.zPlaceholder ||= summary.textContent;
    setupChoicePopup(root, 'multi-select-dropdown', true);
    root.querySelector('[data-z-multi-all]')?.addEventListener('click', () => {
      const options = [...root.querySelectorAll('[data-z-choice-option]:not([disabled])')];
      setValue(root, options.map(option => option.dataset.zChoiceValue), options.map(option => option.dataset.zChoiceLabel || option.textContent.trim()), 'multi-select-dropdown');
    });
  });

  document.querySelectorAll('[data-z-listbox]').forEach(root => {
    const multiple = root.dataset.zMultiple === 'true';
    const select = option => {
      if (!option || option.disabled) return;
      let values = selectedValues(root);
      if (multiple) {
        const index = values.indexOf(option.dataset.zChoiceValue);
        if (index >= 0) values.splice(index, 1); else values.push(option.dataset.zChoiceValue);
      } else values = [option.dataset.zChoiceValue];
      const labels = values.map(value => root.querySelector('[data-z-choice-option][data-z-choice-value="' + CSS.escape(value) + '"]')?.textContent.trim() || value);
      setValue(root, values, labels, 'listbox');
      option.focus();
    };
    root.addEventListener('click', event => {
      const option = event.target.closest('[data-z-choice-option]');
      select(option);
    });
    root.addEventListener('keydown', event => {
      const options = [...root.querySelectorAll('[data-z-choice-option]:not([disabled])')];
      if (!options.length) return;
      const current = document.activeElement?.closest?.('[data-z-choice-option]');
      const index = Math.max(0, options.indexOf(current));
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        options[(index + (event.key === 'ArrowDown' ? 1 : -1) + options.length) % options.length].focus();
      } else if (event.key === 'Home' || event.key === 'End') {
        event.preventDefault();
        options[event.key === 'Home' ? 0 : options.length - 1].focus();
      } else if ((event.key === 'Enter' || event.key === ' ') && current) {
        event.preventDefault();
        select(current);
      }
    });
  });

  document.querySelectorAll('[data-z-dropdown-tree]').forEach(root => {
    const summary = root.querySelector('[data-z-tree-summary]');
    summary.dataset.zPlaceholder ||= summary.textContent;
    const multiple = root.dataset.zMultiple === 'true';
    root.querySelector('[data-z-choice-toggle]')?.addEventListener('click', () => open(root, root.querySelector('[data-z-choice-menu]').hidden));
    root.addEventListener('click', event => {
      const expander = event.target.closest('[data-z-tree-expand]');
      if (expander) {
        const node = expander.closest('[role=treeitem]');
        const child = node.querySelector(':scope > .z-tree-choice-children');
        child.hidden = !child.hidden;
        node.setAttribute('aria-expanded', String(!child.hidden));
        expander.classList.toggle('is-expanded', !child.hidden);
        return;
      }
      const option = event.target.closest('[data-z-choice-option]');
      if (!option || option.disabled) return;
      let values = selectedValues(root);
      if (multiple) {
        const index = values.indexOf(option.dataset.zChoiceValue);
        if (index >= 0) values.splice(index, 1); else values.push(option.dataset.zChoiceValue);
      } else values = [option.dataset.zChoiceValue];
      const labels = values.map(value => root.querySelector('[data-z-choice-option][data-z-choice-value="' + CSS.escape(value) + '"]')?.dataset.zChoiceLabel || value);
      setValue(root, values, labels, 'dropdown-tree');
      if (!multiple) open(root, false);
    });
  });

  document.querySelectorAll('[data-z-mention]').forEach(root => {
    const editor = root.querySelector('[data-z-mention-editor]');
    const menu = root.querySelector('[data-z-mention-menu]');
    const hidden = root.querySelector('[data-z-mention-value]');
    const triggers = root.dataset.zTriggers || '@';
    const openMention = () => {
      const text = editor.textContent || '';
      const triggerIndex = Math.max(...[...triggers].map(current => text.lastIndexOf(current)));
      const trigger = triggerIndex >= 0 ? text[triggerIndex] : null;
      const query = trigger ? text.slice(triggerIndex + 1) : '';
      menu.hidden = !trigger;
      if (trigger) {
        filter(root, query);
        queueRemote(root, query, '[data-z-mention-menu]');
      }
    };
    editor.addEventListener('input', () => {
      hidden.value = editor.textContent || '';
      fire(root, 'mention', hidden.value, hidden.value);
      openMention();
    });
    root.addEventListener('click', event => {
      const option = event.target.closest('[data-z-mention-option]');
      if (!option) return;
      const label = option.dataset.zChoiceLabel || option.textContent.trim();
      const text = editor.textContent || '';
      const triggerIndex = Math.max(...[...triggers].map(trigger => text.lastIndexOf(trigger)));
      editor.textContent = text.slice(0, triggerIndex) + '@' + label + ' ';
      hidden.value = editor.textContent;
      menu.hidden = true;
      editor.focus();
      fire(root, 'mention', hidden.value, label);
    });
  });

  document.addEventListener('pointerdown', event => {
    if (!event.target.closest('[data-z-autocomplete],[data-z-multi-dropdown],[data-z-dropdown-tree],[data-z-multi-column-combobox]')) closeOthers();
    if (!event.target.closest('[data-z-mention]')) document.querySelectorAll('[data-z-mention-menu]').forEach(menu => menu.hidden = true);
  });
})();
