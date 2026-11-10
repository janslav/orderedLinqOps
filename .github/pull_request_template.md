---
name: Pull Request
about: Submit a change to OrderedLinqOps
title: ''
labels: ''
assignees: ''

---

## Description

### Why is this change needed?
<!-- Explain the motivation for this change. Link related issues: Closes #123 -->

### What changed?
<!-- Describe the specific changes you made -->

### How was this tested?
<!-- Explain how you verified the changes work correctly -->

---

## Type of Change

<!-- Mark the type of change with an X -->

- [ ] **Bug fix** (non-breaking change fixing an issue)
- [ ] **Feature** (non-breaking change adding functionality)
- [ ] **Breaking change** (change that would cause existing code to break)
- [ ] **Documentation** (no code changes)
- [ ] **Performance optimization**
- [ ] **Refactoring** (no behavior change)

---

## Checklist

<!-- All items below should be completed before opening the PR -->

- [ ] **CHANGELOG.md** updated in the "Unreleased" section
- [ ] **Tests** added or updated (if applicable)
- [ ] **All public APIs** have complete XML documentation
- [ ] **Code compiles** without warnings (`dotnet build`)
- [ ] **Tests pass** across all frameworks (`dotnet test`)
- [ ] **Code coverage** is ≥ 85% (check codecov report)
- [ ] **Comments** are accurate and explain *why* (not *what*)
- [ ] **No stale comments** (comments match current code)
- [ ] **Multi-target compatibility** verified (.NET Standard 2.0/2.1, .NET 6.0/8.0)
- [ ] **No breaking changes** OR breaking changes are clearly documented
- [ ] **Performance impact** considered (especially for operators)
- [ ] **Branch is up-to-date** with `master`

---

## Related Issues

<!-- Link related GitHub issues using #issue_number or closes #issue_number -->

Closes #

---

## Notes for Reviewers

<!-- Optional: Any notes, concerns, or areas you'd like reviewer feedback on -->

---

## Performance Impact

<!-- For code changes, describe any performance implications -->

---

## Breaking Changes

<!-- If this is a breaking change, explain why and how users should migrate -->

N/A

---

**Remember**: Automated checks will validate:
1. ✅ Mechanical quality (CHANGELOG, docs, tests)
2. ✅ Code review (design, performance, compatibility)
3. ✅ Human approval (final sign-off)
