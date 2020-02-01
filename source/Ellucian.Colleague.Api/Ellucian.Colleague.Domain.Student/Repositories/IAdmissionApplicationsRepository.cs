﻿//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IAdmissionApplicationsRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache);
        Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplications2Async(int offset, int limit, string applicant = "", string academicPeriod = "", string personFilter = "", string[] filterPersonIds = null, bool bypassCache = false);
        Task<AdmissionApplication> GetAdmissionApplicationByIdAsync(string guid);
        Task<AdmissionApplication> GetAdmissionApplicationById2Async(string guid);
        //Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys);
        Task<Dictionary<string, string>> GetStaffOperIdsAsync(List<string> ownerIds);
        Task<Dictionary<string, string>> GetAdmissionApplicationGuidDictionary(IEnumerable<string> applicationIds);
        Task<string> GetRecordKeyAsync(string guid);
        Task<AdmissionApplication> UpdateAdmissionApplicationAsync(AdmissionApplication admissionApplicationEntity);
        Task<AdmissionApplication> CreateAdmissionApplicationAsync(AdmissionApplication admissionApplicationEntity);
        Task<AdmissionApplication> GetAdmissionApplicationSubmissionByIdAsync(string guid, bool bypassCache = false);
        Task<AdmissionApplication> CreateAdmissionApplicationSubmissionAsync(AdmissionApplication entity);
        Task<AdmissionApplication> UpdateAdmissionApplicationSubmissionAsync(AdmissionApplication entity);
        Task<string> GetDefaultApplicationStatus();
    }
}
