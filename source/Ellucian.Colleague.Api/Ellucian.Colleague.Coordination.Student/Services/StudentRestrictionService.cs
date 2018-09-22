// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentRestrictionService : StudentCoordinationService, IStudentRestrictionService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonRestrictionRepository _personRestrictionRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentRestrictionService(IAdapterRegistry adapterRegistry,
                                         IReferenceDataRepository referenceDataRepository,
                                         IStudentRepository studentRepository,
                                         IPersonRepository personRepository,
                                         IPersonRestrictionRepository personRestrictionRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentRepository = studentRepository;
            _personRepository = personRepository;
            _personRestrictionRepository = personRestrictionRepository;
        }

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsAsync(string studentId, bool useCache = true)
        {
            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();

            // In order to access the student restriction the person must be the student, be the student's advisor, or
            // have view student information permission.
            await CheckUserAccessAsync(studentId);

            DateTime today = DateTime.Today;
            IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = (await _personRestrictionRepository.GetAsync(studentId, useCache)).Where(sr => sr.OfficeUseOnly == false && (sr.StartDate.HasValue && sr.StartDate.Value <= today) && (sr.EndDate.HasValue == false || (sr.EndDate.HasValue && sr.EndDate.Value >= today)));
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

        /// <summary>
        /// Retrieves Student restrictions for the given student ID
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="useCache">Whether or not to use cached restrictions</param>
        /// <returns>Student's restrictions</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictions2Async(string studentId, bool useCache = true)
        {
            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();

            // In order to access the student restriction the person must be the student or have the view person restrictions permission
            CheckStudentRestrictionsAccess(studentId);

            DateTime today = DateTime.Today;
            IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = (await _personRestrictionRepository.GetAsync(studentId, useCache)).Where(sr => sr.OfficeUseOnly == false && (sr.StartDate.HasValue && sr.StartDate.Value <= today) && (sr.EndDate.HasValue == false || (sr.EndDate.HasValue && sr.EndDate.Value >= today)));
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

        /// <summary>
        /// Select restrictions for multiple students at once.  Security is based on Advisors (View Student Information)
        /// instead of from a Student user perspective therefore, we show all restrictions, not just non-office use only.
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <returns>List of DTO objects for Person Restrictions</returns>
        /// 
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsByStudentIdsAsync(IEnumerable<string> studentIds)
        {
            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();

            // In order to access the student restriction the person must be the student, be the student's advisor, or
            // have view student information permission.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {

                IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = await _personRestrictionRepository.GetRestrictionsByStudentIdsAsync(studentIds);
                IEnumerable<Restriction> restrictions = await _referenceDataRepository.RestrictionsAsync();

                if (restrictions == null || restrictions.Count() <= 0)
                    logger.Error("No restrictions retrieved from reference data repository.");

                // Get the right adapter for the type mapping
                var personRestrictionAdapter = new PersonRestrictionEntityAdapter(_adapterRegistry, logger);

                foreach (var personRestriction in personRestrictions)
                {
                    Restriction restriction = restrictions.Where(r => r.Code == personRestriction.RestrictionId).FirstOrDefault();
                    if (restriction != null)
                    {
                        var personRestrictionDto = personRestrictionAdapter.MapToType(personRestriction, restriction);
                        results.Add(personRestrictionDto);
                    }
                    else
                    {
                        logger.Error(string.Format("Restriction {0} not found for person restriction {1} and student {2}", personRestriction.RestrictionId, personRestriction.Id, personRestriction.StudentId));
                    }
                }
            }
            else
            {
                throw new PermissionsException("User does not have permissions to access  student restrictions.");
            }

            return results.AsEnumerable();
        }

        /// <summary>
        /// Select restrictions for multiple student restrictions at once.  Security is based on Advisors (View Student Information)
        /// instead of from a Student user perspective therefore, we show all restrictions, not just non-office use only.
        /// </summary>
        /// <param name="ids">List of Student Restrictions keys.</param>
        /// <returns>List of DTO objects for Person Restrictions</returns>
        /// 
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsByIdsAsync(IEnumerable<string> ids)
        {
            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();

            // In order to access the student restriction the person must be the student, be the student's advisor, or
            // have view student information permission.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {

                IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = await _personRestrictionRepository.GetRestrictionsByIdsAsync(ids);
                IEnumerable<Restriction> restrictions = await _referenceDataRepository.RestrictionsAsync();

                // Get the right adapter for the type mapping
                var personRestrictionAdapter = new PersonRestrictionEntityAdapter(_adapterRegistry, logger);

                foreach (var personRestriction in personRestrictions)
                {
                    Restriction restriction = restrictions.Where(r => r.Code == personRestriction.RestrictionId).First();
                    if (restriction != null)
                    {
                        var personRestrictionDto = personRestrictionAdapter.MapToType(personRestriction, restriction);
                        results.Add(personRestrictionDto);
                    }
                }
            }
            else
            {
                throw new PermissionsException("User does not have permissions to access  student restrictions.");
            }

            return results.AsEnumerable();
        }
    }
}
