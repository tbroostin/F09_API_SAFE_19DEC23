//Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

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
            PayrollDeduction payrollDeductionDomainEntity = null;
            Dictionary<string, string> arrangementGuidCollection = null;
            try
            {
                payrollDeductionDomainEntity = await _contributionPayrollDeductionsRepository.GetContributionPayrollDeductionByGuidAsync(guid);

                if (payrollDeductionDomainEntity == null)
                {
                    throw new KeyNotFoundException("contribution-payroll-deductions not found for GUID '" + guid + "'");
                }

                arrangementGuidCollection = await _contributionPayrollDeductionsRepository.GetPerbenGuidsCollectionAsync(new List<string> { payrollDeductionDomainEntity.ArrangementId });

            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("contribution-payroll-deductions not found for GUID '" + guid + "'", ex);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }                
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error");
                throw IntegrationApiException;
            }

            Ellucian.Colleague.Dtos.ContributionPayrollDeductions retval = null;
            try
            {
                retval = ConvertContributionPayrollDeductionsEntityToDto(payrollDeductionDomainEntity,arrangementGuidCollection);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error");
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return retval;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all ContributionPayrollDeductions
        /// </summary>
        /// <returns>Collection of ContributionPayrollDeductions DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>> GetContributionPayrollDeductionsAsync(int offset, int limit,
            string arrangement = "", bool bypassCache = false)
        {
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
                catch (Exception e)
                {
                    return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), 0);
                }
            }

            Tuple<IEnumerable<PayrollDeduction>, int> contributionPayrollDeductionsEntitiesTuple = null;
            try
            {
                contributionPayrollDeductionsEntitiesTuple = await _contributionPayrollDeductionsRepository.GetContributionPayrollDeductionsAsync(offset, limit, arrangementCode, bypassCache);
            }

            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error");
                throw IntegrationApiException;
            }

            if (contributionPayrollDeductionsEntitiesTuple == null || contributionPayrollDeductionsEntitiesTuple.Item1 == null)
            {
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>(), 0);
            }

            var contributionPayrollDeductions = new List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>();

            var arrangementIds = contributionPayrollDeductionsEntitiesTuple.Item1
                    .Where(x => (!string.IsNullOrEmpty(x.ArrangementId)))
                    .Select(x => x.ArrangementId).Distinct().ToList();

            var arrangementGuidCollection = await _contributionPayrollDeductionsRepository.GetPerbenGuidsCollectionAsync(arrangementIds);


            foreach (var contributionPayrollDeduction in contributionPayrollDeductionsEntitiesTuple.Item1)
            {
                try
                {
                    contributionPayrollDeductions.Add(this.ConvertContributionPayrollDeductionsEntityToDto(contributionPayrollDeduction, arrangementGuidCollection));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error");
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>(contributionPayrollDeductions, contributionPayrollDeductionsEntitiesTuple.Item2);
        }


        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a ContributionPayrollDeductions domain entity to its corresponding ContributionPayrollDeductions DTO
        /// </summary>
        /// <param name="source">ContributionPayrollDeductions domain entity</param>
        /// <returns>ContributionPayrollDeductions DTO</returns>
        private Ellucian.Colleague.Dtos.ContributionPayrollDeductions ConvertContributionPayrollDeductionsEntityToDto(PayrollDeduction source, Dictionary<string, string> arrangementGuidCollection)
        {
            var contributionPayrollDeductions = new Ellucian.Colleague.Dtos.ContributionPayrollDeductions();

            contributionPayrollDeductions.Id = source.Guid;

            if (!string.IsNullOrEmpty(source.ArrangementId))
            {
                if (arrangementGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("PERBEN GUID not found for arrangement: '", source.ArrangementId, "'"), "GUID.Not.Found"
                        , source.Id, source.Guid);
                }
                else
                {
                    var arrangementGuid = string.Empty;
                    arrangementGuidCollection.TryGetValue(source.ArrangementId, out arrangementGuid);
                    if (string.IsNullOrEmpty(arrangementGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("PERBEN GUID not found for arrangement: '", source.ArrangementId, "'"), "GUID.Not.Found"
                            , source.Id, source.Guid);
                    }
                    else
                    {
                        contributionPayrollDeductions.Arrangement = new GuidObject2(arrangementGuid);
                    }
                }
            }
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
    }
}