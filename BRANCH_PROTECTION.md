# Branch Protection Rules

This document describes the branch protection rules enforced on the `master` branch of OrderedLinqOps.

## Why Branch Protection?

Branch protection ensures that all code merged to `master`:
- ✅ Passes quality checks (mechanical validation)
- ✅ Receives automated code review
- ✅ Gets human approval
- ✅ Maintains backward compatibility
- ✅ Includes proper documentation and tests

## Rules Enforced

### 1. Require Status Checks to Pass Before Merging
- **Status Check**: "Quality Checks" workflow must pass
- **Strict Mode**: Enabled - branch must be up-to-date with master before merge
- **What it validates**:
  - XML documentation completeness
  - CHANGELOG.md presence (if code changed)
  - Test suite passes across all frameworks
  - No syntax errors or build failures

### 2. Require Pull Request Reviews Before Merging
- **Approvals Required**: 1
- **Dismiss Stale Reviews**: Yes - reviews are dismissed if code changes
- **Code Owner Reviews**: Not required (but encouraged)
- **Who can review**: Anyone with read access to the repo

### 3. Enforce Admins
- **Enabled**: Yes - rules apply to repository admins too
- **Ensures**: Consistency—everyone follows the same process

### 4. Additional Protections
- **Force Push**: Disabled - protects against accidental history rewrite
- **Deletions**: Disabled - protects against accidental branch deletion
- **Linear History**: Disabled - allows merge commits (not required to rebase)

---

## Merge Workflow

```
1. Open Pull Request
   ↓
2. Run Automated Quality Checks
   - GitHub Actions: quality-checks.yml
   - Validates: CHANGELOG, XML docs, tests
   - Posts: Detailed findings on PR
   ↓
3. Trigger Code Review
   - GitHub Actions: code-review.yml
   - Posts: "Review requested" comment
   ↓
4. Copilot Agent Reviews
   - Uses: .copilot-instructions.md
   - Evaluates: Design, performance, compatibility
   - Posts: Detailed review findings
   ↓
5. Human Review & Approval
   - At least 1 approval required
   - Review against: CODE_REVIEW_CHECKLIST.md
   ↓
6. Merge to Master
   - All checks must pass
   - Must be up-to-date with master (strict mode)
   - Automatically closes the PR
```

---

## For Contributors

### Before Pushing
- ✅ Run `dotnet build` to verify compilation
- ✅ Run `dotnet test` to verify tests pass
- ✅ Update CHANGELOG.md if code changed
- ✅ Ensure XML docs on all public APIs
- ✅ Review comments for accuracy

### When Opening PR
- ✅ Fill out PR template completely
- ✅ Link related issues
- ✅ Describe why, what, and how
- ✅ Explain any performance implications

### During Review
- ✅ Address feedback promptly
- ✅ Push new commits to update PR (don't force push)
- ✅ Respond to questions/concerns
- ✅ Request re-review after changes

### Before Merge
- ✅ Ensure branch is up-to-date with master (GitHub will prompt)
- ✅ Get at least 1 approval
- ✅ All CI checks passing
- ✅ Code review complete

---

## For Maintainers

### If CI Fails
1. Check the "Quality Checks" comment on the PR
2. Identify the specific failure
3. Comment on PR asking contributor to fix
4. Wait for contributor to push fixes
5. CI will re-run automatically

### If Review is Stale
- Stale reviews are **automatically dismissed** when new code is pushed
- Request re-review through GitHub UI
- Or ping reviewer in PR comments

### If You Need to Bypass (Emergency Only)
Admins can temporarily:
1. Dismiss a review requirement
2. Push directly to master (not recommended)
3. **Always document why** in a follow-up PR or issue

Better alternative: Use a `hotfix/` branch and fast-track review.

---

## Managing Branch Protection

### View Current Rules
```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection
```

### Update Rules
Edit `.github/scripts/update-branch-protection.sh` and run it, or use:
```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection -X PUT --input protection-config.json
```

See `branch-protection.json` for current configuration.

### Disable/Remove Protection (Not Recommended)
```bash
gh api repos/janslav/orderedLinqOps/branches/master/protection -X DELETE
```

---

## Related Documentation

- [CONTRIBUTING.md](CONTRIBUTING.md) - How to contribute
- [CODE_REVIEW_CHECKLIST.md](CODE_REVIEW_CHECKLIST.md) - What reviewers check
- [.copilot-instructions.md](.copilot-instructions.md) - Agent review guidelines
- [.github/workflows/quality-checks.yml](.github/workflows/quality-checks.yml) - Mechanical validation
- [.github/workflows/code-review.yml](.github/workflows/code-review.yml) - Review workflow
