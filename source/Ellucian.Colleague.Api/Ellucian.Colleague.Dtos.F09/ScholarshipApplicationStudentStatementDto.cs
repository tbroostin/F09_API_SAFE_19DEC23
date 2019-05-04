using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    // F09 added here on 05-04-2019 for Demo Reporting Project

    public class ScholarshipApplicationStudentStatementDto
    {
        public string StudentId { get; set; }

        public string StudentName { get; set; }

        public List<ScholarshipApplicationAwardsDto> Awards { get; set; }

        /// <summary>
        /// Date on which the statement was generated
        /// </summary>
        public DateTime Date { get; set; }
    }
}
