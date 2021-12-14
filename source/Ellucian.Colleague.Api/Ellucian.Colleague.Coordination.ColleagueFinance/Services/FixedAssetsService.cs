//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class FixedAssetsService : BaseCoordinationService, IFixedAssetsService
    {

        private readonly IFixedAssetsRepository _fixedAssetsRepository;
        private readonly IColleagueFinanceReferenceDataRepository _financeReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IPersonRepository _personRepository;


        public FixedAssetsService(
            IFixedAssetsRepository fixedAssetsRepository,
            IColleagueFinanceReferenceDataRepository financeReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IRoomRepository roomRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _fixedAssetsRepository = fixedAssetsRepository;
            _referenceDataRepository = referenceDataRepository;
            _financeReferenceDataRepository = financeReferenceDataRepository;
            _roomRepository = roomRepository;
            _personRepository = personRepository;
        }

        #region GET Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all fixed-assets
        /// </summary>
        /// <returns>Collection of FixedAssets DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.FixedAssets>, int>> GetFixedAssetsAsync(int offset, int limit, bool bypassCache = false)
        {
            List<Dtos.FixedAssets> fixedAssets = new List<Dtos.FixedAssets>();

            Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int> fixedAssetsEntities = null;

            try
            {
                fixedAssetsEntities = await _fixedAssetsRepository.GetFixedAssetsAsync(offset, limit, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Bad.Data");
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            if (fixedAssetsEntities == null || !fixedAssetsEntities.Item1.Any())
            {
                return new Tuple<IEnumerable<Dtos.FixedAssets>, int>(new List<Dtos.FixedAssets>(), 0);
            }

            var personIds = fixedAssetsEntities.Item1
                .Where(x => (!string.IsNullOrEmpty(x.FixStewerdId)))
                .Select(x => x.FixStewerdId).Distinct().ToList();

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

            var hostCountry = await HostCountryAsync();
            foreach (var fixedAssetntity in fixedAssetsEntities.Item1)
            {
                try
                {
                    fixedAssets.Add(await ConvertFixedAssetsEntityToDtoAsync(fixedAssetntity, personGuidCollection, hostCountry, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.FixedAssets>, int>(fixedAssets, fixedAssetsEntities.Item2);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FixedAssets from its GUID
        /// </summary>
        /// <returns>FixedAssets DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FixedAssets> GetFixedAssetsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a fixed asset.");
            }

            Domain.ColleagueFinance.Entities.FixedAssets fixedAssetEntity = null;
            try
            {
                fixedAssetEntity = await _fixedAssetsRepository.GetFixedAssetByIdAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No fixed-assets was found for GUID '{0}'", guid));
            }
            Ellucian.Colleague.Dtos.FixedAssets retval = null;
            try
            {

                var hostCountry = await HostCountryAsync();
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new List<string> { fixedAssetEntity.FixStewerdId });

                retval = await ConvertFixedAssetsEntityToDtoAsync(fixedAssetEntity, personGuidCollection, hostCountry, bypassCache);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No fixed-assets was found for GUID '{0}'", guid));
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return retval;
        }

        /// <summary>
        /// Get fixed assets transfer flag
        /// </summary>
        /// <returns>Collection of <see cref="FixedAssetsFlag">FixedAssetsFlag</see> objects</returns>
        public async Task<IEnumerable<FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync()
        {
            var fxaTransferFlagEntities = await _financeReferenceDataRepository.GetFixedAssetTransferFlagsAsync();
            // Get the right adapter for the type mapping
            var fxaTransferFlagAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.FixedAssetsFlag, FixedAssetsFlag>();
            // Map the entity to the DTO
            var fxaTransferFlagDtos = new List<FixedAssetsFlag>();
            if (fxaTransferFlagEntities != null && fxaTransferFlagEntities.Any())
            {
                foreach (var fxaFlag in fxaTransferFlagEntities)
                {
                    if (fxaFlag != null)
                    {
                        fxaTransferFlagDtos.Add(fxaTransferFlagAdapter.MapToType(fxaFlag));
                    }
                }
            }
            return fxaTransferFlagDtos;
        }
        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FixedAssets domain entity to its corresponding FixedAssets DTO
        /// </summary>
        /// <param name="source">FixedAssets domain entity</param>
        /// <returns>FixedAssets DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.FixedAssets> ConvertFixedAssetsEntityToDtoAsync(Domain.ColleagueFinance.Entities.FixedAssets source,
            Dictionary<string, string> personGuidCollection, string hostCountry, bool bypassCache)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("FixedAssets is required.", "Bad.Data");
                return null;
            }

            var fixedAsset = new Ellucian.Colleague.Dtos.FixedAssets();

            try
            {

                fixedAsset.Id = source.RecordGuid;
                fixedAsset.Description = source.Description;
                fixedAsset.Tag = source.FixPropertyTag;
                if (!string.IsNullOrEmpty(source.FixAssetType))
                {
                    try
                    {
                        var assetTypeGuid = await _financeReferenceDataRepository.GetAssetTypesGuidAsync(source.FixAssetType);
                        if (string.IsNullOrEmpty(assetTypeGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("No Guid found, Entity:'ASSET.TYPES', Record ID:'", source.FixAssetType, "'")
                                , "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            fixedAsset.Type = new GuidObject2(assetTypeGuid);
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                    }
                }

                if (!string.IsNullOrEmpty(source.FixAssetCategory))
                {
                    try
                    {
                        var assetCategoryGuid = await _financeReferenceDataRepository.GetAssetCategoriesGuidAsync(source.FixAssetCategory);
                        if (string.IsNullOrEmpty(assetCategoryGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("No Guid found, Entity:'ASSET.CATEGORIES', Record ID:'", source.FixAssetCategory, "'")
                                , "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            fixedAsset.Category = new GuidObject2(assetCategoryGuid);
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                    }
                }

                var capitalizationStatus = ConvertEntityToCapitalizationStatus(source);
                if (capitalizationStatus != FixedAssetsCapitalizationStatus.NotSet)
                {
                    fixedAsset.CapitalizationStatus = capitalizationStatus;
                }
                var aquisitionMethod = await ConvertEntityToAcquisiotionMethod(source, bypassCache);
                if (aquisitionMethod != FixedAssetsAcquisitionMethod.NotSet)
                {
                    fixedAsset.AcquisitionMethod = aquisitionMethod;
                }
                fixedAsset.Status = ConvertEntityToStatus(source.FixDisposalDate);
                fixedAsset.Condition = await ConvertEntityToItemConditionAsync(source, bypassCache);
                fixedAsset.Location = string.IsNullOrEmpty(source.FixLocation) ? null : source.FixLocation;

                if (!string.IsNullOrEmpty(source.FixBuilding))
                {
                    try
                    {
                        var buildingGuid = await _referenceDataRepository.GetBuildingGuidAsync(source.FixBuilding);
                        if (string.IsNullOrEmpty(buildingGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Building GUID not found for '{0}'", source.FixBuilding)
                                , "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            fixedAsset.Building = string.IsNullOrEmpty(buildingGuid) ? null : new GuidObject2(buildingGuid);
                            fixedAsset.Room = await ConvertEntityToRoomGuidObjectAsync(source, source.FixBuilding, bypassCache);
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.RecordGuid, source.RecordKey);
                    }
                }

                if (string.IsNullOrEmpty(hostCountry))
                {
                    IntegrationApiExceptionAddError("Host country not found.", "Bad.Data", source.RecordGuid, source.RecordKey);
                }
                else
                {
                    var currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD : CurrencyIsoCode.USD;

                    if (source.InsuranceAmountCoverage != null && source.InsuranceAmountCoverage.HasValue)
                    {
                        fixedAsset.InsuredValue = new Amount2DtoProperty() { Value = source.InsuranceAmountCoverage, Currency = currency };
                    }

                    if (source.FixValueAmount != null && source.FixValueAmount.HasValue)
                    {
                        fixedAsset.MarketValue = new Amount2DtoProperty() { Value = source.FixValueAmount, Currency = currency };
                    }

                    if (source.FixAcqisitionCost != null && source.FixAcqisitionCost.HasValue)
                    {
                        fixedAsset.AcquisitionCost = new Amount2DtoProperty() { Value = source.FixAcqisitionCost, Currency = currency };
                    }

                    if (source.FixAllowAmount != null && source.FixAllowAmount.HasValue)
                    {
                        fixedAsset.AccumulatedDepreciation = new Amount2DtoProperty() { Value = source.FixAllowAmount, Currency = currency };
                    }

                    if (source.FixSalvageValue != null && source.FixSalvageValue.HasValue)
                    {
                        fixedAsset.SalvageValue = new Amount2DtoProperty() { Value = source.FixSalvageValue, Currency = currency };
                    }

                    if (source.FixRenewalAmount != null && source.FixRenewalAmount.HasValue)
                    {
                        fixedAsset.RenewalCost = new Amount2DtoProperty() { Value = source.FixRenewalAmount, Currency = currency };
                    }
                }

                if (!string.IsNullOrEmpty(source.FixStewerdId))
                {
                    if (personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for personId: '", source.FixStewerdId, "'"),
                            "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.FixStewerdId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for personId: '", source.FixStewerdId, "'"),
                                "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        fixedAsset.ResponsiblePersons = new List<GuidObject2>() { new GuidObject2(personGuid) };
                    }
                }

                fixedAsset.DepreciationMethod = string.IsNullOrEmpty(source.FixCalculationMethod) ? null : source.FixCalculationMethod;
                fixedAsset.UsefulLife = source.FixUsefulLife.HasValue ? source.FixUsefulLife.Value : default(int?);
                fixedAsset.DepreciationExpenseAccount = string.IsNullOrEmpty(source.FixCalcAccount) ? null : new GuidObject2(source.FixCalcAccount);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.RecordGuid, source.RecordKey);
            }

            return fixedAsset;
        }

        #region Convert Methods  
        /// <summary>
        /// Fixed Asset Capitalization Status enum.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private FixedAssetsCapitalizationStatus ConvertEntityToCapitalizationStatus(Domain.ColleagueFinance.Entities.FixedAssets source)
        {
            if (source != null && string.IsNullOrEmpty(source.CapitalizationStatus))
            {
                IntegrationApiExceptionAddError("Capitalization status is required.", "Bad Data", source.RecordGuid, source.RecordKey);
            }
            else
            {
                switch (source.CapitalizationStatus.ToUpperInvariant())
                {
                    case "Y":
                    case "C":
                    case "P":
                        return FixedAssetsCapitalizationStatus.Capitalized;
                    case "N":
                        return FixedAssetsCapitalizationStatus.Noncapital;
                    default:
                        IntegrationApiExceptionAddError(string.Format("Capitalization status '{0}' is invalid.", source.CapitalizationStatus),
                           "Bad Data", source.RecordGuid, source.RecordKey);
                        return FixedAssetsCapitalizationStatus.NotSet;
                }
            }
            return FixedAssetsCapitalizationStatus.NotSet;
        }

        /// <summary>
        /// Fixed Asset Acquisition Method enum.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<FixedAssetsAcquisitionMethod> ConvertEntityToAcquisiotionMethod(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            var acqMethods = await AcquisitionMethodsAsync(bypassCache);
            if (acqMethods == null)
            {
                IntegrationApiExceptionAddError("Acquisition methods are not defined.", "Bad Data", source.RecordGuid, source.RecordKey);
            }
            else
            {
                var acqMethod = acqMethods.FirstOrDefault(i => i.Code.Equals(source.AcquisitionMethod, StringComparison.OrdinalIgnoreCase));
                if (acqMethod == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate GUID for AcquisitionMethod: '{0}'", source.AcquisitionMethod),
                          "Bad Data", source.RecordGuid, source.RecordKey);
                }
                else
                {
                    switch (acqMethod.AcquisitionType.ToUpperInvariant())
                    {
                        case "P":
                            return FixedAssetsAcquisitionMethod.Purchased;
                        case "L":
                            return FixedAssetsAcquisitionMethod.Leased;
                        case "D":
                            return FixedAssetsAcquisitionMethod.Donation;
                        default:
                            IntegrationApiExceptionAddError(string.Format("The AcquisitionMethod '{0}' is invalid.", source.AcquisitionMethod),
                                "Bad Data", source.RecordGuid, source.RecordKey);
                            return FixedAssetsAcquisitionMethod.NotSet;
                    }
                }
            }
            return FixedAssetsAcquisitionMethod.NotSet;
        }

        /// <summary>
        /// Fixed Asset Status enum.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private FixedAssetsStatus ConvertEntityToStatus(DateTime? source)
        {
            //GET/GET ALL - If the field FIX.DISPOSAL.DATE is populated with a date that is equal to or greater than 'today" publish "disposed".
            //The enum "writtenOff does not apply to Colleague.
            if (source.HasValue && source.Value.Date >= DateTime.Today.Date)
            {
                return FixedAssetsStatus.Disposed;
            }
            return FixedAssetsStatus.NotSet;
        }

        /// <summary>
        /// Item condition description.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<string> ConvertEntityToItemConditionAsync(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            if (source != null && string.IsNullOrEmpty(source.FixInvoiceCondition))
            {
                return null;
            }

            var entity = await ItemsConditionsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                IntegrationApiExceptionAddError("Item conditions are not defined.", "Bad Data", source.RecordGuid, source.RecordKey);
                return null;
            }

            var itemCondition = (entity).FirstOrDefault(i => i.Code.Equals(source.FixInvoiceCondition, StringComparison.OrdinalIgnoreCase));
            if (itemCondition == null)
            {
                IntegrationApiExceptionAddError(string.Format("Item condition not found for key: '{0}'.", source.FixInvoiceCondition),
                    "Bad Data", source.RecordGuid, source.RecordKey);
                return null;
            }
            return itemCondition.Description;
        }

        /// <summary>
        /// Converts source value to room.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="building"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToRoomGuidObjectAsync(Domain.ColleagueFinance.Entities.FixedAssets source, string fixBuilding, bool bypassCache)
        {
            if (string.IsNullOrEmpty(fixBuilding))
                return null;

            if (source == null || string.IsNullOrEmpty(source.FixRoom))
                return null;


            var entity = await RoomsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                IntegrationApiExceptionAddError("Rooms are not defined.", "Bad Data", source.RecordGuid, source.RecordKey);
                return null;
            }

            var room = entity.FirstOrDefault(r => r.Code.Equals(source.FixRoom, StringComparison.OrdinalIgnoreCase) && r.BuildingCode.Equals(fixBuilding, StringComparison.OrdinalIgnoreCase));
            if (room == null)
            {
                IntegrationApiExceptionAddError(string.Format("Room not found for key: '{0}' in building '{1}'.", source.FixRoom, fixBuilding),
                     "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                return null;
            }
            return new GuidObject2(room.Guid);
        }

        #endregion

        #region All Reference Data

        /// <summary>
        /// Rooms.
        /// </summary>
        IEnumerable<Domain.Base.Entities.Room> _rooms = null;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> RoomsAsync(bool bypassCache)
        {
            return _rooms ?? (_rooms = await _roomRepository.GetRoomsAsync(bypassCache));
        }


        /// <summary>
        /// Item Conditions.
        /// </summary>
        IEnumerable<Domain.Base.Entities.ItemCondition> _itemCondition = null;
        private async Task<IEnumerable<Domain.Base.Entities.ItemCondition>> ItemsConditionsAsync(bool bypassCache)
        {
            return _itemCondition ?? (_itemCondition = await _referenceDataRepository.GetItemConditionsAsync(bypassCache));
        }

        /// <summary>
        /// Acquisition Methods.
        /// </summary>
        IEnumerable<Domain.Base.Entities.AcquisitionMethod> _acquisitionMethod = null;
        private async Task<IEnumerable<Domain.Base.Entities.AcquisitionMethod>> AcquisitionMethodsAsync(bool bypassCache)
        {
            return _acquisitionMethod ?? (_acquisitionMethod = await _referenceDataRepository.GetAcquisitionMethodsAsync(bypassCache));
        }

        /// <summary>
        /// Host Country.
        /// </summary>
        string hostCountry = null;
        private async Task<string> HostCountryAsync()
        {
            return hostCountry ?? (hostCountry = await _referenceDataRepository.GetHostCountryAsync());
        }

        #endregion
    }
}