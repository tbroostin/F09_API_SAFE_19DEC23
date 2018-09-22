//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayrollDeductionArrangementService : BaseCoordinationService, IPayrollDeductionArrangementService
    {
        private readonly IPayrollDeductionArrangementRepository _payrollDeductionArrangementRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="payrollDeductionArrangementRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PayrollDeductionArrangementService(
            IPayrollDeductionArrangementRepository payrollDeductionArrangementRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _payrollDeductionArrangementRepository = payrollDeductionArrangementRepository;
            _hrReferenceDataRepository = hrReferenceDataRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
        }

        #region Version 7
        /// <summary>
        /// Gets all payroll deduction arrangements.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>> GetPayrollDeductionArrangementsAsync(int offset, int limit, bool bypassCache = false,
            string person = "", string contribution = "", string deductionType = "", string status = "")
        {
            CheckUserViewPermissions();

            var payrollDeductionArrangements = new List<Dtos.PayrollDeductionArrangements>();
            string personId = "";
            if (!string.IsNullOrEmpty(person))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personId))
                    {
                        // throw new ArgumentException(string.Format("Person id '{0}' is not valid. ", person), "person");
                        return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                    }
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                }
            }
            string deductionTypeCode = "";
            if (!string.IsNullOrEmpty(deductionType))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Guid == deductionType).FirstOrDefault();
                if (deductionEntity == null)
                {
                    // throw new ArgumentException(string.Format("Deduction type of '{0}' is not valid. ", deductionType), "deductionType");
                    return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                }
                deductionTypeCode = deductionEntity.Code;
            }
            // throw empty set if these status are searched on.
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "terminated" || status.ToLower() == "withdrawn" || status.ToLower() == "rejected" || status.ToLower() == "suspended")
                {
                    return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                }
                if (status.ToLower() != "active" && status.ToLower() != "cancelled")
                {
                    throw new ArgumentException(string.Format("Invalid enumeration value of '{0}'. ", status), "status");
                }
            }
            var pageOfItems = await _payrollDeductionArrangementRepository.GetAsync(offset, limit, bypassCache, personId, contribution, deductionTypeCode, status);

            var payrollDeductionArrangementEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (payrollDeductionArrangementEntities != null && payrollDeductionArrangementEntities.Any())
            {
                foreach (var payrollDeductionArrangement in payrollDeductionArrangementEntities)
                {
                    payrollDeductionArrangements.Add(await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangement));
                }

                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(payrollDeductionArrangements, totalRecords);

            }
            else
            {
                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
            }
        }

        /// <summary>
        /// Gets a payroll deduction arrangement change reason by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangementsByGuidAsync(string id, bool bypassCache = false)
        {
            var payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.GetByIdAsync(id);
            var personId = payrollDeductionArrangementEntity.PersonId;

            CheckUserViewPermissions(personId);

            return await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity);
        }

        /// <summary>
        /// Update a payroll deduction arrangement by guid
        /// </summary>
        /// <param name="id">guid for the payroll deduction arrangement</param>
        /// <returns>PayrollDeductionArrangement DTO Object</returns>
        public async Task<Dtos.PayrollDeductionArrangements> UpdatePayrollDeductionArrangementsAsync(string id, Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required to update a payroll deduction arrangement. ");
            }
            if (payrollDeductionArrangementDto == null)
            {
                throw new ArgumentNullException("payrollDeductionarrangement", "The DTO is required to update a payroll deduction arrangement. ");
            }
            var payrollDeductionArrangementEntity = await ConvertPayrollDeductionArrangementsDtoToEntityAsync(payrollDeductionArrangementDto);

            var personId = payrollDeductionArrangementEntity.PersonId;
            CheckUserUpdatePermissions(personId);

            VerifyPayrollDeductionArrangement(payrollDeductionArrangementDto, id);

            var perbenKey = await _payrollDeductionArrangementRepository.GetIdFromGuidAsync(id);
            if (string.IsNullOrEmpty(perbenKey))
            {
                // Validate status property
                Dtos.EnumProperties.PayrollDeductionArrangementStatuses? statusValue = payrollDeductionArrangementDto.Status;
                if (statusValue != Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active)
                {
                    throw new ArgumentOutOfRangeException("status", "A request for a new payroll deduction must have a status of 'active' to be accepted. ");
                }
            }

            payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.UpdateAsync(id, payrollDeductionArrangementEntity);

            if (payrollDeductionArrangementEntity == null)
            {
                throw new KeyNotFoundException(string.Format("The id of '{0}' could not be updated. ", id));
            }
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity);
            
            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Create a new payroll deduction arrangement
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.PayrollDeductionArrangements> CreatePayrollDeductionArrangementsAsync(Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto)
        {
            CheckUserCreatePermissions();

            if (payrollDeductionArrangementDto == null)
            {
                throw new ArgumentNullException("payrollDeductionArrangement", "The DTO is required to create a new payroll deduction arrangement. ");
            }

            VerifyPayrollDeductionArrangement(payrollDeductionArrangementDto);

            var payrollDeductionArrangementEntity = await ConvertPayrollDeductionArrangementsDtoToEntityAsync(payrollDeductionArrangementDto);

            payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.CreateAsync(payrollDeductionArrangementEntity);

            if (payrollDeductionArrangementEntity == null)
            {
                throw new KeyNotFoundException(string.Format("The id of '{0}' could not be created. ", payrollDeductionArrangementDto.Id));
            }
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity);
            
            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>PayrollDeductionArrangement DTO object</returns>
        private async Task<Dtos.PayrollDeductionArrangements> ConvertPayrollDeductionArrangementsEntityToDtoAsync(Domain.HumanResources.Entities.PayrollDeductionArrangements source)
        {
            var currencyCode = Dtos.EnumProperties.CurrencyCodes.USD;
            var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
            if (hostCountry == "CANADA")
            {
                currencyCode = Dtos.EnumProperties.CurrencyCodes.CAD;
            }
            var payrollDeductionArrangement = new Dtos.PayrollDeductionArrangements();

            payrollDeductionArrangement.Id = source.Guid;
            if (string.IsNullOrEmpty(source.PersonId))
            {
                throw new ArgumentNullException("personId", string.Format("Person ID is missing from PayrollDeductionArrangement id '{0}'. ", source.Guid));
            }
            string deductionGuid = "";
            if (!string.IsNullOrEmpty(source.DeductionTypeCode))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Code == source.DeductionTypeCode).FirstOrDefault();
                if (deductionEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a GUID for the deduction type of '{0}' ", source.DeductionTypeCode), "deductionType");
                }
                deductionGuid = deductionEntity.Guid;
            }

            var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
            if (string.IsNullOrEmpty(personGuid))
            {
                throw new ArgumentException(string.Format("Unable to find a GUID for Person '{0}' ", source.PersonId), "person.id");
            }

            payrollDeductionArrangement.Person = new GuidObject2(personGuid);

            // Get the commitment type or payment target (contribution, deduction type)
            var paymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty();
            // We either have a contribution ID and type or we have a deduction Guid.
            if (string.IsNullOrEmpty(deductionGuid) && (!string.IsNullOrEmpty(source.CommitmentContributionId) || !string.IsNullOrEmpty(source.CommitmentType)))
            {
                paymentTarget.Commitment = new Dtos.DtoProperties.PaymentTargetCommitment();
                if (!string.IsNullOrEmpty(source.CommitmentContributionId))
                {
                    paymentTarget.Commitment.Contribution = source.CommitmentContributionId;
                }
                if (!string.IsNullOrEmpty(source.CommitmentType))
                {
                    paymentTarget.Commitment.Type = (Dtos.EnumProperties.CommitmentTypes)Enum.Parse(typeof(Dtos.EnumProperties.CommitmentTypes), source.CommitmentType);
                }
            }
            if (!string.IsNullOrEmpty(deductionGuid))
            {
                paymentTarget.Deduction = new Dtos.DtoProperties.PaymentTargetDeduction();
                paymentTarget.Deduction.DeductionType = new GuidObject2(deductionGuid);
            }
            payrollDeductionArrangement.PaymentTarget = paymentTarget;
            
            // Get the status of the payroll deduction.
            payrollDeductionArrangement.Status = (Dtos.EnumProperties.PayrollDeductionArrangementStatuses)Enum.Parse(typeof(Dtos.EnumProperties.PayrollDeductionArrangementStatuses), source.Status);
            
            // Get Amount per payment
            if (source.AmountPerPayment.HasValue)
            {
                payrollDeductionArrangement.amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Currency = currencyCode,
                    Value = source.AmountPerPayment
                };
            }
            
            // Get total amount to deduct
            if (source.TotalAmount.HasValue)
            {
                payrollDeductionArrangement.TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Currency = currencyCode,
                    Value = source.TotalAmount
                };
            }

            // Get start and end dates
            payrollDeductionArrangement.StartDate = source.StartDate;
            payrollDeductionArrangement.EndDate = source.EndDate;

            // Get the pay period occurances as interval or monthly payments.
            var monthlyPayPeriods = new List<int>();
            foreach (var period in source.MonthlyPayPeriods)
            {
                if (period != null && period != 0)
                {
                    monthlyPayPeriods.Add(period.Value);
                }
            }
            var payPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance();
            if (source.Interval.HasValue)
            {
                payPeriodOccurence.Interval = source.Interval;
            }
            if (monthlyPayPeriods.Any())
            {
                payPeriodOccurence.MonthlyPayPeriods = monthlyPayPeriods;
            }
            payrollDeductionArrangement.PayPeriodOccurence = payPeriodOccurence;

            // Get Change Reason
            if (!string.IsNullOrEmpty(source.ChangeReason))
            {
                var changeReasonEntity = (await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(false)).Where(cr => cr.Code == source.ChangeReason).FirstOrDefault();
                if (changeReasonEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a GUID for the change reason of '{0}' ", source.ChangeReason), "changeReason.Id");
                }
                var changeReasonGuid = changeReasonEntity.Guid;
                payrollDeductionArrangement.ChangeReason = new GuidObject2(changeReasonGuid);
            }

            return payrollDeductionArrangement;
        }

        /// <summary>
        /// Converts dto into a domain entity.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>PayrollDeductionArrangement Domain Entity Object</returns>
        private async Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> ConvertPayrollDeductionArrangementsDtoToEntityAsync(PayrollDeductionArrangements source)
        {
            var guid = source.Id;
            if (source.Person == null || string.IsNullOrEmpty(source.Person.Id))
            {
                throw new ArgumentNullException("person.id", string.Format("Person ID is missing from PayrollDeductionArrangement id '{0}'. ", guid));
            }
            string id = string.Empty;
            if (!string.Equals(new Guid(guid), Guid.Empty))
            {
                id = await _payrollDeductionArrangementRepository.GetIdFromGuidAsync(guid);
            }
            var personId = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
            if (personId == null || personId == string.Empty)
            {
                throw new ArgumentException(string.Format("Person id '{0}' is invalid. ", source.Person.Id), "person.id");
            }
            var payrollDeductionArrangement = new Domain.HumanResources.Entities.PayrollDeductionArrangements(guid,  personId);
            payrollDeductionArrangement.Id = id;

            if (source.PaymentTarget == null || 
                    (
                        ( 
                            source.PaymentTarget.Commitment == null ||
                            source.PaymentTarget.Commitment.Type == Dtos.EnumProperties.CommitmentTypes.NotSet
                        ) 
                        && 
                        ( 
                            source.PaymentTarget.Deduction == null ||
                            source.PaymentTarget.Deduction.DeductionType == null || 
                            string.IsNullOrEmpty(source.PaymentTarget.Deduction.DeductionType.Id)
                        )
                    )
                )
            {
                throw new ArgumentNullException("paymentTarget", string.Format("Payment Target must have either a commitment type or deduction type for PayrollDeductionarrangement id '{0}'", guid));
            }

            if (source.PaymentTarget.Deduction != null && source.PaymentTarget.Deduction.DeductionType != null && !string.IsNullOrEmpty(source.PaymentTarget.Deduction.DeductionType.Id))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Guid == source.PaymentTarget.Deduction.DeductionType.Id).FirstOrDefault();
                if (deductionEntity == null)
                {
                    throw new ArgumentException(string.Format("The id '{0}' is not a valid deduction type. ", source.PaymentTarget.Deduction.DeductionType.Id), "paymentTarget.deduction.deductionType.id");
                }
                payrollDeductionArrangement.DeductionTypeCode = deductionEntity.Code;
            }

            if (source.PaymentTarget != null && source.PaymentTarget.Commitment != null)
            {
                payrollDeductionArrangement.CommitmentType = source.PaymentTarget.Commitment.Type != Dtos.EnumProperties.CommitmentTypes.NotSet ? source.PaymentTarget.Commitment.Type.ToString() : string.Empty;
                payrollDeductionArrangement.CommitmentContributionId = !string.IsNullOrEmpty(source.PaymentTarget.Commitment.Contribution) ? source.PaymentTarget.Commitment.Contribution : string.Empty;
            }
            if (source.Status == Dtos.EnumProperties.PayrollDeductionArrangementStatuses.NotSet)
            {
                source.Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active;
            }
            payrollDeductionArrangement.Status = source.Status.ToString();
            if (source.amountPerPayment != null && source.amountPerPayment.Value.HasValue)
            {
                payrollDeductionArrangement.AmountPerPayment = source.amountPerPayment.Value;
            }
            if (source.TotalAmount != null && source.TotalAmount.Value.HasValue)
            {
                payrollDeductionArrangement.TotalAmount = source.TotalAmount.Value;
            }
            payrollDeductionArrangement.StartDate = source.StartDate.HasValue ? source.StartDate : null;
            payrollDeductionArrangement.EndDate = source.EndDate.HasValue ? source.EndDate : null;
            if (source.PayPeriodOccurence != null && source.PayPeriodOccurence.Interval != null && source.PayPeriodOccurence.Interval.HasValue)
            {
                payrollDeductionArrangement.Interval = source.PayPeriodOccurence.Interval;
            }
            if (source.PayPeriodOccurence != null && source.PayPeriodOccurence.MonthlyPayPeriods != null && source.PayPeriodOccurence.MonthlyPayPeriods.Any())
            {
                var monthlyPayPeriods = new List<int?>();
                foreach (var period in source.PayPeriodOccurence.MonthlyPayPeriods)
                {
                    monthlyPayPeriods.Add(period);
                }
                payrollDeductionArrangement.MonthlyPayPeriods = monthlyPayPeriods;
            }
            if (source.ChangeReason != null && !string.IsNullOrEmpty(source.ChangeReason.Id))
            {
                var changeReasonEntity = (await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(false)).Where(cr => cr.Guid == source.ChangeReason.Id).FirstOrDefault();
                if (changeReasonEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a code for the change reason of '{0}' ", source.ChangeReason.Id), "changeReason.Id");
                }
                payrollDeductionArrangement.ChangeReason = changeReasonEntity.Code;
            }

            return payrollDeductionArrangement;
        }
        #endregion

        #region Version 11
        /// <summary>
        /// Gets all payroll deduction arrangements.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="person">Person GUID filter.</param>
        /// <param name="contribution">Contribution ID filter.</param>
        /// <param name="deductionType">Deduction Type filter.</param>
        /// <param name="statusType">Status Type filter.</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>> GetPayrollDeductionArrangements2Async(int offset, int limit, bool bypassCache = false,
            string person = "", string contribution = "", string deductionType = "", string status = "")
        {
            CheckUserViewPermissions();

            var payrollDeductionArrangements = new List<Dtos.PayrollDeductionArrangements>();
            string personId = "";
            if (!string.IsNullOrEmpty(person))
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(person);
                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentException(string.Format("Person id '{0}' is not valid. ", person), "person");
                }
            }
            string deductionTypeCode = "";
            if (!string.IsNullOrEmpty(deductionType))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Guid == deductionType).FirstOrDefault();
                if (deductionEntity == null)
                {
                    throw new ArgumentException(string.Format("Deduction type of '{0}' is not valid. ", deductionType), "deductionType");
                }
                deductionTypeCode = deductionEntity.Code;
            }
            // throw empty set if these status are searched on.
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "terminated" || status == "withdrawn" || status == "rejected" || status == "suspended")
                {
                    return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                }
            }
            var pageOfItems = await _payrollDeductionArrangementRepository.GetAsync(offset, limit, bypassCache, personId, contribution, deductionTypeCode, status);

            var payrollDeductionArrangementEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (payrollDeductionArrangementEntities != null && payrollDeductionArrangementEntities.Any())
            {
                foreach (var payrollDeductionArrangement in payrollDeductionArrangementEntities)
                {
                    payrollDeductionArrangements.Add(await ConvertPayrollDeductionArrangements2EntityToDtoAsync(payrollDeductionArrangement));
                }

                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(payrollDeductionArrangements, totalRecords);

            }
            else
            {
                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
            }
        }

        /// <summary>
        /// Gets a payroll deduction arrangement change reason by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangements2ByIdAsync(string id, bool bypassCache = false)
        {
            var payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.GetByIdAsync(id);
            var personId = payrollDeductionArrangementEntity.PersonId;

            CheckUserViewPermissions(personId);

            return await ConvertPayrollDeductionArrangements2EntityToDtoAsync(payrollDeductionArrangementEntity);
        }

        /// <summary>
        /// Update a payroll deduction arrangement by guid
        /// </summary>
        /// <param name="id">guid for the payroll deduction arrangement</param>
        /// <returns>PayrollDeductionArrangement DTO Object</returns>
        public async Task<Dtos.PayrollDeductionArrangements> UpdatePayrollDeductionArrangements2Async(string id, Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required to update a payroll deduction arrangement. ");
            }
            if (payrollDeductionArrangementDto == null)
            {
                throw new ArgumentNullException("payrollDeductionarrangement", "The DTO is required to update a payroll deduction arrangement. ");
            }
            var payrollDeductionArrangementEntity = await ConvertPayrollDeductionArrangements2DtoToEntityAsync(payrollDeductionArrangementDto);

            var personId = payrollDeductionArrangementEntity.PersonId;
            CheckUserUpdatePermissions(personId);

            VerifyPayrollDeductionArrangement(payrollDeductionArrangementDto, id);

            var perbenKey = await _payrollDeductionArrangementRepository.GetIdFromGuidAsync(id);
            if (string.IsNullOrEmpty(perbenKey))
            {
                // Validate status property
                Dtos.EnumProperties.PayrollDeductionArrangementStatuses? statusValue = payrollDeductionArrangementDto.Status;
                if (statusValue != Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active)
                {
                    throw new ArgumentOutOfRangeException("status", "A request for a new payroll deduction must have a status of 'active' to be accepted. ");
                }
            }

            payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.UpdateAsync(id, payrollDeductionArrangementEntity);

            if (payrollDeductionArrangementEntity == null)
            {
                throw new KeyNotFoundException(string.Format("The id of '{0}' could not be updated. ", id));
            }
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity);

            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Create a new payroll deduction arrangement
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.PayrollDeductionArrangements> CreatePayrollDeductionArrangements2Async(Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto)
        {
            CheckUserCreatePermissions();

            if (payrollDeductionArrangementDto == null)
            {
                throw new ArgumentNullException("payrollDeductionArrangement", "The DTO is required to create a new payroll deduction arrangement. ");
            }

            VerifyPayrollDeductionArrangement(payrollDeductionArrangementDto);

            var payrollDeductionArrangementEntity = await ConvertPayrollDeductionArrangements2DtoToEntityAsync(payrollDeductionArrangementDto);

            payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.CreateAsync(payrollDeductionArrangementEntity);

            if (payrollDeductionArrangementEntity == null)
            {
                throw new KeyNotFoundException(string.Format("The id of '{0}' could not be created. ", payrollDeductionArrangementDto.Id));
            }
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity);

            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>PayrollDeductionArrangement DTO object</returns>
        private async Task<Dtos.PayrollDeductionArrangements> ConvertPayrollDeductionArrangements2EntityToDtoAsync(Domain.HumanResources.Entities.PayrollDeductionArrangements source)
        {
            var currencyCode = Dtos.EnumProperties.CurrencyCodes.USD;
            var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
            if (hostCountry == "CANADA")
            {
                currencyCode = Dtos.EnumProperties.CurrencyCodes.CAD;
            }
            var payrollDeductionArrangement = new Dtos.PayrollDeductionArrangements();

            payrollDeductionArrangement.Id = source.Guid;
            if (string.IsNullOrEmpty(source.PersonId))
            {
                throw new ArgumentNullException("personId", string.Format("Person ID is missing from PayrollDeductionArrangement id '{0}'. ", source.Guid));
            }
            string deductionGuid = "";
            if (!string.IsNullOrEmpty(source.DeductionTypeCode))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Code == source.DeductionTypeCode).FirstOrDefault();
                if (deductionEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a GUID for the deduction type of '{0}' ", source.DeductionTypeCode), "deductionType");
                }
                deductionGuid = deductionEntity.Guid;
            }

            var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
            if (string.IsNullOrEmpty(personGuid))
            {
                throw new ArgumentException(string.Format("Unable to find a GUID for Person '{0}' ", source.PersonId), "person.id");
            }

            payrollDeductionArrangement.Person = new GuidObject2(personGuid);

            // Get the commitment type or payment target (contribution, deduction type)
            var paymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty();
            // We eithe rhave a contribution ID and type or we have a deduction Guid.
            if (string.IsNullOrEmpty(deductionGuid) && (!string.IsNullOrEmpty(source.CommitmentContributionId) || !string.IsNullOrEmpty(source.CommitmentType)))
            {
                paymentTarget.Commitment = new Dtos.DtoProperties.PaymentTargetCommitment();
                if (!string.IsNullOrEmpty(source.CommitmentContributionId))
                {
                    paymentTarget.Commitment.Contribution = source.CommitmentContributionId;
                }
                if (!string.IsNullOrEmpty(source.CommitmentType))
                {
                    paymentTarget.Commitment.Type = (Dtos.EnumProperties.CommitmentTypes)Enum.Parse(typeof(Dtos.EnumProperties.CommitmentTypes), source.CommitmentType);
                }
            }
            if (!string.IsNullOrEmpty(deductionGuid))
            {
                paymentTarget.Deduction = new Dtos.DtoProperties.PaymentTargetDeduction();
                paymentTarget.Deduction.DeductionType = new GuidObject2(deductionGuid);
            }
            payrollDeductionArrangement.PaymentTarget = paymentTarget;

            // Get the status of the payroll deduction.
            payrollDeductionArrangement.Status = (Dtos.EnumProperties.PayrollDeductionArrangementStatuses)Enum.Parse(typeof(Dtos.EnumProperties.PayrollDeductionArrangementStatuses), source.Status);

            // Get Amount per payment
            if (source.AmountPerPayment.HasValue)
            {
                payrollDeductionArrangement.amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Currency = currencyCode,
                    Value = source.AmountPerPayment
                };
            }

            // Get total amount to deduct
            if (source.TotalAmount.HasValue)
            {
                payrollDeductionArrangement.TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Currency = currencyCode,
                    Value = source.TotalAmount
                };
            }

            // Get start and end dates
            payrollDeductionArrangement.StartDate = source.StartDate;
            payrollDeductionArrangement.EndDate = source.EndDate;

            // Get the pay period occurances as interval or monthly payments.
            var monthlyPayPeriods = new List<int>();
            foreach (var period in source.MonthlyPayPeriods)
            {
                if (period != null && period != 0)
                {
                    monthlyPayPeriods.Add(period.Value);
                }
            }
            var payPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance();
            if (source.Interval.HasValue)
            {
                payPeriodOccurence.Interval = source.Interval;
            }
            if (monthlyPayPeriods.Any())
            {
                payPeriodOccurence.MonthlyPayPeriods = monthlyPayPeriods;
            }
            payrollDeductionArrangement.PayPeriodOccurence = payPeriodOccurence;

            // Get Change Reason
            if (!string.IsNullOrEmpty(source.ChangeReason))
            {
                var changeReasonEntity = (await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(false)).Where(cr => cr.Code == source.ChangeReason).FirstOrDefault();
                if (changeReasonEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a GUID for the change reason of '{0}' ", source.ChangeReason), "changeReason.Id");
                }
                var changeReasonGuid = changeReasonEntity.Guid;
                payrollDeductionArrangement.ChangeReason = new GuidObject2(changeReasonGuid);
            }

            return payrollDeductionArrangement;
        }

        /// <summary>
        /// Converts dto into a domain entity.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>PayrollDeductionArrangement Domain Entity Object</returns>
        private async Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> ConvertPayrollDeductionArrangements2DtoToEntityAsync(PayrollDeductionArrangements source)
        {
            var guid = source.Id;
            if (source.Person == null || string.IsNullOrEmpty(source.Person.Id))
            {
                throw new ArgumentNullException("person.id", string.Format("Person ID is missing from PayrollDeductionArrangement id '{0}'. ", guid));
            }
            string id = string.Empty;
            if (!string.Equals(new Guid(guid), Guid.Empty))
            {
                id = await _payrollDeductionArrangementRepository.GetIdFromGuidAsync(guid);
            }
            var personId = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
            if (personId == null || personId == string.Empty)
            {
                throw new ArgumentException(string.Format("Person id '{0}' is invalid. ", source.Person.Id), "person.id");
            }
            var payrollDeductionArrangement = new Domain.HumanResources.Entities.PayrollDeductionArrangements(guid, personId);
            payrollDeductionArrangement.Id = id;

            if (source.PaymentTarget == null ||
                    (
                        (
                            source.PaymentTarget.Commitment == null ||
                            source.PaymentTarget.Commitment.Type == Dtos.EnumProperties.CommitmentTypes.NotSet
                        )
                        &&
                        (
                            source.PaymentTarget.Deduction == null ||
                            source.PaymentTarget.Deduction.DeductionType == null ||
                            string.IsNullOrEmpty(source.PaymentTarget.Deduction.DeductionType.Id)
                        )
                    )
                )
            {
                throw new ArgumentNullException("paymentTarget", string.Format("Payment Target must have either a commitment type or deduction type for PayrollDeductionarrangement id '{0}'", guid));
            }

            if (source.PaymentTarget.Deduction != null && source.PaymentTarget.Deduction.DeductionType != null && !string.IsNullOrEmpty(source.PaymentTarget.Deduction.DeductionType.Id))
            {
                var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Guid == source.PaymentTarget.Deduction.DeductionType.Id).FirstOrDefault();
                if (deductionEntity == null)
                {
                    throw new ArgumentException(string.Format("The id '{0}' is not a valid deduction type. ", source.PaymentTarget.Deduction.DeductionType.Id), "paymentTarget.deduction.deductionType.id");
                }
                payrollDeductionArrangement.DeductionTypeCode = deductionEntity.Code;
            }

            if (source.PaymentTarget != null && source.PaymentTarget.Commitment != null)
            {
                payrollDeductionArrangement.CommitmentType = source.PaymentTarget.Commitment.Type != Dtos.EnumProperties.CommitmentTypes.NotSet ? source.PaymentTarget.Commitment.Type.ToString() : string.Empty;
                payrollDeductionArrangement.CommitmentContributionId = !string.IsNullOrEmpty(source.PaymentTarget.Commitment.Contribution) ? source.PaymentTarget.Commitment.Contribution : string.Empty;
            }
            if (source.Status == Dtos.EnumProperties.PayrollDeductionArrangementStatuses.NotSet)
            {
                source.Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active;
            }
            payrollDeductionArrangement.Status = source.Status.ToString();
            if (source.amountPerPayment != null && source.amountPerPayment.Value.HasValue)
            {
                payrollDeductionArrangement.AmountPerPayment = source.amountPerPayment.Value;
            }
            if (source.TotalAmount != null && source.TotalAmount.Value.HasValue)
            {
                payrollDeductionArrangement.TotalAmount = source.TotalAmount.Value;
            }
            payrollDeductionArrangement.StartDate = source.StartDate.HasValue ? source.StartDate : null;
            payrollDeductionArrangement.EndDate = source.EndDate.HasValue ? source.EndDate : null;
            if (source.PayPeriodOccurence != null && source.PayPeriodOccurence.Interval != null && source.PayPeriodOccurence.Interval.HasValue)
            {
                payrollDeductionArrangement.Interval = source.PayPeriodOccurence.Interval;
            }
            if (source.PayPeriodOccurence != null && source.PayPeriodOccurence.MonthlyPayPeriods != null && source.PayPeriodOccurence.MonthlyPayPeriods.Any())
            {
                var monthlyPayPeriods = new List<int?>();
                foreach (var period in source.PayPeriodOccurence.MonthlyPayPeriods)
                {
                    monthlyPayPeriods.Add(period);
                }
                payrollDeductionArrangement.MonthlyPayPeriods = monthlyPayPeriods;
            }
            if (source.ChangeReason != null && !string.IsNullOrEmpty(source.ChangeReason.Id))
            {
                var changeReasonEntity = (await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(false)).Where(cr => cr.Guid == source.ChangeReason.Id).FirstOrDefault();
                if (changeReasonEntity == null)
                {
                    throw new ArgumentException(string.Format("Unable to find a code for the change reason of '{0}' ", source.ChangeReason.Id), "changeReason.Id");
                }
                payrollDeductionArrangement.ChangeReason = changeReasonEntity.Code;
            }

            return payrollDeductionArrangement;
        }
        #endregion

        #region Permissions

        /// <summary>
        /// Verifies if the user has the correct permission to view the person.
        /// </summary>
        private void CheckUserViewPermissions(string personId)
        {
            // access is ok if the current user is the person being viewed
            if (!CurrentUser.IsPerson(personId))
            {
                // not the current user, must have view any person permission
                CheckUserViewPermissions();
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permission to view any person.
        /// </summary>
        private void CheckUserViewPermissions()
        {
            // access is ok if the current user has the view any person permission
            if (!HasPermission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view payroll-deduction-arrangements.");
                throw new PermissionsException("User is not authorized to to view payroll-deduction-arrangements.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a person.
        /// </summary>
        private void CheckUserCreatePermissions()
        {
            // access is ok if the current user has the create person permission
            if (!HasPermission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create payroll-deduction-arrangements.");
                throw new PermissionsException("User is not authorized to create payroll-deduction-arrangements.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a person.
        /// </summary>
        private void CheckUserUpdatePermissions(string personId)
        {
            // access is ok if the current user is the person being updated
            if (!CurrentUser.IsPerson(personId))
            {
                // access is ok if the current user has the update person permission
                if (!HasPermission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update payroll-deduction-arrangements.");
                    throw new PermissionsException("User is not authorized to update payroll-deduction-arrangements.");
                }
            }
        }

        #endregion

        #region Validation
        /// <summary>
        /// Validate the payload coming into the API within the body of the request.
        /// </summary>
        /// <param name="payrollDeductionArrangement">From Body, the request payload</param>
        /// <param name="id">From a PUT, the ID from the URI</param>
        private void VerifyPayrollDeductionArrangement(Dtos.PayrollDeductionArrangements payrollDeductionArrangement, string id = "")
        {
            bool postRequest = false;
            if (string.IsNullOrEmpty(id))
            {
                postRequest = true;
            }

            // Validate ID properties
            if (string.IsNullOrEmpty(payrollDeductionArrangement.Id))
            {
                throw new ArgumentNullException("id", "The id must be specified for an update or create request. ");
            }
            if (!string.IsNullOrEmpty(id))
            {
                if (payrollDeductionArrangement.Id == null || !payrollDeductionArrangement.Id.Equals(id))
                {
                    throw new ArgumentOutOfRangeException("id", string.Format("Id on PUT request doesn't match Id in the request body of '{0}'. ", payrollDeductionArrangement.Id));
                }
            }
            var employeeId = payrollDeductionArrangement.Person != null ? payrollDeductionArrangement.Person.Id : string.Empty;
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("person.id", "The person id must be specified for an update or create request. ");
            }

            // Validate status property
            Dtos.EnumProperties.PayrollDeductionArrangementStatuses? statusValue = payrollDeductionArrangement.Status;
            if (postRequest && statusValue != Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active)
            {
                throw new ArgumentOutOfRangeException("status", "A request for a new payroll deduction must have a status of 'active' to be accepted. ");
            }
            if (statusValue == Dtos.EnumProperties.PayrollDeductionArrangementStatuses.NotSet || statusValue == null)
            {
                throw new ArgumentNullException("status", "The status is either invalid or missing.  A status is required for a payroll deduction. ");
            }

            // Validate Payment Target property
            if (payrollDeductionArrangement.PaymentTarget == null)
            {
                throw new ArgumentNullException("paymentTarget", "The paymentTarget property is required for a payroll deduction. ");
            }
            else
            {
                if (payrollDeductionArrangement.PaymentTarget.Commitment == null && payrollDeductionArrangement.PaymentTarget.Deduction == null)
                {
                    throw new ArgumentNullException("paymentTarget", "You must have either a commitment property or deduction property defined within the paymentTarget. ");
                }
                if (payrollDeductionArrangement.PaymentTarget.Commitment != null && (payrollDeductionArrangement.PaymentTarget.Commitment.Type == Dtos.EnumProperties.CommitmentTypes.NotSet || payrollDeductionArrangement.PaymentTarget.Commitment.Type == null))
                {
                    throw new ArgumentNullException("paymentTarget.commitment.type", "The commitment type is either invalid or missing.  The commitment type is required when submitting with the commitment object. ");
                }
                if (payrollDeductionArrangement.PaymentTarget.Deduction != null && (payrollDeductionArrangement.PaymentTarget.Deduction.DeductionType == null || string.IsNullOrEmpty(payrollDeductionArrangement.PaymentTarget.Deduction.DeductionType.Id)))
                {
                    throw new ArgumentNullException("paymentTarget.deduction.deductionType.id", "The paymentTarget.deduction.deductionType.id is required when submitting with the deduction object. ");
                }
            }
            // Validate Payment Amount
            if (payrollDeductionArrangement.amountPerPayment == null || !payrollDeductionArrangement.amountPerPayment.Value.HasValue)
            {
                throw new ArgumentNullException("amountPerPayment.value", "The amountPerPayment.value property is required for a payroll deduction. ");
            }
            if (payrollDeductionArrangement.amountPerPayment != null && payrollDeductionArrangement.amountPerPayment.Value.HasValue)
            {
                if (payrollDeductionArrangement.amountPerPayment.Currency != Dtos.EnumProperties.CurrencyCodes.CAD && payrollDeductionArrangement.amountPerPayment.Currency != Dtos.EnumProperties.CurrencyCodes.USD)
                {
                    throw new ArgumentException("Only USD and CAD currency values are allowed. ", "amountPerPayment.Currency");
                }
            }
            // Validate Total Amount
            if (payrollDeductionArrangement.TotalAmount != null && !payrollDeductionArrangement.TotalAmount.Value.HasValue)
            {
                throw new ArgumentNullException("totalAmount.value", "The totalAmount.value property must be set when submitting the totalAmount object. ");
            }
            if (payrollDeductionArrangement.TotalAmount != null && payrollDeductionArrangement.TotalAmount.Value.HasValue)
            {
                if (payrollDeductionArrangement.TotalAmount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD && payrollDeductionArrangement.TotalAmount.Currency != Dtos.EnumProperties.CurrencyCodes.USD)
                {
                    throw new ArgumentException("Only USD and CAD currency values are allowed. ", "totalAmount.Currency");
                }
            }
            // Validate start and end dates
            if (!payrollDeductionArrangement.StartDate.HasValue)
            {
                throw new ArgumentNullException("startOn", "The startOn property is required when submitting a payroll deduction. ");
            }
            if (payrollDeductionArrangement.EndDate.HasValue && payrollDeductionArrangement.EndDate < payrollDeductionArrangement.StartDate)
            {
                throw new ArgumentNullException("endOn", "The endOn property, if included, must be greater than the startOn propertly. ");
            }
            // Validate Pay Period Occurance
            if (payrollDeductionArrangement.PayPeriodOccurence == null)
            {
                throw new ArgumentNullException("payPeriodOccurence", "The payPeriodOccurance property is required for a payroll deduction. ");
            }
            else
            {
                if (payrollDeductionArrangement.PayPeriodOccurence.Interval == null && payrollDeductionArrangement.PayPeriodOccurence.MonthlyPayPeriods == null)
                {
                    throw new ArgumentNullException("payPeriodOccurence", "You must have either a interval property or monthlyPayPeriods property defined within the payPeriodOccurence. ");
                }
                if (payrollDeductionArrangement.PayPeriodOccurence.Interval != null && (payrollDeductionArrangement.PayPeriodOccurence.Interval <= 0 || payrollDeductionArrangement.PayPeriodOccurence.Interval > 31))
                {
                    throw new ArgumentNullException("payPeriodOccurence.interval", "The payPeriodOccurence.interval must be a positive number between 1 and 31. ");
                }
                if (payrollDeductionArrangement.PayPeriodOccurence.MonthlyPayPeriods != null)
                {
                    foreach (var payPeriod in payrollDeductionArrangement.PayPeriodOccurence.MonthlyPayPeriods)
                    {
                        if (payPeriod == 0 || payPeriod > 31 || payPeriod < -1)
                        {
                            throw new ArgumentNullException("payPeriodOccurence.monthlyPayPeriods", "The payPeriodOccurence.monthlyPayPeriods must be a positive number between -1 and 31, excluding 0. ");
                        }
                    }

                }
                if (payrollDeductionArrangement.PayPeriodOccurence.Interval != null && payrollDeductionArrangement.PayPeriodOccurence.MonthlyPayPeriods != null)
                {
                    throw new ArgumentNullException("payPeriodOccurence", "You have entered both interval property and monthlyPayPeriods property. You can only specify one");
                }
            }
            // Validate Change Reason
            if (payrollDeductionArrangement.ChangeReason != null && string.IsNullOrEmpty(payrollDeductionArrangement.ChangeReason.Id))
            {
                throw new ArgumentNullException("changeReason.id", "The changeReason.id property is required when submitting the changeReason object. ");
            }
        }
        #endregion
    }
}
