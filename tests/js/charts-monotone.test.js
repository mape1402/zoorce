const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../../src/Zoorce.Web.UI.Charts/wwwroot/js/zoorce-charts.js'), 'utf8');
const loadPathFor = () => {
    const context = {
        window: { requestAnimationFrame: callback => callback(), ZoorceData: {} },
        document: { querySelectorAll: () => [] },
        console,
        Intl,
        Number,
        Math,
        Set,
        WeakMap,
        JSON,
        Date,
        Object,
        String,
        Array,
        CustomEvent: class {}
    };
    vm.runInNewContext(source.replace(/\}\)\(\);\s*$/, 'globalThis.__pathFor = pathFor;})();'), context);
    return context.__pathFor;
};

const numbers = value => value.match(/-?\d+(?:\.\d+)?(?:e[+-]?\d+)?/gi).map(Number);
const cubicSegments = pathValue => {
    const values = numbers(pathValue);
    const segments = [];
    let cursor = { x: values[0], y: values[1] };
    for (let index = 2; index < values.length; index += 6) {
        const [x1, y1, x2, y2, x, y] = values.slice(index, index + 6);
        segments.push({ start: cursor, c1: { x: x1, y: y1 }, c2: { x: x2, y: y2 }, end: { x, y } });
        cursor = { x, y };
    }
    return segments;
};
const cubic = (a, b, c, d, t) => (1 - t) ** 3 * a + 3 * (1 - t) ** 2 * t * b + 3 * (1 - t) * t ** 2 * c + t ** 3 * d;
const assertWithinSegmentBounds = (pathValue, epsilon = 1e-8) => cubicSegments(pathValue).forEach(segment => {
    const lower = Math.min(segment.start.y, segment.end.y) - epsilon;
    const upper = Math.max(segment.start.y, segment.end.y) + epsilon;
    for (let step = 0; step <= 100; step += 1) {
        const y = cubic(segment.start.y, segment.c1.y, segment.c2.y, segment.end.y, step / 100);
        assert.ok(y >= lower && y <= upper, `expected ${y} inside [${lower}, ${upper}]`);
    }
});

test('monotone curve does not overshoot a non-negative 0, 0, 19, 0 series', () => {
    const pathFor = loadPathFor();
    const pathValue = pathFor([{ x: 0, y: 0 }, { x: 1, y: 0 }, { x: 2, y: 19 }, { x: 3, y: 0 }], 'Monotone');
    assert.match(pathValue, / C /);
    assertWithinSegmentBounds(pathValue);
});

test('monotone curve preserves increasing, peak, negative and non-uniform series bounds', () => {
    const pathFor = loadPathFor();
    const cases = [
        [{ x: 0, y: 0 }, { x: 1, y: 5 }, { x: 2, y: 10 }, { x: 3, y: 15 }],
        [{ x: 0, y: 0 }, { x: 1, y: 10 }, { x: 2, y: 0 }],
        [{ x: 0, y: -8 }, { x: 1.2, y: -2 }, { x: 4.7, y: -11 }, { x: 10, y: -4 }],
        [{ x: 0, y: 0 }, { x: .5, y: 3 }, { x: 9, y: 1 }, { x: 11, y: 8 }]
    ];
    cases.forEach(points => assertWithinSegmentBounds(pathFor(points, 'Monotone')));
});

test('monotone curve intentionally falls back to straight segments for two or invalid x values', () => {
    const pathFor = loadPathFor();
    assert.equal(pathFor([{ x: 0, y: 0 }, { x: 1, y: 10 }], 'Monotone'), 'M 0 0 L 1 10');
    assert.doesNotMatch(pathFor([{ x: 0, y: 0 }, { x: 0, y: 10 }, { x: 1, y: 4 }], 'Monotone'), / C /);
});

test('trend rendering shares monotone paths with area rendering and retains interactions', () => {
    assert.match(source, /pathFor\(points, this\.configuration\.curve\)/);
    assert.match(source, /bindTooltip\(/);
    assert.match(source, /aria-pressed/);
});
