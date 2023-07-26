// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Request to execute the specified proposed section registration actions to validate
    /// their success or failure, but not actually record the registrations.
    /// NOTE: This API assumes that the Seat Service is used to manage section seat reservations and counts and that the caller
    /// is updating the seat service separately, but in coordination with this API call. This API does not validate that a seat
    /// is available in the sections for which an add registration is requested.
    /// </summary>
    public class StudentRegistrationValidationOnlyRequest
    {
        /// <summary>
        /// List of section registration action requests
        /// </summary>
        public List<SectionRegistrationGuid> SectionActionRequests { get; set; }

        /// <summary>
        /// If true the response will contain a token to stored data from this validation.
        /// The token should be passed to a subsequent call to the skip validation registration
        /// API for the exact same sections and actions, which will use the data stored by this
        /// validation only API.
        /// Pass false in this flag if only running the validation with no plan to follow up 
        /// with a skip validations registration API call.
        /// </summary>
        public bool? RecordValidationDataFlag { get; set; }

        /// <summary>
        /// Pass true if validating the registration of a student cross-registering into sections that will be delivered by a different institution.
        /// Pass false to validate any other registration.
        /// </summary>
        public bool? CrossRegHomeStudent { get; set; }

    }

}
