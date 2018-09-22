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
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PersonBenefitDependentsService : BaseCoordinationService, IPersonBenefitDependentsService
    {

        private readonly IPersonBenefitDependentsRepository _personBenefitDependentsRepository;

        public PersonBenefitDependentsService(

            IPersonBenefitDependentsRepository personBenefitDependentsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _personBenefitDependentsRepository = personBenefitDependentsRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-benefit-dependents
        /// </summary>
        /// <returns>Collection of PersonBenefitDependents DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonBenefitDependents>, int>> GetPersonBenefitDependentsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckGetViewPersonBenefitDependentsPermission();

            var personBenefitDependentsCollection = new List<Ellucian.Colleague.Dtos.PersonBenefitDependents>();

            var pageOfItems = await _personBenefitDependentsRepository.GetPersonBenefitDependentsAsync(offset, limit, bypassCache);

            var personBenefitDependentsEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (personBenefitDependentsEntities != null && personBenefitDependentsEntities.Any())
            {
                foreach (var personBenefitDependents in personBenefitDependentsEntities)
                {
                    personBenefitDependentsCollection.Add(await ConvertPersonBenefitDependentsEntityToDto(personBenefitDependents));
                }
            }

            return new Tuple<IEnumerable<Dtos.PersonBenefitDependents>, int>(personBenefitDependentsCollection, totalRecords);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonBenefitDependents from its GUID
        /// </summary>
        /// <returns>PersonBenefitDependents DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonBenefitDependents> GetPersonBenefitDependentsByGuidAsync(string guid)
        {
            try
            {
                CheckGetViewPersonBenefitDependentsPermission();

                return await ConvertPersonBenefitDependentsEntityToDto(await _personBenefitDependentsRepository.GetPersonBenefitDependentByIdAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("person-benefit-dependents not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("person-benefit-dependents not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Perben domain entity to its corresponding PersonBenefitDependents DTO
        /// </summary>
        /// <param name="source">Perben domain entity</param>
        /// <returns>PersonBenefitDependents DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonBenefitDependents> ConvertPersonBenefitDependentsEntityToDto(PersonBenefitDependent source)
        {
            var personBenefitDependents = new Ellucian.Colleague.Dtos.PersonBenefitDependents();

            personBenefitDependents.Id = source.Guid;
			var deductionGuid = await _personBenefitDependentsRepository.GetGuidFromIdAsync(source.DeductionArrangement, "PERBEN");
            personBenefitDependents.DeductionArrangement = new GuidObject2(deductionGuid);
			var personGuid = await _personBenefitDependentsRepository.GetGuidFromIdAsync(source.DependentPersonId, "PERSON");
            personBenefitDependents.Dependent = new Dtos.DtoProperties.PersonBenefitDependentsDependentDtoProperty() { Person = new GuidObject2(personGuid) };
            personBenefitDependents.ProviderName = source.ProviderName;
            personBenefitDependents.ProviderIdentification = source.ProviderIdentification;
            personBenefitDependents.CoverageStartOn = source.CoverageStartOn;
           	personBenefitDependents.CoverageEndOn = source.CoverageEndOn;
			personBenefitDependents.StudentStatus = !string.IsNullOrEmpty(source.StudentStatus) && source.StudentStatus == "Y" ? PersonBenefitDependentsStudentStatus.Fulltime : PersonBenefitDependentsStudentStatus.Notstudent;
                                                                                                                                    
            return personBenefitDependents;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view PersonBenefitDependents.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetViewPersonBenefitDependentsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewDependents);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Person Benefit Dependents.");
            }
        }
      
    }
   
}