//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Exceptions;

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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets a collection of LeavePlans DTO objects
        /// </summary>
        /// <returns>Collection of LeavePlans DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.LeavePlans>, int>> GetLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            var leavePlansCollection = new List<Ellucian.Colleague.Dtos.LeavePlans>();
            Tuple<IEnumerable<LeavePlan>, int> leavePlansEntities = null;

            try
            {
                leavePlansEntities = await _leavePlansRepository.GetLeavePlansAsync(offset, limit, bypassCache);
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

            if ((leavePlansEntities == null) || (leavePlansEntities.Item1 == null) ||(!leavePlansEntities.Item1.Any()))
            {
                return new Tuple<IEnumerable<LeavePlans>, int>(new List<Dtos.LeavePlans>(), 0);
            }
            foreach (var leavePlans in leavePlansEntities.Item1)
            {
                try
                {
                    leavePlansCollection.Add(await ConvertLeavePlansEntityToDto(leavePlans, bypassCache));
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
            return new Tuple<IEnumerable<LeavePlans>, int>(leavePlansCollection, leavePlansEntities.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a LeavePlans DTO from its GUID
        /// </summary>
        /// <returns>LeavePlans DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LeavePlans> GetLeavePlansByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a leave-plans.");
            }

            LeavePlan leavePlanEntity = null;
            try
            {
                leavePlanEntity = await _leavePlansRepository.GetLeavePlansByIdAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No leave-plans was found for GUID '{0}'", guid));
            }

            Ellucian.Colleague.Dtos.LeavePlans retval = null;
            try
            {
                 retval = await ConvertLeavePlansEntityToDto(leavePlanEntity, bypassCache);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No leave-plans was found for GUID '{0}'", guid));
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return retval;

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a LeavePlan domain entity to its corresponding LeavePlans DTO
        /// </summary>
        /// <param name="source">LeavePlan domain entity</param>
        /// <returns>LeavePlans DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.LeavePlans> ConvertLeavePlansEntityToDto(LeavePlan source, bool bypassCache = false)
        {
            var leavePlans = new Ellucian.Colleague.Dtos.LeavePlans()
            {
                Id = source.Guid,
                Code = source.Id,
                Title = source.Title,
                StartOn = source.StartDate,
                EndOn = source.EndDate,
                WaitDays = source.WaitDays
            };

            if (!string.IsNullOrEmpty(source.Type))
            {
                try
                {
                    var leaveTypeGuid = await _referenceDataRepository.GetLeaveTypesGuidAsync(source.Type);
                    if (string.IsNullOrEmpty(leaveTypeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("No Guid found, Entity:'HR-VALCODES, LEAVE.TYPES', Record ID:'", source.Type, "'")
                            , "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        leavePlans.Type = new GuidObject2(leaveTypeGuid);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.AccrualMethod))
            {
                switch (source.AccrualMethod.ToLower())
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

            if ((!string.IsNullOrEmpty(source.AllowNegative)) 
                && (string.Equals(source.AllowNegative, "Y", StringComparison.OrdinalIgnoreCase)))
            {
                leavePlans.Usage = Dtos.EnumProperties.LeavePlansUsage.Beforeaccrued;
            }
            else
            {
                leavePlans.Usage = Dtos.EnumProperties.LeavePlansUsage.Afteraccrued;
            }

            //get year start month and day
            if (source.YearlyStartDate.HasValue)
            {
                try
                {
                    var planStart = new Dtos.DtoProperties.DateDtoProperty();
                    planStart.Day = int.Parse(source.YearlyStartDate.Value.ToString().Split("/".ToCharArray())[1]);
                    planStart.Month = int.Parse(source.YearlyStartDate.Value.ToString().Split("/".ToCharArray())[0]);
                    leavePlans.PlanYearStart = planStart;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.RollOverLeaveType))
            {
                try
                {
                    var rolloverLeaveTypeGuid = await _referenceDataRepository.GetLeaveTypesGuidAsync(source.RollOverLeaveType);
                    if (string.IsNullOrEmpty(rolloverLeaveTypeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("No Guid found, Entity:'HR-VALCODES, LEAVE.TYPES', Record ID:'", source.RollOverLeaveType, "'")
                            , "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        leavePlans.AlternateRolloverLeaveType = new GuidObject2(rolloverLeaveTypeGuid);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.AccuralFrequency))
            {
                try
                {
                    var employmentFrequencyGuid = await _referenceDataRepository.GetEmploymentFrequenciesGuidAsync(source.AccuralFrequency);
                    if (string.IsNullOrEmpty(employmentFrequencyGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("No Guid found, Entity:'HR-VALCODES, TIME.FREQUENCIES', Record ID:'", source.AccuralFrequency, "'")
                            , "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        leavePlans.AccrualFrequency = new GuidObject2(employmentFrequencyGuid);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", source.Guid, source.Id);
                }
            }

            return leavePlans;
        }
    }
}