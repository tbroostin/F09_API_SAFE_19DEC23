using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student
{
    /// <summary>
    /// Defines constants for Student Domain
    /// </summary>
    public static class StudentConstants
    {
        /// <summary>
        /// The IncludedYearsFor1098E indicates the comma seperated years for which 1098E form can be generated
        /// This constant has become obsolete
        /// </summary>
        public const string IncludedYearsFor1098E = "2017, 2016 , 2015 , 2014, 2013, 2012, 2011";

        /// <summary>
        /// This constant string is assigned while 1098 Tax processing on determining if the tax form is 1098T
        /// </summary>
        public const string TaxForm1098TName = "1098T";

        /// <summary>
        /// This constant string is assigned while 1098 Tax processing on determining if the tax form is 1098E
        /// </summary>
        public const string TaxForm1098EName = "1098E";

    }
}
