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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayClassesService : BaseCoordinationService, IPayClassesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public PayClassesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-classes
        /// </summary>
        /// <returns>Collection of PayClasses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayClasses>> GetPayClassesAsync(bool bypassCache = false)
        {
            var payClassesCollection = new List<Ellucian.Colleague.Dtos.PayClasses>();

            var payClassesEntities = await _referenceDataRepository.GetPayClassesAsync(bypassCache);
            if (payClassesEntities != null && payClassesEntities.Any())
            {
                foreach (var payClasses in payClassesEntities)
                {
                    payClassesCollection.Add(await ConvertPayClassesEntityToDto(payClasses, bypassCache));
                }
            }
            return payClassesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayClasses from its GUID
        /// </summary>
        /// <returns>PayClasses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayClasses> GetPayClassesByGuidAsync(string guid)
        {
            try
            {
                return await ConvertPayClassesEntityToDto((await _referenceDataRepository.GetPayClassesAsync(true)).Where(r => r.Guid == guid).First(), true);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("pay-classes not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("pay-classes not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-classes
        /// </summary>
        /// <returns>Collection of PayClasses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayClasses2>> GetPayClasses2Async(bool bypassCache = false)
        {
            var payClassesCollection = new List<Ellucian.Colleague.Dtos.PayClasses2>();

            var payClassesEntities = await _referenceDataRepository.GetPayClassesAsync(bypassCache);
            if (payClassesEntities != null && payClassesEntities.Any())
            {
                foreach (var payClasses in payClassesEntities)
                {
                    payClassesCollection.Add(await ConvertPayClassesEntityToDto2(payClasses, bypassCache));
                }
            }
            return payClassesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayClasses from its GUID
        /// </summary>
        /// <returns>PayClasses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayClasses2> GetPayClassesByGuid2Async(string guid)
        {
            try
            {
                return await ConvertPayClassesEntityToDto2((await _referenceDataRepository.GetPayClassesAsync(true)).Where(r => r.Guid == guid).First(), true);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("pay-classes not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("pay-classes not found for GUID " + guid, ex);
            }
        }

        private IEnumerable<Domain.HumanResources.Entities.EmploymentFrequency> _employmentFrequency = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentFrequency>> GetAllEmploymentFrequenciesAsync(bool bypassCache)
        {
            if (_employmentFrequency == null)
            {
                _employmentFrequency = await _referenceDataRepository.GetEmploymentFrequenciesAsync(bypassCache);
            }
            return _employmentFrequency;
        }

        private IEnumerable<Domain.HumanResources.Entities.PayCycle2> _payCycle = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.PayCycle2>> GetAllPayCyclesAsync(bool bypassCache)
        {
            if (_payCycle == null)
            {
                _payCycle = await _referenceDataRepository.GetPayCyclesAsync(bypassCache);
            }
            return _payCycle;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Payclass domain entity to its corresponding PayClasses DTO
        /// </summary>
        /// <param name="source">Payclass domain entity</param>
        /// <returns>PayClasses DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PayClasses> ConvertPayClassesEntityToDto(PayClass source, bool bypassCache)
        {
            var payClasses = new Ellucian.Colleague.Dtos.PayClasses();

            payClasses.Id = source.Guid;
            payClasses.Code = source.Code;
            payClasses.Title = source.Description;
            payClasses.Description = null;
            payClasses.PaysPerYear = source.PaysPerYear;
            payClasses.Status = ConvertPayClassesStatusDomainEnumToPayClassesStatusDtoEnum(source.Status);
            payClasses.CompensationType = ConvertPayClassesCompensationTypeDomainEnumToPayClassesCompensationTypeDtoEnum(source.CompensationType);

            var frequencyEntities = await GetAllEmploymentFrequenciesAsync(bypassCache);
            if (frequencyEntities.Any())
            {
                var frequencyEntity = frequencyEntities.FirstOrDefault(ep => ep.Code == source.PayFrequency);
                if (frequencyEntity != null)
                {
                    payClasses.PayFrequency = new GuidObject2(frequencyEntity.Guid);
                }
            }
                  
            //if ((source.CycleHoursPerPeriodHours != null && string.IsNullOrEmpty(source.CycleHoursPerPeriodPeriod)) || (source.YearHoursPerPeriodHours != null && string.IsNullOrEmpty(source.YearHoursPerPeriodPeriod)))
            if (source.CycleHoursPerPeriodHours != null && !string.IsNullOrEmpty(source.CycleHoursPerPeriodPeriod))
            {
                payClasses.HoursPerPeriod = new PayClassHoursPerPeriodDtoProperty() { Hours = source.CycleHoursPerPeriodHours, Period = PayClassesPeriod.PayPeriod };
                //payClasses.HoursPerPeriod.Add(new PayClassHoursPerPeriodDtoProperty() { Hours = source.YearHoursPerPeriodHours, Period = PayClassesPeriod.Year });
            }
                                                                                                                              
            return payClasses;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Payclass domain entity to its corresponding PayClasses DTO
        /// </summary>
        /// <param name="source">Payclass domain entity</param>
        /// <returns>PayClasses DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PayClasses2> ConvertPayClassesEntityToDto2(PayClass source, bool bypassCache)
        {
            var payClasses = new Ellucian.Colleague.Dtos.PayClasses2();

            payClasses.Id = source.Guid;
            payClasses.Code = source.Code;
            payClasses.Title = source.Description;
            payClasses.Description = null;
            payClasses.PaysPerYear = source.PaysPerYear;
            payClasses.Status = ConvertPayClassesStatusDomainEnumToPayClassesStatusDtoEnum(source.Status);
            payClasses.CompensationType = ConvertPayClassesCompensationTypeDomainEnumToPayClassesCompensationTypeDtoEnum(source.CompensationType);

            var frequencyEntities = await GetAllEmploymentFrequenciesAsync(bypassCache);
            if (frequencyEntities.Any())
            {
                var frequencyEntity = frequencyEntities.FirstOrDefault(ep => ep.Code == source.PayFrequency);
                if (frequencyEntity != null)
                {
                    payClasses.PayFrequency = new GuidObject2(frequencyEntity.Guid);
                }
            }

            var payCycleEntities = await GetAllPayCyclesAsync(bypassCache);
            if (payCycleEntities.Any())
            {
                var payCycleEntity = payCycleEntities.FirstOrDefault(ep => ep.Code == source.PayCycle);
                if (payCycleEntity != null)
                {
                    payClasses.PayCycle = new GuidObject2(payCycleEntity.Guid);
                }
            }

            if (!string.IsNullOrEmpty(source.CycleHoursPerPeriodPeriod) && source.CycleHoursPerPeriodPeriod.ToUpper().Equals("HRS"))
            {
                payClasses.HoursPerPeriodList =  new List<PayClassHoursPerPeriodDtoProperty>() { new PayClassHoursPerPeriodDtoProperty() { Hours = source.CycleHoursPerPeriodHours, Period = PayClassesPeriod.PayPeriod } };
            }

            if (!string.IsNullOrEmpty(source.YearHoursPerPeriodPeriod) && source.YearHoursPerPeriodPeriod.ToUpper().Equals("HRS"))
            {
                if (payClasses.HoursPerPeriodList.Any())
                {
                    payClasses.HoursPerPeriodList.Add(new PayClassHoursPerPeriodDtoProperty() { Hours = source.YearHoursPerPeriodHours, Period = PayClassesPeriod.Year });
                }
                else
                {
                    payClasses.HoursPerPeriodList = new List<PayClassHoursPerPeriodDtoProperty>() { new PayClassHoursPerPeriodDtoProperty() { Hours = source.YearHoursPerPeriodHours, Period = PayClassesPeriod.Year } };
                }
            }

            return payClasses;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayClass status domain enumeration value to its corresponding PayClassesStatus DTO enumeration value
        /// </summary>
        /// <param name="source">string domain enumeration value</param>
        /// <returns>PayClassesStatus DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.PayClassesStatus ConvertPayClassesStatusDomainEnumToPayClassesStatusDtoEnum(string source)
        {
            switch (source)
            {

                case "A":
                    return Dtos.EnumProperties.PayClassesStatus.Active;
                case "I":
                    return Dtos.EnumProperties.PayClassesStatus.Inactive;
                default:
                    return Dtos.EnumProperties.PayClassesStatus.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayClass compensation type domain enumeration value to its corresponding PayClassesCompensationType DTO enumeration value
        /// </summary>
        /// <param name="source">string domain enumeration value</param>
        /// <returns>PayClassesCompensationType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.PayClassesCompensationType ConvertPayClassesCompensationTypeDomainEnumToPayClassesCompensationTypeDtoEnum(string source)
        {
            switch (source)
            {

                case "S":
                    return Dtos.EnumProperties.PayClassesCompensationType.Salary;
                case "H":
                    return Dtos.EnumProperties.PayClassesCompensationType.Wages;
                default:
                    return Dtos.EnumProperties.PayClassesCompensationType.NotSet;
            }
        }
      
    }
   
}