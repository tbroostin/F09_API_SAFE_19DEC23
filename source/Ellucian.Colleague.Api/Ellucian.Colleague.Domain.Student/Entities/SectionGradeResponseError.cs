// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// An error related to updating a grade.
    /// </summary>
    [Serializable]
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
