//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Exceptions;

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

            var externalEmploymentsCollection = new List<Ellucian.Colleague.Dtos.ExternalEmployments>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.ExternalEmployments>, int> externalEmploymentsData = null;
            try
            {
                externalEmploymentsData = await _externalEmploymentsRepository.GetExternalEmploymentsAsync(offset, limit);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (externalEmploymentsData != null)
            {
                var externalEmploymentsEntities = externalEmploymentsData.Item1;
                if (externalEmploymentsEntities != null && externalEmploymentsEntities.Any())
                {
                    externalEmploymentsCollection = (await BuildExternalEmploymentsDtoAsync(externalEmploymentsEntities, bypassCache)).ToList();
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentsCollection, 0);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentsCollection, externalEmploymentsData.Item2);  
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

            try
            {
                var externalEmploymentsEntity = await _externalEmploymentsRepository.GetExternalEmploymentsByGuidAsync(guid);

                var externalEmploymentsDto = await BuildExternalEmploymentsDtoAsync(new List<Domain.Base.Entities.ExternalEmployments>()
                { externalEmploymentsEntity });

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                if (externalEmploymentsDto.Any())
                {
                    return externalEmploymentsDto.FirstOrDefault();
                }
                else
                {
                    throw new KeyNotFoundException("No external-employments found for GUID " + guid);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No external-employments found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No external-employments found for GUID " + guid, ex);
            }                       
        }

        /// <summary>
        /// BuildExternalEmploymentsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of ExternalEmployments domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>Collection of ExternalEmployments DTO objects </returns>
        private async Task<IEnumerable<Dtos.ExternalEmployments>> BuildExternalEmploymentsDtoAsync(IEnumerable<Domain.Base.Entities.ExternalEmployments> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var externalEmployment = new List<Dtos.ExternalEmployments>();
            Dictionary<string, string> personGuidCollection = null;

            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                     .Select(x => x.PersonId).Distinct().ToList();
                var organizationIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.OrganizationId)))
                     .Select(x => x.OrganizationId).Distinct().ToList();
                personIds.AddRange(organizationIds);
                personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }


            foreach (var source in sources)
            {
                try
                {
                    externalEmployment.Add(await ConvertExternalEmploymentsEntityToDto(source, personGuidCollection, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, id: source.Id, guid: source.Guid);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return externalEmployment;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ExternalEmployments domain entity to its corresponding ExternalEmployments DTO
        /// </summary>
        /// <param name="source">ExternalEmployments domain entity</param>
        /// <returns>ExternalEmployments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.ExternalEmployments> ConvertExternalEmploymentsEntityToDto(Ellucian.Colleague.Domain.Base.Entities.ExternalEmployments source,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("External Employments entity must be provided.");
                return null;
            }
            var externalEmployments = new Ellucian.Colleague.Dtos.ExternalEmployments();
            //get guid
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError(string.Format("GUID not found for external-employments '{0}'", source.Id), "GUID.Not.Found", "", source.Id);
            }
            else
            {
                externalEmployments.Id = source.Guid;
                //get person guid
                if (!string.IsNullOrEmpty(source.PersonId))
                {

                    if (personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for person id '", source.PersonId, "'"),
                            "", source.Guid, source.Id);
                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for person id: '", source.PersonId, "'"),
                               "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            externalEmployments.Person = new Dtos.GuidObject2(personGuid);
                        }
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for person id: '", source.PersonId, "'"),
                               "GUID.Not.Found", source.Guid, source.Id);
                }
                //get position info
                if (!string.IsNullOrEmpty(source.PositionId))
                {
                    var positions = await GetAllExternalPositionsAsync(bypassCache);
                    if (positions != null)
                    {
                        var extPosition = positions.FirstOrDefault(id => id.Code == source.PositionId);
                        if (extPosition == null)
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for external employments position '", source.PositionId, "'"),
                           "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            externalEmployments.Position = new Dtos.GuidObject2(extPosition.Guid);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for external employments position '", source.PositionId, "'"),
                           "GUID.Not.Found", source.Guid, source.Id);
                    }

                }
                //get organization info               
                var orgInfo = new ExternalEmploymentsOrganization();
                if (!string.IsNullOrEmpty(source.OrganizationId))
                {    
                    if(personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for organization id '", source.OrganizationId, "'"),
                            "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.OrganizationId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for organization id: '", source.OrganizationId, "'"),
                               "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            orgInfo.Detail = new Dtos.GuidObject2(personGuid);
                            orgInfo.Name = source.OrgName;
                        }
                    }
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
                    if (statuses != null)
                    {
                        var exstatus = statuses.FirstOrDefault(id => id.Code == source.Status);
                        if (exstatus == null)
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for external employment status '", source.Status, "'"),
                            "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            externalEmployments.Status = new Dtos.GuidObject2(exstatus.Guid);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for external employment status '", source.Status, "'"),
                           "GUID.Not.Found", source.Guid, source.Id);
                    }      
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
                    if (allvocations != null)
                    {
                        foreach (var voc in source.Vocations)
                        {
                            var vocation = allvocations.FirstOrDefault(id => id.Code == voc);
                            if (vocation == null)
                            {
                                IntegrationApiExceptionAddError(string.Concat("GUID not found for employment vocation '", voc, "'"),
                           "GUID.Not.Found", source.Guid, source.Id);
                            }
                            else
                            {
                                vocations.Add(new Dtos.GuidObject2(vocation.Guid));
                            }
                        }
                        if (vocations.Any())
                        {
                            externalEmployments.Vocations = vocations;
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("GUIDs not found for employment vocations"), "GUID.Not.Found", source.Guid, source.Id);
                    }
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
            }
            return externalEmployments;
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