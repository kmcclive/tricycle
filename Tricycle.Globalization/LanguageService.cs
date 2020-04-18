using System;
using System.Collections.Generic;
using System.Linq;

namespace Tricycle.Globalization
{
    public class LanguageService : ILanguageService
    {
        public Models.Language Find(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            IEnumerable<Iso639.Language> languages = null;

            if (code.Length == 2)
            {
                languages = Iso639.Language.Database.Where(l => code.Equals(l.Part1, StringComparison.OrdinalIgnoreCase));
            }

            if (code.Length == 3)
            {
#pragma warning disable 612
                languages = Iso639.Language.Database.Where(l => code.Equals(l.Part3, StringComparison.OrdinalIgnoreCase) ||
                                                                code.Equals(l.Part2, StringComparison.OrdinalIgnoreCase) ||
                                                                code.Equals(l.Part2B, StringComparison.OrdinalIgnoreCase));
#pragma warning restore 612
            }

            return languages?.Select(l => Map(l)).FirstOrDefault();
        }

        Models.Language Map(Iso639.Language language)
        {
#pragma warning disable 612
            return new Models.Language(language.Name, language.Part3, language.Part2B, language.Part2, language.Part1);
#pragma warning restore 612
        }
    }
}
