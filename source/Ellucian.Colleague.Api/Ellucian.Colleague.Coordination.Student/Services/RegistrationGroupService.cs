// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class RegistrationGroupService : BaseCoordinationService, IRegistrationGroupService
    {
        private readonly IRegistrationGroupRepository _registrationGroupRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ITermRepository _termRepository;
        private ILogger _logger;

        public RegistrationGroupService(IAdapterRegistry adapterRegistry, IRegistrationGroupRepository registrationGroupRepository, ISectionRepository sectionRepository, ITermRepository termRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _registrationGroupRepository = registrationGroupRepository;
            _sectionRepository = sectionRepository;
            _termRepository = termRepository;
            _logger = logger;
        }

        /// <summary>
        /// Returns the section registration date overrides specific to the registration group for any of the requested section Ids. 
        /// </summary>
        /// <param name="sectionIds">Section Ids for which override dates are requested</param>
        /// <param name="considerUsersGroup">This is set to true if the registration group should be considered for dates calculation, otherwise set to false</param>
        /// <returns>Any SectionRegistrationDate items applicable to the sections requested. </returns>
        public async Task<IEnumerable<Dtos.Student.SectionRegistrationDate>> GetSectionRegistrationDatesAsync(IEnumerable<string> sectionIds, bool considerUsersGroup = true)
        {
            if (sectionIds == null || sectionIds.Count() == 0)
            {
                throw new ArgumentNullException("sectionIds", "Must provide at least one section id.");
            }

            // Need to get this person's registration group Id.
            string registrationGroupId = await _registrationGroupRepository.GetRegistrationGroupIdAsync(CurrentUser.PersonId);
            if (string.IsNullOrEmpty(registrationGroupId))
            {
                throw new ApplicationException("Unable to determine the registration group Id for this person.");
            }
            else
            {
                logger.Info(string.Format("User {0} is in registration group {1}.", CurrentUser.PersonId, registrationGroupId));
            }
            // Get the registration group for this person
            var registrationGroup = await _registrationGroupRepository.GetRegistrationGroupAsync(registrationGroupId);
            if (registrationGroup == null)
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("Unable to get registration group  " + registrationGroupId);
                }
                throw new ApplicationException("Unable to retrieve registration group " + registrationGroupId);
            }
            // Get cached section information for the desired sections
            var sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
            if (sections == null || sections.Count() == 0)
            {
                // None of the sections are in a term open for registration and cached so need to do further checking.
                return new List<Dtos.Student.SectionRegistrationDate>();
            }
            // Get Registration Term information
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();

            // We now have all the necessary inforamtion - call domain service to calculate the correct dates for each provided section.
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate> sectionRegistrationDatesEntities = SectionProcessor.GetSectionRegistrationDates(registrationGroup, sections, registrationTerms, considerUsersGroup);

            // For any overrides retrieved we now have to convert them into DTOs and return. 

            List<Dtos.Student.SectionRegistrationDate> sectionRegistrationDatesDtos = new List<Dtos.Student.SectionRegistrationDate>();
            var SectionRegistrationDatesEntityToDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate, Dtos.Student.SectionRegistrationDate>();
            foreach (var sectionRegistrationDates in sectionRegistrationDatesEntities)
            {
                Dtos.Student.SectionRegistrationDate sectionRegistrationDatesDto = SectionRegistrationDatesEntityToDtoAdapter.MapToType(sectionRegistrationDates);
                sectionRegistrationDatesDtos.Add(sectionRegistrationDatesDto);
            }
            return sectionRegistrationDatesDtos;
        }
    }
}
