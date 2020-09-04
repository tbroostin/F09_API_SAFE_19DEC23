// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IDemographicService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.CitizenshipStatus>> GetCitizenshipStatusesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.CitizenshipStatus> GetCitizenshipStatusByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Ethnicity2>> GetEthnicities2Async(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Ethnicity2> GetEthnicityById2Async(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.GeographicAreaType>> GetGeographicAreaTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.GeographicAreaType> GetGeographicAreaTypeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.IdentityDocumentType>> GetIdentityDocumentTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.IdentityDocumentType> GetIdentityDocumentTypeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonFilter>> GetPersonFiltersAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.PersonFilter> GetPersonFilterByGuidAsync(string guid);
        
        Task<IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus>> GetPrivacyStatusesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.PrivacyStatus> GetPrivacyStatusByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Race>> GetRacesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Race> GetRaceByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Race2>> GetRaces2Async(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Race2> GetRaceById2Async(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Religion>> GetReligionsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Religion> GetReligionByIdAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.SocialMediaType>> GetSocialMediaTypesAsync(bool bypassCache);
        Task<Dtos.SocialMediaType> GetSocialMediaTypeByIdAsync(string id);
        
        Task<IEnumerable<Ellucian.Colleague.Dtos.SourceContext>> GetSourceContextsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.SourceContext> GetSourceContextsByIdAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.VisaType>> GetVisaTypesAsync(bool bypassCache);
        Task<Dtos.VisaType> GetVisaTypeByIdAsync(string id);   

        Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus>> GetMaritalStatusesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.MaritalStatus> GetMaritalStatusByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus2>> GetMaritalStatuses2Async(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.MaritalStatus2> GetMaritalStatusById2Async(string id);        
    }
}