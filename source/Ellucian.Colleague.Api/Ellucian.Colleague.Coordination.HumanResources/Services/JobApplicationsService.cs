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
    public class JobApplicationsService : BaseCoordinationService, IJobApplicationsService
    {

        private readonly IJobApplicationsRepository _jobApplicationsRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IPersonRepository _personRepository;
        private IEnumerable<Domain.HumanResources.Entities.Position> _institutionPositions;

        public JobApplicationsService(

            IJobApplicationsRepository jobApplicationsRepository,
            IPositionRepository positionRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _jobApplicationsRepository = jobApplicationsRepository;
            _positionRepository = positionRepository;
            _personRepository = personRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all job-applications
        /// </summary>
        /// <returns>Collection of JobApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.JobApplications>, int>> GetJobApplicationsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckGetViewJobApplicationsPermission();

            var jobApplicationsCollection = new List<Ellucian.Colleague.Dtos.JobApplications>();

            var pageOfItems = await _jobApplicationsRepository.GetJobApplicationsAsync(offset, limit, bypassCache);

            var jobApplicationsEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (jobApplicationsEntities != null && jobApplicationsEntities.Any())
            {
                foreach (var jobApplications in jobApplicationsEntities)
                {
                    jobApplicationsCollection.Add(await ConvertJobApplicationsEntityToDto(jobApplications));
                }

                return new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, totalRecords);
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.JobApplications>, int>(new List<Dtos.JobApplications>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a JobApplications from its GUID
        /// </summary>
        /// <returns>JobApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.JobApplications> GetJobApplicationsByGuidAsync(string guid)
        {
            try
            {
                CheckGetViewJobApplicationsPermission();

                return await ConvertJobApplicationsEntityToDto(await _jobApplicationsRepository.GetJobApplicationByIdAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("job-applications not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("job-applications not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a JobApplication domain entity to its corresponding JobApplications DTO
        /// </summary>
        /// <param name="source">JobApplication domain entity</param>
        /// <returns>JobApplications DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.JobApplications> ConvertJobApplicationsEntityToDto(JobApplication source)
        {
            var jobApplications = new Ellucian.Colleague.Dtos.JobApplications();

            jobApplications.Id = source.Guid;
            var personGuid = await _jobApplicationsRepository.GetGuidFromIdAsync(source.PersonId, "PERSON");
            jobApplications.Person = new GuidObject2(personGuid);
            if (!string.IsNullOrEmpty(source.PositionId))
            {
                var positionGuid = await _jobApplicationsRepository.GetGuidFromIdAsync(source.PositionId, "POSITION");
                jobApplications.Position = new GuidObject2(positionGuid);
            }
            if (source.AppliedOn != null)
            {
                jobApplications.AppliedOn = (DateTime) source.AppliedOn;
            }
            if (source.DesiredCompensationRateValue != null)
            {
                var hostCountry = await GetHostCountryAsync();
                var currencyIsoCode = CurrencyIsoCode.NotSet;
                
                if (!string.IsNullOrEmpty(hostCountry))
                {
                    switch (hostCountry)
                    {
                        case "US":
                        case "USA":
                            currencyIsoCode = CurrencyIsoCode.USD;
                            break;
                        case "CAN":
                        case "CANADA":
                            currencyIsoCode = CurrencyIsoCode.CAD;
                            break;
                    }
                }
                else
                {
                    throw new ArgumentNullException("Host country not found.");
                }
                var isSalary = true;
				if (!string.IsNullOrEmpty(source.PositionId))
                {
                    //var institutionPositions = await GetAllInstitutionPositionsAsync(false);
                    var institutionPosition = await _positionRepository.GetPositionByGuidAsync(jobApplications.Position.Id);
                    if (institutionPosition != null)
                    {
                        isSalary = institutionPosition.IsSalary;
                    }
                }
                jobApplications.DesiredCompensation = new Dtos.DtoProperties.DesiredCompensationProperty()
                {
                    Rate = new JobApplicationsRateDtoProperty() { Value = source.DesiredCompensationRateValue, Currency = currencyIsoCode },
					Period = !isSalary ? SalaryPeriod.Hour : SalaryPeriod.Year,
                };
			}
                                                                                                                        
            return jobApplications;
        }

        /// <summary>
        /// Get all InstitutionPosition Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.Position>> GetAllInstitutionPositionsAsync(bool bypassCache)
        {
            if (_institutionPositions == null)
            {
                var _institutionPositionsTuple = await _positionRepository.GetPositionsAsync(0, 200, bypassCache : bypassCache);
                if (_institutionPositionsTuple != null)
                {
                    _institutionPositions = _institutionPositionsTuple.Item1.ToList();
                }
            }
            return _institutionPositions;
        }

        private string _hostCountry = null;
        private async Task<string> GetHostCountryAsync()
        {
            if (_hostCountry == null)
            {
                _hostCountry = await _personRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view JobApplications.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetViewJobApplicationsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewJobApplications);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Job Applications.");
            }
        }
      
    }

}