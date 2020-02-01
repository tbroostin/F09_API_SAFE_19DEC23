/*Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LanguageRepository : BaseColleagueRepository, ILanguageRepository, IEthosExtended
    {
        public LanguageRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;           
        }

        /// <summary>
        /// Get a collection of Languages
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Languages</returns>
        public async Task<IEnumerable<Language2>> GetLanguagesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<Language2>("CORE", "LANGUAGES",
                (cl, g) => new Language2(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember))
                { IsoCode = cl.ValActionCode3AssocMember }, bypassCache: ignoreCache);           
        }


        public async Task<Language2> GetLanguageByGuidAsync(string id, bool ignoreCache)
        {
            return  (await GetLanguagesAsync(ignoreCache)).Where(r => r.Guid == id).FirstOrDefault();
        }
      

        public async Task<Language2> UpdateLanguageAsync(Language2 language)
        {
            var extendedDataTuple = GetEthosExtendedDataLists();
            var updateRequest = new UpdateLanguageRequest();
            updateRequest.Language = language.Code;
            updateRequest.IsoCode = language.IsoCode;
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // Submit the ISO code change
            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateLanguageRequest, UpdateLanguageResponse>(updateRequest);

            if (updateResponse.LanguageErrors != null && updateResponse.LanguageErrors.Count > 0)
            {
                var exception = new RepositoryException();
                // Register repository errors and throw an exception
                exception.AddErrors(updateResponse.LanguageErrors.ConvertAll(x => (new RepositoryError(x.ErrorCode, x.ErrorMsg))));
                throw exception;
            }

            return await GetLanguageByGuidAsync(language.Guid, true);
        }
    }
}