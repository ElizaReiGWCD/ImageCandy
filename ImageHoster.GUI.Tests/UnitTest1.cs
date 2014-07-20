using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ImageHoster.CQRS.Domain;

namespace ImageHoster.GUI.Tests
{
    [TestClass]
    public class Miscellaneous
    {
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void TestSubset()
        {
            List<int> list1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            List<int> list2 = new List<int>() { 1, 2, 3, 4, 5 };

            Assert.IsFalse(list1.IsSubsetOf(list2));
            Assert.IsTrue(list2.IsSubsetOf(list1));
            Assert.IsTrue(list1.IsSubsetOf(list1));
        }
    }
}
