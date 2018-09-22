// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for accessing nonacademic attendance data
    /// </summary>
    [RegisterType]
    public class NonAcademicAttendanceService : StudentCoordinationService, INonAcademicAttendanceService
    {
        private readonly INonAcademicAttendanceRepository _nonAcademicAttendanceRepository;

        public NonAcademicAttendanceService(IAdapterRegistry adapterRegistry,
            INonAcademicAttendanceRepository nonAcademicAttendanceRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStudentRepository studentRepository,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            this._nonAcademicAttendanceRepository = nonAcademicAttendanceRepository;
        }


        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        public async Task<IEnumerable<NonAcademicAttendanceRequirement>> GetNonAcademicAttendanceRequirementsAsync(string personId)
        {
            logger.Info("Entering NonAcademicAttendanceService.GetNonAcademicAttendanceRequirementsAsync...");
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A unique identifier for a person must be specified in order to retrieve nonacademic attendance requirements.");
            }
            if (!UserIsSelf(personId))
            {
                var message = string.Format("User {0} does not have permission to access nonacademic attendance data for person {1}", CurrentUser.PersonId, personId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            logger.Info("Calling NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync...");
            var nonAcademicAttendanceRequirements = await _nonAcademicAttendanceRepository.GetNonacademicAttendanceRequirementsAsync(personId);
            logger.Info("Returned from NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync.");

            var nonAcademicAttendanceRequirementDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.NonAcademicAttendanceRequirement, Dtos.Student.NonAcademicAttendanceRequirement>();
            if (nonAcademicAttendanceRequirementDtoAdapter == null)
            {
                string errorMessage = "Could not get NonAcademicAttendanceRequirement entity-to-DTO adapter.";
                logger.Debug(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            List<NonAcademicAttendanceRequirement> nonAcademicAttendanceRequirementDtos = new List<NonAcademicAttendanceRequirement>();
            if (nonAcademicAttendanceRequirements != null)
            {
                logger.Debug(String.Format("Call to NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync returned {0} items; converting to DTOs...", nonAcademicAttendanceRequirements.Count()));
                foreach (var entity in nonAcademicAttendanceRequirements)
                {
                    nonAcademicAttendanceRequirementDtos.Add(nonAcademicAttendanceRequirementDtoAdapter.MapToType(entity));
                }
                logger.Debug(String.Format("Converted {0} entities to DTOs in NonAcademicAttendanceService.GetNonAcademicAttendanceRequirementsAsync.",
                    nonAcademicAttendanceRequirementDtos.Count));
            }
            else
            {
                logger.Debug("Call to NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync returned null.");
            }
            logger.Info("Leaving NonAcademicAttendanceService.GetNonAcademicAttendanceRequirementsAsync...");
            return nonAcademicAttendanceRequirementDtos;
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person</returns>
        public async Task<IEnumerable<NonAcademicAttendance>> GetNonAcademicAttendancesAsync(string personId)
        {
            logger.Info("Entering NonAcademicAttendanceService.GetNonAcademicAttendancesAsync...");
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A unique identifier for a person must be specified in order to retrieve nonacademic events attended.");
            }
            if (!UserIsSelf(personId))
            {
                var message = string.Format("User {0} does not have permission to access nonacademic attendance data for person {1}", CurrentUser.PersonId, personId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            logger.Info("Calling NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync...");
            var nonAcademicAttendances = await _nonAcademicAttendanceRepository.GetNonacademicAttendancesAsync(personId);
            logger.Info("Returned from NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync.");

            var nonAcademicAttendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.NonAcademicAttendance, Dtos.Student.NonAcademicAttendance>();
            if (nonAcademicAttendanceDtoAdapter == null)
            {
                string errorMessage = "Could not get NonAcademicAttendance entity-to-DTO adapter.";
                logger.Debug(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            List<NonAcademicAttendance> nonAcademicAttendanceDtos = new List<NonAcademicAttendance>();
            if (nonAcademicAttendances != null)
            {
                logger.Debug(String.Format("Call to NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync returned {0} items; converting to DTOs...", nonAcademicAttendances.Count()));
                foreach (var entity in nonAcademicAttendances)
                {
                    nonAcademicAttendanceDtos.Add(nonAcademicAttendanceDtoAdapter.MapToType(entity));
                }
                logger.Debug(String.Format("Converted {0} entities to DTOs in NonAcademicAttendanceService.GetNonAcademicAttendancesAsync.",
                    nonAcademicAttendanceDtos.Count));
            }
            else
            {
                logger.Debug("Call to NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync returned null.");
            }
            logger.Info("Leaving NonAcademicAttendanceService.GetNonAcademicAttendancesAsync...");
            return nonAcademicAttendanceDtos;
        }

    }
}
