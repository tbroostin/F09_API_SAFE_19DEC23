/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A StudentFinancialAidChecklist is the list of checklist items a student needs to complete for an award year in order
    /// to get Financial Aid for that year.
    /// </summary>
    public class StudentFinancialAidChecklist
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom this checklist belongs
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Award Year this checklist belongs to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// A list of checklist items for this student and year
        /// </summary>
        public List<StudentChecklistItem> ChecklistItems { get; set; }
    }
}
