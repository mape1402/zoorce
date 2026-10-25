const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const script = fs.readFileSync(path.join(__dirname, '../../src/Zoorce.Web.UI.DataGrid/wwwroot/js/zoorce-data-grid.js'), 'utf8');

function createGrid({ loadMode = 'InfiniteScroll', resizeObserver = true } = {}) {
    let height = 37.5;
    let resizeCallback;
    let observer;
    const properties = new Map();
    const header = { getBoundingClientRect: () => ({ height }) };
    const viewport = {
        addEventListener() {},
        style: {
            setProperty: (name, value) => properties.set(name, value),
            removeProperty: name => properties.delete(name)
        }
    };
    const elements = {
        '[data-z-grid-config]': { textContent: JSON.stringify({ loadMode, columns: [], binding: {} }) },
        '[data-z-grid-viewport]': viewport,
        '.z-data-grid thead': header
    };
    const root = {
        matches: selector => selector === '[data-z-data-grid]',
        querySelector: selector => elements[selector] ?? null,
        querySelectorAll: () => [],
        addEventListener() {},
        setAttribute() {},
        removeAttribute() {}
    };
    class TestResizeObserver {
        constructor(callback) { this.callback = callback; observer = this; }
        observe(element) { assert.equal(element, header); }
        disconnect() { this.disconnected = true; }
    }
    const window = {
        ZoorceData: { request: () => new Promise(() => {}) },
        addEventListener: (name, callback) => { if (name === 'resize') resizeCallback = callback; },
        clearTimeout() {}
    };
    if (resizeObserver) window.ResizeObserver = TestResizeObserver;
    const context = { window, document: { querySelectorAll: () => [] }, AbortController, ResizeObserver: TestResizeObserver };
    vm.runInNewContext(script, context);
    window.ZoorceDataGrid.mount(root);
    return {
        properties,
        get observer() { return observer; },
        resize(newHeight) {
            height = newHeight;
            if (observer) observer.callback();
            else resizeCallback?.();
        },
        dispose: () => window.ZoorceDataGrid.dispose(root)
    };
}

test('infinite grid tracks header height and cleans up its observer', () => {
    const grid = createGrid();
    assert.equal(grid.properties.get('--z-grid-header-height'), '37.5px');
    grid.resize(58);
    assert.equal(grid.properties.get('--z-grid-header-height'), '58px');
    grid.dispose();
    assert.equal(grid.observer.disconnected, true);
    assert.equal(grid.properties.has('--z-grid-header-height'), false);
});

test('infinite grid falls back to window resize without ResizeObserver', () => {
    const grid = createGrid({ resizeObserver: false });
    grid.resize(52);
    assert.equal(grid.properties.get('--z-grid-header-height'), '52px');
    grid.dispose();
});

test('paginated grid does not observe or modify the header', () => {
    const grid = createGrid({ loadMode: 'Pagination' });
    assert.equal(grid.observer, undefined);
    assert.equal(grid.properties.has('--z-grid-header-height'), false);
    grid.dispose();
});
