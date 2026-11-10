# Code Review Checklist

Use this checklist when reviewing pull requests for OrderedLinqOps. This applies to both human reviewers and automated code review agents.

## Documentation & Communication

- [ ] PR title clearly describes the change (use Conventional Commits format)
- [ ] PR description explains *why* the change is needed
- [ ] PR description explains *what* changed
- [ ] PR description explains *how* it was tested
- [ ] Any breaking changes are clearly highlighted
- [ ] Related issues are linked (e.g., "Closes #123")

## CHANGELOG

- [ ] CHANGELOG.md has been updated in "Unreleased" section
- [ ] CHANGELOG entry is accurate and describes user-facing changes
- [ ] CHANGELOG uses appropriate category (Added, Changed, Fixed, etc.)
- [ ] For bug fixes: root cause is explained (if complex)

## Code Quality & Standards

### XML Documentation
- [ ] All public methods have complete XML documentation
- [ ] All public types have `<summary>` documentation
- [ ] All `<typeparam>`, `<param>`, and `<returns>` are documented
- [ ] All possible exceptions have `<exception>` entries
- [ ] `<remarks>` explain important behaviors (buffering, streaming, ordering validation)
- [ ] Documentation is accurate and matches implementation

### Comments & Clarity
- [ ] Inline comments explain **why** (not **what** the code does)
- [ ] Comments reference relevant GitHub issues for complex logic
- [ ] No stale or outdated comments (comments match current code behavior)
- [ ] Code is clear enough that unnecessary comments have been removed
- [ ] TODO/FIXME markers are justified and linked to issues if possible

### Design & Pattern Compliance
- [ ] For new operators: follows existing OrderedLinqOps patterns
- [ ] Method names follow LINQ conventions (`OrderedGroupBy`, `OrderedJoin`, etc.)
- [ ] Overloads are consistent with similar LINQ operators
- [ ] `IComparer<TKey>?` is used correctly for optional comparers
- [ ] Default comparer is `Comparer<TKey>.Default`

## Correctness & Behavior

### Ordering Validation
- [ ] Input ordering is validated during iteration (not upfront)
- [ ] `ArgumentException` is thrown with descriptive message for unsorted input
- [ ] Error messages include actual key values and comparison results
- [ ] Error messages suggest remedies (e.g., "Use .OrderBy(keySelector) to pre-sort")

### Streaming & Memory
- [ ] **CRITICAL**: Operator does NOT buffer unnecessary data in memory
- [ ] Results are yielded as soon as they are available (streaming behavior)
- [ ] Only intentional buffering (e.g., groups in `OrderedGroupBy`) exists
- [ ] No large intermediate collections are created unnecessarily

### Multi-Target Compatibility
- [ ] Code compiles for all targets: .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0
- [ ] No platform-specific APIs used without conditional compilation
- [ ] Tests pass on all target frameworks
- [ ] Appropriate `#if` directives used if framework-specific code is needed

### Backward Compatibility
- [ ] No breaking changes to public API (if patch/minor version)
- [ ] Existing overloads are not modified (new overloads OK)
- [ ] Public method signatures remain compatible
- [ ] Exception types thrown remain the same or are subsets
- [ ] Behavior is backward compatible (no changes to semantics)

## Testing

- [ ] Tests are included for new functionality
- [ ] Bug fixes include a test that reproduces the issue
- [ ] Tests are comprehensive (normal cases + edge cases)
- [ ] Tests cover error conditions and exceptions
- [ ] Tests include validation of streaming behavior (not collecting all items)
- [ ] Tests verify all target frameworks work correctly
- [ ] Code coverage is ≥ 85% (codecov report should show this)
- [ ] No tests are skipped or marked as pending
- [ ] Test names are clear and descriptive
- [ ] Tests use NUnit conventions

## Performance

- [ ] No performance regressions (especially critical for streaming operators)
- [ ] Memory allocations are minimized
- [ ] Algorithm complexity is reasonable for typical use cases
- [ ] For significant changes: benchmarks have been run (see #13)
- [ ] Large dataset testing has been considered (1M+ rows)

## File Organization & Naming

- [ ] Files follow repository naming conventions
- [ ] Related overloads are kept in the same file
- [ ] No unnecessary file reorganization (unless that's the PR's purpose)
- [ ] File paths are clear and organized by functionality

## Integration

- [ ] Dependencies are appropriate and minimal
- [ ] No unnecessary external package additions
- [ ] LGPL-3.0 license compatibility is maintained
- [ ] Existing functionality is not broken

## Security

- [ ] No hardcoded secrets, API keys, or credentials
- [ ] Input validation is present where needed
- [ ] No unsafe code without justification
- [ ] No SQL injection or similar injection vulnerabilities
- [ ] Error messages don't leak sensitive information

## Final Checks

- [ ] PR is in "Ready for Review" state (not Draft)
- [ ] All requested changes have been addressed
- [ ] Code review history shows constructive feedback resolution
- [ ] No merge conflicts
- [ ] Branch is up-to-date with `master`

---

## Review Guidelines

### For Automated Mechanical Checks
Focus on:
1. CHANGELOG.md presence and format
2. XML documentation completeness
3. Test file presence and updates
4. File naming conventions

### For Automated Agent Review
Focus on:
1. Code quality and design patterns
2. Streaming/memory efficiency
3. Multi-target compatibility
4. Backward compatibility
5. Comment accuracy (especially for performance-critical code)
6. OrderedLinqOps pattern adherence

### For Human Review
Focus on:
1. Conceptual correctness and approach
2. Performance implications
3. Design decisions and trade-offs
4. API surface consistency
5. Final approval and sign-off

---

## Common Issues to Watch For

### Streaming Violations
❌ **Bad**: Collecting all items to sort them first
```csharp
var sorted = source.OrderBy(x => x).ToList();
foreach (var item in sorted) { ... }
```

✅ **Good**: Trusting input is already sorted
```csharp
foreach (var item in source) { ... }
```

### Stale Comments
❌ **Bad**: Old comment that no longer matches code
```csharp
// Only handle the first item
while (iterator.MoveNext()) // But we loop over ALL items now!
```

✅ **Good**: Updated or removed comments
```csharp
// Process all items, checking ordering constraint
while (iterator.MoveNext())
```

### Missing Error Context
❌ **Bad**: Generic exception
```csharp
throw new ArgumentException("Invalid");
```

✅ **Good**: Descriptive error with context
```csharp
throw new ArgumentException(
    $"The source collection is not ordered. " +
    $"At position {position}: previous key was {previousKey}, " +
    $"but current key is {currentKey}. " +
    $"Use .OrderBy(keySelector) to pre-sort the input.");
```

### Non-Obvious Logic Without Documentation
❌ **Bad**: Complex logic with no explanation
```csharp
var comparisonResult = comparer.Compare(key, grouping.key);
if (comparisonResult > 0) { /* ... */ }
```

✅ **Good**: Explained with comment
```csharp
// When key > grouping.key, the group is complete and can be yielded
var comparisonResult = comparer.Compare(key, grouping.key);
if (comparisonResult > 0) { /* ... */ }
```
