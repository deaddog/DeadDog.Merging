using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeadDog.Merging.Tests
{
    [TestClass()]
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

        [TestMethod()]
        public void mergeTest1()
        {
            AssertMerge
            (
                expect: "test",
                common: "test",
                srcOne: "test",
                srcTwo: "test"
            );
        }

        [TestMethod()]
        public void mergeTest2()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "hest",
                srcTwo: "test",
                expect: "hest"
            );
        }

        [TestMethod()]
        public void mergeTest3()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "test",
                srcTwo: "vest",
                expect: "vest"
            );
        }

        [TestMethod()]
        public void mergeTest4()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "hast",
                srcTwo: "test",
                expect: "hast"
            );
        }

        [TestMethod()]
        public void mergeTest5()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "test",
                srcTwo: "fast",
                expect: "fast"
            );
        }

        [TestMethod()]
        public void mergeTest6()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "vest",
                srcTwo: "tast",
                expect: "vast"
            );
        }

        [TestMethod()]
        public void mergeTest7()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "fest",
                srcTwo: "tester",
                expect: "fester"
            );
        }

        [TestMethod()]
        public void mergeTest8()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "vest",
                srcTwo: "tes",
                expect: "ves"
            );
        }

        [TestMethod()]
        public void mergeTest9()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "",
                srcTwo: "test",
                expect: ""
            );
        }

        [TestMethod()]
        public void mergeTest10()
        {
            AssertMerge
            (
                common: "test",
                srcOne: "te",
                srcTwo: "st",
                expect: ""
            );
        }

        [TestMethod()]
        public void mergeTest11()
        {
            AssertMerge
            (
                common: "aaaaaaaaaabbbbbbbbbb",
                srcOne: "aaaaaaaaaabbbbbbbbbb",
                srcTwo: "bbbbbbbbbbaaaaaaaaaa",
                expect: "bbbbbbbbbbaaaaaaaaaa"
            );
        }

        [TestMethod()]
        public void mergeTest12()
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
