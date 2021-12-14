//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class GrantsService : BaseCoordinationService, IGrantsService
    {
        private readonly IGrantsRepository _grantsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IColleagueFinanceReferenceDataRepository _cfReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public GrantsService(
            IGrantsRepository grantsRepository, 
            IPersonRepository personRepository,
            IColleagueFinanceReferenceDataRepository cfReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _grantsRepository = grantsRepository;
            _personRepository = personRepository;
            _cfReferenceDataRepository = cfReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all grants
        /// </summary>
        /// <returns>Collection of Grants DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.Grant>, int>> GetGrantsAsync(int offset, int limit, string reportingSegment = "", string fiscalYearId = "", bool bypassCache = false)
        {
            var grantsDtoCollection = new List<Dtos.Grant>();
            Tuple<IEnumerable<ProjectCF>, int> grantsEntities = null;

            try
            {
                grantsEntities = await _grantsRepository.GetGrantsAsync(offset, limit, reportingSegment, fiscalYearId, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }

            if (grantsEntities != null && grantsEntities.Item1.Any())
            {
                foreach (var grant in grantsEntities.Item1)
                {
                    grantsDtoCollection.Add(await ConvertGrantsEntityToDtoAsync(grant, bypassCache));
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return grantsDtoCollection.Any() ? new Tuple<IEnumerable<Grant>, int>(grantsDtoCollection, grantsEntities.Item2) :
                new Tuple<IEnumerable<Grant>, int>(new List<Dtos.Grant>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Grants from its GUID
        /// </summary>
        /// <returns>Grants DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Grant> GetGrantsByGuidAsync(string guid, bool bypassCache = true)
        {
            Grant grantsDto = null;
            try
            {
                // await CheckGrantViewPermission();
                var grantsEntity = await _grantsRepository.GetProjectsAsync(guid);
                if (grantsEntity != null)
                {
                    grantsDto = await ConvertGrantsEntityToDtoAsync(grantsEntity, true);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Grants record not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Grants not found for GUID " + guid, ex);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return grantsDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Projects domain entity to its corresponding Grants DTO
        /// </summary>
        /// <param name="source">Projects domain entity</param>
        /// <returns>Grants DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Grant> ConvertGrantsEntityToDtoAsync(ProjectCF source, bool bypassCache)
        {
            var grant = new Ellucian.Colleague.Dtos.Grant();

            grant.Id = source.RecordGuid;

            if (string.IsNullOrEmpty(source.Title))
            {
                // throw new InvalidOperationException(string.Format("Title is required. Id: {0}", source.RecordGuid));
                IntegrationApiExceptionAddError(string.Format("Title is required. Id: {0}", source.RecordGuid), "Bad.Data", source.RecordGuid, source.RecordKey);
            }
            grant.Title = source.Title;

            if(string.IsNullOrEmpty(source.ReferenceCode))
            {
                // throw new InvalidOperationException(string.Format("Reference code is required. Id: {0}", source.RecordGuid));
                IntegrationApiExceptionAddError(string.Format("Reference code is required. Id: {0}", source.RecordGuid), "Bad.Data", source.RecordGuid, source.RecordKey);
            }
            grant.ReferenceCode = source.ReferenceCode;
            grant.SponsorReferenceCode = string.IsNullOrEmpty(source.SponsorReferenceCode)? null : source.SponsorReferenceCode;
            grant.ReportingSegment = string.IsNullOrEmpty(source.ReportingSegment)? null : source.ReportingSegment;
            grant.Amount = await ConvertAmountEntityToDtoAsync(source.BudgetAmount);

            if (!source.StartOn.HasValue)
            {
                // throw new InvalidOperationException(string.Format("Start on date is required. Id: {0}", source.RecordGuid));
                IntegrationApiExceptionAddError(string.Format("Start on date is required. Id: {0}", source.RecordGuid), "Bad.Data", source.RecordGuid, source.RecordKey);
            }
            else
            {
                grant.StartOn = source.StartOn.Value;
            }

            grant.EndOn = source.EndOn.HasValue? source.EndOn : default(DateTime?);
            grant.Status = ConvertEntityStatusToDto(source.CurrentStatus);
            grant.AccountingStrings = source.AccountingStrings != null && source.AccountingStrings.Any()? source.AccountingStrings : null;

            try
            {
                grant.Category = await ConvertEntityProjectTypeToDtoAsync(source.ProjectType, bypassCache);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.RecordGuid, source.RecordKey);
            }

            try
            {
                grant.ReportingPeriods = ConvertEntityReportingPeriodsToDto(source.RecordGuid, source.ReportingPeriods);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.RecordGuid, source.RecordKey);
            }

            grant.PrincipalInvestigator = await ConvertPrincipalInvestigatorEntityToDtoAsync(source.ProjectContactPerson, source.RecordGuid, source.RecordKey);

            return grant;
        }

        /// <summary>
        /// Converts entity to AmountDtoProperty.
        /// </summary>
        /// <param name="budgetAmount"></param>
        /// <returns></returns>
        private async Task<AmountDtoProperty> ConvertAmountEntityToDtoAsync(decimal? budgetAmount)
        {
            if (!budgetAmount.HasValue)
            {
                return null;
            }

            AmountDtoProperty amount = null;

            var currencyCode = Dtos.EnumProperties.CurrencyCodes.USD;
            var hostCountry = await _grantsRepository.GetHostCountryAsync();
            if (hostCountry == "CANADA")
            {
                currencyCode = Dtos.EnumProperties.CurrencyCodes.CAD;
            }
            if (budgetAmount.HasValue)
            {
                amount = new AmountDtoProperty()
                {
                    Value = budgetAmount.Value,
                    Currency = currencyCode
                };
            }
            return amount;
        }

        /// <summary>
        /// Converts to status dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private GrantsStatus ConvertEntityStatusToDto(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (source.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    return GrantsStatus.Active;
                }
            }
            return GrantsStatus.Inactive;
        }

        /// <summary>
        /// Converts entity to project type dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<string> ConvertEntityProjectTypeToDtoAsync(string source, bool bypassCache)
        {
            if(!string.IsNullOrEmpty(source))
            {
                var projectType = (await ProjectTypesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(source, StringComparison.OrdinalIgnoreCase));
                if(projectType == null)
                {
                    throw new KeyNotFoundException(string.Format("Project type not found for code {0}.", source));
                }
                return projectType.Description;
            }
            return null;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertPrincipalInvestigatorEntityToDtoAsync(List<string> source, string sourceGuid, string sourceKey)
        {
            if (source != null && source.Any())
            {
                foreach (var id in source)
                {
                    try
                    {
                        var isCorporation = await _personRepository.IsCorpAsync(id);
                        if (!isCorporation)
                        {
                            var guid = await _personRepository.GetPersonGuidFromIdAsync(id);
                            if (string.IsNullOrEmpty(guid))
                            {
                                throw new KeyNotFoundException(string.Format("Person not found for {0}.", source));
                            }
                            return new GuidObject2(guid);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("No GUID found for person ID {0}.", id), "Bad.Data", sourceGuid, sourceKey);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private IEnumerable<GrantsReportingPeriodProperty> ConvertEntityReportingPeriodsToDto(string guid, IEnumerable<ReportingPeriod> source)
        {
            List<GrantsReportingPeriodProperty> grantRepPeriods = new List<GrantsReportingPeriodProperty>();

            if (source != null && source.Any())
            {
                foreach (var item in source)
                {
                    if(!item.StartDate.HasValue)
                    {
                        throw new ArgumentException(string.Format("Start date is required for reporting period. Guid: {0}", guid));
                    }
                    if (!item.EndDate.HasValue)
                    {
                        throw new ArgumentException(string.Format("End date is required for reporting period. Guid: {0}", guid));
                    }

                    GrantsReportingPeriodProperty grantRepPeriod = new GrantsReportingPeriodProperty()
                    {
                        Start = item.StartDate.Value,
                        End = item.EndDate.Value
                    };
                    grantRepPeriods.Add(grantRepPeriod);
                }
            }
            return grantRepPeriods.Any() ? grantRepPeriods : null;
        }

       
        /// <summary>
        /// Project types.
        /// </summary>
        private IEnumerable<Domain.Base.Entities.ProjectType> _projectTypes;
        private async Task<IEnumerable<Domain.Base.Entities.ProjectType>> ProjectTypesAsync(bool bypassCache)
        {
            if(_projectTypes == null)
            {
                _projectTypes = await _referenceDataRepository.GetProjectTypesAsync();
                return _projectTypes;
            }

            return _projectTypes;
        }
    }
}