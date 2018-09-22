using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Faculty contract position
    /// </summary>
    public class FacultyContractPosition
    {
        /// <summary>
        /// Id for the faculty contract position
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Load period id for the faculty contract position
        /// </summary>
        public string LoadPeriodId { get; set; }

        /// <summary>
        /// Inteded load for the faculty contract position
        /// </summary>
        public decimal? IntendedLoad { get; set; }

        /// <summary>
        /// Faculty contract assignments for the faculty contract position
        /// </summary>
        public List<FacultyContractAssignment> FacultyContractAssignments { get; set; }
        /// <summary>
        /// Position Id for the faculty contract position
        /// </summary>
        public string PositionId { get; set; }
        /// <summary>
        /// Title for the faculty contract position
        /// </summary>
        public string Title { get; set; }
    }
}
