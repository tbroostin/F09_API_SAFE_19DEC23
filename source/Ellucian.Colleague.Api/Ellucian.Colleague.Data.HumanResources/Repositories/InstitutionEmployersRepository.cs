//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{

    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class InstitutionEmployersRepository : BaseColleagueRepository, IInstitutionEmployersRepository
    {
        private readonly int _bulkReadSize;
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        public InstitutionEmployersRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get InstitutionEmployers by a guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>InstitutionEmployers entity objects</returns>
        public async Task<InstitutionEmployers> GetInstitutionEmployerByGuidAsync(string guid)
        {
            var humanResourcesInstitutionEmployerEntity = await GetGuidValcodeAsync<HumanResourcesInstitutionEmployer>("HR", "INTG.INST.EMPLOYER",
                (cl, g) => new HumanResourcesInstitutionEmployer(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: true);

            if (humanResourcesInstitutionEmployerEntity != null && humanResourcesInstitutionEmployerEntity.Any())
            {
                var thisEntity = humanResourcesInstitutionEmployerEntity.FirstOrDefault();
                if ((thisEntity != null && thisEntity.Guid == guid))
                {
                    var employer =  await BuildInstitutionEmployerAsync(guid);
                    employer.Code = thisEntity.Code;
                    return employer;
                }
                else
                {
                    var errorMessage = "No institution employer was found for guid " + guid;
                    logger.Error(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
            }
            else
            {
                var errorMessage = "No institution employer was found for guid " + guid;
                logger.Error(errorMessage);
                throw new KeyNotFoundException(errorMessage);
            }
        }

        /// <summary>
        /// GetInstitutionEmployersAsync
        /// </summary>
        /// <returns>InstitutionEmployers list</returns>
        public async Task<IEnumerable<InstitutionEmployers>> GetInstitutionEmployersAsync(bool bypassCache = false)
        {
            var humanResourcesInstitutionEmployerEntity = await GetGuidValcodeAsync<HumanResourcesInstitutionEmployer>("HR", "INTG.INST.EMPLOYER",
    (cl, g) => new HumanResourcesInstitutionEmployer(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: true);

            if (humanResourcesInstitutionEmployerEntity != null && humanResourcesInstitutionEmployerEntity.Any())
            {
                var thisEntity = humanResourcesInstitutionEmployerEntity.FirstOrDefault();
                var thisGuid = thisEntity.Guid;
                if (thisEntity != null && thisEntity.Code == "INST")
                {
                    if (bypassCache)
                    {
                        var institutionEmployersEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers>();
                        var institutionEmployer = await BuildInstitutionEmployerAsync(thisGuid);
                        institutionEmployer.Code = thisEntity.Code;
                        institutionEmployersEntities.Add(institutionEmployer);
                        return institutionEmployersEntities;
                    }
                    else
                    {
                        return await GetOrAddToCacheAsync<IEnumerable<InstitutionEmployers>>("AllInstitutionEmployers",
                            async () =>
                            {
                                var institutionEmployersEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers>();
                                var institutionEmployer = await BuildInstitutionEmployerAsync(thisGuid);
                                institutionEmployer.Code = thisEntity.Code;
                                institutionEmployersEntities.Add(institutionEmployer);
                                return institutionEmployersEntities;
                            }
                        );
                    }
                }
                else
                {
                    var errorMessage = "No institution employer was found.  Expecting only 'INST' in INTG.INST.EMPLOIYER";
                    logger.Error(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
            }
            else
            {
                var errorMessage = "No guid found for institution-employers";
                logger.Error(errorMessage);
                throw new KeyNotFoundException(errorMessage);
            }
        }

        private async Task<InstitutionEmployers> BuildInstitutionEmployerAsync(string guid)
        {
            var hostInstitution = string.Empty;
            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                if (!string.IsNullOrEmpty(coreDefaultData.DefaultHostCorpId))
                {
                    hostInstitution = coreDefaultData.DefaultHostCorpId;
                    var institutionEmployers = new InstitutionEmployers(guid, hostInstitution);
                    var preferredName = string.Empty;
                    var personData = DataReader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", hostInstitution);
                    if (personData != null)
                    {
                        institutionEmployers.PreferredName = personData.PreferredName;
                    }
                    if (!string.IsNullOrEmpty(personData.PreferredAddress))
                    {
                        var preferredAddress = personData.PreferredAddress;
                        var addressData = DataReader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", preferredAddress);
                        if (addressData != null)
                        {
                            institutionEmployers.AddressLines = addressData.AddressLines;
                            institutionEmployers.City = addressData.City;
                            institutionEmployers.State = addressData.State;
                            institutionEmployers.Country = addressData.Country;
                            if (string.IsNullOrEmpty(institutionEmployers.Country))
                            {
                                institutionEmployers.Country = await GetHostCountryAsync();
                            }
                            institutionEmployers.PostalCode = addressData.Zip;
                            institutionEmployers.PhoneNumber = addressData.AddressPhones.FirstOrDefault();
                        }
                    }
                    return institutionEmployers;
                }
                else
                {
                    var errorMessage = "No host organization defined in Colleague";
                    logger.Error(errorMessage);
                    var exception = new RepositoryException(errorMessage);
                    exception.AddError(new RepositoryError("invalid.host.organization", errorMessage));
                    throw exception;
                }
            }
            else
            {
                var errorMessage = "Unable to access DEFAULTS in Colleague";
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.host.organization", errorMessage));
                throw exception;
            }
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
                () =>
                {
                    var coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get Host Country
        /// </summary>
        /// <returns>string</returns>
        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }
    }
}