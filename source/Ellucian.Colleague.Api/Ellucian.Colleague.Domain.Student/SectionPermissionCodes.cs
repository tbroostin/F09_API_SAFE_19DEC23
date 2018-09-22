// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
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
        // Authority to create registrations
        public const string CreateRegistrations = "CREATE.REGISTRATIONS";
        // Authority to delete registrations
        public const string DeleteRegistrations = "DELETE.REGISTRATIONS";
    }
}
