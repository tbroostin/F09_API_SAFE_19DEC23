using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Faculty contract assignment
    /// </summary>
    public class FacultyContractAssignment
    { 
        /// <summary>
        /// Id for the faculty contract assignment
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Assignment type for the faculty contract assignment
        /// </summary>
        public FacultyContractAssignmentType AssignmentType { get; set; }

        /// <summary>
        /// Position Id for the faculty contract assignment
        /// </summary>
        public string PositionId { get; set; }
        /// <summary>
        /// Assignment Description for faculty contract assignment
        /// </summary>
        public string AssignmentDescription { get; set; }
        /// <summary>
        /// Assignment start date for faculty contract
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Assignment end date for faculty contract
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Assignment inteded for faculty contract
        /// </summary>
        public Decimal? IntendedLoad { get; set; }
        /// <summary>
        /// Assignment amount for faculty contract
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// Role or instructional method for the assignment
        /// </summary>
        public string Role { get; set; }
    }
}
