// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Request to execute the specified section registration actions without validations. Most validation checks are bypassed, but
    /// basic system integrity rules are still enforced. For example the specified student GUID must be an actively enrolled student
    /// and a specified section GUID must exist in the database.
    /// NOTE: This API assumes that the Seat Service is used to manage section seat reservations and counts and that the caller
    /// is updating the seat service separately, but in coordination with this API call.
    /// </summary>
    public class StudentRegistrationSkipValidationsRequest
    {
        /// <summary>
        /// List of section registration action requests
        /// </summary>
        public List<SectionRegistrationGuid> SectionActionRequests { get; set; }

        /// <summary>
        /// Optional token returned by a previous call to the registration validation API.
        /// Pass this token with the exact same section registration actions that were sent to the registration 
        /// validation API, with the exception that any actions that failed with the registration validation API can 
        /// be included or left out. The system will retrieve information stored with the token, such as overrides used, 
        /// and store it whenthis API records the actual registrations.
        /// Do not pass a token when this call is not associated with a corresponding registration validation API call
        /// in the same Colleague instance.That would be the case when cross-registering a visiting student at the delivering
        /// school.
        /// </summary>
        public string ValidationToken { get; set; }

        /// <summary>
        /// Pass true if cross-registering a home student into sections that will be delivered by a different institution.
        /// Otherwise pass false
        /// </summary>
        public bool? CrossRegHomeStudent { get; set; }

        /// <summary>
        /// Pass true if cross-registering a visiting student into sections that this institution delivers
        /// Otherwise pass false
        /// </summary>
        public bool? CrossRegVisitingStudent { get; set; }

    }

}
