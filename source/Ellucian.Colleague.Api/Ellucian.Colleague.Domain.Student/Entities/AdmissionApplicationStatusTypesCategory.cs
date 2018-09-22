// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The top level category of the admission application status type
    /// </summary>
    [Serializable]
    public enum AdmissionApplicationStatusTypesCategory
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// started
        /// </summary>
        Started,
        /// <summary>
        /// submitted
        /// </summary>
        Submitted,
        /// <summary>
        /// readyForReview
        /// </summary>
        Readyforreview,
        /// <summary>
        /// decisionMade
        /// </summary>
        Decisionmade,
        /// <summary>
        /// enrollmentComplete
        /// </summary>
        Enrollmentcomplete,
        /// <summary>
        /// admitted
        /// </summary>
        Admitted,
        /// <summary>
        /// movedToStudentSystem
        /// </summary>
        MovedToStudentSystem,
        /// <summary>
        /// applied
        /// </summary>
        Applied,
        /// <summary>
        /// complete
        /// </summary>
        Complete,
        /// <summary>
        /// accepted
        /// </summary>
        Accepted,
        /// <summary>
        /// waitlisted
        /// </summary>
        WaitListed,
        /// <summary>
        /// rejected
        /// </summary>
        Rejected,
        /// <summary>
        /// withdrawn
        /// </summary>
        Withdrawn,
        /// <summary>
        /// movedToStudent
        /// </summary>
        MovedToStudent,
    }
}
