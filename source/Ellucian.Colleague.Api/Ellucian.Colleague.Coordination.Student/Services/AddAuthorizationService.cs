// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    [RegisterType]
    public class AddAuthorizationService : StudentCoordinationService, IAddAuthorizationService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IAddAuthorizationRepository _addAuthorizationRepository;


        /// <summary>
        /// Constructor for AddAuthorizationService
        /// </summary>
        /// <param name="addAuthorizationRepository"></param>
        /// <param name="sectionRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public AddAuthorizationService(IAddAuthorizationRepository addAuthorizationRepository, ISectionRepository sectionRepository, IStudentRepository studentRepository, IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,  IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _addAuthorizationRepository = addAuthorizationRepository;
            _sectionRepository = sectionRepository;


        }
        /// <summary>
        /// Update Add Authorization
        /// </summary>
        /// <param name="addAuthorization">add authorization to update</param>
        /// <returns>Updated Add Authorization</returns>
        public async Task<Dtos.Student.AddAuthorization> UpdateAddAuthorizationAsync(Dtos.Student.AddAuthorization addAuthorization)
        {
            if (addAuthorization == null)
            {
                throw new ArgumentNullException("addAuthorization", "Add Authorization to update must be included");
            }
            if (string.IsNullOrEmpty(addAuthorization.SectionId))
            {
                throw new ArgumentException("Add Authorization must have a section Id.");
            }
            if (string.IsNullOrEmpty(addAuthorization.Id) && (string.IsNullOrEmpty(addAuthorization.AddAuthorizationCode) || string.IsNullOrEmpty(addAuthorization.StudentId)))
            {
                throw new ArgumentException("Add Authorization must either have an Id or must have both an add code & student Id for update.");
            }
            AddAuthorization addAuthorizationOnfile = null;

            if (string.IsNullOrEmpty(addAuthorization.Id))
            {
                // if item to update has no id, supply one. 
                addAuthorizationOnfile = await _addAuthorizationRepository.GetAddAuthorizationByAddCodeAsync(addAuthorization.SectionId, addAuthorization.AddAuthorizationCode);
                // Found the add authorization for this and it is yet unassigned.  Use this ID for the update
                addAuthorization.Id = addAuthorizationOnfile != null ? addAuthorizationOnfile.Id : null;
            } else
            {
                addAuthorizationOnfile = await _addAuthorizationRepository.GetAsync(addAuthorization.Id);
            }
            if (addAuthorizationOnfile == null)
            {
                var message = "Add authorization not found for section " + addAuthorization.SectionId + " with add code " + addAuthorization.AddAuthorizationCode;
                logger.Error(message);
                throw new KeyNotFoundException();
            }
            // Check Permissions to determine if current user is allowed to do the update
            await CheckAddAuthorizationPermissions(addAuthorization);

            // Validate the update
            IsUpdateAddAuthorizationValid(addAuthorization, addAuthorizationOnfile);
            
            // Convert Dto to Entity
            AddAuthorization addAuthorizationEntity = ConvertAddAuthorizationDtoToEntity(addAuthorization);

            // Update and return
            try
            {
                var updatedAddAuthorization = await _addAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationEntity);
                var addAuthorizationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>();
                Dtos.Student.AddAuthorization updatedAddAuthorizationDto = addAuthorizationDtoAdapter.MapToType(updatedAddAuthorization);
                return updatedAddAuthorizationDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for add authorization with ID " + addAuthorization.Id;
                logger.Info(kex, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update add authorization " + addAuthorization.Id;
                logger.Info(ex, message);
                throw;
            }


        }
        /// <summary>
        /// Retrieves add authorizations for a section
        /// </summary>
        /// <param name="sectionId">id of section</param>
        /// <returns>Add Authorizations for the section.</returns>
        public async Task<IEnumerable<Dtos.Student.AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("Add Authorization must have a section Id.");
            }

            await CheckFacultyAuthorizationsPermissions(sectionId);

            try
            {
                var sectionAddAuthorizations = await _addAuthorizationRepository.GetSectionAddAuthorizationsAsync(sectionId);
                var sectionAddAuthorizationDtos = new List<Dtos.Student.AddAuthorization>();
                if (sectionAddAuthorizations != null && sectionAddAuthorizations.Any())
                {
                    var addAuthorizationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>();

                    foreach (var saa in sectionAddAuthorizations)
                    {
                        Dtos.Student.AddAuthorization saadto = addAuthorizationDtoAdapter.MapToType(saa);
                        sectionAddAuthorizationDtos.Add(saadto);
                    }
                }
                return sectionAddAuthorizationDtos;  
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to retrieve add authorizations for section " + sectionId;
                logger.Info(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Create an add authorization information for a student in a section
        /// </summary>
        /// <param name="addAuthorization">The AddAuthorization to create</param>
        /// <returns>Created AddAuthorization</returns>
        public async Task<Dtos.Student.AddAuthorization> CreateAddAuthorizationAsync(Dtos.Student.AddAuthorizationInput addAuthorizationInput)
        {
            if (addAuthorizationInput == null)
            {
                throw new ArgumentNullException("addAuthorizationInput", "Add Authorization Input must be included to create a new authorization.");
            }
            if (string.IsNullOrEmpty(addAuthorizationInput.SectionId))
            {
                throw new ArgumentException("Add Authorization Input must have a section Id to create new authorization.");
            }
            if (string.IsNullOrEmpty(addAuthorizationInput.StudentId))
            {
                throw new ArgumentException("Add Authorization Input must have a student Id to create new authorization.");
            }

            // Check Permissions to be sure user is faculty on the section.
            await CheckFacultyAuthorizationsPermissions(addAuthorizationInput.SectionId);


            // Convert Dto to Entity
            AddAuthorization addAuthorizationEntity = ConvertAddAuthorizationInputDtoToEntity(addAuthorizationInput);

            // Update and return
            try
            {
                var newAddAuthorization = await _addAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationEntity);
                var addAuthorizationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>();
                Dtos.Student.AddAuthorization updatedAddAuthorizationDto = addAuthorizationDtoAdapter.MapToType(newAddAuthorization);
                return updatedAddAuthorizationDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for newly add authorization";
                logger.Info(kex, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to create add authorization for section " + addAuthorizationInput.SectionId + " and student Id " + addAuthorizationInput.StudentId;
                logger.Info(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Retrieve an add authorization item.
        /// </summary>
        /// <param name="addAuthorization">AddAuthoriation DTO to update</param>
        /// <returns>An Add Authorization</returns>
        public async Task<Dtos.Student.AddAuthorization> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Add Authorization id is required to get an add authorization");
            }

            AddAuthorization addAuthorization = await _addAuthorizationRepository.GetAsync(id);
            var addAuthorizationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>();
            Dtos.Student.AddAuthorization addAuthorizationDto = addAuthorizationDtoAdapter.MapToType(addAuthorization);

            // Check Permissions to determine if current user is allowed to do the update
            await CheckAddAuthorizationPermissions(addAuthorizationDto);
            return addAuthorizationDto;
        }

        /// <summary>
        /// Used to check permissions for accessing an add authorization
        /// </summary>
        /// <param name="addAuthorization"></param>
        /// <returns></returns>
        private async Task CheckAddAuthorizationPermissions(Dtos.Student.AddAuthorization addAuthorization)
        {
            // Check permissions
            // A person can only update an add authorization if
            // 1) They are the student on the authorization
            // 2) There is no student ID or it is not the student BUT they are the faculty member of the section on the authorization.
            if (string.IsNullOrEmpty(addAuthorization.StudentId) || !UserIsSelf(addAuthorization.StudentId))
            {
                // If person doing the update is not the student on the authorization, then only allow update if it is the faculty of the section on the authorization.
                await CheckFacultyAuthorizationsPermissions(addAuthorization.SectionId);
            }
        }

        private async Task CheckFacultyAuthorizationsPermissions(string sectionId)
        {

            List<string> ids = new List<string>() { sectionId };
            IEnumerable<Domain.Student.Entities.Section> sections = await _sectionRepository.GetCachedSectionsAsync(ids);
            Domain.Student.Entities.Section section = null;
            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                if (!UserIsSectionFaculty(section))
                {
                    throw new PermissionsException("You are not authorized to update this add authorization.");
                }

            }
            else
            {
                throw new ArgumentException("Section Id " + sectionId + " does not exist.");
            }
        }

        private AddAuthorization ConvertAddAuthorizationDtoToEntity(Dtos.Student.AddAuthorization addAuthorization)
        {
            // Convert the DTO to an entity
            Domain.Student.Entities.AddAuthorization addAuthorizationEntity = null;
            try
            {
                var addAuthorizationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.AddAuthorization, Domain.Student.Entities.AddAuthorization>();
                addAuthorizationEntity = addAuthorizationDtoToEntityAdapter.MapToType(addAuthorization);
                return addAuthorizationEntity;
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming AddAuthorization Dto to AddAuthorization Entity: " + ex.Message);
                throw new ArgumentException("AddAuthorization is invalid", ex);
            }
        }

        private AddAuthorization ConvertAddAuthorizationInputDtoToEntity(Dtos.Student.AddAuthorizationInput addAuthorizationInput)
        {
            // Convert the Input DTO to an entity
            Domain.Student.Entities.AddAuthorization addAuthorizationEntity = null;
            try
            {
                var addAuthorizationInputDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.AddAuthorizationInput, Domain.Student.Entities.AddAuthorization>();
                addAuthorizationEntity = addAuthorizationInputDtoToEntityAdapter.MapToType(addAuthorizationInput);
                return addAuthorizationEntity;
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming AddAuthorizationInput Dto to AddAuthorization Entity: " + ex.Message);
                throw new ArgumentException("AddAuthorizationInput is invalid", ex);
            }
        }

        private void IsUpdateAddAuthorizationValid(Dtos.Student.AddAuthorization addAuthorizationToUpdate, AddAuthorization addAuthorizationOnfile)
        {
            // The following checks are separated so logging can be specific.

            // Cannot change section Id on an add authorization
            if (addAuthorizationToUpdate.SectionId != addAuthorizationOnfile.SectionId)
            {
                var message = "Validate Authorization Update: Cannot change sectionid for add authorization  " + addAuthorizationOnfile.Id;
                logger.Error(message);
                throw new ArgumentException(message);
            }
            // Cannot change the generated add code on an authorization
            if (addAuthorizationToUpdate.AddAuthorizationCode != addAuthorizationOnfile.AddAuthorizationCode)
            {
                var message = "Validate Authorization Update: Cannot change generated add code for add authorization  " + addAuthorizationOnfile.Id;
                logger.Error(message);
                throw new ArgumentException(message);
            }
            //Since we have the authorization to update - make sure it isn't already assigned to another student.
            if (!string.IsNullOrEmpty(addAuthorizationOnfile.StudentId) && addAuthorizationOnfile.StudentId != addAuthorizationToUpdate.StudentId)
            {
                var message = "Validate Authorization Update: Add authorization for section " + addAuthorizationToUpdate.SectionId + " with add code " + addAuthorizationToUpdate.AddAuthorizationCode + " assigned to another student";
                logger.Error(message);
                throw new ArgumentException(message);
            }
            // Cannot unrevoke a previously revoked authorization
            if (addAuthorizationOnfile.IsRevoked && !addAuthorizationToUpdate.IsRevoked)
            {
                var message = "Validate Authorization Update: Cannot unrevoke add authorization  " + addAuthorizationOnfile.Id;
                logger.Error(message);
                throw new ArgumentException(message);
            }

        }

    }

}
