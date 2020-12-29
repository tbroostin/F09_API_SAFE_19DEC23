// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// An Academic Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Unique Id for this Program
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Program title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Description of the program
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Departments in which this program is offered
        /// </summary>
        public IEnumerable<string> Departments { get; set; }
        /// <summary>
        /// Catalogs in which this program is offered
        /// </summary>
        public IEnumerable<string> Catalogs { get; set; }

        /// <summary>
        /// Anticipated Completion Date of the Program
        /// </summary>
        public DateTime? AnticipatedCompletionDate { get; set; }
        /// <summary>
        /// Start Date of Student Program
        /// </summary>
        public DateTime? ProgramStartDate { get; set; }
        /// <summary>
        /// End Date of Student Program (when inactive or graduated).
        /// </summary>
        public DateTime? ProgramEndDate { get; set; }
        /// <summary>
        /// List of Majors included in this program
        /// </summary>
        public IEnumerable<string> Majors { get; set; }
        /// <summary>
        /// List of Minors included in this program
        /// </summary>
        public IEnumerable<string> Minors { get; set; }
        /// <summary>
        /// List of Ccds included in this program
        /// </summary>
        public IEnumerable<string> Ccds { get; set; }
        /// <summary>
        /// List of specializations included in this program
        /// </summary>
        public IEnumerable<string> Specializations { get; set; }
        /// <summary>
        /// Degree awarded
        /// </summary>
        public string Degree { get; set; }
        /// <summary>
        /// Academic level of this program (e.g., Undergraduate, Graduate)
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// List of programs related to this program for the purposes of fastest path evaluation
        /// </summary>
        public IEnumerable<string> RelatedPrograms { get; set; }

        /// <summary>
        /// The transcript grouping (UG, GRAD, etc) associated to this program
        /// </summary>
        public string TranscriptGrouping { get; set; }
 
        /// <summary>
        /// The transcript grouping to use for presenting the unofficial transcript to the user
        /// </summary>
        public string UnofficialTranscriptGrouping { get; set; }

        /// <summary>
        /// The transcript grouping to use for presenting the official transcript to the user
        /// </summary>
        public string OfficialTranscriptGrouping { get; set; }

        /// <summary>
        /// Indicates whether graduation is allowed from this program.
        /// </summary>
        public bool IsGraduationAllowed { get; set; }

        /// <summary>
        /// Indicates whether the academic program is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Locations where this program is offered.
        /// </summary>
        public IEnumerable<string> Locations { get; set; }
    }
}
