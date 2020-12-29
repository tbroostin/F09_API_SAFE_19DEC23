/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IInstitutionJobsRepository : IEthosExtended
    {
        Task<string> GetInstitutionJobsIdFromGuidAsync(string guid);

        Task<InstitutionJobs> GetInstitutionJobsByGuidAsync(string guid);

        Task<Tuple<IEnumerable<InstitutionJobs>, int>> GetInstitutionJobsAsync(int offset, int limit, string personCode = "",
            string employerCode = "", string positionCode = "", string departmentCode = "", string convertedStartOn = "", string convertedEndOn = "",
            string status = "", string classificationCode = "", string preference = "", bool bypassCache = false, Dictionary<string, string> filterQualifiers = null);

        Task<InstitutionJobs> UpdateInstitutionJobsAsync(InstitutionJobs institutionJobsEntity);

        Task<InstitutionJobs> CreateInstitutionJobsAsync(InstitutionJobs institutionJobsEntity);

        Task<string> GetInstitutionEmployerGuidAsync();
    }
}