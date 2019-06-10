using System;
namespace Tricycle.Media.Models
{
    public class LightLevelProperties
    {
        /// <summary>
        /// Gets or sets the maximum content light level.
        /// </summary>
        public int MaxCll { get; set; }

        /// <summary>
        /// Gets or sets the maximum frame average light level.
        /// </summary>
        public int MaxFall { get; set; }
    }
}
