# ZoorceCode 1.0.0 release

## Contents

- Version: `1.0.0`
- Date: 2026-10-24
- Package entry point: `Zoorce.Web.UI`
- Release notes: [CHANGELOG.md](CHANGELOG.md)

## Release pipeline

Pushing the `v1.0.0` tag invokes `.github/workflows/release.yml`. The workflow builds the CSS, restores, compiles, tests, packs every package project and attaches the `.nupkg` artifacts to the GitHub release.

## Publication prerequisites

- Attach this local repository to its GitHub remote.
- Add the repository license text and package license metadata selected by the owner.
- Configure NuGet publishing credentials if packages should be pushed to nuget.org after the GitHub release.
