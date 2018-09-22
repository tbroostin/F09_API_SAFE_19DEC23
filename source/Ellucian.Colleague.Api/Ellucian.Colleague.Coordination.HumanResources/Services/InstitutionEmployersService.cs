//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class InstitutionEmployersService : BaseCoordinationService, IInstitutionEmployersService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IInstitutionEmployersRepository _institutionEmployersRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private Dictionary<string, string> _employerGuidDictionary;

        public InstitutionEmployersService(
            IPositionRepository positionRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
             IReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            IInstitutionEmployersRepository institutionEmployersRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this._personRepository = personRepository;
            this._institutionEmployersRepository = institutionEmployersRepository;
            _configurationRepository = configurationRepository;

            _employerGuidDictionary = new Dictionary<string, string>();
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets institution employers
        /// </summary>
        /// <returns>Collection of InstitutionEmployers DTO objects</returns>
        public async Task<IEnumerable<Dtos.InstitutionEmployers>> GetInstitutionEmployersAsync()
        {
            try
            {
                var institutionEmployerCollection = new List<Ellucian.Colleague.Dtos.InstitutionEmployers>();
                var institutionEmployerEntities = await _institutionEmployersRepository.GetInstitutionEmployersAsync();
                if (institutionEmployerEntities != null && institutionEmployerEntities.Count() > 0)
                {                    
                    foreach (var institutionEmployer in institutionEmployerEntities)
                    {
                        var thisInstitutionEmployerDto = await (BuildInstitutionEmployer(institutionEmployer));
                        institutionEmployerCollection.Add(thisInstitutionEmployerDto);
                    }
                }
                return institutionEmployerCollection;
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstitutionEmployers from its GUID
        /// </summary>
        /// <returns>InstitutionEmployers DTO object</returns>
        public async Task<Dtos.InstitutionEmployers> GetInstitutionEmployersByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Employer.");
            }
            try
            {
                var institutionEmployerEntity = await _institutionEmployersRepository.GetInstitutionEmployerByGuidAsync(guid);
                var institutionEmployerDto = await (BuildInstitutionEmployer(institutionEmployerEntity));
                return institutionEmployerDto;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("institution-employers not found for GUID " + guid);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Build an institution employer DTO object from domain entity
        /// </summary>
        /// <returns>InstitutionEmployers DTO object</returns>
        private async Task<InstitutionEmployers> BuildInstitutionEmployer(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers institutionEmployer)
        {
            if (string.IsNullOrEmpty(institutionEmployer.Guid))
            {
                throw new ArgumentNullException("Guid is required");
            }
            if (string.IsNullOrEmpty(institutionEmployer.PreferredName))
            {
                throw new ArgumentNullException("Title is required for Employer " + institutionEmployer.EmployerId);
            }
            if (string.IsNullOrEmpty(institutionEmployer.City))
            {
                throw new ArgumentNullException("City is required for Employer " + institutionEmployer.EmployerId);
            }
            if (string.IsNullOrEmpty(institutionEmployer.State))
            {
                throw new ArgumentNullException("State is required for Employer " + institutionEmployer.EmployerId);
            }
            if (string.IsNullOrEmpty(institutionEmployer.Country))
            {
                throw new ArgumentNullException("Country is required for Employer " + institutionEmployer.EmployerId);
            }
            if (string.IsNullOrEmpty(institutionEmployer.PostalCode))
            {
                throw new ArgumentNullException("Postal Code is required for Employer " + institutionEmployer.EmployerId);
            }
            if (institutionEmployer.AddressLines == null || institutionEmployer.AddressLines.Count() <= 0)
            {
                throw new ArgumentNullException("Address Lines are required for Employer " + institutionEmployer.EmployerId);
            }
            var thisInstitutionEmployerDto = new Ellucian.Colleague.Dtos.InstitutionEmployers();
            thisInstitutionEmployerDto.Id = institutionEmployer.Guid;
            thisInstitutionEmployerDto.Title = institutionEmployer.PreferredName;
            thisInstitutionEmployerDto.Code = institutionEmployer.Code;
            var address = new Dtos.DtoProperties.InstitutionEmployersAddress();
            address.AddressLines = institutionEmployer.AddressLines;
            address.City = institutionEmployer.City;
            address.State = institutionEmployer.State;
            address.Country = institutionEmployer.Country;
            address.PostalCode = institutionEmployer.PostalCode;
            thisInstitutionEmployerDto.Address = address;
            thisInstitutionEmployerDto.PhoneNumber = institutionEmployer.PhoneNumber;

            return thisInstitutionEmployerDto;
        }
    }
}