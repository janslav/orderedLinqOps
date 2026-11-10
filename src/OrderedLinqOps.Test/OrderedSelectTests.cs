using System;
using System.Linq;
using NUnit.Framework;


namespace OrderedLinqOps.Test
{
    [TestFixture]
    public class OrderedSelectTests
    {
        [Test]
        public void OrderedSelect_SimpleHappyPath()
        {
            var input = new[] { 1, 2, 3 };

            var actual = input.OrderedSelect(i => i * i, (i, j) => (j, i));
            var expected = new[] { (1, 1), (2, 4), (3, 9) };

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void OrderedSelect_ThrowsWhenUnordered()
        {
            var input = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => input.OrderedSelect(i => i, (i, j) => j * i).ToList());
        }

        [Test]
        public void OrderedSelect_UnorderedExceptionContainsDiagnostics()
        {
            var input = new[] { 1, 3, 2 };

            var ex = Assert.Throws<ArgumentException>(() => input.OrderedSelect(i => i, (i, j) => j * i).ToList());

            Assert.That(ex!.Message, Does.Contain("At zero-based index 2"));
            Assert.That(ex.Message, Does.Contain("previous key was '3', but current key is '2'"));
            Assert.That(ex.Message, Does.Contain("comparer.Compare(current, previous) = -1"));
            Assert.That(ex.Message, Does.Contain("Comparer:"));
            Assert.That(ex.Message, Does.Contain("Verify the comparer matches the actual data ordering"));
            Assert.That(ex.Message, Does.Contain(".OrderBy(keySelector)"));
        }
    }
}