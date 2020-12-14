﻿//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAptitudeAssessmentsService : StudentCoordinationService, IStudentAptitudeAssessmentsService
    {
        private readonly IStudentTestScoresRepository _studentAptitudeAssessmentsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAptitudeAssessmentsRepository _aptitudeAssessmentsRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentAptitudeAssessmentsService(
            IStudentTestScoresRepository studentAptitudeAssessmentsRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAptitudeAssessmentsRepository aptitudeAssessmentsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStudentRepository studentRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _personRepository = personRepository;
            _studentAptitudeAssessmentsRepository = studentAptitudeAssessmentsRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _aptitudeAssessmentsRepository = aptitudeAssessmentsRepository;
        }


        private IEnumerable<Domain.Student.Entities.NonCourse> _appitudeAssessments = null;
        private async Task<IEnumerable<Domain.Student.Entities.NonCourse>> GetAptitudeAssessmentAsync(bool bypassCache = false)
        {
            if (_appitudeAssessments == null)
            {
                _appitudeAssessments = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsAsync(bypassCache);
            }
            return _appitudeAssessments;
        }

        private IEnumerable<Domain.Student.Entities.AssessmentSpecialCircumstance> _assessmentSpecialCircumstances = null;
        private async Task<IEnumerable<Domain.Student.Entities.AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool bypassCache = false)
        {
            if (_assessmentSpecialCircumstances == null)
            {
                _assessmentSpecialCircumstances = await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesAsync(bypassCache);
            }
            return _assessmentSpecialCircumstances;
        }
        private IEnumerable<Domain.Student.Entities.IntgTestPercentileType> _assesmentPercentileTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.IntgTestPercentileType>> GetAssesmentPercentileTypesAsync(bool bypassCache = false)
        {
            if (_assesmentPercentileTypes == null)
            {
                _assesmentPercentileTypes = await _studentReferenceDataRepository.GetIntgTestPercentileTypesAsync(bypassCache);
            }
            return _assesmentPercentileTypes;
        }
        private IEnumerable<Domain.Student.Entities.TestSource> _testSource = null;
        private async Task<IEnumerable<Domain.Student.Entities.TestSource>> GetTestSourcesAsync(bool bypassCache = false)
        {
            if (_testSource == null)
            {
                _testSource = await _studentReferenceDataRepository.GetTestSourcesAsync(bypassCache);
            }
            return _testSource;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>,int>> GetStudentAptitudeAssessmentsAsync(int offset, int limit, bool bypassCache = false)
        {
            var studentAptitudeAssessmentsCollection = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>();
            CheckGetStudentAptitudeAssessmentsPermission();
            var studentAptitudeAssessmentsEntitiesTuple = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresAsync("", offset, limit, bypassCache); if (studentAptitudeAssessmentsEntitiesTuple != null )
            {
                var studentAptitudeAssessmentsEntities = studentAptitudeAssessmentsEntitiesTuple.Item1;
                var totalCount = studentAptitudeAssessmentsEntitiesTuple.Item2;
                if (studentAptitudeAssessmentsEntities != null && studentAptitudeAssessmentsEntities.Any())
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>,int>(await ConvertStudentTestScoresEntityToDtoCollectionAsync(studentAptitudeAssessmentsEntities.ToList(), bypassCache), totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>,int>(new List<Dtos.StudentAptitudeAssessments>(),0);
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(new List<Dtos.StudentAptitudeAssessments>(), 0);
            }           
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>, int>> GetStudentAptitudeAssessments2Async(string studentFilter, int offset, int limit, bool bypassCache = false)
        {
            var studentAptitudeAssessmentsCollection = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>();
            CheckGetStudentAptitudeAssessmentsPermission();
            string studentId = string.Empty;
            if (!string.IsNullOrEmpty(studentFilter))
            {
                try
                {
                    studentId = await _personRepository.GetPersonIdFromGuidAsync(studentFilter);
                    if (string.IsNullOrEmpty(studentId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(new List<Dtos.StudentAptitudeAssessments>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(new List<Dtos.StudentAptitudeAssessments>(), 0);
                }
            }
            var studentAptitudeAssessmentsEntitiesTuple = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresAsync(studentId, offset, limit, bypassCache);
            if (studentAptitudeAssessmentsEntitiesTuple != null)
            {
                var studentAptitudeAssessmentsEntities = studentAptitudeAssessmentsEntitiesTuple.Item1;
                var totalCount = studentAptitudeAssessmentsEntitiesTuple.Item2;
                if (studentAptitudeAssessmentsEntities != null && studentAptitudeAssessmentsEntities.Any())
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(await ConvertStudentTestScoresEntityToDtoCollection2Async(studentAptitudeAssessmentsEntities.ToList(), bypassCache), totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(new List<Dtos.StudentAptitudeAssessments>(), 0);
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments>, int>(new List<Dtos.StudentAptitudeAssessments>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="studentFilter">student ID for filtering</param>
        /// <param name="assessmentFilter">assessment ID for filtering</param>
        /// <param name="personFilter">personFilter ID for named query filtering</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>        
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>, int>> GetStudentAptitudeAssessments3Async(string studentFilter,
            string assessmentFilter, string personFilter, int offset, int limit, bool bypassCache = false)
        {
            CheckGetStudentAptitudeAssessmentsPermission();

            var studentAptitudeAssessmentsCollection = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>();

            #region filters
            var studentId = string.Empty;
            var assessmentId = string.Empty;
            string[] filterPersonIds = new List<string>().ToArray();

            if (!string.IsNullOrEmpty(studentFilter))
            {
                try
                {
                    studentId = await _personRepository.GetPersonIdFromGuidAsync(studentFilter);

                    if (string.IsNullOrEmpty(studentId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                }
            }

            if (!string.IsNullOrEmpty(assessmentFilter))
            {
                try
                {
                    assessmentId = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsIdFromGuidAsync(assessmentFilter);

                    if (string.IsNullOrEmpty(assessmentId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                }
            }


            //convert person filter named query.
            if (!string.IsNullOrEmpty(personFilter))
            {
                try
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                    if (personFilterKeys != null)
                    {
                        filterPersonIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                }
            }
            #endregion

            var studentAptitudeAssessmentsEntitiesTuple = await _studentAptitudeAssessmentsRepository.GetStudentTestScores2Async(offset, limit, studentId,
                assessmentId, filterPersonIds, personFilter, bypassCache);
            if (studentAptitudeAssessmentsEntitiesTuple != null)
            {
                var studentAptitudeAssessmentsEntities = studentAptitudeAssessmentsEntitiesTuple.Item1;
                var totalCount = studentAptitudeAssessmentsEntitiesTuple.Item2;
                if (studentAptitudeAssessmentsEntities != null && studentAptitudeAssessmentsEntities.Any())
                {
                  
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(await ConvertStudentTestScoresEntityToDtoCollection3Async(studentAptitudeAssessmentsEntities.ToList(), bypassCache), totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.StudentAptitudeAssessments2>, int>(new List<Dtos.StudentAptitudeAssessments2>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid", "GUID is required to get an Student Aptitude Assessments.");
                }
                CheckGetStudentAptitudeAssessmentsPermission();

                var studentAptitudeAssessment = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(guid);
                if (studentAptitudeAssessment == null)
                {
                    throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid);
                }
                return await ConvertStudentTestScoresEntityToDtoAsync(studentAptitudeAssessment, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuid2Async(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid", "GUID is required to get an Student Aptitude Assessments.");
                }
                CheckGetStudentAptitudeAssessmentsPermission();

                var studentAptitudeAssessment = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(guid);
                if (studentAptitudeAssessment == null)
                {
                    throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid);
                }
                return await ConvertStudentTestScoresEntityToDto2Async(studentAptitudeAssessment, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2> GetStudentAptitudeAssessmentsByGuid3Async(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid", "GUID is required to get an Student Aptitude Assessments.");
                }
                CheckGetStudentAptitudeAssessmentsPermission();

                var studentAptitudeAssessment = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(guid);
                if (studentAptitudeAssessment == null)
                {
                    throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid);
                }

                var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentAptitudeAssessment.StudentId });

                return await ConvertStudentTestScoresEntityToDto2Async(studentAptitudeAssessment, personGuidCollection, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-aptitude-assessments not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update a StudentAptitudeAssessments.
        /// </summary>
        /// <param name="StudentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<StudentAptitudeAssessments> UpdateStudentAptitudeAssessmentsAsync(StudentAptitudeAssessments studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a StudentAptitudeAssessments for update");
            if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a guid for StudentAptitudeAssessments update");
            if ((studentAptitudeAssessments.Form != null && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)) || (studentAptitudeAssessments.Form != null && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)))
                throw new ArgumentException("StudentAptitudeAssessments", "Both form name and form number must be provided or excluded for StudentAptitudeAssessments.");

            // get the ID associated with the incoming guid
            var studentAptitudeAssessmentsEntityId = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresIdFromGuidAsync(studentAptitudeAssessments.Id);
            if (!string.IsNullOrEmpty(studentAptitudeAssessmentsEntityId))
            {
                // verify the user has the permission to update a studentAptitudeAssessments
                this.CheckCreateStudentAptitudeAssessmentsPermission();

                // pass down the extended data dictionary
                _studentAptitudeAssessmentsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var studentAptitudeAssessmentsEntity
                    = await ConvertStudentAptitudeAssessmentsDtoToEntityAsync(studentAptitudeAssessmentsEntityId, studentAptitudeAssessments);

                    // update the entity in the database
                    var updatedStudentAptitudeAssessmentsEntity =
                        await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(studentAptitudeAssessmentsEntity);

                    //Dictionary<string, string> personGuidCollection = null;
                    //if (studentAptitudeAssessmentsEntity != null)
                    //{
                    //    personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentAptitudeAssessmentsEntity.StudentId });
                    //}

                    return await this.ConvertStudentTestScoresEntityToDtoAsync(updatedStudentAptitudeAssessmentsEntity,  true);
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await CreateStudentAptitudeAssessmentsAsync(studentAptitudeAssessments);
        }


        /// <summary>
        /// Update a StudentAptitudeAssessments.
        /// </summary>
        /// <param name="StudentAptitudeAssessments">The <see cref="StudentAptitudeAssessments2">studentAptitudeAssessments</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentAptitudeAssessments2">studentAptitudeAssessments</see></returns>
        public async Task<StudentAptitudeAssessments2> UpdateStudentAptitudeAssessments2Async(StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a StudentAptitudeAssessments for update");
            if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a guid for StudentAptitudeAssessments update");
            if ((studentAptitudeAssessments.Form != null && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)) || (studentAptitudeAssessments.Form != null && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)))
                throw new ArgumentException("StudentAptitudeAssessments", "Both form name and form number must be provided or excluded for StudentAptitudeAssessments.");

            // get the ID associated with the incoming guid
            var studentAptitudeAssessmentsEntityId = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresIdFromGuidAsync(studentAptitudeAssessments.Id);
            if (!string.IsNullOrEmpty(studentAptitudeAssessmentsEntityId))
            {
                // verify the user has the permission to update a studentAptitudeAssessments
                this.CheckCreateStudentAptitudeAssessmentsPermission();

                // pass down the extended data dictionary
                _studentAptitudeAssessmentsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var studentAptitudeAssessmentsEntity
                    = await ConvertStudentAptitudeAssessments2DtoToEntityAsync(studentAptitudeAssessmentsEntityId, studentAptitudeAssessments);

                    // update the entity in the database
                    var updatedStudentAptitudeAssessmentsEntity =
                        await _studentAptitudeAssessmentsRepository.UpdateStudentTestScoresAsync(studentAptitudeAssessmentsEntity);

                    Dictionary<string, string> personGuidCollection = null;
                    if (studentAptitudeAssessmentsEntity != null)
                    {
                        personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentAptitudeAssessmentsEntity.StudentId });
                    }

                    return await this.ConvertStudentTestScoresEntityToDto2Async(updatedStudentAptitudeAssessmentsEntity, personGuidCollection, true);
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await CreateStudentAptitudeAssessments2Async(studentAptitudeAssessments);
        }


        /// <summary>
        /// Create a StudentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<StudentAptitudeAssessments> CreateStudentAptitudeAssessmentsAsync(StudentAptitudeAssessments studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a StudentAptitudeAssessments for create");
            if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a guid for StudentAptitudeAssessments create");

            if (studentAptitudeAssessments.Status != null &&  studentAptitudeAssessments.Status != StudentAptitudeAssessmentsStatus.NotSet)
            {
                throw new ArgumentException("status", "Status can not be set on StudentAptitudeAssessments create");
            }

            if ((studentAptitudeAssessments.Form != null && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)) || (studentAptitudeAssessments.Form != null && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)))
                throw new ArgumentException("StudentAptitudeAssessments", "Both form name and form number must be provided or excluded for StudentAptitudeAssessments.");

            // verify the user has the permission to create a studentAptitudeAssessments
            this.CheckCreateStudentAptitudeAssessmentsPermission();

            // pass down the extended data dictionary
            _studentAptitudeAssessmentsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {

                var studentAptitudeAssessmentsEntity
                         = await ConvertStudentAptitudeAssessmentsDtoToEntityAsync(string.Empty, studentAptitudeAssessments);

                // create a studentAptitudeAssessments entity in the database
                var createdStudentTestScores =
                    await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(studentAptitudeAssessmentsEntity);

                //Dictionary<string, string> personGuidCollection = null;
                //if (studentAptitudeAssessmentsEntity != null)
                //{
                //    personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentAptitudeAssessmentsEntity.StudentId });
                //}
                // return the newly created studentAptitudeAssessments
                return await this.ConvertStudentTestScoresEntityToDtoAsync(createdStudentTestScores, true);

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

        /// <summary>
        /// Create a StudentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        public async Task<StudentAptitudeAssessments2> CreateStudentAptitudeAssessments2Async(StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a StudentAptitudeAssessments for create");
            if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
                throw new ArgumentNullException("StudentAptitudeAssessments", "Must provide a guid for StudentAptitudeAssessments create");

            if (studentAptitudeAssessments.Status != null && studentAptitudeAssessments.Status != StudentAptitudeAssessmentsStatus.NotSet)
            {
                throw new ArgumentException("status", "Status can not be set on StudentAptitudeAssessments create");
            }

            if ((studentAptitudeAssessments.Form != null && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)) || (studentAptitudeAssessments.Form != null && !string.IsNullOrEmpty(studentAptitudeAssessments.Form.Name) && string.IsNullOrEmpty(studentAptitudeAssessments.Form.Number)))
                throw new ArgumentException("StudentAptitudeAssessments", "Both form name and form number must be provided or excluded for StudentAptitudeAssessments.");

            // verify the user has the permission to create a studentAptitudeAssessments
            this.CheckCreateStudentAptitudeAssessmentsPermission();

            // pass down the extended data dictionary
            _studentAptitudeAssessmentsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {

                var studentAptitudeAssessmentsEntity
                         = await ConvertStudentAptitudeAssessments2DtoToEntityAsync(string.Empty, studentAptitudeAssessments);

                // create a studentAptitudeAssessments entity in the database
                var createdStudentTestScores =
                    await _studentAptitudeAssessmentsRepository.CreateStudentTestScoresAsync(studentAptitudeAssessmentsEntity);

                Dictionary<string, string> personGuidCollection = null;
                if (studentAptitudeAssessmentsEntity != null)
                {
                    personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentAptitudeAssessmentsEntity.StudentId });
                }
                // return the newly created studentAptitudeAssessments
                return await this.ConvertStudentTestScoresEntityToDto2Async(createdStudentTestScores, personGuidCollection, true);

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

        /// <summary>
        /// Deletes student aptitude assessment
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task DeleteStudentAptitudeAssessmentAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id", "Student aptitude assessment id is required to delete.");
            }
            if (!await CheckDeleteStudentAptitudeAssessmentsPermission())
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to delete student aptitude assessment.");
            }
            try
            {
                var studentAptitudeAssessment = await _studentAptitudeAssessmentsRepository.GetStudentTestScoresByGuidAsync(guid);
                if (studentAptitudeAssessment == null)
                {
                    throw new KeyNotFoundException();
                }
                await _studentAptitudeAssessmentsRepository.DeleteAsync(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Student-aptitude-assessments not found for guid: '{0}'.", guid));
            }
        }

        /// <summary>
        /// Convert StudentAptitudeAssessments dto to StudentTestScores domain entity 
        /// </summary>
        /// <param name="studentTestScoresId">StudentTestScore id</param>
        /// <param name="studentAptitudeAssessmentsDto"><see cref="StudentAptitudeAssessments">StudentAptitudeAssessments dto</param>
        /// <returns><see cref="StudentTestScores">StudentTestScores domain entity</returns>
        private async Task<StudentTestScores> ConvertStudentAptitudeAssessmentsDtoToEntityAsync(string studentTestScoresId, StudentAptitudeAssessments studentAptitudeAssessmentsDto)
        {
            if (studentAptitudeAssessmentsDto == null)
            {
                throw new ArgumentNullException("studentAptitudeAssessmentsDto", "studentAptitudeAssessmentsDto is a required field");
            }

            var aptitudeCode = string.Empty;
            try
            {
                if (studentAptitudeAssessmentsDto.Student == null || string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Student.Id))
                {
                    throw new ArgumentNullException("student.id", "student.id is a required field");
                }
                var studentId = await _personRepository.GetPersonIdFromGuidAsync(studentAptitudeAssessmentsDto.Student.Id);
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentException(string.Format("Invalid student.id '{0}'.", studentAptitudeAssessmentsDto.Student.Id), "student.id");
                }
                if (await _personRepository.IsCorpAsync(studentId))
                {
                    throw new ArgumentException(string.Format("Invalid student.id '{0}'. An assessment cannot be assigned to a corporation.", studentAptitudeAssessmentsDto.Student.Id), "student.id");
                }

                if (studentAptitudeAssessmentsDto.Assessment == null || string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Assessment.Id))
                {
                    throw new ArgumentNullException("assessment.id", "assessment.id is a required field");
                }
                var nonCourses = await this.GetAptitudeAssessmentAsync();
                if (nonCourses != null)
                {
                    var nonCourse = nonCourses.FirstOrDefault(asc => asc.Guid == studentAptitudeAssessmentsDto.Assessment.Id);
                    if (nonCourse == null)
                    {
                        throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid Assessment.Id.");
                    }
                    aptitudeCode = nonCourse.Code;
                }


                if ((!studentAptitudeAssessmentsDto.AssessedOn.HasValue) || (studentAptitudeAssessmentsDto.AssessedOn == default(DateTimeOffset)))
                {
                    throw new ArgumentNullException("AssessedOn", "AssessedOn is a required field");
                }
                var studentTestScoreEntity = new StudentTestScores(studentAptitudeAssessmentsDto.Id,
                    studentTestScoresId, studentId, aptitudeCode, "", studentAptitudeAssessmentsDto.AssessedOn.Value.DateTime);

                if (studentTestScoreEntity == null)
                {
                    throw new Exception("An error occurred setting required fields on Student Aptitude Assessment.");
                }

                if ((studentAptitudeAssessmentsDto.Score == null) || (!studentAptitudeAssessmentsDto.Score.Value.HasValue))
                {
                    throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' does not have a score. Score is required.");
                }

                if (studentAptitudeAssessmentsDto.Score.Type == StudentAptitudeAssessmentsScoreType.Literal)
                {
                    throw new ArgumentException("'Literal' score types are not accepted.");
                }
                studentTestScoreEntity.Score = studentAptitudeAssessmentsDto.Score.Value;

                if ((studentAptitudeAssessmentsDto.Percentile != null) && (studentAptitudeAssessmentsDto.Percentile.Any()))
                {
                    var assesmentPercentileTypes = await this.GetAssesmentPercentileTypesAsync();

                    foreach (var percentile in studentAptitudeAssessmentsDto.Percentile)
                    {
                        if ((percentile.Type == null) || (string.IsNullOrEmpty(percentile.Type.Id)))
                        {
                            throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a percentile.Type. Percentile.Type is required.");
                        }

                        if (!percentile.Value.HasValue)
                        {
                            throw new ArgumentNullException("Percentile value is required.");
                        }

                        var assesmentPercentileType = assesmentPercentileTypes.FirstOrDefault(apt => apt.Guid == percentile.Type.Id);
                        if (assesmentPercentileType == null)
                        {
                            throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid percentile.Type. Percentile.Type is required.");
                        }
                        switch (assesmentPercentileType.Code)
                        {
                            case "1":
                                if (studentTestScoreEntity.Percentile1.HasValue)
                                {
                                    throw new ArgumentException("Duplicate percentile types are not permitted");
                                }
                                studentTestScoreEntity.Percentile1 = Convert.ToInt16(percentile.Value);
                                break;
                            case "2":
                                if (studentTestScoreEntity.Percentile2.HasValue)
                                {
                                    throw new ArgumentException("Duplicate percentile types are not permitted");
                                }
                                studentTestScoreEntity.Percentile2 = Convert.ToInt16(percentile.Value);
                                break;
                            default:
                                throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid percentile.Type. Percentile.Type is required.");
                        }
                    }
                }

                if (studentAptitudeAssessmentsDto.Form != null)
                {
                    studentTestScoreEntity.FormName = studentAptitudeAssessmentsDto.Form.Name;
                    studentTestScoreEntity.FormNo = studentAptitudeAssessmentsDto.Form.Number;
                }

                if ((studentAptitudeAssessmentsDto.SpecialCircumstances != null) && (studentAptitudeAssessmentsDto.SpecialCircumstances.Any()))
                {
                    var specialFactors = new List<string>();

                    var assessmentSpecialCircumstances = await this.GetAssessmentSpecialCircumstancesAsync();
                    foreach (var factor in studentAptitudeAssessmentsDto.SpecialCircumstances)
                    {
                        if (!(string.IsNullOrEmpty(factor.Id)))
                        {
                            var assessmentSpecialCircumstance = assessmentSpecialCircumstances.FirstOrDefault(asc => asc.Guid == factor.Id);
                            if (assessmentSpecialCircumstance == null)
                            {
                                throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid SpecialCircumstances.Id.");
                            }
                            specialFactors.Add(assessmentSpecialCircumstance.Code);
                        }
                    }

                    if (specialFactors.Any())
                    {
                        studentTestScoreEntity.SpecialFactors = specialFactors;
                    }
                }

                if ((studentAptitudeAssessmentsDto.Source != null) && (!(string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Source.Id))))
                {
                    var testSources = await this.GetTestSourcesAsync();
                    if (testSources == null)
                    {
                        throw new Exception("Unable to retrieve test sources");
                    }
                    var testSource = testSources.FirstOrDefault(ts => ts.Guid == studentAptitudeAssessmentsDto.Source.Id);
                    if (testSource == null)
                    {
                        throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid Source.Id");
                    }
                    studentTestScoreEntity.Source = testSource.Code;
                }

                if (studentAptitudeAssessmentsDto.Status != StudentAptitudeAssessmentsStatus.NotSet)
                {
                    switch (studentAptitudeAssessmentsDto.Status)
                    {
                        case StudentAptitudeAssessmentsStatus.Inactive:
                            studentTestScoreEntity.StatusCode = "1";
                            break;
                        case StudentAptitudeAssessmentsStatus.Active:
                            studentTestScoreEntity.StatusCode = "2";
                            break;
                        case StudentAptitudeAssessmentsStatus.Notational:
                            studentTestScoreEntity.StatusCode = "3";
                            break;

                    }
                }

                // When the reported value is 'Official', then ApplicationTestSource will be empty;
                if (studentAptitudeAssessmentsDto.Reported == StudentAptitudeAssessmentsReported.Unofficial)
                {
                    studentTestScoreEntity.ApplicationTestSource = "1";
                }

                return studentTestScoreEntity;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("An error occurred creating a Student Aptitude Assessment. " + ex.Message);
            }
        }

        /// <summary>
        /// Convert StudentAptitudeAssessments dto to StudentTestScores domain entity 
        /// </summary>
        /// <param name="studentTestScoresId">StudentTestScore id</param>
        /// <param name="studentAptitudeAssessmentsDto"><see cref="StudentAptitudeAssessments2">StudentAptitudeAssessments2 dto</param>
        /// <returns><see cref="StudentTestScores">StudentTestScores domain entity</returns>
        private async Task<StudentTestScores> ConvertStudentAptitudeAssessments2DtoToEntityAsync(string studentTestScoresId, StudentAptitudeAssessments2 studentAptitudeAssessmentsDto)
        {
            if (studentAptitudeAssessmentsDto == null)
            {
                throw new ArgumentNullException("studentAptitudeAssessmentsDto", "studentAptitudeAssessmentsDto is a required field");
            }

            var aptitudeCode = string.Empty;
            try
            {
                if (studentAptitudeAssessmentsDto.Student == null || string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Student.Id))
                {
                    throw new ArgumentNullException("student.id", "student.id is a required field");
                }
                var studentId = await _personRepository.GetPersonIdFromGuidAsync(studentAptitudeAssessmentsDto.Student.Id);
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentException(string.Format("Invalid student.id '{0}'.", studentAptitudeAssessmentsDto.Student.Id), "student.id");
                }
                if (await _personRepository.IsCorpAsync(studentId))
                {
                    throw new ArgumentException(string.Format("Invalid student.id '{0}'. An assessment cannot be assigned to a corporation.", studentAptitudeAssessmentsDto.Student.Id),"student.id");
                }

                if (studentAptitudeAssessmentsDto.Assessment == null || string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Assessment.Id))
                {
                    throw new ArgumentNullException("assessment.id", "assessment.id is a required field");
                }
                var nonCourses = await this.GetAptitudeAssessmentAsync();
                if (nonCourses != null)
                {
                    var nonCourse = nonCourses.FirstOrDefault(asc => asc.Guid == studentAptitudeAssessmentsDto.Assessment.Id);
                    if (nonCourse == null)
                    {
                        throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid Assessment.Id.");
                    }
                    aptitudeCode = nonCourse.Code;
                }


                if ((!studentAptitudeAssessmentsDto.AssessedOn.HasValue) || (studentAptitudeAssessmentsDto.AssessedOn == default(DateTimeOffset)))
                {
                    throw new ArgumentNullException("AssessedOn", "AssessedOn is a required field");
                }
                var studentTestScoreEntity = new StudentTestScores(studentAptitudeAssessmentsDto.Id,
                    studentTestScoresId, studentId, aptitudeCode, "", studentAptitudeAssessmentsDto.AssessedOn.Value.DateTime);

                if (studentTestScoreEntity == null)
                {
                    throw new Exception("An error occurred setting required fields on Student Aptitude Assessment.");
                }

                if ((studentAptitudeAssessmentsDto.Score == null) || (!studentAptitudeAssessmentsDto.Score.Value.HasValue))
                {
                    throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' does not have a score. Score is required.");
                }

                if (studentAptitudeAssessmentsDto.Score.Type == StudentAptitudeAssessmentsScoreType.Literal)
                {
                    throw new ArgumentException("'Literal' score types are not accepted.");
                }
                studentTestScoreEntity.Score = studentAptitudeAssessmentsDto.Score.Value;

                if ((studentAptitudeAssessmentsDto.Percentile != null) && (studentAptitudeAssessmentsDto.Percentile.Any()))
                {
                    var assesmentPercentileTypes = await this.GetAssesmentPercentileTypesAsync();

                    foreach (var percentile in studentAptitudeAssessmentsDto.Percentile)
                    {
                        if ((percentile.Type == null) || (string.IsNullOrEmpty(percentile.Type.Id)))
                        {
                            throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a percentile.Type. Percentile.Type is required.");
                        }

                        if (!percentile.Value.HasValue)
                        {
                            throw new ArgumentNullException("Percentile value is required.");
                        }

                        var assesmentPercentileType = assesmentPercentileTypes.FirstOrDefault(apt => apt.Guid == percentile.Type.Id);
                        if (assesmentPercentileType == null)
                        {
                            throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid percentile.Type. Percentile.Type is required.");
                        }
                        switch (assesmentPercentileType.Code)
                        {
                            case "1":
                                if (studentTestScoreEntity.Percentile1.HasValue)
                                {
                                    throw new ArgumentException("Duplicate percentile types are not permitted");
                                }
                                studentTestScoreEntity.Percentile1 = Convert.ToInt16(percentile.Value);
                                break;
                            case "2":
                                if (studentTestScoreEntity.Percentile2.HasValue)
                                {
                                    throw new ArgumentException("Duplicate percentile types are not permitted");
                                }
                                studentTestScoreEntity.Percentile2 = Convert.ToInt16(percentile.Value);
                                break;
                            default:
                                throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid percentile.Type. Percentile.Type is required.");
                        }
                    }
                }

                if (studentAptitudeAssessmentsDto.Form != null)
                {
                    studentTestScoreEntity.FormName = studentAptitudeAssessmentsDto.Form.Name;
                    studentTestScoreEntity.FormNo = studentAptitudeAssessmentsDto.Form.Number;
                }

                if ((studentAptitudeAssessmentsDto.SpecialCircumstances != null) && (studentAptitudeAssessmentsDto.SpecialCircumstances.Any()))
                {
                    var specialFactors = new List<string>();

                    var assessmentSpecialCircumstances = await this.GetAssessmentSpecialCircumstancesAsync();
                    foreach (var factor in studentAptitudeAssessmentsDto.SpecialCircumstances)
                    {
                        if (!(string.IsNullOrEmpty(factor.Id)))
                        {
                            var assessmentSpecialCircumstance = assessmentSpecialCircumstances.FirstOrDefault(asc => asc.Guid == factor.Id);
                            if (assessmentSpecialCircumstance == null)
                            {
                                throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid SpecialCircumstances.Id.");
                            }
                            specialFactors.Add(assessmentSpecialCircumstance.Code);
                        }
                    }

                    if (specialFactors.Any())
                    {
                        studentTestScoreEntity.SpecialFactors = specialFactors;
                    }
                }

                if ((studentAptitudeAssessmentsDto.Source != null) && (!(string.IsNullOrEmpty(studentAptitudeAssessmentsDto.Source.Id))))
                {
                    var testSources = await this.GetTestSourcesAsync();
                    if (testSources == null)
                    {
                        throw new Exception("Unable to retrieve test sources");
                    }
                    var testSource = testSources.FirstOrDefault(ts => ts.Guid == studentAptitudeAssessmentsDto.Source.Id);
                    if (testSource == null)
                    {
                        throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentsDto.Id + "' is missing a valid Source.Id");
                    }
                    studentTestScoreEntity.Source = testSource.Code;
                }

                if (studentAptitudeAssessmentsDto.Status != StudentAptitudeAssessmentsStatus.NotSet)
                {
                    switch (studentAptitudeAssessmentsDto.Status)
                    {
                        case StudentAptitudeAssessmentsStatus.Inactive:
                            studentTestScoreEntity.StatusCode = "1";
                            break;
                        case StudentAptitudeAssessmentsStatus.Active:
                            studentTestScoreEntity.StatusCode = "2";
                            break;
                        case StudentAptitudeAssessmentsStatus.Notational:
                            studentTestScoreEntity.StatusCode = "3";
                            break;

                    }
                }

                // When the reported value is 'Official', then ApplicationTestSource will be empty;
                if (studentAptitudeAssessmentsDto.Reported == StudentAptitudeAssessmentsReported.Unofficial)
                {
                    studentTestScoreEntity.ApplicationTestSource = "1";
                }

                return studentTestScoreEntity;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("An error occurred creating a Student Aptitude Assessment. " + ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity collection to its corresponding StudentAptitudeAssessments DTO collection 
        /// </summary>
        /// <param name="studentTestScoresEntities">StudentTestScores domain entity collection </param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO collection</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>> ConvertStudentTestScoresEntityToDtoCollectionAsync(List<StudentTestScores> studentTestScoreEntities, bool bypassCache = false)
        {
            var studentAptitudeAssessmentsDtos = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>();
            foreach (var studentTestScoreEntity in studentTestScoreEntities)
            {
                var studentAptitudeAssessmentDto = await ConvertStudentTestScoresEntityToDtoAsync(studentTestScoreEntity, bypassCache);
                studentAptitudeAssessmentsDtos.Add(studentAptitudeAssessmentDto);
            }

            return studentAptitudeAssessmentsDtos;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity to its corresponding StudentAptitudeAssessments DTO
        /// </summary>
        /// <param name="studentTestScoreEntity">StudentTestScores domain entity</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> ConvertStudentTestScoresEntityToDto2Async(StudentTestScores studentTestScoreEntity, bool bypassCache = false)
        {
            var studentAptitudeAssessmentDto = new StudentAptitudeAssessments();
            try
            {

                studentAptitudeAssessmentDto.Id = studentTestScoreEntity.Guid;
                if (!string.IsNullOrEmpty(studentTestScoreEntity.StudentId))
                {
                    var studentGuid = await _personRepository.GetPersonGuidFromIdAsync(studentTestScoreEntity.StudentId);
                    if (!string.IsNullOrEmpty(studentGuid))
                    {
                        studentAptitudeAssessmentDto.Student = new Dtos.GuidObject2(studentGuid);
                    }
                }
                if (!string.IsNullOrEmpty(studentTestScoreEntity.Code))
                {
                    var nonCourse = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsGuidAsync(studentTestScoreEntity.Code);
                    if (!string.IsNullOrEmpty(nonCourse))
                    {
                        studentAptitudeAssessmentDto.Assessment = new Ellucian.Colleague.Dtos.GuidObject2(nonCourse);
                    }
                }
                studentAptitudeAssessmentDto.AssessedOn = studentTestScoreEntity.DateTaken;
                if (studentTestScoreEntity.Score.HasValue)
                {
                    var score = new StudentAptitudeAssessmentsScore();
                    score.Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric;
                    score.Value = studentTestScoreEntity.Score;
                    studentAptitudeAssessmentDto.Score = score;
                }
                else
                {
                    throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentDto.Id + "' does not have a score. Score is required.");
                }
                var percentiles = new List<StudentAptitudeAssessmentsPercentile>();
                if (studentTestScoreEntity.Percentile1.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("1");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile1;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (studentTestScoreEntity.Percentile2.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("2");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile2;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (percentiles.Any())
                {
                    studentAptitudeAssessmentDto.Percentile = percentiles;
                }
                if (!(string.IsNullOrEmpty(studentTestScoreEntity.FormNo)) || !(string.IsNullOrEmpty(studentTestScoreEntity.FormName)))
                {
                    var form = new StudentAptitudeAssessmentsForm();
                    form.Name = studentTestScoreEntity.FormName;
                    form.Number = studentTestScoreEntity.FormNo;
                    studentAptitudeAssessmentDto.Form = form;
                }
                if (studentTestScoreEntity.SpecialFactors.Any())
                {
                    var factors = new List<Dtos.GuidObject2>();
                    foreach (var factor in studentTestScoreEntity.SpecialFactors)
                        if (!string.IsNullOrEmpty(factor))
                        {
                            var factorGuid = await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesGuidAsync(factor);
                            if (!string.IsNullOrEmpty(factorGuid))
                            {
                                factors.Add(new Dtos.GuidObject2(factorGuid));
                            }
                        }
                    studentAptitudeAssessmentDto.SpecialCircumstances = factors;
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.Source))
                {
                    var sourceGuid = await _studentReferenceDataRepository.GetTestSourcesGuidAsync(studentTestScoreEntity.Source);
                    if (!string.IsNullOrEmpty(sourceGuid))
                    {
                        studentAptitudeAssessmentDto.Source = new Dtos.GuidObject2(sourceGuid);
                    }
                }

                if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("OFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }
                else if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("UNOFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Unofficial;
                }
                else
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.StatusCode))
                {
                    // clients are to set special processing code 2 to EXP on the status that ETSU should use when a student's noncourse expires.
                    if (string.IsNullOrEmpty(studentTestScoreEntity.StatusCodeSpProcessing) &&
                            studentTestScoreEntity.StatusCodeSpProcessing2.Equals("EXP", StringComparison.OrdinalIgnoreCase))
                    {
                        studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                    }
                    else
                    {
                        switch (studentTestScoreEntity.StatusCodeSpProcessing)
                        {
                            case "1":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                                break;
                            case "2":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                studentAptitudeAssessmentDto.Preference = StudentAptitudeAssessmentsPreference.Primary;
                                break;
                            case "3":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Notational;
                                break;
                            default:
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                break;
                        }
                    }
                }
                else
                    studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;

            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "Student Aptitude Assessment not found for ID." + studentTestScoreEntity.Guid));
            }
            return studentAptitudeAssessmentDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity collection to its corresponding StudentAptitudeAssessments DTO collection 
        /// </summary>
        /// <param name="studentTestScoresEntities">StudentTestScores domain entity collection </param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO collection</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>> ConvertStudentTestScoresEntityToDtoCollection2Async(List<StudentTestScores> studentTestScoreEntities, bool bypassCache = false)
        {
            var studentAptitudeAssessmentsDtos = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>();
            foreach (var studentTestScoreEntity in studentTestScoreEntities)
            {
                var studentAptitudeAssessmentDto = await ConvertStudentTestScoresEntityToDto2Async(studentTestScoreEntity, bypassCache);
                studentAptitudeAssessmentsDtos.Add(studentAptitudeAssessmentDto);
            }

            return studentAptitudeAssessmentsDtos;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity collection to its corresponding StudentAptitudeAssessments DTO collection 
        /// </summary>
        /// <param name="studentTestScoresEntities">StudentTestScores domain entity collection </param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO collection</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>> ConvertStudentTestScoresEntityToDtoCollection3Async(List<StudentTestScores> studentTestScoreEntities, bool bypassCache = false)
        {
            var studentAptitudeAssessmentsDtos = new List<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>();
            if (studentTestScoreEntities == null || !studentTestScoreEntities.Any())
            {
                return studentAptitudeAssessmentsDtos;
            }

            var personIds = studentTestScoreEntities
                  .Where(x => (!string.IsNullOrEmpty(x.StudentId)))
                  .Select(x => x.StudentId).Distinct().ToList();
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

            foreach (var studentTestScoreEntity in studentTestScoreEntities)
            {
                studentAptitudeAssessmentsDtos.Add(await ConvertStudentTestScoresEntityToDto2Async(studentTestScoreEntity, personGuidCollection, bypassCache));
            }

            return studentAptitudeAssessmentsDtos;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity to its corresponding StudentAptitudeAssessments DTO
        /// </summary>
        /// <param name="studentTestScoreEntity">StudentTestScores domain entity</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> ConvertStudentTestScoresEntityToDtoAsync(StudentTestScores studentTestScoreEntity, bool bypassCache = false)
        {
            var studentAptitudeAssessmentDto = new StudentAptitudeAssessments();
            try
            {

                studentAptitudeAssessmentDto.Id = studentTestScoreEntity.Guid;
                if (!string.IsNullOrEmpty(studentTestScoreEntity.StudentId))
                {
                    var studentGuid = await _personRepository.GetPersonGuidFromIdAsync(studentTestScoreEntity.StudentId);
                    if (!string.IsNullOrEmpty(studentGuid))
                    {
                        studentAptitudeAssessmentDto.Student = new Dtos.GuidObject2(studentGuid);
                    }
                }
                if (!string.IsNullOrEmpty(studentTestScoreEntity.Code))
                {
                    var nonCourse = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsGuidAsync(studentTestScoreEntity.Code);
                    if (!string.IsNullOrEmpty(nonCourse))
                    {
                        studentAptitudeAssessmentDto.Assessment = new Ellucian.Colleague.Dtos.GuidObject2(nonCourse);
                    }
                }
                studentAptitudeAssessmentDto.AssessedOn = studentTestScoreEntity.DateTaken;
                if (studentTestScoreEntity.Score.HasValue)
                {
                    var score = new StudentAptitudeAssessmentsScore();
                    score.Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric;
                    score.Value = studentTestScoreEntity.Score;
                    studentAptitudeAssessmentDto.Score = score;
                }
                else
                {
                    throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentDto.Id + "' does not have a score. Score is required.");
                }
                var percentiles = new List<StudentAptitudeAssessmentsPercentile>();
                if (studentTestScoreEntity.Percentile1.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("1");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile1;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (studentTestScoreEntity.Percentile2.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("2");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile2;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (percentiles.Any())
                {
                    studentAptitudeAssessmentDto.Percentile = percentiles;
                }
                if (!(string.IsNullOrEmpty(studentTestScoreEntity.FormNo)) || !(string.IsNullOrEmpty(studentTestScoreEntity.FormName)))
                {
                    var form = new StudentAptitudeAssessmentsForm();
                    form.Name = studentTestScoreEntity.FormName;
                    form.Number = studentTestScoreEntity.FormNo;
                    studentAptitudeAssessmentDto.Form = form;
                }
                if (studentTestScoreEntity.SpecialFactors.Any())
                {
                    var factors = new List<Dtos.GuidObject2>();
                    foreach (var factor in studentTestScoreEntity.SpecialFactors)
                    {
                        if (!string.IsNullOrEmpty(factor))
                        {
                            var factorGuid = await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesGuidAsync(factor);
                            if (!string.IsNullOrEmpty(factorGuid))
                            {
                                factors.Add(new Dtos.GuidObject2(factorGuid));
                            }
                        }
                    }
                    studentAptitudeAssessmentDto.SpecialCircumstances = factors;
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.Source))
                {
                    var sourceGuid = await _studentReferenceDataRepository.GetTestSourcesGuidAsync(studentTestScoreEntity.Source);
                    if (!string.IsNullOrEmpty(sourceGuid ))
                    {
                        studentAptitudeAssessmentDto.Source = new Dtos.GuidObject2(sourceGuid);
                    }
                }

                
                if (!string.IsNullOrEmpty(studentTestScoreEntity.StatusCode))
                {
                    //clients are to set special processing code 2 to EXP on the status that ETSU should use when a student's noncourse expires
                    if (string.IsNullOrEmpty(studentTestScoreEntity.StatusCodeSpProcessing)
                        && studentTestScoreEntity.StatusCodeSpProcessing2.Equals("EXP", StringComparison.OrdinalIgnoreCase))
                    {
                        studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                    }
                    else
                    {
                        switch (studentTestScoreEntity.StatusCodeSpProcessing)
                        {
                            case "1":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                                break;
                            case "2":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                studentAptitudeAssessmentDto.Preference = StudentAptitudeAssessmentsPreference.Primary;
                                break;
                            case "3":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                                break;
                            default:
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                break;
                        }
                    }
                }
                else
                    studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;

                if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("OFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }
                else if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("UNOFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Unofficial;
                }
                else
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }

            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "Student Aptitude Assessment not found for ID." + studentTestScoreEntity.Guid));
            }
            return studentAptitudeAssessmentDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTestScores domain entity to its corresponding StudentAptitudeAssessments DTO
        /// </summary>
        /// <param name="studentTestScoreEntity">StudentTestScores domain entity</param>
        /// <param name="personGuidCollection">dictionary of person guid/ids</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>StudentAptitudeAssessments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2> ConvertStudentTestScoresEntityToDto2Async(StudentTestScores studentTestScoreEntity,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            var studentAptitudeAssessmentDto = new StudentAptitudeAssessments2();
            try
            {
                studentAptitudeAssessmentDto.Id = studentTestScoreEntity.Guid;
             
                if (!string.IsNullOrEmpty(studentTestScoreEntity.StudentId))
                {
                    if (personGuidCollection == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Person guid not found in collection, PersonId: '", studentTestScoreEntity.StudentId, "', Record ID: '", studentTestScoreEntity.RecordKey, "'"));
                    }
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(studentTestScoreEntity.StudentId, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("Person guid not found, PersonId: '", studentTestScoreEntity.StudentId, "', Record ID: '", studentTestScoreEntity.RecordKey, "'"));
                    }
                    studentAptitudeAssessmentDto.Student = new Dtos.GuidObject2(personGuid);
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.Code))
                {
                    var nonCourse = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsGuidAsync(studentTestScoreEntity.Code);
                    if (!string.IsNullOrEmpty(nonCourse))
                    {
                        studentAptitudeAssessmentDto.Assessment = new Ellucian.Colleague.Dtos.GuidObject2(nonCourse);
                    }
                }
                studentAptitudeAssessmentDto.AssessedOn = studentTestScoreEntity.DateTaken;
                if (studentTestScoreEntity.Score.HasValue)
                {
                    var score = new StudentAptitudeAssessmentsScore();
                    score.Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric;
                    score.Value = studentTestScoreEntity.Score;
                    studentAptitudeAssessmentDto.Score = score;
                }
                else
                {
                    throw new KeyNotFoundException("The student-aptitude-assessments with ID '" + studentAptitudeAssessmentDto.Id + "' does not have a score. Score is required.");
                }
                var percentiles = new List<StudentAptitudeAssessmentsPercentile>();
                if (studentTestScoreEntity.Percentile1.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("1");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile1;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (studentTestScoreEntity.Percentile2.HasValue)
                {
                    var percentileType = await _studentReferenceDataRepository.GetIntgTestPercentileTypesGuidAsync("2");
                    if (!string.IsNullOrEmpty(percentileType))
                    {
                        var percentile = new StudentAptitudeAssessmentsPercentile();
                        percentile.Value = studentTestScoreEntity.Percentile2;
                        percentile.Type = new Dtos.GuidObject2(percentileType);
                        percentiles.Add(percentile);
                    }
                }
                if (percentiles.Any())
                {
                    studentAptitudeAssessmentDto.Percentile = percentiles;
                }
                if (!(string.IsNullOrEmpty(studentTestScoreEntity.FormNo)) || !(string.IsNullOrEmpty(studentTestScoreEntity.FormName)))
                {
                    var form = new StudentAptitudeAssessmentsForm();
                    form.Name = studentTestScoreEntity.FormName;
                    form.Number = studentTestScoreEntity.FormNo;
                    studentAptitudeAssessmentDto.Form = form;
                }
                if (studentTestScoreEntity.SpecialFactors.Any())
                {
                    var factors = new List<Dtos.GuidObject2>();
                    foreach (var factor in studentTestScoreEntity.SpecialFactors)
                        if (!string.IsNullOrEmpty(factor))
                        {
                            var factorGuid = await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesGuidAsync(factor);
                            if (!string.IsNullOrEmpty(factorGuid))
                            {
                                factors.Add(new Dtos.GuidObject2(factorGuid));
                            }
                        }
                    studentAptitudeAssessmentDto.SpecialCircumstances = factors;
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.Source))
                {
                    var sourceGuid = await _studentReferenceDataRepository.GetTestSourcesGuidAsync(studentTestScoreEntity.Source);
                    if (!string.IsNullOrEmpty(sourceGuid))
                    {
                        studentAptitudeAssessmentDto.Source = new Dtos.GuidObject2(sourceGuid);
                    }
                }

                if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("OFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }
                else if (!string.IsNullOrWhiteSpace(studentTestScoreEntity.ApplicationTestSource) && studentTestScoreEntity.ApplicationTestSource.ToUpper().Equals("UNOFFICIAL", StringComparison.OrdinalIgnoreCase))
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Unofficial;
                }
                else
                {
                    studentAptitudeAssessmentDto.Reported = StudentAptitudeAssessmentsReported.Official;
                }

                if (!string.IsNullOrEmpty(studentTestScoreEntity.StatusCode))
                {
                    // clients are to set special processing code 2 to EXP on the status that ETSU should use when a student's noncourse expires.
                    if (string.IsNullOrEmpty(studentTestScoreEntity.StatusCodeSpProcessing) &&
                            studentTestScoreEntity.StatusCodeSpProcessing2.Equals("EXP", StringComparison.OrdinalIgnoreCase))
                    {
                        studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                    }
                    else
                    {
                        switch (studentTestScoreEntity.StatusCodeSpProcessing)
                        {
                            case "1":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Inactive;
                                break;
                            case "2":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                studentAptitudeAssessmentDto.Preference = StudentAptitudeAssessmentsPreference.Primary;
                                break;
                            case "3":
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Notational;
                                break;
                            default:
                                studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;
                                break;
                        }
                    }
                }
                else
                    studentAptitudeAssessmentDto.Status = StudentAptitudeAssessmentsStatus.Active;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat(ex.Message, "Student Aptitude Assessment ID." + studentTestScoreEntity.Guid));
            }
            return studentAptitudeAssessmentDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Aptitude Assessments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetStudentAptitudeAssessmentsPermission()
        {
            // User is not allowed to view a Student Aptitude Assessments without the appropriate permissions
            if (!HasPermission(StudentPermissionCodes.ViewStudentAptitudeAssessmentsConsent) && !HasPermission(StudentPermissionCodes.CreateStudentAptitudeAssessmentsConsent))
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Student Aptitude Assessments (Test Scores).");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update Student Aptitude Assessments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateStudentAptitudeAssessmentsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.CreateStudentAptitudeAssessmentsConsent);

            // User is not allowed to create or update Student Aptitude Assessments without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update Student Aptitude Assessments (Test Scores).");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to delete Student Aptitude Assessments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<bool> CheckDeleteStudentAptitudeAssessmentsPermission()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.DeleteStudentAptitudeAssessmentsConsent))
            {
                return true;
            }
            return false;
        }
    } 
}