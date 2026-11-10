# Contributing to OrderedLinqOps

Thanks for contributing.

## Core Standards

1. **Streaming first**: avoid unnecessary buffering.
2. **Compatibility first**: avoid unplanned public API breaks.
3. **Accuracy first**: docs/comments must match behavior.
4. **Tests first**: behavior changes need tests.
5. **Target-framework truth**: use `src/OrderedLinqOps/OrderedLinqOps.csproj` as the source of truth.

## Required for Code Changes

- Update `CHANGELOG.md` in `Unreleased` for user-facing changes.
- Keep public API XML docs complete and accurate.
- Keep comments current; remove stale comments.
- Add/update tests for changed behavior, including edge/error cases.

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
