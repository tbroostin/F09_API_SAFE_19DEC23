//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class InstitutionJobsService : BaseCoordinationService, IInstitutionJobsService
    {

        private readonly IPositionRepository _positionRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IInstitutionJobsRepository _institutionJobsRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> _employmentClassification;
        private IEnumerable<Domain.HumanResources.Entities.EmploymentDepartment> _employmentDepartments;
        private IEnumerable<Domain.HumanResources.Entities.JobChangeReason> _jobChangeReasons;
        private IEnumerable<Domain.HumanResources.Entities.PayClass> _payClasses;
        private IEnumerable<Domain.HumanResources.Entities.PayCycle2> _payCycles;

        private Dictionary<string, string> _employerGuidDictionary;

        public InstitutionJobsService(

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
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            this._positionRepository = positionRepository;
            this._hrReferenceDataRepository = hrReferenceDataRepository;
            this._referenceDataRepository = referenceDataRepository;
            this._personRepository = personRepository;
            this._institutionJobsRepository = institutionJobsRepository;
            _configurationRepository = configurationRepository;

            _employerGuidDictionary = new Dictionary<string, string>();
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all institution-jobs
        /// </summary>
        /// <returns>Collection of InstitutionJobs DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstitutionJobs>, int>> GetInstitutionJobsAsync(int offset, int limit,
            string person = "", string employer = "", string position = "", string department = "", string startOn = "",
            string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false)
        {
            try
            {
                //check permissions
                CheckGetInstitutionJobsPermission();

                string personCode = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {

                    personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personCode))
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                }

                string employerCode = string.Empty;
                if (!string.IsNullOrEmpty(employer))
                {

                    employerCode = await _personRepository.GetPersonIdFromGuidAsync(employer);
                    if (string.IsNullOrEmpty(employerCode))
                         return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                }

                var positionCode = string.Empty;
                if (!string.IsNullOrEmpty(position))
                {
                    try
                    {
                        positionCode = await _positionRepository.GetPositionIdFromGuidAsync(position);
                        if (string.IsNullOrEmpty(positionCode))
                           return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                    }
                    catch (KeyNotFoundException)
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                    }
                }

                var convertedStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                var convertedEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));

                //validate status
                if (!string.IsNullOrEmpty(status))
                {
                    status = status.ToLower();

                    if (status == "leave")
                    {
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                    }
                    if (status != "active" && status != "ended")
                    {
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                    }
                }

                var classificationCode = string.Empty;
                if (!string.IsNullOrEmpty(classification))
                {
                    var classifications = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classifications != null)
                    {
                        var employmentClassification = classifications.FirstOrDefault(x => x.Guid == classification);
                        if (employmentClassification != null)
                            classificationCode = employmentClassification.Code;
                    }
                    if (string.IsNullOrEmpty(classificationCode))
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                }

                //validate preference
                if (!string.IsNullOrEmpty(preference))
                {
                    if (preference.ToLower() != "primary")
                    {
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
                    }
                }

                var institutionJobsEntitiesTuple = await _institutionJobsRepository.GetInstitutionJobsAsync(offset, limit, personCode,
                    employerCode, positionCode, department, convertedStartOn, convertedEndOn, status, classificationCode,
                    preference, bypassCache);
                if (institutionJobsEntitiesTuple != null)
                {
                    var institutionJobsEntities = institutionJobsEntitiesTuple.Item1.ToList();
                    var totalCount = institutionJobsEntitiesTuple.Item2;

                    if (institutionJobsEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionJobs>();

                        foreach (var institutionJobsEntity in institutionJobsEntities)
                        {
                            institutionPositions.Add(await this.ConvertInstitutionJobsEntityToDtoAsync(institutionJobsEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionJobs>, int>(new List<Dtos.InstitutionJobs>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all institution-jobs
        /// </summary>
        /// <returns>Collection of InstitutionJobs DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>> GetInstitutionJobs2Async(int offset, int limit,
            string person = "", string employer = "", string position = "", string department = "", string startOn = "",
            string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false)
        {
            try
            {
                //check permissions
                CheckGetInstitutionJobsPermission();

                string personCode = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {

                    personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                }

                string employerCode = string.Empty;
                if (!string.IsNullOrEmpty(employer))
                {

                    employerCode = await _personRepository.GetPersonIdFromGuidAsync(employer);
                    if (string.IsNullOrEmpty(employerCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                }

                var positionCode = string.Empty;
                if (!string.IsNullOrEmpty(position))
                {
                    try
                    {
                        positionCode = await _positionRepository.GetPositionIdFromGuidAsync(position);
                        if (string.IsNullOrEmpty(positionCode))
                            // no results
                            return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                    }
                    catch (KeyNotFoundException)
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                    }
                }

                //var convertedStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                //var convertedEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));

                var convertedStartOn = string.Empty;
                var convertedEndOn = string.Empty;

                if (!(string.IsNullOrWhiteSpace(startOn)))
                {
                    convertedStartOn = await ConvertDateArgument(startOn);
                    if (string.IsNullOrEmpty(convertedStartOn))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                }

                if (!(string.IsNullOrWhiteSpace(endOn)))
                {
                    convertedEndOn = await ConvertDateArgument(endOn);
                    if (string.IsNullOrEmpty(convertedEndOn))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                }

                //validate status
                if (!string.IsNullOrEmpty(status))
                {
                    status = status.ToLower();

                    if (status == "leave")
                    {
                        // throw new ArgumentException("A status of leave may not be established using institution-jobs");
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                    }
                    if (status != "active" && status != "ended")
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                    }

                }

                var classificationCode = string.Empty;
                if (!string.IsNullOrEmpty(classification))
                {
                    var classifications = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classifications != null)
                    {
                        var employmentClassification = classifications.FirstOrDefault(x => x.Guid == classification);
                        if (employmentClassification != null)
                            classificationCode = employmentClassification.Code;
                    }
                    if (string.IsNullOrEmpty(classificationCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);

                }

                //validate preference
                if (!string.IsNullOrEmpty(preference))
                {
                    if (preference.ToLower() != "primary")
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
                    }
                }

                var institutionJobsEntitiesTuple = await _institutionJobsRepository.GetInstitutionJobsAsync(offset, limit, personCode,
                    employerCode, positionCode, department, convertedStartOn, convertedEndOn, status, classificationCode,
                    preference, bypassCache);
                if (institutionJobsEntitiesTuple != null)
                {
                    var institutionJobsEntities = institutionJobsEntitiesTuple.Item1.ToList();
                    var totalCount = institutionJobsEntitiesTuple.Item2;

                    if (institutionJobsEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionJobs2>();

                        foreach (var institutionJobsEntity in institutionJobsEntities)
                        {
                            institutionPositions.Add(await this.ConvertInstitutionJobsEntityToDto2Async(institutionJobsEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionJobs2>, int>(new List<Dtos.InstitutionJobs2>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all institution-jobs
        /// </summary>
        /// <returns>Collection of InstitutionJobs DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>> GetInstitutionJobs3Async(int offset, int limit,
            string person = "", string employer = "", string position = "", string department = "", string startOn = "",
            string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false, Dictionary<string, string> filterQualifiers = null)
        {
            try
            {
                //check permissions
                CheckGetInstitutionJobsPermission();

                string personCode = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                string employerCode = string.Empty;
                if (!string.IsNullOrEmpty(employer))
                {
                    employerCode = await _personRepository.GetPersonIdFromGuidAsync(employer);
                    if (string.IsNullOrEmpty(employerCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                var positionCode = string.Empty;
                if (!string.IsNullOrEmpty(position))
                {
                    try
                    {
                        positionCode = await _positionRepository.GetPositionIdFromGuidAsync(position);
                        if (string.IsNullOrEmpty(positionCode))
                            // no results
                            return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                    }
                    catch (KeyNotFoundException)
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                    }
                }

                //var convertedStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                //var convertedEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));

                var convertedStartOn = string.Empty;
                var convertedEndOn = string.Empty;

                if (!(string.IsNullOrWhiteSpace(startOn)))
                {
                    convertedStartOn = await ConvertDateArgument(startOn);
                    if (string.IsNullOrEmpty(convertedStartOn))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                if (!(string.IsNullOrWhiteSpace(endOn)))
                {
                    convertedEndOn = await ConvertDateArgument(endOn);
                    if (string.IsNullOrEmpty(convertedEndOn))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                //validate status
                if (!string.IsNullOrEmpty(status))
                {
                    status = status.ToLower();

                    if (status == "leave")
                    {
                        // throw new ArgumentException("A status of leave may not be established using institution-jobs");
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                    }
                    if (status != "active" && status != "ended")
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                    }
                }

                var classificationCode = string.Empty;
                if (!string.IsNullOrEmpty(classification))
                {
                    var classifications = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classifications != null)
                    {
                        var employmentClassification = classifications.FirstOrDefault(x => x.Guid == classification);
                        if (employmentClassification != null)
                            classificationCode = employmentClassification.Code;
                    }
                    if (string.IsNullOrEmpty(classificationCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                var departmentCode = string.Empty;
                if (!string.IsNullOrEmpty(department))
                {
                    var departments = await this.GetAllEmploymentDepartmentAsync(bypassCache);
                    if (departments != null)
                    {
                        var employmentDepartment = departments.FirstOrDefault(x => x.Guid == department);
                        if (employmentDepartment != null)
                            departmentCode = employmentDepartment.Code;
                    }
                    if (string.IsNullOrEmpty(departmentCode))
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                }

                //validate preference
                if (!string.IsNullOrEmpty(preference))
                {
                    if (preference.ToLower() != "primary")
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
                    }
                }

                var institutionJobsEntitiesTuple = await _institutionJobsRepository.GetInstitutionJobsAsync(offset, limit, personCode,
                    employerCode, positionCode, departmentCode, convertedStartOn, convertedEndOn, status, classificationCode,
                    preference, bypassCache, filterQualifiers);
                if (institutionJobsEntitiesTuple != null)
                {
                    var ids = new List<string>();

                    var institutionJobsEntities = institutionJobsEntitiesTuple.Item1.ToList();
                    var totalCount = institutionJobsEntitiesTuple.Item2;

                    ids.AddRange(institutionJobsEntities.Where(p => (!string.IsNullOrEmpty(p.PersonId)))
                        .Select(p => p.PersonId).Distinct().ToList());
                    ids.AddRange(institutionJobsEntities.Where(e => (!string.IsNullOrEmpty(e.Employer)))
                        .Select(e => e.Employer).Distinct().ToList());
                    ids.AddRange(institutionJobsEntities.Where(s => (!string.IsNullOrEmpty(s.SupervisorId)))
                        .Select(s => s.SupervisorId).Distinct().ToList());
                    ids.AddRange(institutionJobsEntities.Where(a => (!string.IsNullOrEmpty(a.AlternateSupervisorId)))
                        .Select(a => a.AlternateSupervisorId).Distinct().ToList());

                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                    if (institutionJobsEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionJobs3>();

                        foreach (var institutionJobsEntity in institutionJobsEntities)
                        {
                            institutionPositions.Add(await this.ConvertInstitutionJobsEntityToDto3Async(institutionJobsEntity, personGuidCollection, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionJobs3>, int>(new List<Dtos.InstitutionJobs3>(), 0);
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
        public async Task<Dtos.InstitutionJobs> GetInstitutionJobsByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Job.");
            }
            CheckGetInstitutionJobsPermission();
            try
            {
                var institutionJobsEntity = (await _institutionJobsRepository.GetInstitutionJobsByGuidAsync(guid));
                if (institutionJobsEntity == null)
                {
                    throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
                }
                return await ConvertInstitutionJobsEntityToDtoAsync(institutionJobsEntity);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstitutionJobs from its GUID
        /// </summary>
        /// <returns>InstitutionJobs DTO object</returns>
        public async Task<Dtos.InstitutionJobs2> GetInstitutionJobsByGuid2Async(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Job.");
            }
            CheckGetInstitutionJobsPermission();
            try
            {
                var institutionJobsEntity = (await _institutionJobsRepository.GetInstitutionJobsByGuidAsync(guid));
                if (institutionJobsEntity == null)
                {
                    throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
                }
                return await ConvertInstitutionJobsEntityToDto2Async(institutionJobsEntity);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstitutionJobs from its GUID
        /// </summary>
        /// <returns>InstitutionJobs DTO object</returns>
        public async Task<Dtos.InstitutionJobs3> GetInstitutionJobsByGuid3Async(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Job.");
            }
            CheckGetInstitutionJobsPermission();
            try
            {
                var institutionJobsEntity = (await _institutionJobsRepository.GetInstitutionJobsByGuidAsync(guid));
                if (institutionJobsEntity == null)
                {
                    throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
                }
                //lookup the guids for persons and employer
                var ids = new List<string>() { institutionJobsEntity.PersonId, institutionJobsEntity.Employer, institutionJobsEntity.SupervisorId, institutionJobsEntity.AlternateSupervisorId };
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);
                return await ConvertInstitutionJobsEntityToDto3Async(institutionJobsEntity, personGuidCollection, bypassCache);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Institution Job not found for GUID " + guid);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an InstitutionJobs domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="institutionJobsDto"><see cref="Dtos.InstitutionJobs3">InstitutionJobs</see></param>
        /// <returns><see cref="Dtos.InstitutionJobs3">InstitutionJobs</see></returns>
        /// <exception><see cref="ArgumentNullException">ArgumentNullException</see></exception>
        public async Task<InstitutionJobs3> PutInstitutionJobsAsync(Dtos.InstitutionJobs3 institutionJobsDto)
        {
            if (institutionJobsDto == null)
                throw new ArgumentNullException("institutionJobs", "Must provide a institutionJobs for update");
            if (string.IsNullOrEmpty(institutionJobsDto.Id))
                throw new ArgumentNullException("institutionJobs", "Must provide a guid for institutionJobs update");

            // get the person ID associated with the incoming guid
            var institutionJobsId = await _institutionJobsRepository.GetInstitutionJobsIdFromGuidAsync(institutionJobsDto.Id);

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(institutionJobsId))
            {
                try
                {
                    // verify the user has the permission to update a institutionJobs
                    CheckCreateInstitutionJobsPermission();

                    _institutionJobsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    // map the DTO to entities
                    var institutionJobsEntity
                        = await ConvertInstitutionJobs2DtoToEntityAsync(institutionJobsId, institutionJobsDto, false);

                    // update the entity in the database
                    var updatedInstitutionJobsEntity =
                        await _institutionJobsRepository.UpdateInstitutionJobsAsync(institutionJobsEntity);

                    //lookup the guids for persons and employer
                    var ids = new List<string>() { updatedInstitutionJobsEntity.PersonId, updatedInstitutionJobsEntity.Employer, updatedInstitutionJobsEntity.SupervisorId, updatedInstitutionJobsEntity.AlternateSupervisorId };
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);
                    return await ConvertInstitutionJobsEntityToDto3Async(updatedInstitutionJobsEntity, personGuidCollection, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await PostInstitutionJobsAsync(institutionJobsDto);
        }

        /// <summary>
        /// Create an institutionJobs.
        /// </summary>
        /// <param name="institutionJobs">The <see cref="InstitutionJobs">institutionJobs</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="InstitutionJobs3">institutionJobs</see></returns>
        public async Task<InstitutionJobs3> PostInstitutionJobsAsync(InstitutionJobs3 institutionJobsDto)
        {
            if (institutionJobsDto == null)
            {
                throw new ArgumentNullException("institutionJobsDto", "Must provide an institutionJobs for create");
            }

            if (string.IsNullOrEmpty(institutionJobsDto.Id))
            {
                throw new ArgumentNullException("institutionJobsDto", "Must provide a guid for institutionJobs create");
            }

            // verify the user has the permission to create an institutionJobs
            CheckCreateInstitutionJobsPermission();

            _institutionJobsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var institutionJobsEntity = await ConvertInstitutionJobs2DtoToEntityAsync(institutionJobsDto.Id, institutionJobsDto);
                var newEntity = await _institutionJobsRepository.CreateInstitutionJobsAsync(institutionJobsEntity);

                //lookup the guids for persons and employer
                var ids = new List<string>() { newEntity.PersonId, newEntity.Employer, newEntity.SupervisorId, newEntity.AlternateSupervisorId };
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);
                return await ConvertInstitutionJobsEntityToDto3Async(newEntity, personGuidCollection, false);

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Convert an InstitutionJobs2 Dto to an InstitutionJobs Entity
        /// </summary>
        /// <param name="institutionJobsId">guid</param>
        /// <param name="institutionJobs"><see cref="Dtos.InstitutionJobs3">InstitutionJobs</see></param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="Domain.HumanResources.Entities.InstitutionJobs source">InstitutionJobs</see></returns>
        private async Task<Domain.HumanResources.Entities.InstitutionJobs> ConvertInstitutionJobs2DtoToEntityAsync(string institutionJobsId, InstitutionJobs3 institutionJobs, bool bypassCache = false)
        {
            if (institutionJobs == null || string.IsNullOrEmpty(institutionJobs.Id))
                throw new ArgumentNullException("InstitutionJobs", "Must provide guid for an Institution Jobs");
            if (string.IsNullOrEmpty(institutionJobsId))
                throw new ArgumentNullException("InstitutionJobs", string.Concat("Must provide an id for Institution Job.  Guid: ", institutionJobs.Id));

            Domain.HumanResources.Entities.InstitutionJobs response = null;

            try
            {
                var hostCountry = await GetHostCountryAsync();
                var currencyIsoCode = CurrencyIsoCode.NotSet;
                //var currencyIsoCode = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                //    CurrencyIsoCode.USD;

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

                if ((institutionJobs.Person == null) || (string.IsNullOrEmpty(institutionJobs.Person.Id)))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Jobs.  Id:" + institutionJobsId);
                }

                if (institutionJobs.StartOn == default(DateTime))
                {
                    throw new ArgumentNullException("StartOn date is required for Institution Jobs.  Id:" + institutionJobsId);
                }

                var personId = await _personRepository.GetPersonIdFromGuidAsync(institutionJobs.Person.Id);

                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentNullException("Person not found for Id:" + institutionJobs.Person.Id);
                }

                if ((institutionJobs.Position == null) || (string.IsNullOrEmpty(institutionJobs.Position.Id)))
                {
                    throw new ArgumentNullException("Position ID is required for Institution Jobs. Id:" + institutionJobsId);
                }

                if ((institutionJobs.Status == null))
                {
                    throw new ArgumentNullException("Status is required for Institution Jobs. Id:" + institutionJobsId);
                }

                var position = await _positionRepository.GetPositionByGuidAsync(institutionJobs.Position.Id);
                if ((position == null) || (string.IsNullOrEmpty(position.Id)))
                {
                    throw new ArgumentNullException("Position not found for Id:" + position.Id);
                }


                response = new Domain.HumanResources.Entities.InstitutionJobs(institutionJobs.Id, institutionJobsId, personId, position.Id, institutionJobs.StartOn);

                if ((institutionJobs.Employer == null) || (string.IsNullOrEmpty(institutionJobs.Employer.Id)))
                {
                    throw new ArgumentNullException("Employer ID is required for Institution Jobs. Id:" + institutionJobsId);
                }
                var employer = await _institutionJobsRepository.GetInstitutionEmployerGuidAsync();
                response.Employer = employer;

                if (institutionJobs.Department != null && string.IsNullOrWhiteSpace(institutionJobs.Department.Id))
                {
                    throw new ArgumentNullException("Department is a required field. Position Id:" + position.Id);

                }

                if ((position != null) && (institutionJobs.Department != null ))
                {
                    var employDepartments = await this.GetAllEmploymentDepartmentAsync(bypassCache);
                    if (employDepartments != null)
                    {
                        var employDepartment = employDepartments.FirstOrDefault(ed => ed.Guid == institutionJobs.Department.Id);
                        if (employDepartment != null)
                        {
                            if (position.PositionDept != employDepartment.Code)
                            {
                                throw new ArgumentException("The department is invalid for this position. Position Id:" + position.Id);
                            }
                            response.Department = employDepartment.Code;
                        } else { throw new ArgumentException("The GUID specified for the department is not valid"); }
                    }
                } else
                {
                    if (institutionJobs.Department == null)
                        throw new ArgumentException("The department can not be missing");
                }

                if (institutionJobs.Status == InstitutionJobsStatus.Leave)
                {
                    throw new ArgumentException("A status of leave may not be established using institution-jobs");
                }

                if ((institutionJobs.EndOn != null) && (institutionJobs.EndOn.HasValue))
                {
                    response.EndDate = institutionJobs.EndOn;
                }
                else if (institutionJobs.Status == InstitutionJobsStatus.Ended)
                {
                    response.EndDate = DateTime.Now;
                }

                if ((institutionJobs.JobChangeReason != null) && (!string.IsNullOrEmpty(institutionJobs.JobChangeReason.Id)))
                {
                    var jobChangeReasons = await GetAllJobChangeReasonsAsync(bypassCache);
                    if (jobChangeReasons != null)
                    {
                        var jobChangeReason = jobChangeReasons.FirstOrDefault(jcr => jcr.Guid == institutionJobs.JobChangeReason.Id);
                        if (jobChangeReason != null)
                        {
                            response.EndReason = jobChangeReason.Code;
                        }
                    }
                }

                response.PerposwgItems = new List<Domain.HumanResources.Entities.PersonPositionWageItem>();

                if (institutionJobs.Salaries != null && institutionJobs.Salaries.Any())
                {
                    var personPositionWageItems = new List<Domain.HumanResources.Entities.PersonPositionWageItem>();

                    if (institutionJobs.Salaries.Count() == 1 && institutionJobs.Salaries[0].StartOn == null)
                    {
                        institutionJobs.Salaries[0].StartOn = institutionJobs.StartOn;
                    }
                    if (institutionJobs.Salaries.Count() > 1)
                    {
                        var duplicate = institutionJobs.Salaries.GroupBy(x => x.StartOn).Any(g => g.Count() > 1);
                        if (duplicate)
                        {
                            throw new ArgumentException("salary startOn dates must be unique.");
                        }
                    }
                    foreach (var salary in institutionJobs.Salaries)
                    {
                        if ((salary.SalaryAmount != null) && (salary.SalaryAmount.Rate != null))
                        {
                            var personPositionWageItem = new Domain.HumanResources.Entities.PersonPositionWageItem();

                            if (salary.SalaryAmount.Rate.Value != null && salary.SalaryAmount.Rate.Value.HasValue)
                            {
                                if (salary.SalaryAmount.Rate.Value < 0)
                                {
                                    throw new ArgumentException("Negative salary amounts are not permitted");
                                }
                                if (salary.SalaryAmount.Period == SalaryPeriod.Year)
                                {

                                    personPositionWageItem.HourlyOrSalary = "y";
                                    personPositionWageItem.PayRate = UnformatSalary(salary.SalaryAmount.Rate.Value);
                                }
                                else if (salary.SalaryAmount.Period == SalaryPeriod.Hour)
                                {
                                    personPositionWageItem.HourlyOrSalary = "h";
                                    personPositionWageItem.PayRate = UnformatSalary(salary.SalaryAmount.Rate.Value);
                                }
                                else if (salary.SalaryAmount.Period == SalaryPeriod.Contract)
                                {
                                    throw new ArgumentException("Salary amount period of contract cannot be added to this position");

                                }
                                if (salary.SalaryAmount.Rate.Currency == null)
                                {
                                    throw new ArgumentException("Salary Amount Rate Currency is invalid or missing.");
                                }
                                if (salary.SalaryAmount.Rate.Currency != currencyIsoCode)
                                {
                                    throw new ArgumentException("Only USD or CAD currency is supported, and must be valid for the host country.");
                                }

                            }
                            personPositionWageItem.StartDate = salary.StartOn;
                            personPositionWageItem.EndDate = salary.EndOn;
                            personPositionWageItem.Grade = salary.Grade;
                            personPositionWageItem.Step = salary.Step;

                            if (personPositionWageItems.Any(x => (personPositionWageItem.StartDate >= x.StartDate
                            && personPositionWageItem.StartDate <= x.EndDate) ||
                                (personPositionWageItem.EndDate >= x.StartDate
                                && personPositionWageItem.EndDate <= x.EndDate)))
                            {
                                throw new ArgumentException("The payload contains a salary date range that over laps each other.");
                            }

                            personPositionWageItems.Add(personPositionWageItem);
                        }
                    }
                    if (!personPositionWageItems.Any(x => x.StartDate == response.StartDate))
                    {
                        throw new ArgumentException("There is no matching Salary startOn that matches the root startOn of the payload");
                    }

                    if (personPositionWageItems.Count > 1)
                    {
                        personPositionWageItems.Sort((x, y) => x.StartDate.Value.CompareTo(y.StartDate.Value));

                        if (personPositionWageItems[0].EndDate == null)
                        {
                            throw new ArgumentException("The salary with startOn as " + personPositionWageItems[0].StartDate.Value.Date.ToString() + " requires a endOn date.");
                        }

                        DateTime? dateCheck = personPositionWageItems[0].EndDate;
                        foreach (var wage in personPositionWageItems)
                        {
                            if (wage.StartDate != response.StartDate)
                            {
                                if (dateCheck == null || dateCheck.Value.AddDays(1) != wage.StartDate)
                                {
                                    throw new ArgumentException("When having multiple salaries the endOn must be the day before the next salaries startOn.");
                                }
                                dateCheck = wage.EndDate;
                            }
                        }
                    }

                    response.PerposwgItems = personPositionWageItems;
                }

                if (institutionJobs.AccountingStringAllocations != null)
                {
                    
                    if (institutionJobs.AccountingStringAllocations.Count() == 1 && institutionJobs.AccountingStringAllocations[0].StartOn == null)
                    {
                        institutionJobs.AccountingStringAllocations[0].StartOn = institutionJobs.StartOn;
                    }
                    foreach (var accountingString in institutionJobs.AccountingStringAllocations)
                    {

                        //if we are missing a start on date for the accounting string and we only
                        //have one salary information then we'll populate the accountingString 
                        //start On.
                        if (accountingString.StartOn == null && response.PerposwgItems.Count == 1)
                        {
                            accountingString.StartOn = response.StartDate;
                        }

                        var personPositionWageItem = response.PerposwgItems.Where(x => x.StartDate == accountingString.StartOn).FirstOrDefault();
                        if (personPositionWageItem == null)
                        {
                            throw new Exception("accountingStringAllocations startOn must be associated with a corresponding salary startOn date.");
                        }

                        if(accountingString.EndOn != personPositionWageItem.EndDate)
                        {
                            throw new Exception("accountingStringAllocations endOn does not match the salary endOn.");
                        }

                        if (personPositionWageItem.AccountingStringAllocation == null)
                        {
                            personPositionWageItem.AccountingStringAllocation = new List<PpwgAccountingStringAllocation>();
                        }

                        var ppwgGlAssoc = new PpwgAccountingStringAllocation();

                        if (!(string.IsNullOrWhiteSpace(accountingString.AccountingString)))
                        {
                            ppwgGlAssoc.GlNumber = accountingString.AccountingString;
                        }
                        else
                        {
                            throw new Exception("The accountingString associated with start on " + accountingString.StartOn + " is required but was missing in the payload");
                        }
                        if (accountingString.AllocatedPercentage != null)
                        {
                            ppwgGlAssoc.GlPercentDistribution = accountingString.AllocatedPercentage;
                        }
                        else
                        {
                            throw new Exception("The allocatedPercentage associated with start on " + accountingString.StartOn + " is required but was missing in the payload");
                        }

                        personPositionWageItem.AccountingStringAllocation.Add(ppwgGlAssoc);
                    }

                }

                if ((institutionJobs.HoursPerPeriod != null) && (institutionJobs.HoursPerPeriod.Any()))
                {
                    foreach (var hoursPerPeriod in institutionJobs.HoursPerPeriod)
                    {

                        if (hoursPerPeriod.Period == Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod)
                        {
                            response.CycleWorkTimeUnits = "HRS";
                            response.CycleWorkTimeAmount = hoursPerPeriod.Hours;

                        }
                        else if (hoursPerPeriod.Period == Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year)
                        {
                            response.YearWorkTimeUnits = "HRS";
                            response.YearWorkTimeAmount = hoursPerPeriod.Hours;
                        }
                        else
                        {
                            throw new ArgumentException("HoursPerPeriod.period only supports enumerations for 'year' and/or 'payPeriod'");
                        }
                    }
                }

                if ((institutionJobs.FullTimeEquivalent != null) && (institutionJobs.FullTimeEquivalent.HasValue))
                {
                    response.FullTimeEquivalent = institutionJobs.FullTimeEquivalent;
                }
                if (institutionJobs.Preference != null && institutionJobs.Preference == JobPreference2.Primary)
                {
                    response.Primary = true;
                }


                if (institutionJobs.Classification != null && !(string.IsNullOrEmpty(institutionJobs.Classification.Id)))
                {
                    var classifications = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classifications != null)
                    {
                        var classification = classifications.FirstOrDefault(c => c.Guid == institutionJobs.Classification.Id);
                        if (classification != null)
                        {
                            response.Classification = classification.Code;
                        }
                        else
                        {
                            throw new ArgumentNullException("Classification not found for Id:" + institutionJobs.Classification.Id);
                        }
                    }
                }

                if ((institutionJobs.Supervisors != null) && (institutionJobs.Supervisors.Any()))
                {
                    foreach (var supervisor in institutionJobs.Supervisors)
                    {
                        var supervisorPositionId = await _personRepository.GetPersonIdFromGuidAsync(supervisor.Supervisor.Id);

                        if (string.IsNullOrEmpty(supervisorPositionId))
                        {
                            throw new ArgumentNullException("Supervisor not found for Id:" + supervisor.Supervisor.Id);
                        }
                        if (supervisor.Type == PositionReportsToType.Primary)
                        {
                            if (!(string.IsNullOrEmpty(response.SupervisorId)))
                            {
                                throw new ArgumentException("The institution job cannot contain two or more primary supervisors");
                            }
                            response.SupervisorId = supervisorPositionId;
                        }
                        else if (supervisor.Type == PositionReportsToType.Alternative)
                        {
                            if (!(string.IsNullOrEmpty(response.AlternateSupervisorId)))
                            {
                                throw new ArgumentException("The institution job cannot contain two or more alternate supervisors");
                            }
                            response.AlternateSupervisorId = supervisorPositionId;
                        }
                        else
                        {
                            throw new ArgumentNullException("Supervisor type not recognized: " + supervisor.Type);
                        }
                    }
                }

                if (institutionJobs.PayClass != null && !string.IsNullOrWhiteSpace(institutionJobs.PayClass.Id))
                {
                    var payClasses = await this.GetAllPayClassesAsync(bypassCache);
                    if (payClasses != null)
                    {
                        var payClass = payClasses.FirstOrDefault(c => c.Guid == institutionJobs.PayClass.Id);
                        if (payClass != null)
                        {
                            response.PayClass = payClass.Code;
                        }
                        else
                        {
                            throw new ArgumentException("The GUID specified for payClass is not valid");
                        }
                    }
                }

                if (institutionJobs.PayCycle != null && !string.IsNullOrWhiteSpace(institutionJobs.PayCycle.Id))
                {
                    var payCycles = await this.GetAllPayCycles2Async(bypassCache);
                    if (payCycles != null)
                    {
                        var payCycle = payCycles.FirstOrDefault(c => c.Guid == institutionJobs.PayCycle.Id);
                        if (payCycle != null)
                        {
                            response.PayCycle = payCycle.Code;
                        } else
                        {
                            throw new ArgumentException("The GUID specified for payCycle is not valid");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("An error occurred processing InstitutionJob Id: ", institutionJobsId, " ", ex.Message));
            }

            return response;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobs domain entity to its corresponding InstitutionJobs DTO
        /// </summary>
        /// <param name="source">InstitutionJobs domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobs DTO</returns>
        private async Task<Dtos.InstitutionJobs> ConvertInstitutionJobsEntityToDtoAsync(Domain.HumanResources.Entities.InstitutionJobs source, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobs();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Jobs.");
            }

            try
            {

                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Jobs.  Id:" + source.Id);
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
                    throw new ArgumentNullException("Position ID is required for Institution Jobs. Id:" + source.Id);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);


                institutionJobs.Department = source.Department;

                institutionJobs.StartOn = source.StartDate;
                institutionJobs.EndOn = source.EndDate;


                if (!(string.IsNullOrEmpty(source.EndReason)))
                {
                    var jobChangeReasons = await this.GetAllJobChangeReasonsAsync(bypassCache);
                    if (jobChangeReasons != null)
                    {
                        var jobChangeReason = jobChangeReasons.FirstOrDefault(jcr => jcr.Code == source.EndReason);
                        if (jobChangeReason != null)
                        {
                            institutionJobs.JobChangeReason = new GuidObject2(jobChangeReason.Guid);
                        }
                    }
                }
                var accountingStrings = new List<string>();
                if (source.AccountingStrings != null)
                {
                    foreach (var accountingString in source.AccountingStrings)
                    {
                        if (!(string.IsNullOrWhiteSpace(accountingString)))
                        {
                            accountingStrings.Add(accountingString.Replace("_", "-"));
                        }
                    }
                }
                if (accountingStrings.Any())
                {
                    institutionJobs.AccountingStrings = accountingStrings;
                }

                // If the PERPOS.END.DATE is null or the PERPOS.END.DATE is populated with a date that is on or after the request date.
                if ((!source.EndDate.HasValue) || (source.EndDate >= DateTime.Now))
                {
                    institutionJobs.Status = InstitutionJobsStatus.Active;
                }
                //The PERPOS.END.DATE is not null and is a date prior to the request date.
                else if (source.EndDate < DateTime.Now)
                {
                    institutionJobs.Status = InstitutionJobsStatus.Ended;
                }
                else
                {
                    throw new Exception("Unable to determine institution job status.  Id" + source.Id);
                }

                // Pay Status
                if (source.PayStatus != null)
                {
                    switch (source.PayStatus)
                    {
                        case Domain.HumanResources.Entities.PayStatus.PartialPay:
                            {
                                institutionJobs.PayStatus = Dtos.EnumProperties.PayStatus.PartialPay;
                                break;
                            }
                        case Domain.HumanResources.Entities.PayStatus.WithoutPay:
                            {
                                institutionJobs.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                                break;
                            }
                        case Domain.HumanResources.Entities.PayStatus.WithPay:
                            {
                                institutionJobs.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;
                                break;
                            }
                        default:
                            break;
                    }
                }

                // Benefits Status
                if (source.BenefitsStatus != null)
                {
                    switch (source.BenefitsStatus)
                    {
                        case Domain.HumanResources.Entities.BenefitsStatus.WithBenefits:
                            {
                                institutionJobs.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits;
                                break;
                            }
                        case Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits:
                            {
                                institutionJobs.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                                break;
                            }
                        default:
                            break;
                    }
                }

                var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                if ((source.CycleWorkTimeUnits == "HRS") && (source.CycleWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.CycleWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                    });
                }
                if ((source.YearWorkTimeUnits == "HRS") && (source.YearWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.YearWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                    });
                }
                institutionJobs.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;



                if ((source.FullTimeEquivalent != null) && (source.FullTimeEquivalent > 0))
                    institutionJobs.FullTimeEquivalent = source.FullTimeEquivalent;


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
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJob  supervisor Id: ", source.Id));
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
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJob Id: ", source.Id));
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;


                if (!(string.IsNullOrWhiteSpace(source.PayRate)))
                {
                    var salary = new SalaryDtoProperty();
                    if (source.Grade != "***")
                    { salary.Grade = source.Grade; }

                    if (source.Step != "***")
                    { salary.Step = source.Step; }

                    var hostCountry = source.HostCountry;
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
                            default:
                                throw new ArgumentException("Currency is only supported for 'USA' and 'CAD' ");
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("Host country not found.");
                    }

                    var amount = new Amount2DtoProperty { Currency = currencyIsoCode };
                    var salaryAmount = new SalaryAmountDtoProperty { Rate = amount };

                    if (source.IsSalary)
                    {
                        salaryAmount.Period = SalaryPeriod.Year;
                        amount.Value = FormatSalary(source.PayRate, true);
                    }
                    else
                    {
                        salaryAmount.Period = SalaryPeriod.Hour;
                        amount.Value = FormatSalary(source.PayRate, false);
                    }

                    salary.SalaryAmount = salaryAmount;
                    if (amount.Value != null)
                        institutionJobs.Salary = salary;
                }

                if (!(string.IsNullOrEmpty(source.Classification)))
                {
                    var classificaitons = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classificaitons != null)
                    {
                        var classification = classificaitons.FirstOrDefault(c => c.Code == source.Classification);
                        if (classification != null)
                        {
                            institutionJobs.Classification = new GuidObject2(classification.Guid);
                        }
                    }
                }

                if (source.Primary)
                    institutionJobs.Preference = JobPreference.Primary;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("An error occurred obtaining InstitutionJob Id: ", source.Id, " ", ex.Message));
            }
            return institutionJobs;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobs domain entity to its corresponding InstitutionJobs2 DTO
        /// </summary>
        /// <param name="source">InstitutionJobs domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobs2 DTO</returns>
        private async Task<Dtos.InstitutionJobs2> ConvertInstitutionJobsEntityToDto2Async(Domain.HumanResources.Entities.InstitutionJobs source, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobs2();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Jobs.");
            }

            try
            {
                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Jobs.  Id:" + source.Id);
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
                    throw new ArgumentNullException("Position ID is required for Institution Jobs. Id:" + source.Id);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);

                institutionJobs.Department = source.Department;

                institutionJobs.StartOn = source.StartDate;
                institutionJobs.EndOn = source.EndDate;

                if (!(string.IsNullOrEmpty(source.EndReason)))
                {
                    var jobChangeReasons = await this.GetAllJobChangeReasonsAsync(bypassCache);
                    if (jobChangeReasons != null)
                    {
                        var jobChangeReason = jobChangeReasons.FirstOrDefault(jcr => jcr.Code == source.EndReason);
                        if (jobChangeReason != null)
                        {
                            institutionJobs.JobChangeReason = new GuidObject2(jobChangeReason.Guid);
                        }
                    }
                }

                // If the PERPOS.END.DATE is null or the PERPOS.END.DATE is populated with a date that is on or after the request date.
                if ((!source.EndDate.HasValue) || (source.EndDate.Value.Date >= DateTime.Now.Date))
                {
                    institutionJobs.Status = InstitutionJobsStatus.Active;
                }
                //The PERPOS.END.DATE is not null and is a date prior to the request date.
                else if (source.EndDate < DateTime.Now)
                {
                    institutionJobs.Status = InstitutionJobsStatus.Ended;
                }
                else
                {
                    throw new Exception("Unable to determine institution job status.  Id" + source.Id);
                }

                var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                if ((source.CycleWorkTimeUnits == "HRS") && (source.CycleWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.CycleWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                    });
                }
                if ((source.YearWorkTimeUnits == "HRS") && (source.YearWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.YearWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                    });
                }
                institutionJobs.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;

                if ((source.FullTimeEquivalent != null) && (source.FullTimeEquivalent > 0))
                    institutionJobs.FullTimeEquivalent = source.FullTimeEquivalent;

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
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJob  supervisor Id: ", source.Id));
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
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJob Id: ", source.Id));
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;

                if (source.PerposwgItems != null && source.PerposwgItems.Any())
                {
                    var wages = source.PerposwgItems.GroupBy(x => x.recordkey).Select(x => x.First()).OrderBy(x => x.StartDate);
                    var currencyIsoCode = GetCurrencyIsoCode(source.HostCountry);
                    if (wages != null && wages.Any())
                    {
                        var salaries = new List<SalaryDtoProperty>();
                        var accountingStringAllocations = new List<AccountingStringAllocationsDtoProperty>();
                        foreach (var wage in wages)
                        {
                            if (!(string.IsNullOrWhiteSpace(wage.PayRate)))
                            {
                                var salary = new SalaryDtoProperty
                                {
                                    StartOn = wage.StartDate,
                                };
                                if (wage.Grade != "***")
                                    salary.Grade = wage.Grade;

                                if (wage.Step != "***")
                                    salary.Step = wage.Step;

                                if (wage.EndDate != default(DateTime))
                                {
                                    salary.EndOn = wage.EndDate;
                                }
                                var amount = new Amount2DtoProperty { Currency = currencyIsoCode };
                                var salaryAmount = new SalaryAmountDtoProperty { Rate = amount };

                                if (source.IsSalary)
                                {
                                    salaryAmount.Period = SalaryPeriod.Year;
                                    amount.Value = FormatSalary(wage.PayRate, true);
                                }
                                else
                                {
                                    salaryAmount.Period = SalaryPeriod.Hour;
                                    amount.Value = FormatSalary(wage.PayRate, false);
                                }
                                if ((salaryAmount.Rate != null) && (salaryAmount.Rate.Value != null)
                                    && (salaryAmount.Rate.Value.HasValue))
                                {
                                    salary.SalaryAmount = salaryAmount;
                                    salaries.Add(salary);
                                }

                                //    var perposwgItems = source.PerposwgItems.OrderBy(x => x.StartDate);

                                if (wage.AccountingStringAllocation != null && wage.AccountingStringAllocation.Any())
                                {
                                    foreach (var ppwgGlAssoc in wage.AccountingStringAllocation)
                                    {
                                        var accountingString = ppwgGlAssoc.GlNumber;

                                        if (!(string.IsNullOrEmpty(accountingString)))
                                        {
                                            var accountingStringAllocation = new AccountingStringAllocationsDtoProperty();
                                            accountingString = accountingString.Replace("_", "-");
                                            accountingStringAllocation.AccountingString = string.IsNullOrEmpty(ppwgGlAssoc.PpwgProjectsId) ?
                                                accountingString : string.Concat(accountingString, '*', ppwgGlAssoc.PpwgProjectsId);
                                            accountingStringAllocation.AllocatedPercentage = ppwgGlAssoc.GlPercentDistribution;
                                            accountingStringAllocation.StartOn = wage.StartDate;
                                            if (wage.EndDate != default(DateTime))
                                            {
                                                accountingStringAllocation.EndOn = wage.EndDate;
                                            }
                                            accountingStringAllocations.Add(accountingStringAllocation);

                                        }
                                    }
                                }

                            }
                        }
                        if ((salaries != null) && (salaries.Any()))
                        {
                            institutionJobs.Salaries = salaries;
                        }
                        if (accountingStringAllocations.Any())
                        {
                            institutionJobs.AccountingStringAllocations = accountingStringAllocations;
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(source.Classification)))
                {
                    var classificaitons = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classificaitons != null)
                    {
                        var classification = classificaitons.FirstOrDefault(c => c.Code == source.Classification);
                        if (classification != null)
                        {
                            institutionJobs.Classification = new GuidObject2(classification.Guid);
                        }
                    }
                }

                if (source.Primary)
                    institutionJobs.Preference = JobPreference.Primary;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("An error occurred obtaining InstitutionJob Id: ", source.Id, " ", ex.Message));
            }
            return institutionJobs;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstitutionJobs domain entity to its corresponding InstitutionJobs3 DTO
        /// </summary>
        /// <param name="source">InstitutionJobs domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>InstitutionJobs3 DTO</returns>
        private async Task<Dtos.InstitutionJobs3> ConvertInstitutionJobsEntityToDto3Async(Domain.HumanResources.Entities.InstitutionJobs source, Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            var institutionJobs = new Ellucian.Colleague.Dtos.InstitutionJobs3();
            if (source == null)
            {
                throw new ArgumentNullException("An unexpected error occurred extracting Institution Jobs.");
            }

            if (personGuidCollection == null)
            {
                throw new ArgumentNullException("personGuidCollection is null or empty.  An error occurred extracting person guids");
            }
            try
            {

                institutionJobs.Id = source.Guid;

                if (string.IsNullOrEmpty(source.PersonId))
                {
                    throw new ArgumentNullException("Person ID is required for Institution Jobs.  Id:" + source.Id);
                }
                //var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                //if (!(string.IsNullOrEmpty(personGuid)))
                //    institutionJobs.Person = new GuidObject2(personGuid);

                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                    throw new KeyNotFoundException(string.Concat("Person guid not found, PersonId: '", source.PersonId, "', Record ID: '", source.Id, "'"));
                }
                institutionJobs.Person = new GuidObject2(personGuid);
                institutionJobs.Employer = new GuidObject2(await _institutionJobsRepository.GetInstitutionEmployerGuidAsync()); 

                if (string.IsNullOrEmpty(source.PositionId))
                {
                    throw new ArgumentNullException("Position ID is required for Institution Jobs. Id:" + source.Id);
                }
                var positionGuid = await _positionRepository.GetPositionGuidFromIdAsync(source.PositionId);
                if (!(string.IsNullOrEmpty(positionGuid)))
                    institutionJobs.Position = new GuidObject2(positionGuid);


                if (!(string.IsNullOrWhiteSpace(source.Department))) 
                {
                    var employDepartments = await this.GetAllEmploymentDepartmentAsync(bypassCache);
                    if (employDepartments != null)
                    {
                        var employDepartment = employDepartments.FirstOrDefault(ed => ed.Code == source.Department);
                        if (employDepartment != null)
                        {
                            institutionJobs.Department = new GuidObject2(employDepartment.Guid);
                        }
                    }
                    // making sure change sticks
                    if (institutionJobs.Department == null) { throw new ArgumentNullException("Unable to locate department for code "+ source.Department + ", Position " + source.PositionId + " is required to have a department."); }

                }
                else
                {
                    throw new ArgumentNullException("Position " + source.PositionId + " is required to have a department.");
                }
               
                institutionJobs.StartOn = source.StartDate;
                institutionJobs.EndOn = source.EndDate;

                if (!(string.IsNullOrEmpty(source.EndReason)))
                {
                    var jobChangeReasons = await this.GetAllJobChangeReasonsAsync(bypassCache);
                    if (jobChangeReasons != null)
                    {
                        var jobChangeReason = jobChangeReasons.FirstOrDefault(jcr => jcr.Code == source.EndReason);
                        if (jobChangeReason != null)
                        {
                            institutionJobs.JobChangeReason = new GuidObject2(jobChangeReason.Guid);
                        }
                    }
                }

                // If the PERPOS.END.DATE is null or the PERPOS.END.DATE is populated with a date that is on or after the request date.
                if ((!source.EndDate.HasValue) || (source.EndDate.Value.Date >= DateTime.Now.Date))
                {
                    institutionJobs.Status = InstitutionJobsStatus.Active;
                }
                //The PERPOS.END.DATE is not null and is a date prior to the request date.
                else if (source.EndDate < DateTime.Now)
                {
                    institutionJobs.Status = InstitutionJobsStatus.Ended;
                }
                else
                {
                    throw new Exception("Unable to determine institution job status.  Id" + source.Id);
                }

                var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                if ((source.CycleWorkTimeUnits == "HRS") && (source.CycleWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.CycleWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                    });
                }
                if ((source.YearWorkTimeUnits == "HRS") && (source.YearWorkTimeAmount.HasValue))
                {
                    hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                    {
                        Hours = source.YearWorkTimeAmount,
                        Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                    });
                }
                institutionJobs.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;

                if ((source.FullTimeEquivalent != null) && (source.FullTimeEquivalent > 0))
                    institutionJobs.FullTimeEquivalent = source.FullTimeEquivalent;

                var supervisors = new List<SupervisorsDtoProperty>();
                if (!string.IsNullOrEmpty(source.SupervisorId))
                {
                    try
                    {
                        //var supervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.SupervisorId);
                        var supervisorPositionGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.SupervisorId, out supervisorPositionGuid);
                        if (string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            throw new KeyNotFoundException(string.Concat("Person guid not found, SupervisorId: '", source.SupervisorId, "', Record ID: '", source.Id, "'"));
                        }
                        
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
                            logger.Error(ex, string.Concat("An error occurred obtaining InstitutionJob  supervisor Id: ", source.Id));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(source.AlternateSupervisorId))
                {
                    try
                    {
                        //var altSupervisorPositionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AlternateSupervisorId);
                        var altSupervisorPositionGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.AlternateSupervisorId, out altSupervisorPositionGuid);
                        if (string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            throw new KeyNotFoundException(string.Concat("Person guid not found, AlternateSupervisorId: '", source.AlternateSupervisorId, "', Record ID: '", source.Id, "'"));
                        }
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
                            logger.Error(ex, string.Concat("An error occurred obtaining alt supervisor InstitutionJob Id: ", source.Id));
                        }
                    }
                }
                if (supervisors.Any())
                    institutionJobs.Supervisors = supervisors;

                if (source.PerposwgItems != null && source.PerposwgItems.Any())
                {
                    var wages = source.PerposwgItems.GroupBy(x => x.recordkey).Select(x => x.First()).OrderBy(x => x.StartDate);
                    var currencyIsoCode = GetCurrencyIsoCode(source.HostCountry);
                    if (wages != null && wages.Any())
                    {
                        var salaries = new List<SalaryDtoProperty>();
                        var accountingStringAllocations = new List<AccountingStringAllocationsDtoProperty>();
                        foreach (var wage in wages)
                        {
                            if (!(string.IsNullOrWhiteSpace(wage.PayRate)))
                            {
                                var salary = new SalaryDtoProperty
                                {
                                    StartOn = wage.StartDate,
                                };
                                if (wage.Grade != "***")
                                    salary.Grade = wage.Grade;

                                if (wage.Step != "***")
                                    salary.Step = wage.Step;

                                if (wage.EndDate != default(DateTime))
                                {
                                    salary.EndOn = wage.EndDate;
                                }
                                var amount = new Amount2DtoProperty { Currency = currencyIsoCode };
                                var salaryAmount = new SalaryAmountDtoProperty { Rate = amount };

                                if (source.IsSalary)
                                {
                                    salaryAmount.Period = SalaryPeriod.Year;
                                    amount.Value = FormatSalary(wage.PayRate, true);
                                }
                                else
                                {
                                    salaryAmount.Period = SalaryPeriod.Hour;
                                    amount.Value = FormatSalary(wage.PayRate, false);
                                }
                                if ((salaryAmount.Rate != null) && (salaryAmount.Rate.Value != null)
                                    && (salaryAmount.Rate.Value.HasValue))
                                {
                                    salary.SalaryAmount = salaryAmount;
                                    salaries.Add(salary);
                                

                                //    var perposwgItems = source.PerposwgItems.OrderBy(x => x.StartDate);

                                    if (wage.AccountingStringAllocation != null && wage.AccountingStringAllocation.Any())
                                    {
                                        foreach (var ppwgGlAssoc in wage.AccountingStringAllocation)
                                        {
                                            var accountingString = ppwgGlAssoc.GlNumber;

                                            if (!(string.IsNullOrEmpty(accountingString)))
                                            {
                                                var accountingStringAllocation = new AccountingStringAllocationsDtoProperty();
                                                accountingString = accountingString.Replace("_", "-");
                                                accountingStringAllocation.AccountingString = string.IsNullOrEmpty(ppwgGlAssoc.PpwgProjectsId) ?
                                               accountingString : string.Concat(accountingString, '*', ppwgGlAssoc.PpwgProjectsId);
                                                accountingStringAllocation.AllocatedPercentage = ppwgGlAssoc.GlPercentDistribution;
                                                accountingStringAllocation.StartOn = wage.StartDate;
                                                if (wage.EndDate != default(DateTime))
                                                {
                                                    accountingStringAllocation.EndOn = wage.EndDate;
                                                }
                                                accountingStringAllocations.Add(accountingStringAllocation);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if ((salaries != null) && (salaries.Any()))
                        {
                            institutionJobs.Salaries = salaries;
                        }
                        if (accountingStringAllocations.Any())
                        {
                            institutionJobs.AccountingStringAllocations = accountingStringAllocations;
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(source.Classification)))
                {
                    var classificaitons = await this.GetAllEmploymentClassificationAsync(bypassCache);
                    if (classificaitons != null)
                    {
                        var classification = classificaitons.FirstOrDefault(c => c.Code == source.Classification);
                        if (classification != null)
                        {
                            institutionJobs.Classification = new GuidObject2(classification.Guid);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(source.PayClass)))
                {
                    var payClasses = await this.GetAllPayClassesAsync(bypassCache);
                    if (payClasses != null)
                    {
                        var payClass = payClasses.FirstOrDefault(c => c.Code == source.PayClass);
                        if (payClass != null)
                        {
                            institutionJobs.PayClass = new GuidObject2(payClass.Guid);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(source.PayCycle)))
                {
                    var payCycles = await this.GetAllPayCycles2Async(bypassCache);
                    if (payCycles != null)
                    {
                        var payCycle = payCycles.FirstOrDefault(c => c.Code == source.PayCycle);
                        if (payCycle != null)
                        {
                            institutionJobs.PayCycle = new GuidObject2(payCycle.Guid);
                        }
                    }
                }
                if (source.Primary)
                    institutionJobs.Preference = JobPreference2.Primary;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("An error occurred obtaining InstitutionJob Id: ", source.Id, " ", ex.Message));
            }
            return institutionJobs;
        }

        private static CurrencyIsoCode GetCurrencyIsoCode(string hostCountry)
        {
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
                    default:
                        throw new ArgumentException("Currency is only supported for 'USA' and 'CAD' ");
                }
            }
            else
            {
                throw new ArgumentNullException("Host country not found.");
            }

            return currencyIsoCode;
        }

        /// <summary>
        /// Helper method to format salary 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="isSalary"></param>
        /// <returns></returns>
        private decimal? FormatSalary(string amount, bool isSalary)
        {
            if (string.IsNullOrWhiteSpace(amount)) return null;
            try
            {
                var paddedValue = amount == "0" ? amount: isSalary ? amount.Insert(amount.Length - 2, ".") : amount.Insert(amount.Length - 4, ".");
                return Convert.ToDecimal(paddedValue);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to unformat salary 
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private string UnformatSalary(decimal? amount)
        {
            if (!(amount.HasValue)) return null;

            return amount.ToString(); //.Replace(".", "");
        }


        /// <summary>
        /// Helper method to determine if the user has permission to view Institution Jobs.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetInstitutionJobsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewInstitutionJob);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Institution Jobs.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update Institution Jobs.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateInstitutionJobsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.CreateInstitutionJob);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create or update Institution Jobs.");
            }
        }

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _referenceDataRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        /// <summary>
        /// Get all JobChangeReasons Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.JobChangeReason>> GetAllJobChangeReasonsAsync(bool bypassCache)
        {
            if (_jobChangeReasons == null)
            {
                _jobChangeReasons = await _hrReferenceDataRepository.GetJobChangeReasonsAsync(bypassCache);
            }
            return _jobChangeReasons;
        }

        /// <summary>
        /// Get all EmploymentClassification Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentClassification>> GetAllEmploymentClassificationAsync(bool bypassCache)
        {
            if (_employmentClassification == null)
            {
                _employmentClassification = await _hrReferenceDataRepository.GetEmploymentClassificationsAsync(bypassCache);
            }
            return _employmentClassification;
        }

        /// <summary>
        /// Get all EmploymentDepartments Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentDepartment>> GetAllEmploymentDepartmentAsync(bool bypassCache)
        {
            if (_employmentDepartments == null)
            {
                _employmentDepartments = await _hrReferenceDataRepository.GetEmploymentDepartmentsAsync(bypassCache);
            }
            return _employmentDepartments;
        }

        /// <summary>
        /// Get all PayClass Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.PayClass>> GetAllPayClassesAsync(bool bypassCache)
        {
            if (_payClasses == null)
            {
                _payClasses = await _hrReferenceDataRepository.GetPayClassesAsync(bypassCache);
            }
            return _payClasses;
        }

        /// <summary>
        /// Get all PayCycle2 Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.HumanResources.Entities.PayCycle2>> GetAllPayCycles2Async(bool bypassCache)
        {
            if (_payCycles == null)
            {
                _payCycles = await _hrReferenceDataRepository.GetPayCyclesAsync(bypassCache);
                foreach(var payCycle in _payCycles)
                {

                }
            }
            return _payCycles;
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