using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Utilities.F09.Extensions;

namespace Ellucian.Colleague.Utilities.F09
{
    public static class Helpers
    {
        public static bool? ConvertBoolNullable(this string s)
        {
            return !s.Empty()
                ? (bool?)s.ToLower().Contains('y')
                : null;
        }
        public static bool ConvertBool(this string s)
        {
            return !s.Empty() && s.ToLower().Contains('y');
        }

        public static string ConvertYesNo(this bool? b)
        {
            return b.HasValue ? (b.Value ? "Y" : "N") : null;
        }
        public static string ConvertYesNull(this bool? b)
        {
            return b == true ? "Y" : null;
        }

        public static string ConvertYesNo(this bool b)
        {
            return ConvertYesNo(b as bool?);
        }

        public static string ConvertYesNull(this bool b)
        {
            return ConvertYesNull(b as bool?);
        }
    }
}
