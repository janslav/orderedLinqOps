using System;
using System.Collections.Generic;
using System.Globalization;
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
            var csv = BuildOrdersCsv(customerCount: 250, ordersPerCustomer: 10);
            var rows = ParseOrdersCsv(csv);

            var summary = rows
                .OrderedGroupBy(r => r.CustomerId, r => r.Amount)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    Total = group.Sum(x => x),
                    OrderCount = group.Count()
                })
                .ToList();

            Assert.That(summary.Count, Is.EqualTo(250));
            Assert.That(summary.Sum(x => x.Total), Is.EqualTo(rows.Sum(x => x.Amount)).Within(0.01m));
            Assert.That(summary.First().CustomerId, Is.EqualTo(1));
            Assert.That(summary.Last().CustomerId, Is.EqualTo(250));
        }

        [Test]
        public void OrderedLinqOps_ValidatesCsvInputWithOrderedSelect()
        {
            var rows = BuildOrders(customerCount: 250, ordersPerCustomer: 10);

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
            var customers = BuildCustomerProfiles(customerCount: 250);
            var orders = BuildOrders(customerCount: 250, ordersPerCustomer: 10);

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
            var customers = BuildCustomerProfiles(customerCount: 250);
            var orders = BuildOrders(customerCount: 250, ordersPerCustomer: 10);

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
            var rows = BuildOrders(customerCount: 500, ordersPerCustomer: 12);

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

            Assert.That(pipeline.Count, Is.EqualTo(500));
            Assert.That(pipeline.Sum(x => x.TotalAmount), Is.EqualTo(rows.Sum(x => x.Amount)).Within(0.01m));
            Assert.That(pipeline[0].CustomerId, Is.EqualTo(1));
            Assert.That(pipeline[^1].CustomerId, Is.EqualTo(500));
        }

        private static List<OrderRow> BuildOrders(int customerCount, int ordersPerCustomer)
        {
            var regions = new[] { "North", "South", "East", "West" };
            var labels = new[] { "new", "repeat", "vip", "standard" };

            var rows = Enumerable.Range(1, customerCount)
                .SelectMany(customerId => Enumerable.Range(1, ordersPerCustomer)
                    .Select(orderIndex => new OrderRow(
                        CustomerId: customerId,
                        OrderId: ((customerId - 1) * ordersPerCustomer) + orderIndex,
                        Region: regions[(customerId + orderIndex) % regions.Length],
                        Amount: (customerId * 25m) + (orderIndex * 4.25m) + (customerId % 7),
                        Label: $"{labels[(customerId + orderIndex) % labels.Length]}-{customerId}-{orderIndex}")))
                .OrderBy(x => x.CustomerId)
                .ThenBy(x => x.OrderId)
                .ToList();

            return rows;
        }

        private static IReadOnlyList<CustomerProfile> BuildCustomerProfiles(int customerCount)
        {
            var regions = new[] { "North", "South", "East", "West" };
            var segments = new[] { "retail", "enterprise", "partner", "priority" };

            return Enumerable.Range(1, customerCount)
                .Select(customerId => new CustomerProfile(
                    CustomerId: customerId,
                    Region: regions[(customerId - 1) % regions.Length],
                    Segment: segments[(customerId - 1) % segments.Length],
                    Balance: customerId * 1000m + (customerId % 17) * 25m))
                .OrderBy(x => x.CustomerId)
                .ToList();
        }

        private static string BuildOrdersCsv(int customerCount, int ordersPerCustomer)
        {
            var builder = new StringBuilder();
            builder.AppendLine("customer_id\torder_id\tregion\tamount\tlabel");

            foreach (var row in BuildOrders(customerCount, ordersPerCustomer))
            {
                builder.AppendLine(string.Join("\t",
                    row.CustomerId,
                    row.OrderId,
                    row.Region,
                    row.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                    row.Label));
            }

            return builder.ToString();
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
