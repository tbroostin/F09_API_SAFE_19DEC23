//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    [RegisterType]
    public class BudgetPhasesService : BaseCoordinationService, IBudgetPhasesService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public BudgetPhasesService(
            IBudgetRepository budgetRepository,
            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _budgetRepository = budgetRepository;
            _referenceDataRepository = referenceDataRepository;
        }


        public async Task<IEnumerable<BudgetPhases>> GetBudgetPhasesAsync(string budgetCodeGuid, bool bypassCache = false)
        {
            CheckViewBudgetPhasesPermission();
            var budgetPhasesCollection = new List<Ellucian.Colleague.Dtos.BudgetPhases>();
            var budgetCode = string.Empty;
            if (!string.IsNullOrEmpty(budgetCodeGuid))
            {
                try
                {
                    budgetCode = await _budgetRepository.GetBudgetCodesIdFromGuidAsync(budgetCodeGuid);
                }
                catch (KeyNotFoundException)
                {
                    return budgetPhasesCollection;
                }
            }

            var budgetPhasesEntities = await _budgetRepository.GetBudgetPhasesAsync(budgetCode, bypassCache);

            if (budgetPhasesEntities.Item1 != null)
            {
                foreach (var budgetPhases in budgetPhasesEntities.Item1)
                {
                    budgetPhasesCollection.Add(await ConvertBudgetPhasesEntityToDtoAsync(budgetPhases, budgetPhasesEntities.Item1, bypassCache));
                }
            }
            return budgetPhasesCollection;
        }

        public async Task<BudgetPhases> GetBudgetPhasesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                CheckViewBudgetPhasesPermission();

                return await ConvertBudgetPhasesEntityToDtoAsync(await _budgetRepository.GetBudgetPhasesAsync(guid), null, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No budget phase was found for guid " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No budget phase was found for guid " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Budget domain entity to its corresponding BudgetPhases DTO
        /// </summary>
        /// <param name="source">Budget domain entity</param>
        /// <returns>BudgetPhases DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.BudgetPhases> ConvertBudgetPhasesEntityToDtoAsync(Budget source, 
            IEnumerable<Budget> budgetPhases = null, bool bypassCache = false)
        {
            var budgetPhase = new Ellucian.Colleague.Dtos.BudgetPhases();

            var budgetId = source.RecordKey;

            if (string.IsNullOrEmpty(budgetId))
            {
                throw new ArgumentNullException("Budget record missing record key.");
            }
            var containsUnderScore = budgetId.Contains("_");
            var budgetCode = containsUnderScore && !(string.IsNullOrEmpty(budgetId))
                ? budgetId.Substring(0, budgetId.IndexOf('_')) : budgetId;


            budgetPhase.Id = source.BudgetPhaseGuid;
            budgetPhase.Code = budgetId;
            budgetPhase.Description = null;

            if (!(string.IsNullOrEmpty(budgetId)))
            {
                if (containsUnderScore)
                {
                    budgetPhase.Title = string.IsNullOrEmpty(source.CurrentVersionDesc)
                        ? source.CurrentVersionName : source.CurrentVersionDesc;
                }
                else
                {
                    budgetPhase.Title = string.IsNullOrEmpty(source.Title) ? budgetId : source.Title;
                }

                if (budgetPhases != null && budgetPhases.Any())
                {
                    var budgetPhaseCode = budgetPhases.FirstOrDefault(b => b.RecordKey == budgetCode);
                    if (budgetPhaseCode != null)
                    {
                        budgetPhase.BudgetCode = new GuidObject2(budgetPhaseCode.BudgetCodeGuid);
                    }
                }
                else
                {
                    var budgetCodeGuid = await _budgetRepository.GetBudgetCodesGuidFromIdAsync(budgetCode);
                    if (budgetCodeGuid != null)
                    {
                        budgetPhase.BudgetCode = new GuidObject2(budgetCodeGuid);
                    }
                }
            }

            budgetPhase.Status = !containsUnderScore && source.Status == "W"
                ? OpenStatus.Open : OpenStatus.Closed;
            if (source.Version != null)
            {
                var version = source.Version.FirstOrDefault();

                if ((!string.IsNullOrEmpty(version)) && (!string.IsNullOrEmpty(budgetCode)))
                {
                    //budget-code + an underscore + the first value of BU.VERSION
                    var budgetCodeVersion = string.Concat(budgetCode.Trim(), "_", version.Trim());

                    if (budgetPhases != null && budgetPhases.Any())
                    {
                        var budgetVersion = budgetPhases.FirstOrDefault(b => b.RecordKey == budgetCodeVersion);
                        if (budgetVersion != null)
                        {
                            budgetPhase.BasePhase = new GuidObject2(budgetVersion.BudgetPhaseGuid);
                        }
                    }
                    else {
                        var budgetVersionGuid = await _budgetRepository.GetBudgetPhasesGuidFromIdAsync(budgetCodeVersion);
                        if (budgetVersionGuid != null)
                        {
                            budgetPhase.BasePhase = new GuidObject2(budgetVersionGuid);
                        }
                    }
                }
            }
            return budgetPhase;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to outgoing payments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBudgetPhasesPermission()
        {
            var hasPermission = HasPermission(BudgetManagementPermissionCodes.ViewBudgetPhase);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view budget-phases.");
            }
        }
    }
}