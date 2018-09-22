/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmploymentProficiencyService : BaseCoordinationService, IEmploymentProficiencyService
    {

        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public EmploymentProficiencyService(

            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {

            _hrReferenceDataRepository = hrReferenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employee proficiencies
        /// </summary>
        /// <returns>Collection of EmployeeProficiencies DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentProficiency>> GetEmploymentProficienciesAsync(bool bypassCache = false)
        {
            var employeeProficiencyCollection = new List<Ellucian.Colleague.Dtos.EmploymentProficiency>();

            var employeeProficiencyEntities = await _hrReferenceDataRepository.GetEmploymentProficienciesAsync(bypassCache);
            if (employeeProficiencyEntities != null && employeeProficiencyEntities.Any())
            {
                foreach (var employeeProficiency in employeeProficiencyEntities)
                {
                    employeeProficiencyCollection.Add(ConvertEmployeeProficiencyEntityToDto(employeeProficiency));
                }
            }
            return employeeProficiencyCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a employee proficiency from its GUID
        /// </summary>
        /// <returns>EmployeeProficiency DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentProficiency> GetEmploymentProficiencyByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmployeeProficiencyEntityToDto((await _hrReferenceDataRepository.GetEmploymentProficienciesAsync(true)).Where(r => r.Guid == guid).FirstOrDefault());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Employee proficiency not found for GUID " + guid, ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new KeyNotFoundException("Employee proficiency not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Employee proficiency not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmployeeProficiency domain entity to its corresponding EmployeeProficiency DTO
        /// </summary>
        /// <param name="source">EmployeeProficiency domain entity</param>
        /// <returns>EmployeeProficiency DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentProficiency ConvertEmployeeProficiencyEntityToDto(EmploymentProficiency source)
        {
            var employeeProficiency = new Ellucian.Colleague.Dtos.EmploymentProficiency();

            employeeProficiency.Id = source.Guid;
            employeeProficiency.Code = source.Code;
            employeeProficiency.Title = source.Description;
            //employeeProficiency.Description = null;
            employeeProficiency.Description = !string.IsNullOrEmpty(source.Comment) ? source.Comment : null;
            employeeProficiency.employeeProficiencyType = ConvertEmployeeProficiencyTypeDomainEnumToEmployeeProficiencyTypeDtoEnum(source.Certification);
            employeeProficiency.employeeProficiencyLicensing = ConvertEmployeeProficiencyLicensingDomainEnumToEmployeeProficiencyLicensingDtoEnum(source.Certification);
            employeeProficiency.employmentProficiencyLicensingAuthority =
                !string.IsNullOrEmpty(source.Authority) ?
                new Dtos.DtoProperties.EmploymentProficiencyLicensingAuthorityDtoProperty() 
                { name = source.Authority } : null;
            return employeeProficiency;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmployeeProficiencyType domain enumeration value to its corresponding EmployeeProficiencyType DTO enumeration value
        /// </summary>
        /// <param name="source">EmployeeProficiencyType domain enumeration value</param>
        /// <returns>EmployeeProficiencyType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EmploymentProficiencyType ConvertEmployeeProficiencyTypeDomainEnumToEmployeeProficiencyTypeDtoEnum(string source)
        {
            switch (source)
            {
                case "Y":
                    return Dtos.EmploymentProficiencyType.Certification;
                default:
                    return Dtos.EmploymentProficiencyType.Skill;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmployeeProficiencyLicensing domain enumeration value to its corresponding EmployeeProficiencyType DTO enumeration value
        /// </summary>
        /// <param name="source">EmployeeProficiencyLicensing domain enumeration value</param>
        /// <returns>EmployeeProficiencyLicensing DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EmploymentProficiencyLicensing ConvertEmployeeProficiencyLicensingDomainEnumToEmployeeProficiencyLicensingDtoEnum(string source)
        {
            switch (source)
            {
                case "Y":
                    return Dtos.EmploymentProficiencyLicensing.Required;
                default:
                    return Dtos.EmploymentProficiencyLicensing.NotRequired;
            }
        }

    }
}