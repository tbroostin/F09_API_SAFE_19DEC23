/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Base;
using System;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class OrganizationalChartService : BaseCoordinationService, IOrganizationalChartService
    {
        private readonly IOrganizationalChartDomainService _organizationalChartDomainService;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;

        public OrganizationalChartService(
            IOrganizationalChartDomainService organizationalChartDomainService,
            IPersonBaseRepository personBaseRepository,
            IEmployeeRepository employeeRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _organizationalChartDomainService = organizationalChartDomainService;
            _personBaseRepository = personBaseRepository;
            _employeeRepository = employeeRepository;
            _hrReferenceDataRepository = hrReferenceDataRepository;
        }

        /// <summary>
        /// Returns OrgChartEmployee objects
        /// </summary>
        /// <exception cref="PermissionsException">Authenticated user must be a supervisor or have access to requsted summary objects</exception>
        /// <returns>A collection of OrgChartEmployee dtos</returns>
        public async Task<IEnumerable<Dtos.HumanResources.OrgChartEmployee>> GetOrganizationalChartAsync(string rootEmployeeId)
        {
            if (!HasPermission(HumanResourcesPermissionCodes.ViewOrgChart))
            {
                throw new PermissionsException(string.Format("User does not have permission to access to view organzational chart data"));
            }

            var orgChartEmployeeEntities = await _organizationalChartDomainService.GetOrganizationalChartEmployeesAsync(rootEmployeeId);
            
            List<Dtos.HumanResources.OrgChartEmployee> orgChartEmployeesDtos = new List<Dtos.HumanResources.OrgChartEmployee>();
            var orgChartEmployeeEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.OrgChartEmployee, Dtos.HumanResources.OrgChartEmployee>();
            foreach(var orgChartEmployeeEntity in orgChartEmployeeEntities)
            {
                orgChartEmployeesDtos.Add(orgChartEmployeeEntityToDtoAdapter.MapToType(orgChartEmployeeEntity));
            }
            return orgChartEmployeesDtos;
        }

        /// <summary>
        /// Returns OrgChartEmployee object
        /// </summary>
        /// <exception cref="PermissionsException">Authenticated user must be a supervisor or have access to requsted summary objects</exception>
        /// <returns>A single OrgChartEmployee dtos</returns>
        public async Task<Dtos.HumanResources.OrgChartEmployee> GetOrganizationalChartEmployeeAsync(string rootEmployeeId)
        {
            if (!HasPermission(HumanResourcesPermissionCodes.ViewOrgChart))
            {
                throw new PermissionsException(string.Format("User does not have permission to access to view organzational chart data"));
            }

            var orgChartEmployeeEntity = await _organizationalChartDomainService.GetOrganizationalChartEmployeeAsync(rootEmployeeId);
            var orgChartEmployeeEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.OrgChartEmployee, Dtos.HumanResources.OrgChartEmployee>();
            var orgChartEmployeesDto = orgChartEmployeeEntityToDtoAdapter.MapToType(orgChartEmployeeEntity);

            return orgChartEmployeesDto;
        }

        /// <summary>
        /// Gets a list of employees matching the given search criteria.
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria</param>
        /// <returns>A list of <see cref="EmployeeSearchResult"> objects.</see></returns>
        public async Task<IEnumerable<EmployeeSearchResult>> QueryEmployeesByPostAsync(EmployeeNameQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria must be provided to search for persons.");
            }
            // add PersonalPronounCode

            var personBases = await _personBaseRepository.SearchByIdsOrNamesAsync(criteria.Ids, criteria.QueryKeyword);

            var employeeKeys = await _employeeRepository.GetEmployeeKeysAsync(includeNonEmployees: criteria.IncludeNonEmployees, activeOnly: criteria.ActiveOnly);

            var employeePersonBases = personBases.Where(pb => employeeKeys.Any(id => id == pb.Id));

            var results = new List<EmployeeSearchResult>();

            if (employeePersonBases != null && employeePersonBases.Any())
            {
                var hrssConfiguration = await _hrReferenceDataRepository.GetHrssConfigurationAsync();

                if (hrssConfiguration != null && !string.IsNullOrWhiteSpace(hrssConfiguration.HrssDisplayNameHierarchy))
                {
                    NameAddressHierarchy nameHierarchy = null;
                    try
                    {
                        nameHierarchy = await _personBaseRepository.GetCachedNameAddressHierarchyAsync(hrssConfiguration.HrssDisplayNameHierarchy);
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, string.Format("Unable to find name address hierarchy with ID {0}. Not calculating hierarchy name.", nameHierarchy));

                    }
                    if (nameHierarchy != null)
                    {
                        employeePersonBases = GetPersonHierarchyName(employeePersonBases, nameHierarchy);
                    }
                }

                var personBaseEntityToEmployeeSearchResultDtoAdapter = _adapterRegistry.GetAdapter<PersonBase, EmployeeSearchResult>();

                results.AddRange(employeePersonBases.Select(pb => personBaseEntityToEmployeeSearchResultDtoAdapter.MapToType(pb)));
            }
            return results;
        }

        /// <summary>
        /// Add Person display name to each person base entity in list based on name heirarchy 
        /// </summary>
        /// <param name="personBaseList">contains list of person base enity</param>
        /// <returns></returns>
        private IEnumerable<PersonBase> GetPersonHierarchyName(IEnumerable<PersonBase> personBaseList, NameAddressHierarchy nameHierarchy)
        {
            if (personBaseList != null && personBaseList.Any())
            {
                foreach (var personBase in personBaseList)
                {
                    personBase.PersonDisplayName = PersonNameService.GetHierarchyName(personBase, nameHierarchy);
                }
            }
            return personBaseList;
        }
    }
}