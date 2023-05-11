//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
	[RegisterType]
	public class AdmissionApplicationSupportingItemsService : BaseCoordinationService, IAdmissionApplicationSupportingItemsService
	{

		private readonly IReferenceDataRepository _referenceDataRepository;
		private readonly IAdmissionApplicationSupportingItemsRepository _supportingItemsRepository;

		public AdmissionApplicationSupportingItemsService(

			IAdmissionApplicationSupportingItemsRepository admissionApplicationSupportingItemsRepository,
			IReferenceDataRepository referenceDataRepository,
			IAdapterRegistry adapterRegistry,
			ICurrentUserFactory currentUserFactory,
			IRoleRepository roleRepository,
			IConfigurationRepository configurationRepository,
			ILogger logger)
			: base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
		{
			_supportingItemsRepository = admissionApplicationSupportingItemsRepository;
			_referenceDataRepository = referenceDataRepository;
		}

        IEnumerable<Domain.Base.Entities.CommunicationCode>  _allSupportingItemTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.CommunicationCode>> GetAllSupportingItemTypesAsync(bool ignoreCache)
        {
            if (_allSupportingItemTypes == null)
            {
                _allSupportingItemTypes = await _referenceDataRepository.GetAdmissionApplicationSupportingItemTypesAsync(ignoreCache);
            }
            return _allSupportingItemTypes;
        }

        IEnumerable<Domain.Base.Entities.CorrStatus> _allCorrStatuses = null;
        private async Task<IEnumerable<Domain.Base.Entities.CorrStatus>> GetAllCorrStatusesAsync(bool ignoreCache)
        {
            if (_allCorrStatuses == null)
            {
                _allCorrStatuses = await _referenceDataRepository.GetCorrStatusesAsync(ignoreCache);
            }
            return _allCorrStatuses;
        }

        /// <summary>
        /// Gets all admission-application-supporting-items
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> objects</returns> 
        public async Task<Tuple<IEnumerable<AdmissionApplicationSupportingItems>, int>> GetAdmissionApplicationSupportingItemsAsync(int offset, int limit, bool bypassCache = false)
		{
			//CheckViewPermissions();

			var admissionApplicationSupportingItemsCollection = new List<AdmissionApplicationSupportingItems>();

			Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int> supportingItemsTuple = null;
			try
			{
				supportingItemsTuple = await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsAsync(offset, limit, bypassCache);
			}
			catch (RepositoryException ex)
			{
				IntegrationApiExceptionAddError(ex);
				throw IntegrationApiException;
			}
			var totalItems = supportingItemsTuple.Item2;
			var admissionApplicationSupportingItemsEntities = supportingItemsTuple.Item1;

			if (admissionApplicationSupportingItemsEntities != null && admissionApplicationSupportingItemsEntities.Any())
			{
				foreach (var admissionApplicationSupportingItems in admissionApplicationSupportingItemsEntities)
				{
					admissionApplicationSupportingItemsCollection.Add(await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(admissionApplicationSupportingItems, bypassCache));
				}
			}
			if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
			{
				throw IntegrationApiException;
			}
			return new Tuple<IEnumerable<AdmissionApplicationSupportingItems>, int>(admissionApplicationSupportingItemsCollection, totalItems);
		}

		/// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
		/// <summary>
		/// Get an AdmissionApplicationSupportingItems from its GUID
		/// </summary>
		/// <returns>AdmissionApplicationSupportingItems DTO object</returns>
		public async Task<AdmissionApplicationSupportingItems> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid, bool bypassCache = true)
		{
			//CheckViewPermissions();

			AdmissionApplicationSupportingItems supportingItemDto = new AdmissionApplicationSupportingItems();
			AdmissionApplicationSupportingItem supportingItemEntity = null;
			try
			{
				supportingItemEntity = await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
			}
			catch (KeyNotFoundException ex)
			{
				throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + guid, ex);
			}
			catch (InvalidOperationException ex)
			{
				throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + guid, ex);
			}
			catch (RepositoryException ex)
			{
				IntegrationApiExceptionAddError(ex);
				throw IntegrationApiException;
			}

			if (supportingItemEntity != null)
			{
				supportingItemDto = await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(supportingItemEntity, bypassCache);
			}
			if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
			{
				throw IntegrationApiException;
			}
			return supportingItemDto;
		}

		/// <summary>
		/// Update a AdmissionApplicationSupportingItems.
		/// </summary>
		/// <param name="AdmissionApplicationSupportingItems">The <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> entity to update in the database.</param>
		/// <returns>The newly updated <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see></returns>
		public async Task<AdmissionApplicationSupportingItems> UpdateAdmissionApplicationSupportingItemsAsync(AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
		{
			if (admissionApplicationSupportingItems == null)
				throw new ArgumentNullException("AdmissionApplicationSupportingItems", "Must provide a AdmissionApplicationSupportingItems for update");
			if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
				throw new ArgumentNullException("AdmissionApplicationSupportingItems", "Must provide a guid for AdmissionApplicationSupportingItems update");

			// get the ID associated with the incoming guid
			var primaryKey = string.Empty;
			var secondaryKey = string.Empty;
            try
            {
                var admissionApplicationSupportingItemsEntityId = await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(admissionApplicationSupportingItems.Id);
                if (admissionApplicationSupportingItemsEntityId.Value != null)
                {
                    primaryKey = admissionApplicationSupportingItemsEntityId.Value.PrimaryKey;
                    secondaryKey = admissionApplicationSupportingItemsEntityId.Value.SecondaryKey;
                }
            }
			catch (Exception ex)
			{
				logger.Error(ex, "Primary and Secondary keys are null.  Continue as a CREATE instead of an update.");

			}

			if (!string.IsNullOrEmpty(primaryKey) && !string.IsNullOrEmpty(secondaryKey))
			{
				// verify the user has the permission to update a admissionApplicationSupportingItems
				//CheckUpdatePermissions();

                _supportingItemsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var admissionApplicationSupportingItemsEntity
                = await ConvertAdmissionApplicationSupportingItemsDtoToEntityAsync(primaryKey, secondaryKey, admissionApplicationSupportingItems);
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

				AdmissionApplicationSupportingItem updatedAdmissionApplicationSupportingItemsEntity;
				try
                {
                    // update the entity in the database
                    updatedAdmissionApplicationSupportingItemsEntity =
                        await _supportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItemsEntity);

                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + admissionApplicationSupportingItems.Id, ex);
                }
                catch (ArgumentException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message);
                    throw IntegrationApiException;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message);
                    throw IntegrationApiException;
                }

                AdmissionApplicationSupportingItems supportingItemDto = new AdmissionApplicationSupportingItems();
                if (updatedAdmissionApplicationSupportingItemsEntity != null)
                {
                    supportingItemDto = await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(updatedAdmissionApplicationSupportingItemsEntity, true);
                    if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                    {
                        throw IntegrationApiException;
                    }
                }
                return supportingItemDto;
            }
			// perform a create instead
			return await CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
		}

		/// <summary>
		/// Create a AdmissionApplicationSupportingItems.
		/// </summary>
		/// <param name="admissionApplicationSupportingItems">The <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> entity to create in the database.</param>
		/// <returns>The newly created <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see></returns>
		public async Task<AdmissionApplicationSupportingItems> CreateAdmissionApplicationSupportingItemsAsync(AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
		{
			if (admissionApplicationSupportingItems == null)
				throw new ArgumentNullException("AdmissionApplicationSupportingItems", "Must provide a AdmissionApplicationSupportingItems for update");
			if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
				throw new ArgumentNullException("AdmissionApplicationSupportingItems", "Must provide a guid for AdmissionApplicationSupportingItems update");

			// verify the user has the permission to create a admissionApplicationSupportingItems
			//CheckUpdatePermissions();

			_supportingItemsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var primaryKey = string.Empty;
            var secondaryKey = string.Empty;

            if (admissionApplicationSupportingItems.AssignedOn == null || admissionApplicationSupportingItems.AssignedOn.Equals(default(DateTime)))
            {
                admissionApplicationSupportingItems.AssignedOn = DateTime.Today;
            }

            var admissionApplicationSupportingItemsEntity
                     = await ConvertAdmissionApplicationSupportingItemsDtoToEntityAsync(primaryKey, secondaryKey, admissionApplicationSupportingItems);
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

			AdmissionApplicationSupportingItem createdAdmissionApplicationSupportingItems;
			try
            {
                // create a admissionApplicationSupportingItems entity in the database
                createdAdmissionApplicationSupportingItems =
                    await _supportingItemsRepository.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItemsEntity);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + admissionApplicationSupportingItems.Id, ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
                throw IntegrationApiException;
            }
            // return the newly created admissionApplicationSupportingItems

            AdmissionApplicationSupportingItems supportingItemDto = new AdmissionApplicationSupportingItems();
            if (createdAdmissionApplicationSupportingItems != null)
            {
                supportingItemDto = await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(createdAdmissionApplicationSupportingItems, true);
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
            }
            return supportingItemDto;
        }

		private async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> ConvertAdmissionApplicationSupportingItemsDtoToEntityAsync(string primaryKey, string secondaryKey, AdmissionApplicationSupportingItems source, bool bypassCache = false)
		{
			var guid = source.Id;
			var personId = primaryKey;
			var applicationId = string.Empty;
			var receivedCode = string.Empty;
			var instance = string.Empty;
			var status = string.Empty;
			var statusAction = string.Empty;

			// The secondary key contains application key, "*", CC code, "*", Assign Date, "*" and instance
			// Since instance isn't in the payload but is required to update the correct item in MAILING
			// we need to extract it and pass it on to the update repository method.
			if (!string.IsNullOrEmpty(secondaryKey))
			{
				var indexValues = secondaryKey.Split('*');
				if (indexValues != null && indexValues.Count() > 3)
					applicationId = indexValues[0];
					instance = indexValues[3];
			}
			if (source.Application == null || string.IsNullOrEmpty(source.Application.Id))
			{
				IntegrationApiExceptionAddError("The Application Id is required for a PUT or POST request.",
					"Validation.Exception", guid, secondaryKey);
			}
			if (source.Type == null || string.IsNullOrEmpty(source.Type.Id))
			{
				IntegrationApiExceptionAddError("The Type Id is required for a PUT or POST request.",
					"Validation.Exception", guid, secondaryKey);
			}
			if (source.Status == null || 
				(source.Status.Type == AdmissionApplicationSupportingItemsType.NotSet &&
				(source.Status.Detail == null || string.IsNullOrEmpty(source.Status.Detail.Id))))
			{
				IntegrationApiExceptionAddError("The Status Type or Status Detail ID is required for a PUT or POST request.",
					"Validation.Exception", guid, secondaryKey);
			}
			if (string.IsNullOrEmpty(applicationId) && source.Application != null && !string.IsNullOrEmpty(source.Application.Id))
			{
				try
				{
					applicationId = await _supportingItemsRepository.GetApplicationIdFromGuidAsync(source.Application.Id);
					if (string.IsNullOrEmpty(applicationId))
					{
						IntegrationApiExceptionAddError(string.Format("No application was found for guid '{0}'.", source.Application.Id),
							"Validation.Exception", guid, secondaryKey);
					}
				}
				catch (Exception)
				{
					IntegrationApiExceptionAddError(string.Format("No application was found for guid '{0}'.", source.Application.Id),
						"Validation.Exception", guid, secondaryKey);
				}
			}
			else if (!string.IsNullOrWhiteSpace(applicationId) && source.Application != null && !string.IsNullOrEmpty(source.Application.Id))
			{
				try
				{
					var applId = await _supportingItemsRepository.GetApplicationIdFromGuidAsync(source.Application.Id);
					if (!string.IsNullOrEmpty(applId) && !applId.Equals(applicationId, StringComparison.OrdinalIgnoreCase))
					{
						IntegrationApiExceptionAddError(string.Format("The supporting items record '{0}' points to a different application.id of '{1}'.", guid, applicationId),
							"Validation.Exception", guid, secondaryKey);
					}
				}
				catch (Exception)
				{
					IntegrationApiExceptionAddError(string.Format("No application was found for guid '{0}'.", source.Application.Id),
						"Validation.Exception", guid, secondaryKey);
				}
			}

			if (source.Type != null && !string.IsNullOrEmpty(source.Type.Id))
			{
				try
				{
					receivedCode = await _supportingItemsRepository.GetIdFromGuidAsync(source.Type.Id, "CC.CODES");
					if (string.IsNullOrEmpty(receivedCode))
					{
						IntegrationApiExceptionAddError(string.Format("No type was found for guid '{0}'.", source.Type.Id),
							"Validation.Exception", guid, secondaryKey);
					}
				}
				catch (Exception)
				{
					IntegrationApiExceptionAddError(string.Format("No type was found for guid '{0}'.", source.Type.Id),
							"Validation.Exception", guid, secondaryKey);
				}
			}
			if (source.Status != null && source.Status.Detail != null && !string.IsNullOrEmpty(source.Status.Detail.Id))
			{
				// Only extract status ID for incomplete, waived, or received.  Otherwise, the status property
				// will be unset when it gets to the actual updating of the MAILING record in Colleague.
				if (source.Status.Type != AdmissionApplicationSupportingItemsType.Notreceived)
				{
					try
					{
						var statusEntity = (await _referenceDataRepository.GetCorrStatusesAsync(bypassCache)).FirstOrDefault(cs => cs.Guid == source.Status.Detail.Id);
						if (statusEntity == null)
						{
							IntegrationApiExceptionAddError(string.Format("Status type not found for id {0}.", source.Status.Detail.Id),
								"Validation.Exception", guid, secondaryKey);
						}
						status = statusEntity.Code;
						statusAction = statusEntity.Action;
						if (source.Status != null && source.Status.Type != AdmissionApplicationSupportingItemsType.NotSet)
						{
							if ((statusAction != "1" && statusAction != "0" && source.Status.Type != AdmissionApplicationSupportingItemsType.Incomplete)
								|| (statusAction == "1" && source.Status.Type != AdmissionApplicationSupportingItemsType.Received)
								|| (statusAction == "0" && source.Status.Type != AdmissionApplicationSupportingItemsType.Waived))
							{
								IntegrationApiExceptionAddError(string.Format("status.type of '{0}' doesn't match the type for status.detail.id of '{1}'.", source.Status.Type.ToString(), source.Status.Detail.Id),
								"Validation.Exception", guid, secondaryKey);
							}
						}
					}
					catch (Exception)
					{
						IntegrationApiExceptionAddError(string.Format("Status type not found for id {0}.", source.Status.Detail.Id),
								"Validation.Exception", guid, secondaryKey);
					}
				}
			}
			else
			{
				if (source.Status != null && source.Status.Type != AdmissionApplicationSupportingItemsType.NotSet)
				{
					var statusEntities = (await _referenceDataRepository.GetCorrStatusesAsync(bypassCache));
					switch (source.Status.Type)
					{
						case AdmissionApplicationSupportingItemsType.Incomplete:
							{
								var statusEntity = statusEntities.FirstOrDefault(cs => cs.Action != "1" && cs.Action != "0");
								if (statusEntity != null)
								{
									status = statusEntity.Code;
									statusAction = statusEntity.Action;
								}
								break;
							}
						case AdmissionApplicationSupportingItemsType.Notreceived:
							{
								var statusEntity = statusEntities.FirstOrDefault(cs => cs.Action != "1" && cs.Action != "0");
								if (statusEntity != null)
								{
									status = string.Empty;
									statusAction = string.Empty;
								}
								break;
							}
						case AdmissionApplicationSupportingItemsType.Received:
							{
								var statusEntity = statusEntities.FirstOrDefault(cs => cs.Action == "1");
								if (statusEntity != null)
								{
									status = statusEntity.Code;
									statusAction = statusEntity.Action;
								}
								break;
							}
						case AdmissionApplicationSupportingItemsType.Waived:
							{
								var statusEntity = statusEntities.FirstOrDefault(cs => cs.Action == "0");
								if (statusEntity != null)
								{
									status = statusEntity.Code;
									statusAction = statusEntity.Action;
								}
								break;
							}
						default:
							{
								break;
							}
					}
				}
			}
			if (string.IsNullOrEmpty(personId))
			{
				personId = await _supportingItemsRepository.GetPersonIdFromApplicationIdAsync(applicationId);
			}
			// Check for assignedOn date
			if (source.AssignedOn == null || !source.AssignedOn.HasValue)
			{
				IntegrationApiExceptionAddError("The assignedOn date is a required property.",
							"Validation.Exception", guid, secondaryKey);
			}

			AdmissionApplicationSupportingItem admissionApplicationSupportingItems = null;
			try
			{
				admissionApplicationSupportingItems = new AdmissionApplicationSupportingItem(
					source.Id, personId, applicationId, receivedCode, instance, source.AssignedOn, status);
			
				if (source.RequiredByDate != null && !source.RequiredByDate.Equals(DateTime.MinValue))
					admissionApplicationSupportingItems.ActionDate = source.RequiredByDate;
				if (source.ReceivedOn != null && !source.ReceivedOn.Equals(DateTime.MinValue))
					admissionApplicationSupportingItems.ReceivedDate = source.ReceivedOn.HasValue? source.ReceivedOn.Value.Date.Date : default(DateTime?);
				admissionApplicationSupportingItems.StatusAction = statusAction;
				admissionApplicationSupportingItems.Comment = source.ExternalReference;
				admissionApplicationSupportingItems.Required = source.Required == AdmissionApplicationSupportingItemsRequired.Mandatory ? true : false;
			}
			catch (Exception ex)
			{
				// If we haven't already reported an issue with required data, then include the error here.
				if (IntegrationApiException == null || IntegrationApiException.Errors == null || !IntegrationApiException.Errors.Any())
				{
					IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", guid, secondaryKey);
				}
			}

			return admissionApplicationSupportingItems;
		}

		/// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
		/// <summary>
		/// Converts a Mailing domain entity to its corresponding AdmissionApplicationSupportingItems DTO
		/// </summary>
		/// <param name="source">Mailing domain entity</param>
		/// <returns>AdmissionApplicationSupportingItems DTO</returns>
		private async Task<AdmissionApplicationSupportingItems> ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(Domain.Student.Entities.AdmissionApplicationSupportingItem source, bool bypassCache)
		{
			var admissionApplicationSupportingItems = new AdmissionApplicationSupportingItems();

			admissionApplicationSupportingItems.Id = source.Guid;

            if (!string.IsNullOrEmpty(source.ApplicationId))
				admissionApplicationSupportingItems.Application = new GuidObject2(source.ApplicationId);

			if (source.AssignedDate.HasValue)
				admissionApplicationSupportingItems.AssignedOn = source.AssignedDate.Value;

			if (!string.IsNullOrEmpty(source.ReceivedCode))
			{
				try
				{
					var suppItemType = (await GetAllSupportingItemTypesAsync(bypassCache)).FirstOrDefault(sit => sit.Code == source.ReceivedCode);
					if (suppItemType != null)
					{
						admissionApplicationSupportingItems.Type = new GuidObject2(suppItemType.Guid);
					}
					else
					{
						IntegrationApiExceptionAddError(string.Format("Could not find a GUID for CC.CODES '{0}'", source.ReceivedCode),
						"GUID.Not.Found", source.Guid, source.MailingId);
					}
				}
				catch (Exception)
				{
					IntegrationApiExceptionAddError(string.Format("Could not find a GUID for CC.CODES '{0}'", source.ReceivedCode),
						"GUID.Not.Found", source.Guid, source.MailingId);
				}
			}

			if (source.ReceivedDate.HasValue)
				admissionApplicationSupportingItems.ReceivedOn = source.ReceivedDate.Value;

			if (source.AssignedDate.HasValue)
				admissionApplicationSupportingItems.AssignedOn = source.AssignedDate.Value;

			if (source.ActionDate.HasValue)
				admissionApplicationSupportingItems.RequiredByDate = source.ActionDate.Value;

			admissionApplicationSupportingItems.Status = new Dtos.DtoProperties.AdmissionApplicationSupportingItemsStatus();
			if (string.IsNullOrEmpty(source.Status))
			{
				admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Notreceived;
			}
			else
			{
				try
				{
					var corrStatus = (await GetAllCorrStatusesAsync(bypassCache)).FirstOrDefault(cs => cs.Code == source.Status);
					if (corrStatus != null)
					{
						admissionApplicationSupportingItems.Status.Detail = new GuidObject2(corrStatus.Guid);
					}
					switch (source.StatusAction)
					{
						case "1":
							{
								admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Received;
								break;
							}
						case "0":
							{
								admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Waived;
								break;
							}
						default:
							if (string.IsNullOrEmpty(source.StatusAction))
							{
								admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Incomplete;
							}
							else
							{
								admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Notreceived;
							}
							break;
					}
				}
				catch (Exception)
				{
					IntegrationApiExceptionAddError(string.Format("Could not find a GUID for valcode CORR.STATUSES '{0}'", source.Status),
						"GUID.Not.Found", source.Guid, source.MailingId);
				}
			}

			admissionApplicationSupportingItems.ExternalReference = source.Comment;

			if (source.Required)
				admissionApplicationSupportingItems.Required = AdmissionApplicationSupportingItemsRequired.Mandatory;

			return admissionApplicationSupportingItems;
		}

		/// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
		/// <summary>
		/// Checks admission application supporting items view permissions
		/// </summary>
		private void CheckViewPermissions()
		{
			// access is ok if the current user has the view housing request
			if (!HasPermission(StudentPermissionCodes.ViewApplicationSupportingItems) && !HasPermission(StudentPermissionCodes.UpdateApplicationSupportingItems))
			{
				logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-application-supporting-items.");
				throw new PermissionsException("User is not authorized to view admission-application-supporting-items.");
			}
		}

		/// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
		/// <summary>
		/// Checks admission application supporting items view permissions
		/// </summary>
		private void CheckUpdatePermissions()
		{
			// access is ok if the current user has the view housing request
			if (!HasPermission(StudentPermissionCodes.UpdateApplicationSupportingItems))
			{
				logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update admission-application-supporting-items.");
				throw new PermissionsException("User is not authorized to update admission-application-supporting-items.");
			}
		}
	}
}