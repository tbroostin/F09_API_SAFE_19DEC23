//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionApplication
    {
        public AdmissionApplication(string guid, string applicationKey)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Admission application guid is required.");
            }
            if (string.IsNullOrEmpty(applicationKey))
            {
                throw new ArgumentNullException("Admission application record key is required.");
            }

            Guid = guid;
            ApplicationRecordKey = applicationKey;
        }

        /// <summary>
        /// For POST.
        /// </summary>
        /// <param name="applicationKey"></param>
        public AdmissionApplication(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Admission application guid is required.");
            }

            Guid = guid;
        }
        public string ApplicantPersonId { get; set; }

        public string Guid { get; private set; }

        public string ApplicationRecordKey { get; private set; }

        public string ApplicationOwnerId { get; set; }

        public string ApplicationNo { get; set; }

        public string ApplicationStartTerm { get; set; }

        public List<AdmissionApplicationStatus> AdmissionApplicationStatuses { get; set; }

        public string ApplicationSource { get; set; }

        public string PersonSource { get; set; }

        public string ApplicationAdmissionsRep { get; set; }

        public string ApplicationAdmitStatus { get; set; }

        public List<string> ApplicationLocations { get; set; }

        public string ApplicationResidencyStatus { get; set; }

        public string ApplicationStudentLoadIntent { get; set; }

        public string ApplicationAcadProgram { get; set; }

        public string ApplicationAcadProgramGuid { get; set; }

        public List<string> ApplicationStprAcadPrograms { get; set; }

        public string ApplicationWithdrawReason { get; set; }

        public DateTime? ApplicationWithdrawDate { get; set; }

        public string ApplicationAttendedInstead { get; set; }

        public string ApplicationComments { get; set; }

        public string ApplicationSchool { get; set; }

        public string ApplicationIntgType { get; set; }
        public DateTime? AppliedOn { get; set; }
        public DateTime? WithdrawnOn { get; set; }
        public DateTime? AdmittedOn { get; set; }
        public DateTime? MatriculatedOn { get; set; }
        public string ApplicationAcadLevel { get; set; }
        public List<ApplicationDiscipline> ApplicationDisciplines { get; set; }
        public List<string> ApplicationCredentials { get; set; }
        public string ApplicationProgramOwner { get; set; }
        public string EducationalGoal { get; set; }
        public List<string> CareerGoals { get; set; }
        public List<string> Influences { get; set; }
    }
}
