// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for AdmissionApplicationSupportingItems Repository
    /// </summary>
    public interface IAdmissionApplicationSupportingItemsRepository : IEthosExtended
    {

        Task<KeyValuePair<string, GuidLookupResult>> GetAdmissionApplicationSupportingItemsIdFromGuidAsync(string guid);

        Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid);

        Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> GetAdmissionApplicationSupportingItemsByIdAsync(string id, string corresReceivedCode, DateTime? corresReceivedAssignDate, string corresReceivedInstance);

        Task<Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>> GetAdmissionApplicationSupportingItemsAsync(int offset, int limit, bool bypassCache = false);

        Task<string> GetGuidFromIdAsync(string entity, string id, string secondaryField = "", string secondaryKey = "");

        Task<string> GetIdFromGuidAsync(string id);

        Task<string> GetPersonIdFromApplicationIdAsync(string id);

        Task<string> GetApplicationIdFromGuidAsync(string guid);

        Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> UpdateAdmissionApplicationSupportingItemsAsync(Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplictaionSupportingItem);

        Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> CreateAdmissionApplicationSupportingItemsAsync(Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplictaionSupportingItem);
    }
}
