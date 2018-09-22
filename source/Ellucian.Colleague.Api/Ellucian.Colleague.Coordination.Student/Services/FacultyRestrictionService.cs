// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Base.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class FacultyRestrictionService : StudentCoordinationService, IFacultyRestrictionService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IPersonRestrictionRepository _personRestrictionRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public FacultyRestrictionService(IAdapterRegistry adapterRegistry,
                                         IReferenceDataRepository referenceDataRepository,
                                         IFacultyRepository facultyRepository,
                                         IPersonRestrictionRepository personRestrictionRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IStudentRepository studentRepository,
                                         IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _facultyRepository = facultyRepository;
            _personRestrictionRepository = personRestrictionRepository;
        }

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetFacultyRestrictionsAsync(string facultyId)
        {
            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();
            Domain.Student.Entities.Faculty faculty = await _facultyRepository.GetAsync(facultyId);

            if (faculty == null)
            {
                throw new ApplicationException("Faculty not found in repository");
            }

            CheckUserAccess(faculty);


            DateTime today = DateTime.Now;
            IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = (await _personRestrictionRepository.GetAsync(facultyId)).Where(sr => sr.OfficeUseOnly == false && (sr.StartDate.HasValue && sr.StartDate.Value <= today) && (sr.EndDate.HasValue == false || (sr.EndDate.HasValue && sr.EndDate.Value > today)));
            IEnumerable<Restriction> restrictions = await _referenceDataRepository.RestrictionsAsync();

            // Get the right adapter for the type mapping
            var personRestrictionAdapter = new PersonRestrictionEntityAdapter(_adapterRegistry, logger);

            foreach (var personRestriction in personRestrictions)
            {
                Restriction restriction = restrictions.Where(r => r.Code == personRestriction.RestrictionId).First();
                if (restriction != null && restriction.OfficeUseOnly == false)
                {
                    var personRestrictionDto = personRestrictionAdapter.MapToType(personRestriction, restriction);
                    results.Add(personRestrictionDto);
                }
            }

            return results.AsEnumerable();
        }

        public void CheckUserAccess(Domain.Student.Entities.Faculty faculty)
        {
            if (!UserIsSelf(faculty.Id))
            {
                throw new PermissionsException("User does not have permissions to access this faculty");
            }
        }

    }
}
