const { test } = require('node:test');
const assert = require('node:assert/strict');
const { coveragePercent } = require('../../scripts/test-coverage.cjs');

test('coverage threshold uses actual line counts, without rounding', () => {
  assert.equal(coveragePercent('<coverage lines-valid="1000" lines-covered="899"/>'), 89.9);
  assert.equal(coveragePercent('<coverage lines-covered="90" lines-valid="100"/>'), 90);
});

test('coverage threshold rejects missing and invalid data', () => {
  for (const xml of ['', '<coverage/>', '<coverage lines-covered="0" lines-valid="0"/>', '<coverage lines-covered="2" lines-valid="1"/>']) {
    assert.throws(() => coveragePercent(xml));
  }
});
