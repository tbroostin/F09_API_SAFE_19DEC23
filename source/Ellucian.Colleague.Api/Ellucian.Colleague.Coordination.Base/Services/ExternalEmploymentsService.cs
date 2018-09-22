//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Dtos.DtoProperties;
using System.Diagnostics;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class ExternalEmploymentsService : BaseCoordinationService, IExternalEmploymentsService
    {
        private readonly IExternalEmploymentsRepository _externalEmploymentsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private IEnumerable<Positions> _externalPositions;
        private IEnumerable<ExternalEmploymentStatus> _externalEmployStatuses;
        private IEnumerable<Vocation> _employmentVocations;
        public ExternalEmploymentsService(

            IExternalEmploymentsRepository externalEmploymentsRepository,
            IAdapterRegistry adapterRegistry,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _externalEmploymentsRepository = externalEmploymentsRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all external-employments
        /// </summary>
        /// <returns>Collection of ExternalEmployments DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmployments>, int>> GetExternalEmploymentsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckExternalEmploymentsPermission();
            var externalEmploymentsCollection = new List<Ellucian.Colleague.Dtos.ExternalEmployments>();
            var externalEmploymentsEntities = await _externalEmploymentsRepository.GetExternalEmploymentsAsync(offset, limit);
            var totalRecords = externalEmploymentsEntities.Item2;
           

            if (externalEmploymentsEntities != null && externalEmploymentsEntities.Item1.Any())
            {
                foreach (var externalEmployments in externalEmploymentsEntities.Item1)
                {
                    externalEmploymentsCollection.Add(await ConvertExternalEmploymentsEntityToDto(externalEmployments,bypassCache));
                }
            }
           return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmployments>, int>(externalEmploymentsCollection, totalRecords); 
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ExternalEmployments from its GUID
        /// </summary>
        /// <returns>ExternalEmployments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ExternalEmployments> GetExternalEmploymentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an external-employments.");
            }
            CheckExternalEmploymentsPermission();
            try
            {
                return await ConvertExternalEmploymentsEntityToDto(await _externalEmploymentsRepository.GetExternalEmploymentsByGuidAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("external-employments not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("external-employments not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ExternalEmployments domain entity to its corresponding ExternalEmployments DTO
        /// </summary>
        /// <param name="source">ExternalEmployments domain entity</param>
        /// <returns>ExternalEmployments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.ExternalEmployments> ConvertExternalEmploymentsEntityToDto(Ellucian.Colleague.Domain.Base.Entities.ExternalEmployments source, bool bypassCache = false)
        {
            var externalEmployments = new Ellucian.Colleague.Dtos.ExternalEmployments();
            //get guid
            if (string.IsNullOrEmpty(source.Guid))
            {
                throw new ArgumentException(string.Concat("Unable to find a GUID for external employments. Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
            }
            externalEmployments.Id = source.Guid;
            //get person guid
            if (!string.IsNullOrEmpty(source.PersonId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (string.IsNullOrEmpty(personGuid))
                {
                    throw new ArgumentException(string.Concat("Unable to find a GUID for Person '", source.PersonId, "' person.id.Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
                }
                externalEmployments.Person = new Dtos.GuidObject2(personGuid);
            }
            else
            {
                throw new ArgumentException(string.Concat("Unable to find Person Id. Person Id is required. Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
            }
            //get position info
            if (!string.IsNullOrEmpty(source.PositionId))
            {
                var positions = await GetAllExternalPositionsAsync(bypassCache);
                if (positions == null)
                {
                    throw new Exception("Unable to retrieve external employments positions");
                }
                var extPosition = positions.FirstOrDefault(id => id.Code == source.PositionId);
                if (extPosition == null)
                {
                    throw new ArgumentException(string.Concat("Unable to find a GUID for external employments position '", source.PositionId, "' position.id. Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
                }
                externalEmployments.Position = new Dtos.GuidObject2(extPosition.Guid);
            }
            //get organization info
            var orgInfo = new ExternalEmploymentsOrganization();
            if (!string.IsNullOrEmpty(source.OrganizationId))
            {
                var orgGuid = await _personRepository.GetPersonGuidFromIdAsync(source.OrganizationId);
                if (string.IsNullOrEmpty(orgGuid))
                {
                    throw new ArgumentException(string.Concat("Unable to find a GUID for Organization '", source.OrganizationId, "' organization.id.   Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
                }
                orgInfo.Detail = new Dtos.GuidObject2(orgGuid);
                orgInfo.Name = source.OrgName;
                
            }
            else
            {
                //Otherwise, if the EMP.SELF.EMPLOYED.FLAG is 'Y' then publish "Self Employed".
               //the EMP.UNKNOWN.EMPLOYER.FLAG is 'Y' then publish "Unknown Employer".
                if (String.Equals(source.selfEmployed, (string)"Y", StringComparison.OrdinalIgnoreCase)) 
                {
                    orgInfo.Name = "Self Employed";
                }
                else if (String.Equals(source.unknownEmployer, (string)"Y", StringComparison.OrdinalIgnoreCase))
                {
                    orgInfo.Name = "Unknown Employer";
                }
                else
                {
                    orgInfo.Name = "Unknown Employer";
                }

            }
            externalEmployments.Organization = orgInfo;
            // get starton
            if (source.StartDate.HasValue)
            {
                var startDate = new DateDtoProperty
                {
                    Month = source.StartDate.Value.Month,
                    Day = source.StartDate.Value.Day,
                    Year = source.StartDate.Value.Year
                };
                externalEmployments.StartOn = startDate;
            }                                                                                                                                                                                      
            // get Endon
            if (source.EndDate.HasValue)
            {
                var endtDate = new DateDtoProperty
                {
                    Month = source.EndDate.Value.Month,
                    Day = source.EndDate.Value.Day,
                    Year = source.EndDate.Value.Year
                };
                externalEmployments.EndOn = endtDate;
            }
            //get job title
            if (!string.IsNullOrEmpty(source.JobTitle))
            {
                externalEmployments.JobTitle = source.JobTitle;
            }
            //get priority 
            if (!string.IsNullOrEmpty(source.PrincipalEmployment))
            {
                 if (String.Equals(source.PrincipalEmployment, (string)"Y", StringComparison.OrdinalIgnoreCase))
                 {
                     externalEmployments.Priority = ExternalEmploymentsPriority.Primary;
                 }
                 else
                 {
                     externalEmployments.Priority = ExternalEmploymentsPriority.Secondary;
                 }
            }
            //get status
            if (!string.IsNullOrEmpty(source.Status))
            {
                var statuses = await GetAllExternalEmploymentStatusesAsync(bypassCache);
                if (statuses == null)
                {
                    throw new Exception("Unable to retrieve external employment statuses");
                }
                var exstatus = statuses.FirstOrDefault(id => id.Code == source.Status);
                if (exstatus == null)
                {
                    throw new ArgumentException(string.Concat("Unable to find a GUID for  external employment status ", source.Status, " status.id.   Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
                }
                externalEmployments.Status = new Dtos.GuidObject2(exstatus.Guid);
            }
            //get hours worked
            if (source.HoursWorked != null)
            {
                var hoursworked = new HoursPerPeriodDtoProperty();
                hoursworked.Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Week;
                hoursworked.Hours = source.HoursWorked;
                externalEmployments.HoursWorked = hoursworked;
            }
            // get vocations
            if (source.Vocations != null && source.Vocations.Any())
            {
                var allvocations = await GetAllEmploymenVocationsAsync(bypassCache);
                var vocations = new List<Dtos.GuidObject2>();
                if (allvocations == null)
                {
                    throw new Exception("Unable to retrieve employment vocations");
                }
                foreach (var voc in source.Vocations)
                {
                    var vocation = allvocations.FirstOrDefault(id => id.Code == voc);
                    if (vocation == null)
                    {
                        throw new ArgumentException(string.Concat("Unable to find a GUID for employment vocation ", voc, " vocations.id.   Entity: 'EMPLOYMT', Record ID: '", source.Id, "' "));
                    }

                    vocations.Add(new Dtos.GuidObject2(vocation.Guid));
                }
                externalEmployments.Vocations = vocations;
            }
            // get superviosrs
            if (source.Supervisors != null && source.Supervisors.Any())
            {
                var supervisors = new List<ExternalEmploymentsSupervisors>();
                foreach (var super in source.Supervisors)
                {
                    var supervisor = new ExternalEmploymentsSupervisors();
                    if (!string.IsNullOrEmpty(super.SupervisorFirstName) && !string.IsNullOrEmpty(super.SupervisorLastName))
                        supervisor.Name = string.Concat(super.SupervisorFirstName, " ", super.SupervisorLastName);
                    else if (!string.IsNullOrEmpty(super.SupervisorFirstName))
                        supervisor.Name = super.SupervisorFirstName;
                    else if (!string.IsNullOrEmpty(super.SupervisorLastName))
                        supervisor.Name = super.SupervisorLastName;
                    if (!string.IsNullOrEmpty(super.SupervisorEmail))
                        supervisor.Email = super.SupervisorEmail;
                    if (!string.IsNullOrEmpty(super.SupervisorPhone))
                        supervisor.Phone = super.SupervisorPhone;
                    supervisors.Add(supervisor);
                }
                externalEmployments.Supervisors = supervisors;
            }
            //get comments
            if (!string.IsNullOrEmpty(source.comments))
                externalEmployments.Comment = source.comments;
            return externalEmployments;
        }

        

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to external employments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckExternalEmploymentsPermission()
        {
            var hasPermission = HasPermission(BasePermissionCodes.ViewAnyExternalEmployments);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view external employments.");
            }
        }


        /// <summary>
        /// Get all external employment positions
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>external Positions</returns>
        private async Task<IEnumerable<Positions>> GetAllExternalPositionsAsync(bool bypassCache)
        {
            if (_externalPositions == null)
            {
                _externalPositions = await _referenceDataRepository.GetPositionsAsync(bypassCache);
            }
            return _externalPositions;
        }

        /// <summary>
        /// Get all external employment statuses
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>ExternalEmploymentStatus</returns>
        private async Task<IEnumerable<ExternalEmploymentStatus>> GetAllExternalEmploymentStatusesAsync(bool bypassCache)
        {
            if (_externalEmployStatuses == null)
            {
                _externalEmployStatuses = await _referenceDataRepository.GetExternalEmploymentStatusesAsync(bypassCache);
            }
            return _externalEmployStatuses;
        }

        /// <summary>
        /// Get all employment vocations
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Vocation</returns>
        private async Task<IEnumerable<Vocation>> GetAllEmploymenVocationsAsync(bool bypassCache)
        {
            if (_employmentVocations == null)
            {
                _employmentVocations = await _referenceDataRepository.GetVocationsAsync(bypassCache);
            }
            return _employmentVocations;
        }
      
    }
   
}