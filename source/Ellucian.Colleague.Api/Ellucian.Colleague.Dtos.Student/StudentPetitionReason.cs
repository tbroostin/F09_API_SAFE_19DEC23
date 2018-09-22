// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Locally defined reasons for a student petition or faculty consent
    /// </summary>
    public class StudentPetitionReason
    {
        /// <summary>
        /// The reason code (unique identifier)
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The description for this code.
        /// </summary>
        public string Description { get; set; }
    }
}
