using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Diagnostics.Utilities;

namespace Tricycle.Diagnostics.Tests.Utilities
{
    [TestClass]
    public class ProcessUtilityTests
    {
        [TestMethod]
        public void TestEscapeFilePath()
        {
            var utility = ProcessUtility.Self;

            Assert.ThrowsException<ArgumentNullException>(() => utility.EscapeFilePath(null));

            string path = "/Users/Fred Mertz/info.txt";
            string expected = "\"/Users/Fred Mertz/info.txt\"";

            Assert.AreEqual(expected, utility.EscapeFilePath(path));

            path = @"C:\Users\Fred Mertz\";
            expected = "\"C:\\Users\\Fred Mertz\\\\\"";

            Assert.AreEqual(expected, utility.EscapeFilePath(path));

            path = "/Users/Fred Mertz/\"good movie\".mkv";
            expected = "\"/Users/Fred Mertz/\\\"good movie\\\".mkv\"";

            Assert.AreEqual(expected, utility.EscapeFilePath(path));
        }
    }
}
