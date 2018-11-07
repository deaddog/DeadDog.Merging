using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeadDog.Merging.Tests
{
    [TestClass]
    public class MergeTests
    {
        private static void AssertMerge(string common, string srcOne, string srcTwo, string expect)
        {
            var merged = Merger.merge
            (
                ancestor: common,
                a: srcOne,
                b: srcTwo
            );

            Assert.AreEqual(expect, merged);
        }

        [TestMethod]
        public void Identity()
        {
            AssertMerge
            (
                expect: "test",
                common: "test",
                srcOne: "test",
                srcTwo: "test"
            );
        }

        [TestMethod]
        public void Source1Change()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "hest",
                srcTwo: "test",
                expect: "hest"
            );
        }

        [TestMethod]
        public void Source2Change()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "test",
                srcTwo: "vest",
                expect: "vest"
            );
        }

        [TestMethod]
        public void Source1MultiChange()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "hast",
                srcTwo: "test",
                expect: "hast"
            );
        }

        [TestMethod]
        public void Source2MultiChange()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "test",
                srcTwo: "fast",
                expect: "fast"
            );
        }

        [TestMethod]
        public void ChangeInBothSources()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "vest",
                srcTwo: "tast",
                expect: "vast"
            );
        }

        [TestMethod]
        public void ChangeWithElongation()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "fest",
                srcTwo: "tester",
                expect: "fester"
            );
        }

        [TestMethod]
        public void ChangeWithShortening()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "vest",
                srcTwo: "tes",
                expect: "ves"
            );
        }

        [TestMethod]
        public void OneSourceCompleteDeletion()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "",
                srcTwo: "test",
                expect: ""
            );
        }

        [TestMethod]
        public void TwoSourcesDistinctDeletion()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "te",
                srcTwo: "st",
                expect: ""
            );
        }

        [TestMethod]
        public void SingleSourceMove()
        {
            AssertMerge
            (
                common: "aaaaaaaaaabbbbbbbbbb",
                srcOne: "aaaaaaaaaabbbbbbbbbb",
                srcTwo: "bbbbbbbbbbaaaaaaaaaa",
                expect: "bbbbbbbbbbaaaaaaaaaa"
            );
        }

        [TestMethod]
        public void OneMoveOneChange()
        {
            AssertMerge
            (
                common: "aaaaaaaaaabbbbbbbbbb",
                srcOne: "aaaaaaaaaabbbbccbbbb",
                srcTwo: "bbbbbbbbbbaaaaaaaaaa",
                expect: "bbbbccbbbbaaaaaaaaaa"
            );
        }
    }
}
