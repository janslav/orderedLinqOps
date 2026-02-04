# OrderedLinqOps

[![CI](https://github.com/janslav/orderedLinqOps/actions/workflows/ci.yml/badge.svg)](https://github.com/janslav/orderedLinqOps/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/janslav/orderedLinqOps/branch/master/graph/badge.svg)](https://codecov.io/gh/janslav/orderedLinqOps)
[![NuGet](https://img.shields.io/nuget/v/OrderedLinqOps.svg)](https://www.nuget.org/packages/OrderedLinqOps/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/OrderedLinqOps.svg)](https://www.nuget.org/packages/OrderedLinqOps/)
[![License](https://img.shields.io/badge/license-LGPL--3.0-blue.svg)](LICENSE.txt)

Alternatives to some hashing-based LINQ operators (`GroupBy`, `Join`, `GroupJoin`), based on ordered inputs. Available as a NuGet package.

**Key Benefits:**
- **Memory Efficient**: Operators don't buffer data in memory, making them ideal for large datasets
- **Performance**: Faster than hash-based LINQ operators when data is already sorted
- **Stream-Friendly**: Process data as it arrives without waiting for the entire collection

**Important Limitation:**
- Input collections **must be pre-sorted** by the key. If not sorted, an exception is thrown during iteration.

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package OrderedLinqOps
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package OrderedLinqOps
```

Or add directly to your `.csproj` file:

```xml
<PackageReference Include="OrderedLinqOps" Version="*" />
```

**Supported Frameworks:**
- .NET Standard 2.0
- .NET Standard 2.1
- .NET 6.0
- .NET 8.0

## Usage

### OrderedGroupBy

The example below is inspired by the one at LINQ's `GroupBy`, with one crucial difference: the input collection **must be pre-sorted** by the grouping key (Age). If not sorted, an exception is thrown during iteration.

```C#
using OrderedLinqOps;

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

### Available Operators

All operators require pre-sorted input:

- **`OrderedGroupBy`**: Groups elements by a key, similar to LINQ's `GroupBy` but requires sorted input
- **`OrderedJoin`**: Joins two sorted sequences, similar to LINQ's `Join`
- **`OrderedGroupJoin`**: Performs a grouped join on two sorted sequences, similar to LINQ's `GroupJoin`

Each operator has multiple overloads to support different scenarios and custom comparers.

## Motivation

LINQ operators based on hashing and "true-false equality" are good enough for most purposes. 

Sometimes, however, you are working with big data that you can't or won't buffer all in memory, but you can source it in sorted order. For example, when data comes from a SQL database with an `ORDER BY` clause. You can then group/join the data as it streams through, without overflowing memory. All you need is to define an "ordered equality" comparer that matches the SQL ordering rules.

Additionally, when you have pre-ordered data and an ordering comparer, it makes sense to leverage that for better performance. Without needing to build hash tables or perform hash lookups, order-based processing is:
- **Faster** than normal LINQ's hash-based operators
- **More memory-efficient** for large datasets
- **Stream-compatible** for processing data pipelines

### Use Cases

Perfect for scenarios such as:
- Processing large datasets from databases with `ORDER BY` clauses
- Streaming data that arrives in sorted order
- ETL (Extract, Transform, Load) operations on sorted files
- Any case where you already have sorted data and want optimal performance

## Building and Testing

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- .NET 6.0 runtime (for running tests on .NET 6.0 target)

### Build
```bash
dotnet restore
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Generate Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports are generated in the `coverage` directory in Cobertura XML format.

## Continuous Integration

This project uses GitHub Actions for CI/CD:

### CI Workflow
- **Trigger**: Pushes to `master` and all pull requests
- **Actions**:
  - Builds the project for all target frameworks
  - Runs all tests with code coverage
  - Uploads coverage to Codecov
  - Generates coverage summary in Actions UI
  - Uploads test results and NuGet packages as artifacts
- **Status**: [![CI](https://github.com/janslav/orderedLinqOps/actions/workflows/ci.yml/badge.svg)](https://github.com/janslav/orderedLinqOps/actions/workflows/ci.yml)

### Publish Workflow
- **Trigger**: Git tags matching `v*.*.*` (e.g., `v1.2.0`)
- **Actions**:
  - Builds and tests the project
  - Packs NuGet package with version from tag
  - Publishes to NuGet.org using `NUGET_API_KEY` secret
  - Uploads package as artifact

## Release Process (for Maintainers)

To publish a new version to NuGet.org:

1. **Update Version**: Edit `src/OrderedLinqOps/OrderedLinqOps.csproj` and update the `<Version>` element and `<PackageReleaseNotes>`

2. **Commit Changes**:
   ```bash
   git add src/OrderedLinqOps/OrderedLinqOps.csproj
   git commit -m "Bump version to X.Y.Z"
   git push origin master
   ```

3. **Create and Push Tag**:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
   
   Example for version 1.3.0:
   ```bash
   git tag v1.3.0
   git push origin v1.3.0
   ```

4. **Automated Publishing**: The GitHub Actions workflow will automatically:
   - Build and test the project
   - Pack the NuGet package with the version from the tag
   - Publish to NuGet.org (requires `NUGET_API_KEY` repository secret)
   - Upload the package as a workflow artifact

5. **Verify**: Check the [Actions tab](https://github.com/janslav/orderedLinqOps/actions) to ensure the publish workflow completes successfully

**Note**: The `NUGET_API_KEY` secret must be configured in the repository settings with a valid NuGet.org API key.

## See Also

- Stephen Cleary's [Comparers](https://github.com/StephenCleary/Comparers) library for easy declarative creation of both hashing and sorting comparers

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under LGPL 3.0 - see the [LICENSE.txt](LICENSE.txt) file for details

