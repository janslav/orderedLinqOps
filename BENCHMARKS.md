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
| OrderedJoin vs Join | 32.70 μs | 37.60 μs | 47.58 KB | 80.80 KB |
| OrderedGroupJoin vs GroupJoin | 76.15 μs | 43.82 μs | 128.30 KB | 74.55 KB |

### 10,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 202.17 μs | 280.85 μs | 574.02 KB | 726.34 KB |
| OrderedJoin vs Join | 335.62 μs | 275.24 μs | 469.95 KB | 622.54 KB |
| OrderedGroupJoin vs GroupJoin | 606.01 μs | 470.96 μs | 1,058.21 KB | 726.45 KB |

### 100,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 2,242.81 μs | 6,432.96 μs | 5,737.23 KB | 7,645.39 KB |
| OrderedJoin vs Join | 3,401.64 μs | 5,235.86 μs | 4,693.49 KB | 5,815.59 KB |
| OrderedGroupJoin vs GroupJoin | 6,121.45 μs | 7,169.69 μs | 10,357.19 KB | 6,601.44 KB |

### 1,000,000 rows

| Operator pair | Ordered mean | LINQ mean | Ordered alloc | LINQ alloc |
|---|---:|---:|---:|---:|
| OrderedGroupBy vs GroupBy | 23,240.73 μs | 135,557.12 μs | 57,367.20 KB | 74,398.84 KB |
| OrderedJoin vs Join | 35,557.19 μs | 85,808.28 μs | 46,927.76 KB | 64,252.03 KB |
| OrderedGroupJoin vs GroupJoin | 70,903.85 μs | 111,924.23 μs | 116,552.85 KB | 63,958.96 KB |

## Recommendations for real-world usage

- Prefer **OrderedGroupBy** when input is already sorted by group key; this benchmark shows better throughput and lower allocations at all tested scales.
- Prefer **OrderedJoin** for larger sorted datasets (100K+), where it outperforms LINQ join while keeping allocations lower.
- Use **OrderedGroupJoin** when sorted streaming semantics are required; in this implementation it scales well in time at larger sizes but currently allocates more memory than LINQ `GroupJoin`.
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

