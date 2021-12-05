//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Ellucian.Colleague.Domain.Exceptions;

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
            var jobApplicationsCollection = new List<Ellucian.Colleague.Dtos.JobApplications>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.JobApplication>, int> jobApplicationsData = null;
            try
            {
                jobApplicationsData = await _jobApplicationsRepository.GetJobApplicationsAsync(offset, limit, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            if (jobApplicationsData != null)
            {
                var jobApplicationsEntities = jobApplicationsData.Item1;
                int totalRecords = jobApplicationsData.Item2;

                if (jobApplicationsEntities != null && jobApplicationsEntities.Any())
                {
                    jobApplicationsCollection = (await BuildJobApplicationsDtoAsync(jobApplicationsEntities, bypassCache)).ToList();
                    if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                    {
                        throw IntegrationApiException;
                    }
                    return new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, totalRecords);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.JobApplications>, int>(new List<Dtos.JobApplications>(), 0);
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, 0);
            }            
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a JobApplications from its GUID
        /// </summary>
        /// <returns>JobApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.JobApplications> GetJobApplicationsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a job-applications.");
            }

            try
            {
                var jobApplicationsEntity = await _jobApplicationsRepository.GetJobApplicationByIdAsync(guid);

                var jobApplicationsDto = await BuildJobApplicationsDtoAsync(new List<Domain.HumanResources.Entities.JobApplication>()
                { jobApplicationsEntity });

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                if (jobApplicationsDto != null)
                {
                    return jobApplicationsDto.FirstOrDefault();
                }
                else
                {
                    throw new KeyNotFoundException("No job-applications found for GUID " + guid);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No job-applications found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No job-applications found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// BuildJobApplicationsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of JobApplication domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>Collection of JobApplications DTO objects </returns>
        private async Task<IEnumerable<Dtos.JobApplications>> BuildJobApplicationsDtoAsync(IEnumerable<Domain.HumanResources.Entities.JobApplication> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var jobApplications = new List<Dtos.JobApplications>();
            Dictionary<string, string> personGuidCollection = null;

            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                     .Select(x => x.PersonId).Distinct().ToList();              
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
                    jobApplications.Add(await ConvertJobApplicationsEntityToDto(source, personGuidCollection, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, id: source.PersonId, guid: source.Guid);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return jobApplications;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a JobApplication domain entity to its corresponding JobApplications DTO
        /// </summary>
        /// <param name="source">JobApplication domain entity</param>
        /// <returns>JobApplications DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.JobApplications> ConvertJobApplicationsEntityToDto(JobApplication source, 
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("Job Applications entity must be provided.");
                return null;
            }

            var jobApplications = new Ellucian.Colleague.Dtos.JobApplications();
            //get guid
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError(string.Format("Unable to find a GUID for job applications", "GUID.Not.Found", "", source.PersonId));
            }
            else
            {
                jobApplications.Id = source.Guid;
                //get person guid
                if (!string.IsNullOrEmpty(source.PersonId))
                {

                    if (personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for person id '", source.PersonId, "'"),
                            "", source.Guid, source.PersonId);
                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for person id: '", source.PersonId, "'"),
                               "GUID.Not.Found", source.Guid, source.PersonId);
                        }
                        else
                        {
                            jobApplications.Person = new Dtos.GuidObject2(personGuid);
                        }
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to find Person Id. Person Id is required."), "GUID.Not.Found", source.Guid, source.PersonId);
                }

                //get position info
                if (!string.IsNullOrEmpty(source.PositionId))
                {
                    try
                    {
                        var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);

                        if (!string.IsNullOrEmpty(positionGuid))
                        {
                            jobApplications.Position = new Dtos.GuidObject2(positionGuid);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID not found for position '{0}'", source.PositionId), "GUID.Not.Found", source.Guid, source.PersonId);
                    }
                }
                if (source.AppliedOn != null)
                {
                    jobApplications.AppliedOn = (DateTime)source.AppliedOn;
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
                        IntegrationApiExceptionAddError("Host country not found.", "Bad.Data", source.Guid, source.PersonId);
                    }
                    var isSalary = true;
                    if (!string.IsNullOrEmpty(source.PositionId))
                    {
                        try
                        {
                            var institutionPosition = await _positionRepository.GetPositionByIdAsync(source.PositionId);
                            if (institutionPosition != null)
                            {
                                isSalary = institutionPosition.IsSalary;
                            }
                        }
                        catch(RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid position {0} for GUID {1}.", source.PositionId, jobApplications.Position.Id), "Bad.Data", source.Guid, source.PersonId);
                        }
                    }
                    jobApplications.DesiredCompensation = new Dtos.DtoProperties.DesiredCompensationProperty()
                    {
                        Rate = new JobApplicationsRateDtoProperty() { Value = source.DesiredCompensationRateValue, Currency = currencyIsoCode },
                        Period = !isSalary ? SalaryPeriod.Hour : SalaryPeriod.Year,
                    };
                }
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
                var _institutionPositionsTuple = await _positionRepository.GetPositionsAsync(0, 200, bypassCache: bypassCache);
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
    }
}