// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A designation of a student's status within a course
    /// </summary>
    [DataContract]
    public class SectionRegistrationStatusItem2 : CodeItem2
    {
        /// <summary>
        /// The current status of the registration for the section.
        /// </summary>
        [DataMember(Name = "status")]
        public SectionRegistrationStatusItemStatus Status { get; set; }
    }
}