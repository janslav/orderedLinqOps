# Changelog

All notable changes to OrderedLinqOps should be documented here in relation to the repository state that existed before the change.

Entries should describe the actual delta introduced by the change, not internal cleanup work done while iterating on the same PR. Fixes that only correct mistakes in the PR itself do not belong in the changelog unless they also change shipped behavior or contributor-facing process expectations.

## [Unreleased]

### Added
- Added contributor-facing guidance for code quality, documentation, and PR workflow expectations.
- Added a repository review checklist to separate mechanical CI gates from higher-level code review concerns.
- Added branch protection documentation describing the required review and status-check workflow for `master`.
- Added changelog enforcement guidance and CI checks for code changes that require a documented release note.
- Added an automated mechanical benchmark-regression check that compares Ordered operator performance on PR code against `master` when Ordered operator implementation files change.

### Changed
- Clarified the split between CI automation and reviewer responsibilities for quality enforcement.
- Simplified the PR template and contributor docs to reduce redundancy and keep the guidance aligned with the repository's actual target frameworks.
- Updated the repository's contribution guidance to use the project file as the source of truth for supported frameworks and commit hygiene expectations.
- Improved out-of-order exception diagnostics to include zero-based index, previous/current key values, comparer result, comparer type, and remediation guidance.
- Updated benchmark documentation with investigation findings on per-group buffering behavior and refreshed benchmark results.
- Clarified README memory-efficiency wording to reflect incremental processing with per-group buffering.
- Narrowed mechanical benchmark regression checks to a representative Ordered-only benchmark suite and added a guard that skips comparison when `master` does not yet contain the benchmark project.

### Fixed
- Reduced `OrderedGroupJoin` allocations by removing unnecessary outer pre-grouping and streaming outer elements directly.

### Deprecated
- No deprecated APIs or workflow changes are recorded for this release yet.

### Removed
- No removals recorded for this release yet.

### Security
- No security fixes recorded for this release yet.
