#!/usr/bin/env python3
import csv
import math
import re
import sys
from pathlib import Path


def parse_duration_to_us(value: str) -> float:
    match = re.match(r"^\s*([0-9][0-9,]*(?:\.[0-9]+)?)\s*(ns|μs|ms|s)\s*$", value)
    if not match:
        raise ValueError(f"Unsupported duration format: {value}")

    number = float(match.group(1).replace(",", ""))
    unit = match.group(2)
    if unit == "ns":
        return number / 1000.0
    if unit == "μs":
        return number
    if unit == "ms":
        return number * 1000.0
    if unit == "s":
        return number * 1_000_000.0
    raise ValueError(f"Unsupported duration unit: {unit}")


def parse_size_to_kb(value: str) -> float:
    match = re.match(r"^\s*([0-9][0-9,]*(?:\.[0-9]+)?)\s*(B|KB|MB|GB)\s*$", value)
    if not match:
        raise ValueError(f"Unsupported allocation format: {value}")

    number = float(match.group(1).replace(",", ""))
    unit = match.group(2)
    if unit == "B":
        return number / 1024.0
    if unit == "KB":
        return number
    if unit == "MB":
        return number * 1024.0
    if unit == "GB":
        return number * 1024.0 * 1024.0
    raise ValueError(f"Unsupported allocation unit: {unit}")


def load_ordered_rows(path: Path) -> dict[tuple[str, int], dict[str, float]]:
    rows: dict[tuple[str, int], dict[str, float]] = {}
    with path.open(newline="", encoding="utf-8") as file:
        for row in csv.DictReader(file):
            method = row["Method"]
            if not method.startswith("Ordered"):
                continue

            size = int(row["OrderRowCount"])
            rows[(method, size)] = {
                "mean_us": parse_duration_to_us(row["Mean"]),
                "allocated_kb": parse_size_to_kb(row["Allocated"]),
            }
    return rows


def main() -> int:
    if len(sys.argv) != 6:
        print("Usage: compare_ordered_benchmarks.py <baseline.csv> <candidate.csv> <time_threshold_pct> <alloc_threshold_pct> <summary.md>")
        return 2

    baseline_path = Path(sys.argv[1])
    candidate_path = Path(sys.argv[2])
    time_threshold_pct = float(sys.argv[3])
    alloc_threshold_pct = float(sys.argv[4])
    summary_path = Path(sys.argv[5])

    baseline = load_ordered_rows(baseline_path)
    candidate = load_ordered_rows(candidate_path)

    missing = sorted(set(baseline.keys()) ^ set(candidate.keys()))
    if missing:
        with summary_path.open("w", encoding="utf-8") as file:
            file.write("## Ordered benchmark regression check\n\n")
            file.write("❌ Could not compare benchmark results because method/size keys differ between baseline and candidate.\n")
        print("Mismatch in benchmark keys:", missing)
        return 1

    keys = sorted(baseline.keys(), key=lambda x: (x[0], x[1]))
    regressions: list[str] = []

    lines = [
        "## Ordered benchmark regression check",
        "",
        f"Thresholds: time > {time_threshold_pct:.1f}% regression, allocation > {alloc_threshold_pct:.1f}% regression",
        "",
        "| Method | Rows | Baseline Mean (μs) | PR Mean (μs) | Δ Time | Baseline Alloc (KB) | PR Alloc (KB) | Δ Alloc | Status |",
        "|---|---:|---:|---:|---:|---:|---:|---:|---|",
    ]

    for method, size in keys:
        base = baseline[(method, size)]
        cand = candidate[(method, size)]

        base_time = base["mean_us"]
        cand_time = cand["mean_us"]
        base_alloc = base["allocated_kb"]
        cand_alloc = cand["allocated_kb"]

        time_delta_pct = 0.0 if math.isclose(base_time, 0.0) else ((cand_time - base_time) / base_time) * 100.0
        alloc_delta_pct = 0.0 if math.isclose(base_alloc, 0.0) else ((cand_alloc - base_alloc) / base_alloc) * 100.0

        time_regressed = time_delta_pct > time_threshold_pct
        alloc_regressed = alloc_delta_pct > alloc_threshold_pct
        status = "✅"
        if time_regressed or alloc_regressed:
            status = "❌"
            regressions.append(
                f"{method} ({size} rows): time {time_delta_pct:+.2f}%, alloc {alloc_delta_pct:+.2f}%"
            )

        lines.append(
            f"| {method} | {size} | {base_time:.2f} | {cand_time:.2f} | {time_delta_pct:+.2f}% | "
            f"{base_alloc:.2f} | {cand_alloc:.2f} | {alloc_delta_pct:+.2f}% | {status} |"
        )

    lines.append("")
    if regressions:
        lines.append("❌ Regression threshold exceeded:")
        for item in regressions:
            lines.append(f"- {item}")
    else:
        lines.append("✅ No regression threshold exceeded for Ordered operators.")

    summary_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print("\n".join(lines))
    return 1 if regressions else 0


if __name__ == "__main__":
    raise SystemExit(main())
