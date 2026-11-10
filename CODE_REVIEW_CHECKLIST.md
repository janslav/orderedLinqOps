# Code Review Checklist for OrderedLinqOps

Use this for human and agent reviews.

## Pre-Screened by CI (do not re-check mechanically)

CI gates already verify:
- Build passes
- Tests pass
- XML documentation gate
- CHANGELOG update requirement (for code changes)

If CI is red, address that first.

## Quality Checks (reviewer responsibility)

### 1) Correctness and behavior
- [ ] Ordering validation is correct and occurs during iteration
- [ ] Error messages are actionable
- [ ] Null/empty/single-item paths behave correctly
- [ ] No behavior regressions in existing operators

### 2) Streaming and memory
- [ ] No unnecessary buffering introduced
- [ ] Iteration remains lazy where expected
- [ ] Allocation impact is reasonable for the operator

### 3) API compatibility
- [ ] Public API changes are intentional and documented
- [ ] Existing signatures/semantics are preserved unless explicitly planned

### 4) Documentation and comments
- [ ] Public API docs are accurate and complete
- [ ] Comments explain why (not what)
- [ ] No stale comments remain after the change

### 5) Pattern consistency
- [ ] New/changed operators follow conventions used by existing operators
- [ ] Naming and overload style are consistent with the codebase

### 6) Target-framework compatibility
- [ ] Compatibility expectations match target frameworks declared in `src/OrderedLinqOps/OrderedLinqOps.csproj`

### 7) Tests quality
- [ ] Tests cover changed behavior and edge cases
- [ ] Tests validate negative/error paths where relevant

## Decision

- **Request changes** for correctness, streaming, compatibility, or stale-doc/comment issues.
- **Comment** for non-blocking suggestions.
- **Approve** only when all applicable checks are satisfied.
