// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student
{
    [Serializable]
    public static class SectionPermissionCodes
    {
        // Authority to update grades
        public const string UpdateGrades = "UPDATE.GRADES";
        // Authority to update registrations
        public const string UpdateRegistrations = "UPDATE.REGISTRATIONS";
        // Authority to view registrations
        public const string ViewRegistrations = "VIEW.REGISTRATIONS";
        // Authority to perform registration checks
        public const string PerformRegistrationChecks = "PERFORM.REGISTRATION.CHECKS";
    }
}
