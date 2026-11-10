using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace OrderedLinqOps.Test
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void OrderedLinqOps_ReadsAndProcessesSortedCsvResults()
        {
            // Similar to reading a sorted table from a database and grouping rows by customer key.
            // The input is already sorted by customer_id, so OrderedGroupBy should stream through it in order.
            var rows = LoadOrderRowsFromCsv();

            var summary = rows
                .OrderedGroupBy(r => r.CustomerId, r => r.Amount)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    Total = group.Sum(x => x),
                    OrderCount = group.Count()
                })
                .ToList();

            var expectedSummary = rows
                .GroupBy(r => r.CustomerId)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    Total = group.Sum(r => r.Amount),
                    OrderCount = group.Count()
                })
                .ToList();

            summary.Should().BeEquivalentTo(expectedSummary, options => options.WithStrictOrdering());
            summary.Sum(x => x.Total).Should().BeApproximately(rows.Sum(x => x.Amount), 0.01m);
            summary.Should().HaveCount(250);
        }

        [Test]
        public void OrderedLinqOps_ValidatesCsvInputWithOrderedSelect()
        {
            // Think of this as projecting the rows from a sorted database result set
            // before passing them downstream, while checking that the order never goes backwards.
            var rows = LoadOrderRowsFromCsv();

            var projected = rows
                .OrderedSelect(r => r.CustomerId, (customerId, row) => new
                {
                    CustomerId = customerId,
                    Amount = row.Amount
                })
                .ToList();

            var customerIds = projected.Select(x => x.CustomerId).ToList();
            customerIds.Should().HaveCount(rows.Count);
            customerIds.Should().BeInAscendingOrder();
            projected.First().CustomerId.Should().Be(1);
            projected.Last().CustomerId.Should().Be(250);
        }

        [Test]
        public void OrderedLinqOps_JoinsLargePreSortedDatasets()
        {
            // Similar to joining two sorted database tables on a primary/foreign key.
            var customers = LoadCustomerProfilesFromCsv();
            var orders = LoadOrderRowsFromCsv();

            var actualJoined = orders
                .OrderedJoin(customers,
                    order => order.CustomerId,
                    customer => customer.CustomerId,
                    (order, customer) => new
                    {
                        customer.CustomerId,
                        customer.Region,
                        order.Amount
                    })
                .ToList();

            var expectedJoined = orders
                .Join(customers,
                    order => order.CustomerId,
                    customer => customer.CustomerId,
                    (order, customer) => new
                    {
                        customer.CustomerId,
                        customer.Region,
                        order.Amount
                    })
                .ToList();

            actualJoined.Should().BeEquivalentTo(expectedJoined, options => options.WithStrictOrdering());
            actualJoined.Should().HaveCount(orders.Count);
        }

        [Test]
        public void OrderedLinqOps_GroupJoinsLargePreSortedDatasets()
        {
            // Similar to a LEFT OUTER JOIN in SQL where each customer still appears even without orders.
            var customers = LoadCustomerProfilesFromCsv();
            var orders = LoadOrderRowsFromCsv();

            var actualGrouped = customers
                .OrderedGroupJoin(orders,
                    customer => customer.CustomerId,
                    order => order.CustomerId,
                    (customer, matches) => new
                    {
                        customer.CustomerId,
                        customer.Region,
                        OrderCount = matches.Count,
                        TotalAmount = matches.Sum(x => x.Amount)
                    })
                .ToList();

            var expectedGrouped = customers
                .GroupJoin(orders,
                    customer => customer.CustomerId,
                    order => order.CustomerId,
                    (customer, matches) => new
                    {
                        customer.CustomerId,
                        customer.Region,
                        OrderCount = matches.Count(),
                        TotalAmount = matches.Sum(x => x.Amount)
                    })
                .ToList();

            actualGrouped.Should().BeEquivalentTo(expectedGrouped, options => options.WithStrictOrdering());
            actualGrouped.Should().HaveCount(customers.Count);
            actualGrouped.Sum(x => x.TotalAmount).Should().BeApproximately(orders.Sum(x => x.Amount), 0.01m);
        }

        [Test]
        public void OrderedLinqOps_StreamingPipelineUsesMultipleOperatorsOnLargeSortedDataset()
        {
            // Like a database pipeline: read sorted rows, filter/project them, then aggregate by customer.
            var rows = LoadOrderRowsFromCsv();

            var pipeline = rows
                .OrderedSelect(r => r.CustomerId, (customerId, row) => new
                {
                    CustomerId = customerId,
                    row.Amount
                })
                .OrderedGroupBy(x => x.CustomerId, x => x.Amount)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    TotalAmount = group.Sum(x => x),
                    AverageAmount = group.Average(x => x)
                })
                .ToList();

            var expectedPipeline = rows
                .GroupBy(x => x.CustomerId)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    TotalAmount = group.Sum(x => x.Amount),
                    AverageAmount = group.Average(x => x.Amount)
                })
                .ToList();

            pipeline.Should().BeEquivalentTo(expectedPipeline, options => options.WithStrictOrdering());
            pipeline.Should().HaveCount(250);
            pipeline.Sum(x => x.TotalAmount).Should().BeApproximately(rows.Sum(x => x.Amount), 0.01m);
            pipeline.First().CustomerId.Should().Be(1);
            pipeline.Last().CustomerId.Should().Be(250);
        }

        private static List<OrderRow> LoadOrderRowsFromCsv()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "orders.csv");
            return ParseOrdersCsv(File.ReadAllText(path));
        }

        private static List<CustomerProfile> LoadCustomerProfilesFromCsv()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "customers.csv");
            return ParseCustomersCsv(File.ReadAllText(path));
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
                        Amount: decimal.Parse(parts[1], CultureInfo.InvariantCulture));
                })
                .OrderBy(x => x.CustomerId)
                .ToList();
        }

        private sealed record OrderRow(int CustomerId, decimal Amount);

        private sealed record CustomerProfile(int CustomerId, string Region);
    }
}
