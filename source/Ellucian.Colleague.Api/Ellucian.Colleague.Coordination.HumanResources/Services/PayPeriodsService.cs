//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayPeriodsService : BaseCoordinationService, IPayPeriodsService
    {

        private readonly IPayPeriodsRepository _payPeriodRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public PayPeriodsService(

            IPayPeriodsRepository payPeriodRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _payPeriodRepository = payPeriodRepository;
            _hrReferenceDataRepository = hrReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-periods
        /// </summary>
        /// <returns>Collection of PayPeriods DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>, int>> GetPayPeriodsAsync(int offset, int limit, 
            string payCycle = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            var payCycleCode = string.Empty;
            if (!string.IsNullOrEmpty(payCycle))
            {
                var payCycles = await this.GetPayCyclesAsync(bypassCache);
                if (payCycles != null)
                {
                    var pc = payCycles.FirstOrDefault(x => x.Guid == payCycle);
                    if (pc != null)
                        payCycleCode = pc.Code;
                }
                if (string.IsNullOrEmpty(payCycleCode))
                    // no results
                    return new Tuple<IEnumerable<Dtos.PayPeriods>, int>(new List<Dtos.PayPeriods>(), 0);

            }

            var convertedStartOn = string.Empty;
            var convertedEndOn = string.Empty;

            if (!(string.IsNullOrWhiteSpace(startOn)))
            {
                convertedStartOn = await ConvertDateArgument(startOn);
                if (string.IsNullOrEmpty(convertedStartOn))
                    // no results
                    return new Tuple<IEnumerable<Dtos.PayPeriods>, int>(new List<Dtos.PayPeriods>(), 0);
            }

            if (!(string.IsNullOrWhiteSpace(endOn)))
            {
                convertedEndOn = await ConvertDateArgument(endOn);
                if (string.IsNullOrEmpty(convertedEndOn))
                    // no results
                    return new Tuple<IEnumerable<Dtos.PayPeriods>, int>(new List<Dtos.PayPeriods>(), 0);
            }

            var payPeriodsCollection = new List<Ellucian.Colleague.Dtos.PayPeriods>();

            var pageOfItems = await _payPeriodRepository.GetPayPeriodsAsync(offset, limit, payCycleCode, convertedStartOn, convertedEndOn, bypassCache);
            
            var payPeriodsEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (payPeriodsEntities != null && payPeriodsEntities.Any())
            {
                foreach (var payPeriods in payPeriodsEntities)
                {
                    payPeriodsCollection.Add(await ConvertPayPeriodsEntityToDto(payPeriods));
                }
                return new Tuple<IEnumerable<Dtos.PayPeriods>, int>(payPeriodsCollection, totalRecords);
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.PayPeriods>, int>(new List<Ellucian.Colleague.Dtos.PayPeriods>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayPeriods from its GUID
        /// </summary>
        /// <returns>PayPeriods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayPeriods> GetPayPeriodsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Pay Period.");
            }

            try
            {
                var entity = await _payPeriodRepository.GetPayPeriodByIdAsync(guid);

                if (entity != null) 
                {
                    return await ConvertPayPeriodsEntityToDto(entity, true);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("pay-periods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("pay-periods not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Paycycle domain entity to its corresponding PayPeriods DTO
        /// </summary>
        /// <param name="source">Paycycle domain entity</param>
        /// <returns>PayPeriods DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PayPeriods> ConvertPayPeriodsEntityToDto(PayPeriod source, bool bypassCache = false)
        {
            var payPeriods = new Ellucian.Colleague.Dtos.PayPeriods();

            payPeriods.Id = source.Id;
            payPeriods.Title = source.Description;
            payPeriods.Description = null;
            payPeriods.StartOn = source.StartDate2;
            payPeriods.EndOn = source.EndDate2;
            payPeriods.PayDate = source.PayDate;
            var payCycle = await GetPayCyclesAsync(bypassCache);

            if (payCycle.Any())
            {
                var payCycleEntity = payCycle.FirstOrDefault(ep => ep.Code == source.PayCycle);
                if (payCycleEntity != null)
                {
                    payPeriods.PayCycle = new GuidObject2(payCycleEntity.Guid);
                }
                else
                {
                    throw new KeyNotFoundException("Key not found for pay cycle property of pay-periods.");
                }
            }
            else
            {
                throw new RepositoryException("Pay cycles records not found.");
            }

            payPeriods.TimeEntryEndOn = source.TimeEntryEndOn;
                                                                                                                                    
            return payPeriods;
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
                return await _referenceDataRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        //get pay-cycles
        private IEnumerable<PayCycle2> _payCycles = null;
        private async Task<IEnumerable<PayCycle2>> GetPayCyclesAsync(bool bypassCache)
        {
            if (_payCycles == null)
            {
                _payCycles = await _hrReferenceDataRepository.GetPayCyclesAsync(bypassCache);
            }
            return _payCycles;
        }
      
    }
   
}