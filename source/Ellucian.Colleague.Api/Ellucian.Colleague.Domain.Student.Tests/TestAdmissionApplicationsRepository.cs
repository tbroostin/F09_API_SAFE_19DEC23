// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAdmissionApplicationsRepository : IAdmissionApplicationsRepository
    {
        public List<AdmissionApplication> applicationEntities = new List<AdmissionApplication>();

        private string[,] applicationData = {
            //                                                                             ADMIT  INST                          APPL    START
            //GUID                                   ID   APPLICANT  PROGRAM    REP        STATUS ATTEND    COMMENT  LOCATIONS  NO      TERM      LOAD  STATUS   ADMIT SOURCE WITHDRAW RESIDENCY
            {"1c5bbcbc-80e3-8151-4042-db9893ac337a", "1", "0003748", "BA-MATH", "0003849", "FR", "0004899", "",      "DT",      "3430", "2017/SP", "F", "MS,AD", "AD", "EDX", "",   "IN"},
            {"138951cc-459e-7912-a065-0471a7a2c644", "2", "0006374", "AA-NURS", "0003849", "GD", "",        "",      "MC,DT",   "2293", "2017/FA", "P", "RE",    "",    "SV",  "AC", "IN"},
            {"fbdfac70-88a0-69a1-4362-62ea5cdafd69", "3", "0037487", "MA-LAW",  "",        "ND", "",        "",      "",        "3345", "2017/SP", "F", "AC,AP", "AD",  "WI",  "",   "CC"},
            {"d328fd10-9c90-b1a3-4a2f-543bc099be37", "4", "2003894", "MS-SCI",  "",        "TR", "",        "",      "",        "4490", "2017/FA", "F", "AC,AD", "",    "EDX", "FP", "RE"}
        };

        public Dictionary<string, string> EthosExtendedDataDictionary
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Populate()
        {
            // There are 17 fields for each application in the array
            var items = applicationData.Length / 17;

            for (int x = 0; x < items; x++)
            {
                var application = new AdmissionApplication(applicationData[x, 0], applicationData[x, 1]);
                application.ApplicantPersonId = applicationData[x, 2];
                application.ApplicationAcadProgram = applicationData[x, 3];
                application.ApplicationAdmissionsRep = applicationData[x, 4];
                application.ApplicationAdmitStatus = applicationData[x, 5];
                application.ApplicationAttendedInstead = applicationData[x, 6];
                application.ApplicationComments = applicationData[x, 7];
                var aLocations = new List<string>();
                List<string> locations = applicationData[x, 8].Split(',').ToList();
                foreach (var d in locations)
                {
                    aLocations.Add(d);
                }
                application.ApplicationLocations = aLocations;
                application.ApplicationNo = applicationData[x, 9];
                application.ApplicationStartTerm = applicationData[x, 10];
                application.ApplicationStudentLoadIntent = applicationData[x, 11];
                application.AdmissionApplicationStatuses = new List<AdmissionApplicationStatus>();
                List<string> statuses = applicationData[x, 12].Split(',').ToList();
                foreach (var s in statuses)
                {
                    application.AdmissionApplicationStatuses.Add(new AdmissionApplicationStatus()
                    {
                        ApplicationStatus = s,
                        ApplicationStatusDate = DateTime.Today,
                        ApplicationStatusTime = DateTime.Now
                    });
                }
                application.ApplicationAdmitStatus = applicationData[x, 13];
                application.ApplicationSource = applicationData[x, 14];
                application.ApplicationWithdrawReason = applicationData[x, 15];
                application.ApplicationResidencyStatus = applicationData[x, 16];

                applicationEntities.Add(application);
            }
        }

        public Task<AdmissionApplication> GetAdmissionApplicationByIdAsync(string guid)
        {
            Populate();
            var appl = applicationEntities.FirstOrDefault(p => p.Guid == guid);
            if (appl != null)
            {
                return Task.FromResult(appl);
            }
            throw new KeyNotFoundException(string.Format("Application {0} not found in person", guid));
        }

        public Task<AdmissionApplication> GetAdmissionApplicationById2Async(string guid)
        {
            Populate();
            var appl = applicationEntities.FirstOrDefault(p => p.Guid == guid);
            if (appl != null)
            {
                return Task.FromResult(appl);
            }
            throw new KeyNotFoundException(string.Format("Application {0} not found in person", guid));
        }

        public Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache)
        {
            Populate();
            var totalRecords = applicationEntities.Count();
            return Task.FromResult(new Tuple<IEnumerable<AdmissionApplication>, int>(applicationEntities, totalRecords));
        }

        //public Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplications2Async(int offset, int limit, bool bypassCache)
        //{
        //    Populate();
        //    var totalRecords = applicationEntities.Count();
        //    return Task.FromResult(new Tuple<IEnumerable<AdmissionApplication>, int>(applicationEntities, totalRecords));
        //}

        public Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            return Task.FromResult(dictionary);
        }

        public Task<Dictionary<string, string>> GetStaffOperIdsAsync(List<string> ownerIds)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            return Task.FromResult(dictionary);
        }

        public Task<Dictionary<string, string>> GetAdmissionApplicationGuidDictionary(IEnumerable<string> applicationIds)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRecordKeyAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<AdmissionApplication> UpdateAdmissionApplicationAsync(AdmissionApplication admissionApplicationEntity)
        {
            throw new NotImplementedException();
        }

        public Task<AdmissionApplication> CreateAdmissionApplicationAsync(AdmissionApplication admissionApplicationEntity)
        {
            throw new NotImplementedException();
        }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }
    }
}
