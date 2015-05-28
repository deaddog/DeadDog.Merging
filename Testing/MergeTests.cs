﻿using System;
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
            string result = Merge.merge("test", "test", "test");

            Assert.AreEqual("test", result);
        }

        [TestMethod()]
        public void mergeTest2()
        {
            string result = Merge.merge("test", "hest", "test");

            Assert.AreEqual("hest", result);
        }

        [TestMethod()]
        public void mergeTest3()
        {
            string result = Merge.merge("test", "test", "vest");

            Assert.AreEqual("vest", result);
        }

        [TestMethod()]
        public void mergeTest4()
        {
            string result = Merge.merge("test", "hast", "test");

            Assert.AreEqual("hast", result);
        }

        [TestMethod()]
        public void mergeTest5()
        {
            string result = Merge.merge("test", "test", "fast");

            Assert.AreEqual("fast", result);
        }

        [TestMethod()]
        public void mergeTest6()
        {
            string result = Merge.merge("test", "vest", "tast");

            Assert.AreEqual("vast", result);
        }

        [TestMethod()]
        public void mergeTest7()
        {
            string result = Merge.merge("test", "fest", "tester");

            Assert.AreEqual("fester", result);
        }

        [TestMethod()]
        public void mergeTest8()
        {
            string result = Merge.merge("test", "vest", "tes");

            Assert.AreEqual("ves", result);
        }
    }
}
