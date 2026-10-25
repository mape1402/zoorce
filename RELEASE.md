# ZoorceCode releases

The release process follows Mule's build-and-release workflow. Update `.release` to a single version tag such as `v1.0.0` and add the corresponding `## [v1.0.0]` section to `CHANGELOG.md`. When that change reaches `main`, the workflow builds and tests the solution, enforces coverage, creates `releases/v1.0.0`, extracts the changelog notes, packages the libraries, creates the GitHub tag and release, and publishes the packages to NuGet.

The marker only triggers publication when it changes in a push to `main`. A manual workflow dispatch is also supported from an existing `releases/vX.Y.Z` branch. Existing tags pointing to another commit are rejected; published versions must not be moved or overwritten.

## Publication prerequisites

- Set the repository variable `NUGET_USER` to the nuget.org account that configured the Trusted Publishing policy.
- Configure a NuGet Trusted Publishing policy for this repository and `.github/workflows/build-and-release.yml`.
- Select and add a license before publishing packages. No license has been designated yet.

The initial marker is `v1.0.0`. Merging its addition into `main` will initiate publication after the checks succeed, so configure the prerequisites first.
