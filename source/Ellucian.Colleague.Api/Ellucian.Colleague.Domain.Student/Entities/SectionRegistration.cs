// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionRegistration
    {
        // Optional GUID of the STUDENT.ACAD.CRED record association with the registration.
        public string Guid { get; set; }
        // COURSE.SECTIONS.ID value
        public string SectionId { get; set; }
        // The action to take on the section
        public RegistrationAction Action { get; set; }
        // An optional number of credits for an Add or Wailist action. This would be set for a variable credit section.
        // There is no reason to set this for a fixed credit section, though if set, it must equal the fixed number of
        // credits of the section.
        public decimal? Credits { get; set; }
        // There is no need to ever set this property. Although backend registration accepts this value, it ignores it and 
        // always assigns the number of CEUS of the section to the registration.
        public decimal? Ceus { get; set; }
        // An optional effective registration date, if different from the current date. Not all registration repository
        // methods apply this property.
        public DateTime? RegistrationDate { get; set; }
        // Optional academic level for the registration. Not all registration repository
        // methods apply this property.
        public string AcademicLevelCode { get; set; }
        // A code from the STUDENT.ACAD.CRED.STATUS.REASONS valcode. Applies only when Action is Drop. Not all registration repository
        // methods apply this property.
        public string DropReasonCode { get; set; }
        // System ID of a record in the INT.TO.WDRL.CODES file. Applies only when Action is Drop. Not all registration repository
        // methods apply this property.
        public string IntentToWithdrawId { get; set; }
        // Section registration start on date
        public DateTimeOffset? InvolvementStartOn { get; set; }
        // Section registration end on date
        public DateTimeOffset? InvolvementEndOn { get; set; }
    }
}
