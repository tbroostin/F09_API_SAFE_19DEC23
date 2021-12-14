// Copyright 2021 Ellucian Company L.P. and its affiliates.
 using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Method of Anonymous Grading being used
    /// </summary>
    [Serializable]
    public enum AnonymousGradingType
    {
        /// <summary>
        /// Anonymous Grading has not be configured or is not being used
        /// </summary>
        None,

        /// <summary>
        /// Anonymous Grading is done by term and a unique ids is generated for each student term 
        /// </summary>
        Term,

        /// <summary>
        /// Anonymous Grading is done by section and a unique id is generated for each student course section
        /// </summary>
        Section,
    }
}
