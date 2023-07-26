// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationRequest
    {
        /// <summary>
        /// ID of student that the registration actions will run against
        /// </summary>
        public string StudentId { get; set; }
        
        /// <summary>
        /// List of section registration actions (add, drop, etc.) to perform, and/or section registration attributes to update for this student.
        /// </summary>
        public List<SectionRegistration> Sections { get; set; }

        /// <summary>
        /// Should a student and student enrollment be created? (Ethos only)
        /// </summary>
        public bool CreateStudentFlag { get; set; }

        /// <summary>
        /// If true run registration without validations (WebAPI only)
        /// </summary>
        public bool SkipValidationRegistration { get; set; }

        /// <summary>
        /// If true run registration validations only but do not actually register (WebAPI only)
        /// </summary>
        public bool ValidationOnlyRegistration { get; set; }

        /// <summary>
        /// If true disallow registering into a section the student already is/was registered in. 
        /// A setting to preserve legacy behavior that allowed this.
        /// </summary>
        public bool PreventDoubleRegistrationIntoSection { get; set; }

        /// <summary>
        /// If true do not automatically add missing coreq sections.
        /// </summary>
        public bool DoNotAutoAddMissingCoreqSections { get; set; }

        /// <summary>
        /// If true, store validation information when ValidationOnlyRegistration is true and return a token
        /// in the response entity which can be used to apply that data with a skip validation registration.
        /// This flag has no effect when ValidationOnlyRegistration is not true.
        /// </summary>
        public bool RecordValidationData { get; set; }

        /// <summary>
        /// For use with a skip-validation registration request. 
        /// Optional token returned by a previous call to the registration validation check only API.
        /// Pass this token with the same section registration actions that were sent to the validation only API. 
        /// The system will retrieve information stored with the validation only API and apply it to the execution 
        /// of the registration actions.
        /// Do not pass when the API is not connected to a previous call to the registration validation check only API.
        /// </summary>
        public string ValidationToken { get; set; }

        /// <summary>
        /// When true indicates a validation-only or skip-validation cross-registration of a home student into a section 
        /// delivered by a different school.
        /// Only used with the register-validation-only and register-skip-validation endpoints.
        /// </summary>
        public bool CrossRegHomeStudent { get; set; }

        /// <summary>
        /// When true indicates a skip-validation cross-registration of a visiting student into a section delivered by 
        /// this school.
        /// Only used with the register-skip-validation endpoints.
        /// </summary>
        public bool CrossRegVisitingStudent { get; set; }

        /// <summary>
        /// When true registration skips capacity checking. The caller is handling capacity checking elsewhere -- typically with a direct
        /// call to the seat service.
        /// This flag controls capacity checking specifically while SkipValidationRegistration true skips validations generally including
        /// capacity checking.
        /// </summary>
        public bool SkipCapacityCheck { get; set; }

        public RegistrationRequest(string studentId, IEnumerable<SectionRegistration> sections)
        {
            StudentId = studentId;
            Sections = sections.ToList();

            // Default values
            // The defaults are the legacy behavior before each of these flags were added to registration.
            SkipValidationRegistration = false;
            ValidationOnlyRegistration = false;
            PreventDoubleRegistrationIntoSection = false;
            DoNotAutoAddMissingCoreqSections = false;
            RecordValidationData = false;
            CrossRegHomeStudent = false;
            CrossRegVisitingStudent = false;
            SkipCapacityCheck = false;
        }
    }
}
