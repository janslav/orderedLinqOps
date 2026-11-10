# Contributing to OrderedLinqOps

Thank you for contributing to OrderedLinqOps! This document outlines the quality standards and processes we follow to maintain a high-quality, reliable LINQ operator library.

## Core Principles

OrderedLinqOps is a performance-critical streaming library. All contributions should uphold these principles:

1. **Memory Efficiency**: Operators must NOT buffer data in memory—they should stream results as they are produced
2. **Performance**: Leverage sorted input for speed; benchmark significant changes
3. **Reliability**: Maintain backward compatibility; changes to public APIs require careful consideration
4. **Clarity**: Code, tests, and documentation must be clear and maintainable
5. **Multi-Target Support**: All code must work across .NET Standard 2.0, 2.1, .NET 6.0, and .NET 8.0

## Quality Standards

### 1. XML Documentation

**All public methods and types MUST have complete XML documentation.**

Format:
```csharp
/// <summary>
/// Brief one-line description of what this does.
/// </summary>
/// <remarks>
/// Longer explanation if needed. For OrderedLinqOps operators, note:
/// - Whether input ordering is validated
/// - Whether the operation buffers data
/// - Any exceptions thrown and when
/// </remarks>
/// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
/// <param name="source">The source sequence.</param>
/// <param name="keySelector">A function to extract the key for each element.</param>
/// <param name="comparer">A comparer to determine ordering (optional).</param>
/// <returns>An enumerable of the results.</returns>
/// <exception cref="ArgumentNullException">Thrown when source, keySelector, or other required parameters are null.</exception>
/// <exception cref="ArgumentException">Thrown if the input sequence is not properly ordered.</exception>
public static IEnumerable<TResult> OrderedGroupBy<TSource, TKey, TResult>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    Func<TKey, IReadOnlyCollection<TSource>, TResult> resultSelector,
    IComparer<TKey>? comparer = null)
```

**Requirements:**
- Every public method and type must have `<summary>`
- Document all `<typeparam>`, `<param>`, and `<returns>`
- Document all possible `<exception>` types thrown
- Include `<remarks>` for important behavior notes (buffering, streaming, ordering validation)

### 2. Inline Comments

Inline comments should explain **why** decisions were made, not **what** the code does (the code itself should be clear enough).

**Good comment:**
```csharp
// Compare keys using the provided comparer; if out of order, throw immediately
// rather than deferring to detection during iteration
var comparisonResult = comparer.Compare(key, previousKey);
```

**Bad comment:**
```csharp
// Compare the keys
var comparisonResult = comparer.Compare(key, previousKey);
```

**Guidelines:**
- Only comment non-obvious logic
- Link to GitHub issues when explaining complex decisions
- Use TODO/FIXME markers sparingly and ensure they're addressed or explained
- **Never leave comments stale**—if you change code that comments reference, update the comments
- Delete comments that no longer apply

### 3. Tests

**Every code change that affects logic must include tests.**

Requirements:
- New operators must have comprehensive test coverage
- Bug fixes should include a test that reproduces the issue
- Test coverage should remain ≥ 85%
- Tests must verify all target frameworks work correctly
- Use NUnit for all tests

Example test structure:
```csharp
[TestFixture]
public class OrderedGroupByTests
{
    [Test]
    public void OrderedGroupBy_WithOrderedInput_GroupsCorrectly()
    {
        var input = new[] { (key: 1, value: "a"), (key: 1, value: "b"), (key: 2, value: "c") };
        var result = input.OrderedGroupBy(x => x.key);
        
        Assert.AreEqual(2, result.Count());
        // ... more assertions
    }

    [Test]
    public void OrderedGroupBy_WithUnorderedInput_ThrowsException()
    {
        var input = new[] { (key: 1, value: "a"), (key: 3, value: "b"), (key: 2, value: "c") };
        
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await input.OrderedGroupBy(x => x.key).ToListAsync());
        Assert.That(ex.Message, Contains.Substring("not ordered"));
    }
}
```

### 4. Changelog

**Every code change that affects users must have a CHANGELOG entry.**

- Edit `CHANGELOG.md` in the "Unreleased" section
- Use format: `- **Category**: Brief description`
- Categories: `Added`, `Changed`, `Fixed`, `Deprecated`, `Removed`, `Security`

Example:
```markdown
## [Unreleased]

### Added
- `OrderedDistinct` operator for removing consecutive duplicates in sorted sequences

### Changed
- Improved error messages for out-of-order violations to include actual key values
- Performance optimization in `OrderedJoin` for large datasets

### Fixed
- Bug where `OrderedGroupJoin` incorrectly handled empty inner sequences
```

### 5. Code Organization

**Follow existing patterns:**
- Operator implementations are extension methods in `OrderedLinqOperators` class
- Each operator may have multiple overloads supporting different scenarios
- Keep related overloads in the same file
- Use `IComparer<TKey>?` pattern for optional comparers (default to `Comparer<TKey>.Default`)
- Throw `ArgumentException` with descriptive message if input is not sorted

### 6. Performance Considerations

For OrderedLinqOps, performance matters. When making changes:

- **Do NOT buffer data unnecessarily** - streaming is the whole point
- Test with realistic dataset sizes (1K, 10K, 100K, 1M+ rows)
- Profile memory usage and allocation counts
- Document any performance implications in PR description
- For significant changes, run benchmarks (see #13)

## Pull Request Process

### Before Creating a PR

1. **Create a branch** from `master`:
   ```bash
   git checkout -b feat/my-feature
   ```

2. **Make your changes** following the quality standards above

3. **Run tests locally**:
   ```bash
   dotnet test
   ```

4. **Run code quality checks**:
   - Verify no XML doc warnings: `dotnet build -p:TreatWarningsAsErrors=true`
   - Check formatting is consistent
   - Ensure CHANGELOG.md is updated

### Submitting a PR

1. **Push your branch** and create a PR
2. **Fill out the PR template** completely:
   - Explain *why* the change is needed
   - Describe *what* changed
   - Detail *how* it was tested
   - Note any breaking changes
3. **Check the PR checklist**:
   - [ ] CHANGELOG.md updated
   - [ ] Tests added/updated
   - [ ] Public APIs have XML docs
   - [ ] Comments reviewed and accurate
   - [ ] Multi-target compatibility verified

### Automated Review

Your PR will be reviewed by:

1. **Mechanical Quality Checks** (GitHub Actions):
   - CHANGELOG.md presence
   - XML documentation completeness
   - Test presence for code changes
   - File naming and organization
   - Posts findings automatically

2. **Automated Code Review** (Copilot Agent):
   - Code quality and design
   - Performance implications
   - Multi-target compatibility
   - Backward compatibility
   - Comment accuracy
   - Operator pattern compliance
   - Posts review comments

3. **Human Review**:
   - Final approval from maintainers
   - Can trust automated checks caught most issues

All checks must pass before merging.

## Development Setup

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- .NET 6.0 runtime (for testing multi-target)

### Build
```bash
dotnet restore
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Check Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports generate in the `coverage` directory in Cobertura XML format.

## Commit Message Guidelines

Use Conventional Commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `test`, `refactor`, `perf`, `chore`, `ci`

**Scope**: The component affected (e.g., `OrderedGroupBy`, `OrderedJoin`, `docs`, `ci`)

**Example**:
```
feat(OrderedGroupBy): add support for custom equality comparison

Previously, OrderedGroupBy only used IComparer<TKey>. This change
adds support for IEqualityComparer for grouping logic, improving
flexibility in scenarios with complex key types.

Closes #42
```

## Coding Style

- Use C# 11+ features (latest language version)
- Follow Microsoft naming conventions
- Use `var` for obvious types, explicit types for clarity
- Use LINQ and extension methods where appropriate
- Prefer immutability where possible
- Keep methods focused and reasonable in size

Example:
```csharp
// Good: clear, uses modern C#
public static IEnumerable<IGrouping<TKey, TElement>> OrderedGroupBy<TSource, TKey, TElement>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    Func<TSource, TElement> elementSelector,
    IComparer<TKey>? comparer = null) =>
    OrderedGroupByInternal(source, keySelector, elementSelector, comparer);
```

## Licensing

By contributing, you agree that your contributions will be licensed under LGPL-3.0 (see LICENSE.txt).

## Questions?

- Open a GitHub issue for questions or suggestions
- Check existing issues and discussions first
- Be respectful and constructive

Thank you for making OrderedLinqOps better! 🚀
