using System;
using System.Linq;
using Iso639;

namespace Tricycle.Globalization
{
    public class LanguageService : ILanguageService
    {
        public Language Find(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            if (code.Length == 2)
            {
                return Language.Database.FirstOrDefault(l => code.Equals(l.Part1, StringComparison.OrdinalIgnoreCase));
            }

            if (code.Length == 3)
            {
                return Language.Database.FirstOrDefault(l =>
                    code.Equals(l.Part3, StringComparison.OrdinalIgnoreCase) ||
                    code.Equals(l.Part2, StringComparison.OrdinalIgnoreCase) ||
                    code.Equals(l.Part2B, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }
    }
}
