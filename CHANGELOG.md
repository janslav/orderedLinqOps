# Changelog

All notable changes to OrderedLinqOps should be documented here in relation to the repository state that existed before the change.

Entries should describe the actual delta introduced by the change, not internal cleanup work done while iterating on the same PR. Fixes that only correct mistakes in the PR itself do not belong in the changelog unless they also change shipped behavior or contributor-facing process expectations.

## [Unreleased]

### Added
- Added contributor-facing guidance for code quality, documentation, and PR workflow expectations.
- Added a repository review checklist to separate mechanical CI gates from higher-level code review concerns.
- Added branch protection documentation describing the required review and status-check workflow for `master`.

### Changed
- Clarified the split between CI automation and reviewer responsibilities for quality enforcement.
- Simplified the PR template and contributor docs to reduce redundancy and keep the guidance aligned with the repository's actual target frameworks.
- Updated the repository's contribution guidance to use the project file as the source of truth for supported frameworks and commit hygiene expectations.
