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

            string path, expected;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = @"C:\Users\Fred Mertz\info.txt";
                expected = "\"C:\\Users\\Fred Mertz\\info.txt\"";
            }
            else
            {
                path = @"/Users/Fred Mertz/info.txt";
                expected = @"/Users/Fred\ Mertz/info.txt";
            }

            Assert.AreEqual(expected, utility.EscapeFilePath(path));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = @"C:\Users\Fred Mertz\";
                expected = "\"C:\\Users\\Fred Mertz\\\\";

                Assert.AreEqual(expected, utility.EscapeFilePath(path));
            }
        }
    }
}
