// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student academic program
    /// </summary>
    public class StudentAcademicProgram
    {
        /// <summary>
        /// Id number of the student
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Id of the Academic program
        /// /// </summary>
        public string AcadamicProgramId { get; set; }

        /// <summary>
        /// Catalog year of the Academic program
        /// </summary>
        public string CatalogYear { get; set; }

        /// <summary>
        /// Start Date of the Academic program
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// List of active programs for the student
        /// </summary>
        public List<StudentActivePrograms> ActivePrograms { get; set; }

        /// <summary>
        /// Department of the Academic program
        /// </summary>
        public string Department { get; set; }
        
        /// <summary>
        /// Location of the Academic program
        /// </summary>
        public string Location { get; set; }

    }
}
