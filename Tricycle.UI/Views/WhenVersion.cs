using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tricycle.UI.Views
{
    [ContentProperty("Versions")]
    public class WhenVersion<T>
    {
        public IList<When> Versions { get; set; } = new List<When>();
        public T Default { get; set; }

        public static implicit operator T(WhenVersion<T> whenVersion)
        {
            foreach (var condition in whenVersion.Versions)
            {
                var predicate = GetPredicate(condition.Is);
                var version = Version.Parse(condition.Version);

                if (predicate(version))
                {
                    var value = condition.Value.ConvertTo(typeof(T), (Func<MemberInfo>)null, null, out Exception exception);

                    if(exception != null)
                    {
                        throw new XamlParseException(exception.Message, exception);
                    }

                    return (T)value;
                }
            }

            return whenVersion.Default;
        }

        static Predicate<Version> GetPredicate(VersionOperator op)
        {
            switch (op)
            {
                case VersionOperator.GreaterThan:
                    return v => DeviceInfo.Version > v;
                case VersionOperator.GreaterThanOrEqualTo:
                    return v => DeviceInfo.Version >= v;
                case VersionOperator.LessThan:
                    return v => DeviceInfo.Version < v;
                case VersionOperator.LessThanOrEqualTo:
                    return v => DeviceInfo.Version <= v;
                case VersionOperator.EqualTo:
                default:
                    return v => DeviceInfo.Version == v;
            }
        }
    }

    public enum VersionOperator
    {
        EqualTo,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo
    }

    [ContentProperty("Value")]
    public class When
    {
        public VersionOperator Is { get; set; }
        public string Version { get; set; }
        public object Value { get; set; }
    }
}
