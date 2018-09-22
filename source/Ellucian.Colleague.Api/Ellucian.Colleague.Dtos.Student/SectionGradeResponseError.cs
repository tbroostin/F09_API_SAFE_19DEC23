// Copyright 2014 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Represents an error related to importing a grade.
    /// </summary>
    public class SectionGradeResponseError
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Property or field that caused an error
        /// </summary>
        public string Property { get; set; }
    }
}
