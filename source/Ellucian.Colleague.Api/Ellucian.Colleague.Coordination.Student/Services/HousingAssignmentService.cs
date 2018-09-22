//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class HousingAssignmentService : BaseCoordinationService, IHousingAssignmentService
    {

        private readonly IHousingAssignmentRepository _housingAssignmentRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="housingAssignmentRepository"></param>
        /// <param name="housingRequestRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="roomRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="studentReferenceDataRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public HousingAssignmentService(

            IHousingAssignmentRepository housingAssignmentRepository,
            IPersonRepository personRepository,
            ITermRepository termRepository,
            IRoomRepository roomRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _housingAssignmentRepository = housingAssignmentRepository;
            _personRepository = personRepository;
            _termRepository = termRepository;
            _roomRepository = roomRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        #region All the reference data

        /// <summary>
        /// Clears from the cache.
        /// </summary>
        private void ClearReferenceData()
        {
            _academicPeriods = null;
            _billingOverrideReasons = null;
            _rooms = null;
            _housingResidentType = null;
            _accountingCodes = null;
        }

        //Academic Periods
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriods()
        {
            if (_academicPeriods == null)
            {
                var termEntities = await _termRepository.GetAsync();
                _academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
            }
            return _academicPeriods;
        }

        //Billing Override Reasons
        private IEnumerable<Domain.Student.Entities.BillingOverrideReasons> _billingOverrideReasons;
        private async Task<IEnumerable<Domain.Student.Entities.BillingOverrideReasons>> GetBillingOverrideReasons(bool bypassCache)
        {
            if (_billingOverrideReasons == null)
            {
                _billingOverrideReasons = await _studentReferenceDataRepository.GetBillingOverrideReasonsAsync(bypassCache);
            }
            return _billingOverrideReasons;
        }

        //Rooms
        private IEnumerable<Domain.Base.Entities.Room> _rooms;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetRooms(bool bypassCache)
        {
            if (_rooms == null)
            {
                _rooms = await _roomRepository.GetRoomsAsync(bypassCache);
            }
            return _rooms;
        }

        //GetHousingResidentTypesAsync
        private IEnumerable<Domain.Student.Entities.HousingResidentType> _housingResidentType;
        private async Task<IEnumerable<Domain.Student.Entities.HousingResidentType>> GetHousingResidentTypes(bool bypassCache)
        {
            if (_housingResidentType == null)
            {
                _housingResidentType = await _studentReferenceDataRepository.GetHousingResidentTypesAsync(bypassCache);
            }
            return _housingResidentType;
        }

        //AccountingCodes
        private IEnumerable<Domain.Student.Entities.AccountingCode> _accountingCodes;
        private async Task<IEnumerable<Domain.Student.Entities.AccountingCode>> GetAccountingCodes(bool bypassCache)
        {
            if (_accountingCodes == null)
            {
                _accountingCodes = await _studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache);
            }
            return _accountingCodes;
        }

        /// <summary>
        /// Get Host Country
        /// </summary>
        /// <returns>string representng the host country</returns>
        private string _hostCountry;
        private async Task<string> GetHostCountryAsync()
        {
            if (_hostCountry == null)
            {
                _hostCountry = await _studentReferenceDataRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }

        //PersonGuids
        /// <summary>
        /// Person ids, guid key value pairs
        /// </summary>
        private IDictionary<string, string> _personGuidsDict;
        private async Task<IDictionary<string, string>> GetPersonGuidsAsync()
        {
            if (_personIds != null && _personIds.Any())
            {
                if (_personGuidsDict == null)
                {
                    IDictionary<string, string> dict = await _housingAssignmentRepository.GetPersonGuidsAsync(_personIds);
                    if (dict != null && dict.Any())
                    {
                        _personGuidsDict = new Dictionary<string, string>();
                        dict.ToList().ForEach(i =>
                        {
                            if (!_personGuidsDict.ContainsKey(i.Key))
                            {
                                _personGuidsDict.Add(i.Key, i.Value);
                            }
                        });
                    }
                }
            }
            return _personGuidsDict;
        }

        /// <summary>
        /// Builds person record keys local cache
        /// </summary>
        private List<string> _personIds;
        private void BuildLocalPersonGuids(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment> housingAssignmentEntities)
        {
            _personIds = new List<string>();
            if (housingAssignmentEntities != null && housingAssignmentEntities.Any())
            {
                List<string> personIds = new List<string>();

                var ids = housingAssignmentEntities.Select(i => i.StudentId).Distinct().ToList();
                if (ids != null && ids.Any())
                {
                    personIds.AddRange(ids);
                }
                _personIds.AddRange(personIds);
            }
        }
        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all housing-assignments
        /// </summary>
        /// <returns>Collection of HousingAssignments DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                CheckViewHousingAssignmentPermissions();

                var housingAssignmentsCollection = new List<Ellucian.Colleague.Dtos.HousingAssignment>();

                var housingAssignmentsEntities = await _housingAssignmentRepository.GetHousingAssignmentsAsync(offset, limit, bypassCache);

                var totalCount = housingAssignmentsEntities.Item2;

                if (housingAssignmentsEntities != null && housingAssignmentsEntities.Item1.Any())
                {
                    BuildLocalPersonGuids(housingAssignmentsEntities.Item1);

                    foreach (var housingAssignments in housingAssignmentsEntities.Item1)
                    {
                        housingAssignmentsCollection.Add(await ConvertHousingAssignmentsEntityToDto(housingAssignments, bypassCache));
                    }
                }

                return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(housingAssignmentsCollection, totalCount);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a HousingAssignments from its GUID
        /// </summary>
        /// <returns>HousingAssignments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.HousingAssignment> GetHousingAssignmentByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                CheckViewHousingAssignmentPermissions();

                var housingAssignmentEntity = await _housingAssignmentRepository.GetHousingAssignmentByGuidAsync(guid);

                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { housingAssignmentEntity });

                return await ConvertHousingAssignmentsEntityToDto(housingAssignmentEntity, bypassCache);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Create housing assignment.
        /// </summary>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        public async Task<Dtos.HousingAssignment> CreateHousingAssignmentAsync(Dtos.HousingAssignment housingAssignmentDto)
        {
            if (housingAssignmentDto == null)
            {
                throw new ArgumentNullException("housingAssignmentDto", "Must provide a guid for housing assignment create.");
            }

            try
            {
                CheckCreateUpdateHousingAssignmentPermissions();

                _housingAssignmentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                var housingAssignmentEntity = await ConvertDtoToEntity(null, housingAssignmentDto);
                var createdHousingAssignmentEntity = await _housingAssignmentRepository.UpdateHousingAssignmentAsync(housingAssignmentEntity);
                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { createdHousingAssignmentEntity });

                return await this.ConvertHousingAssignmentsEntityToDto(createdHousingAssignmentEntity, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Update housing assignment.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        public async Task<Dtos.HousingAssignment> UpdateHousingAssignmentAsync(string guid, Dtos.HousingAssignment housingAssignmentDto)
        {
            if (housingAssignmentDto == null)
            {
                throw new ArgumentNullException("housingAssignmentDto", "Must provide a guid for housing assignment update.");
            }

            try
            {
                // get the ID associated with the incoming guid
                var housingAssignmentId = await _housingAssignmentRepository.GetHousingAssignmentKeyAsync(housingAssignmentDto.Id);
                if (!string.IsNullOrEmpty(housingAssignmentId))
                {
                    CheckCreateUpdateHousingAssignmentPermissions();

                    _housingAssignmentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    Ellucian.Colleague.Domain.Student.Entities.HousingAssignment housingAssignmentEntity = await ConvertDtoToEntity(guid, housingAssignmentDto);

                    Domain.Student.Entities.HousingAssignment updatedHousingAssignmentEntity = await _housingAssignmentRepository.UpdateHousingAssignmentAsync(housingAssignmentEntity);

                    BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { housingAssignmentEntity });

                    ClearReferenceData();

                    return await this.ConvertHousingAssignmentsEntityToDto(updatedHousingAssignmentEntity, true);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return await CreateHousingAssignmentAsync(housingAssignmentDto);
        }

        /// <summary>
        /// Convert housing request dto to entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.HousingAssignment> ConvertDtoToEntity(string guid, Dtos.HousingAssignment source)
        {
            try
            {
                //person
                var personKey = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
                if (string.IsNullOrEmpty(personKey))
                {
                    throw new KeyNotFoundException(string.Format("No person found. guid: {0}", source.Person.Id));
                }

                //room
                var roomEntity = (await GetRooms(true)).FirstOrDefault(i => i.Guid.Equals(source.Room.Id, StringComparison.OrdinalIgnoreCase));
                if (roomEntity == null)
                {
                    throw new KeyNotFoundException(string.Format("No room found. guid: {0}", source.Room.Id));
                }

                //Create the domain entity
                Domain.Student.Entities.HousingAssignment destinationEntity = string.IsNullOrEmpty(guid) ?
                    new Domain.Student.Entities.HousingAssignment(source.Id, personKey, roomEntity.Id, source.StartOn.Value, source.EndOn.Value) :
                    new Domain.Student.Entities.HousingAssignment(source.Id, await GetHousingAssignmentKeyAsync(source), personKey, roomEntity.Id, source.StartOn.Value, source.EndOn.Value);

                //academicPeriod
                if (source.RatePeriod == RatePeriod.Term && string.IsNullOrEmpty(source.AcademicPeriod.Id))
                {
                    throw new InvalidOperationException("A term must be specified if the rate period is Term .");
                }

                if (source.AcademicPeriod != null && !string.IsNullOrEmpty(source.AcademicPeriod.Id))
                {
                    var acadPeriod = (await GetAcademicPeriods()).FirstOrDefault(i => i.Guid.Equals(source.AcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                    if (acadPeriod == null)
                    {
                        throw new KeyNotFoundException(string.Format("No academic period found. guid: {0}", source.AcademicPeriod.Id));
                    }
                    destinationEntity.Term = acadPeriod.Code;
                }
                //status
                var status = string.Empty;
                if (source.Status != HousingAssignmentsStatus.NotSet)
                {
                    destinationEntity.Status = ConvertStatusDtoToEntity(source.Status);
                }

                if (!source.StatusDate.HasValue)
                {
                    throw new InvalidOperationException("Status date is required.");
                }
                if (source.StatusDate.HasValue)
                {
                    destinationEntity.StatusDate = source.StatusDate.Value;
                }

                //request
                if (source.HousingRequest != null && !string.IsNullOrEmpty(source.HousingRequest.Id))
                {
                    destinationEntity.HousingRequest = source.HousingRequest.Id;
                }

                //roomRate
                if (source.RoomRate != null && string.IsNullOrEmpty(source.RoomRate.Id))
                {
                    throw new InvalidOperationException("Id is required for the room rate.");
                }
                if (source.RoomRate != null && !string.IsNullOrEmpty(source.RoomRate.Id))
                {
                    var rmRate = (await RoomRatesAsync(true)).FirstOrDefault(i => i.Guid.Equals(source.RoomRate.Id, StringComparison.OrdinalIgnoreCase));
                    if (rmRate == null)
                    {
                        throw new KeyNotFoundException(string.Format("No room rate found for guid: {0}", source.RoomRate.Id));
                    }
                    if (rmRate.EndDate.HasValue && rmRate.EndDate.Value > source.EndOn.Value)
                    {
                        throw new InvalidOperationException("The specified Housing Rate Table is not active on housing assignment start date.");
                    }
                    destinationEntity.RoomRateTable = rmRate.Code;
                }

                //ratePeriod
                if (source.RoomRate != null && string.IsNullOrEmpty(source.RoomRate.Id) && source.RatePeriod.HasValue)
                {
                    throw new InvalidOperationException("A valid roomrate id is required when rate period is specified.");
                }

                if (source.RatePeriod.HasValue && source.RatePeriod != Dtos.EnumProperties.RatePeriod.NotSet)
                {
                    destinationEntity.RatePeriod = ConvertRatePeriodDtoToEntity(source.RatePeriod);
                }

                //rateoverride
                if (source.RateOverride != null)
                {
                    //RateOverride
                    if (source.RateOverride.HousingAssignmentRate != null && source.RateOverride.HousingAssignmentRate.RateValue == null)
                    {
                        throw new InvalidOperationException("Rate value is required for the rate override.");
                    }

                    if (source.RateOverride.HousingAssignmentRate != null && source.RateOverride.HousingAssignmentRate.RateValue != null &&
                        source.RateOverride.HousingAssignmentRate.RateValue < 0)
                    {
                        throw new ArgumentException("The override rate value must be set greater than zero. ");
                    }

                    //rateOverride.HousingAssignmentRate.RateValue
                    destinationEntity.RateOverride = source.RateOverride.HousingAssignmentRate.RateValue;

                    if (source.RateOverride.HousingAssignmentRate.RateValue != null && source.RateOverride.HousingAssignmentRate.RateCurrency != Dtos.EnumProperties.CurrencyIsoCode.USD &&
                        source.RateOverride.HousingAssignmentRate.RateCurrency != Dtos.EnumProperties.CurrencyIsoCode.CAD)
                    {
                        throw new ArgumentException("The override rate currency must be set to either 'USD' or 'CAD'. ");
                    }

                    //RateOverrideReason
                    if (source.RateOverride.RateOverrideReason != null && string.IsNullOrEmpty(source.RateOverride.RateOverrideReason.Id))
                    {
                        throw new InvalidOperationException("An Override Rate Reason must be specified with an Override Rate.");
                    }
                    var rateOverrideReason = (await GetBillingOverrideReasons(true)).FirstOrDefault(i => i.Guid.Equals(source.RateOverride.RateOverrideReason.Id, StringComparison.OrdinalIgnoreCase));
                    if (rateOverrideReason == null)
                    {
                        throw new KeyNotFoundException(string.Format("No rate override reason found for guid: {0}", source.RateOverride.RateOverrideReason.Id));
                    }
                    destinationEntity.RateOverrideReason = rateOverrideReason.Code;
                }

                //additionalCharges
                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null || (i.AccountingCode != null && string.IsNullOrEmpty(i.AccountingCode.Id)))))
                {
                    throw new InvalidOperationException(string.Format("Accounting code is required for additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null || (i.AccountingCode != null && i.HousingAssignmentRate == null))))
                {
                    throw new InvalidOperationException(string.Format("Charge is required for additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null ||
                    (i.AccountingCode != null && i.HousingAssignmentRate != null && i.HousingAssignmentRate.RateCurrency == CurrencyIsoCode.NotSet))))
                {
                    throw new InvalidOperationException(string.Format("Currency is required for the additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null ||
                    (i.AccountingCode != null && i.HousingAssignmentRate != null && (i.HousingAssignmentRate.RateValue == null || !i.HousingAssignmentRate.RateValue.HasValue)))))
                {
                    throw new InvalidOperationException("Value is required for the additional charges.");
                }

                if (source.AdditionalCharges != null && source.AdditionalCharges.Any())
                {
                    List<ArAdditionalAmount> arAddlAmountList = new List<ArAdditionalAmount>();
                    foreach (var additionalCharge in source.AdditionalCharges)
                    {
                        var acctCode = (await this.GetAccountingCodes(true)).FirstOrDefault(i => i.Guid.Equals(additionalCharge.AccountingCode.Id));
                        if (acctCode == null)
                        {
                            throw new KeyNotFoundException(string.Format("No accounting code found for guid: {0}", additionalCharge.AccountingCode.Id));
                        }
                        ArAdditionalAmount addlAmt = new ArAdditionalAmount()
                        {
                            AraaArCode = acctCode.Code
                        };
                        if (additionalCharge.HousingAssignmentRate.RateValue.HasValue)
                        {
                            if (additionalCharge.HousingAssignmentRate.RateValue.Value > 0)
                            {
                                addlAmt.AraaChargeAmt = additionalCharge.HousingAssignmentRate.RateValue.Value;
                            }
                            else if (additionalCharge.HousingAssignmentRate.RateValue.Value < 0)
                            {
                                addlAmt.AraaCrAmt = additionalCharge.HousingAssignmentRate.RateValue.Value;
                            }
                        }
                        arAddlAmountList.Add(addlAmt);
                    }
                    destinationEntity.ArAdditionalAmounts = arAddlAmountList;
                }

                //residentType
                if (source.ResidentType != null && string.IsNullOrEmpty(source.ResidentType.Id))
                {
                    throw new InvalidOperationException("Id is required for the resident type.");
                }

                if (source.ResidentType != null)
                {
                    var resType = (await GetHousingResidentTypes(true)).FirstOrDefault(i => i.Guid.Equals(source.ResidentType.Id, StringComparison.OrdinalIgnoreCase));
                    if (resType == null)
                    {
                        throw new KeyNotFoundException(string.Format("No resident type found for guid: {0}", source.ResidentType.Id));
                    }
                    destinationEntity.ResidentStaffIndicator = resType.Code;
                }


                //contractNumber
                if (!string.IsNullOrEmpty(source.ContractNumber))
                {
                    if (source.ContractNumber.Length > 16)
                    {
                        throw new InvalidOperationException("Contract number cannot be more than 16 characters.");
                    }
                    destinationEntity.ContractNumber = source.ContractNumber;
                }

                //comments
                if (!string.IsNullOrEmpty(source.Comment))
                {
                    destinationEntity.Comments = source.Comment;
                }

                return destinationEntity;
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(string.Concat(e.Message, " housing assignment guid: ", source.Id));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Convert rate period dto to entity.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string ConvertRatePeriodDtoToEntity(RatePeriod? source)
        {
            switch (source.Value)
            {
                case RatePeriod.Day:
                    return "D";
                case RatePeriod.Week:
                    return "W";
                case RatePeriod.Month:
                    return "M";
                case RatePeriod.Term:
                    return "T";
                case RatePeriod.Year:
                    return "Y";
                case RatePeriod.NotSet:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Convert status dto to entity.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string ConvertStatusDtoToEntity(HousingAssignmentsStatus source)
        {
            switch (source)
            {
                case HousingAssignmentsStatus.Pending:
                    return "R";
                case HousingAssignmentsStatus.Assigned:
                    return "A";
                case HousingAssignmentsStatus.Canceled:
                    return "C";
                case HousingAssignmentsStatus.Terminated:
                    return "T";
                case HousingAssignmentsStatus.Prorated:
                    return "L";
                case HousingAssignmentsStatus.NotSet:
                default:
                    throw new InvalidOperationException("Status is required.");
            }
        }

        /// <summary>
        /// Gets housing assignment key.
        /// </summary>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        private async Task<string> GetHousingAssignmentKeyAsync(Dtos.HousingAssignment housingAssignmentDto)
        {
            return await _housingAssignmentRepository.GetHousingAssignmentKeyAsync(housingAssignmentDto.Id);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HousingAssignments domain entity to its corresponding HousingAssignments DTO
        /// </summary>
        /// <param name="source">HousingAssignments domain entity</param>
        /// <returns>HousingAssignments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.HousingAssignment> ConvertHousingAssignmentsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            var housingAssignment = new Ellucian.Colleague.Dtos.HousingAssignment();

            housingAssignment.Id = source.Guid;
            housingAssignment.HousingRequest = string.IsNullOrEmpty(source.HousingRequest) ? null : new GuidObject2(source.HousingRequest);
            housingAssignment.Person = await ConvertPersonEntityToDto(source);
            housingAssignment.Room = await ConvertRoomEntityToDto(string.Concat(source.Building, "*", source.RoomId), bypassCache);
            housingAssignment.AcademicPeriod = await ConvertTermEntityToDto(source);
            if (!source.StartOn.HasValue)
            {
                throw new InvalidOperationException(string.Format("Start on date is required. Guid: {0}", source.Guid));
            }
            housingAssignment.StartOn = source.StartOn.Value;
            housingAssignment.EndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTimeOffset?);
            var statusStatusDate = ConvertStatusEntityToDto(source);

            housingAssignment.Status = statusStatusDate.Item1;
            housingAssignment.StatusDate = statusStatusDate.Item2.HasValue ? statusStatusDate.Item2.Value.Date : default(DateTime?);
            housingAssignment.ContractNumber = string.IsNullOrEmpty(source.ContractNumber) ? null : source.ContractNumber;
            housingAssignment.Comment = string.IsNullOrEmpty(source.Comments) ? null : source.Comments;
            housingAssignment.RoomRate = await ConvertRoomRateEntityToDto(source, bypassCache);
            housingAssignment.RatePeriod = ConvertEntityRatePeriodToDto(source.RatePeriod);
            housingAssignment.RateOverride = source.RateOverride.HasValue ? await ConvertOverrideEntityToDto(source.RateOverride, source.RateOverrideReason, bypassCache) : null;
            housingAssignment.AdditionalCharges = await ConvertAdditionalChargesEntityToDto(source, bypassCache);
            housingAssignment.ResidentType = await ConvertHousingResTypeEntityToDto(source.ResidentStaffIndicator, bypassCache);

            return housingAssignment;
        }

        /// <summary>
        /// Converts room rate entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertRoomRateEntityToDto(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source.RoomRateTable))
            {
                return null;
            }
            var roomRate = (await RoomRatesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.RoomRateTable, StringComparison.OrdinalIgnoreCase));
            if (roomRate == null)
            {
                throw new KeyNotFoundException(string.Format("No room rate for code{0}, guid: {1}", source.RoomRateTable, source.Guid));
            }
            return new GuidObject2(roomRate.Guid);
        }

        /// <summary>
        /// Converts person to student guid object
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertPersonEntityToDto(Domain.Student.Entities.HousingAssignment source)
        {
            if (string.IsNullOrEmpty(source.StudentId))
            {
                throw new InvalidOperationException(string.Format("Student id is required. Guid: {0}", source.Guid));
            }

            var studentGuidKP = (await this.GetPersonGuidsAsync()).FirstOrDefault(i => i.Key.Equals(source.StudentId, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(studentGuidKP.Value))
            {
                throw new KeyNotFoundException(string.Format("No student guid found for id: {0}", source.StudentId));
            }
            return new GuidObject2(studentGuidKP.Value);
        }

        /// <summary>
        /// Converts room to guid object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertRoomEntityToDto(string source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            var room = (await this.GetRooms(bypassCache)).FirstOrDefault(i => i.Id.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (room == null)
            {
                throw new KeyNotFoundException(string.Format("No room found for code {0}", source));
            }
            return new GuidObject2(room.Guid);
        }

        /// <summary>
        /// Converts term to academic period guid object
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertTermEntityToDto(Domain.Student.Entities.HousingAssignment source)
        {
            if (string.IsNullOrEmpty(source.Term))
            {
                return null;
            }
            var acadPeriod = (await this.GetAcademicPeriods()).FirstOrDefault(i => i.Code.Equals(source.Term));
            if (acadPeriod == null)
            {
                throw new KeyNotFoundException(string.Format("No academic period found for term: {0}", source.Term));
            }
            return new GuidObject2(acadPeriod.Guid);
        }

        /// <summary>
        /// Converts status to HousingAssignmentsStatus dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Tuple<HousingAssignmentsStatus, DateTimeOffset?> ConvertStatusEntityToDto(Domain.Student.Entities.HousingAssignment source)
        {
            if (source.Statuses == null || !source.Statuses.Any())
            {
                throw new InvalidOperationException(string.Format("Status is required. Guid: {0}", source.Guid));
            }

            // Colleague considers the topmost/first status in the list to be the effective status.  Colleague does not keep it in sorted order.
            var status = source.Statuses.FirstOrDefault();

            switch (status.Status)
            {
                case "A":
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Assigned, (DateTimeOffset?)status.StatusDate);
                case "C":
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Canceled, (DateTimeOffset?)status.StatusDate);
                case "T":
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Terminated, (DateTimeOffset?)status.StatusDate);
                case "R":
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Pending, (DateTimeOffset?)status.StatusDate);
                case "L":
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Prorated, (DateTimeOffset?)status.StatusDate);
                default:
                    return new Tuple<HousingAssignmentsStatus, DateTimeOffset?>(HousingAssignmentsStatus.Pending, (DateTimeOffset?)status.StatusDate);
            }
        }

        /// <summary>
        /// Converts to RatePersion dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private RatePeriod? ConvertEntityRatePeriodToDto(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            switch (source.ToUpperInvariant())
            {
                case "D":
                    return RatePeriod.Day;
                case "W":
                    return RatePeriod.Week;
                case "M":
                    return RatePeriod.Month;
                case "Y":
                    return RatePeriod.Year;
                case "T":
                    return RatePeriod.Term;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts to rate override dto
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="reason"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<HousingAssignmentRateOverrideProperty> ConvertOverrideEntityToDto(decimal? rate, string reason, bool bypassCache)
        {
            HousingAssignmentRateOverrideProperty overrideRate = new HousingAssignmentRateOverrideProperty();

            if (rate.HasValue)
            {
                overrideRate.HousingAssignmentRate = new HousingAssignmentRateChargeProperty()
                {
                    RateValue = rate.Value,
                    RateCurrency = (await GetHostCountryAsync()).ToUpper().Equals("USA", StringComparison.OrdinalIgnoreCase) ? CurrencyIsoCode.USD : CurrencyIsoCode.NotSet
                };
            }

            if (!string.IsNullOrEmpty(reason))
            {
                var billingOverrideReason = (await this.GetBillingOverrideReasons(bypassCache)).FirstOrDefault(i => i.Code.Equals(reason, StringComparison.OrdinalIgnoreCase));
                if (billingOverrideReason == null)
                {
                    throw new KeyNotFoundException(string.Format("No billing override reason found for code {0}", reason));
                }
                overrideRate.RateOverrideReason = new GuidObject2(billingOverrideReason.Guid);
            }
            return overrideRate;
        }

        /// <summary>
        /// Converts to additional charges dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<HousingAssignmentAdditionalChargeProperty>> ConvertAdditionalChargesEntityToDto(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            List<HousingAssignmentAdditionalChargeProperty> addlCharges = new List<HousingAssignmentAdditionalChargeProperty>();

            if (source.ArAdditionalAmounts != null && source.ArAdditionalAmounts.Any())
            {
                foreach (var item in source.ArAdditionalAmounts)
                {
                    var accountingCode = (await this.GetAccountingCodes(bypassCache)).FirstOrDefault(i => i.Code.Equals(item.AraaArCode, StringComparison.OrdinalIgnoreCase));
                    if (accountingCode == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting code found for code: {0}", item));
                    }
                    HousingAssignmentAdditionalChargeProperty acctAddlCharges = new HousingAssignmentAdditionalChargeProperty()
                    {
                        AccountingCode = new GuidObject2(accountingCode.Guid),
                        HousingAssignmentRate = new HousingAssignmentRateChargeProperty()
                        {
                            RateValue = ConvertChargeCreditToValue(item.AraaChargeAmt, item.AraaCrAmt),
                            RateCurrency = (await GetHostCountryAsync()).ToUpper().Equals("USA", StringComparison.OrdinalIgnoreCase) ? CurrencyIsoCode.USD : CurrencyIsoCode.NotSet
                        }
                    };
                    addlCharges.Add(acctAddlCharges);
                }
            }

            return addlCharges.Any() ? addlCharges : null;
        }

        /// <summary>
        /// Converts to res type to guid object dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertHousingResTypeEntityToDto(string source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            var residentType = (await this.GetHousingResidentTypes(bypassCache)).FirstOrDefault(i => i.Code.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (residentType == null)
            {
                throw new KeyNotFoundException(string.Format("No resident type found for code {0}", source));
            }
            return new GuidObject2(residentType.Guid);
        }

        /// <summary>
        /// Converts to rate value dto
        /// </summary>
        /// <param name="chargeAmount"></param>
        /// <param name="creditAmount"></param>
        /// <returns></returns>
        private decimal? ConvertChargeCreditToValue(decimal? chargeAmount, decimal? creditAmount)
        {
            if (chargeAmount != null)
            {
                return chargeAmount;
            }
            if (creditAmount != null)
            {
                return creditAmount * -1;
            }
            if (chargeAmount == null && creditAmount == null)
            {
                return 0;
            }
            return null;
        }

        /// <summary>
        /// Checks housing assignment view permissions
        /// </summary>
        private void CheckViewHousingAssignmentPermissions()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.ViewHousingAssignment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view housing-assignments.");
                throw new PermissionsException("User is not authorized to view housing-assignments.");
            }
        }

        /// <summary>
        /// Checks housing assignment create or update permissions
        /// </summary>
        private void CheckCreateUpdateHousingAssignmentPermissions()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.CreateUpdateHousingAssignment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create or update housing-assignments.");
                throw new PermissionsException("User is not authorized to create or update housing-assignments.");
            }
        }

        /// <summary>
        /// Gets room rates.
        /// </summary>
        IEnumerable<RoomRate> _roomrates = null;
        private async Task<IEnumerable<RoomRate>> RoomRatesAsync(bool bypassCache)
        {
            if (_roomrates == null)
            {
                _roomrates = await _studentReferenceDataRepository.GetRoomRatesAsync(bypassCache);
            }
            return _roomrates;
        }
    }
}