// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base
{
    [Serializable]
    public static class BasePermissionCodes
    {
        // Access to create person
        public const string CreatePerson = "CREATE.PERSON";

        // Access to update a person
        public const string UpdatePerson = "UPDATE.PERSON";

        //Acces to view an address
        public const string ViewAddress = "VIEW.ADDRESS";

        //Acces to update an address
        public const string UpdateAddress = "UPDATE.ADDRESS";

        //Acces to create a organization
        public const string CreateOrganization = "CREATE.ORGANIZATION";

        //Acces to update a organization
        public const string UpdateOrganization = "UPDATE.ORGANIZATION";

        //Acces to view a organization
        public const string ViewOrganization = "VIEW.ANY.ORGANIZATION";

        // Access to view any person's data
        public const string ViewAnyPerson = "VIEW.ANY.PERSON";

        // Access to view any person relationships
        public const string ViewAnyPersonRelationship = "VIEW.PERSON.RELATIONSHIPS";

        // Access to view any nonperson relationships
        public const string ViewAnyNonPersonRelationship = "VIEW.NONPERSON.RELATIONSHIPS";

        // Access to create/update any person relationships
        public const string UpdatePersonRelationship = "UPDATE.PERSON.RELATIONSHIPS";

        // Access to create/update any nonperson relationships
        public const string UpdateNonPersonRelationship = "UPDATE.NONPERSON.RELATIONSHIPS";

        // Access to delete any person relationships
        public const string DeletePersonRelationship = "DELETE.PERSON.RELATIONSHIPS";

        // Access to delete any nonperson relationships
        public const string DeleteNonPersonRelationship = "DELETE.NONPERSON.RELATIONSHIPS";

        // Access to view integration configuration information
        public const string ViewIntegrationConfig = "VIEW.INTEGRATION.CONFIG";

        // Enables a person to update their own email addresses.
        // Used in endpoint UpdatePersonProfile: PUT persons/{personId} application/vnd.ellucian-person-profile.v{}+json
        public const string UpdateOwnEmail = "UPDATE.OWN.EMAIL";

        // Enables a person to update their own phone numbers
        // Used in endpoint UpdatePersonProfile: PUT persons/{personId} application/vnd.ellucian-person-profile.v{}+json
        public const string UpdateOwnPhone = "UPDATE.OWN.PHONE";

        // Enables a person to update their own phone numbers
        // Used in endpoint UpdatePersonProfile: PUT persons/{personId} application/vnd.ellucian-person-profile.v{}+json
        public const string UpdateOwnAddress = "UPDATE.OWN.ADDRESS";

        // Access to view any GL Accounts
        public const string ViewAnyGlAccts = "VIEW.ANY.GL.ACCTS";

        // Access to view any Projects
        public const string ViewAnyProjects = "VIEW.ANY.PROJECTS";

        // Access to view external employments
        public const string ViewAnyExternalEmployments = "VIEW.EXTERNAL.EMPLOYMENTS";

        // Access to view any Projects Line Items
        public const string ViewAnyProjectsLineItems = "VIEW.ANY.PROJECTS.LINE.ITEMS";

        // Enables a vender to create, update, or delete banking information
        public const string EditVendorBankingInformation = "EDIT.VENDOR.BANKING.INFORMATION";

        // Enables an employee to create, update, or delete banking information
        public const string EditEChecksBankingInformation = "EDIT.ECHECKS.BANKING.INFORMATION";

        // Enables access to view others' emergency and missing person contacts
        // Used in endpoint GetEmergencyInformation: GET /persons/{personId}/emergency-information
        public const string ViewPersonEmergencyContacts = "VIEW.PERSON.EMERGENCY.CONTACTS";

        // Enables access to view others' health conditions
        // Used in endpoint GetEmergencyInformation: GET /persons/{personId}/emergency-information
        public const string ViewPersonHealthConditions = "VIEW.PERSON.HEALTH.CONDITIONS";

        // Enables access to view others' additional emergency information (e.g. insurance, hospital)
        // Used in endpoint GetEmergencyInformation: GET /persons/{personId}/emergency-information
        public const string ViewPersonOtherEmergencyInformation = "VIEW.PERSON.OTHER.EMERGENCY.INFORMATION";

        // Enables access to view others' Person Restrictions
        // Used in endpoint GetStudentRestrictions: GET students/{studentId}/restrictions
        public const string ViewPersonRestrictions = "VIEW.PERSON.RESTRICTIONS";

        // Enables a person to update the organizational relationships (assign managers/subordinates)
        // Used in endpoints for modifying organizational relationships
        // CreateOrganizationalRelationship: POST /organizational-relationships
        // UpdateOrganizationalRelationship: POST /organizational-relationships/{id}
        // DeleteOrganizationalRelationship: DELETE /organizational-relationships/{id}
        public const string UpdateOrganizationalRelationships = "UPDATE.ORGANIZATIONAL.RELATIONSHIPS";

        /// <summary>
        /// Enables a person to view employee W2 tax information
        /// Used in TaxFormConsentService
        /// </summary>
        public const string ViewEmployeeW2 = "VIEW.EMPLOYEE.W2";

        /// <summary>
        /// Enables a person to view employee 1095C tax information
        /// Used in TaxFormConsent service
        /// </summary>
        public const string ViewEmployee1095C = "VIEW.EMPLOYEE.1095C";
        
        /// <summary>
        /// Enable user to view their own T4A information
        /// </summary>
        public const string ViewT4A = "VIEW.T4A";

        /// <summary>
        /// Enable user to view another user's T4A information (ie: Tax Information Admin)
        /// </summary>
        public const string ViewRecipientT4A = "VIEW.RECIPIENT.T4A";

        /// <summary>
        /// Enables a user to view another employee's T4 information (ie: Tax Information Admin)
        /// </summary>
        public const string ViewEmployeeT4 = "VIEW.EMPLOYEE.T4";

        /// <summary>
        /// Enables a user to view their own W2 information
        /// </summary>
        public const string ViewW2 = "VIEW.W2";

        /// <summary>
        /// Enables a user to view their own 1095C information
        /// </summary>
        public const string View1095C = "VIEW.1095C";

        /// <summary>
        /// Enables a user to view their own T4 information
        /// </summary>
        public const string ViewT4 = "VIEW.T4";

        /// <summary>
        /// Enables access to the user's own 1098 tax form data
        /// </summary>
        public const string View1098 = "VIEW.1098";

        /// <summary>
        /// Enables access to other users' 1098 data (ie: Tax Information Admin)
        /// </summary>
        public const string ViewStudent1098 = "VIEW.STUDENT.1098";

        /// <summary>
        /// Enables access to the user's own T2202A tax form data
        /// </summary>
        public const string ViewT2202A = "VIEW.T2202A";

        /// <summary>
        /// Enables access to other users' T2202A data (ie: Tax Information Admin)
        /// </summary>
        public const string ViewStudentT2202A = "VIEW.STUDENT.T2202A";

        // Access to view any comments
        public const string ViewComment = "VIEW.COMMENT";

        // Access to create/update any comments
        public const string UpdateComment = "UPDATE.COMMENT";

        // Access to delete any comments
        public const string DeleteComment = "DELETE.COMMENT";

        /// <summary>
        /// Permission to view account activity of anyone.
        /// </summary>
        public const string ViewStudentAccountActivity = "VIEW.STUDENT.ACCOUNT.ACTIVITY";

        /// <summary>
        /// Enables a user to view their own 1099MI information
        /// </summary>
        public const string View1099MISC = "VIEW.1099MISC";

        // Access to view any personal relationships
        public const string ViewAnyRelationship = "VIEW.RELATIONSHIP";

        // Access to view any person contacts
        public const string ViewAnyPersonContact = "VIEW.PERSON.CONTACT";

        // Access to view any person guardians
        public const string ViewAnyPersonGuardian = "VIEW.PERSON.GUARDIAN";

        // Access to view any person visas
        public const string ViewAnyPersonVisa = "VIEW.PERSON.VISA";

        // Access to update/create any person visas
        public const string UpdateAnyPersonVisa = "UPDATE.PERSON.VISA";

        // Access to view any educational institutions
        public const string ViewEducationalInstitution = "VIEW.EDUCATIONAL.INSTITUTION";

        // Access to Recruiter operations
        public const string PerformRecruiterOperations = "PERFORM.RECRUITER.OPERATIONS";
    }
}