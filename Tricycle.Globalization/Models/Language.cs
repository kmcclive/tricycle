using System;
namespace Tricycle.Globalization.Models
{
    public class Language
    {
        /// <summary>
        /// Gets the 3-digit ISO-639-3 language code.
        /// <para>
        /// The ISO-639-3 standard provides the widest definition set, and this property is guaranteed to not be <c>null</c>.
        /// </para>
        /// </summary>
        public string Part3 { get; }

        /// <summary>
        /// Gets the obsolete "bibliographic" ISO-639-2/B code, or <c>null</c> if none is defined.
        /// <para>These are legacy values, and very rarely seen anywhere in the wild.</para>
        /// <para>Only 22 languages have a B code defined.</para>
        /// </summary>
        public string Part2B { get; }

        /// <summary>
        /// Gets the 3-digit "terminological" ISO-639-2/T language code, or <c>null</c> if none is defined.
        /// </summary>
        public string Part2 { get; }

        /// <summary>
        /// Gets the 2-digit ISO-639-1 language code, or <c>null</c> if none is defined.
        /// </summary>
        public string Part1 { get; }

        /// <summary>
        /// Gets a brief name/description of the language.
        /// </summary>
        public string Name { get; }

        public Language()
        {

        }

        public Language(string name, string part3, string part2B, string part2, string part1)
        {
            Name = name;
            Part3 = part3;
            Part2B = part2B;
            Part2 = part2;
            Part1 = part1;
        }
    }
}
