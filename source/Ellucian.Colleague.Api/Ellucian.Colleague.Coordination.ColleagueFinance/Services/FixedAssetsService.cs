//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
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
            try
            {
                if (!await CheckViewFixedAssetsPermission())
                {
                    throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view fixed assets.");
                }
                
                var fixedAssetsEntities = await _fixedAssetsRepository.GetFixedAssetsAsync(offset, limit, bypassCache);
                if (fixedAssetsEntities != null && fixedAssetsEntities.Item1.Any())
                {
                    foreach (var fixedAssetntity in fixedAssetsEntities.Item1)
                    {
                        fixedAssets.Add(await ConvertFixedAssetsEntityToDtoAsync(fixedAssetntity, bypassCache));
                    }
                }
                return fixedAssets != null && fixedAssets.Any() ? new Tuple<IEnumerable<Dtos.FixedAssets>, int>(fixedAssets, fixedAssetsEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.FixedAssets>, int>(new List<Dtos.FixedAssets>(), 0);
            }
            catch (Exception)
            {
                throw;
            }
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
            try
            {
                if (!await CheckViewFixedAssetsPermission())
                {
                    throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view fixed assets.");
                }

                var fixedAssetEntity = await _fixedAssetsRepository.GetFixedAssetByIdAsync(guid);
                return await ConvertFixedAssetsEntityToDtoAsync(fixedAssetEntity, bypassCache);
            }
            catch (Exception)
            {
                throw;
            }
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
        private async Task<Ellucian.Colleague.Dtos.FixedAssets> ConvertFixedAssetsEntityToDtoAsync(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            var fixedAsset = new Ellucian.Colleague.Dtos.FixedAssets();
            var building = await ConvertEntityToToBuildingAsync(source.FixBuilding, bypassCache);

            fixedAsset.Id = source.RecordGuid;
            fixedAsset.Description = source.Description;
            fixedAsset.Tag = source.FixPropertyTag;
            fixedAsset.Type = await ConvertEntityToAssetTypeGuidObjectAsync(source, bypassCache);
            fixedAsset.Category = await ConvertEntityToAssetCategoryGuidObjectAsync(source, bypassCache);
            fixedAsset.CapitalizationStatus = ConvertEntityToCapitalizationStatus(source);
            fixedAsset.AcquisitionMethod = await ConvertEntityToAcquisiotionMethod(source, bypassCache);
            fixedAsset.Status = ConvertEntityToStatus(source.FixDisposalDate);
            fixedAsset.Condition = await ConvertEntityToItemConditionAsync(source, bypassCache);
            fixedAsset.Location = string.IsNullOrEmpty(source.FixLocation)? null : source.FixLocation;
            fixedAsset.Building = building != null ? new GuidObject2(building.Guid) : null;
            fixedAsset.Room = await ConvertEntityToRoomGuidObjectAsync(source, building, bypassCache);
            fixedAsset.InsuredValue = await ConvertEntityToInsuredValueDto(source.InsuranceAmountCoverage, bypassCache);
            fixedAsset.MarketValue = await ConvertEntityToInsuredValueDto(source.FixValueAmount, bypassCache);
            fixedAsset.AcquisitionCost = await ConvertEntityToInsuredValueDto(source.FixAcqisitionCost, bypassCache);
            fixedAsset.AccumulatedDepreciation = await ConvertEntityToInsuredValueDto(source.FixAllowAmount, bypassCache);
            fixedAsset.DepreciationMethod = string.IsNullOrEmpty(source.FixCalculationMethod)? null : source.FixCalculationMethod;
            fixedAsset.SalvageValue = await ConvertEntityToInsuredValueDto(source.FixSalvageValue, bypassCache);
            fixedAsset.UsefulLife = source.FixUsefulLife.HasValue? source.FixUsefulLife.Value : default(int?);
            fixedAsset.DepreciationExpenseAccount = ConvertEntityToAcquisiotionDeprExpAcctGuidObjectAsync(source.FixCalcAccount);
            fixedAsset.RenewalCost = await ConvertEntityToInsuredValueDto(source.FixRenewalAmount, bypassCache);
            fixedAsset.ResponsiblePersons = await ConvertEntityToPersonGuidObjectAsync(source);

            return fixedAsset;
        }

        #region Convert Methods

        /// <summary>
        /// Asset Type to GuidObject2.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAssetTypeGuidObjectAsync(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            if(source != null && string.IsNullOrEmpty(source.FixAssetType))
            {
                return null;
            }

            var entity = await AssetTypesAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                throw new InvalidOperationException("Asset types are not defined.");
            }

            var assetType = entity.FirstOrDefault(i => i.Code.Equals(source.FixAssetType, StringComparison.OrdinalIgnoreCase));
            if(assetType == null)
            {
                throw new KeyNotFoundException(string.Format("Asset type not found for key: {0}. Guid: {1}", source.FixAssetType, source.RecordGuid));
            }
            return new GuidObject2(assetType.Guid);
        }

        /// <summary>
        /// Asset Category to GuidObject2.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAssetCategoryGuidObjectAsync(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            if (source != null && string.IsNullOrEmpty(source.FixAssetCategory))
            {
                return null;
            }

            var entity = await AssetCategoriesAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                throw new InvalidOperationException("Asset categories are not defined.");
            }

            var assetCategory = entity.FirstOrDefault(i => i.Code.Equals(source.FixAssetCategory, StringComparison.OrdinalIgnoreCase));
            if (assetCategory == null)
            {
                throw new KeyNotFoundException(string.Format("Asset category not found for key: {0}. Guid: {1}", source.FixAssetCategory, source.RecordGuid));
            }
            return new GuidObject2(assetCategory.Guid);
        }

        /// <summary>
        /// Fixed Asset Capitalization Status enum.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private FixedAssetsCapitalizationStatus ConvertEntityToCapitalizationStatus(Domain.ColleagueFinance.Entities.FixedAssets source)
        {            
            if(source != null && string.IsNullOrEmpty(source.CapitalizationStatus))
            {
                throw new ArgumentNullException(string.Format("Capitalization status is required. Guid: {0}", source.RecordGuid));
            }

            switch (source.CapitalizationStatus.ToUpperInvariant())
            {
                case "Y":
                case "C":
                case "P":
                    return FixedAssetsCapitalizationStatus.Capitalized;
                case "N":
                    return FixedAssetsCapitalizationStatus.Noncapital;
                default:
                    throw new InvalidOperationException(string.Format("The value {0} is invalid. Guid: {1}", source.CapitalizationStatus, source.RecordGuid));
            }
        }

        /// <summary>
        /// Fixed Asset Acquisition Method enum.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<FixedAssetsAcquisitionMethod> ConvertEntityToAcquisiotionMethod(Domain.ColleagueFinance.Entities.FixedAssets source, bool bypassCache)
        {
            var acqMethods = await AcquisitionMethodsAsync(bypassCache);
            if(acqMethods == null)
            {
                throw new InvalidOperationException("Acquisition methods are not defined.");
            }
            var acqMethod = acqMethods.FirstOrDefault(i => i.Code.Equals(source.AcquisitionMethod, StringComparison.OrdinalIgnoreCase));
            if(acqMethod == null)
            {
                throw new KeyNotFoundException(string.Format("Acquisition method not found for key: {0}. Guid: {1}", source.AcquisitionMethod, source.RecordGuid));
            }

            switch (acqMethod.AcquisitionType.ToUpperInvariant())
            {
                case "P":
                    return FixedAssetsAcquisitionMethod.Purchased;
                case "L":
                    return FixedAssetsAcquisitionMethod.Leased;
                case "D":
                    return FixedAssetsAcquisitionMethod.Donation;
                default:
                    throw new InvalidOperationException(string.Format("The value {0} is invalid. Guid:{1}", source.AcquisitionMethod, source.RecordGuid));
            }
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
            if(source != null && string.IsNullOrEmpty(source.FixInvoiceCondition))
            {
                return null;
            }

            var entity = await ItemsConditionsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                throw new InvalidOperationException("Item conditions are not defined.");
            }

            var itemCondition = (entity).FirstOrDefault(i => i.Code.Equals(source.FixInvoiceCondition, StringComparison.OrdinalIgnoreCase));
            if(itemCondition == null)
            {
                throw new KeyNotFoundException(string.Format("Item condition not found for key: {0}. Guid: {1}", source.FixInvoiceCondition, source.RecordGuid));
            }
            return itemCondition.Description;
        }

        /// <summary>
        /// Converts source value to building.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Domain.Base.Entities.Building> ConvertEntityToToBuildingAsync(string source, bool bypassCache)
        {
            if(string.IsNullOrEmpty(source))
            {
                return null;
            }

            var entity = await BuildingsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                throw new InvalidOperationException("Buildings are not defined.");
            }

            var building = entity.FirstOrDefault(b => b.Code.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (building == null)
            {
                throw new KeyNotFoundException(string.Format("Building not found for key: {0}.", source));
            }
            return building;
        }

        /// <summary>
        /// Converts source value to room.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="building"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToRoomGuidObjectAsync(Domain.ColleagueFinance.Entities.FixedAssets source, Domain.Base.Entities.Building building, bool bypassCache)
        {
            if(building != null)
            {
                if(source != null && !string.IsNullOrEmpty(source.FixRoom))
                {
                    var entity = await RoomsAsync(bypassCache);
                    if (entity == null || !entity.Any())
                    {
                        throw new InvalidOperationException("Rooms are not defined.");
                    }

                    var room = entity.FirstOrDefault(r => r.Code.Equals(source.FixRoom, StringComparison.OrdinalIgnoreCase) && r.BuildingCode.Equals(building.Code, StringComparison.OrdinalIgnoreCase));
                    if(room == null)
                    {
                        throw new KeyNotFoundException(string.Format("Room not found for key: {0} in building {1}. Guid: {2}", source.FixRoom, building.Code, source.RecordGuid));
                    }
                    return new GuidObject2(room.Guid);
                }
            }
            return null;
        }

        /// <summary>
        /// Converts to various currency value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Amount2DtoProperty> ConvertEntityToInsuredValueDto(decimal? source,  bool bypassCache)
        {
            if(source.HasValue)
            {
                Amount2DtoProperty currencyValue =  new Amount2DtoProperty()
                {
                    Value = source.Value
                };

                var currencyStr = await HostCountryAsync(bypassCache);
                if (!string.IsNullOrEmpty(currencyStr))
                {
                    Dtos.EnumProperties.CurrencyIsoCode currency = Dtos.EnumProperties.CurrencyIsoCode.USD;
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                        CurrencyIsoCode.USD;
                    currencyValue.Currency = currency;
                }
                else
                {
                    throw new InvalidOperationException("Host country is not defined.");
                }
                return currencyValue;
            }
            return null;
        }

        /// <summary>
        /// Converts to acquisition cost to GuidObject2.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private GuidObject2 ConvertEntityToAcquisiotionDeprExpAcctGuidObjectAsync(string source)
        {
            if(!string.IsNullOrEmpty(source))
            {
                return new GuidObject2(source);
            }
            return null;
        }

        /// <summary>
        /// Converts person id to GuidObject2.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GuidObject2>> ConvertEntityToPersonGuidObjectAsync(Domain.ColleagueFinance.Entities.FixedAssets source)
        {
            List<GuidObject2> personIds = new List<GuidObject2>();
            if (source != null && !string.IsNullOrEmpty(source.FixStewerdId))
            {
                var person = await _personRepository.GetPersonGuidFromIdAsync(source.FixStewerdId);
                if (string.IsNullOrEmpty(person))
                {
                    throw new Exception(string.Format("Person not found for key: {0}. Guid: {1}", source.FixStewerdId, source.RecordGuid));
                }
                personIds.Add(new GuidObject2(person));
            }
            return personIds.Any() ? personIds : null;
        }

        #endregion

        #region All Reference Data

        /// <summary>
        /// Asset Categories reference data.
        /// </summary>
        IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AssetCategories> _assetCategories = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AssetCategories>> AssetCategoriesAsync(bool bypassCache)
        {
            return _assetCategories ?? (_assetCategories = await _financeReferenceDataRepository.GetAssetCategoriesAsync(bypassCache));
        }

        /// <summary>
        /// Asset Types reference data.
        /// </summary>
        IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes> _assetTypes = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes>> AssetTypesAsync(bool bypassCache)
        {
            return _assetTypes ?? (_assetTypes = await _financeReferenceDataRepository.GetAssetTypesAsync(bypassCache));
        }

        /// <summary>
        /// Rooms.
        /// </summary>
        IEnumerable<Domain.Base.Entities.Room> _rooms = null;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> RoomsAsync(bool bypassCache)
        {
            return _rooms ?? (_rooms = await _roomRepository.GetRoomsAsync(bypassCache));
        }

        /// <summary>
        /// Buildings.
        /// </summary>
        IEnumerable<Domain.Base.Entities.Building> _buildings = null;
        private async Task<IEnumerable<Domain.Base.Entities.Building>> BuildingsAsync(bool bypassCache)
        {
            return _buildings ?? (_buildings = await _referenceDataRepository.GetBuildings2Async(bypassCache));
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
        private async Task<string> HostCountryAsync(bool bypassCache)
        {
            return hostCountry ?? (hostCountry = await _fixedAssetsRepository.GetHostCountryAsync());
        }

        #endregion

        #region Permission Check
        
        /// <summary>
        /// Permissions code that allows an external system to perform the READ operation.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckViewFixedAssetsPermission()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(ColleagueFinancePermissionCodes.ViewFixedAssets))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}