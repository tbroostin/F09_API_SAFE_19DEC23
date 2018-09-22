//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class ContributionPayrollDeductionsService : BaseCoordinationService, IContributionPayrollDeductionsService
    {
        private readonly IContributionPayrollDeductionsRepository _contributionPayrollDeductionsRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ContributionPayrollDeductionsService(

            IContributionPayrollDeductionsRepository contributionPayrollDeductionsRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _contributionPayrollDeductionsRepository = contributionPayrollDeductionsRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a ContributionPayrollDeductions from its GUID
        /// </summary>
        /// <returns>ContributionPayrollDeductions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ContributionPayrollDeductions> GetContributionPayrollDeductionsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a ContributionPayrollDeduction.");
            } 
            try
            {
                CheckGetViewContributionPayrollDeductionsPermission();

                return ConvertContributionPayrollDeductionsEntityToDto(
                    (await _contributionPayrollDeductionsRepository.GetContributionPayrollDeductionByGuidAsync(guid)));

            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("contribution-payroll-deductions not found for GUID " + guid, ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("contribution-payroll-deductions not found for GUID " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all ContributionPayrollDeductions
        /// </summary>
        /// <returns>Collection of ContributionPayrollDeductions DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>> GetContributionPayrollDeductionsAsync(int offset, int limit,
            string arrangement = "",  bool bypassCache = false)
        {
            try
            {
                CheckGetViewContributionPayrollDeductionsPermission();

                string arrangementCode = "";
                if (!string.IsNullOrEmpty(arrangement))
                {
                    try
                    {
                        arrangementCode = await _contributionPayrollDeductionsRepository.GetKeyFromGuidAsync(arrangement);
                        if (string.IsNullOrEmpty(arrangementCode))
                        {
                            return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), 0);
                        }
                    }
                    catch(KeyNotFoundException e)
                    {
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), 0);
                    }
                }

                var contributionPayrollDeductionsEntitiesTuple = await _contributionPayrollDeductionsRepository.GetContributionPayrollDeductionsAsync(offset, limit, arrangementCode, bypassCache);
                if (contributionPayrollDeductionsEntitiesTuple != null)
                {
                    var contributionPayrollDeductionsEntities = contributionPayrollDeductionsEntitiesTuple.Item1.ToList();
                    var totalCount = contributionPayrollDeductionsEntitiesTuple.Item2;

                    if (contributionPayrollDeductionsEntities.Any())
                    {
                        var contributionPayrollDeductions = new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>();

                        foreach (var contributionPayrollDeduction in contributionPayrollDeductionsEntities)
                        {
                            contributionPayrollDeductions.Add(this.ConvertContributionPayrollDeductionsEntityToDto(contributionPayrollDeduction));
                        }
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(contributionPayrollDeductions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a ContributionPayrollDeductions domain entity to its corresponding ContributionPayrollDeductions DTO
        /// </summary>
        /// <param name="source">ContributionPayrollDeductions domain entity</param>
        /// <returns>ContributionPayrollDeductions DTO</returns>
        private Ellucian.Colleague.Dtos.ContributionPayrollDeductions ConvertContributionPayrollDeductionsEntityToDto(PayrollDeduction source)
        {
            var contributionPayrollDeductions = new Ellucian.Colleague.Dtos.ContributionPayrollDeductions();

            contributionPayrollDeductions.Id = source.Guid;
            contributionPayrollDeductions.Arrangement = new GuidObject2(source.ArrangementGuid);
            contributionPayrollDeductions.DeductedOn = source.DeductionDate;
            var amount = new AmountDtoProperty
            {
                Value = source.Amount
            };
            if (source.AmountCountry.Equals("USA", StringComparison.OrdinalIgnoreCase))
            {
                amount.Currency = CurrencyCodes.USD;
            }
            else if (source.AmountCountry.Equals("CAN", StringComparison.OrdinalIgnoreCase))
            {
                amount.Currency = CurrencyCodes.CAD;
            }
            contributionPayrollDeductions.Amount = amount;

            return contributionPayrollDeductions;
        }


        /// <summary>
        /// Helper method to determine if the user has permission to view ContributionPayrollDeductions.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetViewContributionPayrollDeductionsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Institution Jobs.");
            }
        }
    }
}