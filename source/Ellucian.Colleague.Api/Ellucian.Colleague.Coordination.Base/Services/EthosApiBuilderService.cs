// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EthosApiBuilderService : BaseCoordinationService, IEthosApiBuilderService
    {
        private readonly IEthosApiBuilderRepository _ethosApiBuilderRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;

        /// <summary>
        /// Constructor for EthosApiBuilderService
        /// </summary>
        /// <param name="extendedDataRepository">extendedDataRepository</param>
        /// <param name="personRepository">personRepository</param>
        /// <param name="logger">logger</param>
        public EthosApiBuilderService(IAdapterRegistry adapterRegistry, 
            IEthosApiBuilderRepository extendedDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _ethosApiBuilderRepository = extendedDataRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }

        #region GET methods

        /// <summary>
        /// Gets all extended datas based on paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>> GetEthosApiBuilderAsync(int offset, int limit, string resourceName, Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter> filterDictionary, bool bypassCache)
        {
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);
            CheckUserEthosApiBuilderViewPermissions(configuration);

            try
            {
                List<Dtos.EthosApiBuilder> extendedDataDtos = new List<Dtos.EthosApiBuilder>();
                var filterDefinitions = ConvertFilterDictionary(filterDictionary);
                if ((filterDictionary != null && filterDictionary.Any()) && (filterDefinitions == null || !filterDefinitions.Any() || (filterDictionary.Count() != filterDefinitions.Count())))
                {
                    return new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(extendedDataDtos, 0);
                }
                
                Tuple<IEnumerable<EthosApiBuilder>, int> extendedDataEntities = await _ethosApiBuilderRepository.GetEthosApiBuilderAsync(offset, limit, configuration, filterDefinitions, bypassCache);

                if (extendedDataEntities != null && extendedDataEntities.Item2 > 0)
                {
                    foreach (var extendedDataEntity in extendedDataEntities.Item1)
                    {
                        Dtos.EthosApiBuilder extendedDataDto = ConvertEthosApiBuilderEntityToDtoAsync(extendedDataEntity, bypassCache);
                        extendedDataDtos.Add(extendedDataDto);
                    }
                }
                return extendedDataDtos.Any() ? new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(extendedDataDtos, extendedDataEntities.Item2) :
                                              new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(extendedDataDtos, 0);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Validation.Exception");
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Gets extended data record by extended data id
        /// </summary>
        /// <param name="id">the guid for extended data record</param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        public async Task<Dtos.EthosApiBuilder> GetEthosApiBuilderByIdAsync(string id, string resourceName)
        {
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);
            CheckUserEthosApiBuilderViewPermissions(configuration);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Must provide an id for the {0} resource", configuration.ResourceName));
            }
            try
            {
                EthosApiBuilder extendedDataEntity = await _ethosApiBuilderRepository.GetEthosApiBuilderByIdAsync(id, configuration);

                if (extendedDataEntity == null)
                {
                    throw new KeyNotFoundException(string.Format("{0} id {1} not found.", configuration.ResourceName, id));
                }

                Dtos.EthosApiBuilder extendedDataDto = ConvertEthosApiBuilderEntityToDtoAsync(extendedDataEntity, false);
                return extendedDataDto;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Validation.Exception");
                throw IntegrationApiException;
            }
        }

        public async Task<string> GetRecordIdFromGuidAsync(string guid, string entityName, string secondaryKey = "")
        {
            string recordKey = string.Empty;
            var guidInfo = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(guid);
            if (guidInfo != null && guidInfo.Entity == entityName)
            {
                recordKey = guidInfo.PrimaryKey;
                if (!string.IsNullOrEmpty(secondaryKey) && !string.IsNullOrEmpty(guidInfo.SecondaryKey))
                {
                    recordKey = guidInfo.SecondaryKey;
                }
            }
            return recordKey;
        }

        public async Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "")
        {
            return await _ethosApiBuilderRepository.GetRecordIdFromTranslationAsync(sourceData, entityName, sourceColumn, tableName);
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person's guardian.
        /// </summary>
        private void CheckUserEthosApiBuilderViewPermissions(EthosApiConfiguration configuration)
        {
            // access is ok if the current user has the view or update permission
            bool userHasPermission = false;
            foreach (var method in configuration.HttpMethods)
            {
                if (string.IsNullOrEmpty(method.Permission) && method.Method == "GET")
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "GET" || method.Method == "PUT" || method.Method == "POST")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }
            
            if (!userHasPermission)
            {
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' is not authorized to view {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User is not authorized to view {0}.", configuration.ResourceName));
            }
        }

        #endregion

        #region PUT Method
        /// <summary>
        /// Updates existing extended data record
        /// </summary>
        /// <param name="id">the guid for extended data record</param>
        /// <param name="extendedData">Dtos.EthosApiBuilder</param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        public async Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync(string id, Dtos.EthosApiBuilder extendedData, string resourceName)
        {
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);
            CheckUserEthosApiBuilderUpdatePermissions(configuration);

            if (string.IsNullOrEmpty(id))
            {
                id = extendedData.Id;
            }
            if (extendedData == null || string.IsNullOrEmpty(extendedData.Id))
            {
                throw new KeyNotFoundException("person id is a required property for extendedData");
            }

            var extendedDataLookUpResult = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(id);
            var personLookUpResult = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(extendedData.Id);          

            if (personLookUpResult == null)
            {
                throw new KeyNotFoundException("Person id associated to id " + extendedData.Id + " not found.");
            }

            if (extendedDataLookUpResult != null && !extendedDataLookUpResult.PrimaryKey.Equals(personLookUpResult.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("The person id is for a different person than the id of the extendedData.");
            }

            if ((extendedDataLookUpResult != null && string.IsNullOrEmpty(extendedDataLookUpResult.PrimaryKey)) || string.IsNullOrEmpty(personLookUpResult.PrimaryKey))
            {
                throw new KeyNotFoundException("Person id or extendedData id were not found.");
            }

            _ethosApiBuilderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            EthosApiBuilder extendedDataRequest = ConvertEthosApiBuilderDtoToRequestAsync(personLookUpResult.PrimaryKey, extendedData);

            EthosApiBuilder extendedDataResponse = await _ethosApiBuilderRepository.UpdateEthosApiBuilderAsync(extendedDataRequest, configuration);

            Dtos.EthosApiBuilder extendedDataDto = await GetEthosApiBuilderByIdAsync(extendedDataResponse.Guid, configuration.ResourceName);

            return extendedDataDto;
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a person's guardian.
        /// </summary>
        private void CheckUserEthosApiBuilderUpdatePermissions(EthosApiConfiguration configuration)
        {
            // access is ok if the current user has the view or update permission
            bool userHasPermission = false;
            foreach (var method in configuration.HttpMethods)
            {
                if (string.IsNullOrEmpty(method.Permission) && method.Method == "PUT")
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "PUT")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }

            if (!userHasPermission)
            {
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' is not authorized to update {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User is not authorized to update {0}.", configuration.ResourceName));
            }
        }

        #endregion

        #region Convert Methods
        /// <summary>
        /// Converts extended data dto to extended data request
        /// </summary>
        /// <param name="entityId">entityId</param>
        /// <param name="extendedData">Dtos.EthosApiBuilder</param>
        /// <returns>EthosApiBuilderRequest</returns>
        private EthosApiBuilder ConvertEthosApiBuilderDtoToRequestAsync(string entityId, Dtos.EthosApiBuilder extendedData)
        {

            var extendedDataRequest = new EthosApiBuilder(extendedData.Id, entityId, "");        

            return extendedDataRequest;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="extendedDataEntity">EthosApiBuilder</param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        private Dtos.EthosApiBuilder ConvertEthosApiBuilderEntityToDtoAsync(EthosApiBuilder extendedDataEntity, bool bypassCache)
        {
            Dtos.EthosApiBuilder extendedDataDto = new Dtos.EthosApiBuilder();
            extendedDataDto.Id = extendedDataEntity.Guid;
            
            return extendedDataDto;
        }

        private Dictionary<string, EthosExtensibleDataFilter> ConvertFilterDictionary(Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter> filterDictionary)
        {
            var returnDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
            foreach (var dictItem in filterDictionary)
            {
                var dictKey = dictItem.Key;
                var filterRow = dictItem.Value;
                if (!string.IsNullOrEmpty(dictKey) && filterRow != null && filterRow.FilterValue != null && filterRow.FilterValue.Any())
                {
                    string jsonPropertyType = filterRow.JsonPropertyType.ToString();

                    var row = new EthosExtensibleDataFilter(filterRow.ColleagueColumnName, filterRow.ColleagueFileName,
                        filterRow.JsonTitle, filterRow.JsonPath, jsonPropertyType, filterRow.FilterValue)
                    {
                        GuidColumnName = filterRow.GuidColumnName,
                        GuidDatabaseUsageType = filterRow.GuidDatabaseUsageType,
                        GuidFileName = filterRow.GuidFileName,
                        SavingField = filterRow.SavingField,
                        SavingOption = filterRow.SavingOption,
                        SelectColumnName = filterRow.SelectColumnName,
                        SelectFileName = filterRow.SelectFileName,
                        SelectSubroutineName = filterRow.SelectSubroutineName,
                        SelectParagraph = filterRow.SelectParagraph,
                        TransColumnName = filterRow.TransColumnName,
                        TransFileName = filterRow.TransFileName,
                        TransTableName = filterRow.TransTableName,
                        SelectRules = filterRow.SelectRules,
                        SelectionCriteria = new List<EthosApiSelectCriteria>(),
                        SortColumns = new List<EthosApiSortCriteria>(),
                        Enumerations = new List<EthosApiEnumerations>()
                    };

                    foreach (var selectionCriteria in filterRow.SelectionCriteria)
                    {
                        row.SelectionCriteria.Add(new EthosApiSelectCriteria(selectionCriteria.SelectConnector,
                            selectionCriteria.SelectColumn,
                            selectionCriteria.SelectOper,
                            selectionCriteria.SelectValue)
                        );
                    }

                    foreach (var sortCriteria in filterRow.SortColumns)
                    {
                        row.SortColumns.Add(new EthosApiSortCriteria(sortCriteria.SortColumn, sortCriteria.SortSequence));
                    }

                    foreach (var enums in filterRow.Enumerations)
                    {
                        row.Enumerations.Add(new EthosApiEnumerations(enums.EnumerationValue, enums.ColleagueValue));
                    }

                    if (!returnDictionary.ContainsKey(dictKey))
                    {
                        returnDictionary.Add(dictKey, row);
                    }
                }
            }

            return returnDictionary;
        }

        #endregion
    }
}