const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../../src/Zoorce.Web.UI/wwwroot/js/zoorce-valky.js'), 'utf8');

function setup(reduced = false) {
    const listeners = {};
    let observerCallback;
    const timers = [];
    const motion = { matches: reduced, addEventListener() {} };
    const document = { documentElement: {}, addEventListener: (name, callback) => { listeners[name] = callback; }, querySelectorAll: () => [] };
    class MutationObserver { constructor(callback) { observerCallback = callback; } observe() {} }
    class CustomEvent { constructor(type, options) { this.type = type; Object.assign(this, options); } }
    vm.runInNewContext(source, { document, matchMedia: () => motion, MutationObserver, CustomEvent, Element: class {}, setTimeout: callback => { timers.push(callback); return timers.length; }, clearTimeout() {} });
    const events = [];
    const companion = { dataset: { zValkyState: 'celebrating', zValkyCelebration: 'once' }, dispatchEvent: event => events.push(event) };
    return { listeners, companion, events, timers, mutate: () => observerCallback([{ type: 'attributes', target: companion, addedNodes: [] }]) };
}

test('one-shot celebration returns to idle and informs the host', () => {
    const { listeners, companion, events } = setup();
    listeners.animationend({ animationName: 'z-valky-celebrate', target: { closest: () => companion } });
    assert.equal(companion.dataset.zValkyState, 'idle');
    assert.equal(events[0].type, 'zoorce:valky-state-change');
    assert.equal(events[0].detail.reason, 'celebration-complete');
});

test('looping or stale animation completion never resets another state', () => {
    const { listeners, companion, events } = setup();
    companion.dataset.zValkyCelebration = 'loop';
    listeners.animationend({ animationName: 'z-valky-celebrate', target: { closest: () => companion } });
    companion.dataset.zValkyCelebration = 'once';
    companion.dataset.zValkyState = 'thinking';
    listeners.animationend({ animationName: 'z-valky-celebrate', target: { closest: () => companion } });
    assert.equal(companion.dataset.zValkyState, 'thinking');
    assert.equal(events.length, 0);
});

test('reduced motion still finishes a one-shot celebration', () => {
    const { companion, timers, mutate } = setup(true);
    mutate();
    assert.equal(timers.length, 1);
    timers[0]();
    assert.equal(companion.dataset.zValkyState, 'idle');
});
