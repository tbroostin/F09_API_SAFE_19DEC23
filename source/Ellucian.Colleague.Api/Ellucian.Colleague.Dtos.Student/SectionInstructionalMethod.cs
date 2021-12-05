// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// DTO to hold the Section instructional methods takem from section's meetings and if meetings are empty
    ///  then will be loaded from SEC.INSTR.METHODS
    /// </summary>
    public class SectionInstructionalMethod
    {
        /// <summary>
        /// Instructional method code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Instructional method description
        /// </summary>
      public string Description { get; set; }
        /// <summary>
        /// Is onstructional method online
        /// </summary>
        public bool IsOnline { get;  set; }
    }
}

