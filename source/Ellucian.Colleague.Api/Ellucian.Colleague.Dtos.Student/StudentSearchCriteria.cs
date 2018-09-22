using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains INCOMING advisee search/filter request
    /// Search criteria must either contain an AdviseeKeyword or an AdvisorKeyword value of at least 2 characters. It cannot contain both.
    /// </summary>
    public class StudentSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public StudentSearchCriteria()
        {

        }
        /// <summary>
        /// Used when requesting a search of advisees by name or Id. [last, first middle] or [first middle last] or [last] or [Id] expected.
        /// Must contain at least 2 characters when provided.
        /// </summary>
        public string StudentKeyword { get; set; }
       
    }
}
