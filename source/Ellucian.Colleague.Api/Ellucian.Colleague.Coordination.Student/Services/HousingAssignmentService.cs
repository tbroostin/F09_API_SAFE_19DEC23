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
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;

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
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, Dtos.HousingAssignment criteriaFilter, bool bypassCache = false)
        {

                //CheckViewHousingAssignmentPermissions();
                string person = string.Empty, term = string.Empty, status = string.Empty, startDate = string.Empty, endDate = string.Empty;
            if (criteriaFilter != null)
            {
                //process person guid filter
                var personGuid = criteriaFilter.Person != null ? criteriaFilter.Person.Id : string.Empty;
                if (!string.IsNullOrEmpty(personGuid))
                {
                    try
                    {
                        person = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(new List<Dtos.HousingAssignment>(), 0);
                    }
                }

                //process academicPeriod guid filter
                var academicPeriodGuid = criteriaFilter.AcademicPeriod != null ? criteriaFilter.AcademicPeriod.Id : string.Empty;
                if (!string.IsNullOrEmpty(academicPeriodGuid))
                {
                    try
                    {
                        var academicPeriods = await GetAcademicPeriods();
                        if (academicPeriods == null)
                        {
                            return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(new List<Dtos.HousingAssignment>(), 0);
                        }
                        var academicPeriodEntity = academicPeriods.FirstOrDefault(mp => mp.Guid == academicPeriodGuid);
                        if (academicPeriodEntity == null)
                        {
                            return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(new List<Dtos.HousingAssignment>(), 0);
                        }
                        term = academicPeriodEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(new List<Dtos.HousingAssignment>(), 0);
                    }
                }
                //process status filter
                if (criteriaFilter.Status != null)
                    status = ConvertStatusDtoToEntity(criteriaFilter.Status);
                //process startOn and endOn filter
                try
                {
                    startDate = criteriaFilter.StartOn.HasValue ? await ConvertDateArgument(criteriaFilter.StartOn.ToString()) : string.Empty;
                    endDate = criteriaFilter.EndOn.HasValue ? await ConvertDateArgument(criteriaFilter.EndOn.ToString()) : string.Empty;
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(new List<Dtos.HousingAssignment>(), 0);
                }
            }

            try
            {
                var housingAssignmentsCollection = new List<Ellucian.Colleague.Dtos.HousingAssignment>();

                var housingAssignmentsEntities = await _housingAssignmentRepository.GetHousingAssignmentsAsync(offset, limit, person, term, status, startDate, endDate, bypassCache);

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
                //CheckViewHousingAssignmentPermissions();

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
                //CheckCreateUpdateHousingAssignmentPermissions();

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
                    //CheckCreateUpdateHousingAssignmentPermissions();

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
                //person is required
                if (source.Person == null || string.IsNullOrEmpty(source.Person.Id))
                {
                    throw new InvalidOperationException("Person is required.");
                }            
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
                    new Domain.Student.Entities.HousingAssignment(source.Id, await GetHousingAssignmentKeyAsync(source.Id), personKey, roomEntity.Id, source.StartOn.Value,
                    source.EndOn.Value);

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
        private string ConvertStatusDtoToEntity(HousingAssignmentsStatus? source)
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
                    return string.Empty;
                default:
                    // Used for filter so issue no error if problem found.  (Need to return empty set instead.)
                    return null;
            }
        }

        /// <summary>
        /// Gets housing assignment key.
        /// </summary>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        private async Task<string> GetHousingAssignmentKeyAsync(string id)
        {
            return await _housingAssignmentRepository.GetHousingAssignmentKeyAsync(id);
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
                throw new InvalidOperationException(string.Format("Person id is required. Guid: {0}", source.Guid));
            }

            var studentGuidKP = (await this.GetPersonGuidsAsync()).FirstOrDefault(i => i.Key.Equals(source.StudentId, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(studentGuidKP.Value))
            {
                throw new KeyNotFoundException(string.Format("No person guid found for id: {0}", source.StudentId));
            }
            return new GuidObject2(studentGuidKP.Value);
        }

        /// <summary>
        /// Converts person to student guid object
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertPersonEntityToDto2(Domain.Student.Entities.HousingAssignment source)
        {
            if (string.IsNullOrEmpty(source.StudentId))
            {
                IntegrationApiExceptionAddError("Student id is required.", guid: source.Guid, id: source.RecordKey);
                return null;
            }
            try
            {
                var studentGuidKP = (await this.GetPersonGuidsAsync()).FirstOrDefault(i => i.Key.Equals(source.StudentId, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(studentGuidKP.Value))
                {
                    IntegrationApiExceptionAddError("No student guid found for id: " + source.StudentId, guid: source.Guid, id: source.RecordKey);
                    return null;
                }
                return new GuidObject2(studentGuidKP.Value);
            }
            catch
            {
                IntegrationApiExceptionAddError("No student guid found for id: " + source.StudentId, guid: source.Guid, id: source.RecordKey);
                return null;
            }
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
        /// Converts room to guid object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertRoomEntityToDto2(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            var buildingRoom = string.Concat(source.Building, "*", source.RoomId);
            if (string.IsNullOrEmpty(buildingRoom))
            {
                return null;
            }
            try
            {
                var room = (await this.GetRooms(bypassCache)).FirstOrDefault(i => i.Id.Equals(buildingRoom, StringComparison.OrdinalIgnoreCase));
                return new GuidObject2(room.Guid);
            }
            catch
            {
                IntegrationApiExceptionAddError("No room found for code: " + buildingRoom, guid: source.Guid, id: source.RecordKey);
                return null;
            }            
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
        /// Converts term to academic period guid object
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertTermEntityToDto2(Domain.Student.Entities.HousingAssignment source)
        {
            if (string.IsNullOrEmpty(source.Term))
            {
                return null;
            }
            try
            {
                var acadPeriod = (await this.GetAcademicPeriods()).FirstOrDefault(i => i.Code.Equals(source.Term));
                return new GuidObject2(acadPeriod.Guid);
            }
            catch
            {
                IntegrationApiExceptionAddError("No academic period found for term: " + source.Term, guid: source.Guid, id: source.RecordKey);
                return null;
            }            
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
        /// Converts status to HousingAssignmentsStatus dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Tuple<HousingAssignmentsStatus, DateTimeOffset?> ConvertStatusEntityToDto2(Domain.Student.Entities.HousingAssignment source)
        {
            if (source.Statuses == null || !source.Statuses.Any())
            {
                IntegrationApiExceptionAddError("Status is required.", guid: source.Guid, id: source.RecordKey);
                return null;
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
        /// Converts to rate override dto
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="reason"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<HousingAssignmentRateOverrideProperty> ConvertOverrideEntityToDto2(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            var rate = source.RateOverride;
            var reason = source.RateOverrideReason;
                
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
                    IntegrationApiExceptionAddError("No billing override reason found for code: " + reason, guid: source.Guid, id: source.RecordKey);
                    return null;
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
        /// Converts to additional charges dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<HousingAssignmentAdditionalChargeProperty>> ConvertAdditionalChargesEntityToDto2(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            List<HousingAssignmentAdditionalChargeProperty> addlCharges = new List<HousingAssignmentAdditionalChargeProperty>();

            if (source.ArAdditionalAmounts != null && source.ArAdditionalAmounts.Any())
            {
                foreach (var item in source.ArAdditionalAmounts)
                {
                    try
                    {
                        var accountingCode = (await this.GetAccountingCodes(bypassCache)).FirstOrDefault(i => i.Code.Equals(item.AraaArCode, StringComparison.OrdinalIgnoreCase)); 
                        if (accountingCode == null)
                        {
                            if (!string.IsNullOrEmpty(item.AraaArCode))
                            {
                                IntegrationApiExceptionAddError("No accounting code found for code: " + item.AraaArCode, guid: source.Guid, id: source.RecordKey);
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("No accounting code found.", guid: source.Guid, id: source.RecordKey);
                            }
                            return null;
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
                    catch
                    {
                        if (!string.IsNullOrEmpty(item.AraaArCode))
                        {
                            IntegrationApiExceptionAddError("No accounting code found for code: " + item.AraaArCode, guid: source.Guid, id: source.RecordKey);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("No accounting code found.", guid: source.Guid, id: source.RecordKey);
                        }
                        return null;
                    }
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
        /// Converts to res type to guid object dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertHousingResTypeEntityToDto2(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source.ResidentStaffIndicator))
            {
                return null;
            }
            try
            {
                var residentType = (await this.GetHousingResidentTypes(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.ResidentStaffIndicator, StringComparison.OrdinalIgnoreCase));
                return new GuidObject2(residentType.Guid);
            }
            catch
            {
                IntegrationApiExceptionAddError("No resident type found for code: " + source.ResidentStaffIndicator, guid: source.Guid, id: source.RecordKey);
                return null;
            }            
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
            if (!HasPermission(StudentPermissionCodes.ViewHousingAssignment) && !HasPermission(StudentPermissionCodes.CreateUpdateHousingAssignment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view housing-assignments.");
                throw new PermissionsException("User is not authorized to view housing-assignments.");
            }
        }

        /// <summary>
        /// Checks housing assignment view permissions
        /// </summary>
        private void CheckViewHousingAssignmentPermissions2()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.ViewHousingAssignment) && !HasPermission(StudentPermissionCodes.CreateUpdateHousingAssignment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view housing-assignments.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view housing-assignments.", "Access.Denied",
                    httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
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
        /// Checks housing assignment create or update permissions
        /// </summary>
        private void CheckCreateUpdateHousingAssignmentPermissions2()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.CreateUpdateHousingAssignment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create or update housing-assignments.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to create or update housing-assignments.", "Access.Denied",
                    httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
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

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _studentReferenceDataRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                // Used for filter so issue no error if problem found.  (Need to return empty set instead.)
                return null;
            }
        }

        #region 16.0.0

        /// <summary>
        /// Get ALL 16.0.0
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaFilter"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<HousingAssignment2>, int>> GetHousingAssignments2Async(int offset, int limit, HousingAssignment2 criteriaFilter, bool bypassCache = false)
        {
            //CheckViewHousingAssignmentPermissions2();
            string person = string.Empty, term = string.Empty, status = string.Empty, startDate = string.Empty, endDate = string.Empty;
            if (criteriaFilter != null)
            {
                //process person guid filter
                var personGuid = criteriaFilter.Person != null ? criteriaFilter.Person.Id : string.Empty;
                if (!string.IsNullOrEmpty(personGuid))
                {
                    try
                    {
                        person = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                    }
                }

                //process academicPeriod guid filter
                var academicPeriodGuid = criteriaFilter.AcademicPeriod != null ? criteriaFilter.AcademicPeriod.Id : string.Empty;
                if (!string.IsNullOrEmpty(academicPeriodGuid))
                {
                    try
                    {
                        var academicPeriods = await GetAcademicPeriods();
                        if (academicPeriods == null)
                        {
                            return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                        }
                        var academicPeriodEntity = academicPeriods.FirstOrDefault(mp => mp.Guid == academicPeriodGuid);
                        if (academicPeriodEntity == null)
                        {
                            return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                        }
                        term = academicPeriodEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                    }
                }
                //process status filter
                if (criteriaFilter.Status != null)
                    status = ConvertStatusDtoToEntity(criteriaFilter.Status);
                if (status == null)
                {
                    return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                }

                //process startOn and endOn filter
                try
                {
                    startDate = criteriaFilter.StartOn.HasValue ? await ConvertDateArgument(criteriaFilter.StartOn.ToString()) : string.Empty;
                    if (startDate == null)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                    }
                    endDate = criteriaFilter.EndOn.HasValue ? await ConvertDateArgument(criteriaFilter.EndOn.ToString()) : string.Empty;
                    if (endDate == null)
                    {
                        return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(new List<Dtos.HousingAssignment2>(), 0);
                }
            }

            try
            {
                var housingAssignmentsCollection = new List<Ellucian.Colleague.Dtos.HousingAssignment2>();

                var housingAssignmentsEntities = await _housingAssignmentRepository.GetHousingAssignmentsAsync(offset, limit, person, term, status, startDate, endDate, bypassCache);

                var totalCount = housingAssignmentsEntities.Item2;

                if (housingAssignmentsEntities != null && housingAssignmentsEntities.Item1.Any())
                {
                    BuildLocalPersonGuids(housingAssignmentsEntities.Item1);

                    foreach (var housingAssignments in housingAssignmentsEntities.Item1)
                    {
                        housingAssignmentsCollection.Add(await ConvertHousingAssignmentsEntityToDto2Async(housingAssignments, bypassCache));
                    }
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                }

                return new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(housingAssignmentsCollection, totalCount);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Gets By Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<HousingAssignment2> GetHousingAssignmentByGuid2Async(string id, bool bypassCache = false)
        {
            //CheckViewHousingAssignmentPermissions2();

            try
            {
                var housingAssignmentEntity = await _housingAssignmentRepository.GetHousingAssignmentByGuidAsync(id);

                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { housingAssignmentEntity });

                var housingAssignmentDtos = await ConvertHousingAssignmentsEntityToDto2Async(housingAssignmentEntity, bypassCache);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return housingAssignmentDtos;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: id);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Updates housing assignment.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        public async Task<HousingAssignment2> UpdateHousingAssignment2Async(string guid, HousingAssignment2 housingAssignmentDto)
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
                    //CheckCreateUpdateHousingAssignmentPermissions2();

                    _housingAssignmentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    Ellucian.Colleague.Domain.Student.Entities.HousingAssignment housingAssignmentEntity = await ConvertDtoToEntity2Async(guid, housingAssignmentDto);

                    Domain.Student.Entities.HousingAssignment updatedHousingAssignmentEntity = await _housingAssignmentRepository.UpdateHousingAssignmentAsync(housingAssignmentEntity);

                    BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { housingAssignmentEntity });

                    ClearReferenceData();

                    var housingAssignmentDto2 = await this.ConvertHousingAssignmentsEntityToDto2Async(updatedHousingAssignmentEntity, true);
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                    return housingAssignmentDto2;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return await CreateHousingAssignment2Async(housingAssignmentDto);
        }

        /// <summary>
        /// Creates housing assignment.
        /// </summary>
        /// <param name="housingAssignmentDto"></param>
        /// <returns></returns>
        public async Task<HousingAssignment2> CreateHousingAssignment2Async(HousingAssignment2 housingAssignmentDto)
        {
            if (housingAssignmentDto == null)
            {
                throw new ArgumentNullException("housingAssignmentDto", "Must provide a guid for housing assignment create.");
            }

            try
            {
               //CheckCreateUpdateHousingAssignmentPermissions2();

                _housingAssignmentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                var housingAssignmentEntity = await ConvertDtoToEntity2Async(null, housingAssignmentDto);
                var createdHousingAssignmentEntity = await _housingAssignmentRepository.UpdateHousingAssignmentAsync(housingAssignmentEntity);
                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingAssignment>() { createdHousingAssignmentEntity });

                var housingAssignmentDto2 = await this.ConvertHousingAssignmentsEntityToDto2Async(createdHousingAssignmentEntity, true);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return housingAssignmentDto2;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HousingAssignments domain entity to its corresponding HousingAssignments DTO
        /// </summary>
        /// <param name="source">HousingAssignments domain entity</param>
        /// <returns>HousingAssignments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.HousingAssignment2> ConvertHousingAssignmentsEntityToDto2Async(Ellucian.Colleague.Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            var housingAssignment = new Ellucian.Colleague.Dtos.HousingAssignment2();

            housingAssignment.Id = source.Guid;
            housingAssignment.HousingRequest = string.IsNullOrEmpty(source.HousingRequest) ? null : new GuidObject2(source.HousingRequest);
            housingAssignment.Person = await ConvertPersonEntityToDto2(source);
            housingAssignment.Room = await ConvertRoomEntityToDto2(source, bypassCache);
            housingAssignment.AcademicPeriod = await ConvertTermEntityToDto2(source);
            if (!source.StartOn.HasValue)
            {
                IntegrationApiExceptionAddError("Start on date is required.", guid: source.Guid, id: source.RecordKey);
            }
            housingAssignment.StartOn = source.StartOn.Value;
            housingAssignment.EndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTimeOffset?);
            var statusStatusDate = ConvertStatusEntityToDto2(source);
            if (statusStatusDate != null)
            {
                housingAssignment.Status = statusStatusDate.Item1;
                housingAssignment.StatusDate = statusStatusDate.Item2.HasValue ? statusStatusDate.Item2.Value.Date : default(DateTime?);
            }
            housingAssignment.ContractNumber = string.IsNullOrEmpty(source.ContractNumber) ? null : source.ContractNumber;
            housingAssignment.Comment = string.IsNullOrEmpty(source.Comments) ? null : source.Comments;
            housingAssignment.RoomRates = await ConvertRoomRateEntityToDto2(source, bypassCache);
            housingAssignment.RatePeriod = ConvertEntityRatePeriodToDto(source.RatePeriod);
            housingAssignment.RateOverride = source.RateOverride.HasValue ? await ConvertOverrideEntityToDto2(source, bypassCache) : null;
            housingAssignment.AdditionalCharges = await ConvertAdditionalChargesEntityToDto2(source, bypassCache);
            housingAssignment.ResidentType = await ConvertHousingResTypeEntityToDto2(source, bypassCache);

            return housingAssignment;
        }

        /// <summary>
        /// Convert housing request dto to entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.HousingAssignment> ConvertDtoToEntity2Async(string guid, Dtos.HousingAssignment2 source)
        {
            try
            {
                //person is required
                if (source.Person == null || string.IsNullOrEmpty(source.Person.Id))
                {
                    IntegrationApiExceptionAddError("Person is required.");
                }
                var personKey = string.Empty;
                try
                {
                    personKey = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
                    if (string.IsNullOrEmpty(personKey))
                    {
                        IntegrationApiExceptionAddError(string.Format("No person found. guid: {0}", source.Person.Id));
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("No person found. guid: {0}", source.Person.Id));
                }                

                //room
                var roomEntity = (await GetRooms(true)).FirstOrDefault(i => i.Guid.Equals(source.Room.Id, StringComparison.OrdinalIgnoreCase));
                if (roomEntity == null)
                {
                    IntegrationApiExceptionAddError(string.Format("No room found. guid: {0}", source.Room.Id));
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                //Create the domain entity
                Domain.Student.Entities.HousingAssignment destinationEntity = string.IsNullOrEmpty(guid) ?
                    new Domain.Student.Entities.HousingAssignment(source.Id, personKey, roomEntity.Id, source.StartOn.Value, source.EndOn.Value) :
                    new Domain.Student.Entities.HousingAssignment(source.Id, await GetHousingAssignmentKeyAsync(source.Id), personKey, roomEntity.Id, source.StartOn.Value,
                    source.EndOn.Value);

                //academicPeriod
                if (source.RatePeriod == RatePeriod.Term && string.IsNullOrEmpty(source.AcademicPeriod.Id))
                {
                    IntegrationApiExceptionAddError("A term must be specified if the rate period is Term.");
                }

                if (source.AcademicPeriod != null && !string.IsNullOrEmpty(source.AcademicPeriod.Id))
                {
                    var acadPeriod = (await GetAcademicPeriods()).FirstOrDefault(i => i.Guid.Equals(source.AcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                    if (acadPeriod == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("No academic period found. guid: {0}", source.AcademicPeriod.Id));
                    }
                    else
                    {
                        destinationEntity.Term = acadPeriod.Code;
                    }
                }
                //status
                var status = string.Empty;
                if (source.Status != null)
                {
                    if (source.Status != HousingAssignmentsStatus.NotSet)
                    {
                        destinationEntity.Status = ConvertStatusDtoToEntity(source.Status);
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError("Status is required.");
                }

                if (!source.StatusDate.HasValue)
                {
                    IntegrationApiExceptionAddError("Status date is required.");
                }
                else
                {
                    destinationEntity.StatusDate = source.StatusDate.Value;
                }

                //request
                if (source.HousingRequest != null && !string.IsNullOrEmpty(source.HousingRequest.Id))
                {
                    destinationEntity.HousingRequest = source.HousingRequest.Id;
                }

                //roomRate
                var roomRates = source.RoomRates ?? null;
                if (roomRates != null && roomRates.Any() && roomRates.Count() > 1)
                {
                    IntegrationApiExceptionAddError("Only a single room rate is permitted.");
                }

                if (roomRates != null && roomRates.Any())
                {
                    var singleRoomRate = roomRates.FirstOrDefault();

                    if (string.IsNullOrEmpty(singleRoomRate.Id))
                    {
                        IntegrationApiExceptionAddError("Id is required for the room rate.");
                    }
                    else
                    {
                        var rmRates = await RoomRatesAsync(true);
                        if (rmRates != null && rmRates.Any())
                        {
                            var rmRate = rmRates.FirstOrDefault(i => i.Guid.Equals(singleRoomRate.Id, StringComparison.OrdinalIgnoreCase));
                            if (rmRate == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("No room rate found for guid: '{0}'", singleRoomRate.Id));
                            }
                            if (rmRate != null)
                            {
                                if (rmRate.EndDate.HasValue && rmRate.EndDate.Value > source.EndOn.Value)
                                {
                                    IntegrationApiExceptionAddError("The specified Housing Rate Table is not active on housing assignment start date.");
                                }
                                destinationEntity.RoomRateTable = rmRate.Code;
                            }
                        }
                    }

                    //ratePeriod
                    if (string.IsNullOrEmpty(singleRoomRate.Id) && source.RatePeriod.HasValue)
                    {
                        IntegrationApiExceptionAddError("A valid roomrate id is required when rate period is specified.");
                    }
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
                        IntegrationApiExceptionAddError("Rate value is required for the rate override.");
                    }

                    if (source.RateOverride.HousingAssignmentRate != null && source.RateOverride.HousingAssignmentRate.RateValue != null &&
                        source.RateOverride.HousingAssignmentRate.RateValue < 0)
                    {
                        IntegrationApiExceptionAddError("The override rate value must be set greater than zero.");
                    }

                    //rateOverride.HousingAssignmentRate.RateValue
                    destinationEntity.RateOverride = source.RateOverride.HousingAssignmentRate.RateValue;

                    if (source.RateOverride.HousingAssignmentRate.RateValue != null && source.RateOverride.HousingAssignmentRate.RateCurrency != Dtos.EnumProperties.CurrencyIsoCode.USD &&
                        source.RateOverride.HousingAssignmentRate.RateCurrency != Dtos.EnumProperties.CurrencyIsoCode.CAD)
                    {
                        IntegrationApiExceptionAddError("The override rate currency must be set to either 'USD' or 'CAD'.");
                    }

                    //RateOverrideReason
                    if (source.RateOverride.RateOverrideReason != null && string.IsNullOrEmpty(source.RateOverride.RateOverrideReason.Id))
                    {
                        IntegrationApiExceptionAddError("An Override Rate Reason must be specified with an Override Rate.");
                    }
                    var rateOverrideReason = (await GetBillingOverrideReasons(true)).FirstOrDefault(i => i.Guid.Equals(source.RateOverride.RateOverrideReason.Id, StringComparison.OrdinalIgnoreCase));
                    if (rateOverrideReason == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("No rate override reason found for guid: {0}", source.RateOverride.RateOverrideReason.Id));
                    }
                    else
                    {
                        destinationEntity.RateOverrideReason = rateOverrideReason.Code;
                    }
                }

                //additionalCharges
                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null || (i.AccountingCode != null && string.IsNullOrEmpty(i.AccountingCode.Id)))))
                {
                    IntegrationApiExceptionAddError(string.Format("Accounting code is required for additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null || (i.AccountingCode != null && i.HousingAssignmentRate == null))))
                {
                    IntegrationApiExceptionAddError(string.Format("Charge is required for additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null ||
                    (i.AccountingCode != null && i.HousingAssignmentRate != null && i.HousingAssignmentRate.RateCurrency == CurrencyIsoCode.NotSet))))
                {
                    IntegrationApiExceptionAddError(string.Format("Currency is required for the additional charges."));
                }

                if (source.AdditionalCharges != null && (source.AdditionalCharges.Any(i => i.AccountingCode == null ||
                    (i.AccountingCode != null && i.HousingAssignmentRate != null && (i.HousingAssignmentRate.RateValue == null || !i.HousingAssignmentRate.RateValue.HasValue)))))
                {
                    IntegrationApiExceptionAddError("Value is required for the additional charges.");
                }

                if (source.AdditionalCharges != null && source.AdditionalCharges.Any())
                {
                    List<ArAdditionalAmount> arAddlAmountList = new List<ArAdditionalAmount>();
                    foreach (var additionalCharge in source.AdditionalCharges)
                    {
                        if (additionalCharge.AccountingCode != null && additionalCharge.AccountingCode.Id != null)
                        {
                            var acctCode = (await this.GetAccountingCodes(true)).FirstOrDefault(i => i.Guid.Equals(additionalCharge.AccountingCode.Id));
                            if (acctCode == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("No accounting code found for guid: {0}", additionalCharge.AccountingCode.Id));
                            }
                            else
                            {
                                ArAdditionalAmount addlAmt = new ArAdditionalAmount()
                                {
                                    AraaArCode = acctCode.Code
                                };
                                if (additionalCharge.HousingAssignmentRate != null && additionalCharge.HousingAssignmentRate.RateValue != null)
                                {
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
                            }
                        }
                    }
                    if (arAddlAmountList.Any())
                    {
                        destinationEntity.ArAdditionalAmounts = arAddlAmountList;
                    }                    
                }

                //residentType
                if (source.ResidentType != null && string.IsNullOrEmpty(source.ResidentType.Id))
                {
                    IntegrationApiExceptionAddError("Id is required for the resident type.");
                }

                if (source.ResidentType != null)
                {
                    var resType = (await GetHousingResidentTypes(true)).FirstOrDefault(i => i.Guid.Equals(source.ResidentType.Id, StringComparison.OrdinalIgnoreCase));
                    if (resType == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("No resident type found for guid: {0}", source.ResidentType.Id));
                    }
                    else
                    {
                        destinationEntity.ResidentStaffIndicator = resType.Code;
                    }
                }


                //contractNumber
                if (!string.IsNullOrEmpty(source.ContractNumber))
                {
                    if (source.ContractNumber.Length > 16)
                    {
                        IntegrationApiExceptionAddError("Contract number cannot be more than 16 characters.");
                    }
                    else
                    {
                        destinationEntity.ContractNumber = source.ContractNumber;
                    }
                }

                //comments
                if (!string.IsNullOrEmpty(source.Comment))
                {
                    destinationEntity.Comments = source.Comment;
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return destinationEntity;
                                
            }      
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Converts room rate entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<List<GuidObject2>> ConvertRoomRateEntityToDto2(Domain.Student.Entities.HousingAssignment source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source.RoomRateTable))
            {
                return null;
            }
            try
            {
                var rmRates = await RoomRatesAsync(bypassCache);
                if (rmRates != null && rmRates.Any())
                {
                    try
                    {
                        var roomRate = rmRates.FirstOrDefault(i => i.Code.Equals(source.RoomRateTable, StringComparison.OrdinalIgnoreCase));
                        return new List<GuidObject2>() { new GuidObject2(roomRate.Guid) };
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError("No room rate for code " + source.RoomRateTable, guid: source.Guid, id: source.RecordKey);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                IntegrationApiExceptionAddError("No room rate for code " + source.RoomRateTable, guid: source.Guid, id: source.RecordKey);
                return null;
            }            
        }
        #endregion
    }
}