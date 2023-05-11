// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Web.Dependency;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class OfficeHoursService : StudentCoordinationService, IOfficeHoursService
    {
        private readonly IOfficeHoursRepository _addOfficeHoursRepository;
        private readonly IFacultyRepository _facultyRepository;

        /// <summary>
        /// Constructor for OfficeHoursService
        /// </summary>
        /// <param name="addOfficeHoursRepository"></param>
        /// <param name="facultyRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public OfficeHoursService(IOfficeHoursRepository addOfficeHoursRepository, IFacultyRepository facultyRepository, IStudentRepository studentRepository,
            IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry,ICurrentUserFactory currentUserFactory, 
            IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _addOfficeHoursRepository = addOfficeHoursRepository;
            _facultyRepository = facultyRepository;
        }

        /// <summary>
        /// Add office hours information for faculty
        /// </summary>
        /// <param name="addOfficeHours">The AddOfficeHours to create</param>
        /// <returns>Added OfficeHours</returns>
        public async Task<Dtos.Student.AddOfficeHours> AddOfficeHoursAsync(Dtos.Student.AddOfficeHours addOfficeHours)
        {
            if (addOfficeHours == null)
            {
                throw new ArgumentNullException("addOfficeHours", "Add office hours must be provided to create a new office hours.");
            }

            await AuthorizeUser(addOfficeHours.Id);

            var addOfficeHoursEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.AddOfficeHours, AddOfficeHours>();
            var addOfficeHoursEntity = addOfficeHoursEntityAdapter.MapToType(addOfficeHours);
                        
            try
            {
                var newAddOfficeHours = await _addOfficeHoursRepository.AddOfficeHoursAsync(addOfficeHoursEntity);
                var addOfficeHoursDtoAdapter = _adapterRegistry.GetAdapter<AddOfficeHours, Dtos.Student.AddOfficeHours>();
                Dtos.Student.AddOfficeHours addOfficeHoursDto = addOfficeHoursDtoAdapter.MapToType(newAddOfficeHours);
                return addOfficeHoursDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for newly add office hours";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while adding office hours";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to create office Hours ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Update office hours information for a faculty
        /// </summary>
        /// <param name="UpdateOfficeHours">The office hours information to update</param>
        /// <returns>Updated OfficeHours</returns>
        public async Task<Dtos.Student.UpdateOfficeHours> UpdateOfficeHoursAsync(Dtos.Student.UpdateOfficeHours updateOfficeHours)
        {
            if (updateOfficeHours == null)
            {
                throw new ArgumentNullException("updateOfficeHours", "Office hours item must be provided to update.");
            }

            await AuthorizeUser(updateOfficeHours.Id);

            var updateOfficeHoursEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.UpdateOfficeHours, UpdateOfficeHours>();
            var updateOfficeHoursEntity = updateOfficeHoursEntityAdapter.MapToType(updateOfficeHours);

            try
            {
                var updatedAddOfficeHours = await _addOfficeHoursRepository.UpdateOfficeHoursAsync(updateOfficeHoursEntity);
                var updateOfficeHoursDtoAdapter = _adapterRegistry.GetAdapter<UpdateOfficeHours, Dtos.Student.UpdateOfficeHours>();
                Dtos.Student.UpdateOfficeHours updateOfficeHoursDto = updateOfficeHoursDtoAdapter.MapToType(updatedAddOfficeHours);
                return updateOfficeHoursDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for updated";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while updating office hours";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update office Hours ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// delete office hours information for a faculty
        /// </summary>
        /// <param name="deleteOfficeHours">The office hours information to delete</param>
        /// <returns>Deleted OfficeHours</returns>
        public async Task<Dtos.Student.DeleteOfficeHours> DeleteOfficeHoursAsync(Dtos.Student.DeleteOfficeHours deleteOfficeHours)
        {
            if (deleteOfficeHours == null)
            {
                throw new ArgumentNullException("deleteOfficeHours", "Office hours item must be provided to delete.");
            }

            await AuthorizeUser(deleteOfficeHours.Id);

            var deleteOfficeHoursEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DeleteOfficeHours, DeleteOfficeHours>();
            var deleteOfficeHoursEntity = deleteOfficeHoursEntityAdapter.MapToType(deleteOfficeHours);
                        
            try
            {
                var deletedAddOfficeHours = await _addOfficeHoursRepository.DeleteOfficeHoursAsync(deleteOfficeHoursEntity);
                var deleteOfficeHoursDtoAdapter = _adapterRegistry.GetAdapter<DeleteOfficeHours, Dtos.Student.DeleteOfficeHours>();
                Dtos.Student.DeleteOfficeHours updateOfficeHoursDto = deleteOfficeHoursDtoAdapter.MapToType(deletedAddOfficeHours);
                return updateOfficeHoursDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for delete";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while deleting office hours";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to delete office Hours ";
                logger.Error(ex, message);
                throw;
            }
        }

        public async Task AuthorizeUser(string personId) 
        {                      
            if (UserIsSelf(personId))
            {
                if (await UserIsAdvisorAsync(personId) || (await _facultyRepository.GetAsync(personId)) != null ) 
                { 
                    return; 
                }
                else
                {
                    var message1 = "User does not have permissions to perform this action. User should either be an advisor or faculty.";
                    logger.Info(message1);
                    throw new PermissionsException(message1);
                }
            }
            var message2 = "User does not have permissions to access this person.";
            logger.Info(message2);
            throw new PermissionsException(message2);
        }
    }
}
