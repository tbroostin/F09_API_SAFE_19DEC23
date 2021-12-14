// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// DTO to hold the Faculty data for section with the Instruction method
    /// </summary>
    public class SectionFaculty
    {    
        /// <summary>
        /// Faculty If of this section
        /// </summary>
        public string FacultyId { get; set; }


        /// <summary>
        /// Instructional method code
        /// </summary>
        public string InstructionalMethodCode { get; set; }
    }
}

