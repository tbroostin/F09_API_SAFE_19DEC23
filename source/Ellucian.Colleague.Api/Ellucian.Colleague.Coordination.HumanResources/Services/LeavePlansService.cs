//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class LeavePlansService : BaseCoordinationService, ILeavePlansService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;
        private readonly ILeavePlansRepository _leavePlansRepository;


        public LeavePlansService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            ILeavePlansRepository leavePlansRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _leavePlansRepository = leavePlansRepository;
        }

        //get leave types
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeaveType> _leaveTypes = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeaveType>> GetLeaveTypesAsync(bool bypassCache)
        {
            if (_leaveTypes == null)
            {
                _leaveTypes = await _referenceDataRepository.GetLeaveTypesAsync(bypassCache);
            }
            return _leaveTypes;
        }
        //get employment frequences
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentFrequency> _employFrequencies = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentFrequency>> GetEmplymentFrequencies(bool bypassCache)
        {
            if (_employFrequencies == null)
            {
                _employFrequencies = await _referenceDataRepository.GetEmploymentFrequenciesAsync(bypassCache);
            }
            return _employFrequencies;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all leave-plans
        /// </summary>
        /// <returns>Collection of LeavePlans DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.LeavePlans>, int>> GetLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            var leavePlansCollection = new List<Ellucian.Colleague.Dtos.LeavePlans>();
            int leavePlansCount = 0;
            try
            {
                var leavePlansEntities = await _leavePlansRepository.GetLeavePlansAsync(offset, limit, bypassCache);
                if (leavePlansEntities != null)
                {
                    leavePlansCount = leavePlansEntities.Item2;
                    foreach (var leavePlans in leavePlansEntities.Item1)
                    {
                        var leaveplanDto = await ConvertLeavePlansEntityToDto(leavePlans, bypassCache);
                        if (leaveplanDto != null)
                        {
                            leavePlansCollection.Add(leaveplanDto);
                        }

                    }
                    return new Tuple<IEnumerable<LeavePlans>, int>(leavePlansCollection, leavePlansCount);

                }
                else
                {
                    return new Tuple<IEnumerable<LeavePlans>, int>(new List<Dtos.LeavePlans>(), 0);

                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a LeavePlans from its GUID
        /// </summary>
        /// <returns>LeavePlans DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LeavePlans> GetLeavePlansByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return await ConvertLeavePlansEntityToDto(await _leavePlansRepository.GetLeavePlansByIdAsync(guid),bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("leave-plans not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("leave-plans not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Leavplan domain entity to its corresponding LeavePlans DTO
        /// </summary>
        /// <param name="source">Leavplan domain entity</param>
        /// <returns>LeavePlans DTO</returns>
        private async  Task<Ellucian.Colleague.Dtos.LeavePlans> ConvertLeavePlansEntityToDto(LeavePlan source, bool bypassCache)
        {
            var leavePlans = new Ellucian.Colleague.Dtos.LeavePlans();

            leavePlans.Id = source.Guid;
            leavePlans.Code = source.Id;
            leavePlans.Title = source.Title;
            // get type
            if (!string.IsNullOrEmpty(source.Type))
            {
                var types = await GetLeaveTypesAsync(bypassCache);
                if (types == null || !types.Any())
                {
                    throw new ArgumentException("Leave Types are missing.");
                }
                var type = types.FirstOrDefault(c => c.Code == source.Type);
                if (type == null)
                {
                    throw new ArgumentException("Invalid leave type '" + source.Type + "' in the arguments");
                }
                leavePlans.Type = new GuidObject2(type.Guid);
            }
            // get accrualMethods
            if (!string.IsNullOrEmpty(source.AccrualMethod))
            { 
                switch(source.AccrualMethod.ToLower())
                {
                    case "s":
                        leavePlans.AccrualMethod = Dtos.EnumProperties.LeavePlansAccrualMethod.Payrollaccrual;
                        break;
                    case "h":
                        leavePlans.AccrualMethod = Dtos.EnumProperties.LeavePlansAccrualMethod.Payrollaccrual;
                        break;
                    case "p":
                        leavePlans.AccrualMethod = Dtos.EnumProperties.LeavePlansAccrualMethod.Frontload;
                        break;
                    default:
                        leavePlans.AccrualMethod = Dtos.EnumProperties.LeavePlansAccrualMethod.NotSet;
                        break;
                }
            }
            //get usage. 
            if (string.Equals(source.AllowNegative, "Y", StringComparison.OrdinalIgnoreCase))
            {
                leavePlans.Usage = Dtos.EnumProperties.LeavePlansUsage.Beforeaccrued;
            }
            else
            {
                leavePlans.Usage = Dtos.EnumProperties.LeavePlansUsage.Afteraccrued;
            }
            //get starton & endOn
            leavePlans.StartOn = source.StartDate;
            leavePlans.EndOn = source.EndDate;
            //get year start month and day
            if (source.YearlyStartDate.HasValue)
            {
                var planStart = new Dtos.DtoProperties.DateDtoProperty();
                planStart.Day = int.Parse(source.YearlyStartDate.Value.ToString().Split("/".ToCharArray())[1]);
                planStart.Month = int.Parse(source.YearlyStartDate.Value.ToString().Split("/".ToCharArray())[0]);
                leavePlans.PlanYearStart = planStart;
            }
            //get roll over leave type
            if (!string.IsNullOrEmpty(source.RollOverLeaveType))
            {
                var types = await GetLeaveTypesAsync(bypassCache);
                if (types == null || !types.Any())
                {
                    throw new ArgumentException("Leave Types are missing.");
                }
                var type = types.FirstOrDefault(c => c.Code == source.RollOverLeaveType);
                if (type == null)
                {
                    throw new ArgumentException("Invalid leave type '" + source.RollOverLeaveType + "' in the arguments");
                }
                leavePlans.AlternateRolloverLeaveType = new GuidObject2(type.Guid);
            }
            //get accural frequency
            if (!string.IsNullOrEmpty(source.AccuralFrequency))
            {
                var frequencies = await GetEmplymentFrequencies(bypassCache);
                if (frequencies == null || !frequencies.Any())
                {
                    throw new ArgumentException("Employment Frequencies are missing.");
                }
                var type = frequencies.FirstOrDefault(c => c.Code == source.AccuralFrequency);
                if (type == null)
                {
                    throw new ArgumentException("Invalid employment frequency '" + source.Type + "' in the arguments");
                }
                leavePlans.AccrualFrequency = new GuidObject2(type.Guid);
            }
            //get wait Days
            leavePlans.WaitDays = source.WaitDays;
            return leavePlans;
        }

    }
}