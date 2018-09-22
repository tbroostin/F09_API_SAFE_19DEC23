using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A wrapper around a list of StudentAwards
    /// </summary>
    public class StudentAwardPackage
    {
        /// <summary>
        /// A list of student awards that comprise part or all of a single student's award package for an award year.
        /// </summary>
        public IEnumerable<StudentAward> StudentAwards { get; set; }
    }
}
