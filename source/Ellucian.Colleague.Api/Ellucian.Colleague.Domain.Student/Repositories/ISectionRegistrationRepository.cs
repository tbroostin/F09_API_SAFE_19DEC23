// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ISectionRegistrationRepository : IEthosExtended
    {
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request);

        Task<SectionRegistrationResponse> GetAsync(string guid);
        Task<SectionRegistrationResponse> UpdateAsync(SectionRegistrationRequest request, string guid, string personId, string sectionId, string statusId);
        Task<string> GetSectionRegistrationIdFromGuidAsync(string guid);
        Task<string> GetGradeGuidFromIdAsync(string id);
        Task<SectionRegistrationResponse> UpdateGradesAsync(SectionRegistrationResponse response, SectionRegistrationRequest request);

        //Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrationsAsync(int offset, int limit);
        Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrationsAsync(int offset, int limit, string sectionId, string personId);
        //V7
        Task<SectionRegistrationResponse> Update2Async(SectionRegistrationRequest request, string guid, string personId, string sectionId, string statusCode);
        Task<bool> CheckStuAcadCredRecord(string id);
    }
}
