using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tricycle.Utilities;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class ArgumentPropertyReflector : IArgumentPropertyReflector
    {
        static readonly IArgumentConverter DEFAULT_CONVERTER = new ArgumentConverter();

        readonly IDictionary<Type, IArgumentConverter> _convertersByType = new Dictionary<Type, IArgumentConverter>()
        {
            { DEFAULT_CONVERTER.GetType(), DEFAULT_CONVERTER }
        };

        public IList<ArgumentProperty> Reflect(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.GetCustomAttribute<ArgumentIgnoreAttribute>() == null)
                                .OrderBy(p => p.GetCustomAttribute<ArgumentOrderAttribute>()?.Order ?? int.MaxValue)
                                .Select(p => Map(p, obj))
                                .Where(p => p.Value != null)
                                .ToList();
        }

        ArgumentProperty Map(PropertyInfo property, object obj)
        {
            Type converterType = property.GetCustomAttribute<ArgumentConverterAttribute>()?.Converter;

            return new ArgumentProperty()
            {
                PropertyName = property.Name,
                ArgumentName = property.GetCustomAttribute<ArgumentAttribute>()?.Name,
                Converter = GetConverter(converterType),
                HasCustomConverter = converterType != null,
                Value = property.GetValue(obj)
            };
        }

        IArgumentConverter GetConverter(Type type)
        {
            IArgumentConverter result = DEFAULT_CONVERTER;

            if (type != null)
            {
                result = _convertersByType.GetValueOrDefault(type)
                         ?? Activator.CreateInstance(type) as IArgumentConverter;
            }

            if (result != null)
            {
                result.Reflector = this;
            }

            return result;
        }
    }
}
