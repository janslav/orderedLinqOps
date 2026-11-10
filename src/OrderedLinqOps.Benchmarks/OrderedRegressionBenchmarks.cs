using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using OrderedLinqOps;

[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
[SimpleJob(RuntimeMoniker.HostProcess, launchCount: 1, warmupCount: 1, iterationCount: 3)]
[RankColumn]
public class OrderedRegressionBenchmarks
{
    private List<CustomerProfile> customers = null!;
    private List<OrderRow> orders = null!;

    [Params(100_000)]
    public int OrderRowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        (customers, orders) = IntegrationFixtureData.CreateScaled(OrderRowCount);
    }

    [Benchmark]
    public long OrderedGroupBy()
    {
        long checksum = 0;
        foreach (var group in orders.OrderedGroupBy(o => o.CustomerId, o => o.Amount))
        {
            checksum += group.Key;
            foreach (var amount in group)
            {
                checksum += amount;
            }
        }

        return checksum;
    }

    [Benchmark]
    public long OrderedJoin()
    {
        long checksum = 0;
        foreach (var value in orders.OrderedJoin(customers, order => order.CustomerId, customer => customer.CustomerId,
                     (order, customer) => order.Amount + customer.CustomerId))
        {
            checksum += value;
        }

        return checksum;
    }

    [Benchmark]
    public long OrderedGroupJoin()
    {
        long checksum = 0;
        foreach (var value in customers.OrderedGroupJoin(orders, customer => customer.CustomerId, order => order.CustomerId, Summarize))
        {
            checksum += value;
        }

        return checksum;
    }

    private static long Summarize(CustomerProfile customer, IEnumerable<OrderRow> matches)
    {
        long summary = customer.CustomerId;
        foreach (var match in matches)
        {
            summary += match.Amount;
        }

        return summary;
    }

    private sealed class IntegrationFixtureData
    {
        public static (List<CustomerProfile> Customers, List<OrderRow> Orders) CreateScaled(int targetOrderRowCount)
        {
            var basePath = Path.Combine(AppContext.BaseDirectory, "Data");
            var baseCustomers = ParseCustomersCsv(File.ReadAllText(Path.Combine(basePath, "customers.csv")));
            var baseOrders = ParseOrdersCsv(File.ReadAllText(Path.Combine(basePath, "orders.csv")));

            var baseCustomerMaxId = baseCustomers[^1].CustomerId;
            var baseOrderCount = baseOrders.Count;
            var repetitions = (targetOrderRowCount + baseOrderCount - 1) / baseOrderCount;

            var customers = new List<CustomerProfile>(repetitions * baseCustomers.Count);
            var orders = new List<OrderRow>(targetOrderRowCount);

            for (var repetition = 0; repetition < repetitions; repetition++)
            {
                var offset = repetition * baseCustomerMaxId;

                foreach (var customer in baseCustomers)
                {
                    customers.Add(new CustomerProfile(customer.CustomerId + offset, customer.Region));
                }

                foreach (var order in baseOrders)
                {
                    if (orders.Count == targetOrderRowCount)
                    {
                        break;
                    }

                    orders.Add(new OrderRow(order.CustomerId + offset, order.Amount));
                }
            }

            return (customers, orders);
        }

        private static List<CustomerProfile> ParseCustomersCsv(string csv)
        {
            return csv
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line =>
                {
                    var parts = line.Split('\t');
                    return new CustomerProfile(
                        CustomerId: int.Parse(parts[0], CultureInfo.InvariantCulture),
                        Region: parts[1]);
                })
                .ToList();
        }

        private static List<OrderRow> ParseOrdersCsv(string csv)
        {
            return csv
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line =>
                {
                    var parts = line.Split('\t');
                    return new OrderRow(
                        CustomerId: int.Parse(parts[0], CultureInfo.InvariantCulture),
                        Amount: (long)(decimal.Parse(parts[1], CultureInfo.InvariantCulture) * 100m));
                })
                .OrderBy(x => x.CustomerId)
                .ToList();
        }
    }

    private sealed record OrderRow(int CustomerId, long Amount);

    private sealed record CustomerProfile(int CustomerId, string Region);
}
