using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Utilities.F09.Extensions
{
    public static class Generics
    {
        public static T To<T, T1>(this T1 from, string removePattern = "")
        {
            var to = Activator.CreateInstance<T>();
            // Remove duplicate code
            return To(from, to, removePattern);
        }
        public static T To<T, T1>(this T1 from, T to, string removePattern = "")
        {
            var toProperties = to.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in from.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var nameCompare = Regex.Replace(prop.Name.ToLower(), removePattern, String.Empty, RegexOptions.IgnoreCase);
                var toProp = toProperties.FirstOrDefault(x => prop.PropertyType == x.PropertyType && Regex.IsMatch(x.Name, nameCompare, RegexOptions.IgnoreCase));
                var propValue = prop.GetValue(from, null);
                if (toProp != null && propValue != null)
                {
                    toProp.SetValue(to, propValue, null);
                }
            }
            // some things are fields in Collague, but we'll be using them as properties
            foreach (var field in from.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var nameCompare = Regex.Replace(field.Name.ToLower(), removePattern, String.Empty, RegexOptions.IgnoreCase);
                var toProp = toProperties.FirstOrDefault(x => field.FieldType == x.PropertyType && Regex.IsMatch(x.Name, nameCompare, RegexOptions.IgnoreCase));
                var propValue = field.GetValue(from);
                if (toProp != null && propValue != null)
                {
                    toProp.SetValue(to, propValue, null);
                }
            }
            return to;
        }

        public static IEnumerable<PropertyInfo> GetValCodeProperties<T>(this T item) => item.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.IsDefined(typeof(ValCodeAttribute), false));

        public static T ApplyActionValCodeProperties<T>(this T item, Func<string, ValCodeAttribute, string> action)
        {
            foreach (var valCodeProperty in item.GetValCodeProperties())
            {
                if (valCodeProperty.PropertyType == typeof(string))
                {
                    //var val = action(valCodeProperty.GetValue(item, null) as string, valCodeProperty.GetCustomAttribute<ValCodeAttribute>());
                    var val = action(valCodeProperty.GetValue(item, null) as string,
                        (ValCodeAttribute)valCodeProperty.GetCustomAttributes(typeof(ValCodeAttribute), false)
                            .First());
                    valCodeProperty.SetValue(item, val, null);
                }
                else if (valCodeProperty.PropertyType == typeof(List<string>))
                {
                    List<string> values = valCodeProperty.GetValue(item, null) as List<string> ?? new List<string>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        values[i] = action(values[i],
                            (ValCodeAttribute)valCodeProperty.GetCustomAttributes(typeof(ValCodeAttribute), false)
                                .First());

                    }

                    valCodeProperty.SetValue(item, values, null);
                }
                else
                {
                    // ignore it, valcodes should only be strings
                }
            }

            return item;
        }

        public static ValCodeAttribute GetValCodeAttribute<T>(this T item, Expression<Func<T, object>> exprFunc)
        {
            var body = (PropertyInfo)((MemberExpression)exprFunc.Body).Member;
            return (ValCodeAttribute)body.GetCustomAttributes(typeof(ValCodeAttribute), false).First();
        }

        public static T ApplyActionValCodePropertiesRecursive<T>(this T item,
            Func<string, ValCodeAttribute, string> action)
        {
            if (item == null || item.Equals(default(T))) { return default(T); }
            // Check each property,
            // the property is a class (assume it's complex) go look for the applied Attribute
            // if it's A list of things, see if the things is also a class, should ignore things like List<string>
            // for the recurse
            var complexObjects = item.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType != typeof(string) && (x.PropertyType.IsClass ||
                        (typeof(IEnumerable).IsAssignableFrom(x.PropertyType) &&
                        x.PropertyType.GetGenericArguments()[0].IsClass)));
            foreach (var complex in complexObjects)
            {
                if (typeof(IEnumerable).IsAssignableFrom(complex.PropertyType))
                {
                    foreach (var i in (IEnumerable)complex.GetValue(item, null) ?? new List<object>())
                    {
                        ApplyActionValCodePropertiesRecursive(i, action);
                    }
                }
                else
                {
                    ApplyActionValCodePropertiesRecursive(complex.GetValue(item, null), action);
                }
            }

            ApplyActionValCodeProperties(item, action);

            return item;
        }

    }
}
