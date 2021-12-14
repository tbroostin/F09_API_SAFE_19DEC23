// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Portal;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Portal Web Part Service layer
    /// </summary>
    [RegisterType]
    public class PortalService : BaseCoordinationService, IPortalService
    {
        private readonly IPortalRepository _portalRepository;

        public PortalService(IAdapterRegistry adapterRegistry, IPortalRepository portalRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _portalRepository = portalRepository;
        }

        /// <summary>
        /// This returns the list of course ids that are applicable for deletion from the Portal. 
        /// </summary>
        public async Task<Dtos.Student.Portal.PortalDeletedCoursesResult> GetCoursesForDeletionAsync()
        {
            PortalDeletedCoursesResult portalDeletedCoursesDto = null;
            try
            {
                CheckUserPortalPermission();
                Domain.Student.Entities.Portal.PortalDeletedCoursesResult portalDeletedCoursesResultEntity = await _portalRepository.GetCoursesForDeletionAsync();
                if (portalDeletedCoursesResultEntity != null)
                {
                    var portalDeletedCoursesEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Portal.PortalDeletedCoursesResult, Dtos.Student.Portal.PortalDeletedCoursesResult>();
                    portalDeletedCoursesDto = portalDeletedCoursesEntityToDtoAdapter.MapToType(portalDeletedCoursesResultEntity);
                }
                else
                {
                    logger.Warn("Portal call to retrieve courses for deletion from repository returned null entity");
                }
                return portalDeletedCoursesDto;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, "User does not have appropriate permissions to retrieve deleted courses for Portal");
                throw;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while retrieving deleted courses for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving deleted courses for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// This returns the list of sections that are applicable for update from the Portal. 
        /// </summary>
        public async Task<Dtos.Student.Portal.PortalUpdatedSectionsResult> GetSectionsForUpdateAsync()
        {
            PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = null;
            try
            {
                CheckUserPortalPermission();
                var portalUpdatedSectionsResultEntity = await _portalRepository.GetSectionsForUpdateAsync();
                if (portalUpdatedSectionsResultEntity != null)
                {
                    var portalUpdatedSectionsResultEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Portal.PortalUpdatedSectionsResult, Dtos.Student.Portal.PortalUpdatedSectionsResult>();
                    portalUpdatedSectionsResultDto = portalUpdatedSectionsResultEntityToDtoAdapter.MapToType(portalUpdatedSectionsResultEntity);
                }
                else
                {
                    logger.Warn("Portal call to retrieve updated sections result from repository returned null entity");
                }
                return portalUpdatedSectionsResultDto;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, "User does not have appropriate permissions to retrieve updated sections for Portal");
                throw;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while retrieving updated sections for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving updated sections for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// This returns the list of course section ids that are applicable for deletion from the Portal. 
        /// </summary>
        public async Task<Dtos.Student.Portal.PortalDeletedSectionsResult> GetSectionsForDeletionAsync()
        {
            PortalDeletedSectionsResult portalDeletedSectionsDto = null;
            try
            {
                CheckUserPortalPermission();
                var portalDeletedSectionsResultEntity = await _portalRepository.GetSectionsForDeletionAsync();
                if (portalDeletedSectionsResultEntity != null)
                {
                    var portalDeletedSectionsEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Portal.PortalDeletedSectionsResult, Dtos.Student.Portal.PortalDeletedSectionsResult>();
                    portalDeletedSectionsDto = portalDeletedSectionsEntityToDtoAdapter.MapToType(portalDeletedSectionsResultEntity);
                }
                else
                {
                    logger.Warn("Portal call to retrieve sections for deletion from repository returned null entity");
                }
                return portalDeletedSectionsDto;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, "User does not have appropriate permissions to retrieve deleted sections for Portal");
                throw;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while retrieving deleted sections for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving deleted sections for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// Returns event and reminders to be displayed in the Portal for the authenticated user.
        /// </summary>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <returns>A <see cref="PortalEventsAndReminders"/> object</returns>
        public async Task<Dtos.Student.Portal.PortalEventsAndReminders> GetEventsAndRemindersAsync(PortalEventsAndRemindersQueryCriteria criteria)
        {
            Dtos.Student.Portal.PortalEventsAndReminders portalEventsAndReminders = new PortalEventsAndReminders();
            Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria criteriaEntity;
            if (criteria == null)
            {
                criteriaEntity = new Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria();
            }
            else
            {
                var portalEventsAndRemindersQueryCriteriaDtoToEntityAdapter = new PortalEventsAndRemindersQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
                criteriaEntity = portalEventsAndRemindersQueryCriteriaDtoToEntityAdapter.MapToType(criteria);
            }
            try
            {
                var portalEventsAndRemindersEntity = await _portalRepository.GetEventsAndRemindersAsync(CurrentUser.PersonId, criteriaEntity);
                if (portalEventsAndRemindersEntity != null)
                {
                    var portalEventsAndRemindersEntityToDtoAdapter = new PortalEventsAndRemindersEntityAdapter(_adapterRegistry, logger);
                    portalEventsAndReminders = portalEventsAndRemindersEntityToDtoAdapter.MapToType(portalEventsAndRemindersEntity);
                }
                else
                {
                    logger.Error(string.Format("Portal call to retrieve Portal events and reminders for user {0} from repository returned null entity.", CurrentUser.PersonId));
                }
                return portalEventsAndReminders;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while retrieving Portal events and reminders at service layer.";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving Portal events and reminders at the coordination service layer.";
                logger.Error(ex, error);
                throw;
            }

        }


        /// <summary>
        /// This returns the courses that are applicable for update from the Portal. 
        /// </summary>
        public async Task<Dtos.Student.Portal.PortalUpdatedCoursesResult> GetCoursesForUpdateAsync()
        {
            PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = null;
            try
            {
                CheckUserPortalPermission();
                var portalUpdatedCoursesResultEntity = await _portalRepository.GetCoursesForUpdateAsync();
                if (portalUpdatedCoursesResultEntity != null)
                {
                    var portalUpdatedCoursesResultEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Portal.PortalUpdatedCoursesResult, Dtos.Student.Portal.PortalUpdatedCoursesResult>();
                    portalUpdatedCoursesResultDto = portalUpdatedCoursesResultEntityToDtoAdapter.MapToType(portalUpdatedCoursesResultEntity);
                }
                else
                {
                    logger.Warn("Portal call to retrieve updated courses result from repository returned null entity");
                }
                return portalUpdatedCoursesResultDto;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, "User does not have appropriate permissions to retrieve updated courses for Portal");
                throw;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while retrieving updated courses for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving updated courses for Portal at service layer";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        public async Task<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>> UpdateStudentPreferredCourseSectionsAsync(string studentId, IEnumerable<string> courseSectionIds)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student ID is required when updating a student's list of preferred course sections.");
            }
            if (courseSectionIds == null || !courseSectionIds.Any(id => !string.IsNullOrWhiteSpace(id)))
            {
                throw new ArgumentNullException("At least one course section ID is required when updating a student's list of preferred course sections.");
            }
            List<PortalStudentPreferredCourseSectionUpdateResult> results = new List<PortalStudentPreferredCourseSectionUpdateResult>();
            try
            {
                CheckIfUserIsSelf(studentId);
                var resultEntities = await _portalRepository.UpdateStudentPreferredCourseSectionsAsync(studentId, courseSectionIds);
                if (resultEntities != null)
                {
                    var dtoAdapter = new PortalStudentPreferredCourseSectionUpdateResultDtoAdapter(_adapterRegistry, logger);
                    foreach(var result in resultEntities)
                    {
                        results.Add(dtoAdapter.MapToType(result));
                    }
                }
                else
                {
                    logger.Warn("Portal call to update student's preferred course sections returned null.");
                }
                return results;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, "Users may only update their own preferred course sections.");
                throw;
            }
            catch (RepositoryException ex)
            {
                string error = "Repository Exception occurred while updating student's preferred course sections.";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while updating student's preferred course sections at service layer.";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// Verify the current user has permission to sync courses or sections that are applicable for deletion or updates.
        /// </summary>
        /// <param name="id">The person id sending the request. </param>
        private void CheckUserPortalPermission()
        {
            // They're allowed to see data if they are the logged in user with PORTAL.CATALOG.ADMIN permission
            if (!HasPermission(StudentPermissionCodes.PortalCatalogAdmin))
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code PORTAL.CATALOG.ADMIN");
                throw new PermissionsException();
            }
        }
    }
}