# Contributing to OrderedLinqOps

Thanks for contributing.

## Core Standards

1. **Streaming first**: avoid unnecessary buffering.
2. **Compatibility first**: avoid unplanned public API breaks.
3. **Accuracy first**: docs/comments must match behavior.
4. **Tests first**: behavior changes need tests.
5. **Target-framework truth**: use `src/OrderedLinqOps/OrderedLinqOps.csproj` as the source of truth.

## Required for Code Changes

- Update `CHANGELOG.md` in `Unreleased` for meaningful repository changes.
- Follow the changelog semantics described in `CHANGELOG.md`: entries describe the delta relative to the repository state before the PR, not internal cleanup or self-corrections made while working on the same PR.
- Make a changelog entry whenever a PR changes shipped behavior, public API surface, contributor workflow expectations, or repository-facing tooling.
- Keep public API XML docs complete and accurate.
- Keep comments current; remove stale comments.
- Add/update tests for changed behavior, including edge/error cases.

## Changelog Rules

Write entries under the `Unreleased` section in Keep a Changelog format. Use the categories below and keep each bullet focused on the user-visible or contributor-facing delta:

```md
## [Unreleased]

### Added
- Added support for ...

### Changed
- Updated ... to ...

### Fixed
- Fixed ... when ...

### Deprecated
- Deprecated ... in favor of ...

### Removed
- Removed ...

### Security
- Fixed a vulnerability in ...
```

When to add entries:
- Always for `src/` code changes that affect behavior, compatibility, or tooling.
- Always for changes to contributor-facing rules or repository workflows that affect future contributors.
- Do not add a changelog entry for pure test-only changes when no production code changed.
- Do not add entries for internal refactors or reformatting that do not change shipped behavior.

Use concise, descriptive entries that tell a reviewer what changed and why it matters.

## PR Process

Before opening a PR:

```bash
dotnet restore
dotnet build
dotnet test
```

When opening a PR:
- Fill out the PR template.
- Link related issues.
- Explain why, what, and how verified.

During review:
- Address feedback.
- Keep commit history clean (squash/fixup review-only noise commits).
- Multiple commits are fine when each is a complete, meaningful step.

## Review Model

The workflow is split intentionally:
- **CI bot**: mechanical gates (build/test/changelog/XML-doc gate).
- **Agent/Human review**: correctness, streaming behavior, compatibility, patterns, and doc/comment quality.

## Development Setup

Install the .NET SDK required by the repository target frameworks (see `src/OrderedLinqOps/OrderedLinqOps.csproj`).

## Commit Messages

Use Conventional Commits:

```text
<type>(<scope>): <subject>
```

Examples of `type`: `feat`, `fix`, `docs`, `test`, `refactor`, `perf`, `chore`, `ci`.
