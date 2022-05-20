using System;
using Tricycle.Globalization.Models;

namespace Tricycle.Globalization.Tests;

[TestClass]
public class LanguageServiceTests
{
    static readonly Language FRENCH = new Language("French", "fra", "fre", "fra", "fr");
    static readonly Language HEBREW = new Language("Ancient Hebrew", "hbo", null, null, null);

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
        var language = _service.Find(FRENCH.Part1);

        Assert.IsNotNull(language);
        Assert.AreEqual(FRENCH.Name, language.Name);
        Assert.AreEqual(FRENCH.Part3, language.Part3);
        Assert.AreEqual(FRENCH.Part2B, language.Part2B);
        Assert.AreEqual(FRENCH.Part2, language.Part2);
        Assert.AreEqual(FRENCH.Part1, language.Part1);
    }

    [TestMethod]
    public void FindReturnsLanguageForPart2()
    {
        var language = _service.Find(FRENCH.Part2);

        Assert.IsNotNull(language);
        Assert.AreEqual(FRENCH.Name, language.Name);
        Assert.AreEqual(FRENCH.Part3, language.Part3);
        Assert.AreEqual(FRENCH.Part2B, language.Part2B);
        Assert.AreEqual(FRENCH.Part2, language.Part2);
        Assert.AreEqual(FRENCH.Part1, language.Part1);
    }

    [TestMethod]
    public void FindReturnsLanguageForPart2B()
    {
        var language = _service.Find(FRENCH.Part2B);

        Assert.IsNotNull(language);
        Assert.AreEqual(FRENCH.Name, language.Name);
        Assert.AreEqual(FRENCH.Part3, language.Part3);
        Assert.AreEqual(FRENCH.Part2B, language.Part2B);
        Assert.AreEqual(FRENCH.Part2, language.Part2);
        Assert.AreEqual(FRENCH.Part1, language.Part1);
    }

    [TestMethod]
    public void FindReturnsLanguageForPart3()
    {
        var language = _service.Find(HEBREW.Part3);

        Assert.IsNotNull(language);
        Assert.AreEqual(HEBREW.Name, language.Name);
        Assert.AreEqual(HEBREW.Part3, language.Part3);
        Assert.AreEqual(HEBREW.Part2B, language.Part2B);
        Assert.AreEqual(HEBREW.Part2, language.Part2);
        Assert.AreEqual(HEBREW.Part1, language.Part1);
    }
}
