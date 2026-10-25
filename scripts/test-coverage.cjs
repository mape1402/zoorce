const { spawnSync } = require('node:child_process');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');

function coveragePercent(xml) {
  const root = xml.match(/<coverage\b[^>]*>/)?.[0];
  const covered = root?.match(/\blines-covered="(\d+)"/)?.[1];
  const valid = root?.match(/\blines-valid="(\d+)"/)?.[1];
  if (covered === undefined || valid === undefined || Number(valid) === 0 || Number(covered) > Number(valid)) {
    throw new Error('The coverage report does not contain valid line counts.');
  }
  return Number(covered) * 100 / Number(valid);
}

function run() {
  const root = path.resolve(__dirname, '..');
  const resultDir = fs.mkdtempSync(path.join(os.tmpdir(), 'zoorce-coverage-'));
  try {
    const result = spawnSync('dotnet', [
      'test', 'tests/Zoorce.Web.UI.Tests/Zoorce.Web.UI.Tests.csproj',
      '--configuration', 'Release', '--no-restore', '--collect', 'XPlat Code Coverage',
      '--settings', 'tests/coverage.runsettings', '--results-directory', resultDir
    ], { cwd: root, stdio: 'inherit' });
    if (result.error) throw result.error;
    if (result.status !== 0) return result.status ?? 1;

    const reports = fs.readdirSync(resultDir, { withFileTypes: true })
      .filter(entry => entry.isDirectory())
      .map(entry => path.join(resultDir, entry.name, 'coverage.cobertura.xml'))
      .filter(file => fs.existsSync(file));
    if (reports.length !== 1) throw new Error(`Expected one coverage report; found ${reports.length}.`);
    const xml = fs.readFileSync(reports[0], 'utf8');
    const percent = coveragePercent(xml);
    const output = path.join(root, 'tests/Zoorce.Web.UI.Tests/artifacts/coverage/coverage.cobertura.xml');
    fs.mkdirSync(path.dirname(output), { recursive: true });
    fs.writeFileSync(output, xml);
    console.log(`Line coverage: ${percent.toFixed(2)}% (required: 90%)`);
    return percent >= 90 ? 0 : 1;
  } finally {
    fs.rmSync(resultDir, { recursive: true, force: true });
  }
}

if (require.main === module) {
  try { process.exitCode = run(); }
  catch (error) { console.error(error.message); process.exitCode = 1; }
}

module.exports = { coveragePercent };
