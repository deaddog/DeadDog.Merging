using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeadDog.Merging.Tests
{
    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void ContainsSelf()
        {
            var zero = new Range(0, 0);
            var one = new Range(1, 1);
            var zeroTwo = new Range(0, 2);

            Assert.IsTrue(zero.Contains(zero, includeStart: true));
            Assert.IsTrue(one.Contains(one, includeStart: true));
            Assert.IsTrue(zeroTwo.Contains(zeroTwo, includeStart: true));

            Assert.IsFalse(zero.Contains(zero, includeStart: false));
            Assert.IsFalse(one.Contains(one, includeStart: false));
            Assert.IsFalse(zeroTwo.Contains(zeroTwo, includeStart: false));
        }

        [TestMethod]
        public void ContainsUnit()
        {
            var zeroTwo = new Range(0, 2);

            Assert.IsTrue(zeroTwo.Contains(new Range(0, 0), includeStart: true));
            Assert.IsTrue(zeroTwo.Contains(new Range(1, 1), includeStart: true));
            Assert.IsTrue(zeroTwo.Contains(new Range(2, 2), includeStart: true));

            Assert.IsFalse(zeroTwo.Contains(new Range(0, 0), includeStart: false));
            Assert.IsTrue(zeroTwo.Contains(new Range(1, 1), includeStart: false));
            Assert.IsTrue(zeroTwo.Contains(new Range(2, 2), includeStart: false));
        }

        [TestMethod]
        public void OverlapsSelf()
        {
            var zero = new Range(0, 0);
            var one = new Range(1, 1);
            var zeroTwo = new Range(0, 2);

            Assert.IsTrue(zero.OverlapsWith(zero));
            Assert.IsTrue(one.OverlapsWith(one));
            Assert.IsTrue(zeroTwo.OverlapsWith(zeroTwo));
        }

        [TestMethod]
        public void OverlapsUnit()
        {
            var zeroTwo = new Range(0, 2);

            Assert.IsTrue(zeroTwo.OverlapsWith(new Range(0, 0)));
            Assert.IsTrue(zeroTwo.OverlapsWith(new Range(1, 1)));
            Assert.IsFalse(zeroTwo.OverlapsWith(new Range(2, 2)));
        }

        [TestMethod]
        public void JoinSelf()
        {
            var zero = new Range(0, 0);
            var one = new Range(1, 1);
            var zeroTwo = new Range(0, 2);

            Assert.AreEqual(zero, Range.Join(zero, zero));
            Assert.AreEqual(one, Range.Join(one, one));
            Assert.AreEqual(zeroTwo, Range.Join(zeroTwo, zeroTwo));
        }

        [TestMethod]
        public void JoinUnit()
        {
            var zeroTwo = new Range(0, 2);

            Assert.AreEqual(zeroTwo, Range.Join(zeroTwo, new Range(0, 0)));
            Assert.AreEqual(zeroTwo, Range.Join(zeroTwo, new Range(1, 1)));
            Assert.AreEqual(zeroTwo, Range.Join(zeroTwo, new Range(2, 2)));
        }
    }
}
