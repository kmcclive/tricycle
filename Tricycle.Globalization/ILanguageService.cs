using System;
using Iso639;

namespace Tricycle.Globalization
{
    /// <summary>
    /// Supports finding languages.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Finds the language for a specified code.
        /// </summary>
        /// <param name="code">The code to find the language for.</param>
        /// <returns>The corresponding <see cref="Language"/> if found; otherwise, <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="code"/> is <c>null</c>.</exception>
        Language Find(string code);
    }
}
