# Branch Protection Rules

This document describes branch protection for `master`.

## Goals

- Keep `master` merge-only
- Require automated checks
- Require at least one human approval

## Enforced Rules

1. **Required status checks**
   - `Quality Checks` workflow must pass
   - Branch must be up to date with `master` before merge

2. **Required pull request review**
   - Minimum 1 approval
   - Dismiss stale approvals on new commits

3. **Admin enforcement**
   - Rules apply to admins as well

4. **History safety**
   - No force-push
   - No branch deletion

## Merge Expectations

- Keep commit history clean before merge.
- Squash/fixup review-only noise commits.
- Multiple commits are fine when each commit is a complete, meaningful step.

## Managing Protection

Use GitHub UI (Settings → Branches) or GitHub CLI directly.

View current protection:

```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection
```

Update protection:

```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection -X PUT --input protection-config.json
```

Disable protection (emergency only):

```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection -X DELETE
```
