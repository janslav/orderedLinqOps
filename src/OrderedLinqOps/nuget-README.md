# OrderedLinqOps

Lightweight, streaming alternatives for some common LINQ operations (GroupBy, Join, GroupJoin) that rely on ordered inputs rather than hashing. This package provides "ordered" variants that process sequences in a single pass and yield results as the inputs are read.

This README is intended for consumers of the NuGet package. For full development, contribution, CI and testing information see the project repository.

## Package

Install (Package Manager Console)
```powershell
Install-Package OrderedLinqOps
```

Install (dotnet CLI)
```bash
dotnet add package OrderedLinqOps
```

Package ID: OrderedLinqOps  
Supported Target Frameworks: netstandard2.0, netstandard2.1, net6.0, net8.0

## Quick summary

- OrderedSelect: project while verifying the input is non-decreasing by a key
- OrderedGroupBy: streaming grouping assuming the input is ordered by key
- OrderedJoin / OrderedGroupJoin: streaming joins that assume both inputs are ordered by key

These methods are intended for scenarios where inputs are naturally ordered and you want a low-memory, streaming approach instead of hash-based grouping/joining.

## OrderedGroupBy example

```csharp
var pets = new[] // Input must be ordered by Age
{
    new { Name = "Whiskers", Age = 1 },
    new { Name = "Boots", Age = 4 },
    new { Name = "Daisy", Age = 4 },
    new { Name = "Barley", Age = 8 }
};

var grouped = pets.OrderedGroupBy(p => p.Age, p => p.Name);

// yields:
// 1 -> ["Whiskers"]
// 4 -> ["Boots", "Daisy"]
// 8 -> ["Barley"]
```

## Correctness & exceptions

- Precondition: Inputs must be ordered according to the provided key selector and comparer. If ordering is not satisfied, methods throw ArgumentException during enumeration.
- Null checking: Public API performs null checks and throws ArgumentNullException for null arguments where appropriate.

## Performance considerations

- These methods stream through inputs with minimal buffering. Use them when inputs are large and already ordered (sorted) — they avoid building full hash tables.
- If inputs are not ordered, do not use these methods (either sort first or use standard LINQ methods like GroupBy/Join which are hash-based or require buffering).

## Licensing

Licensed under LGPL-3.0-or-later. See the package or project LICENSE file for details.

## Support & reporting issues

If you find bugs, documentation errors, or want to request features, please open an issue in the project repository:

https://github.com/janslav/orderedLinqOps/issues

Include a short repro case when possible.

## Release notes & versioning

The package follows semantic versioning. Release artifacts and full changelog are published with each NuGet release — see the package release notes on NuGet or the repository releases.

## Contact / Contribution

For contribution, tests, CI, and development instructions, refer to the repository (this package README is intentionally concise for NuGet consumers). The repository contains development-oriented documentation including CI workflows, test instructions, and contribution guidelines.

Repository: https://github.com/janslav/orderedLinqOps

Thank you for using OrderedLinqOps!