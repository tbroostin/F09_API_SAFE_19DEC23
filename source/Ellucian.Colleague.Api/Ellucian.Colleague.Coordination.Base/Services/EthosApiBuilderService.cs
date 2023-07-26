// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
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
        char _XM = Convert.ToChar(250);

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
            
            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;

            CheckUserEthosApiBuilderViewAllPermissions(configuration);

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
                        Dtos.EthosApiBuilder extendedDataDto = ConvertEthosApiBuilderEntityToDto(extendedDataEntity, bypassCache);
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

            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;
            
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
                    throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", configuration.ResourceName, id));
                }

                Dtos.EthosApiBuilder extendedDataDto = ConvertEthosApiBuilderEntityToDto(extendedDataEntity, false);
                return extendedDataDto;
            }
            catch (KeyNotFoundException ex)
            {
                if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    IntegrationApiExceptionAddError(ex.Message, "Key.Not.Found", httpStatusCode: System.Net.HttpStatusCode.NotFound);
                }
                else
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", httpStatusCode: System.Net.HttpStatusCode.NotFound);
                }
                throw IntegrationApiException;
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
        /// Checks User GET by ID permissions (view) for Business Process API
        /// </summary>
        /// <param name="configuration">Configuration of the Business Process API</param>
        /// <param name="method">Method requested for permissions verification</param>
        /// <returns></returns>
        public async Task<string> GetEthosApiBuilderPermissionsAsync(string resourceName, string method)
        {
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);

            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;

            if (method == "GET" || method == "GET_ID")
            {
                CheckUserEthosApiBuilderViewPermissions(configuration);
            }
            else if (method == "GET_ALL" || method == "POST_QAPI")
            {
                CheckUserEthosApiBuilderViewAllPermissions(configuration);
            }
            else if (method == "PUT")
            {
                CheckUserEthosApiBuilderUpdatePermissions(configuration);
            }
            else if (method == "POST")
            {
                CheckUserEthosApiBuilderCreatePermissions(configuration);
            }
            else if (method == "DELETE")
            {
                CheckUserEthosApiBuilderDeletePermissions(configuration);
            }

            return CurrentUser.PersonId;
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
                if (string.IsNullOrEmpty(method.Permission) && (method.Method == "GET" || method.Method == "PUT" || method.Method == "POST" || method.Method == "GET_ID"))
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "GET" || method.Method == "PUT" || method.Method == "POST" || method.Method == "GET_ID")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }
            
            if (!userHasPermission)
            {
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' does not have permission to view {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User '" + CurrentUser.UserId + "' does not have permission to view {0}.", configuration.ResourceName));
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person's guardian.
        /// </summary>
        private void CheckUserEthosApiBuilderViewAllPermissions(EthosApiConfiguration configuration)
        {
            // access is ok if the current user has the view or update permission
            bool userHasPermission = false;
            foreach (var method in configuration.HttpMethods)
            {
                if (string.IsNullOrEmpty(method.Permission) && (method.Method == "GET" || method.Method == "GET_ALL" || method.Method == "POST_QAPI"))
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "GET" || method.Method == "GET_ALL" || method.Method == "POST_QAPI")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }

            if (!userHasPermission)
            {
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' does not have permission to view {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User '" + CurrentUser.UserId + "' does not have permission to view {0}.", configuration.ResourceName));
            }
        }

        #endregion

        #region PUT Method
        /// <summary>
        /// Updates existing extended data record
        /// </summary>
        /// <param name="id">the guid for extended data record</param>
        /// <param name="extendedData">Dtos.EthosApiBuilder</param>
        /// <param name="ethosResourceRouteInfo"></param>
        /// <param name="filterDictionary"></param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        public async Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync(string id, Dtos.EthosApiBuilder extendedData, Web.Http.EthosExtend.EthosResourceRouteInfo ethosResourceRouteInfo, Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter> filterDictionary)
        {
            var resourceName = ethosResourceRouteInfo.ResourceName;
            var resourceVersionNumber = ethosResourceRouteInfo.ResourceVersionNumber;
            var extendedSchemaId = ethosResourceRouteInfo.ExtendedSchemaResourceId;
            var returnRestrictedFields = ethosResourceRouteInfo.ReturnRestrictedFields;
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);
            var extendedDataConfig = await _configurationRepository.GetExtendedEthosConfigurationByResource(resourceName, resourceVersionNumber, extendedSchemaId);

            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;

            CheckUserEthosApiBuilderUpdatePermissions(configuration);

            if (string.IsNullOrEmpty(id))
            {
                id = extendedData._Id;
            }
            if (extendedData == null || string.IsNullOrEmpty(extendedData._Id))
            {
                throw new KeyNotFoundException(string.Format("id is a required property for {0}", resourceName));
            }

            string primaryKey = id;
            string primaryEntity = configuration.PrimaryEntity;
            if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                primaryKey = UnEncodePrimaryKey(id).Split(_XM)[0];
                string idInBody = extendedData._Id;
                if (!idInBody.Contains("+"))
                {
                    idInBody = _configurationRepository.UnEncodePrimaryKey(extendedData._Id);
                }
                if (idInBody.Contains("+"))
                {
                    idInBody = idInBody.Split('+')[1];
                }
                var idSplit = primaryKey.Split('+');
                if (idSplit.Count() > 1)
                {
                    primaryKey = idSplit[1];
                }
                else
                {
                    primaryKey = idSplit[0];
                }
                if (!primaryKey.Equals(idInBody, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(configuration.ApiType) || !configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new KeyNotFoundException(string.Format("Request Id '{0}' doesn't match request body Id '{2}'.  Invalid Id for {1}: '{2}'", primaryKey, resourceName, extendedData._Id));
                }
            }
            else
            {
                var extendedDataLookUpResult = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(id);
                var personLookUpResult = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(extendedData._Id);

                if (personLookUpResult == null)
                {
                    throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", resourceName, extendedData._Id));
                }

                if (extendedDataLookUpResult != null && !extendedDataLookUpResult.PrimaryKey.Equals(personLookUpResult.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", resourceName, id));
                }

                if (extendedDataLookUpResult != null && !extendedDataLookUpResult.Entity.Equals(configuration.PrimaryGuidFileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException(string.Format("The id is for a different resource than {0}.", resourceName));
                }

                if ((extendedDataLookUpResult != null && string.IsNullOrEmpty(extendedDataLookUpResult.PrimaryKey)) || string.IsNullOrEmpty(personLookUpResult.PrimaryKey))
                {
                    throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", resourceName, id));
                }
                primaryKey = extendedDataLookUpResult.PrimaryKey;
            }

            _ethosApiBuilderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            EthosApiBuilder extendedDataRequest = ConvertEthosApiBuilderDtoToRequest(primaryKey, extendedData, primaryEntity);
            var filterDefinitions = ConvertFilterDictionary(filterDictionary);

            // A Business Process API (Type "T") will return the list of returned fields so that
            // the GET following the PUT or POST isn't required any longer.  Just use the result
            // coming from the PUT/POST operation by setting the service level property.
            if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var responseDictionary = await _ethosApiBuilderRepository.UpdateEthosBusinessProcessApiAsync(extendedDataRequest, configuration, filterDefinitions, returnRestrictedFields, extendedDataConfig);
                    EthosExtendedDataDictionary = responseDictionary.FirstOrDefault().Value;
                    extendedData._Id = responseDictionary.FirstOrDefault().Key;
                    _ethosApiBuilderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                    _configurationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                    SecureDataDefinition = _ethosApiBuilderRepository.GetSecureDataDefinition();
                }
                catch (IntegrationApiException ex)
                {
                    var messages = ex.Errors;
                    IntegrationApiException = new IntegrationApiException();
                    foreach (var message in messages)
                    {
                        var guid = string.IsNullOrEmpty(message.Guid) ? extendedDataRequest.Guid : message.Guid;
                        var primaryId = string.IsNullOrEmpty(message.Id) ? string.Empty : message.Id;
                        IntegrationApiExceptionAddError(message.Message, "Create.Update.Success.Exception", guid, primaryId, System.Net.HttpStatusCode.NotFound);
                    }
                    throw IntegrationApiException;
                }
            }
            else
            {
                EthosApiBuilder extendedDataResponse = await _ethosApiBuilderRepository.UpdateEthosApiBuilderAsync(extendedDataRequest, configuration);
                try
                {
                    Dtos.EthosApiBuilder extendedDataDto = await GetEthosApiBuilderByIdAsync(extendedDataResponse.Guid, configuration.ResourceName);
                    return extendedDataDto;
                }
                catch (IntegrationApiException ex)
                {
                    var messages = ex.Errors;
                    IntegrationApiException = new IntegrationApiException();
                    foreach (var message in messages)
                    {
                        var guid = string.IsNullOrEmpty(message.Guid) ? extendedDataRequest.Guid : message.Guid;
                        var primaryId = string.IsNullOrEmpty(message.Id) ? primaryKey : message.Id;
                        IntegrationApiExceptionAddError(message.Message, "Create.Update.Success.Exception", guid, primaryId, System.Net.HttpStatusCode.NotFound);
                    }
                    throw IntegrationApiException;
                }
            }

            return extendedData;
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
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' does not have permission to update {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User '" + CurrentUser.UserId + "' does not have permission to update {0}.", configuration.ResourceName));
            }
        }

        #endregion

        #region POST Method
        /// <summary>
        /// Updates existing extended data record
        /// </summary>
        /// <param name="id">the guid for extended data record</param>
        /// <param name="extendedData">Dtos.EthosApiBuilder</param>
        /// <param name="ethosResourceRouteInfo"></param>
        /// <param name="filterDictionary"></param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        public async Task<Dtos.EthosApiBuilder> PostEthosApiBuilderAsync(Dtos.EthosApiBuilder extendedData, Web.Http.EthosExtend.EthosResourceRouteInfo ethosResourceRouteInfo, Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter> filterDictionary)
        {
            var resourceName = ethosResourceRouteInfo.ResourceName;
            var resourceVersionNumber = ethosResourceRouteInfo.ResourceVersionNumber;
            var extendedSchemaId = ethosResourceRouteInfo.ExtendedSchemaResourceId;
            var returnRestrictedFields = ethosResourceRouteInfo.ReturnRestrictedFields;
            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);
            var extendedDataConfig = await _configurationRepository.GetExtendedEthosConfigurationByResource(resourceName, resourceVersionNumber, extendedSchemaId);

            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;

            CheckUserEthosApiBuilderCreatePermissions(configuration);

            if (extendedData == null || string.IsNullOrEmpty(extendedData._Id))
            {
                throw new KeyNotFoundException(string.Format("id is a required property for {0}", resourceName));
            }

            var filterDefinitions = ConvertFilterDictionary(filterDictionary);

            _ethosApiBuilderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            _configurationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("S", StringComparison.OrdinalIgnoreCase))
            {
                return extendedData;
            }

            EthosApiBuilder extendedDataRequest = ConvertEthosApiBuilderDtoToRequest("$NEW", extendedData, configuration.PrimaryEntity);

            // A Business Process API (Type "T") will return the list of returned fields so that
            // the GET following the PUT or POST isn't required any longer.  Just use the result
            // coming from the PUT/POST operation by setting the service level property.
            if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var responseDictionary = await _ethosApiBuilderRepository.UpdateEthosBusinessProcessApiAsync(extendedDataRequest, configuration, filterDefinitions, returnRestrictedFields, extendedDataConfig);
                    EthosExtendedDataDictionary = responseDictionary.FirstOrDefault().Value;
                    extendedData._Id = responseDictionary.FirstOrDefault().Key;
                    _ethosApiBuilderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                    _configurationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                    SecureDataDefinition = _ethosApiBuilderRepository.GetSecureDataDefinition();
                }
                catch (IntegrationApiException ex)
                {
                    var messages = ex.Errors;
                    IntegrationApiException = new IntegrationApiException();
                    foreach (var message in messages)
                    {
                        var guid = string.IsNullOrEmpty(message.Guid) ? extendedDataRequest.Guid : message.Guid;
                        var primaryId = string.IsNullOrEmpty(message.Id) ? string.Empty : message.Id;
                        IntegrationApiExceptionAddError(message.Message, "Create.Update.Success.Exception", guid, primaryId, System.Net.HttpStatusCode.NotFound);
                    }
                    throw IntegrationApiException;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(configuration.PrimaryGuidSource) || !string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    EthosApiBuilder extendedDataResponse = await _ethosApiBuilderRepository.UpdateEthosApiBuilderAsync(extendedDataRequest, configuration);
                    _ethosApiBuilderRepository.EthosExtendedDataDictionary = null;
                    _configurationRepository.EthosExtendedDataDictionary = null;
                    if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
                    {
                        return ConvertEthosApiBuilderEntityToDto(extendedDataResponse, false);
                    }

                    try
                    {
                        return await GetEthosApiBuilderByIdAsync(extendedDataResponse.Guid, configuration.ResourceName);
                    }
                    catch (IntegrationApiException ex)
                    {
                        var messages = ex.Errors;
                        IntegrationApiException = new IntegrationApiException();
                        foreach (var message in messages)
                        {
                            var guid = string.IsNullOrEmpty(message.Guid) ? extendedDataRequest.Guid : message.Guid;
                            var primaryId = string.IsNullOrEmpty(message.Id) ? string.Empty : message.Id;
                            IntegrationApiExceptionAddError(message.Message, "Create.Update.Success.Exception", guid, primaryId, System.Net.HttpStatusCode.NotFound);
                        }
                        throw IntegrationApiException;
                    }
                }
            }

            return extendedData;

        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a person's guardian.
        /// </summary>
        private void CheckUserEthosApiBuilderCreatePermissions(EthosApiConfiguration configuration)
        {
            // access is ok if the current user has the view or update permission
            bool userHasPermission = false;
            foreach (var method in configuration.HttpMethods)
            {
                if (string.IsNullOrEmpty(method.Permission) && method.Method == "POST")
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "POST")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }

            if (!userHasPermission)
            {
                var message = string.Format("User '" + CurrentUser.UserId + "' does not have permission to create {0}.", configuration.ResourceName);
                if (configuration.ApiType != null && configuration.ApiType.Equals("S", StringComparison.OrdinalIgnoreCase))
                    message = string.Format("User '" + CurrentUser.UserId + "' does not have permission to access the subroutine {0}.", configuration.PrimaryEntity);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        #endregion

        #region DELETE Method
        /// <summary>
        /// Updates existing extended data record
        /// </summary>
        /// <param name="id">the guid for extended data record</param>
        /// <param name="resourceName">Resource Name being processed for delete</param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        public async Task DeleteEthosApiBuilderAsync(string id, string resourceName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Must provide an id for the {0} resource", resourceName));
            }

            var configuration = await _configurationRepository.GetEthosApiConfigurationByResource(resourceName);

            // These configuraiton properties must be set at run-time and cannot be cached.
            configuration.CurrentUserId = CurrentUser.PersonId;

            CheckUserEthosApiBuilderDeletePermissions(configuration);

            if (string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                var extendedDataLookUpResult = await _ethosApiBuilderRepository.GetRecordInfoFromGuidAsync(id);
                if (extendedDataLookUpResult == null)
                {
                    throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", resourceName, id));
                }
                if (extendedDataLookUpResult != null && !extendedDataLookUpResult.Entity.Equals(configuration.PrimaryGuidFileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException(string.Format("The id is for a different resource than {0}.", resourceName));
                }
            }

            await _ethosApiBuilderRepository.DeleteEthosApiBuilderAsync(id, configuration);
        }

      
        /// <summary>
        /// Verifies if the user has the correct permissions to delete.
        /// </summary>
        private void CheckUserEthosApiBuilderDeletePermissions(EthosApiConfiguration configuration)
        {
            // access is ok if the current user has the view or update permission
            bool userHasPermission = false;
            foreach (var method in configuration.HttpMethods)
            {
                if (string.IsNullOrEmpty(method.Permission) && method.Method == "DELETE")
                {
                    userHasPermission = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(method.Permission) && !userHasPermission)
                    {
                        if (method.Method == "DELETE")
                        {
                            userHasPermission = HasPermission(method.Permission);
                        }
                    }
                }
            }

            if (!userHasPermission)
            {
                logger.Error(string.Format("User '" + CurrentUser.UserId + "' does not have permission to delete {0}.", configuration.ResourceName));
                throw new PermissionsException(string.Format("User '" + CurrentUser.UserId + "' does not have permission to delete {0}.", configuration.ResourceName));
            }
        }

        #endregion

        #region Convert Methods

        /// <summary>
        ///  GetEthosApiConfigurationByResource
        /// </summary>
        /// <param name="resourceName">resourceName</param>
        /// <returns>EthosApiConfiguration</returns>
        public async Task<Ellucian.Colleague.Domain.Base.Entities.EthosApiConfiguration> GetEthosApiConfigurationByResourceAsync(string resourceName)
        {
            return await _configurationRepository.GetEthosApiConfigurationByResource(resourceName, false);
        }

        /// <summary>
        /// Gets  extended configurations by resource, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <param name="bypassCache">bypassCache </param>
        /// <returns>List with all of the extended configurations if available. Returns an null if none available or none configured</returns>
        public async Task<List<Domain.Base.Entities.EthosExtensibleData>> GetExtendedEthosVersionsConfigurationsByResource(Ellucian.Web.Http.EthosExtend.EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false, bool customOnly = true)
        {
            if (!bypassCache)
            {
                bypassCache = ethosResourceRouteInfo.BypassCache;
            }
            return (await _configurationRepository.GetEthosExtensibilityConfigurationEntitiesByResource(ethosResourceRouteInfo.ResourceName, customOnly, bypassCache)).ToList();
        }

        /// <summary>
        ///  Encode a Primary Key
        /// </summary>
        /// <param name="id">Id to encode</param>
        /// <returns>EthosApiConfiguration</returns>
        public string EncodePrimaryKey(string id)
        {
            return _configurationRepository.EncodePrimaryKey(id);
        }

        /// <summary>
        ///  Un-Encode a Primary Key
        /// </summary>
        /// <param name="id">Id to encode</param>
        /// <returns>EthosApiConfiguration</returns>
        public string UnEncodePrimaryKey(string id)
        {
            return _configurationRepository.UnEncodePrimaryKey(id);
        }

        /// <summary>
        /// Converts extended data dto to extended data request
        /// </summary>
        /// <param name="entityId">entityId</param>
        /// <param name="extendedData">Dtos.EthosApiBuilder</param>
        /// <returns>EthosApiBuilderRequest</returns>
        private EthosApiBuilder ConvertEthosApiBuilderDtoToRequest(string entityId, Dtos.EthosApiBuilder extendedData, string entity)
        {

            var extendedDataRequest = new EthosApiBuilder(extendedData._Id, entityId, entity);        

            return extendedDataRequest;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="extendedDataEntity">EthosApiBuilder</param>
        /// <returns>Dtos.EthosApiBuilder</returns>
        private Dtos.EthosApiBuilder ConvertEthosApiBuilderEntityToDto(EthosApiBuilder extendedDataEntity, bool bypassCache)
        {
            Dtos.EthosApiBuilder extendedDataDto = new Dtos.EthosApiBuilder();
            extendedDataDto._Id = extendedDataEntity.Guid;
            
            return extendedDataDto;
        }

        private Dictionary<string, EthosExtensibleDataFilter> ConvertFilterDictionary(Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter> filterDictionary)
        {
            var returnDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
            foreach (var dictItem in filterDictionary)
            {
                var dictKey = dictItem.Key;
                var filterRow = dictItem.Value;
                if (!string.IsNullOrEmpty(dictKey) && filterRow != null)
                {
                    bool namedQuery = false;
                    string jsonPropertyType = filterRow.JsonPropertyType.ToString();
                    if (string.IsNullOrEmpty(filterRow.ColleagueFileName) && !filterRow.KeyQuery)
                    {
                        namedQuery = true;
                    }

                    var row = new EthosExtensibleDataFilter(filterRow.ColleagueColumnName, filterRow.ColleagueFileName,
                        filterRow.JsonTitle, filterRow.JsonPath, jsonPropertyType, filterRow.FilterValue, namedQuery: namedQuery, keyQuery: filterRow.KeyQuery)
                    {
                        DatabaseUsageType = filterRow.DatabaseUsageType,
                        ColleagueFieldPosition = filterRow.ColleaguePropertyPosition,
                        Required = filterRow.Required,
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
                        ValidFilterOpers = filterRow.ValidFilterOpers,
                        SelectionCriteria = new List<EthosApiSelectCriteria>(),
                        SortColumns = new List<EthosApiSortCriteria>(),
                        Enumerations = new List<EthosApiEnumerations>()
                    };

                    // Validate Filter Operators and convert
                    if (!string.IsNullOrEmpty(filterRow.FilterOper))
                    {
                        switch (filterRow.FilterOper)
                        {
                            case "$lte":
                                row.FilterOper = "LE";
                                break;
                            case "$gte":
                                row.FilterOper = "GE";
                                break;
                            case "$lt":
                                row.FilterOper = "LT";
                                break;
                            case "$gt":
                                row.FilterOper = "GT";
                                break;
                            case "$ne":
                                row.FilterOper = "NE";
                                break;
                            case "$eq":
                                row.FilterOper = "EQ";
                                break;
                            default:
                                row.FilterOper = "EQ";
                                break;
                        }
                    }

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