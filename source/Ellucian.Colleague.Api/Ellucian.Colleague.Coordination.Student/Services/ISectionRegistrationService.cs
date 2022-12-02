// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;

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

        //V16.0.0
        Task<Dtos.SectionRegistration4> GetSectionRegistrationByGuid3Async(string guid, bool bypassCache = true);
        Task<Tuple<IEnumerable<Dtos.SectionRegistration4>, int>> GetSectionRegistrations3Async(int offset, int limit, Dtos.SectionRegistration4 criteria, 
            string academicPeriod, string sectionInstructor, Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter registrationStatusesByAcademicPeriodFilter, bool bypassCache = false);
        Task<Dtos.SectionRegistration4> CreateSectionRegistration3Async(Dtos.SectionRegistration4 registrationDto);
        Task<Dtos.SectionRegistration4> UpdateSectionRegistration3Async(string guid, Dtos.SectionRegistration4 registrationDto, bool updateRegistraiton = true);
        
        //Section Registration Grade Options V1.0.0
        Task<Tuple<IEnumerable<SectionRegistrationsGradeOptions>, int>> GetSectionRegistrationsGradeOptionsAsync(int offset, int limit, SectionRegistrationsGradeOptions criteriaObj, bool bypassCache);
        Task<SectionRegistrationsGradeOptions> GetSectionRegistrationsGradeOptionsByGuidAsync(string guid, bool bypassCache = false);

        Task<Dtos.SectionRegistrations> CheckSectionRegistrations(Dtos.SectionRegistrations registrationDto);
    }
}