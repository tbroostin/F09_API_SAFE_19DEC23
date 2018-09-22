//Copyright 2017 Ellucian Company L.P. and its affiliates.
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

		/// <summary>
		/// Gets all admission-application-supporting-items
		/// </summary>
		/// <param name="offset">Offset for paging results</param>
		/// <param name="limit">Limit for paging results</param>
		/// <param name="bypassCache">Flag to bypass cache</param>
		/// <returns>Collection of <see cref="AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> objects</returns> 
		public async Task<Tuple<IEnumerable<AdmissionApplicationSupportingItems>, int>> GetAdmissionApplicationSupportingItemsAsync(int offset, int limit, bool bypassCache = false)
		{
			CheckViewPermissions();

			var admissionApplicationSupportingItemsCollection = new List<AdmissionApplicationSupportingItems>();

			var supportingItemsTuple = await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsAsync(offset, limit, bypassCache);
			var totalItems = supportingItemsTuple.Item2;
			var admissionApplicationSupportingItemsEntities = supportingItemsTuple.Item1;

			if (admissionApplicationSupportingItemsEntities != null && admissionApplicationSupportingItemsEntities.Any())
			{
				foreach (var admissionApplicationSupportingItems in admissionApplicationSupportingItemsEntities)
				{
					admissionApplicationSupportingItemsCollection.Add(await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(admissionApplicationSupportingItems, bypassCache));
				}
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
			CheckViewPermissions();

			try
			{
				return await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync((await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsByGuidAsync(guid)), bypassCache);
			}
			catch (KeyNotFoundException ex)
			{
				throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + guid, ex);
			}
			catch (InvalidOperationException ex)
			{
				throw new KeyNotFoundException("admission-application-supporting-items not found for GUID " + guid, ex);
			}
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
			var admissionApplicationSupportingItemsEntityId = await _supportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(admissionApplicationSupportingItems.Id);
			if (admissionApplicationSupportingItemsEntityId.Value != null)
			{
				primaryKey = admissionApplicationSupportingItemsEntityId.Value.PrimaryKey;
				secondaryKey = admissionApplicationSupportingItemsEntityId.Value.SecondaryKey;
			}
			if (!string.IsNullOrEmpty(primaryKey) && !string.IsNullOrEmpty(secondaryKey))
			{
				// verify the user has the permission to update a admissionApplicationSupportingItems
				CheckUpdatePermissions();

				_supportingItemsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

				try
				{
					// map the DTO to entities
					var admissionApplicationSupportingItemsEntity
					= await ConvertAdmissionApplicationSupportingItemsDtoToEntityAsync(primaryKey, secondaryKey, admissionApplicationSupportingItems);

					// update the entity in the database
					var updatedAdmissionApplicationSupportingItemsEntity =
						await _supportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItemsEntity);

					return await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(updatedAdmissionApplicationSupportingItemsEntity, true);
				}
				catch (RepositoryException ex)
				{
					throw ex;
				}
				catch (KeyNotFoundException ex)
				{
					throw ex;
				}
				catch (ArgumentException ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message, ex.InnerException);
				}
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
			CheckUpdatePermissions();

			_supportingItemsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

			try
			{
				var primaryKey = string.Empty;
				var secondaryKey = string.Empty;

				if (admissionApplicationSupportingItems.AssignedOn == null || admissionApplicationSupportingItems.AssignedOn.Equals(default(DateTime)))
				{
					admissionApplicationSupportingItems.AssignedOn = DateTime.Today;
				}

				var admissionApplicationSupportingItemsEntity
						 = await ConvertAdmissionApplicationSupportingItemsDtoToEntityAsync(primaryKey, secondaryKey, admissionApplicationSupportingItems);

				// create a admissionApplicationSupportingItems entity in the database
				var createdAdmissionApplicationSupportingItems =
					await _supportingItemsRepository.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItemsEntity);
				// return the newly created admissionApplicationSupportingItems
				return await ConvertAdmissionApplicationSupportingItemsEntityToDtoAsync(createdAdmissionApplicationSupportingItems, true);

			}
			catch (RepositoryException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex.InnerException);
			}
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
				throw new ArgumentNullException("The Application Id is required for a PUT or POST request. ", "admissionApplicationSupportingItem.Application.Id");
			}
			if (source.Type == null || string.IsNullOrEmpty(source.Type.Id))
			{
				throw new ArgumentNullException("The Type Id is required for a PUT or POST request. ", "admissionApplicationSupportingItem.Type.Id");
			}
			if (source.Status == null || 
				(source.Status.Type == AdmissionApplicationSupportingItemsType.NotSet &&
				(source.Status.Detail == null || string.IsNullOrEmpty(source.Status.Detail.Id))))
			{
				throw new ArgumentNullException("The Status Type or Status Detail ID is required for a PUT or POST request. ", "admissionApplicationSupportingItem.Status");
			}
			if (string.IsNullOrEmpty(applicationId) && source.Application != null && !string.IsNullOrEmpty(source.Application.Id))
			{
				applicationId = await _supportingItemsRepository.GetApplicationIdFromGuidAsync(source.Application.Id);
				if (string.IsNullOrEmpty(applicationId))
				{
					throw new ArgumentException(string.Format("No application was found for guid '{0}' ", source.Application.Id), "admissionApplicationSupportingItem.Application.Id");
				}
			}
			else if (!string.IsNullOrWhiteSpace(applicationId) && source.Application != null && !string.IsNullOrEmpty(source.Application.Id))
			{
				var applId = await _supportingItemsRepository.GetApplicationIdFromGuidAsync(source.Application.Id);
				if(!string.IsNullOrEmpty(applId) && !applId.Equals(applicationId, StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException(string.Format("No application was found for guid '{0}' ", source.Application.Id), "admissionApplicationSupportingItem.Application.Id");
				}
			}

			if (source.Type != null && !string.IsNullOrEmpty(source.Type.Id))
			{
				receivedCode = await _supportingItemsRepository.GetIdFromGuidAsync(source.Type.Id);
				if (string.IsNullOrEmpty(receivedCode))
				{
					throw new ArgumentException(string.Format("No type was found for guid '{0}' ", source.Type.Id), "admissionApplicationSupportingItem.Type.Id");
				}
			}
			if (source.Status != null && source.Status.Detail != null && !string.IsNullOrEmpty(source.Status.Detail.Id))
			{
				var statusEntity = (await _referenceDataRepository.GetCorrStatusesAsync(bypassCache)).FirstOrDefault(cs => cs.Guid == source.Status.Detail.Id);
				if (statusEntity == null)
				{
					throw new KeyNotFoundException(string.Format("Status type not found for id {0}.", source.Status.Detail.Id));
				}
				status = statusEntity.Code;
				statusAction = statusEntity.Action;
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
				personId = await _supportingItemsRepository.GetPersonIdFromApplicationIdAsync(applicationId);

			var admissionApplicationSupportingItems = new Domain.Student.Entities.AdmissionApplicationSupportingItem(
				source.Id, personId, applicationId, receivedCode, instance, source.AssignedOn, status);

			if (source.RequiredByDate != null && !source.RequiredByDate.Equals(DateTime.MinValue))
				admissionApplicationSupportingItems.ActionDate = source.RequiredByDate;
			if (source.ReceivedOn != null && !source.ReceivedOn.Equals(DateTime.MinValue))
				admissionApplicationSupportingItems.ReceivedDate = source.ReceivedOn.HasValue? source.ReceivedOn.Value.Date.Date : default(DateTime?);
			admissionApplicationSupportingItems.StatusAction = statusAction;
			admissionApplicationSupportingItems.Comment = source.ExternalReference;
			admissionApplicationSupportingItems.Required = source.Required == AdmissionApplicationSupportingItemsRequired.Mandatory ? true : false;

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
			var applGuid = await _supportingItemsRepository.GetGuidFromIdAsync("APPLICATIONS", source.ApplicationId);
			if (!string.IsNullOrEmpty(applGuid))
				admissionApplicationSupportingItems.Application = new GuidObject2(applGuid);
			if (source.AssignedDate.HasValue)
				admissionApplicationSupportingItems.AssignedOn = source.AssignedDate.Value;
			if (!string.IsNullOrEmpty(source.ReceivedCode))
			{
				var ccGuid = await _supportingItemsRepository.GetGuidFromIdAsync("CC.CODES", source.ReceivedCode);
				if (!string.IsNullOrEmpty(ccGuid))
					admissionApplicationSupportingItems.Type = new GuidObject2(ccGuid);
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
				var statusGuid = await _supportingItemsRepository.GetGuidFromIdAsync("VALCODES", "CORR.STATUSES", "VAL.INTERNAL.CODE", source.Status);
				if (string.IsNullOrEmpty(statusGuid))
				{
					statusGuid = await _supportingItemsRepository.GetGuidFromIdAsync("CORE.VALCODES", "CORR.STATUSES", "VAL.INTERNAL.CODE", source.Status);
				}
				if (!string.IsNullOrEmpty(statusGuid))
				{
					admissionApplicationSupportingItems.Status.Detail = new GuidObject2(statusGuid);
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
			if (!HasPermission(StudentPermissionCodes.ViewApplicationSupportingItems))
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