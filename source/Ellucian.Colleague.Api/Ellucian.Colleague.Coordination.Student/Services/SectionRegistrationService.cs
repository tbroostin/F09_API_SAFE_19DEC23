// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// SectionRegistrationService is an application that responds to a request for Section registration Management
    /// </summary>
    [RegisterType]
    public class SectionRegistrationService :StudentCoordinationService, ISectionRegistrationService
    {
        private readonly ISectionRegistrationRepository _sectionRegistrationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonBaseRepository _personBaseRepository;

        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
        private readonly IConfigurationRepository _configurationRepository;

        public SectionRegistrationService(IAdapterRegistry adapterRegistry,
            IPersonRepository personRepository,
            IPersonBaseRepository personBaseRepository,
            ISectionRepository sectionRepository,
            ISectionRegistrationRepository sectionRegistrationRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStudentRepository studentRepository,
            IGradeRepository gradeRepository,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository)

            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _personRepository = personRepository;
            _personBaseRepository = personBaseRepository;
            _sectionRegistrationRepository = sectionRegistrationRepository;
            _sectionRepository = sectionRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _gradeRepository = gradeRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GradeSchemesAsync()
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync();
            }
            return _gradeSchemes;
        }

        private IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem> _secRegStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem>> GetSectionRegistrationStatusesAsync(bool ignoreCache)
        {
            if (_secRegStatuses == null)
            {                
                _secRegStatuses = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(ignoreCache);
            }
            return _secRegStatuses;
        }

        private IEnumerable<Domain.Student.Entities.Grade> _grades = null;
        private async Task<IEnumerable<Domain.Student.Entities.Grade>> GetGradeHedmAsync(bool ignoreCache)
        {
            if (_grades == null)
            {
                _grades = await _gradeRepository.GetHedmAsync(ignoreCache);
            }
            return _grades;
        }

        private IEnumerable<GradeChangeReason> _gradeChangeReasons = null;
        private async Task<IEnumerable<GradeChangeReason>> GetGradeChangeReasonAsync(bool ignoreCache)
        {
            if (_gradeChangeReasons == null)
            {
                _gradeChangeReasons = await _referenceDataRepository.GetGradeChangeReasonAsync(ignoreCache);
            }
            return _gradeChangeReasons;
        }

        #region Get Methods
   
        /// <summary>
        /// Get a section registration
        /// </summary>
        /// <returns>The section registration <see cref="Dtos.SectionRegistration2">object</see></returns>
        public async Task<Dtos.SectionRegistration2> GetSectionRegistrationAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("GetSectionRegistration", "Must provide a SectionRegistration id for retrieval");

            // get user permissions
            CheckUserRegistrationViewPermissions();

            // process the request
            var response = await _sectionRegistrationRepository.GetAsync(guid);

            // convert response to SectionRegistration
            var dtoResponse = await ConvertResponsetoDto2(response);

            // log any messages from the response
            if (response == null || response.Messages.Count > 0)
            {
                if (logger.IsInfoEnabled)
                {
                    string errorMessage = string.Empty;
                    foreach (var msg in response.Messages)
                    {
                        errorMessage += string.Format(Environment.NewLine + "'{0}'", msg);
                    }
                    logger.Info(errorMessage);
                }
                throw new InvalidOperationException("section-registration for Id " + guid + " could not be found");
            }

            // convert response to SectionRegistration
            return dtoResponse;
        }

        /// <summary>
        /// Get a section registration V7
        /// </summary>
        /// <returns>The section registration <see cref="Dtos.SectionRegistration3">object</see></returns>
        public async Task<Dtos.SectionRegistration3> GetSectionRegistration2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("GetSectionRegistration", "Must provide a SectionRegistration id for retrieval");

            // get user permissions
            CheckUserRegistrationViewPermissions();

            // process the request
            var response = await _sectionRegistrationRepository.GetAsync(guid);

            // convert response to SectionRegistration
            var dtoResponse = await ConvertResponsetoDto3(response);

            // log any messages from the response
            if (response == null || response.Messages.Count > 0)
            {
                if (logger.IsInfoEnabled)
                {
                    string errorMessage = string.Empty;
                    foreach (var msg in response.Messages)
                    {
                        errorMessage += string.Format(Environment.NewLine + "'{0}'", msg);
                    }
                    logger.Info(errorMessage);
                }
                throw new InvalidOperationException("section-registration for Id " + guid + " could not be found");
            }

            // convert response to SectionRegistration
            return dtoResponse;
        }

        /// <summary>
        /// Filter for section registrations V6
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="section">section</param>
        /// <param name="registrant">registrant</param>
        /// <returns>Tuple containing IEnumerable Dtos.SectionRegistration2 object and record count</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionRegistration2>, int>> GetSectionRegistrationsAsync(int offset, int limit, string section, string registrant)
        {
            // get user permissions
            CheckUserRegistrationViewPermissions();

            string  personId = string.Empty, sectionId = string.Empty;
            var sectRegistrationList = new List<Dtos.SectionRegistration2>();
            if (!string.IsNullOrEmpty(section))
            {
                try
                {
                    sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(section);
                    if (string.IsNullOrEmpty(sectionId))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(sectRegistrationList, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(sectRegistrationList, 0);
                }
            }
            if (!string.IsNullOrEmpty(registrant))
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(registrant);
                if (string.IsNullOrEmpty(personId))
                    return new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(sectRegistrationList, 0);
            }

            var responses = await _sectionRegistrationRepository.GetSectionRegistrationsAsync(offset, limit, sectionId, personId);

            if (responses == null)
            {
                return new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(sectRegistrationList, 0);
            }
            foreach (var response in responses.Item1)
            {
                // convert response to SectionRegistration
                sectRegistrationList.Add(await ConvertResponsetoDto2(response));
            }

            return new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(sectRegistrationList, responses.Item2);

        }

        /// <summary>
        /// Filter for section registrations V7
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="section">section</param>
        /// <param name="registrant">registrant</param>
        /// <returns>Tuple containing IEnumerable Dtos.SectionRegistration2 object and record count</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionRegistration3>, int>> GetSectionRegistrations2Async(int offset, int limit, string section, string registrant)
        {
            // get user permissions
            CheckUserRegistrationViewPermissions();
            
            //if the section guid is input is invalid, the ingore it. 
            string personId = string.Empty, sectionId = string.Empty;
            var sectRegistrationList = new List<Dtos.SectionRegistration3>();
            if (!string.IsNullOrEmpty(section))
            {
                try
                {
                    sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(section);
                    if (string.IsNullOrEmpty(sectionId))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(sectRegistrationList, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(sectRegistrationList, 0);
                }
            }

            if (!string.IsNullOrEmpty(registrant))
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(registrant);
                if (string.IsNullOrEmpty(personId))
                    return new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(sectRegistrationList, 0);
            }
            var responses = await _sectionRegistrationRepository.GetSectionRegistrationsAsync(offset, limit, sectionId, personId);

            if (responses == null)
            {
                return new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(sectRegistrationList, 0);
            }

            foreach (var response in responses.Item1)
            {
                // convert response to SectionRegistration
                sectRegistrationList.Add(await ConvertResponsetoDto3(response));
            }

            return new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(sectRegistrationList, responses.Item2);
        }

        #endregion

        #region Create Methods
        /// <summary>
        /// Creates a registration record
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        public async Task<Dtos.SectionRegistration2> CreateSectionRegistrationAsync(Dtos.SectionRegistration2 registrationDto)
        {
            var guid = string.Empty;
            return await UpdateSectionRegistrationAsync(guid, registrationDto);
        }

        /// <summary>
        /// Creates a registration record
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        public async Task<Dtos.SectionRegistration3> CreateSectionRegistration2Async(Dtos.SectionRegistration3 registrationDto)
        {
            var guid = string.Empty;
            return await UpdateSectionRegistration2Async(guid, registrationDto);
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Update a section registration (Obsolete as of HeDM version 4)
        /// </summary>
        /// <param name="registrationDto">The <see cref="Dtos.SectionRegistration">section registration</see> entity to update in the database.</param>
        /// <returns>The modified section registration <see cref="Dtos.SectionRegistration">object</see></returns>
        public async Task<Dtos.SectionRegistration> UpdateRegistrationAsync(Dtos.SectionRegistration registrationDto)
        {
            if (registrationDto == null)
                throw new ArgumentNullException("registrationDto", "Must provide a SectionRegistration object for update");
            if (string.IsNullOrEmpty(registrationDto.Guid))
                throw new ArgumentNullException("registrationDto", "Must provide a guid for section registration update");
            if (string.IsNullOrEmpty(registrationDto.Registrant.Guid))
                throw new ArgumentNullException("registrationDto", "Must provide a guid for person");
            if (string.IsNullOrEmpty(registrationDto.Section.Guid))
                throw new ArgumentNullException("registrationDto", "Must provide a guid for section");

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Registrant.Guid);
            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to guid '" + registrationDto.Registrant.Guid + "' not found in repository");

            // get user permissions
            CheckUserRegistrationUpdatePermissions(personId);

            // get the section ID associated to the incoming guid
            var sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(registrationDto.Section.Guid);
            if (string.IsNullOrEmpty(sectionId))
                throw new KeyNotFoundException("Section ID associated to guid '" + registrationDto.Guid + "' not found in repository");

            // convert the request
            var request = ConvertDtotoRequestEntity(registrationDto, personId, sectionId);
            // process the request
            var response = await _sectionRegistrationRepository.RegisterAsync(request);
            // convert response to SectionRegistration
            var dtoResponse = ConvertResponsetoDto(registrationDto, response);
            // log any messages from the response
            if (response.Messages.Count > 0)
            {
                var inError = "Registration should be verified";
                var inString = string.Format("Person Id: '{0}', Section Id: '{1}'", personId, sectionId);

                if (logger.IsInfoEnabled)
                {
                    string errorMessage = string.Format("'{0}'" + Environment.NewLine + "'{1}'", inError, inString);
                    foreach (var msg in response.Messages)
                    {
                        errorMessage += string.Format(Environment.NewLine + "'{0}'", msg);
                    }
                    logger.Info(errorMessage);
                }
            }
            return dtoResponse;
        }

        /// <summary>
        /// Update a section registration
        /// </summary>
        /// <param name="guid">Guid for section registration</param>
        /// <param name="registrationDto">The <see cref="Dtos.SectionRegistration2">section registration</see> entity to update in the database.</param>
        /// <returns>The modified section registration <see cref="Dtos.SectionRegistration2">object</see></returns>
        public async Task<Dtos.SectionRegistration2> UpdateSectionRegistrationAsync(string guid, Dtos.SectionRegistration2 registrationDto)
        {
            //check if required fields are filled
            CheckForRequiredFields(registrationDto);

            //Check business logic
            await CheckForBusinessRulesAsync(guid,registrationDto);

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Registrant.Id);

            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to id '" + registrationDto.Registrant.Id + "' not found");

           
            CheckUserRegistrationUpdatePermissions(personId);
           

            _sectionRegistrationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            string statusCode = string.Empty;
            if (registrationDto.Status.Detail != null && !string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
            {
                // get optional Status Code from Detail guid.
                var statusCollection = await GetSectionRegistrationStatusesAsync(false);
                var statusItem = statusCollection.Where(r => r.Guid == registrationDto.Status.Detail.Id).FirstOrDefault();

                if (statusItem != null)
                {
                    statusCode = statusItem.Code;
                }
                else
                {
                    throw new ArgumentNullException(string.Concat("Registration Status with id ", registrationDto.Status.Detail.Id.ToString(), " was not found in the valid Section Registration Status List."));
                }
            }

            // get the section ID associated to the incoming guid
            var sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(registrationDto.Section.Id);
            if (string.IsNullOrEmpty(sectionId))
                throw new KeyNotFoundException("Section ID associated to guid '" + registrationDto.Id + "' not found");

            // convert the request
            var request = ConvertDtoToRequest2Entity(registrationDto, personId, sectionId);
            await ConvertDtoToGradesAsync(registrationDto, request);

            // process the request
            var response = await _sectionRegistrationRepository.UpdateAsync(request, guid, personId, sectionId, statusCode);
            
            //Assign the new guid
            request.RegGuid = response.Id;
            var stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Id);

            //check to see if the student acad cred record is not deleted, if it is then just return the request DTO
            if (await _sectionRegistrationRepository.CheckStuAcadCredRecord(stcKey))
            {

                //Process the ImportGradesRequest if there are no error occured after registering a person
                if (!response.ErrorOccured)
                {

                    request.StudentAcadCredId = string.IsNullOrEmpty(stcKey) ? string.Empty : stcKey;
                    await _sectionRegistrationRepository.UpdateGradesAsync(response, request);
                }
                else
                {
                    throw new InvalidOperationException(response.Messages.ElementAt(0).Message);
                }

                // convert response to SectionRegistration
                var dtoResponse = await ConvertResponsetoDto2(response);

                // log any messages from the response
                if (response.Messages.Any())
                {
                    var inError = "Registration may have failed";
                    var inString = string.Format("Person Id: '{0}', Section Id: '{1}'", personId, sectionId);

                    if (logger.IsInfoEnabled)
                    {
                        string errorMessage = string.Format("'{0}'" + Environment.NewLine + "'{1}'", inError, inString);
                        foreach (var msg in response.Messages)
                        {
                            errorMessage += string.Format(Environment.NewLine + "'{0}'", msg);
                        }
                        logger.Info(errorMessage);
                    }
                }
                return dtoResponse;
            }
            else
            {
                return registrationDto;
            }
        }

        /// <summary>
        /// Update a section registration
        /// </summary>
        /// <param name="guid">Guid for section registration</param>
        /// <param name="registrationDto">The <see cref="Dtos.SectionRegistration2">section registration</see> entity to update in the database.</param>
        /// <returns>The modified section registration <see cref="Dtos.SectionRegistration2">object</see></returns>
        public async Task<Dtos.SectionRegistration3> UpdateSectionRegistration2Async(string guid, Dtos.SectionRegistration3 registrationDto)
        {
            //check if required fields are filled
            CheckForRequiredFields2(registrationDto);

            //Check business logic
            await CheckForBusinessRules2Async(guid, registrationDto);

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Registrant.Id);

            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to id '" + registrationDto.Registrant.Id + "' not found");

            CheckUserRegistrationUpdatePermissions(personId);
            

            _sectionRegistrationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            string statusCode = string.Empty;
            if (registrationDto.Status.Detail != null && !string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
            {
                // get optional Status Code from Detail guid.
                var statusCollection = await GetSectionRegistrationStatusesAsync(false);
                var statusItem = statusCollection.Where(r => r.Guid == registrationDto.Status.Detail.Id).FirstOrDefault();

                if (statusItem != null)
                {
                    statusCode = statusItem.Code;
                }
                else
                {
                    throw new ArgumentNullException(string.Concat("Registration Status with id ", registrationDto.Status.Detail.Id.ToString(), " was not found in the valid Section Registration Status List."));
                }
            }

            // get the section ID associated to the incoming guid
            var sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(registrationDto.Section.Id);
            if (string.IsNullOrEmpty(sectionId))
                throw new KeyNotFoundException("Section ID associated to guid '" + registrationDto.Id + "' not found");
            
            // convert the request
            var request = await ConvertDtoToRequest3Entity(registrationDto, personId, sectionId);
            await ConvertDtoToGrades2Async(registrationDto, request);

            // process the request
            var response = await _sectionRegistrationRepository.Update2Async(request, guid, personId, sectionId, statusCode);

            //Assign the new guid
            request.RegGuid = response.Id;
            var stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Id);

            //check to see if the student acad cred record is not deleted, if it is then just return the request DTO
            if (await _sectionRegistrationRepository.CheckStuAcadCredRecord(stcKey))
            {

                //Process the ImportGradesRequest if there are no error occured after registering a person
                if (!response.ErrorOccured)
                {
                    request.StudentAcadCredId = string.IsNullOrEmpty(stcKey) ? string.Empty : stcKey;
                   
                    await _sectionRegistrationRepository.UpdateGradesAsync(response, request);
                }
                else
                {
                    throw new InvalidOperationException(response.Messages.ElementAt(0).Message);
                }

                // convert response to SectionRegistration
                var dtoResponse = await ConvertResponsetoDto3(response);

                // log any messages from the response
                if (response.Messages.Any())
                {
                    var inError = "Registration may have failed";
                    var inString = string.Format("Person Id: '{0}', Section Id: '{1}'", personId, sectionId);

                    if (logger.IsInfoEnabled)
                    {
                        string errorMessage = string.Format("'{0}'" + Environment.NewLine + "'{1}'", inError, inString);
                        foreach (var msg in response.Messages)
                        {
                            errorMessage += string.Format(Environment.NewLine + "'{0}'", msg);
                        }
                        logger.Info(errorMessage);
                    }
                }
                return dtoResponse;
            }
            else
            {
                return registrationDto;
            }
        }

        /// <summary>
        /// Converts Dto to grades
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task ConvertDtoToGradesAsync(Dtos.SectionRegistration2 registrationDto,
            SectionRegistrationRequest request)
        {
            var sectionGradeTypeDtos = await GetSectionGradeTypesAsync(false);

            if (gradeEntities == null)
                gradeEntities = await GetGradeHedmAsync(true);

            var gradeChangeReasons = (await GetGradeChangeReasonAsync(false)).ToList();

            if (registrationDto.SectionRegistrationGrades != null)
            {
                foreach (var sectionGrade in registrationDto.SectionRegistrationGrades)
                {
                    var gradeType = sectionGradeTypeDtos.FirstOrDefault(t => t.Id == sectionGrade.SectionGradeType.Id);
                    if (gradeType == null)
                        throw new KeyNotFoundException(
                            string.Format("Section grade type id associated to guid '{0}' not found",
                                sectionGrade.SectionGradeType.Id));


                    var personId = string.Empty;
                    if (sectionGrade.Submission != null && sectionGrade.Submission.SubmittedBy != null)
                    {
                        personId =
                            await _personRepository.GetPersonIdFromGuidAsync(sectionGrade.Submission.SubmittedBy.Id);
                        if (string.IsNullOrEmpty(personId))
                        {
                            throw new KeyNotFoundException(
                                string.Format("submission submittedBy id associated to guid '{0}' not found",
                                    sectionGrade.Submission.SubmittedBy.Id));
                        }
                        if (await _personRepository.IsCorpAsync(personId))
                        {
                            throw new ArgumentNullException("registrationDto.submission.submittedBy.id", "The grade submittedby Id must be a person.");
                        }
                    }

                    if (gradeType.Code.Equals("FINAL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sectionGrade.Submission == null)
                        {
                            request.FinalTermGrade = new TermGrade(registrationDto.Id, null, personId,  gradeType.Code);
                               //gradeType.Code.Substring(0, 1));
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                request.FinalTermGrade = new TermGrade(registrationDto.Id, null, personId, gradeType.Code);
                                 //   gradeType.Code.Substring(0, 1));
                            }
                            else
                            {
                                request.FinalTermGrade = new TermGrade(registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId, gradeType.Code); //gradeType.Code.Substring(0, 1));
                            }

                        }

                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));
                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            request.FinalTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        request.FinalTermGrade.Grade = grade.LetterGrade;
                        request.FinalTermGrade.GradeKey = grade.Id;
                    }
                    else if (gradeType.Code.Equals("VERIFIED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sectionGrade.Submission == null)
                        {
                            request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id, null, personId,
                                gradeType.Code);
                            // gradeType.Code.Substring(0, 1));
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id, null, personId,  gradeType.Code);
                                    //gradeType.Code.Substring(0, 1));
                            }
                            else
                            {
                                request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId, gradeType.Code); // gradeType.Code.Substring(0, 1));
                            }
                        }

                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));
                        
                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            request.VerifiedTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        request.VerifiedTermGrade.Grade = grade.LetterGrade;
                        request.VerifiedTermGrade.GradeKey = grade.Id;
                    }
                    else
                    {
                        if (request.MidTermGrades == null)
                            request.MidTermGrades = new List<Domain.Student.Entities.MidTermGrade>();

                        int position;
                        bool parsed = int.TryParse(gradeType.Code.Substring(3), out position);

                        if (!parsed)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", gradeType.Code));

                        if (position <= 0 || position > 6)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", gradeType.Code));

                        Domain.Student.Entities.MidTermGrade midTermGrade = null;

                        if (sectionGrade.Submission == null)
                        {
                            midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id, null,
                                personId);
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id,
                                    null, personId);
                            }
                            else
                            {
                                midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId);
                            }

                        }

                        midTermGrade.GradeTypeCode = gradeType.Code;
                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));

                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            midTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        midTermGrade.Grade = grade.LetterGrade;
                        midTermGrade.GradeKey = grade.Id;
                        request.MidTermGrades.Add(midTermGrade);
                    }

                }
            }
            if (registrationDto.Process != null)
            {
                if (registrationDto.Process.GradeExtension != null)
                {
                    request.GradeExtentionExpDate = registrationDto.Process.GradeExtension.ExpiresOn;
                }
            }

            if (registrationDto.Involvement != null)
            {
                request.InvolvementStartOn = registrationDto.Involvement.StartOn;
                request.InvolvementEndOn = registrationDto.Involvement.EndOn;
            }

            if (registrationDto.SectionRegistrationReporting != null)
            {
                if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance != null)
                {
                    if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance.Status ==
                        Dtos.ReportingStatusType.NeverAttended)
                    {
                        request.ReportingStatus = "Y";
                    }
                    else
                    {
                        request.ReportingStatus = "N";
                        request.ReportingLastDayOfAttendance =
                            registrationDto.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn;
                    }
                }
            }
        }

        /// <summary>
        /// Converts Dto to grades
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task ConvertDtoToGrades2Async(Dtos.SectionRegistration3 registrationDto,
            SectionRegistrationRequest request)
        {
            var sectionGradeTypeDtos = await GetSectionGradeTypesAsync(false);

            if (gradeEntities == null)
                gradeEntities = await GetGradeHedmAsync(true);

            var gradeChangeReasons = (await GetGradeChangeReasonAsync(false)).ToList();

            if (registrationDto.SectionRegistrationGrades != null)
            {
                foreach (var sectionGrade in registrationDto.SectionRegistrationGrades)
                {
                    var gradeType = sectionGradeTypeDtos.FirstOrDefault(t => t.Id == sectionGrade.SectionGradeType.Id);
                    if (gradeType == null)
                        throw new KeyNotFoundException(
                            string.Format("Section grade type id associated to guid '{0}' not found",
                                sectionGrade.SectionGradeType.Id));


                    var personId = string.Empty;
                    if (sectionGrade.Submission != null && sectionGrade.Submission.SubmittedBy != null)
                    {
                        personId =
                            await _personRepository.GetPersonIdFromGuidAsync(sectionGrade.Submission.SubmittedBy.Id);
                        if (string.IsNullOrEmpty(personId))
                        {
                            throw new KeyNotFoundException(
                                string.Format("submission submittedBy id associated to guid '{0}' not found",
                                    sectionGrade.Submission.SubmittedBy.Id));
                        }
                        if (await _personRepository.IsCorpAsync(personId))
                        {
                            throw new ArgumentNullException("registrationDto.submission.submittedBy.id", "The id for grade submittedby must be a person");
                        }
                    }

                    if (gradeType.Code.Equals("FINAL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sectionGrade.Submission == null)
                        {
                            request.FinalTermGrade = new TermGrade(registrationDto.Id, null, personId, gradeType.Code);
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                request.FinalTermGrade = new TermGrade(registrationDto.Id, null, personId, gradeType.Code);
                            }
                            else
                            {
                                request.FinalTermGrade = new TermGrade(registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId, gradeType.Code);
                            }

                        }

                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));
                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            request.FinalTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        request.FinalTermGrade.Grade = grade.LetterGrade;
                        request.FinalTermGrade.GradeKey = grade.Id;
                    }
                    else if (gradeType.Code.Equals("VERIFIED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sectionGrade.Submission == null)
                        {
                            request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id, null, personId,
                                gradeType.Code);
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id, null, personId, gradeType.Code);
                            }
                            else
                            {
                                request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId, gradeType.Code);
                            }
                        }

                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));

                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            request.VerifiedTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        request.VerifiedTermGrade.Grade = grade.LetterGrade;
                        request.VerifiedTermGrade.GradeKey = grade.Id;
                    }
                    else
                    {
                        if (request.MidTermGrades == null)
                            request.MidTermGrades = new List<Domain.Student.Entities.MidTermGrade>();

                        int position;
                        bool parsed = int.TryParse(gradeType.Code.Substring(3), out position);

                        if (!parsed)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", gradeType.Code));

                        if (position <= 0 || position > 6)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", gradeType.Code));

                        Domain.Student.Entities.MidTermGrade midTermGrade = null;

                        if (sectionGrade.Submission == null)
                        {
                            midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id, null,
                                personId);
                        }
                        else
                        {
                            if (sectionGrade.Submission.SubmittedOn == null)
                            {
                                midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id,
                                    null, personId);
                            }
                            else
                            {
                                midTermGrade = new Domain.Student.Entities.MidTermGrade(position, registrationDto.Id,
                                    sectionGrade.Submission.SubmittedOn.Value, personId);
                            }

                        }

                        midTermGrade.GradeTypeCode = gradeType.Code;
                        var grade = gradeEntities.FirstOrDefault(g => g.Guid == sectionGrade.SectionGrade.Id);
                        if (grade == null)
                            throw new KeyNotFoundException(
                                string.Format("Section grade id associated to guid '{0}' not found",
                                    sectionGrade.SectionGrade.Id));

                        if ((sectionGrade.Submission != null) && (sectionGrade.Submission.SubmissionReason != null))
                        {
                            var gradeChangeReason =
                                gradeChangeReasons.FirstOrDefault(
                                    x => x.Guid == sectionGrade.Submission.SubmissionReason.Id);
                            if (gradeChangeReason == null)
                                throw new KeyNotFoundException(
                                    string.Format("Grade Change Reason associated to guid '{0}' not found",
                                        sectionGrade.Submission.SubmissionReason.Id));

                            midTermGrade.GradeChangeReason = gradeChangeReason.Code;
                        }
                        midTermGrade.Grade = grade.LetterGrade;
                        midTermGrade.GradeKey = grade.Id;
                        request.MidTermGrades.Add(midTermGrade);
                    }

                }
            }
            if (registrationDto.Process != null)
            {
                if (registrationDto.Process.GradeExtension != null)
                {
                    request.GradeExtentionExpDate = registrationDto.Process.GradeExtension.ExpiresOn;
                }
            }

            if (registrationDto.Involvement != null)
            {
                request.InvolvementStartOn = registrationDto.Involvement.StartOn;
                request.InvolvementEndOn = registrationDto.Involvement.EndOn;
            }

            if (registrationDto.SectionRegistrationReporting != null)
            {
                if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance != null)
                {
                    if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance.Status ==
                        Dtos.ReportingStatusType.NeverAttended)
                    {
                        request.ReportingStatus = "Y";
                    }
                    else
                    {
                        request.ReportingStatus = "N";
                        request.ReportingLastDayOfAttendance =
                            registrationDto.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn;
                    }
                }
            }
        }

        /// <summary>
        /// Checks all the required fields in section registration Dto
        /// </summary>
        /// <param name="registrationDto">registrationDto</param>
        private void CheckForRequiredFields(Dtos.SectionRegistration2 registrationDto)
        {
            if (registrationDto == null)
                throw new ArgumentNullException("registrationDto", "Must provide a SectionRegistration object for update");

            if (string.IsNullOrEmpty(registrationDto.Id))
                throw new ArgumentNullException("registrationDto.id", "Must provide an id for section-registrations");

            if (registrationDto.Registrant == null)
                throw new ArgumentNullException("registrationDto.registrant", "Must provide a registrant object for update");

            if (string.IsNullOrEmpty(registrationDto.Registrant.Id))
                throw new ArgumentNullException("registrationDto.registrant.id", "Must provide an id for person");

            if (registrationDto.Section == null)
                throw new ArgumentNullException("registrationDto.section", "Must provide a section object for update");

            if (string.IsNullOrEmpty(registrationDto.Section.Id))
                throw new ArgumentNullException("registrationDto.section.id", "Must provide an id for section");

            if (registrationDto.Status == null)
                throw new ArgumentNullException("registrationDto.status", "Must provide a status object for update");

            if (registrationDto.Status.RegistrationStatus == null)
                throw new ArgumentNullException("registrationDto.status.registrationStatus", "Must provide registration status");

            if (registrationDto.Status.SectionRegistrationStatusReason == null)
                throw new ArgumentNullException("registrationDto.status.sectionregistrationstatusreason", "Must provide section registration status reason");

            if (registrationDto.Status.Detail != null && string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
                throw new ArgumentNullException("registrationDto.status.detail.id", "Must provide an id for status");

            if(registrationDto.AwardGradeScheme == null)
                throw new ArgumentNullException("registrationDto.awardgradescheme", "Must provide an award grade scheme for update");

            if (string.IsNullOrEmpty(registrationDto.AwardGradeScheme.Id))
                throw new ArgumentNullException("registrationDto.awardgradescheme.id", "Must provide an id for status");

            if (registrationDto.Approvals != null && registrationDto.Approvals.Any())
            {
                foreach (var approval in registrationDto.Approvals)
                {
                    if (approval.ApprovalEntity == null)
                        throw new ArgumentNullException("approval.approvalEntity", "Must provide an approval entity for approval");
                    if (approval.ApprovalType == null)
                        throw new ArgumentNullException("approval.approvalType", "Must provide an approval type for approval");
                }
            }

            if (registrationDto.Transcript != null)
            {
                if (registrationDto.Transcript.GradeScheme != null && string.IsNullOrEmpty(registrationDto.Transcript.GradeScheme.Id))
                    throw new ArgumentNullException("registrationDto.transcript.gradeScheme.id", "Must provide an id for transcript gradescheme");

                if (registrationDto.Transcript.Mode == null)
                {
                    throw new ArgumentNullException("registrationDto.transcript.mode", "Must provide transcript mode");
                }
            }

            if (registrationDto.SectionRegistrationGrades != null && registrationDto.SectionRegistrationGrades.Count() == 0)
                throw new ArgumentNullException("registrationDto.sectionRegistrationGrades", "Must provide section registration grades");

            if (registrationDto.SectionRegistrationGrades != null && registrationDto.SectionRegistrationGrades.Any())
            {
                foreach (var grade in registrationDto.SectionRegistrationGrades)
                {
                    if (grade == null)
                        throw new ArgumentNullException("sectionRegistrationGrade", "Must provide a grade object for update");

                    if (string.IsNullOrEmpty(grade.SectionGrade.Id))
                        throw new ArgumentNullException("registrationDto.sectionGrade.id", "Must provide an id for a section grade");

                    if (grade.SectionGradeType == null)
                        throw new ArgumentNullException("registrationDto.sectionGradeType", "Must provide a section gradetype");

                    if (string.IsNullOrEmpty(grade.SectionGradeType.Id))
                        throw new ArgumentNullException("registrationDto.sectionGradeType.id", "Must provide an id for a section gradetype");

                    if (grade.Submission != null)
                    {
                        if (grade.Submission.SubmittedBy != null && string.IsNullOrEmpty(grade.Submission.SubmittedBy.Id))
                            throw new ArgumentNullException("registrationDto.submission.submittedBy.id", "Must provide an id for grade submittedby");

                        if (grade.Submission.SubmissionReason != null && string.IsNullOrEmpty(grade.Submission.SubmissionReason.Id))
                            throw new ArgumentNullException("registrationDto.submission.submissionReason.id", "Must provide an id for grade submission reason");
                    }
                }
            }

            if (registrationDto.SectionRegistrationReporting != null && registrationDto.SectionRegistrationReporting.CountryCode == null)
                throw new ArgumentNullException("registrationDto.reporting.countryCode", "Must provide a country code for update");

            if (registrationDto.Process != null)
            {
                if (registrationDto.Process.GradeExtension != null)
                {
                    if (registrationDto.Process.GradeExtension.ExpiresOn == null)
                        throw new ArgumentNullException("registrationDto.Process.gradeExtension.expiresOn", "Must provide an expiresOn for grade process expiresOn date");

                    if (registrationDto.Process.GradeExtension.DefaultGrade != null && string.IsNullOrEmpty(registrationDto.Process.GradeExtension.DefaultGrade.Id))
                        throw new ArgumentNullException("registrationDto.Process.gradeExtension.defaultGrade", "Must provide an Id for the default grade");

                    if (registrationDto.Process.Transcript != null)
                    {
                        if (registrationDto.Process.Transcript.VerifiedOn == null)
                            throw new ArgumentNullException("registrationDto.Process.transcript.verifiedOn", "Must provide verifiedOn for the transcript");

                        if (registrationDto.Process.Transcript.VerifiedBy != null && string.IsNullOrEmpty(registrationDto.Process.Transcript.VerifiedBy.Id))
                            throw new ArgumentNullException("registrationDto.Process.Transcript.verifiedBy.id", "Must provide an Id for the VerifiedBy");
                    }
                }
            }
        }

        /// <summary>
        /// Checks all the required fields in section registration Dto
        /// </summary>
        /// <param name="registrationDto">registrationDto</param>
        private void CheckForRequiredFields2(Dtos.SectionRegistration3 registrationDto)
        {
            if (registrationDto == null)
                throw new ArgumentNullException("registrationDto", "Must provide a SectionRegistration object for update");

            if (string.IsNullOrEmpty(registrationDto.Id))
                throw new ArgumentNullException("registrationDto.id", "Must provide an id for section-registrations");

            if (registrationDto.Registrant == null)
                throw new ArgumentNullException("registrationDto.registrant", "Must provide a registrant object for update");

            if (string.IsNullOrEmpty(registrationDto.Registrant.Id))
                throw new ArgumentNullException("registrationDto.registrant.id", "Must provide an id for person");

            //V7 changes 
            //Academic Level
            if (registrationDto.AcademicLevel == null)
            {
                throw new ArgumentNullException("registrationDto.academicLevel", "Must provide academic level for person");
            }

            if (registrationDto.AcademicLevel != null && string.IsNullOrEmpty(registrationDto.AcademicLevel.Id))
            {
                throw new ArgumentNullException("registrationDto.academicLevel.id", "Must provide an id for academic level");
            }

            //Credit category
            if (registrationDto.Credit != null && registrationDto.Credit.CreditCategory == null)
            {
                throw new ArgumentNullException("registrationDto.credit.creditCategory", "Must provide a credit category for credit");
            }

            if (registrationDto.Credit != null && registrationDto.Credit.CreditCategory != null && registrationDto.Credit.CreditCategory.CreditType == null)
            {
                throw new ArgumentNullException("registrationDto.credit.creditCategory.creditType", "Must provide a credit category type for credit category");
            }

            if (registrationDto.Credit != null && registrationDto.Credit.CreditCategory != null && registrationDto.Credit.CreditCategory.Detail != null && 
                string.IsNullOrEmpty(registrationDto.Credit.CreditCategory.Detail.Id))
            {
                throw new ArgumentNullException("registrationDto.credit.creditCategory.creditType", "Must provide an id for credit category");
            }
            //End V7 changes

            if (registrationDto.Section == null)
                throw new ArgumentNullException("registrationDto.section", "Must provide a section object for update");

            if (string.IsNullOrEmpty(registrationDto.Section.Id))
                throw new ArgumentNullException("registrationDto.section.id", "Must provide an id for section");

            if (registrationDto.Status == null)
                throw new ArgumentNullException("registrationDto.status", "Must provide a status object for update");

            if (registrationDto.Status.RegistrationStatus == null)
                throw new ArgumentNullException("registrationDto.status.registrationStatus", "Must provide registration status");

            if (registrationDto.Status.SectionRegistrationStatusReason == null)
                throw new ArgumentNullException("registrationDto.status.sectionregistrationstatusreason", "Must provide section registration status reason");

            if (registrationDto.Status.Detail != null && string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
                throw new ArgumentNullException("registrationDto.status.detail.id", "Must provide an id for status");

            if (registrationDto.AwardGradeScheme == null)
                throw new ArgumentNullException("registrationDto.awardgradescheme", "Must provide an award grade scheme for update");

            if (string.IsNullOrEmpty(registrationDto.AwardGradeScheme.Id))
                throw new ArgumentNullException("registrationDto.awardgradescheme.id", "Must provide an id for status");

            if (registrationDto.Approvals != null && registrationDto.Approvals.Any())
            {
                foreach (var approval in registrationDto.Approvals)
                {
                    if (approval.ApprovalEntity == null)
                        throw new ArgumentNullException("approval.approvalEntity", "Must provide an approval entity for approval");
                    if (approval.ApprovalType == null)
                        throw new ArgumentNullException("approval.approvalType", "Must provide an approval type for approval");
                }
            }

            if (registrationDto.Transcript != null)
            {
                if (registrationDto.Transcript.GradeScheme != null && string.IsNullOrEmpty(registrationDto.Transcript.GradeScheme.Id))
                    throw new ArgumentNullException("registrationDto.transcript.gradeScheme.id", "Must provide an id for transcript gradescheme");

                if (registrationDto.Transcript.Mode == null)
                {
                    throw new ArgumentNullException("registrationDto.transcript.mode", "Must provide transcript mode");
                }
            }

            if (registrationDto.SectionRegistrationGrades != null && registrationDto.SectionRegistrationGrades.Count() == 0)
                throw new ArgumentNullException("registrationDto.sectionRegistrationGrades", "Must provide section registration grades");

            if (registrationDto.SectionRegistrationGrades != null && registrationDto.SectionRegistrationGrades.Any())
            {
                foreach (var grade in registrationDto.SectionRegistrationGrades)
                {
                    if (grade == null)
                        throw new ArgumentNullException("sectionRegistrationGrade", "Must provide a grade object for update");

                    if (string.IsNullOrEmpty(grade.SectionGrade.Id))
                        throw new ArgumentNullException("registrationDto.sectionGrade.id", "Must provide an id for a section grade");

                    if (grade.SectionGradeType == null)
                        throw new ArgumentNullException("registrationDto.sectionGradeType", "Must provide a section gradetype");

                    if (string.IsNullOrEmpty(grade.SectionGradeType.Id))
                        throw new ArgumentNullException("registrationDto.sectionGradeType.id", "Must provide an id for a section gradetype");

                    if (grade.Submission != null)
                    {
                        if (grade.Submission.SubmittedBy != null && string.IsNullOrEmpty(grade.Submission.SubmittedBy.Id))
                            throw new ArgumentNullException("registrationDto.submission.submittedBy.id", "Must provide an id for grade submittedby");

                        if (grade.Submission.SubmissionReason != null && string.IsNullOrEmpty(grade.Submission.SubmissionReason.Id))
                            throw new ArgumentNullException("registrationDto.submission.submissionReason.id", "Must provide an id for grade submission reason");
                    }
                }
            }

            if (registrationDto.SectionRegistrationReporting != null && registrationDto.SectionRegistrationReporting.CountryCode == null)
                throw new ArgumentNullException("registrationDto.reporting.countryCode", "Must provide a country code for update");

            if (registrationDto.Process != null)
            {
                if (registrationDto.Process.GradeExtension != null)
                {
                    if (registrationDto.Process.GradeExtension.ExpiresOn == null)
                        throw new ArgumentNullException("registrationDto.Process.gradeExtension.expiresOn", "Must provide an expiresOn for grade process expiresOn date");

                    if (registrationDto.Process.GradeExtension.DefaultGrade != null && string.IsNullOrEmpty(registrationDto.Process.GradeExtension.DefaultGrade.Id))
                        throw new ArgumentNullException("registrationDto.Process.gradeExtension.defaultGrade", "Must provide an Id for the default grade");

                    if (registrationDto.Process.Transcript != null)
                    {
                        if (registrationDto.Process.Transcript.VerifiedOn == null)
                            throw new ArgumentNullException("registrationDto.Process.transcript.verifiedOn", "Must provide verifiedOn for the transcript");

                        if (registrationDto.Process.Transcript.VerifiedBy != null && string.IsNullOrEmpty(registrationDto.Process.Transcript.VerifiedBy.Id))
                            throw new ArgumentNullException("registrationDto.Process.Transcript.verifiedBy.id", "Must provide an Id for the VerifiedBy");
                    }
                }
            }
        }

        /// <summary>
        /// Checks all the business rules
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="registrationDto">SectionRegistration2 object</param>
        /// <returns></returns>
        private async Task CheckForBusinessRulesAsync(string guid, Dtos.SectionRegistration2 registrationDto)
        {
           #region Validate award & transcript id and section
            if (_gradeSchemes == null) _gradeSchemes = await this.GradeSchemesAsync();

            var awardGradeScheme = _gradeSchemes.FirstOrDefault(gs => gs.Guid == registrationDto.AwardGradeScheme.Id);
            if (awardGradeScheme == null)
                throw new KeyNotFoundException("Provided awardgradescheme id is invalid");

            if ((registrationDto.Transcript!= null) && (registrationDto.Transcript.GradeScheme != null))
            {
                var transcriptGradeScheme =
                    _gradeSchemes.FirstOrDefault(gs => gs.Guid == registrationDto.Transcript.GradeScheme.Id);

                if (transcriptGradeScheme == null)
                    throw new KeyNotFoundException("Provided transcriptGradeScheme id is invalid");
            }
            //Compare award grade scheme & grade scheme
            if (registrationDto.AwardGradeScheme != null && registrationDto.Transcript!= null && registrationDto.Transcript.GradeScheme != null)
            {
                if (!registrationDto.AwardGradeScheme.Id.Equals(registrationDto.Transcript.GradeScheme.Id))
                    throw new ArgumentException("Colleague requires that the awardGradeScheme be the same as the transcript.GradeScheme");
            }
            //Check transcript mode withdraw
            if (registrationDto.Transcript != null && registrationDto.Status != null)
            {
                if (registrationDto.Transcript.Mode != null && 
                    registrationDto.Status.RegistrationStatus != null & registrationDto.Status.SectionRegistrationStatusReason != null)
                { 
                    if(registrationDto.Transcript.Mode == Dtos.TranscriptMode.Withdraw && 
                       (registrationDto.Status.SectionRegistrationStatusReason != Dtos.RegistrationStatusReason2.Withdrawn ||
                        registrationDto.Status.RegistrationStatus != Dtos.RegistrationStatus2.NotRegistered))
                        throw new InvalidOperationException("For transcript mode withdraw, status registrationStatus code should be notRegistered and sectionRegistrationStatusReason should be withdrawn");

                }
            }

            var section = await _sectionRepository.GetSectionByGuidAsync(registrationDto.Section.Id);
            if (section == null)
                throw new KeyNotFoundException("Provided sectionId is invalid");
            //HED-3214
            var sectionGradeScheme = _gradeSchemes.FirstOrDefault(gs => gs.Code.Equals(section.GradeSchemeCode, StringComparison.OrdinalIgnoreCase));
            if (sectionGradeScheme != null && registrationDto.AwardGradeScheme != null && registrationDto.Transcript != null && registrationDto.Transcript.GradeScheme != null)
            {
                if (!sectionGradeScheme.Guid.Equals(registrationDto.AwardGradeScheme.Id, StringComparison.OrdinalIgnoreCase) &&
                    !sectionGradeScheme.Guid.Equals(registrationDto.Transcript.GradeScheme.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The section grade scheme does not match the award grade scheme and transcript grade scheme.");
                }
            }

            #endregion

            #region Grades

            gradeEntities = await GetGradeHedmAsync(true);
            var sectionGradeTypeDtos = await GetSectionGradeTypesAsync(false);
            var finalGradeType = sectionGradeTypeDtos.FirstOrDefault(g => g.Code.Equals("FINAL"));

            if (registrationDto.SectionRegistrationGrades != null)
            {
                foreach (var sectionRegistrationGrade in registrationDto.SectionRegistrationGrades)
                {
                    //Check if the grades in registrationDto are valid
                    var tempGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == sectionRegistrationGrade.SectionGrade.Id);
                    if (tempGradeEntity == null)
                        throw new KeyNotFoundException(string.Format("The grade submitted with id '{0}' is not a valid grade for the student in the section.", sectionRegistrationGrade.SectionGrade.Id));

                    //Check if the grade type of each grade is valid
                    var anyGradeType = sectionGradeTypeDtos.Any(gt => gt.Id == sectionRegistrationGrade.SectionGradeType.Id);
                    if (!anyGradeType)
                        throw new KeyNotFoundException(string.Format("The grade type with id '{0}' is not a valid Colleague grade type and is not permitted in Colleague.", sectionRegistrationGrade.SectionGradeType.Id));

                    //Check if the grades grade-scheme is valid
                    var anyGradeScheme = _gradeSchemes.Any(gsch => gsch.Code == tempGradeEntity.GradeSchemeCode);
                    if (!anyGradeScheme)
                        throw new KeyNotFoundException(string.Format("The grade submission with id '{0}' for a specific grade type was not in the corresponding awardGradeScheme.", sectionRegistrationGrade.SectionGrade.Id));

                    //Check if the grade submittedby is a valid person in Colleague
                    if (sectionRegistrationGrade.Submission != null)
                    {
                        if (sectionRegistrationGrade.Submission.SubmittedBy != null && !string.IsNullOrEmpty(sectionRegistrationGrade.Submission.SubmittedBy.Id))
                        {
                            var submittedById = await _personRepository.GetPersonIdFromGuidAsync(sectionRegistrationGrade.Submission.SubmittedBy.Id);
                            if (string.IsNullOrEmpty(submittedById))
                                throw new KeyNotFoundException("The person who submitted the grade is not a valid person in Colleague.");
                        }
                    }
                }
            }

            #endregion           

            #region Process
            if (registrationDto.Process == null)
            {
                if (finalGradeType != null)
                {
                    if (registrationDto.SectionRegistrationGrades != null)
                    {
                        var finalGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault(g => g.SectionGradeType.Id == finalGradeType.Id);
                        if (registrationDto.Transcript != null && registrationDto.Transcript.Mode != null)
                        {
                            if (registrationDto.Transcript.Mode == Dtos.TranscriptMode.Incomplete && finalGrade != null)
                            {
                                throw new ArgumentNullException("An expiration date is required when the transcript mode is incomplete.");
                            }
                        }
                        //HED-3505: Passing in a midterm grade and a final grade (that has a corresponding incomplete grade) with NO expiresOn date
                        //The midterm grade posts as expected but no final grade was posted. The API errored with a message of: 
                        //"Expiration date is required with a final grade of A". I'm assuming this is intended behavior...? When the API fails - 
                        //should it fail completely or is a partial fail acceptable?
                        if (finalGrade != null)
                        {
                            Domain.Student.Entities.Grade finalGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == finalGrade.SectionGrade.Id);
                            if (finalGradeEntity != null)
                            {
                                Domain.Student.Entities.Grade defaultGradeEntity = gradeEntities.FirstOrDefault(g => g.Id == finalGradeEntity.IncompleteGrade);
                                if (defaultGradeEntity != null)
                                {
                                    throw new ArgumentNullException("An expiration date is required when an final grade has a corresponding incomplete grade.");
                                }
                            }
                        }
                    }
                }
            }

            if (registrationDto.Process != null)
            {
                //HED-3222 Passing in an incomplete grade with an expiresOn date in the PAST marks the record in Colleague as a VERIFIED grade. 
                //This is incorrect as the API isn't allowed to verify a grade. The result should be that the final grade needs to have an expiresOn date in the future.  
                //Or, the expiresOn date in the past shouldn't be accepted at all.
                if (registrationDto.Process.GradeExtension != null)
                {
                    if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn < DateTime.Today)
                    {
                        throw new InvalidOperationException("The grade extension expiresOn date must be in future.");
                    }
                }
                //Check transcript verifiedby guid
                if (registrationDto.Process.Transcript != null)
                {

                    if (registrationDto.Process.Transcript.VerifiedBy != null && !string.IsNullOrEmpty(registrationDto.Process.Transcript.VerifiedBy.Id))
                    {
                        var verifiedById = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Process.Transcript.VerifiedBy.Id);
                        if (string.IsNullOrEmpty(verifiedById))
                            throw new KeyNotFoundException("The person who verified the grade is not a valid person in Colleague");
                    }
                    //Check transcript verifiedOn before section start date
                    if (registrationDto.Process.Transcript.VerifiedOn != null && section.StartDate != null)
                    {
                        if (registrationDto.Process.Transcript.VerifiedOn.Value.Date < section.StartDate.Date)
                        {
                            throw new ArgumentNullException("The grade verification date cannot be prior to the section start date.");
                        }
                    }
                }

                //logic to check final grade, default grade checks, incomplete                
                if (finalGradeType != null)
                {
                    if (registrationDto.SectionRegistrationGrades != null)
                    {
                        var finalGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault(g => g.SectionGradeType.Id == finalGradeType.Id);
                        //checks if ExpireOn has value & final grade is null
                        if (registrationDto.Process.GradeExtension != null)
                        {
                            if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn.HasValue && finalGrade == null)
                            {
                                throw new ArgumentNullException("A final grade is required when the grade expiration date is submitted.");
                            }
                        }

                        if (registrationDto.Transcript != null && registrationDto.Transcript.Mode != null)
                        {
                            //if the mode is incomplete then the grade needs to be a final type grade
                            if (registrationDto.Transcript.Mode == Dtos.TranscriptMode.Incomplete && finalGrade == null)
                            {
                                throw new ArgumentNullException("A final grade is required when the transcript mode is 'incomplete'.");
                            }
                        }

                        if (finalGrade != null)
                        {
                            Domain.Student.Entities.Grade finalGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == finalGrade.SectionGrade.Id);

                            if (finalGradeEntity != null)
                            {
                                if (registrationDto.Process.GradeExtension != null)
                                {
                                    if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn.HasValue && 
                                        string.IsNullOrEmpty(finalGradeEntity.IncompleteGrade))
                                    {
                                        throw new ArgumentNullException("Only grades defined in the Colleague GRADES file that have a corresponding incomplete grade (GRD.INCOMPLETE.GRADE) can be given an expiration date in Colleague.");
                                    }

                                    Domain.Student.Entities.Grade defaultGradeEntity = gradeEntities.FirstOrDefault(g => g.Id == finalGradeEntity.IncompleteGrade);
                                    if (defaultGradeEntity != null)
                                    {
                                        if (registrationDto.Process.GradeExtension.DefaultGrade != null && 
                                            !registrationDto.Process.GradeExtension.DefaultGrade.Id.Equals(defaultGradeEntity.Guid, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            throw new ArgumentNullException("The default grade must correspond to the final grade specified for the student.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Involvement
            //Check involvement startOn before section start date
            if (registrationDto.Involvement != null)
            {
                if (registrationDto.Involvement.StartOn != null && registrationDto.Involvement.EndOn != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > registrationDto.Involvement.EndOn.Value.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be after the participation end date.");
                    }
                }

                if (registrationDto.Involvement.StartOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date < section.StartDate.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be before the section start date.");
                    }
                }
                //Check involvement startOn after section end date
                if (registrationDto.Involvement.StartOn != null && section.EndDate != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > section.EndDate.Value.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be after the section end date.");
                    }
                }
                //Check involvement endOn after section start date
                if (registrationDto.Involvement.EndOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.EndOn.Value.Date < section.StartDate.Date)
                    {
                        throw new InvalidOperationException("The participation end date can't be before the section start date.");
                    }
                }
            }

            #endregion

            #region Reporting

            if (registrationDto.SectionRegistrationReporting != null)
            {
                if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance != null)
                {
                    if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance.Status == Dtos.ReportingStatusType.NeverAttended &&
                        registrationDto.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn != null)
                        throw new InvalidOperationException("A student who never attended should not have a last date of attendance specified.");
                }
            }
            #endregion
        }

        /// <summary>
        /// Checks all the business rules
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="registrationDto">SectionRegistration2 object</param>
        /// <returns></returns>
        private async Task CheckForBusinessRules2Async(string guid, Dtos.SectionRegistration3 registrationDto)
        {
            #region Validate award & transcript id and section
            if (_gradeSchemes == null) _gradeSchemes = await this.GradeSchemesAsync();

            var awardGradeScheme = _gradeSchemes.FirstOrDefault(gs => gs.Guid == registrationDto.AwardGradeScheme.Id);
            if (awardGradeScheme == null)
                throw new KeyNotFoundException("Provided awardgradescheme id is invalid");

            if ((registrationDto.Transcript != null) && (registrationDto.Transcript.GradeScheme != null))
            {
                var transcriptGradeScheme =
                    _gradeSchemes.FirstOrDefault(gs => gs.Guid == registrationDto.Transcript.GradeScheme.Id);

                if (transcriptGradeScheme == null)
                    throw new KeyNotFoundException("Provided transcriptGradeScheme id is invalid");
            }
            //Compare award grade scheme & grade scheme
            if (registrationDto.AwardGradeScheme != null && registrationDto.Transcript != null && registrationDto.Transcript.GradeScheme != null)
            {
                if (!registrationDto.AwardGradeScheme.Id.Equals(registrationDto.Transcript.GradeScheme.Id))
                    throw new ArgumentException("Colleague requires that the awardGradeScheme be the same as the transcript.GradeScheme");
            }
            //Check transcript mode withdraw
            if (registrationDto.Transcript != null && registrationDto.Status != null)
            {
                if (registrationDto.Transcript.Mode != null &&
                    registrationDto.Status.RegistrationStatus != null & registrationDto.Status.SectionRegistrationStatusReason != null)
                {
                    if (registrationDto.Transcript.Mode == Dtos.TranscriptMode.Withdraw &&
                       (registrationDto.Status.SectionRegistrationStatusReason != Dtos.RegistrationStatusReason2.Withdrawn ||
                        registrationDto.Status.RegistrationStatus != Dtos.RegistrationStatus2.NotRegistered))
                        throw new InvalidOperationException("For transcript mode withdraw, status registrationStatus code should be notRegistered and sectionRegistrationStatusReason should be withdrawn");

                }
            }

            var section = await _sectionRepository.GetSectionByGuidAsync(registrationDto.Section.Id);
            if (section == null)
                throw new KeyNotFoundException("Provided sectionId is invalid");
            //HED-3214
            var sectionGradeScheme = _gradeSchemes.FirstOrDefault(gs => gs.Code.Equals(section.GradeSchemeCode, StringComparison.OrdinalIgnoreCase));
            if (sectionGradeScheme != null && registrationDto.AwardGradeScheme != null && registrationDto.Transcript != null && registrationDto.Transcript.GradeScheme != null)
            {
                if (!sectionGradeScheme.Guid.Equals(registrationDto.AwardGradeScheme.Id, StringComparison.OrdinalIgnoreCase) &&
                    !sectionGradeScheme.Guid.Equals(registrationDto.Transcript.GradeScheme.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The section grade scheme does not match the award grade scheme and transcript grade scheme.");
                }
            }

            #endregion

            #region Grades

            gradeEntities = await GetGradeHedmAsync(true);
            var sectionGradeTypeDtos = await GetSectionGradeTypesAsync(false);
            var finalGradeType = sectionGradeTypeDtos.FirstOrDefault(g => g.Code.Equals("FINAL"));

            if (registrationDto.SectionRegistrationGrades != null)
            {
                foreach (var sectionRegistrationGrade in registrationDto.SectionRegistrationGrades)
                {
                    //Check if the grades in registrationDto are valid
                    var tempGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == sectionRegistrationGrade.SectionGrade.Id);
                    if (tempGradeEntity == null)
                        throw new KeyNotFoundException(string.Format("The grade submitted with id '{0}' is not a valid grade for the student in the section.", sectionRegistrationGrade.SectionGrade.Id));

                    //Check if the grade type of each grade is valid
                    var anyGradeType = sectionGradeTypeDtos.Any(gt => gt.Id == sectionRegistrationGrade.SectionGradeType.Id);
                    if (!anyGradeType)
                        throw new KeyNotFoundException(string.Format("The grade type with id '{0}' is not a valid Colleague grade type and is not permitted in Colleague.", sectionRegistrationGrade.SectionGradeType.Id));

                    //Check if the grades grade-scheme is valid
                    var anyGradeScheme = _gradeSchemes.Any(gsch => gsch.Code == tempGradeEntity.GradeSchemeCode);
                    if (!anyGradeScheme)
                        throw new KeyNotFoundException(string.Format("The grade submission with id '{0}' for a specific grade type was not in the corresponding awardGradeScheme.", sectionRegistrationGrade.SectionGrade.Id));

                    //Check if the grade submittedby is a valid person in Colleague
                    if (sectionRegistrationGrade.Submission != null)
                    {
                        if (sectionRegistrationGrade.Submission.SubmittedBy != null && !string.IsNullOrEmpty(sectionRegistrationGrade.Submission.SubmittedBy.Id))
                        {
                            var submittedById = await _personRepository.GetPersonIdFromGuidAsync(sectionRegistrationGrade.Submission.SubmittedBy.Id);
                            if (string.IsNullOrEmpty(submittedById))
                                throw new KeyNotFoundException("The person who submitted the grade is not a valid person in Colleague.");
                        }
                    }
                }
            }

            #endregion

            #region Process
            if (registrationDto.Process == null)
            {
                if (finalGradeType != null)
                {
                    if (registrationDto.SectionRegistrationGrades != null)
                    {
                        var finalGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault(g => g.SectionGradeType.Id == finalGradeType.Id);
                        if (registrationDto.Transcript != null && registrationDto.Transcript.Mode != null)
                        {
                            if (registrationDto.Transcript.Mode == Dtos.TranscriptMode.Incomplete && finalGrade != null)
                            {
                                throw new ArgumentNullException("An expiration date is required when the transcript mode is incomplete.");
                            }
                        }
                        //HED-3505: Passing in a midterm grade and a final grade (that has a corresponding incomplete grade) with NO expiresOn date
                        //The midterm grade posts as expected but no final grade was posted. The API errored with a message of: 
                        //"Expiration date is required with a final grade of A". I'm assuming this is intended behavior...? When the API fails - 
                        //should it fail completely or is a partial fail acceptable?
                        if (finalGrade != null)
                        {
                            Domain.Student.Entities.Grade finalGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == finalGrade.SectionGrade.Id);
                            if (finalGradeEntity != null)
                            {
                                Domain.Student.Entities.Grade defaultGradeEntity = gradeEntities.FirstOrDefault(g => g.Id == finalGradeEntity.IncompleteGrade);
                                if (defaultGradeEntity != null)
                                {
                                    throw new ArgumentNullException("An expiration date is required when an final grade has a corresponding incomplete grade.");
                                }
                            }
                        }
                    }
                }
            }

            if (registrationDto.Process != null)
            {
                //HED-3222 Passing in an incomplete grade with an expiresOn date in the PAST marks the record in Colleague as a VERIFIED grade. 
                //This is incorrect as the API isn't allowed to verify a grade. The result should be that the final grade needs to have an expiresOn date in the future.  
                //Or, the expiresOn date in the past shouldn't be accepted at all.
                if (registrationDto.Process.GradeExtension != null)
                {
                    if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn < DateTime.Today)
                    {
                        throw new InvalidOperationException("The grade extension expiresOn date must be in future.");
                    }
                }
                //Check transcript verifiedby guid
                if (registrationDto.Process.Transcript != null)
                {

                    if (registrationDto.Process.Transcript.VerifiedBy != null && !string.IsNullOrEmpty(registrationDto.Process.Transcript.VerifiedBy.Id))
                    {
                        var verifiedById = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Process.Transcript.VerifiedBy.Id);
                        if (string.IsNullOrEmpty(verifiedById))
                            throw new KeyNotFoundException("The person who verified the grade is not a valid person in Colleague");
                    }
                    //Check transcript verifiedOn before section start date
                    if (registrationDto.Process.Transcript.VerifiedOn != null && section.StartDate != null)
                    {
                        if (registrationDto.Process.Transcript.VerifiedOn.Value.Date < section.StartDate.Date)
                        {
                            throw new ArgumentNullException("The grade verification date cannot be prior to the section start date.");
                        }
                    }
                }

                //logic to check final grade, default grade checks, incomplete                
                if (finalGradeType != null)
                {
                    if (registrationDto.SectionRegistrationGrades != null)
                    {
                        var finalGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault(g => g.SectionGradeType.Id == finalGradeType.Id);
                        //checks if ExpireOn has value & final grade is null
                        if (registrationDto.Process.GradeExtension != null)
                        {
                            if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn.HasValue && finalGrade == null)
                            {
                                throw new ArgumentNullException("A final grade is required when the grade expiration date is submitted.");
                            }
                        }

                        if (registrationDto.Transcript != null && registrationDto.Transcript.Mode != null)
                        {
                            //if the mode is incomplete then the grade needs to be a final type grade
                            if (registrationDto.Transcript.Mode == Dtos.TranscriptMode.Incomplete && finalGrade == null)
                            {
                                throw new ArgumentNullException("A final grade is required when the transcript mode is 'incomplete'.");
                            }
                        }

                        if (finalGrade != null)
                        {
                            Domain.Student.Entities.Grade finalGradeEntity = gradeEntities.FirstOrDefault(g => g.Guid == finalGrade.SectionGrade.Id);

                            if (finalGradeEntity != null)
                            {
                                if (registrationDto.Process.GradeExtension != null)
                                {
                                    if (registrationDto.Process.GradeExtension.ExpiresOn != null && registrationDto.Process.GradeExtension.ExpiresOn.HasValue &&
                                        string.IsNullOrEmpty(finalGradeEntity.IncompleteGrade))
                                    {
                                        throw new ArgumentNullException("Only grades defined in the Colleague GRADES file that have a corresponding incomplete grade (GRD.INCOMPLETE.GRADE) can be given an expiration date in Colleague.");
                                    }

                                    Domain.Student.Entities.Grade defaultGradeEntity = gradeEntities.FirstOrDefault(g => g.Id == finalGradeEntity.IncompleteGrade);
                                    if (defaultGradeEntity != null)
                                    {
                                        if (registrationDto.Process.GradeExtension.DefaultGrade != null &&
                                            !registrationDto.Process.GradeExtension.DefaultGrade.Id.Equals(defaultGradeEntity.Guid, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            throw new ArgumentNullException("The default grade must correspond to the final grade specified for the student.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Involvement
            //Check involvement startOn before section start date
            if (registrationDto.Involvement != null)
            {
                if (registrationDto.Involvement.StartOn != null && registrationDto.Involvement.EndOn != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > registrationDto.Involvement.EndOn.Value.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be after the participation end date.");
                    }
                }

                if (registrationDto.Involvement.StartOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date < section.StartDate.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be before the section start date.");
                    }
                }
                //Check involvement startOn after section end date
                if (registrationDto.Involvement.StartOn != null && section.EndDate != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > section.EndDate.Value.Date)
                    {
                        throw new InvalidOperationException("The participation start date can't be after the section end date.");
                    }
                }
                //Check involvement endOn after section start date
                if (registrationDto.Involvement.EndOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.EndOn.Value.Date < section.StartDate.Date)
                    {
                        throw new InvalidOperationException("The participation end date can't be before the section start date.");
                    }
                }
            }

            #endregion

            #region Reporting

            if (registrationDto.SectionRegistrationReporting != null)
            {
                if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance != null)
                {
                    if (registrationDto.SectionRegistrationReporting.LastDayOfAttendance.Status == Dtos.ReportingStatusType.NeverAttended &&
                        registrationDto.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn != null)
                        throw new InvalidOperationException("A student who never attended should not have a last date of attendance specified.");
                }
            }
            #endregion
        }
        
        #endregion
        
        #region Convert Dtos and Entities

        /// <summary>
        /// Convert a Dto to a registration request.
        /// </summary>
        /// <param name="sectionRegistrationDto">The section <see cref="Dtos.SectionRegistration">registration</see> Object.</param>
        /// <returns>Registration <see cref="RegistrationRequest">request</see></returns>
        private RegistrationRequest ConvertDtotoRequestEntity(Dtos.SectionRegistration sectionRegistrationDto, string personId, string sectionId)
        {
            Ellucian.Colleague.Domain.Student.Entities.SectionRegistration sectionRegistration = new Domain.Student.Entities.SectionRegistration();
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistration> sectionRegistrations = new List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistration>();

            // Set appropriate action in the request.
            switch (sectionRegistrationDto.Status.RegistrationStatus)
            {
                case Dtos.RegistrationStatus.Registered:
                    {
                        sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                        break;
                    }
                case Dtos.RegistrationStatus.NotRegistered:
                    {
                        if (sectionRegistrationDto.Status.SectionRegistrationStatusReason == Dtos.RegistrationStatusReason.Pending)
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Waitlist;
                        }
                        else
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Drop;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            // sectionRegistration.RegistrationDate = DateTime.Now;
            // Set section ID and add to list of sections
            sectionRegistration.SectionId = sectionId;
            sectionRegistration.Guid = sectionRegistrationDto.Guid;
            sectionRegistrations.Add(sectionRegistration);
            // Return a new registration request object
            return new RegistrationRequest(personId, sectionRegistrations) { CreateStudentFlag = true };
        }
        /// <summary>
        /// Convert the Registration Response into the DTO response for Section Registration
        /// </summary>
        /// <param name="registrationDto">Original Section Registration object</param>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns>SectionRegistration Object <see cref="Dtos.SectionRegistration"/></returns>
        private Dtos.SectionRegistration ConvertResponsetoDto(Dtos.SectionRegistration registrationDto, Domain.Student.Entities.RegistrationResponse response)
        {
            var sectionRegistration = new Dtos.SectionRegistration()
            {
                Approvals = registrationDto.Approvals,
                Guid = registrationDto.Guid,
                MetadataObject = registrationDto.MetadataObject,
                Registrant = registrationDto.Registrant,
                Section = registrationDto.Section,
                Status = registrationDto.Status
            };
            // sectionRegistration.Status.Detail = response.StatusGuid;
            return sectionRegistration;
        }

        /// <summary>
        /// Convert a Dto to a registration request.
        /// </summary>
        /// <param name="sectionRegistrationDto">The section <see cref="Dtos.SectionRegistration">registration</see> Object.</param>
        /// <returns>Registration <see cref="RegistrationRequest">request</see></returns>
        private SectionRegistrationRequest ConvertDtoToRequest2Entity(Dtos.SectionRegistration2 sectionRegistrationDto, string personId, string sectionId)
        {
            Ellucian.Colleague.Domain.Student.Entities.SectionRegistration sectionRegistration = new Domain.Student.Entities.SectionRegistration();

            // Set appropriate action in the request.
            switch (sectionRegistrationDto.Status.RegistrationStatus)
            {
                case Dtos.RegistrationStatus2.Registered:
                    {
                        //Transcript is not required so if its null then assign Add as default Action
                        if (sectionRegistrationDto.Transcript == null || sectionRegistrationDto.Transcript.Mode == null)
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                        }
                        else
                        {
                            // Using Transcript Mode, update Pass/Fail or Audit flag.
                            switch (sectionRegistrationDto.Transcript.Mode)
                            {
                                case Dtos.TranscriptMode.Audit:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Audit;
                                    break;
                                case Dtos.TranscriptMode.PassFail:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.PassFail;
                                    break;
                                default:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                                    break;
                            }
                        }
                    }
                    break;
                case Dtos.RegistrationStatus2.NotRegistered:
                    {
                        if (sectionRegistrationDto.Status.SectionRegistrationStatusReason == Dtos.RegistrationStatusReason2.Pending)
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Waitlist;
                        }
                        else
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Drop;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            sectionRegistration.SectionId = sectionId;
            // Return a new registration request object
            return new SectionRegistrationRequest(personId, sectionRegistrationDto.Id, sectionRegistration) { CreateStudentFlag = true };
        }

        /// <summary>
        /// Convert a Dto to a registration request.
        /// </summary>
        /// <param name="sectionRegistrationDto">The section <see cref="Dtos.SectionRegistration">registration</see> Object.</param>
        /// <returns>Registration <see cref="RegistrationRequest">request</see></returns>
        private async Task<SectionRegistrationRequest> ConvertDtoToRequest3Entity(Dtos.SectionRegistration3 sectionRegistrationDto, string personId, string sectionId)
        {
            Ellucian.Colleague.Domain.Student.Entities.SectionRegistration sectionRegistration = new Domain.Student.Entities.SectionRegistration();

            // Set appropriate action in the request.
            switch (sectionRegistrationDto.Status.RegistrationStatus)
            {
                case Dtos.RegistrationStatus2.Registered:
                    {
                        //Transcript is not required so if its null then assign Add as default Action
                        if (sectionRegistrationDto.Transcript == null || sectionRegistrationDto.Transcript.Mode == null)
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                        }
                        else
                        {
                            // Using Transcript Mode, update Pass/Fail or Audit flag.
                            switch (sectionRegistrationDto.Transcript.Mode)
                            {
                                case Dtos.TranscriptMode.Audit:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Audit;
                                    break;
                                case Dtos.TranscriptMode.PassFail:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.PassFail;
                                    break;
                                default:
                                    sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                                    break;
                            }
                        }
                    }
                    break;
                case Dtos.RegistrationStatus2.NotRegistered:
                    {
                        if (sectionRegistrationDto.Status.SectionRegistrationStatusReason == Dtos.RegistrationStatusReason2.Pending)
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Waitlist;
                        }
                        else
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Drop;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            sectionRegistration.SectionId = sectionId;

            //Academic levels
            if (sectionRegistrationDto.AcademicLevel != null && !string.IsNullOrEmpty(sectionRegistrationDto.AcademicLevel.Id))
            {
                var academicLevel = (await AcademicLevelsAsync()).FirstOrDefault(al => al.Guid.Equals(sectionRegistrationDto.AcademicLevel.Id, StringComparison.OrdinalIgnoreCase));
                if (academicLevel == null)
                {
                    throw new KeyNotFoundException("Academic level ID associated to guid '" + sectionRegistrationDto.AcademicLevel.Id + "' not found");
                }
                sectionRegistration.AcademicLevelCode = academicLevel.Code;
            }

            //Attempted credit or ceus
            if (sectionRegistrationDto.Credit != null && sectionRegistrationDto.Credit.Measure != null)
            {
                if (sectionRegistrationDto.Credit.Measure == Dtos.CreditMeasure2.Credit || sectionRegistrationDto.Credit.Measure == Dtos.CreditMeasure2.Hours)
                {
                    if (sectionRegistrationDto.Credit.AttemptedCredit != null)
                    {
                        sectionRegistration.Credits = sectionRegistrationDto.Credit.AttemptedCredit;
                    }
                }
                if (sectionRegistrationDto.Credit.Measure == Dtos.CreditMeasure2.CEU)
                {
                    if (sectionRegistrationDto.Credit.AttemptedCredit != null)
                    {
                        sectionRegistration.Ceus = sectionRegistrationDto.Credit.AttemptedCredit;
                    }
                }
            }
            // Return a new registration request object
            return new SectionRegistrationRequest(personId, sectionRegistrationDto.Id, sectionRegistration) { CreateStudentFlag = true };
        }

        /// <summary>
        /// Convert the Registration Response into the DTO response for Section Registration
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns>SectionRegistration Object <see cref="Dtos.SectionRegistration2"/></returns>
        private async Task<Dtos.SectionRegistration2> ConvertResponsetoDto2(SectionRegistrationResponse response)
        {
            var gradeSchemeGuid = ConvertCodeToGuid(await GradeSchemesAsync(), response.GradeScheme);
            var sectionRegistration = new Dtos.SectionRegistration2()
            {
                Approvals = new List<Dtos.Approval2>() { new Dtos.Approval2() { ApprovalType = Dtos.ApprovalType2.All, ApprovalEntity = Dtos.ApprovalEntity.System } },
                Id = response.Id,
                Registrant = new Dtos.GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(response.StudentId)),
                Section = new Dtos.GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(response.SectionId)),
                Status = await ConvertResponseStatusToRegistrationStatusAsync(response.StatusCode),
                AwardGradeScheme = new Dtos.GuidObject2(gradeSchemeGuid),
                Transcript = new Dtos.SectionRegistrationTranscript() { GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid), Mode = await ConvertPassAuditToMode(response) },
                SectionRegistrationGrades = await ConvertResponseGradesToSectionRegistrationGradesAsync(response),
                Process = await ConvertResponsetoSectionRegistrationProcessAsync(response),
                Involvement = ConvertResponseSectionRegistrationToInvolvement(response),
                SectionRegistrationReporting = ConvertResponseToSectionRegistrationReporting(response)
            };

            return sectionRegistration;
        }

        /// <summary>
        /// Convert the Registration Response into the DTO response for Section Registration
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns>SectionRegistration Object <see cref="Dtos.SectionRegistration2"/></returns>
        private async Task<Dtos.SectionRegistration3> ConvertResponsetoDto3(SectionRegistrationResponse response)
        {
            var gradeSchemeGuid = ConvertCodeToGuid(await GradeSchemesAsync(), response.GradeScheme);
            var sectionRegistration = new Dtos.SectionRegistration3()
            {
                Approvals = new List<Dtos.Approval2>() { new Dtos.Approval2() { ApprovalType = Dtos.ApprovalType2.All, ApprovalEntity = Dtos.ApprovalEntity.System } },
                Id = response.Id,
                Registrant = new Dtos.GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(response.StudentId)),
                Section = new Dtos.GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(response.SectionId)),
                Status = await ConvertResponseStatusToRegistrationStatusAsync(response.StatusCode),
                AwardGradeScheme = new Dtos.GuidObject2(gradeSchemeGuid),
                Transcript = new Dtos.SectionRegistrationTranscript() { GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid), Mode = await ConvertPassAuditToMode(response) },
                SectionRegistrationGrades = await ConvertResponseGradesToSectionRegistrationGradesAsync(response),
                Process = await ConvertResponsetoSectionRegistrationProcessAsync(response),
                Involvement = ConvertResponseSectionRegistrationToInvolvement(response),
                SectionRegistrationReporting = ConvertResponseToSectionRegistrationReporting(response),
                AcademicLevel = await ConvertResponseToAcademicLevelAsync(response),
                RepeatedSection = ConvertResponseToRepeatedSection(response),
                Credit = await ConvertResponseToSectionRegistrationCreditAsync(response),
                QualityPoints = ConvertResponseToQualifiedPoints(response)
            };

            return sectionRegistration;
        }

        /// <summary>
        /// Converts Response in to Grades
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.SectionRegistrationGrade>> ConvertResponseGradesToSectionRegistrationGradesAsync(SectionRegistrationResponse response)
        {
            List<Dtos.SectionRegistrationGrade> sectionRegistrationgrades = new List<Dtos.SectionRegistrationGrade>();
            var sectionGradeTypeDtos = await GetSectionGradeTypesAsync(false);
            var gradeChangeReasonEntities = await GetGradeChangeReasonAsync(true);
        
            //Get final grade
            if (response.FinalTermGrade != null)
            {
                string gradeChangeReasonGuid = null;
                if (response.FinalTermGrade.GradeChangeReason != null)
                {
                    var gradeChangeReason =
                        gradeChangeReasonEntities.FirstOrDefault( x => x.Code == response.FinalTermGrade.GradeChangeReason);
                    gradeChangeReasonGuid = gradeChangeReason == null ? null: gradeChangeReason.Guid;
                }
                Dtos.SectionRegistrationGrade finalSectionRegistrationGrade =
                    await GetSectionRegistrationGradeAsync(response, response.FinalTermGrade.GradeId, "FINAL", 
                                                      response.FinalTermGrade.SubmittedOn, Dtos.SubmissionMethodType.Manual,
                                                      sectionGradeTypeDtos, gradeChangeReasonGuid, response.FinalTermGrade.SubmittedBy);
                sectionRegistrationgrades.Add(finalSectionRegistrationGrade);
            }

            //Get verified grade
            if (response.VerifiedTermGrade != null)
            {
                string gradeChangeReasonGuid = null;
                if (response.VerifiedTermGrade.GradeChangeReason != null)
                {
                    var gradeChangeReason =
                        gradeChangeReasonEntities.FirstOrDefault( x => x.Code == response.VerifiedTermGrade.GradeChangeReason);
                    gradeChangeReasonGuid = gradeChangeReason == null ? null : gradeChangeReason.Guid;
                }
                Dtos.SectionRegistrationGrade verifiedSectionRegistrationGrade =
                    await GetSectionRegistrationGradeAsync(response, response.VerifiedTermGrade.GradeId, "VERIFIED", 
                                                      response.VerifiedTermGrade.SubmittedOn, Dtos.SubmissionMethodType.Auto,
                                                      sectionGradeTypeDtos, gradeChangeReasonGuid, response.VerifiedTermGrade.SubmittedBy);
                sectionRegistrationgrades.Add(verifiedSectionRegistrationGrade);
            }

            //Get mid-term grades
            if (response.MidTermGrades != null && response.MidTermGrades.Count > 0)
            {
                List<Dtos.SectionRegistrationGrade> midTermSectionRegistrationGrades =
                    await GetMidTermSectionRegistrationGradesAsync(response, sectionGradeTypeDtos, gradeChangeReasonEntities);
                sectionRegistrationgrades.AddRange(midTermSectionRegistrationGrades);
            }

            if (!sectionRegistrationgrades.Any())
            {
                return null;
            }

            return sectionRegistrationgrades;
        }

        /// <summary>
        /// Gets midterm grades
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <param name="sectionGradeTypeDtos"></param>
        /// <param name="gradeChangeReasonEntities"></param>
        /// <returns></returns>
        private async Task<List<Dtos.SectionRegistrationGrade>> GetMidTermSectionRegistrationGradesAsync(SectionRegistrationResponse response, 
                                                                                                    IEnumerable<Dtos.SectionGradeType> sectionGradeTypeDtos,
                                                                                                    IEnumerable<GradeChangeReason> gradeChangeReasonEntities)
        {
            List<Dtos.SectionRegistrationGrade> sectionRegistrationGrades = new List<Dtos.SectionRegistrationGrade>();
            string gradeChangeReasonGuid = null;

            if (response.MidTermGrades != null && response.MidTermGrades.Any())
            {
                foreach (var grade in response.MidTermGrades)
                {
                    gradeChangeReasonGuid = null;
                    if (response.VerifiedTermGrade != null && response.VerifiedTermGrade.GradeChangeReason != null)
                    {
                        var gradeChangeReason =
                            gradeChangeReasonEntities.FirstOrDefault(x => x.Code == grade.GradeChangeReason);
                        gradeChangeReasonGuid = gradeChangeReason == null ? null : gradeChangeReason.Guid;
                    }
                    //Build MID Term Grades: Start constructiong midterm SectionRegistrationGrade and its child objects
                    string midTermCode = string.Format("MID{0}", grade.Position);
                    Dtos.SectionRegistrationGrade sectionRegistrationGrade = 
                        await GetSectionRegistrationGradeAsync(response, grade.GradeId, midTermCode, grade.GradeTimestamp, Dtos.SubmissionMethodType.Manual,
                                                          sectionGradeTypeDtos, gradeChangeReasonGuid, grade.SubmittedBy);
                    sectionRegistrationGrades.Add(sectionRegistrationGrade);
                }
            }

            return sectionRegistrationGrades;
        }

        /// <summary>
        /// Common method to construct Midterm, Final, Verified grades
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <param name="gradeId">GradeId</param>
        /// <param name="gradeCode">GradeCode</param>
        /// <param name="submittedOn">SubmittedOn</param>
        /// <param name="submissionMethod">SubmissionMethodType</param>
        /// <param name="sectionGradeTypeDtos">SectionGradeTypes</param>
        /// <param name="gradeChangeReasonGuid">GradeChangeReasonGuid</param>
        /// <returns></returns>
        private async Task<Dtos.SectionRegistrationGrade> GetSectionRegistrationGradeAsync(SectionRegistrationResponse response, string gradeId, string gradeCode, 
                                                                                      DateTimeOffset? submittedOn, Dtos.SubmissionMethodType submissionMethod,
                                                                                      IEnumerable<Dtos.SectionGradeType> sectionGradeTypeDtos, string gradeChangeReasonGuid, 
                                                                                      string submittedBy)
        {
            //Build Grade: Start constructiong SectionRegistrationGrade and its child objects
            Dtos.SectionRegistrationGrade sectionRegistrationGrade = new Dtos.SectionRegistrationGrade();

            //get grade guid
            var gradeGuid = await _sectionRegistrationRepository.GetGradeGuidFromIdAsync(gradeId);
            sectionRegistrationGrade.SectionGrade = new Dtos.GuidObject2(gradeGuid);

            //Get grade grade-type
            var sectionGradeType = sectionGradeTypeDtos.FirstOrDefault(gt => gt.Code == gradeCode);
            if (sectionGradeType != null)
            {
                sectionRegistrationGrade.SectionGradeType = new Dtos.GuidObject2(sectionGradeType.Id);
            }
            Dtos.GuidObject2 submittedByGuidObject = null;
            try
            {
                if (!string.IsNullOrEmpty(submittedBy))
                {
                    //only display the submittedBy if the Id is a person 
                    // check if the input in numbers then it is possibly Id, otherwise it is OPERS 
                    int num;
                    if (!Int32.TryParse(submittedBy, out num))
                    {
                        submittedBy = await _personBaseRepository.GetPersonIdFromOpersAsync(submittedBy);
                    }
                    if (!await _personRepository.IsCorpAsync(submittedBy))
                    submittedByGuidObject = new Dtos.GuidObject2(await _personBaseRepository.GetPersonGuidFromOpersAsync(submittedBy));
                }
                else
                {
                    //submittedByGuidObject = new Dtos.GuidObject2();
                    submittedByGuidObject = null;
                }
            }
            catch (ArgumentNullException)
            {
                logger.Error("No corresponding guid found for submittedBy: " + "'" + submittedBy + "'");
                //submittedByGuidObject = new Dtos.GuidObject2();
            }
            Dtos.GuidObject2 submitReasonGuidObject = null;
            try
            {
                if (!string.IsNullOrEmpty(gradeChangeReasonGuid))
                {
                    submitReasonGuidObject = new Dtos.GuidObject2(gradeChangeReasonGuid);
                }
                else
                {
                    submitReasonGuidObject = null;
                }
            }
            catch (ArgumentNullException)
            {
                logger.Error("No corresponding guid found for reason: " + "'" + gradeChangeReasonGuid + "'");
            }
            Dtos.Submission submission = new Dtos.Submission()
            {
                SubmissionMethod = submissionMethod,
                SubmittedOn = submittedOn,
                SubmissionReason = submitReasonGuidObject,
                //SubmissionReason = new Dtos.GuidObject2(gradeChangeReasonGuid),
                SubmittedBy = submittedByGuidObject
            };
            sectionRegistrationGrade.Submission = submission;
            return sectionRegistrationGrade;
        }

        /// <summary>
        /// Converts response to section registration process & grade extention
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns></returns>
        private async Task<Dtos.SectionRegistrationProcess> ConvertResponsetoSectionRegistrationProcessAsync(SectionRegistrationResponse response)
        {
            Dtos.SectionRegistrationProcess process = new Dtos.SectionRegistrationProcess();
            Dtos.Transcript transcript = new Dtos.Transcript();
            Dtos.GradeExtension gradeExtension = await GetGradeExtensionAsync(response);

            Dtos.GuidObject2 transcriptVerifiedByGuidObject = null;
            try
            {
                if (!string.IsNullOrEmpty(response.TranscriptVerifiedBy))
                {
                    transcriptVerifiedByGuidObject = new Dtos.GuidObject2(await _personBaseRepository.GetPersonGuidFromOpersAsync(response.TranscriptVerifiedBy));
                }
            }
            catch (ArgumentNullException)
            {
                logger.Error("No corresponding guid found for transcriptVerifiedBy: " + "'" + response.TranscriptVerifiedBy + "'");
            }

            transcript.VerifiedOn = response.TranscriptVerifiedGradeDate;
            transcript.VerifiedBy = transcriptVerifiedByGuidObject;

            if (transcript.VerifiedBy == null && transcript.VerifiedOn == null)
            {
                transcript = null;
            }
            process.GradeExtension = gradeExtension;
            process.Transcript = transcript;

            if (process.GradeExtension == null && process.Transcript == null)
            {
                return null;
            }

            return process;
        }

        /// <summary>
        /// Populates Grade Extension
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <param name="gradeExtension"></param>
        /// <returns></returns>
        private async Task<Dtos.GradeExtension> GetGradeExtensionAsync(SectionRegistrationResponse response)
        {
            Dtos.GradeExtension gradeExtension = new Dtos.GradeExtension();
            gradeExtension.ExpiresOn = response.GradeExtentionExpDate;

            var gradeEntities = await GetGradeHedmAsync(true);
            Domain.Student.Entities.Grade defaultGradeEntity = response.FinalTermGrade == null ? null : 
                                                               gradeEntities.FirstOrDefault(g => g.Id == response.FinalTermGrade.GradeId);

            //if the default grade entity is not null & incomplete grade is not null or empty then get the default grade
            if (defaultGradeEntity != null && !string.IsNullOrEmpty(defaultGradeEntity.IncompleteGrade))
            {
                var defaultGrade = gradeEntities.FirstOrDefault(g => g.Id == defaultGradeEntity.IncompleteGrade);
                if (defaultGrade != null)
                {
                    gradeExtension.DefaultGrade = new Dtos.GuidObject2(defaultGrade.Guid);
                }
                else
                {
                    gradeExtension.DefaultGrade = null;
                }
            }
            else
            {
                gradeExtension.DefaultGrade = null;
            }

            if (gradeExtension.ExpiresOn == null && gradeExtension.DefaultGrade == null)
            {
                return null;
            }

            return gradeExtension;
        }

        /// <summary>
        /// Converts response to involvement
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns>Dtos.SectionRegistrationInvolvement</returns>
        private Dtos.SectionRegistrationInvolvement ConvertResponseSectionRegistrationToInvolvement(SectionRegistrationResponse response)
        {
            Dtos.SectionRegistrationInvolvement involvement = new Dtos.SectionRegistrationInvolvement();
            involvement.StartOn = response.InvolvementStartOn;
            involvement.EndOn = response.InvolvementEndOn;
            if (involvement.StartOn == null && involvement.EndOn == null)
            {
                return null;
            }

            return involvement;
        }

        /// <summary>
        /// Properties required for governmental or other reporting.
        /// </summary>
        /// <param name="response">Response from Registration Process with warnings and messages</param>
        /// <returns>Dtos.SectionRegistrationReporting</returns>
        private Dtos.SectionRegistrationReporting ConvertResponseToSectionRegistrationReporting(SectionRegistrationResponse response)
        {
            Dtos.SectionRegistrationReporting sectionRegistrationReporting = new Dtos.SectionRegistrationReporting();

            sectionRegistrationReporting.CountryCode = Dtos.CountryCodeType.USA;
            /*
                We assign null to the ReportingStatus based on empty or null string in db,
                If it is set to Y then we set it as NeverAttended
                If it is set as N then we set it as Attended
            */
            Dtos.ReportingStatusType? repStatusType = null;
            if (response.ReportingStatus != null)
            {
                //If its "Y" then set it to Dtos.ReportingStatusType.NeverAttended
                if (response.ReportingStatus.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    repStatusType = Dtos.ReportingStatusType.NeverAttended;
                }
                //If its "N" then set it to Dtos.ReportingStatusType.Attended
                if (response.ReportingStatus.Equals("N", StringComparison.OrdinalIgnoreCase))
                {
                    repStatusType = Dtos.ReportingStatusType.Attended;
                }
            }

            if (response.ReportingLastDayOdAttendance == null && repStatusType == null)
            {
                sectionRegistrationReporting.LastDayOfAttendance = null;
            }
            else
            {
                sectionRegistrationReporting.LastDayOfAttendance = new Dtos.LastDayOfAttendance() 
                { 
                    LastAttendedOn = response.ReportingLastDayOdAttendance, 
                    Status = repStatusType 
                };
            }
                

            return sectionRegistrationReporting;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all section grade types
        /// </summary>
        /// <returns>Collection of SectionGradeType DTO objects</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionGradeType>> GetSectionGradeTypesAsync(bool bypassCache = false)
        {
            var sectionGradeTypeCollection = new List<Ellucian.Colleague.Dtos.SectionGradeType>();

            var sectionGradeTypeEntities = await _studentReferenceDataRepository.GetSectionGradeTypesAsync(bypassCache);
            if (sectionGradeTypeEntities != null && sectionGradeTypeEntities.Count() > 0)
            {
                foreach (var sectionGradeType in sectionGradeTypeEntities)
                {
                    sectionGradeTypeCollection.Add(ConvertSectionGradeTypeEntityToDto(sectionGradeType));
                }
            }
            return sectionGradeTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a SectionGradeType domain entity to its corresponding SectionGradeType DTO
        /// </summary>
        /// <param name="source">SectionGradeType domain entity</param>
        /// <returns>SectionGradeType DTO</returns>
        private Ellucian.Colleague.Dtos.SectionGradeType ConvertSectionGradeTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.SectionGradeType source)
        {
            var sectionGradeType = new Ellucian.Colleague.Dtos.SectionGradeType();
            sectionGradeType.Id = source.Guid;
            sectionGradeType.Code = source.Code;
            sectionGradeType.Title = source.Description;
            sectionGradeType.Description = null;

            return sectionGradeType;
        }

        private async Task<Dtos.TranscriptMode> ConvertPassAuditToMode(SectionRegistrationResponse response)
        {
            Dtos.TranscriptMode transcriptMode = Dtos.TranscriptMode.Standard;
            if (response.PassAudit.ToUpperInvariant().Equals("P") || response.PassAudit.ToUpperInvariant().Equals("A"))
            {
                switch (response.PassAudit.ToUpperInvariant())
                {
                    case ("P"):
                        transcriptMode = Dtos.TranscriptMode.PassFail;
                        return transcriptMode;
                    case ("A"):
                        transcriptMode = Dtos.TranscriptMode.Audit;
                        return transcriptMode;
                }
            }

            if (response.StatusCode.ToUpperInvariant().Equals("W"))
            {
                transcriptMode = Dtos.TranscriptMode.Withdraw;
                return transcriptMode;
            }

            if (response.FinalTermGrade != null)
            {
                if (gradeEntities == null)
                    gradeEntities = await GetGradeHedmAsync(true);

                if (gradeEntities != null)
                {
                    Domain.Student.Entities.Grade defaultGradeEntity = gradeEntities.FirstOrDefault(g => g.Id == response.FinalTermGrade.GradeId);
                    if (defaultGradeEntity != null)
                    {
                        if (!string.IsNullOrEmpty(defaultGradeEntity.IncompleteGrade))
                        {
                            transcriptMode = Dtos.TranscriptMode.Incomplete;
                        }
                    }
                    var status = (await GetSectionRegistrationStatusesAsync(false)).Where(r => r.Code == response.StatusCode).FirstOrDefault();
                    if (status != null && status.Status != null)
                    {
                        if (status.Status.SectionRegistrationStatusReason == RegistrationStatusReason.Withdrawn)
                            transcriptMode = Dtos.TranscriptMode.Withdraw;
                    }
                }
            }

            return transcriptMode;
        }

        /// <summary>
        /// Convert the response status information into the DTO status information
        /// </summary>
        /// <param name="statusCode">Status Code from the response</param>
        /// <returns>SectionRegistrationStatus <see cref="Dtos.SectionRegistrationStatus2"/> DTO object</returns>
        private async Task<Dtos.SectionRegistrationStatus2> ConvertResponseStatusToRegistrationStatusAsync(string statusCode)
        {
            var detail = new Dtos.GuidObject2();
            var registrationStatus = new Dtos.RegistrationStatus2();
            var statusReason = new Dtos.RegistrationStatusReason2();

            var status = (await GetSectionRegistrationStatusesAsync(false)).Where(r => r.Code == statusCode).FirstOrDefault();
            if (status != null)
            {
                detail = new Dtos.GuidObject2() { Id = status.Guid };
                var regStatus = status.Status.RegistrationStatus;
                var regStatusReason = status.Status.SectionRegistrationStatusReason;

                switch (regStatus)
                {
                    case RegistrationStatus.Registered:
                        {
                            registrationStatus = Dtos.RegistrationStatus2.Registered;
                            break;
                        }
                    case RegistrationStatus.NotRegistered:
                        {
                            registrationStatus = Dtos.RegistrationStatus2.NotRegistered;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                switch (regStatusReason)
                {
                    case RegistrationStatusReason.Canceled:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Canceled;
                            break;
                        }
                    case RegistrationStatusReason.Dropped:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Dropped;
                            break;
                        }
                    case RegistrationStatusReason.Pending:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Pending;
                            break;
                        }
                    case RegistrationStatusReason.Registered:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Registered;
                            break;
                        }
                    case RegistrationStatusReason.Withdrawn:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Withdrawn;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return new Dtos.SectionRegistrationStatus2()
            {
                Detail = detail,
                RegistrationStatus = registrationStatus,
                SectionRegistrationStatusReason = statusReason
            };
        }
        
        /// <summary>
        /// Converts response acad level
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<Dtos.GuidObject2> ConvertResponseToAcademicLevelAsync(SectionRegistrationResponse response)
        {
            Dtos.GuidObject2 acadLevelGuidObj = null;
            var acadLevels = await AcademicLevelsAsync();
            if (!string.IsNullOrEmpty(response.AcademicLevel) && acadLevels != null)
            {
                var acadLevel = acadLevels.FirstOrDefault(al => al.Code.Equals(response.AcademicLevel, StringComparison.OrdinalIgnoreCase));
                acadLevelGuidObj = acadLevel != null ? new Dtos.GuidObject2(acadLevel.Guid) : null;
            }
            return acadLevelGuidObj;
        }

        /// <summary>
        /// Converts response to repeated section
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private RepeatedSection? ConvertResponseToRepeatedSection(SectionRegistrationResponse response)
        {
            RepeatedSection? repeatedSection = null;

            if (response.VerifiedTermGrade != null && !string.IsNullOrEmpty(response.VerifiedTermGrade.GradeId))
            {
                //STC.VERIFIED.GRADE is populated and STC.REPL.CODE is null and STC.REPEATED.ACAD.CRED is null
                if (string.IsNullOrEmpty(response.ReplCode) && (response.RepeatedAcadCreds == null || !response.RepeatedAcadCreds.Any()))
                {
                    repeatedSection = RepeatedSection.NotRepeated;
                }
                //STC.VERIFIED.GRADE is populated and STC.REPL.CODE contains a value and STC.ALTCUM.CONTRIB.CMPL.CRED is 0 and STC.ALTCUM.CONTRIB.GPA.CRED is 0
                if (!string.IsNullOrEmpty(response.ReplCode) && response.AltcumContribCmplCred == 0 && response.AltcumContribGpaCred == 0)
                {
                    repeatedSection = RepeatedSection.RepeatedIncludeNeither;
                }
                //STC.VERIFIED.GRADE is populated and STC.REPL.CODE contains a value and STC.ALTCUM.CONTRIB.CMPL.CRED is >0 and STC.ALTCUM.CONTRIB.GPA.CRED is > 0
                if (!string.IsNullOrEmpty(response.ReplCode) && response.AltcumContribCmplCred > 0 && response.AltcumContribGpaCred > 0)
                {
                    repeatedSection = RepeatedSection.RepeatedIncludeBoth;
                }
                //STC.VERIFIED.GRADE is populated and STC.REPL.CODE contains a value and STC.ALTCUM.CONTRIB.CMPL.CRED is > 0 and STC.ALTCUM.CONTRIB.GPA.CRED is 0
                if (!string.IsNullOrEmpty(response.ReplCode) && response.AltcumContribCmplCred > 0 && response.AltcumContribGpaCred == 0)
                {
                    repeatedSection = RepeatedSection.RepeatedIncludeCredit;
                }
                //STC.VERIFIED.GRADE is populated and STC.REPL.CODE contains a value and STC.ALTCUM.CONTRIB.CMPL.CRED is 0 and STC.ALTCUM.CONTRIB.GRADE.PTS is > 0
                if (!string.IsNullOrEmpty(response.ReplCode) && response.AltcumContribCmplCred == 0 && response.AltcumContribGpaCred > 0)
                {
                    repeatedSection = RepeatedSection.RepeatedIncludeQualityPoints;
                }
            }
            return repeatedSection;
        }

        /// <summary>
        /// Converts response to credit
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.Credit3DtoProperty> ConvertResponseToSectionRegistrationCreditAsync(SectionRegistrationResponse response)
        {
            var credit = new Credit3DtoProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (!string.IsNullOrEmpty(response.CreditType) && creditTypeItems.Any(ct => ct.Code.Equals(response.CreditType, StringComparison.OrdinalIgnoreCase)))
            {
                var creditTypeItem = creditTypeItems.FirstOrDefault(ct => ct.Code == response.CreditType);                

                credit.CreditCategory = new CreditIdAndTypeProperty2();
                credit.CreditCategory.Detail = new Ellucian.Colleague.Dtos.GuidObject2(creditTypeItem.Guid);
                switch (creditTypeItem.CreditType)
                {

                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                        break;
                    case CreditType.Exchange:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                        break;
                    case CreditType.Other:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                        break;
                    case CreditType.None:
                        credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;
                }
            }

            if (response.Credit != null)
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.AttemptedCredit = response.Credit;
                if (response.VerifiedTermGrade != null && !string.IsNullOrEmpty(response.VerifiedTermGrade.GradeId))
                {
                    credit.EarnedCredit = response.EarnedCredit;
                }
            }
            else if (response.Ceus != null)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.AttemptedCredit = response.Ceus;
                if (response.VerifiedTermGrade != null && !string.IsNullOrEmpty(response.VerifiedTermGrade.GradeId))
                {
                    credit.EarnedCredit = response.EarnedCeus;
                }
            }

            return credit;
        }

        /// <summary>
        /// Converts response to qualified points
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private decimal? ConvertResponseToQualifiedPoints(SectionRegistrationResponse response)
        {
            decimal? qualifiedPoint = null;
            if (response.VerifiedTermGrade != null && !string.IsNullOrEmpty(response.VerifiedTermGrade.GradeId) && response.Credit != null)
            {
                qualifiedPoint = response.GradePoint;
            }
            return qualifiedPoint;
        }

        /// <summary>
        /// Academic Levels
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> AcademicLevelsAsync()
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync();
            }
            return _academicLevels;
        }

        /// <summary>
        /// Credit Categories
        /// </summary>
        private IEnumerable<Domain.Student.Entities.CreditCategory> _creditTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.CreditCategory>> CreditTypesAsync()
        {
            if (_creditTypes == null)
            {
                _creditTypes = await _studentReferenceDataRepository.GetCreditCategoriesAsync();
            }
            return _creditTypes;
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Verifies if the user has the correct permissions to update a registration.
        /// </summary>
        private void CheckUserRegistrationUpdatePermissions(string personId)
        {
            // access is ok if the current user is the person being updated
            if (!CurrentUser.IsPerson(personId))
            {
                // access is ok if the current user has the update registrations permission
                if (!HasPermission(SectionPermissionCodes.UpdateRegistrations))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update section-registrations.");
                    throw new PermissionsException("User is not authorized to update section-registrations.");
                }
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a registration.
        /// </summary>
        private void CheckUserRegistrationCreatePermissions(string personId)
        {
            // access is ok if the current user is the person being created
            if (!CurrentUser.IsPerson(personId))
            {
                // access is ok if the current user has the create registrations permission
                if (!HasPermission(SectionPermissionCodes.CreateRegistrations))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create section-registrations.");
                    throw new PermissionsException("User is not authorized to create section-registrations.");
                }
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person.
        /// </summary>
        private void CheckUserRegistrationViewPermissions()
        {
            // access is ok if the current user has the view registrations permission
            if (!HasPermission(SectionPermissionCodes.ViewRegistrations))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view section-registrations.");
                throw new PermissionsException("User is not authorized to view section-registrations.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to delete a registration.
        /// </summary>
        private void CheckUserRegistrationDeletePermissions()
        {
            // access is ok if the current user has the delete registrations permission
            if (!HasPermission(SectionPermissionCodes.DeleteRegistrations))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to delete section-registrations.");
                throw new PermissionsException("User is not authorized to delete section-registrations.");
            }
        }

        #endregion
    }
}