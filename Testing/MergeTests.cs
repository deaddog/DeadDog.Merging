using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog.Merging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DeadDog.Merging.Tests
{
    [TestClass()]
    public class MergeTests
    {
        [TestMethod()]
        public void mergeTest1()
        {
            string result = Merger.merge("test", "test", "test");

            Assert.AreEqual("test", result);
        }

        [TestMethod()]
        public void mergeTest2()
        {
            string result = Merger.merge("test", "hest", "test");

            Assert.AreEqual("hest", result);
        }

        [TestMethod()]
        public void mergeTest3()
        {
            string result = Merger.merge("test", "test", "vest");

            Assert.AreEqual("vest", result);
        }

        [TestMethod()]
        public void mergeTest4()
        {
            string result = Merger.merge("test", "hast", "test");

            Assert.AreEqual("hast", result);
        }

        [TestMethod()]
        public void mergeTest5()
        {
            string result = Merger.merge("test", "test", "fast");

            Assert.AreEqual("fast", result);
        }

        [TestMethod()]
        public void mergeTest6()
        {
            string result = Merger.merge("test", "vest", "tast");

            Assert.AreEqual("vast", result);
        }

        [TestMethod()]
        public void mergeTest7()
        {
            string result = Merger.merge("test", "fest", "tester");

            Assert.AreEqual("fester", result);
        }

        [TestMethod()]
        public void mergeTest8()
        {
            string result = Merger.merge("test", "vest", "tes");

            Assert.AreEqual("ves", result);
        }

        [TestMethod()]
        public void mergeTest9()
        {
            string result = Merger.merge("test", "", "test");

            Assert.AreEqual("", result);
        }

        [TestMethod()]
        public void mergeTest10()
        {
            string result = Merger.merge("test", "te", "st");

            Assert.AreEqual("", result);
        }

        [TestMethod()]
        public void mergeTest11()
        {
            string result = Merger.merge("aaaaaaaaaabbbbbbbbbb", "aaaaaaaaaabbbbbbbbbb", "bbbbbbbbbbaaaaaaaaaa");

            Assert.AreEqual("bbbbbbbbbbaaaaaaaaaa", result);
        }

        [TestMethod()]
        public void mergeTest12()
        {
            string result = Merger.merge("aaaaaaaaaabbbbbbbbbb", "aaaaaaaaaabbbbccbbbb", "bbbbbbbbbbaaaaaaaaaa");

            Assert.AreEqual("bbbbccbbbbaaaaaaaaaa", result);
        }
    }
}
