using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace OrderedLinqOps.Test
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void OrderedLinqOps_ReadsAndProcessesSortedCsvResults()
        {
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
                .OrderBy(x => x.CustomerId)
                .ToList();

            CollectionAssert.AreEqual(expectedSummary.Select(x => x.CustomerId), summary.Select(x => x.CustomerId));
            Assert.That(summary.Sum(x => x.Total), Is.EqualTo(rows.Sum(x => x.Amount)).Within(0.01m));
            Assert.That(summary.Count, Is.EqualTo(250));
        }

        [Test]
        public void OrderedLinqOps_ValidatesCsvInputWithOrderedSelect()
        {
            var rows = LoadOrderRowsFromCsv();

            var projected = rows
                .OrderedSelect(r => r.CustomerId, (customerId, row) => new
                {
                    customerId,
                    row.Region,
                    row.Amount,
                    row.Label
                })
                .ToList();

            var customerIds = projected.Select(x => x.customerId).ToList();
            Assert.That(customerIds.Count, Is.EqualTo(rows.Count));
            Assert.That(customerIds.Zip(customerIds.Skip(1), (previous, current) => previous <= current).All(x => x), Is.True);
            Assert.That(projected.First().customerId, Is.EqualTo(1));
            Assert.That(projected.Last().customerId, Is.EqualTo(250));
        }

        [Test]
        public void OrderedLinqOps_JoinsLargePreSortedDatasets()
        {
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
                        order.OrderId,
                        order.Amount,
                        order.Label
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
                        order.OrderId,
                        order.Amount,
                        order.Label
                    })
                .ToList();

            CollectionAssert.AreEquivalent(expectedJoined, actualJoined);
            Assert.That(actualJoined.Count, Is.EqualTo(orders.Count));
        }

        [Test]
        public void OrderedLinqOps_GroupJoinsLargePreSortedDatasets()
        {
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

            CollectionAssert.AreEquivalent(expectedGrouped, actualGrouped);
            Assert.That(actualGrouped.Count, Is.EqualTo(customers.Count));
            Assert.That(actualGrouped.Sum(x => x.TotalAmount), Is.EqualTo(orders.Sum(x => x.Amount)).Within(0.01m));
        }

        [Test]
        public void OrderedLinqOps_StreamingPipelineUsesMultipleOperatorsOnLargeSortedDataset()
        {
            var rows = LoadOrderRowsFromCsv();

            var pipeline = rows
                .OrderedSelect(r => r.CustomerId, (customerId, row) => new
                {
                    CustomerId = customerId,
                    row.Region,
                    row.Label,
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
                .Select(r => new { CustomerId = r.CustomerId, Region = r.Region, Label = r.Label, Amount = r.Amount })
                .GroupBy(x => x.CustomerId)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    TotalAmount = group.Sum(x => x.Amount),
                    AverageAmount = group.Average(x => x.Amount)
                })
                .OrderBy(x => x.CustomerId)
                .ToList();

            Assert.That(pipeline.Count, Is.EqualTo(250));
            Assert.That(pipeline.Sum(x => x.TotalAmount), Is.EqualTo(rows.Sum(x => x.Amount)).Within(0.01m));
            Assert.That(pipeline[0].CustomerId, Is.EqualTo(1));
            Assert.That(pipeline[^1].CustomerId, Is.EqualTo(250));
            CollectionAssert.AreEquivalent(expectedPipeline.Select(x => x.CustomerId), pipeline.Select(x => x.CustomerId));
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
                        Region: parts[1],
                        Segment: parts[2],
                        Balance: decimal.Parse(parts[3], CultureInfo.InvariantCulture));
                })
                .OrderBy(x => x.CustomerId)
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
                        OrderId: int.Parse(parts[1], CultureInfo.InvariantCulture),
                        Region: parts[2],
                        Amount: decimal.Parse(parts[3], CultureInfo.InvariantCulture),
                        Label: parts[4]);
                })
                .OrderBy(x => x.CustomerId)
                .ThenBy(x => x.OrderId)
                .ToList();
        }

        private sealed record OrderRow(int CustomerId, int OrderId, string Region, decimal Amount, string Label);

        private sealed record CustomerProfile(int CustomerId, string Region, string Segment, decimal Balance);
    }
}
