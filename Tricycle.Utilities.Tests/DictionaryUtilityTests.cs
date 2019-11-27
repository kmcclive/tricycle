using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tricycle.Utilities.Tests
{
    [TestClass]
    public class DictionaryUtilityTests
    {
        [TestMethod]
        public void TestGetValueOrDefault()
        {
            IDictionary<string, int> dictionary = null;

            Assert.ThrowsException<ArgumentNullException>(() => dictionary.GetValueOrDefault(string.Empty));

            string key = "two";
            int value = 2;

            dictionary = new Dictionary<string, int>()
            {
                { "one", 1 },
                { key, value },
                { "three", 3 }
            };

            Assert.ThrowsException<ArgumentNullException>(() => dictionary.GetValueOrDefault(null));

            Assert.AreEqual(value, dictionary.GetValueOrDefault(key));
            Assert.AreEqual(default, dictionary.GetValueOrDefault("four"));
        }
    }
}
