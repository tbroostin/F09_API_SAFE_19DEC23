//Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Entities;

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



        //private IEnumerable<Domain.HumanResources.Entities.DeductionType> _deductionTypes = null;
        //private async Task<IEnumerable<Domain.HumanResources.Entities.DeductionType>> GetDeductionTypesAsync(bool bypassCache)
        //{
        //    if (_deductionTypes == null)
        //    {
        //        _deductionTypes = await _hrReferenceDataRepository.GetDeductionTypesAsync(bypassCache);
        //    }
        //    return _deductionTypes;
        //}


        private IEnumerable<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason> _payrollDeductionArrangementChangeReasons = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonsAsync(bool bypassCache)
        {
            if (_payrollDeductionArrangementChangeReasons == null)
            {
                _payrollDeductionArrangementChangeReasons = await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(bypassCache);
            }
            return _payrollDeductionArrangementChangeReasons;
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

            var payrollDeductionArrangements = new List<Dtos.PayrollDeductionArrangements>();
            string personId = "";
            if (!string.IsNullOrEmpty(person))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personId))
                    {
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
                try
                {
                    var deductionEntity = (await _hrReferenceDataRepository.GetDeductionTypesAsync(false)).Where(dt => dt.Guid == deductionType).FirstOrDefault();
                    if (deductionEntity == null)
                    {
                        return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                    }
                    deductionTypeCode = deductionEntity.Code;
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
                }
            }

            if ((!string.IsNullOrWhiteSpace(status))
                && ((!string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
                    || (!string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase))))
            {
                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
            }

            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int> pageOfItems = null;

            try
            {
                pageOfItems = await _payrollDeductionArrangementRepository.GetAsync(offset, limit, bypassCache, personId, contribution, deductionTypeCode, status);
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

            if (pageOfItems == null || pageOfItems.Item1 == null)
            {
                return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(new List<Dtos.PayrollDeductionArrangements>(), 0);
            }

            var personIds = pageOfItems.Item1
                 .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                 .Select(x => x.PersonId).Distinct().ToList();

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

            var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
            foreach (var payrollDeductionArrangement in pageOfItems.Item1)
            {
                try
                {
                    payrollDeductionArrangements.Add(await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangement, hostCountry, 
                        personGuidCollection, bypassCache));
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

            return new Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>(payrollDeductionArrangements, pageOfItems.Item2);

        }

        /// <summary>
        /// Gets a payroll deduction arrangement by guid.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangementsByGuidAsync(string id, bool bypassCache = false)
        {
            Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangementEntity = null;

            try
            {
                payrollDeductionArrangementEntity = await _payrollDeductionArrangementRepository.GetByIdAsync(id);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No payroll-deduction-arrangements was found for GUID '" + id + "'", ex);
            }
            Dtos.PayrollDeductionArrangements retval = null;
            try
            {
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new List<string> { payrollDeductionArrangementEntity.PersonId });
                var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
                retval = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity, hostCountry, personGuidCollection, bypassCache);
              
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No payroll-deduction-arrangements was found for GUID '" + id + "'", ex);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return retval;
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

            //var personId = payrollDeductionArrangementEntity.PersonId;


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
            var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new List<string> { payrollDeductionArrangementEntity.PersonId });
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity, hostCountry, personGuidCollection);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Create a new payroll deduction arrangement
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.PayrollDeductionArrangements> CreatePayrollDeductionArrangementsAsync(Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto)
        {
           
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
            var hostCountry = await _payrollDeductionArrangementRepository.GetHostCountryAsync();
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new List<string> { payrollDeductionArrangementEntity.PersonId });
            payrollDeductionArrangementDto = await ConvertPayrollDeductionArrangementsEntityToDtoAsync(payrollDeductionArrangementEntity, hostCountry, personGuidCollection);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return payrollDeductionArrangementDto;
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>PayrollDeductionArrangement DTO object</returns>
        private async Task<Dtos.PayrollDeductionArrangements> ConvertPayrollDeductionArrangementsEntityToDtoAsync
            (Domain.HumanResources.Entities.PayrollDeductionArrangements source, string hostCountry, Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {

            if (source == null)
            {
                IntegrationApiExceptionAddError("PayrollDeductionArrangements is required.");
                return null;

            }
            var payrollDeductionArrangement = new Dtos.PayrollDeductionArrangements()
            {
                Id = source.Guid,
                StartDate = source.StartDate,
                EndDate = source.EndDate
            };


            if (!string.IsNullOrEmpty(source.DeductionTypeCode))
            {
                try
                {
                    var deductionGuid = await _hrReferenceDataRepository.GetDeductionTypesGuidAsync(source.DeductionTypeCode);

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

                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data");
                }
            }

            if (string.IsNullOrEmpty(source.PersonId))
            {
                IntegrationApiExceptionAddError("PersonId is a required field.", "Bad.Data", source.Guid, source.Id);
            }
            else
            {
                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to locate guid for personId: '", source.PersonId, "'"), "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to locate guid for personId: '", source.PersonId, "'"), "GUID.Not.Found", source.Guid, source.Id);
                    }
                    payrollDeductionArrangement.Person = new GuidObject2(personGuid);
                }
            }
            
            // Get the status of the payroll deduction.
            payrollDeductionArrangement.Status = (Dtos.EnumProperties.PayrollDeductionArrangementStatuses)Enum.Parse(typeof(Dtos.EnumProperties.PayrollDeductionArrangementStatuses), source.Status);

            var currencyCode = Dtos.EnumProperties.CurrencyCodes.USD;          
            if (hostCountry == "CANADA")
            {
                currencyCode = Dtos.EnumProperties.CurrencyCodes.CAD;
            }

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
                try
                {
                    payrollDeductionArrangement.ChangeReason = new GuidObject2(
                       await _hrReferenceDataRepository.GetPayrollDeductionArrangementChangeReasonsGuidAsync(source.ChangeReason));
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data");
                }
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
