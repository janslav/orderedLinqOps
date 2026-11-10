# OrderedLinqOps Benchmarks

This document tracks performance measurements of `OrderedGroupBy`, `OrderedJoin`, and `OrderedGroupJoin` against standard LINQ (`GroupBy`, `Join`, `GroupJoin`).

## Methodology

- **Benchmark framework**: [BenchmarkDotNet](https://benchmarkdotnet.org/) (industry standard for .NET microbenchmarks).
- **Execution mode**: Release build, `ShortRunJob`, `MemoryDiagnoser`, single benchmark process.
- **Input data source**: `src/OrderedLinqOps.Test/Data/orders.csv` and `src/OrderedLinqOps.Test/Data/customers.csv`.
- **Scaling strategy**: deterministic repeated multiples of integration-test CSV rows, with key offsets per repetition to keep sequences sorted and comparable.
- **Dataset sizes**: 1K, 10K, 100K, 1M **order rows**.
- **Enumeration model**: all benchmarks consume operators via streaming `foreach` enumeration; no `ToList()` materialization is benchmarked.
- **Fairness**: each ordered operator is compared directly with its LINQ counterpart on identical generated data.

## System specs used

- OS: Ubuntu 24.04.5 LTS (Linux 7.0.0-1017-azure)
- CPU: Intel Xeon Platinum 8573C (4 vCPUs visible)
- Memory: 15 GiB
- .NET SDK: 10.0.403
- Runtime: .NET 10.0.14 x64 RyuJIT

## Results

> Raw BenchmarkDotNet report: `BenchmarkDotNet.Artifacts/results/OrderedOperatorsBenchmarks-report-github.md`

### 1,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 21.02 μs | 25.83 μs | 57.69 KB | 74.45 KB |
| OrderedJoin vs Join | 38.47 μs | 43.40 μs | 47.58 KB | 80.80 KB |
| OrderedGroupJoin vs GroupJoin | 39.23 μs | 50.89 μs | 34.52 KB | 74.55 KB |

### 10,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 246.15 μs | 333.79 μs | 574.02 KB | 726.34 KB |
| OrderedJoin vs Join | 382.49 μs | 333.39 μs | 469.95 KB | 622.54 KB |
| OrderedGroupJoin vs GroupJoin | 356.30 μs | 446.83 μs | 339.43 KB | 622.04 KB |

### 100,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 2,454.05 μs | 6,780.97 μs | 5,737.23 KB | 7,645.37 KB |
| OrderedJoin vs Join | 3,827.03 μs | 6,164.01 μs | 4,693.50 KB | 5,815.59 KB |
| OrderedGroupJoin vs GroupJoin | 3,638.38 μs | 8,311.91 μs | 3,388.41 KB | 6,601.43 KB |

### 1,000,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 26,554.91 μs | 159,858.23 μs | 57,367.20 KB | 74,398.93 KB |
| OrderedJoin vs Join | 40,176.18 μs | 81,311.51 μs | 46,927.77 KB | 64,251.98 KB |
| OrderedGroupJoin vs GroupJoin | 36,481.12 μs | 170,789.25 μs | 33,877.97 KB | 63,958.96 KB |

## Investigation findings: why the gap can look smaller than expected

- `OrderedGroupBy` and `OrderedGroupJoin` still buffer **within each key-group** (they do not buffer the entire input sequence, but they do allocate per-group collections).
- When benchmark data has relatively small groups, hash-based LINQ can remain competitive at smaller scales, reducing the visible gap.
- `OrderedJoin` and `OrderedGroupJoin` are sensitive to implementation details around temporary grouping.
- We found and fixed a key issue in `OrderedGroupJoin`: the previous implementation grouped the **outer** stream first, introducing extra buffering/allocation. The current implementation streams outer elements directly and reuses the current inner grouped segment.

## Recommendations for real-world usage

- Prefer **OrderedGroupBy** when input is already sorted by group key; this benchmark shows better throughput and lower allocations at all tested scales.
- Prefer **OrderedJoin** for larger sorted datasets (100K+), where it typically outperforms LINQ join while keeping allocations lower.
- Prefer **OrderedGroupJoin** for sorted workloads after the current implementation fix; it now shows lower allocations and better scaling than LINQ `GroupJoin` in this benchmark.
- For small datasets (around 1K), differences are often minor; choose readability unless streaming constraints or memory pressure are key requirements.

## Comparing/checking new PRs (industry-standard workflow)

1. **Use BenchmarkDotNet as the source of truth** (already configured in `src/OrderedLinqOps.Benchmarks`).
2. **Run benchmarks on a consistent runner** (same VM class/CPU where possible).
3. **Capture both time and allocation deltas** from:
   - `BenchmarkDotNet.Artifacts/results/OrderedOperatorsBenchmarks-report-github.md`
   - `BenchmarkDotNet.Artifacts/results/OrderedOperatorsBenchmarks-report.csv`
4. **Compare PR vs baseline (master)**:
   - Run benchmarks on `master`, save CSV as baseline artifact.
   - Run benchmarks on PR branch, compare means/allocations per operator and size.
   - Treat regressions at 100K/1M rows as most significant for this library’s target scenarios.
5. **Gate manually or via CI policy**:
   - Common open-source policy: fail/review PRs that regress key benchmarks beyond an agreed threshold (for example >10% time or allocation regression in critical cases).
