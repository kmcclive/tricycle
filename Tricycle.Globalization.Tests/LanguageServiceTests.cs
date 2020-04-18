using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tricycle.Globalization.Tests
{
    [TestClass]
    public class LanguageServiceTests
    {
        LanguageService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new LanguageService();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindThrowsExceptionWhenCodeIsNull()
        {
            _service.Find(null);
        }

        [TestMethod]
        public void FindReturnsNullForEmptyCode()
        {
            Assert.IsNull(_service.Find(string.Empty));
        }

        [TestMethod]
        public void FindReturnsLanguageForPart1()
        {
            var language = _service.Find("fr");

            Assert.AreEqual("French", language?.Name);
        }

        [TestMethod]
        public void FindReturnsLanguageForPart2()
        {
            var language = _service.Find("fra");

            Assert.AreEqual("French", language?.Name);
        }

        [TestMethod]
        public void FindReturnsLanguageForPart2B()
        {
            var language = _service.Find("fre");

            Assert.AreEqual("French", language?.Name);
        }

        [TestMethod]
        public void FindReturnsLanguageForPart3()
        {
            var language = _service.Find("hbo");

            Assert.AreEqual("Ancient Hebrew", language?.Name);
        }
    }
}
