# Code Review Checklist for OrderedLinqOps

Use this checklist when reviewing pull requests. This focuses on **code quality** checks—mechanical checks are pre-screened by CI.

---

## 🤖 Pre-Screened by CI (Not Your Job)

Before this checklist, the bot has already verified:
- ✅ Build compiles
- ✅ All tests pass
- ✅ XML documentation file exists
- ✅ CHANGELOG.md is updated (if code changed)
- ✅ Labels applied for any failures

**If these fail**, the PR is blocked. No need to review further until they pass.

The agent's job starts here: evaluate code *quality*, not just pass/fail mechanics.

---

## A. Documentation Quality ⭐ (Critical)

### XML Docs: Complete & Accurate
- [ ] `<summary>`: Clear, concise description
- [ ] `<param>`: Every parameter documented with its role
- [ ] `<returns>`: Explains what is returned and *how* (streaming vs buffered)
- [ ] `<exception>`: Lists exceptions with *when* they occur
  - Example: "when input is unsorted" (not just "ArgumentException")
- [ ] `<remarks>`: Explains important behaviors (streaming strategy, ordering assumptions, performance)
- [ ] Documentation **matches implementation** (not generic or outdated)

### Comments: Fresh & Accurate
- [ ] Comments explain *why* not *what* the code does
- [ ] **NO stale comments** (if code changed, comments must be updated or removed) ⚠️ **CRITICAL**
- [ ] Complex logic is documented
- [ ] Ordering assumptions documented
- [ ] Performance considerations documented

### Examples & Remarks
- [ ] For complex operators: usage examples in remarks
- [ ] Edge cases explained
- [ ] Known limitations documented

---

## B. Correctness & Behavior ⭐ (Critical)

### Streaming Compliance (Memory Efficiency)
- [ ] **NO unnecessary buffering** (no `ToList()`, `ToArray()` unless intentional)
- [ ] Results are yielded as items are processed
- [ ] Iterator pattern used properly:
  ```csharp
  using (var iterator = source.GetEnumerator())
  {
      while (iterator.MoveNext())
      {
          yield return ProcessItem(iterator.Current);
      }
  }
  ```
- [ ] Only intentional buffering (e.g., `OrderedGroupBy`) is documented

### Ordering Validation
- [ ] Validation happens *during iteration* (not upfront)
- [ ] Throws `ArgumentException` when ordering is violated
- [ ] Error message includes:
  - Current position/key
  - Previous key value
  - Current key value
  - Suggestion to fix it
  - Example: "At position 5: previous key was 10, current key is 8. Use .OrderBy() to pre-sort."
- [ ] Respects the provided `IComparer<TKey>`

### Edge Cases
- [ ] Empty sequences handled correctly
- [ ] Single-item sequences handled correctly
- [ ] Null inputs properly rejected with `ArgumentNullException`
- [ ] Duplicate keys handled correctly (if relevant)
- [ ] Large sequences handled efficiently

### Backward Compatibility
- [ ] No changes to existing public API signatures
- [ ] New overloads only (additions OK, breaking changes NOT OK)
- [ ] Existing behavior unchanged
- [ ] Exception types remain compatible

---

## C. Multi-Target Compatibility ⭐ (Critical)

- [ ] No framework-specific breaks across .NET Standard 2.0, 2.1, .NET 6.0, 8.0
- [ ] No .NET 6+ syntax in .NET Standard code
- [ ] Conditional compilation used properly if framework-specific APIs needed:
  ```csharp
  #if NETSTANDARD2_0
      var result = source.Where(x => x != null);
  #else
      var result = source.Where(x => x is not null);
  #endif
  ```
- [ ] Type constraints work across all targets

---

## D. Performance & Memory ⭐ (Critical)

- [ ] **No unexpected allocations**:
  - Enumerators reused, not recreated per iteration
  - Comparers cached, not recreated
  - No LINQ chains that re-enumerate
  - Lambdas not captured unnecessarily

- [ ] **Algorithm efficiency**:
  - Time complexity matches operator's intent
  - Acceptable performance for large datasets (1M+ items)
  - Sorted input is leveraged (not ignored)

- [ ] **Memory footprint**:
  - Streaming = O(1) memory (not O(n))
  - No hidden buffers or caches
  - Minimal enumerator state

---

## E. Pattern Compliance ⭐ (Critical)

### Follows OrderedLinqOps Conventions
- [ ] Extension method on `IEnumerable<T>` (or `IOrderedEnumerable<T>`)
- [ ] Generic parameters: `<TSource, TKey, TResult>` (or subset)
- [ ] Method name matches LINQ conventions
- [ ] `IComparer<TKey>? comparer = null` parameter (defaults to `Comparer<TKey>.Default`)

### Consistent with Existing Operators
- [ ] Similar operators have matching signatures
- [ ] Null handling: `ArgumentNullException` for required params
- [ ] Parameter order matches pattern
- [ ] Exception types match pattern

Standard pattern to follow:
```csharp
public static IEnumerable<TResult> OperatorName<TSource, TKey, TResult>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    IComparer<TKey>? comparer = null)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
    
    comparer ??= Comparer<TKey>.Default;
    
    // Streaming implementation
}
```

---

## F. Testing Quality ⭐ (Critical)

- [ ] **Normal cases**: Typical usage patterns tested
- [ ] **Edge cases**: Empty sequences, single items, large datasets
- [ ] **Error conditions**: Null inputs, unsorted input, invalid parameters
- [ ] **All exception paths**: Every `throw` statement should have a test
- [ ] **Streaming behavior verified**: Tests check iteration count or lazy evaluation (not collecting all items)
- [ ] **Multi-target**: Tests pass on all frameworks (.NET Standard 2.0, 2.1, .NET 6.0, 8.0)
- [ ] **Coverage ≥ 85%**: Codecov report should show adequate coverage

Example test pattern:
```csharp
[TestMethod]
public void OrderedDistinct_WithSortedInput_ReturnsUniqueItems()
{
    var input = new[] { 1, 1, 2, 2, 3, 3 };
    var result = input.OrderedDistinct().ToList(); // OK to call ToList() in test
    
    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
}
```

---

## Common Issues to Watch

### 🚩 Critical Blockers (Request Changes)
- Breaks streaming by buffering (ToList, ToArray, etc.)
- Missing or incorrect ordering validation
- Breaking change to public API
- **Stale or misleading comments** (**comments MUST stay fresh**)
- Pattern inconsistency with existing operators
- Multi-target compatibility broken
- Backward compatibility violated

### ⚠️ High Priority (Request Changes)
- XML docs incomplete or inaccurate
- Performance regression detected
- Error messages missing or unhelpful
- Edge cases not tested

### 💡 Suggestions (Comment only)
- Code clarity improvements
- Better variable names
- Test coverage edge cases
- Performance optimizations beyond requirements

---

## How to Post Your Review

```markdown
## 🔍 Code Review (Automated)

### ✅ Strengths
- [What's good about this PR]

### ❌ Critical Issues
- **[File.cs:Line]**: [Issue with specific example]

### ⚠️ High Priority
- **[File.cs:Line]**: [Issue description]

### 💡 Suggestions
- [Non-blocking improvement]

### 🎯 Recommendation
[Approve / Request Changes / Comment]
```

**Decision Rules:**
- **Request Changes** if any critical or high priority issues exist
- **Approve** only if all standards met and no critical issues
- **Comment Only** if only suggestions (no blocking)

---

## Quick Reference: What CI Bot Checks vs. What Agent Checks

| Check | Who | What |
|-------|-----|------|
| Build compiles | CI Bot | Does `dotnet build` succeed? |
| Tests pass | CI Bot | Do all tests pass? |
| XML docs **exist** | CI Bot | Are public APIs documented? |
| CHANGELOG updated | CI Bot | Was CHANGELOG.md changed? |
| **XML docs *complete*** | **Agent** | **Are `<summary>`, `<param>`, `<returns>`, `<exception>` all filled out?** |
| **Streaming correct** | **Agent** | **No buffering? Iteration done properly?** |
| **Ordering validation** | **Agent** | **Detects unsorted input? Good error message?** |
| **Patterns followed** | **Agent** | **Consistent with existing operators?** |
| **Multi-target works** | **Agent** | **No breaking framework-specific code?** |
| **Tests thorough** | **Agent** | **Edge cases covered? Streaming verified?** |
| **Comments fresh** | **Agent** | **Updated with code changes? No stale comments?** |

---

## Questions?

- See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution standards
- See [.copilot-instructions.md](.copilot-instructions.md) for agent guidance
- Review existing operators in [src/OrderedLinqOps/](src/OrderedLinqOps/) for patterns
