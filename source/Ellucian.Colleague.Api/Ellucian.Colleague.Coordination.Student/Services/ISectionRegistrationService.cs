// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface ISectionRegistrationService : IBaseService
    {
        Task<Dtos.SectionRegistration> UpdateRegistrationAsync(Dtos.SectionRegistration registrationDto);

        Task<Dtos.SectionRegistration2> GetSectionRegistrationAsync(string guid);
        Task<Dtos.SectionRegistration2> CreateSectionRegistrationAsync(Dtos.SectionRegistration2 registrationDto);
        Task<Dtos.SectionRegistration2> UpdateSectionRegistrationAsync(string guid, Dtos.SectionRegistration2 registrationDto);
        Task<Tuple<IEnumerable<Dtos.SectionRegistration2>, int>> GetSectionRegistrationsAsync(int offset, int limit, string section, string registrant);
        //V7 changes
        Task<Dtos.SectionRegistration3> GetSectionRegistration2Async(string guid);
        Task<Tuple<IEnumerable<Dtos.SectionRegistration3>, int>> GetSectionRegistrations2Async(int offset, int limit, string section, string registrant);
        Task<Dtos.SectionRegistration3> UpdateSectionRegistration2Async(string guid, Dtos.SectionRegistration3 sectionRegistration);
        Task<Dtos.SectionRegistration3> CreateSectionRegistration2Async(Dtos.SectionRegistration3 sectionRegistration);
    }
}
