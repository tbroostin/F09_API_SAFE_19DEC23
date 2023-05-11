// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Departmental oversight permissions entity 
    /// </summary>
    [Serializable]
    public class DepartmentalOversightPermissions
    {
        /// <summary>
        /// Indicates whether the user can view section roster
        /// </summary>
        public bool CanViewSectionRoster { get { return canViewSectionRoster; } }
        private readonly bool canViewSectionRoster;

        /// <summary>
        /// Indicates whether the user can view section attendance
        /// </summary>
        public bool CanViewSectionAttendance { get { return canViewSectionAttendance; } }
        private readonly bool canViewSectionAttendance;

        /// <summary>
        /// Indicates whether the user can view section drop roster
        /// </summary>
        public bool CanViewSectionDropRoster { get { return canViewSectionDropRoster; } }
        private readonly bool canViewSectionDropRoster;

        /// <summary>
        /// Indicates whether the user can view section census
        /// </summary>
        public bool CanViewSectionCensus { get { return canViewSectionCensus; } }
        private readonly bool canViewSectionCensus;

        /// <summary>
        /// Indicates whether the user can view section grading
        /// </summary>
        public bool CanViewSectionGrading { get { return canViewSectionGrading; } }
        private readonly bool canViewSectionGrading;

        /// <summary>
        /// Indicates whether the user can view section books
        /// </summary>
        public bool CanViewSectionBooks { get { return canViewSectionBooks; } }
        private readonly bool canViewSectionBooks;

        /// <summary>
        /// Indicates whether the user can view section prerequisite waiver
        /// </summary>
        public bool CanViewSectionPrerequisiteWaiver { get { return canViewSectionPrerequisiteWaiver; } }
        private readonly bool canViewSectionPrerequisiteWaiver;

        /// <summary>
        /// Indicates whether the user can view section student petitions
        /// </summary>
        public bool CanViewSectionStudentPetitions { get { return canViewSectionStudentPetitions; } }
        private readonly bool canViewSectionStudentPetitions;

        /// <summary>
        /// Indicates whether the user can view section faculty consents
        /// </summary>
        public bool CanViewSectionFacultyConsents { get { return canViewSectionFacultyConsents; } }
        private readonly bool canViewSectionFacultyConsents;

        /// <summary>
        /// Indicates whether the user can view section add authorizations
        /// </summary>
        public bool CanViewSectionAddAuthorizations { get { return canViewSectionAddAuthorizations; } }
        private readonly bool canViewSectionAddAuthorizations;

        /// <summary>
        /// Indicates whether the user can view section waitlists
        /// </summary>
        public bool CanViewSectionWaitlists { get { return canViewSectionWaitlists; } }
        private readonly bool canViewSectionWaitlists;

        /// <summary>
        /// Indicates whether the user can create/modify section books
        /// </summary>
        public bool CanCreateSectionBooks { get { return canCreateSectionBooks; } }
        private readonly bool canCreateSectionBooks;

        /// <summary>
        /// Indicates whether the user can create/modify section add authorization
        /// </summary>
        public bool CanCreateSectionAddAuthorization { get { return canCreateSectionAddAuthorization; } }
        private readonly bool canCreateSectionAddAuthorization;

        /// <summary>
        /// Indicates whether the user can create/modify section grading
        /// </summary>
        public bool CanCreateSectionGrading { get { return canCreateSectionGrading; } }
        private readonly bool canCreateSectionGrading;

        /// <summary>
        /// Indicates whether the user can create section prerequiste waiver
        /// </summary>
        public bool CanCreateSectionPrerequisiteWaiver { get { return canCreateSectionPrerequisiteWaiver; } }
        private readonly bool canCreateSectionPrerequisiteWaiver;

        /// <summary>
        /// Indicates whether the user can create/modify section student petition
        public bool CanCreateSectionStudentPetition { get { return canCreateSectionStudentPetition; } }
        private readonly bool canCreateSectionStudentPetition;

        /// <summary>
        /// Indicates whether the user can create/modify section faculty consent
        /// </summary>
        public bool CanCreateSectionFacultyConsent { get { return canCreateSectionFacultyConsent; } }
        private readonly bool canCreateSectionFacultyConsent;

        /// <summary>
        /// Indicates whether the user can search for students in the sections
        /// </summary>
        public bool CanSearchStudents { get { return canSearchStudents; } }
        private readonly bool canSearchStudents;


        /// <summary>
        /// Indicates whether the user can create/modify section attendance
        /// </summary>
        public bool CanCreateSectionAttendance { get { return canCreateSectionAttendance; } }
        private readonly bool canCreateSectionAttendance;


        /// <summary>
        /// Indicates whether the user can create/modify section census
        /// </summary>
        public bool CanCreateSectionCensus { get { return canCreateSectionCensus; } }
        private readonly bool canCreateSectionCensus;

        /// <summary>
        /// Indicates whether the user is eligible to drop/modify a student from their sections
        /// </summary>
        public bool CanCreateSectionDropRoster { get { return canCreateSectionDropRoster; } }
        private readonly bool canCreateSectionDropRoster;

        public DepartmentalOversightPermissions()
        {
            canViewSectionRoster = false;
            canViewSectionAttendance = false;
            canViewSectionDropRoster = false;
            canViewSectionCensus = false;
            canViewSectionGrading = false;
            canViewSectionBooks = false;
            canViewSectionPrerequisiteWaiver = false;
            canViewSectionStudentPetitions = false;
            canViewSectionFacultyConsents = false;
            canViewSectionAddAuthorizations = false;
            canViewSectionWaitlists = false;
            canCreateSectionBooks = false;
            canCreateSectionAddAuthorization = false;
            canCreateSectionGrading = false;
            canCreateSectionPrerequisiteWaiver = false;
            canCreateSectionStudentPetition = false;
            canCreateSectionFacultyConsent = false;
            canSearchStudents = false;
            canCreateSectionAttendance = false;
            canCreateSectionCensus = false;
            canCreateSectionDropRoster = false;
        }

        /// <summary>
        /// Creates a new <see cref="DepartmentalOversightPermissions"/> object and sets permissions based on the supplied permission codes
        /// </summary>
        /// <param name="permissionCodes">Permission codes</param>
        public DepartmentalOversightPermissions(IEnumerable<string> permissionCodes)
        {
            if (permissionCodes == null)
            {
                throw new ArgumentNullException("permissionCodes", "Collection of permission codes cannot be null when building departmental oversight permissions.");
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionRoster))
            {
                canViewSectionRoster = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionAttendance))
            {
                canViewSectionAttendance = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionDropRoster))
            {
                canViewSectionDropRoster = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionCensus))
            {
                canViewSectionCensus = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionGrading))
            {
                canViewSectionGrading = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionBooks))
            {
                canViewSectionBooks = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionPrerequisiteWaiver))
            {
                canViewSectionPrerequisiteWaiver = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionStudentPetitions))
            {
                canViewSectionStudentPetitions = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionFacultyConsents))
            {
                canViewSectionFacultyConsents = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionAddAuthorizations))
            {
                canViewSectionAddAuthorizations = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewSectionWaitlists))
            {
                canViewSectionWaitlists = true;
            }

            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionBooks))
            {
                canCreateSectionBooks = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionAddAuthorization))
            {
                canCreateSectionAddAuthorization = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionGrading))
            {
                canCreateSectionGrading = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionPrerequisiteWaiver))
            {
                canCreateSectionPrerequisiteWaiver = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionStudentPetition))
            {
                canCreateSectionStudentPetition = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionFacultyConsent))
            {
                canCreateSectionFacultyConsent = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.ViewStudentInformation))
            {
                canSearchStudents = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionAttendance))
            {
                canCreateSectionAttendance = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionCensus))
            {
                canCreateSectionCensus = true;
            }
            if (permissionCodes.Contains(DepartmentalOversightPermissionCodes.CreateSectionDropRoster))
            {
                canCreateSectionDropRoster = true;
            }

        }
    }
}
