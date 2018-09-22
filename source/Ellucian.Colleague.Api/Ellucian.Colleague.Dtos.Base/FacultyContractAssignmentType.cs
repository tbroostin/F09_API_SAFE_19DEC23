using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    
    /// <summary>
    /// Enum for Faculty contract assignment type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FacultyContractAssignmentType
    {
        /// <summary>
        /// This is a course section faculty
        /// </summary>
        CourseSectionFaculty,
        /// <summary>
        /// This is a campus organization advisor
        /// </summary>
        CampusOrganizationAdvisor,
        /// <summary>
        /// This is a campus organization member
        /// </summary>
        CampusOrganizationMember
    }
}
