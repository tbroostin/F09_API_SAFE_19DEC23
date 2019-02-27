// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
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
    /// <summary>
    /// SectionRegistrationService is an application that responds to a request for Section registration Management
    /// </summary>
    [RegisterType]
    public class SectionRegistrationService : StudentCoordinationService, ISectionRegistrationService
    {
        private readonly ISectionRegistrationRepository _sectionRegistrationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly ITermRepository _termRepository;

        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
        private readonly IConfigurationRepository _configurationRepository;

        Dictionary<string, string> personIdDict = null;
        Dictionary<string, string> sectionIdDict = null;
        Dictionary<string, KeyValuePair<string, string>> operIdsWithGuids = null;

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
            ITermRepository termRepository,
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
            _termRepository = termRepository;
        }

        /// <summary>
        /// Academic Levels
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> AcademicLevelsAsync(bool bypassCache = false)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
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

        /// <summary>
        /// Get all terms
        /// </summary>
        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private IEnumerable<Domain.Student.Entities.Term> Terms(bool bypassCache = false)
        {
            if (_terms == null)
            {
                _terms = _termRepository.Get();
            }
            return _terms;
        }

        /// <summary>
        /// Get all academic periods
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> AcademicPeriods(bool bypassCache = false)
        {
            if (_academicPeriods == null)
            {
                _academicPeriods = _termRepository.GetAcademicPeriods(Terms(bypassCache));
            }
            return _academicPeriods;
        }

        private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GradeSchemesAsync(bool bypassCache = false)
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache);
            }
            return _gradeSchemes;
        }

        private IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem> _secRegStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem>> GetSectionRegistrationStatusesAsync(bool ignoreCache = false)
        {
            if (_secRegStatuses == null)
            {
                _secRegStatuses = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(ignoreCache);
            }
            return _secRegStatuses;
        }

        private IEnumerable<Domain.Student.Entities.GradingTerm> _gradingTerms = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradingTerm>> GetGradingTermAsync(bool ignoreCache = false)
        {
            if (_gradingTerms == null)
            {
                _gradingTerms = await _studentReferenceDataRepository.GetGradingTermsAsync(ignoreCache);
            }
            return _gradingTerms;
        }

        private IEnumerable<Domain.Student.Entities.Grade> _grades = null;
        private async Task<IEnumerable<Domain.Student.Entities.Grade>> GetGradeHedmAsync(bool ignoreCache = false)
        {
            if (_grades == null)
            {
                _grades = await _gradeRepository.GetHedmAsync(ignoreCache);
            }
            return _grades;
        }

        private IEnumerable<Domain.Base.Entities.GradeChangeReason> _gradeChangeReasons = null;
        private async Task<IEnumerable<Domain.Base.Entities.GradeChangeReason>> GetGradeChangeReasonAsync(bool ignoreCache = false)
        {
            if (_gradeChangeReasons == null)
            {
                _gradeChangeReasons = await _referenceDataRepository.GetGradeChangeReasonAsync(ignoreCache);
            }
            return _gradeChangeReasons;
        }

        private IEnumerable<Domain.Base.Entities.Location> _locationEntities = null;
        /// <summary>
        /// Gets Location entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.Location collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache = false)
        {
            if (_locationEntities == null)
            {
                _locationEntities = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _locationEntities;
        }

        #region Get Methods

        #region V16.0.0
        public async Task<Tuple<IEnumerable<Dtos.SectionRegistration4>, int>> GetSectionRegistrations3Async(int offset, int limit, SectionRegistration4 criteria,
            string academicPeriod, string sectionInstructor, bool bypassCache = false)
        {
            // get user permissions
            CheckUserRegistrationViewPermissions();
            SectionRegistrationResponse sectReg = new SectionRegistrationResponse(new List<RegistrationMessage>());
            string acadPeriodNewValue = string.Empty, sectInstructorNewValue = string.Empty;
            var sectRegistrationList = new List<Dtos.SectionRegistration4>();
            int totalCount = 0;

            if (criteria != null && criteria.Section != null && !string.IsNullOrEmpty(criteria.Section.Id))
            {
                try
                {
                    sectReg.SectionId = await _sectionRepository.GetSectionIdFromGuidAsync(criteria.Section.Id);
                    if (string.IsNullOrEmpty(sectReg.SectionId))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                }
            }
            if (criteria != null && criteria.Registrant != null && !string.IsNullOrEmpty(criteria.Registrant.Id))
            {
                try
                {
                    sectReg.StudentId = await _personRepository.GetPersonIdFromGuidAsync(criteria.Registrant.Id);
                    if (string.IsNullOrEmpty(sectReg.StudentId))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                try
                {
                    var acadPeriod = (AcademicPeriods(bypassCache)).FirstOrDefault(ap => ap.Guid.Equals(academicPeriod, StringComparison.OrdinalIgnoreCase));
                    if (acadPeriod == null || string.IsNullOrEmpty(acadPeriod.Code))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                    }
                    acadPeriodNewValue = acadPeriod.Code;
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                }
            }
            if (!string.IsNullOrEmpty(sectionInstructor))
            {
                try
                {
                    sectInstructorNewValue = await _personRepository.GetPersonIdFromGuidAsync(sectionInstructor);
                    if (string.IsNullOrEmpty(sectInstructorNewValue))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                    }

                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
                }
            }

            var responses = await _sectionRegistrationRepository.GetSectionRegistrations2Async(offset, limit, sectReg, acadPeriodNewValue, sectInstructorNewValue);
            //No records
            if (!responses.Item1.Any())
            {
                return new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(sectRegistrationList, 0);
            }

            //Get all person ids
            var personIds = responses.Item1.Where(p => !string.IsNullOrEmpty(p.StudentId)).Select(s => s.StudentId);
            personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(personIds.Distinct().ToArray());

            //Get all section ids
            var sectionIds = responses.Item1.Where(p => !string.IsNullOrEmpty(p.SectionId)).Select(s => s.SectionId);
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds.Distinct().ToArray());

            //Get section reg statuses to whose special processing code is 1 or 2
            var sectRegStatuses = (await GetSectionRegistrationStatusesAsync(bypassCache))
                    .Where(i => i.Status.RegistrationStatus == Domain.Student.Entities.RegistrationStatus.Registered)
                    .Select(s => s.Code).ToList();

            totalCount = responses.Item2;

            foreach (var response in responses.Item1)
            {
                // var recordKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Guid);
                var recordKey = response.StudentAcadCredKey;
                if (string.IsNullOrEmpty(recordKey))
                {
                    // throw new KeyNotFoundException(string.Format("Invalid GUID for section registration: {0}.", guid));
                    IntegrationApiExceptionAddError(string.Format("Invalid GUID for section registration: {0}.", response.Guid), "sectionRegistrations.id");
                    throw IntegrationApiException;
                }
                // convert response to SectionRegistration
                sectRegistrationList.Add(await ConvertResponsetoDto4(response, sectRegStatuses, recordKey));
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return sectRegistrationList.Any() ? new Tuple<IEnumerable<SectionRegistration4>, int>(sectRegistrationList, totalCount) :
                new Tuple<IEnumerable<SectionRegistration4>, int>(new List<Dtos.SectionRegistration4>(), 0);
        }

        /// <summary>
        /// Get Section Registration by guid V16.0.0
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Dtos.SectionRegistration4> GetSectionRegistrationByGuid3Async(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                // throw new ArgumentNullException("guid", "Must provide a SectionRegistration id for retrieval.");
                IntegrationApiExceptionAddError("Must provide a SectionRegistration id for retrieval.", "guid");
                throw IntegrationApiException;
            }

            // get user permissions
            CheckUserRegistrationViewPermissions();

            var recordKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(recordKey))
            {
                // throw new KeyNotFoundException(string.Format("Invalid GUID for section registration: {0}.", guid));
                IntegrationApiExceptionAddError(string.Format("Invalid GUID for section registration: {0}.", guid), "guid");
                throw IntegrationApiException;
            }

            // process the request
            var response = await _sectionRegistrationRepository.GetSectionRegistrationByIdAsync(recordKey);

            //Get all person ids
            var personIds = new[] { response.StudentId };
            personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(personIds.ToArray());

            //Get all section ids
            var sectionIds = new[] { response.SectionId };
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds.ToArray());

            //Get section reg statuses to whose special processing code is 1 or 2
            var sectRegStatuses = (await GetSectionRegistrationStatusesAsync(bypassCache))
                    .Where(i => i.Status.RegistrationStatus == Domain.Student.Entities.RegistrationStatus.Registered)
                    .Select(s => s.Code).ToList();

            // convert response to SectionRegistration
            var dtoResponse = await ConvertResponsetoDto4(response, sectRegStatuses, recordKey);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            // convert response to SectionRegistration
            return dtoResponse;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sectRegStatuses"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Dtos.SectionRegistration4> ConvertResponsetoDto4(SectionRegistrationResponse source, IEnumerable<string> sectRegStatuses, string recordKey, bool bypassCache = false)
        {
            var dto = new Dtos.SectionRegistration4() { Id = source.Guid };
            // Make sure whe have a valid GUID for the record we are dealing with
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for section-registrations entity.", "sectionRegistrations.id", id: source.StudentAcadCredKey);
            }

            try
            {
                string studentId = null;
                if (personIdDict != null && personIdDict.Any() && personIdDict.TryGetValue(source.StudentId, out studentId))
                {
                    dto.Registrant = new Dtos.GuidObject2(studentId);
                }

                if (string.IsNullOrEmpty(studentId))
                {
                    // throw new InvalidOperationException(string.Format("No registrant found for guid {0}.", source.Id));
                    IntegrationApiExceptionAddError(string.Format("No guid found for registrant id '{0}'.", source.StudentId),
                        "sectionRegistrations.registrant.id", source.Guid, recordKey);
                }
            }
            catch (ArgumentNullException)
            {
                // throw new ArgumentNullException("registrant.id", "Registrant id is required.");
                IntegrationApiExceptionAddError("Registrant id is required.",
                    "sectionRegistrations.registrant.id", source.Guid, recordKey);
            }

            try
            {
                string sectionId = null;
                if (sectionIdDict != null && sectionIdDict.Any() && sectionIdDict.TryGetValue(source.SectionId, out sectionId))
                {
                    dto.Section = new Dtos.GuidObject2(sectionId);
                }

                if (string.IsNullOrEmpty(sectionId))
                {
                    // throw new InvalidOperationException(string.Format("No section found for guid {0}.", source.Id));
                    IntegrationApiExceptionAddError(string.Format("No guid found for section id '{0}'.", source.SectionId),
                        "sectionRegistrations.section.id", source.Guid, recordKey);
                }
            }
            catch (ArgumentNullException)
            {
                // throw new ArgumentNullException("section.id", "Section id is required.");
                IntegrationApiExceptionAddError("Section id is required.",
                    "sectionRegistrations.section.id", source.Guid, recordKey);
            }

            //AcademicLevel
            dto.AcademicLevel = await ConvertResponseToAcademicLevelAsync(source);

            //OriginallyRegisteredOn
            if (source.StatusDateTuple != null && source.StatusDateTuple.Any() && sectRegStatuses != null && sectRegStatuses.Any())
            {
                var statuses = source.StatusDateTuple.OrderBy(i => i.Item2).FirstOrDefault(s => sectRegStatuses.Contains(s.Item1));
                if (statuses != null)
                {
                    dto.OriginallyRegisteredOn = statuses.Item2.Value;
                }
            }

            //Status
            //add try catch around this to display record Id for the bad data
            try
            {
                dto.Status = await ConvertResponseStatusToRegistrationStatus2Async(source.StatusCode, source.Guid, recordKey);
            }
            catch (ArgumentException ex)
            {
                // throw new ArgumentException(string.Concat(ex.Message, "Entity:'STUDENT.ACAD.CRED', Record ID:'", source.Id, "'"));
                IntegrationApiExceptionAddError(string.Concat(ex.Message, "Entity:'STUDENT.ACAD.CRED', Record ID:'", source.Guid, "'"),
                    "sectionRegistrations.registrationStatus", source.Guid, recordKey);
            }


            //Status Date
            dto.StatusDate = source.StatusDate.HasValue ? source.StatusDate : default(DateTime?);

            //approvals
            dto.Approval = new List<Dtos.Approval2>()
            {
                new Dtos.Approval2() { ApprovalType = Dtos.ApprovalType2.All, ApprovalEntity = Dtos.ApprovalEntity.System }
            };

            //Credit 
            if (source.Ceus.HasValue || source.Credit.HasValue)
                dto.Credit = ConvertResponseToSectionRegistrationCredit(source);

            //gradingOption
            var gradeSchemeGuid = ConvertCodeToGuid(await GradeSchemesAsync(), source.GradeScheme);
            if (string.IsNullOrEmpty(gradeSchemeGuid))
            {
                IntegrationApiExceptionAddError(string.Format("Invalid Grade Scheme or missing GUID for grade scheme code '{0}'.",source.GradeScheme),
                    "sectionRegistrations.gradeOption.gradeScheme.id", source.Guid, source.StudentAcadCredKey);
            }
            else
            {
                dto.GradingOption = new SectionRegistrationTranscript2()
                {
                    GradeScheme = new GuidObject2(gradeSchemeGuid),
                    Mode = ConvertToGradingOptionMode(source, bypassCache)
                };
            }

            //Involvement 
            dto.Involvement = ConvertResponseSectionRegistrationToInvolvement(source);

            //Override
            string overrideAcadPeriodGuid = null;
            string overrideSiteGuid = null;
            if (!string.IsNullOrEmpty(source.OverrideAcadPeriod))
            {
                var acadPeriod = (AcademicPeriods(bypassCache)).FirstOrDefault(ap => ap.Code.Equals(source.OverrideAcadPeriod, StringComparison.OrdinalIgnoreCase));
                if (acadPeriod == null)
                {
                    // throw new KeyNotFoundException(string.Format("Academic period not found. ID: {0}", source.OverrideAcadPeriod));
                    IntegrationApiExceptionAddError(string.Format("Academic period not found. ID: {0}", source.OverrideAcadPeriod),
                        "sectionRegistrations.override.academicPeriod.id", source.Guid, recordKey);
                }
                else
                {
                    overrideAcadPeriodGuid = acadPeriod.Guid;
                }

            }

            if (!string.IsNullOrEmpty(source.OverrideSite))
            {
                var overrideSite = (await GetLocationsAsync(bypassCache)).FirstOrDefault(l => l.Code.Equals(source.OverrideSite, StringComparison.OrdinalIgnoreCase));
                if (overrideSite == null)
                {
                    // throw new KeyNotFoundException(string.Format("Site not found. ID: {0}", source.OverrideSite));
                    IntegrationApiExceptionAddError(string.Format("Site not found. ID: {0}", source.OverrideSite),
                        "sectionRegistrations.override.site.id", source.Guid, recordKey);
                }
                else
                {
                    overrideSiteGuid = overrideSite.Guid;
                }
            }
            if (!string.IsNullOrEmpty(overrideAcadPeriodGuid) || !string.IsNullOrEmpty(overrideSiteGuid))
            {
                dto.Override = new SectionRegistrationsOverrideDtoProperty();
                dto.Override.AcademicPeriod = !string.IsNullOrEmpty(overrideAcadPeriodGuid) ? new GuidObject2(overrideAcadPeriodGuid) : null;
                dto.Override.Site = !string.IsNullOrEmpty(overrideSiteGuid) ? new GuidObject2(overrideSiteGuid) : null;
            }
            return dto;
        }

        /// <summary>
        /// Converts entity status to dto status.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private async Task<SectionRegistrationStatusDtoProperty> ConvertResponseStatusToRegistrationStatus2Async(string statusCode, string guid, string recordKey)
        {
            Dtos.GuidObject2 detail = null;
            var registrationStatus = new RegistrationStatus3();
            var statusReason = new RegistrationStatusReason3();

            var status = (await GetSectionRegistrationStatusesAsync(false)).Where(r => r.Code == statusCode).FirstOrDefault();
            if (status == null)
            {
                // throw new ArgumentException(string.Concat("The section registration status of '", statusCode , "' is invalid. "));
                IntegrationApiExceptionAddError(string.Concat("The section registration status of '", statusCode, "' is invalid. "),
                    "sectionRegistrations.status", guid, recordKey);
            }
            else
            {
                detail = new Dtos.GuidObject2() { Id = status.Guid };
                var regStatus = status.Status.RegistrationStatus;
                var regStatusReason = status.Status.SectionRegistrationStatusReason;

                switch (regStatus)
                {
                    case Domain.Student.Entities.RegistrationStatus.Registered:
                        {
                            registrationStatus = RegistrationStatus3.Registered;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatus.NotRegistered:
                        {
                            registrationStatus = RegistrationStatus3.NotRegistered;
                            break;
                        }
                    default:
                        {
                            registrationStatus = RegistrationStatus3.NotRegistered;
                            break;
                        }
                }

                switch (regStatusReason)
                {
                    case Domain.Student.Entities.RegistrationStatusReason.Canceled:
                        {
                            statusReason = RegistrationStatusReason3.Canceled;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Dropped:
                        {
                            statusReason = RegistrationStatusReason3.Dropped;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Registered:
                        {
                            statusReason = RegistrationStatusReason3.Registered;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Withdrawn:
                        {
                            statusReason = RegistrationStatusReason3.Withdrawn;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return new SectionRegistrationStatusDtoProperty()
            {
                Detail = detail,
                RegistrationStatus = registrationStatus,
                SectionRegistrationStatusReason = statusReason
            };
        }

        /// <summary>
        /// Converts response to credit
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private Dtos.DtoProperties.Credit4DtoProperty ConvertResponseToSectionRegistrationCredit(SectionRegistrationResponse source)
        {
            var credit = new Credit4DtoProperty();
            if (source.Ceus.HasValue)
            {
                credit.Measure = StudentCourseTransferMeasure.Ceu;
                credit.RegistrationCredit = source.Ceus.Value;
            }
            else if (source.Credit.HasValue)
            {
                credit.Measure = StudentCourseTransferMeasure.Credit;
                credit.RegistrationCredit = source.Credit.Value;
            }

            return credit;
        }

        private TranscriptMode2 ConvertToGradingOptionMode(SectionRegistrationResponse response, bool bypassCache)
        {
            TranscriptMode2 transcriptMode = TranscriptMode2.Standard;
            if (response.PassAudit.ToUpperInvariant().Equals("P") || response.PassAudit.ToUpperInvariant().Equals("A"))
            {
                switch (response.PassAudit.ToUpperInvariant())
                {
                    case ("P"):
                        transcriptMode = TranscriptMode2.PassFail;
                        return transcriptMode;
                    case ("A"):
                        transcriptMode = TranscriptMode2.Audit;
                        return transcriptMode;
                }
            }

            return transcriptMode;
        }

        #endregion V16.0.0

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

            List<string> personIds = new List<string>(), operIds = new List<string>();
            BuildPersonOperIds(response, personIds, operIds);

            //Gets all the opers ids & then get guids associated with it.
            operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
            //Get all person ids
            personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

            //Get all section ids
            var sectionIds = string.IsNullOrEmpty(response.SectionId) ? null : new[] { response.SectionId };
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds);


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

            List<string> personIds = new List<string>(), operIds = new List<string>();
            BuildPersonOperIds(response, personIds, operIds);

            //Gets all the opers ids & then get guids associated with it.
            operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
            //Get all person ids
            personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

            //Get all section ids
            var sectionIds = string.IsNullOrEmpty(response.SectionId) ? null : new[] { response.SectionId };
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds);


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

            string personId = string.Empty, sectionId = string.Empty;
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

            List<string> personIds = new List<string>(), operIds = new List<string>();
            responses.Item1.ToList().ForEach(r =>
            {
                BuildPersonOperIds(r, personIds, operIds);
            });

            //Gets all the opers ids & then get guids associated with it.
            operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
            //Get all person ids
            personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

            //Get all section ids
            var sectionIds = responses.Item1.Where(p => !string.IsNullOrEmpty(p.SectionId)).Select(s => s.SectionId).ToList();
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds.Distinct().ToArray());

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

            List<string> personIds = new List<string>(), operIds = new List<string>();
            responses.Item1.ToList().ForEach(r =>
            {
                BuildPersonOperIds(r, personIds, operIds);
            });

            //Gets all the opers ids & then get guids associated with it.
            operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
            //Get all person ids
            personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

            //Get all section ids
            var sectionIds = responses.Item1.Where(p => !string.IsNullOrEmpty(p.SectionId)).Select(s => s.SectionId);
            sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds.Distinct().ToArray());


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

        /// <summary>
        /// Creates a registration record
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        public async Task<Dtos.SectionRegistration4> CreateSectionRegistration3Async(Dtos.SectionRegistration4 registrationDto)
        {
            var guid = string.Empty;
            return await UpdateSectionRegistration3Async(guid, registrationDto);
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
            await CheckForBusinessRulesAsync(guid, registrationDto);

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
            request.RegGuid = response.Guid;
            var stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Guid);

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

                List<string> personIds = new List<string>(), operIds = new List<string>();
                BuildPersonOperIds(response, personIds, operIds);

                //Gets all the opers ids & then get guids associated with it.
                operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
                //Get all person ids
                personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

                //Get all section ids
                var sectionIds = string.IsNullOrEmpty(response.SectionId) ? null : new[] { response.SectionId };
                sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds);


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
            string stcKey = string.Empty;
            if (!string.IsNullOrEmpty(guid))
            {
                guid = guid.ToLowerInvariant();
                try
                {
                    stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(guid);
                }
                catch
                {
                    // Fall through with a null stcKey in case we are doing PUT with a new GUID.
                }
            }
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
            request.StudentAcadCredId = stcKey;

            SectionRegistrationResponse response = null;
            try
            {
                // process the request
                response = await _sectionRegistrationRepository.Update2Async(request, guid, personId, sectionId, statusCode);
            }
            catch (RepositoryException ex)
            {
                // Convert v2 error messages into v1 error messages
                var repoError = new RepositoryException("Errors Occurred in section-registrations update.");
                foreach(var error in ex.Errors)
                {
                    repoError.AddError(new RepositoryError(error.Code, error.Message));
                }
                throw repoError;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //Assign the new guid
            request.RegGuid = response.Guid;
            stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Guid);

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

                List<string> personIds = new List<string>(), operIds = new List<string>();
                BuildPersonOperIds(response, personIds, operIds);

                //Gets all the opers ids & then get guids associated with it.
                operIdsWithGuids = await _personRepository.GetPersonGuidsFromOperKeysAsync(operIds.Distinct());
                //Get all person ids
                personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(personIds.Distinct());

                //Get all section ids
                var sectionIds = string.IsNullOrEmpty(response.SectionId) ? null : new[] { response.SectionId };
                sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds);


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
        /// Update a section registration
        /// </summary>
        /// <param name="guid">Guid for section registration</param>
        /// <param name="registrationDto">The <see cref="Dtos.SectionRegistration2">section registration</see> entity to update in the database.</param>
        /// <returns>The modified section registration <see cref="Dtos.SectionRegistration2">object</see></returns>
        public async Task<Dtos.SectionRegistration4> UpdateSectionRegistration3Async(string guid, Dtos.SectionRegistration4 registrationDto)
        {
            string stcKey = string.Empty;
            if (!string.IsNullOrEmpty(guid))
            {
                guid = guid.ToLowerInvariant();
                try
                {
                    stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(guid);
                }
                catch
                {
                    // Fall through with a null stcKey in case we are doing PUT with a new GUID.
                }
            }
            //check if required fields are filled
            CheckForRequiredFields3(guid, stcKey, registrationDto);

            //Check business logic
            await CheckForBusinessRules3Async(guid, stcKey, registrationDto);

            // get the person ID associated with the incoming guid
            string personId = string.Empty;
            try
            {
                if (registrationDto.Registrant != null && !string.IsNullOrEmpty(registrationDto.Registrant.Id))
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(registrationDto.Registrant.Id);
                }
            }
            catch
            {
                // Fall through to null or empty check
            }

            if (registrationDto.Registrant != null && !string.IsNullOrEmpty(registrationDto.Registrant.Id) && string.IsNullOrEmpty(personId))
                //throw new KeyNotFoundException("Person ID associated to id '" + registrationDto.Registrant.Id + "' not found");
                IntegrationApiExceptionAddError("Person associated to id '" + registrationDto.Registrant.Id + "' not found",
                    "sectionRegistrations.registrant.id", registrationDto.Id, "", System.Net.HttpStatusCode.NotFound);

            CheckUserRegistrationUpdatePermissions(personId);
          
            _sectionRegistrationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            string statusCode = string.Empty;
            if (registrationDto.Status != null && registrationDto.Status.Detail != null && !string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
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
                    // throw new ArgumentNullException(string.Concat("Registration Status with id ", registrationDto.Status.Detail.Id.ToString(), " was not found in the valid Section Registration Status List."));
                    IntegrationApiExceptionAddError(string.Concat("Registration Status with id '",
                        registrationDto.Status.Detail.Id.ToString(),
                        "' was not found in the valid Section Registration Status List."),
                        "sectionRegistrations.status.detail.id", registrationDto.Id);
                }
            }

            // get the section ID associated to the incoming guid
            string sectionId = string.Empty;
            try
            {
                if (registrationDto.Section != null && !string.IsNullOrEmpty(registrationDto.Section.Id))
                {
                    sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(registrationDto.Section.Id);
                }
            }
            catch
            {
                // Fall through to null or empty check
            }
            if (registrationDto.Section != null && !string.IsNullOrEmpty(registrationDto.Section.Id) && string.IsNullOrEmpty(sectionId))
                // throw new KeyNotFoundException("Section ID associated to guid '" + registrationDto.Id + "' not found");
                IntegrationApiExceptionAddError("Section ID associated to guid '" + registrationDto.Section.Id + "' not found",
                    "sectionRegistrations.section.id", registrationDto.Id, "", System.Net.HttpStatusCode.NotFound);

            // convert the request
            var request = await ConvertDtoToRequest4Entity(registrationDto, personId, sectionId);
            request.StudentAcadCredId = stcKey;

            // If we have errors, do not call the update method
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            // process the request
            SectionRegistrationResponse response = null;
            try
            {
                response = await _sectionRegistrationRepository.Update2Async(request, guid, personId, sectionId, statusCode);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            
            // var stcKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(response.Guid);
            stcKey = response.StudentAcadCredKey;

            //check to see if the student acad cred record is not deleted, if it is then just return the request DTO
            if (await _sectionRegistrationRepository.CheckStuAcadCredRecord(stcKey))
            {
                //Get section reg statuses to whose special processing code is 1 or 2
                var sectRegStatuses = (await GetSectionRegistrationStatusesAsync(true))
                        .Where(i => i.Status.RegistrationStatus == Domain.Student.Entities.RegistrationStatus.Registered)
                        .Select(s => s.Code).ToList();

                //Get all person ids
                personIdDict = await _personRepository.GetPersonGuidsWithNoCorpCollectionAsync(new List<string> { response.StudentId });

                //Get all section ids
                var sectionIds = string.IsNullOrEmpty(response.SectionId) ? null : new[] { response.SectionId };
                sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds);

                // convert response to SectionRegistration
                var dtoResponse = await ConvertResponsetoDto4(response, sectRegStatuses, stcKey);

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

                // If we have errors, do not call the update method
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dtoResponse;
            }
            else
            {

                // If we have errors, do not call the update method
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return registrationDto;
            }
        }

        /// <summary>
        /// Converts to person & operIds.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="personIds"></param>
        /// <param name="operIds"></param>
        private void BuildPersonOperIds(SectionRegistrationResponse response, List<string> personIds, List<string> operIds)
        {
            int num;
            var tempPersonIds = new List<string>();
            var tempOperIds = new List<string>();
            if (!string.IsNullOrEmpty(response.StudentId))
            {
                tempPersonIds.Add(response.StudentId);
            }
            if (!string.IsNullOrWhiteSpace(response.TranscriptVerifiedBy) && !tempOperIds.Contains(response.TranscriptVerifiedBy))
            {
                if (int.TryParse(response.TranscriptVerifiedBy, out num))
                {
                    tempPersonIds.Add(response.TranscriptVerifiedBy);
                }
                else
                {
                    tempOperIds.Add(response.TranscriptVerifiedBy);
                }
            }
            if (response.FinalTermGrade != null && !string.IsNullOrWhiteSpace(response.FinalTermGrade.SubmittedBy) &&
                !tempOperIds.Contains(response.FinalTermGrade.SubmittedBy))
            {
                if (int.TryParse(response.FinalTermGrade.SubmittedBy, out num))
                {
                    tempPersonIds.Add(response.FinalTermGrade.SubmittedBy);
                }
                else
                {
                    tempOperIds.Add(response.FinalTermGrade.SubmittedBy);
                }
            }
            if (response.VerifiedTermGrade != null && !string.IsNullOrWhiteSpace(response.VerifiedTermGrade.SubmittedBy) &&
                !tempOperIds.Contains(response.VerifiedTermGrade.SubmittedBy))
            {
                if (int.TryParse(response.VerifiedTermGrade.SubmittedBy, out num))
                {
                    tempPersonIds.Add(response.VerifiedTermGrade.SubmittedBy);
                }
                else
                {
                    tempOperIds.Add(response.VerifiedTermGrade.SubmittedBy);
                }
            }
            if (response.MidTermGrades != null && response.MidTermGrades.Any())
            {
                response.MidTermGrades.ForEach(mt =>
                {
                    if (mt != null && !string.IsNullOrWhiteSpace(mt.SubmittedBy) && !tempOperIds.Contains(mt.SubmittedBy))
                    {
                        if (int.TryParse(mt.SubmittedBy, out num))
                        {
                            tempPersonIds.Add(mt.SubmittedBy);
                        }
                        else
                        {
                            tempOperIds.Add(mt.SubmittedBy);
                        }
                    }
                });
            }
            personIds.AddRange(tempPersonIds);
            operIds.AddRange(tempOperIds);
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
                            request.FinalTermGrade = new TermGrade(registrationDto.Id, null, personId, gradeType.Code);
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
                                request.VerifiedTermGrade = new VerifiedTermGrade(registrationDto.Id, null, personId, gradeType.Code);
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
        /// Checks all the required fields in section registration Dto
        /// </summary>
        /// <param name="registrationDto">registrationDto</param>
        private void CheckForRequiredFields3(string guid, string stcKey, Dtos.SectionRegistration4 registrationDto)
        {
            if (registrationDto == null)
            {
                // throw new ArgumentNullException("registrationDto", "Must provide a SectionRegistration object for update");
                IntegrationApiExceptionAddError("Must provide a SectionRegistration object for update", "sectionRegistrations", guid, stcKey);
                throw IntegrationApiException;
            }

            if (string.IsNullOrEmpty(registrationDto.Id))
                // throw new ArgumentNullException("registrationDto.id", "Must provide an id for section-registrations");
                IntegrationApiExceptionAddError("Must provide an id for section-registrations", "sectionRegistrations.id", guid, stcKey);

            if (registrationDto.Registrant == null)
                // throw new ArgumentNullException("registrationDto.registrant", "Must provide a registrant object for update");
                IntegrationApiExceptionAddError("Must provide a registrant object for update", "sectionRegistrations.registrant", guid, stcKey);

            if (registrationDto.Registrant != null && string.IsNullOrEmpty(registrationDto.Registrant.Id))
                // throw new ArgumentNullException("registrationDto.registrant.id", "Must provide an id for person");
                IntegrationApiExceptionAddError("Must provide an id for person", "sectionRegistrations.registrant.id", guid, stcKey);

            //Academic Level
            if (registrationDto.AcademicLevel == null)
            {
                // throw new ArgumentNullException("registrationDto.academicLevel", "Must provide academic level for person");
                IntegrationApiExceptionAddError("Must provide academic level for person", "sectionRegistrations.academicLevel", guid, stcKey);
            }

            if (registrationDto.AcademicLevel != null && string.IsNullOrEmpty(registrationDto.AcademicLevel.Id))
            {
                // throw new ArgumentNullException("registrationDto.academicLevel.id", "Must provide an id for academic level");
                IntegrationApiExceptionAddError("Must provide an id for academic level", "sectionRegistrations.academicLevel.id", guid, stcKey);
            }

            //Credits
            if (registrationDto.Credit != null && (registrationDto.Credit.Measure == null || registrationDto.Credit.Measure == StudentCourseTransferMeasure.NotSet))
            {
                // throw new ArgumentNullException("registrationDto.credit.measure", "Must provide a credit measure for credit");
                IntegrationApiExceptionAddError("Must provide a credit measure for credit", "sectionRegistrations.credit.measure", guid, stcKey);
            }

            if (registrationDto.Section == null)
                // throw new ArgumentNullException("registrationDto.section", "Must provide a section object for update");
                IntegrationApiExceptionAddError("Must provide a section object for update", "sectionRegistrations.section", guid, stcKey);

            if (registrationDto.Section != null && string.IsNullOrEmpty(registrationDto.Section.Id))
                // throw new ArgumentNullException("registrationDto.section.id", "Must provide an id for section");
                IntegrationApiExceptionAddError("Must provide an id for section", "sectionRegistrations.section.id", guid, stcKey);

            if (registrationDto.Status == null)
                //throw new ArgumentNullException("registrationDto.status", "Must provide a status object for update");
                IntegrationApiExceptionAddError("Must provide a status object for update", "sectionRegistrations.status", guid, stcKey);

            if (registrationDto.Status != null && registrationDto.Status.RegistrationStatus == RegistrationStatus3.NotSet)
                // throw new ArgumentNullException("registrationDto.status.registrationStatus", "Must provide registration status");
                IntegrationApiExceptionAddError("Must provide registration status", "sectionRegistrations.status.registrationStatus", guid, stcKey);

            if (registrationDto.Status != null && registrationDto.Status.SectionRegistrationStatusReason == RegistrationStatusReason3.NotSet)
                // throw new ArgumentNullException("registrationDto.status.sectionregistrationstatusreason", "Must provide section registration status reason");
                IntegrationApiExceptionAddError("Must provide section registration status reason", "sectionRegistrations.status.sectionRegistrationStatusReason", guid, stcKey);

            if (registrationDto.Status != null && registrationDto.Status.Detail != null && string.IsNullOrEmpty(registrationDto.Status.Detail.Id))
                // throw new ArgumentNullException("registrationDto.status.detail.id", "Must provide an id for status");
                IntegrationApiExceptionAddError("Must provide an id for status", "sectionRegistrations.status.detail.id", guid, stcKey);

            if (registrationDto.GradingOption != null && registrationDto.GradingOption.GradeScheme != null && string.IsNullOrEmpty(registrationDto.GradingOption.GradeScheme.Id))
                // throw new ArgumentNullException("registrationDto.gradingOption.gradescheme.id", "Must provide an id for grading option grade scheme");
                IntegrationApiExceptionAddError("Must provide an id for grading option grade scheme", "sectionRegistrations.gradingOption.gradescheme.id", guid, stcKey);
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

        /// <summary>
        /// Checks all the business rules
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="registrationDto">SectionRegistration2 object</param>
        /// <returns></returns>
        private async Task CheckForBusinessRules3Async(string guid, string stcKey, Dtos.SectionRegistration4 registrationDto)
        {
            #region Validate registration status and status reason
            if (registrationDto.Status != null && registrationDto.Status.RegistrationStatus == RegistrationStatus3.NotRegistered && string.IsNullOrEmpty(stcKey))
            {
                IntegrationApiExceptionAddError("Registration status of 'notRegistered' is not allowed when creating new section-registrations.",
                    "sectionRegistrations.status.registrationStatus", guid, stcKey);
            }
            if (registrationDto.Status != null && registrationDto.Status.RegistrationStatus == RegistrationStatus3.Registered && registrationDto.Status.SectionRegistrationStatusReason != RegistrationStatusReason3.Registered)
            {
                IntegrationApiExceptionAddError("Registration status of 'registered' requires a status reason of 'registered'.",
                    "sectionRegistrations.status.registrationStatus", guid, stcKey);
            }
            if (registrationDto.Status != null && registrationDto.Status.RegistrationStatus == RegistrationStatus3.NotRegistered && registrationDto.Status.SectionRegistrationStatusReason == RegistrationStatusReason3.Registered)
            {
                IntegrationApiExceptionAddError("Registration status of 'notRegistered' cannot have a status reason of 'registered'.",
                    "sectionRegistrations.status.registrationStatus", guid, stcKey);
            }
            #endregion

            #region Validate grading option and section
            if (_gradeSchemes == null) _gradeSchemes = await this.GradeSchemesAsync();

            if (registrationDto.GradingOption != null && registrationDto.GradingOption.GradeScheme != null && !string.IsNullOrEmpty(registrationDto.GradingOption.GradeScheme.Id))
            {
                var awardGradeScheme = _gradeSchemes.FirstOrDefault(gs => gs.Guid == registrationDto.GradingOption.GradeScheme.Id);
                if (awardGradeScheme == null)
                    // throw new KeyNotFoundException("Provided grading option grade scheme id is invalid");
                    IntegrationApiExceptionAddError(string.Format("Provided grading option grade scheme id '{0}' is invalid", registrationDto.GradingOption.GradeScheme.Id),
                        "sectionRegistrations.gradingOption.gradeScheme.id",
                        guid, stcKey, System.Net.HttpStatusCode.NotFound);
            }
            Domain.Student.Entities.Section section = null;
            if (registrationDto.Section != null && !string.IsNullOrEmpty(registrationDto.Section.Id))
            {
                try
                {
                    section = await _sectionRepository.GetSectionByGuidAsync(registrationDto.Section.Id);
                }
                catch
                {
                    // Fall through to null check
                }
                if (section == null)
                    // throw new KeyNotFoundException("Provided sectionId is invalid");
                    IntegrationApiExceptionAddError(string.Format("Provided section id '{0}' is invalid", registrationDto.Section.Id),
                        "sectionRegistrations.section.id",
                        guid, stcKey, System.Net.HttpStatusCode.NotFound);
            }

            #endregion

            #region Involvement
            //Check involvement startOn before section start date
            if (registrationDto.Involvement != null && section != null)
            {
                if (registrationDto.Involvement.StartOn != null && registrationDto.Involvement.EndOn != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > registrationDto.Involvement.EndOn.Value.Date)
                    {
                        // throw new InvalidOperationException("The participation start date can't be after the participation end date.");
                        IntegrationApiExceptionAddError("The participation start date cannot be after the participation end date.",
                            "sectionRegistrations.involvement.startOn", guid, stcKey);
                    }
                }

                if (registrationDto.Involvement.StartOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date < section.StartDate.Date)
                    {
                        // throw new InvalidOperationException("The participation start date can't be before the section start date.");
                        IntegrationApiExceptionAddError(string.Format("The participation start date cannot be before the section start date of '{0}'.", section.StartDate.Date.ToShortDateString()),
                            "sectionRegistrations.involvement.startOn", guid, stcKey);
                    }
                }
                //Check involvement startOn after section end date
                if (registrationDto.Involvement.StartOn != null && section.EndDate != null && section.EndDate.HasValue)
                {
                    if (registrationDto.Involvement.StartOn.Value.Date > section.EndDate.Value.Date)
                    {
                        // throw new InvalidOperationException("The participation start date can't be after the section end date.");
                        IntegrationApiExceptionAddError(string.Format("The participation start date cannot be after the section end date of '{0}'.", section.EndDate.Value.Date.ToShortDateString()),
                            "sectionRegistrations.involvement.startOn", guid, stcKey);
                    }
                }
                //Check involvement endOn after section start date
                if (registrationDto.Involvement.EndOn != null && section.StartDate != null)
                {
                    if (registrationDto.Involvement.EndOn.Value.Date < section.StartDate.Date)
                    {
                        // throw new InvalidOperationException("The participation end date can't be before the section start date.");
                        IntegrationApiExceptionAddError(string.Format("The participation end date cannot be before the section start date of '{0}'.", section.StartDate.Date.ToShortDateString()),
                            "sectionRegistrations.involvement.endOn", guid, stcKey);
                    }
                }
                //Check involvement endOn after section end date
                if (registrationDto.Involvement.EndOn != null && section.EndDate != null && section.EndDate.HasValue)
                {
                    if (registrationDto.Involvement.EndOn.Value.Date > section.EndDate.Value.Date)
                    {
                        // throw new InvalidOperationException("The participation end date can't be after the section end date.");
                        IntegrationApiExceptionAddError(string.Format("The participation end date cannot be after the section end date of '{0}'.", section.EndDate.Value.Date.ToShortDateString()),
                            "sectionRegistrations.involvement.endOn", guid, stcKey);
                    }
                }
            }

            #endregion

            #region Overrides
            if (registrationDto.Override != null)
            {
                SectionRegistrationResponse registration = null;
                if (!string.IsNullOrEmpty(guid) && guid != "00000000-0000-0000-0000-000000000000")
                    registration = await _sectionRegistrationRepository.GetSectionRegistrationByIdAsync(await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(guid));
                if (registration != null)
                {
                    if (registrationDto.Override.AcademicPeriod != null && !string.IsNullOrEmpty(registrationDto.Override.AcademicPeriod.Id))
                    {
                        var overrideTerm = AcademicPeriods().Where(ap => ap.Guid == registrationDto.Override.AcademicPeriod.Id).FirstOrDefault();
                        if (overrideTerm == null || overrideTerm.Code != registration.OverrideAcadPeriod)
                        {
                            IntegrationApiExceptionAddError("The academicPeriod cannot be overridden using section-registratons PUT or POST",
                                "sectionRegistrations.override.academicPeriod.id", guid);
                        }
                    }
                    if (registrationDto.Override.Site != null && !string.IsNullOrEmpty(registrationDto.Override.Site.Id))
                    {
                        var overrideSite = (await GetLocationsAsync()).Where(ap => ap.Guid == registrationDto.Override.Site.Id).FirstOrDefault();
                        if (overrideSite == null || overrideSite.Code != registration.OverrideSite)
                        {
                            IntegrationApiExceptionAddError("The site cannot be overridden using section-registratons PUT or POST",
                                "sectionRegistrations.override.site.id", guid);
                        }
                    }
                }
                else
                {
                    if (registrationDto.Override.AcademicPeriod != null && !string.IsNullOrEmpty(registrationDto.Override.AcademicPeriod.Id))
                    {
                        var overrideTerm = AcademicPeriods().Where(ap => ap.Guid == registrationDto.Override.AcademicPeriod.Id).FirstOrDefault();
                        if (overrideTerm == null || overrideTerm.Code != section.TermId)
                        {
                            IntegrationApiExceptionAddError("The academicPeriod cannot be overridden using section-registratons PUT or POST",
                                "sectionRegistrations.override.academicPeriod.id", guid);
                        }
                    }
                    if (registrationDto.Override.Site != null && !string.IsNullOrEmpty(registrationDto.Override.Site.Id))
                    {
                        var overrideSite = (await GetLocationsAsync()).Where(ap => ap.Guid == registrationDto.Override.Site.Id).FirstOrDefault();
                        if (overrideSite == null || overrideSite.Code != section.Location)
                        {
                            IntegrationApiExceptionAddError("The site cannot be overridden using section-registratons PUT or POST",
                                "sectionRegistrations.override.site.id", guid);
                        }
                    }
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
            // Make sure whe have a valid GUID for the record we are dealing with
            if (string.IsNullOrEmpty(registrationDto.Guid))
            {
                throw new KeyNotFoundException("Could not find a GUID for section-registrations entity.");
            }
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
        /// Convert a Dto to a registration request.
        /// </summary>
        /// <param name="sectionRegistrationDto">The section <see cref="Dtos.SectionRegistration4">registration</see> Object.</param>
        /// <returns>Registration <see cref="RegistrationRequest">request</see></returns>
        private async Task<SectionRegistrationRequest> ConvertDtoToRequest4Entity(Dtos.SectionRegistration4 sectionRegistrationDto, string personId, string sectionId)
        {
            Ellucian.Colleague.Domain.Student.Entities.SectionRegistration sectionRegistration = new Domain.Student.Entities.SectionRegistration();

            if (sectionRegistrationDto.Status != null && sectionRegistrationDto.Status.RegistrationStatus != RegistrationStatus3.NotSet)
            {
                // Set appropriate action in the request.
                switch (sectionRegistrationDto.Status.RegistrationStatus)
                {
                    case Dtos.EnumProperties.RegistrationStatus3.Registered:
                        {
                            //Transcript is not required so if its null then assign Add as default Action
                            if (sectionRegistrationDto.GradingOption == null || sectionRegistrationDto.GradingOption.Mode == null)
                            {
                                sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                            }
                            else
                            {
                                // Using Transcript Mode, update Pass/Fail or Audit flag.
                                switch (sectionRegistrationDto.GradingOption.Mode)
                                {
                                    case TranscriptMode2.Audit:
                                        sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Audit;
                                        break;
                                    case TranscriptMode2.PassFail:
                                        sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.PassFail;
                                        break;
                                    default:
                                        sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Add;
                                        break;
                                }
                            }
                        }
                        break;
                    case Dtos.EnumProperties.RegistrationStatus3.NotRegistered:
                        {
                            sectionRegistration.Action = Domain.Student.Entities.RegistrationAction.Drop;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            sectionRegistration.SectionId = sectionId;
            sectionRegistration.RegistrationDate = sectionRegistrationDto.StatusDate;

            //Academic levels
            if (sectionRegistrationDto.AcademicLevel != null && !string.IsNullOrEmpty(sectionRegistrationDto.AcademicLevel.Id))
            {
                var academicLevel = (await AcademicLevelsAsync()).FirstOrDefault(al => al.Guid.Equals(sectionRegistrationDto.AcademicLevel.Id, StringComparison.OrdinalIgnoreCase));
                if (academicLevel == null)
                {
                    // throw new KeyNotFoundException("Academic level ID associated to guid '" + sectionRegistrationDto.AcademicLevel.Id + "' not found");
                    IntegrationApiExceptionAddError("Academic level ID associated to guid '" + sectionRegistrationDto.AcademicLevel.Id + "' not found",
                        "sectionRegistrations.academicLevel.id", sectionRegistrationDto.Id);
                }
                else
                {
                    sectionRegistration.AcademicLevelCode = academicLevel.Code;
                }
            }

            //Attempted credit or ceus
            if (sectionRegistrationDto.Credit != null && sectionRegistrationDto.Credit.Measure != null)
            {
                if (sectionRegistrationDto.Credit.Measure == StudentCourseTransferMeasure.Credit || sectionRegistrationDto.Credit.Measure == StudentCourseTransferMeasure.Hour)
                {
                    if (sectionRegistrationDto.Credit.RegistrationCredit != null)
                    {
                        sectionRegistration.Credits = sectionRegistrationDto.Credit.RegistrationCredit;
                    }
                }
                if (sectionRegistrationDto.Credit.Measure == StudentCourseTransferMeasure.Ceu)
                {
                    if (sectionRegistrationDto.Credit.RegistrationCredit != null)
                    {
                        sectionRegistration.Ceus = sectionRegistrationDto.Credit.RegistrationCredit;
                    }
                }
            }
            // Return a new registration request object
            var request = new SectionRegistrationRequest(personId, sectionRegistrationDto.Id, sectionRegistration)
            {
                CreateStudentFlag = true
            };
            if (sectionRegistrationDto.Involvement != null && sectionRegistrationDto.Involvement.StartOn != null && sectionRegistrationDto.Involvement.StartOn.HasValue)
            {
                request.InvolvementStartOn = sectionRegistrationDto.Involvement.StartOn;
            }
            if (sectionRegistrationDto.Involvement != null && sectionRegistrationDto.Involvement.EndOn != null && sectionRegistrationDto.Involvement.EndOn.HasValue)
            {
                request.InvolvementEndOn = sectionRegistrationDto.Involvement.EndOn;
            }
            return request;
        }

        /// <summary>
        /// Convert the Registration Response into the DTO response for Section Registration
        /// </summary>
        /// <param name="source">Response from Registration Process with warnings and messages</param>
        /// <returns>SectionRegistration Object <see cref="Dtos.SectionRegistration2"/></returns>
        private async Task<Dtos.SectionRegistration2> ConvertResponsetoDto2(SectionRegistrationResponse source)
        {
            var gradeSchemeGuid = ConvertCodeToGuid(await GradeSchemesAsync(), source.GradeScheme);
            var dto = new Dtos.SectionRegistration2();
            dto.Approvals = new List<Dtos.Approval2>() { new Dtos.Approval2() { ApprovalType = Dtos.ApprovalType2.All, ApprovalEntity = Dtos.ApprovalEntity.System } };
            dto.Id = source.Guid;

            // Make sure whe have a valid GUID for the record we are dealing with
            if (string.IsNullOrEmpty(source.Guid))
            {
                throw new ArgumentNullException(string.Format("sectionRegistrations.id", "Could not find a GUID for section-registrations entity '{0}'.", source.StudentAcadCredKey));
            }

            try
            {
                string studentId = string.Empty;
                if (personIdDict != null && personIdDict.Any() && personIdDict.TryGetValue(source.StudentId, out studentId))
                {
                    dto.Registrant = new Dtos.GuidObject2(studentId);
                }
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    throw new InvalidOperationException(string.Format("No registrant found for guid {0}.", source.Guid));
                }
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("registrant.id", "Registrant id is required.");
            }

            try
            {
                string sectionId = string.Empty;
                if (sectionIdDict != null && sectionIdDict.Any() && sectionIdDict.TryGetValue(source.SectionId, out sectionId))
                {
                    dto.Section = new Dtos.GuidObject2(sectionId);
                }
                if (string.IsNullOrWhiteSpace(sectionId))
                {
                    throw new InvalidOperationException(string.Format("No section found for guid {0}.", source.Guid));
                }
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("section.id", "Section id is required.");
            }
            //add try catch around this to display record Id for the bad data
            try
            {
                dto.Status = await ConvertResponseStatusToRegistrationStatusAsync(source.StatusCode);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Concat(ex.Message, "Entity:'STUDENT.ACAD.CRED', Record ID:'", source.Guid, "'"));
            }
            dto.AwardGradeScheme = new Dtos.GuidObject2(gradeSchemeGuid);
            dto.Transcript = new Dtos.SectionRegistrationTranscript() { GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid), Mode = await ConvertPassAuditToMode(source) };
            dto.SectionRegistrationGrades = await ConvertResponseGradesToSectionRegistrationGradesAsync(source);
            dto.Process = await ConvertResponsetoSectionRegistrationProcessAsync(source);
            dto.Involvement = ConvertResponseSectionRegistrationToInvolvement(source);
            dto.SectionRegistrationReporting = ConvertResponseToSectionRegistrationReporting(source);

            return dto;
        }

        /// <summary>
        /// Convert the Registration Response into the DTO response for Section Registration
        /// </summary>
        /// <param name="source">Response from Registration Process with warnings and messages</param>
        /// <returns>SectionRegistration Object <see cref="Dtos.SectionRegistration2"/></returns>
        private async Task<Dtos.SectionRegistration3> ConvertResponsetoDto3(SectionRegistrationResponse source)
        {
            var gradeSchemeGuid = ConvertCodeToGuid(await GradeSchemesAsync(), source.GradeScheme);
            var dto = new Dtos.SectionRegistration3();
            dto.Approvals = new List<Dtos.Approval2>() { new Dtos.Approval2() { ApprovalType = Dtos.ApprovalType2.All, ApprovalEntity = Dtos.ApprovalEntity.System } };
            dto.Id = source.Guid;

            // Make sure whe have a valid GUID for the record we are dealing with
            if (string.IsNullOrEmpty(source.Guid))
            {
                throw new ArgumentNullException(string.Format("sectionRegistrations.id", "Could not find a GUID for section-registrations entity '{0}'.", source.StudentAcadCredKey));
            }

            try
            {
                string studentId = string.Empty;
                if (personIdDict != null && personIdDict.Any() && !personIdDict.TryGetValue(source.StudentId, out studentId))
                {
                    throw new InvalidOperationException(string.Format("No registrant found for guid {0}.", source.Guid));
                }
                dto.Registrant = new Dtos.GuidObject2(studentId);
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("registrant.id", "Registrant id is required.");
            }

            try
            {
                string sectionId = string.Empty;
                if (sectionIdDict != null && sectionIdDict.Any() && !sectionIdDict.TryGetValue(source.SectionId, out sectionId))
                {
                    throw new InvalidOperationException(string.Format("No section found for guid {0}.", source.Guid));
                }
                dto.Section = new Dtos.GuidObject2(sectionId);
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("section.id", "Section id is required.");
            }

            //add try catch around this to display record Id for the bad data
            try
            {
                dto.Status = await ConvertResponseStatusToRegistrationStatusAsync(source.StatusCode);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Concat(ex.Message, "Entity:'STUDENT.ACAD.CRED', Record ID:'", source.Guid, "'"));
            }
            dto.AwardGradeScheme = new Dtos.GuidObject2(gradeSchemeGuid);
            dto.Transcript = new Dtos.SectionRegistrationTranscript() { GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid), Mode = await ConvertPassAuditToMode(source) };
            dto.SectionRegistrationGrades = await ConvertResponseGradesToSectionRegistrationGradesAsync(source);
            dto.Process = await ConvertResponsetoSectionRegistrationProcessAsync(source);
            dto.Involvement = ConvertResponseSectionRegistrationToInvolvement(source);
            dto.SectionRegistrationReporting = ConvertResponseToSectionRegistrationReporting(source);
            dto.AcademicLevel = await ConvertResponseToAcademicLevelAsync(source);
            dto.RepeatedSection = ConvertResponseToRepeatedSection(source);
            dto.Credit = await ConvertResponseToSectionRegistrationCreditAsync(source);
            dto.QualityPoints = ConvertResponseToQualifiedPoints(source);

            return dto;
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
                        gradeChangeReasonEntities.FirstOrDefault(x => x.Code == response.FinalTermGrade.GradeChangeReason);
                    gradeChangeReasonGuid = gradeChangeReason == null ? null : gradeChangeReason.Guid;
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
                        gradeChangeReasonEntities.FirstOrDefault(x => x.Code == response.VerifiedTermGrade.GradeChangeReason);
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
                                                                                                    IEnumerable<Domain.Base.Entities.GradeChangeReason> gradeChangeReasonEntities)
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
                    KeyValuePair<string, string> guidKVP;
                    if (operIdsWithGuids != null && operIdsWithGuids.Any() && (operIdsWithGuids.TryGetValue(submittedBy, out guidKVP)))
                    {
                        var guid = guidKVP.Value;
                        if (!string.IsNullOrEmpty(guid))
                        {
                            submittedByGuidObject = new GuidObject2(guid);
                        }
                    }

                    //only display the submittedBy if the Id is a person 
                    // check if the input in numbers then it is possibly Id, otherwise it is OPERS
                    string studentId = string.Empty;
                    if (personIdDict != null && personIdDict.Any() && personIdDict.TryGetValue(submittedBy, out studentId))
                    {
                        submittedByGuidObject = new Dtos.GuidObject2(studentId);
                    }
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
                    if (operIdsWithGuids != null && operIdsWithGuids.Any())
                    {
                        var guidkvPair = new KeyValuePair<string, string>();
                        if (operIdsWithGuids.TryGetValue(response.TranscriptVerifiedBy, out guidkvPair))
                        {
                            if (!string.IsNullOrEmpty(guidkvPair.Value))
                            {
                                transcriptVerifiedByGuidObject = new GuidObject2(guidkvPair.Value);
                            }
                        }
                    }
                }
            }
            catch (ArgumentNullException)
            {
                logger.Error(string.Format("No corresponding guid found for transcriptVerifiedBy: '{0}'", response.TranscriptVerifiedBy));
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
            involvement.StartOn = response.InvolvementStartOn.HasValue ? response.InvolvementStartOn.Value.Date : default(DateTimeOffset?);
            involvement.EndOn = response.InvolvementEndOn.HasValue ? response.InvolvementEndOn.Value.Date : default(DateTimeOffset?);
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
                        if (status.Status.SectionRegistrationStatusReason == Domain.Student.Entities.RegistrationStatusReason.Withdrawn)
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
            if (status == null)
            {
                throw new ArgumentException(string.Concat("The section registration status of '", statusCode, "' is invalid. "));
            }
            else
            {
                detail = new Dtos.GuidObject2() { Id = status.Guid };
                var regStatus = status.Status.RegistrationStatus;
                var regStatusReason = status.Status.SectionRegistrationStatusReason;

                switch (regStatus)
                {
                    case Domain.Student.Entities.RegistrationStatus.Registered:
                        {
                            registrationStatus = Dtos.RegistrationStatus2.Registered;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatus.NotRegistered:
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
                    case Domain.Student.Entities.RegistrationStatusReason.Canceled:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Canceled;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Dropped:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Dropped;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Pending:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Pending;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Registered:
                        {
                            statusReason = Dtos.RegistrationStatusReason2.Registered;
                            break;
                        }
                    case Domain.Student.Entities.RegistrationStatusReason.Withdrawn:
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
                    // throw new PermissionsException("User is not authorized to update section-registrations.");
                    IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to update section-registrations.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                    throw IntegrationApiException;
                }
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person.
        /// </summary>
        private void CheckUserRegistrationViewPermissions()
        {
            // access is ok if the current user has the view, create, or update registrations permission
            if (!HasPermission(SectionPermissionCodes.ViewRegistrations) && !HasPermission(SectionPermissionCodes.UpdateRegistrations))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view section-registrations.");
                // throw new PermissionsException("User is not authorized to view section-registrations.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view section-registrations.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }
        #endregion

        #region Section Registration Grade Options

        /// <summary>
        /// Gets collection of section registration grade options.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Dtos.SectionRegistrationsGradeOptions</returns>
        public async Task<Tuple<IEnumerable<SectionRegistrationsGradeOptions>, int>> GetSectionRegistrationsGradeOptionsAsync(int offset, int limit, SectionRegistrationsGradeOptions criteriaObj, bool bypassCache)
        {
            CheckUserRegistrationViewPermissions();

            List<SectionRegistrationsGradeOptions> dtos = new List<SectionRegistrationsGradeOptions>();
            StudentAcadCredCourseSecInfo criteria = null;
            if (criteriaObj != null && criteriaObj.Section != null && !string.IsNullOrEmpty(criteriaObj.Section.Id))
            {
                try
                {
                    var sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(criteriaObj.Section.Id);
                    if (string.IsNullOrEmpty(sectionId))
                    {
                        return new Tuple<IEnumerable<Dtos.SectionRegistrationsGradeOptions>, int>(dtos, 0);
                    }
                    criteria = new StudentAcadCredCourseSecInfo(sectionId);
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.SectionRegistrationsGradeOptions>, int>(dtos, 0);
                }
            }

            Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int> entities = null;

            try
            {
                entities = await _sectionRegistrationRepository.GetSectionRegistrationGradeOptionsAsync(offset, limit, criteria);
            }
            catch (RepositoryException)
            {
                throw;
            }

            if (entities == null || !entities.Item1.Any())
            {
                return new Tuple<IEnumerable<SectionRegistrationsGradeOptions>, int>(new List<SectionRegistrationsGradeOptions>(), 0);
            }

            var totalCount = entities.Item2;

            //Get all section ID's
            var sectionGuidDictionary = new Dictionary<string, string>();
            try
            {
                var sectionRecordKeys = entities.Item1.Where(i => !string.IsNullOrEmpty(i.SectionId)).Select(item => item.SectionId).Distinct().ToList();
                sectionGuidDictionary = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionRecordKeys.ToArray());
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while getting course section guids.", ex);
            }

            //Get grade terms
            var gradeTerms = await GetGradingTermAsync(bypassCache);
            List<string> gradeCodes = null;
            if (gradeTerms != null && gradeTerms.Any())
            {
                gradeCodes = gradeTerms.Select(i => i.Code).ToList();
            }

            List<Domain.Student.Entities.GradeScheme> gradeSchemes = null;
            List<Domain.Student.Entities.Grade> grades = null;
            var acadGradeSchemes = entities.Item1.Where(i => !string.IsNullOrEmpty(i.GradeScheme)).Distinct().Select(s => s.GradeScheme).ToList();
            if (acadGradeSchemes != null && acadGradeSchemes.Any())
            {
                //get Grade schemes
                gradeSchemes = (await GradeSchemesAsync(bypassCache)).Where(i => acadGradeSchemes.Contains(i.Code)).ToList();
                //get grade definitions
                grades = (await GetGradeHedmAsync(bypassCache)).Where(g => acadGradeSchemes.Contains(g.GradeSchemeCode) && g.ExcludeFromFacultyGrading == false).ToList();
            }

            foreach (var entity in entities.Item1)
            {
                dtos.Add(ConvertGradeOptionEntityToDto(entity, sectionGuidDictionary, gradeCodes, gradeSchemes, grades, bypassCache));
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return dtos.Any() ? new Tuple<IEnumerable<SectionRegistrationsGradeOptions>, int>(dtos, totalCount) :
                new Tuple<IEnumerable<SectionRegistrationsGradeOptions>, int>(new List<SectionRegistrationsGradeOptions>(), 0);
        }

        /// <summary>
        /// Gets a section registration grade option by guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<SectionRegistrationsGradeOptions> GetSectionRegistrationsGradeOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            CheckUserRegistrationViewPermissions();

            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a section registrations grade options GUID for retrieval.");
            string recordKey = string.Empty;
            try
            {
                recordKey = await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(guid);
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("No section registration was found for guid: {0}.", guid));
            }

            StudentAcadCredCourseSecInfo entity = null;
            try
            {
            // process the request
            entity = await _sectionRegistrationRepository.GetSectionRegistrationGradeOptionsByIdAsync(recordKey);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("No section registration was found for guid: {0}.", guid));
            }

            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            //Get all section ID's
            Dictionary<string, string> sectionGuidDictionary = new Dictionary<string, string>();
            try
            {
                var sectionRecordKey = entity.SectionId;
                sectionGuidDictionary = await _sectionRepository.GetSectionGuidsCollectionAsync(new List<string>() { sectionRecordKey });
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            //Get grade terms
            var gradeTerms = await GetGradingTermAsync(bypassCache);
            List<string> gradeCodes = null;
            if (gradeTerms != null && gradeTerms.Any())
            {
                gradeCodes = gradeTerms.Select(i => i.Code).ToList();
            }

            List<Domain.Student.Entities.GradeScheme> gradeSchemes = null;
            List<Domain.Student.Entities.Grade> grades = null;
            if (!string.IsNullOrEmpty(entity.GradeScheme))
            {
                //get Grade schemes
                var acadGradeSchemes = new List<string>() { entity.GradeScheme };
                gradeSchemes = (await GradeSchemesAsync(bypassCache)).Where(i => acadGradeSchemes.Contains(i.Code)).Distinct().ToList();

                //get grade definitions
                grades = (await GetGradeHedmAsync(bypassCache)).Where(g => acadGradeSchemes.Contains(g.GradeSchemeCode) && g.ExcludeFromFacultyGrading == false).Distinct().ToList();
            }
            var dto = ConvertGradeOptionEntityToDto(entity, sectionGuidDictionary, gradeCodes, gradeSchemes, grades, bypassCache);

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return dto;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sectionGuidDictionary"></param>
        /// <param name="gradeCodes"></param>
        /// <param name="terms"></param>
        /// <param name="gradeSchemes"></param>
        /// <param name="grades"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private SectionRegistrationsGradeOptions ConvertGradeOptionEntityToDto(StudentAcadCredCourseSecInfo source, Dictionary<string, string> sectionGuidDictionary,
            List<string> gradeCodes, List<Domain.Student.Entities.GradeScheme> gradeSchemes, List<Domain.Student.Entities.Grade> grades, bool bypassCache)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source", "Source is required.");
            }

            SectionRegistrationsGradeOptions dto = new SectionRegistrationsGradeOptions();

            //dto.id
            dto.Id = source.RecordGuid;

            //dto.section.id
            string sectionGuid = null;
            if (!sectionGuidDictionary.TryGetValue(source.SectionId, out sectionGuid))
            {
                IntegrationApiExceptionAddError(string.Format("Section ID is required. Record key '{0}'.", source.RecordKey), 
                    "section.id", source.RecordGuid, source.RecordKey);
            }
            else
            {
                dto.Section = new GuidObject2(sectionGuid);
            }

            //dto.sectionGradability
            if (!string.IsNullOrEmpty(source.Term) && gradeCodes != null && gradeCodes.Any())
            {
                if (gradeCodes.Contains(source.Term))
                {
                    dto.SectionGradability = SectionRegistrationsGradeOptionsSectionGradability.Gradable;
                }
                else
                {
                    dto.SectionGradability = SectionRegistrationsGradeOptionsSectionGradability.Notgradable;
                }
            }
            else if (gradeCodes != null && gradeCodes.Any() && source.StartDate.HasValue && source.EndDate.HasValue)
            {
                var earlyTerm = AcademicPeriods(bypassCache).Where(t => gradeCodes.Contains(t.Code)).OrderBy(d => d.StartDate).FirstOrDefault();
                var lateTerm = AcademicPeriods(bypassCache).Where(t => gradeCodes.Contains(t.Code)).OrderByDescending(d => d.EndDate).FirstOrDefault();

                if (earlyTerm != null && earlyTerm.StartDate != null && lateTerm != null && lateTerm.EndDate != null &&
                    source.StartDate.Value >= earlyTerm.StartDate && source.EndDate.Value <= lateTerm.EndDate)
                {
                    dto.SectionGradability = SectionRegistrationsGradeOptionsSectionGradability.Gradable;
                }
                else
                {
                    dto.SectionGradability = SectionRegistrationsGradeOptionsSectionGradability.Notgradable;
                }
            }
            else if (gradeCodes == null || (gradeCodes != null && ! gradeCodes.Any()))
            {
                dto.SectionGradability = SectionRegistrationsGradeOptionsSectionGradability.Notgradable;
            }

            //dto.gradeStatus
            if (!string.IsNullOrEmpty(source.VerifiedGrade))
            {
                dto.GradeStatus = SectionRegistrationsGradeOptionsGradeStatus.Verified;
            }
            else if (string.IsNullOrEmpty(source.VerifiedGrade) && !string.IsNullOrEmpty(source.FinalGrade))
            {
                dto.GradeStatus = SectionRegistrationsGradeOptionsGradeStatus.Unverified;
            }
            else
            {
                dto.GradeStatus = null;
            }

            //dto.studentGradeScheme && dto.grades
            if (!string.IsNullOrEmpty(source.GradeScheme) && gradeSchemes != null && gradeSchemes.Any())
            {
                //dto.studentGradeScheme
                var grdScheme = gradeSchemes.FirstOrDefault(sch => sch.Code.Equals(source.GradeScheme, StringComparison.OrdinalIgnoreCase));
                if (grdScheme == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Grade scheme not found. Code '{0}'.", source.GradeScheme), 
                        "studentGradeScheme.detail.id", source.RecordGuid, source.RecordKey);
                }
                else
                {
                    dto.StudentGradeScheme = new StudentGradeSchemeDtoProperty()
                    {
                        Detail = new GuidObject2(grdScheme.Guid),
                        Title = string.IsNullOrEmpty(grdScheme.Description) ? null : grdScheme.Description
                    };
                }
                //dto.grades
                if (grades != null && grades.Any())
                {
                    var grds = grades.Where(grd => grd.GradeSchemeCode.Equals(grdScheme.Code, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (grds != null && grds.Any())
                    {
                        List<SectionRegistrationsGradeOptionsGrades> dtoGrades = new List<SectionRegistrationsGradeOptionsGrades>();
                        foreach (var grd in grds)
                        {
                            //grades.grade.id & grades.value
                            SectionRegistrationsGradeOptionsGrades dtoGrade = new SectionRegistrationsGradeOptionsGrades()
                            {
                                Grade = new GuidObject2(grd.Guid),
                                Value = grd.GradeValue.HasValue ? grd.GradeValue.Value.ToString() : null
                            };

                            //grades.incompleteGradeDetails.finalGradeDefault.id
                            if (!string.IsNullOrEmpty(grd.IncompleteGrade))
                            {
                                var incompleteGrd = grds.FirstOrDefault(item => item.Id.Equals(grd.IncompleteGrade, StringComparison.OrdinalIgnoreCase));
                                if (incompleteGrd == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Grade not found. ID '{0}'.", grd.IncompleteGrade), 
                                        "grades.incompleteGradeDetails.finalGradeDefault.id", source.RecordGuid, source.RecordKey);
                                }
                                else
                                {
                                    dtoGrade.IncompleteGradeDetails = new IncompleteGradeDetailsDtoProperty()
                                    {
                                        FinalGradeDefault = new GuidObject2(incompleteGrd.Guid)
                                    };
                                }
                            }

                            //grades.lastDateOfAttendanceRequiredness
                            if (grd.RequireLastAttendanceDate)
                            {
                                dtoGrade.LastDateOfAttendanceRequiredness = RequiredNotRequired.Required;
                            }
                            else
                            {
                                dtoGrade.LastDateOfAttendanceRequiredness = RequiredNotRequired.NotRequired;
                            }
                            dtoGrades.Add(dtoGrade);
                        }

                        dto.Grades = dtoGrades.Any() ? dtoGrades : null;
                    }
                }
            }

            return dto;
        }

        #endregion
    }
}