//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class InstitutionJobSupervisorsService : BaseCoordinationService, IInstitutionJobSupervisorsService
    {
       
        private readonly IPositionRepository _positionRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;    
        private readonly IPersonRepository _personRepository;
        private readonly IInstitutionJobsRepository _institutionJobsRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private Dictionary<string, string> _employerGuidDictionary;

        public InstitutionJobSupervisorsService(

            IPositionRepository positionRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
             IReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            IInstitutionJobsRepository institutionJobsRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {

            this._positionRepository = positionRepository;
            this._hrReferenceDataRepository = hrReferenceDataRepository;
            this._referenceDataRepository = referenceDataRepository;
            this._personRepository = personRepository;
            this._institutionJobsRepository = institutionJobsRepository;
            this._configurationRepository = configurationRepository;

            _employerGuidDictionary = new Dictionary<string, string>();
        }

       // /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
       // /// <summary>
       // /// Gets all institution-jobs
       // /// </summary>
       // /// <returns>Collection of InstitutionJobs DTO objects</returns>
       // public async Task<Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>> GetInstitutionJobSupervisorsAsync(int offset, int limit, bool bypassCache = false)
       //{
       //     try
       //     {
       //         //check permissions
       //         CheckGetInstitutionJobSupervisorsPermission();

       //         var institutionJobSupervisorsEntities = await _hrReferenceDataRepository.GetInstitutionJobSupervisorsAsync(bypassCache);

       //         var totalCount = institutionJobSupervisorsEntities.Count();

       //         if (totalCount > 0)
       //         {
       //             var subInstitutionList = institutionJobSupervisorsEntities.Skip(offset).Take(limit).ToList();

       //             var institutionJobSupervisors = new List<Colleague.Dtos.InstitutionJobSupervisors>();

       //             foreach (var institutionJobsEntity in subInstitutionList)
       //             {
       //                 institutionJobSupervisors.Add(await this.ConvertInstitutionJobSupervisorsEntityToDtoAsync(institutionJobsEntity, bypassCache));
       //             }
       //             return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(institutionJobSupervisors, totalCount);

       //         }
       //         else
       //         {
       //             return new Tuple<IEnumerable<InstitutionJobSupervisors>, int>(new List<InstitutionJobSupervisors>(), totalCount);
       //         }
       //         //no results
       //         return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(new List<Dtos.InstitutionJobSupervisors>(), 0);
       //     }
       //     catch (Exception e)
       //     {
       //         throw new ArgumentException(e.Message);
       //     }
       // }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all institution-jobs
        /// </summary>
        /// <returns>Collection of InstitutionJobs DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>> GetInstitutionJobSupervisorsAsync(int offset, int limit, bool ignoreCache = false)
        {
            try
            {
                //check permissions
                CheckGetInstitutionJobSupervisorsPermission();

                var institutionJobEntities = await _institutionJobsRepository.GetInstitutionJobsAsync(offset, limit, bypassCache: ignoreCache);

                var totalCount = institutionJobEntities.Item1 != null ? institutionJobEntities.Item1.Count() : 0;

                if (totalCount > 0)
                {
                    //var subInstitutionList = institutionJobSupervisorsEntities.Skip(offset).Take(limit).ToList();

                    var institutionJobSupervisors = new List<Colleague.Dtos.InstitutionJobSupervisors>();

                    foreach (var institutionJobsEntity in institutionJobEntities.Item1)
                    {
                        institutionJobSupervisors.Add(await this.ConvertInstitutionJobSupervisorsEntityToDto2Async(institutionJobsEntity, ignoreCache));
                    }
                    return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(institutionJobSupervisors, institutionJobEntities.Item2);

                }
                else
                {
                    return new Tuple<IEnumerable<InstitutionJobSupervisors>, int>(new List<InstitutionJobSupervisors>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(new List<Dtos.InstitutionJobSupervisors>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstitutionJobs from its GUID
        /// </summary>
        /// <returns>InstitutionJobs DTO object</returns>
        public async Task<Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Job Supervisor.");
            }
            CheckGetInstitutionJobSupervisorsPermission();
            try
            {
                //var institutionJobsEntity = (await _hrReferenceDataRepository.GetInstitutionJobSupervisorsAsync(true)).Where(h => h.Guid == guid);
                var institutionJobsEntity = (await _institutionJobsRepository.GetInstitutionJobsByGuidAsync(guid));
                if (institutionJobsEntity == null)
                {
                    throw new KeyNotFoundException("Institution Job Supervisor not found for GUID " + guid);
                }
                return await ConvertInstitutionJobSupervisorsEntityToDto2Async(institutionJobsEntity);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Institution Job Supervisor not found for GUID " + guid);
            }     
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all institution-jobs
        /// </summary>
        /// <returns>Collection of InstitutionJobs DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>> GetInstitutionJobSupervisors2Async(int offset, int limit, bool ignoreCache = false)
        {
            try
            {
                //check permissions
                CheckGetInstitutionJobSupervisorsPermission();

                var institutionJobEntities = await _institutionJobsRepository.GetInstitutionJobsAsync(offset, limit, bypassCache: ignoreCache);

                var totalCount = institutionJobEntities.Item1 != null ? institutionJobEntities.Item1.Count() : 0;

                if (totalCount > 0)
                {
                    //var subInstitutionList = institutionJobSupervisorsEntities.Skip(offset).Take(limit).ToList();

                    var institutionJobSupervisors = new List<Colleague.Dtos.InstitutionJobSupervisors>();

                    foreach (var institutionJobsEntity in institutionJobEntities.Item1)
                    {
                        institutionJobSupervisors.Add(await this.ConvertInstitutionJobSupervisorsEntityToDto3Async(institutionJobsEntity, ignoreCache));
                    }
                    return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(institutionJobSupervisors, institutionJobEntities.Item2);

                }
                else
                {
                    return new Tuple<IEnumerable<InstitutionJobSupervisors>, int>(new List<InstitutionJobSupervisors>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionJobSupervisors>, int>(new List<Dtos.InstitutionJobSupervisors>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstitutionJobs from its GUID
        /// </summary>
        /// <returns>InstitutionJobs DTO object</returns>
        public async Task<Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuid2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Job Supervisor.");
            }
            CheckGetInstitutionJobSupervisorsPermission();
            try
            {
                //var institutionJobsEntity = (await _hrReferenceDataRepository.GetInstitutionJobSupervisorsAsync(true)).Where(h => h.Guid == guid);
                var institutionJobsEntity = (await _institutionJobsRepository.GetInstitutionJobsByGuidAsync(guid));
                if (institutionJobsEntity == null)
                {
                    throw new KeyNotFoundException("Institution Job Supervisor not found for GUID " + guid);
                }
                return await ConvertInstitutionJobSupervisorsEntityToDto3Async(institutionJobsEntity);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Institution Job Supervisor not found for GUID " + guid);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobSupervisors domain entity to its corresponding InstitutionJobSupervisors DTO
        /// </summary>
        /// <param name="source">InstitutionJobSupervisors domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobSupervisors DTO</returns>
        private async Task<Dtos.InstitutionJobSupervisors> ConvertInstitutionJobSupervisorsEntityToDtoAsync(Domain.HumanResources.Entities.InstitutionJobSupervisor source, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobSupervisors();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Job Supervisors.");
            }

            try
            {

                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Job Supervisors.  Id:" + source.Code);
                }
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (!(string.IsNullOrEmpty(personGuid)))
                    institutionJobs.Person = new GuidObject2(personGuid);

                var employerGuid = string.Empty;
                if (!_employerGuidDictionary.TryGetValue(source.Employer, out employerGuid))
                {
                    employerGuid = await _personRepository.GetPersonGuidFromIdAsync(source.Employer);
                    if (!(string.IsNullOrEmpty(employerGuid))) 
                        _employerGuidDictionary.Add(source.Employer, employerGuid);
                }
                if (!(string.IsNullOrEmpty(employerGuid)))
                    institutionJobs.Employer = new GuidObject2(employerGuid);


                if (string.IsNullOrEmpty(source.PositionId))
                {
                    throw new ArgumentNullException("Position ID is required for Institution Job Supervisors. Id:" + source.Code);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);

                institutionJobs.Department = source.Department;

                var supervisors = new List<SupervisorsDtoProperty>();
                if (!string.IsNullOrEmpty(source.SupervisorId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.SupervisorId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJobSupervisor  supervisor Id: ", source.Code));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(source.AlternateSupervisorId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AlternateSupervisorId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJobSupervisor Id: ", source.Code) );
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;

            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Concat("An error occurred obtaining InstitutionJobSupervisor Id: ", source.Code, " ", ex.Message));
            }
            return institutionJobs;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobSupervisors domain entity to its corresponding InstitutionJobSupervisors DTO
        /// </summary>
        /// <param name="source">InstitutionJobSupervisors domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobSupervisors DTO</returns>
        private async Task<Dtos.InstitutionJobSupervisors> ConvertInstitutionJobSupervisorsEntityToDto2Async(Domain.HumanResources.Entities.InstitutionJobs source, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobSupervisors();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Job Supervisors.");
            }

            try
            {

                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Job Supervisors.  Id:" + source.Id);
                }
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (!(string.IsNullOrEmpty(personGuid)))
                    institutionJobs.Person = new GuidObject2(personGuid);

                var employerGuid = string.Empty;
                if (!_employerGuidDictionary.TryGetValue(source.Employer, out employerGuid))
                {
                    employerGuid = await _personRepository.GetPersonGuidFromIdAsync(source.Employer);
                    if (!(string.IsNullOrEmpty(employerGuid)))
                        _employerGuidDictionary.Add(source.Employer, employerGuid);
                }
                if (!(string.IsNullOrEmpty(employerGuid)))
                    institutionJobs.Employer = new GuidObject2(employerGuid);


                if (string.IsNullOrEmpty(source.PositionId))
                {
                    throw new ArgumentNullException("Position ID is required for Institution Job Supervisors. Id:" + source.Id);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);

                institutionJobs.Department = source.Department;

                var supervisors = new List<SupervisorsDtoProperty>();
                if (!string.IsNullOrEmpty(source.SupervisorId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.SupervisorId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJobSupervisor  supervisor Id: ", source.Id));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(source.AlternateSupervisorId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AlternateSupervisorId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJobSupervisor Id: ", source.Id));
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;

            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Concat("An error occurred obtaining InstitutionJobSupervisor Id: ", source.Id, " ", ex.Message));
            }
            return institutionJobs;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobSupervisors domain entity to its corresponding InstitutionJobSupervisors DTO
        /// </summary>
        /// <param name="source">InstitutionJobSupervisors domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobSupervisors DTO</returns>
        private async Task<Dtos.InstitutionJobSupervisors> ConvertInstitutionJobSupervisorsEntityToDto3Async(Domain.HumanResources.Entities.InstitutionJobs source, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobSupervisors();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Job Supervisors.");
            }

            try
            {

                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Job Supervisors.  Id:" + source.Id);
                }
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (!(string.IsNullOrEmpty(personGuid)))
                    institutionJobs.Person = new GuidObject2(personGuid);

                var employerGuid = string.Empty;
                if (!_employerGuidDictionary.TryGetValue(source.Employer, out employerGuid))
                {
                    //employerGuid = await _personRepository.GetPersonGuidFromIdAsync(source.Employer);
                    employerGuid = await _institutionJobsRepository.GetInstitutionEmployerGuidAsync();
                    if (!(string.IsNullOrEmpty(employerGuid)))
                        _employerGuidDictionary.Add(source.Employer, employerGuid);
                }
                if (!(string.IsNullOrEmpty(employerGuid)))
                    institutionJobs.Employer = new GuidObject2(employerGuid);


                if (string.IsNullOrEmpty(source.PositionId))
                {
                    throw new ArgumentNullException("Position ID is required for Institution Job Supervisors. Id:" + source.Id);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);

                institutionJobs.Department = source.Department;

                var supervisors = new List<SupervisorsDtoProperty>();
                if (!string.IsNullOrEmpty(source.SupervisorId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.SupervisorId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJobSupervisor  supervisor Id: ", source.Id));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(source.AlternateSupervisorId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AlternateSupervisorId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var supervisorsDtoProperty = new SupervisorsDtoProperty()
                            {
                                Supervisor = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            supervisors.Add(supervisorsDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJobSupervisor Id: ", source.Id));
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;

            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Concat("An error occurred obtaining InstitutionJobSupervisor Id: ", source.Id, " ", ex.Message));
            }
            return institutionJobs;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Institution Job Supervisors.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetInstitutionJobSupervisorsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewInstitutionJobSupervisor);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Institution Job Supervisors.");
            }
        }

    }
}