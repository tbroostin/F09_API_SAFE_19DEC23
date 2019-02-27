//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
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
    public class StudentGradePointAveragesService : BaseCoordinationService, IStudentGradePointAveragesService
    {
        private readonly IStudentGradePointAveragesRepository _studentGradePointAveragesRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAcademicCredentialsRepository _academicCredentialsRepository;


        public StudentGradePointAveragesService(
            IStudentGradePointAveragesRepository studentGradePointAveragesRepository,
            IPersonRepository personRepository,
            IStudentRepository studentRepository,
            ITermRepository termRepository,
            IAcademicCredentialsRepository academicCredentialsRepository,
            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentGradePointAveragesRepository = studentGradePointAveragesRepository;
            _personRepository = personRepository;
            _studentRepository = studentRepository;
            _termRepository = termRepository;
            _academicCredentialsRepository = academicCredentialsRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        public async Task<Tuple<IEnumerable<StudentGradePointAverages>, int>> GetStudentGradePointAveragesAsync(int offset, int limit, StudentGradePointAverages criteriaObj,
            string gradeDateFilterValue, bool bypassCache)
        {
            //Check if user has view permissions.
            if (!await HasStudentGradePointAveragesPermissionsAsync())
            {
                // throw new PermissionsException(string.Format("User {0} does not have permission to view grade point averages.", CurrentUser.UserId));
                IntegrationApiExceptionAddError(string.Format("User '{0}' is not authorized to view grade point averages.", CurrentUser.UserId), "Authentication.Required", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }

            /*
                Handle all the filters 
            */
            StudentAcademicCredit sGpa = null;
            if (criteriaObj != null && criteriaObj.Student != null && !string.IsNullOrEmpty(criteriaObj.Student.Id))
            {
                var studentId = await _personRepository.GetPersonIdFromGuidAsync(criteriaObj.Student.Id);
                if (string.IsNullOrEmpty(studentId))
                {
                    return new Tuple<IEnumerable<StudentGradePointAverages>, int>(new List<StudentGradePointAverages>(), 0);
                }
                sGpa = new StudentAcademicCredit(studentId);
            }

            if (criteriaObj != null && criteriaObj.PeriodBased != null && criteriaObj.PeriodBased.Any())
            {
                List<string> academicPeriods = new List<string>();
                foreach (var periodBased in criteriaObj.PeriodBased)
                {
                    if(periodBased.AcademicPeriod != null && !string.IsNullOrEmpty(periodBased.AcademicPeriod.Id))
                    {
                        var acadPeriod = (await AcademicPeriodsAsync(bypassCache)).FirstOrDefault(ap => ap.Guid.Equals(periodBased.AcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                        if (acadPeriod == null || string.IsNullOrEmpty(acadPeriod.Code))
                        {
                            return new Tuple<IEnumerable<StudentGradePointAverages>, int>(new List<StudentGradePointAverages>(), 0);
                        }
                        academicPeriods.Add(acadPeriod.Code);
                    }
                }
                if (sGpa == null) sGpa = new StudentAcademicCredit();
                sGpa.AcademicPeriods = academicPeriods;
            }

            string gradeDate = !string.IsNullOrEmpty(gradeDateFilterValue) ? await ConvertDateArgument(gradeDateFilterValue) : string.Empty;

            //Get all student acad credits to calculate gpa's
            Tuple<IEnumerable<StudentAcademicCredit>, int> gpaEntities = null;
            try
            {
                gpaEntities = await _studentGradePointAveragesRepository.GetStudentGpasAsync(offset, limit, sGpa, gradeDate);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            
            //total count
            int totalCount = gpaEntities.Item2;

            List<StudentGradePointAverages> gpas = new List<StudentGradePointAverages>();

            if (gpaEntities != null && gpaEntities.Item1.Any())
            {
                //Get all person id's
                var personStIds = gpaEntities.Item1.Select(repo => repo.StudentId);
                Dictionary<string, string> personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(personStIds);

                //Get all id's for academic periods
                var termIdList = new List<string>();
                var termIds = gpaEntities.Item1
                                         .Where(i => i.StudentGPAInfoList != null && i.StudentGPAInfoList.Any())
                                         .SelectMany(id => id.StudentGPAInfoList.Where(t => !string.IsNullOrWhiteSpace(t.Term))
                                                                                .Select(t => t.Term)).Distinct().ToList();
                var reportingTermIds = gpaEntities.Item1
                                         .Where(i => i.StudentGPAInfoList != null && i.StudentGPAInfoList.Any())
                                         .SelectMany(id => id.StudentGPAInfoList.Where(t => !string.IsNullOrWhiteSpace(t.StcReportingTerm))
                                                                                .Select(t => t.StcReportingTerm)).Distinct().ToList();
                termIdList.AddRange(termIds.Union(reportingTermIds).Distinct());

                var acadPeriods = (await AcademicPeriodsAsync(bypassCache)).Where(rec => termIdList.Contains(rec.Code));

                //Get all student acad programs info based marked cred ids
                var markCredIds = gpaEntities.Item1.Where(i => i.StudentGPAInfoList != null && i.StudentGPAInfoList.Any())
                                                   .SelectMany(cr => cr.StudentGPAInfoList)
                                                   .SelectMany(i => i.MarkAcadCredentials);
                IEnumerable<StudentAcademicCredentialProgramInfo> studCredPrgInfo = null;
                if (markCredIds != null || markCredIds.Any())
                {
                    studCredPrgInfo = await _studentGradePointAveragesRepository.GetStudentCredProgramInfoAsync(markCredIds.Distinct());
                }

                //Get the altcum flag
                bool useAltCumFlag = await _studentGradePointAveragesRepository.UseAlternativeCumulativeValuesAsync();

                foreach (var gpaEntity in gpaEntities.Item1)
                {
                    var dto = await ConvertStudentGradePointAveragesEntityToDtoAsync(gpaEntity, personIdDict, acadPeriods, studCredPrgInfo, useAltCumFlag, bypassCache);
                    if (IntegrationApiException == null)
                        gpas.Add(dto);
                }
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return gpas.Any() ? new Tuple<IEnumerable<StudentGradePointAverages>, int>(gpas, totalCount) :
                new Tuple<IEnumerable<StudentGradePointAverages>, int>(new List<StudentGradePointAverages>(), 0);
        }


        /// <summary>
        /// Gets record key for student grade point average based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<StudentGradePointAverages> GetStudentGradePointAveragesByGuidAsync(string guid, bool bypassCache = false)
        {
            if(string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a student grade point average.");
            }

            //Check if user has view permissions.
            if (!await HasStudentGradePointAveragesPermissionsAsync())
            {
                // throw new PermissionsException(string.Format("User {0} does not have permission to view grade point averages.", CurrentUser.UserId));
                IntegrationApiExceptionAddError(string.Format("User '{0}' is not authorized to view grade point averages.", CurrentUser.UserId), "Authentication.Required", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
            string stGpaRecordKey = string.Empty;
            try
            {
                stGpaRecordKey = await _studentGradePointAveragesRepository.GetStudentGradePointAverageIdFromGuidAsync(guid);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
                throw IntegrationApiException;
            }

            if (string.IsNullOrEmpty(stGpaRecordKey))
            {
                throw new KeyNotFoundException(string.Format("Student grade point average not found for guid '{0}'.", guid));
            }

            StudentAcademicCredit entity = await _studentGradePointAveragesRepository.GetStudentCredProgramInfoByIdAsync(stGpaRecordKey);
            
            if(entity == null)
            {
                throw new KeyNotFoundException(string.Format("Student grade point average not found for guid '{0}'.", guid));
            }

            //Get all person id's
            var personStIds = entity.StudentId;
            Dictionary<string, string> personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { personStIds });

            //Get all id's for academic periods
            var termIdList = new List<string>();
            var termIds = entity.StudentGPAInfoList != null && entity.StudentGPAInfoList.Any() ? 
                          entity.StudentGPAInfoList.Where(t => !string.IsNullOrWhiteSpace(t.Term))
                                                   .Select(t => t.Term).Distinct() : 
                                                   new List<string>();

            var reportingTermIds = entity.StudentGPAInfoList != null && entity.StudentGPAInfoList.Any() ?
                                   entity.StudentGPAInfoList.Where(t => !string.IsNullOrWhiteSpace(t.StcReportingTerm))
                                                            .Select(t => t.StcReportingTerm).Distinct() :
                                                            new List<string>();

            termIdList.AddRange(termIds.Union(reportingTermIds).Distinct());

            var acadPeriods = (await AcademicPeriodsAsync(bypassCache)).Where(rec => termIdList.Contains(rec.Code)).ToList();

            //Get all student acad programs info based marked cred ids
            var markCredIds = entity.StudentGPAInfoList != null && entity.StudentGPAInfoList.Any() ? 
                              entity.StudentGPAInfoList.SelectMany(i => i.MarkAcadCredentials).Distinct() : 
                              new List<string>();
            IEnumerable<StudentAcademicCredentialProgramInfo> studCredPrgInfo = null;
            if (markCredIds != null || markCredIds.Any())
            {
                studCredPrgInfo = await _studentGradePointAveragesRepository.GetStudentCredProgramInfoAsync(markCredIds.Distinct());
            }

            //Get the altcum flag
            bool useAltCumFlag = await _studentGradePointAveragesRepository.UseAlternativeCumulativeValuesAsync();

            var dto = await ConvertStudentGradePointAveragesEntityToDtoAsync(entity, personIdDict, acadPeriods, studCredPrgInfo, useAltCumFlag, bypassCache);
            
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return dto;
        }

        #region Convert Methods

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personIdDict"></param>
        /// <param name="acadPeriods"></param>
        /// <param name="studCredPrgInfo"></param>
        /// <param name="useAltCumFlag"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<StudentGradePointAverages> ConvertStudentGradePointAveragesEntityToDtoAsync(StudentAcademicCredit source, Dictionary<string, string> personIdDict, 
            IEnumerable<AcademicPeriod> acadPeriods, IEnumerable<StudentAcademicCredentialProgramInfo> studCredPrgInfo, bool useAltCumFlag, bool bypassCache = false)
        {
            StudentGradePointAverages dto = new StudentGradePointAverages();
            decimal? instAttemptedCredits, instEarnedCredits, instQualityPoints, allAttemptedCredits, allEarnedCredits, allQualityPoints = 0.00m;

            dto.Id = source.PersonSTGuid;
            string personId = string.Empty;
            if (!personIdDict.TryGetValue(source.StudentId, out personId))
            {
                //throw new ArgumentNullException("student.id", "Student id is required.");
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student ID '{0}'", source.StudentId), "student.id", source.PersonSTGuid, source.StudentId);
                return dto;
            }
            dto.Student = new GuidObject2(personId);

            //Get credit type codes for "institution"
            var creditTypes = await CreditTypesAsync(bypassCache);
            var instCreditTypes = creditTypes.Where(i => !string.IsNullOrWhiteSpace(i.Category) && i.Category.Equals("I", StringComparison.OrdinalIgnoreCase))
                                             .Select(c => c.Code)
                                             .ToList();

            var termBased = source.StudentGPAInfoList.Where(r => !string.IsNullOrEmpty(r.Term) && r.StcAttCredit.HasValue)
                                                            .GroupBy(i => i.Term)                                                            
                                                            .Select(c => c).ToList();
            var reportingTermBased = source.StudentGPAInfoList.Where(r => !string.IsNullOrEmpty(r.Term) &&
                                                                          !string.IsNullOrEmpty(r.StcReportingTerm) &&
                                                                          r.Term != r.StcReportingTerm &&
                                                                          r.StcAttCredit.HasValue)
                                                              .GroupBy(i => i.StcReportingTerm)
                                                              .Select(c => c).ToList();
            var periodBasedCreds = reportingTermBased == null && !reportingTermBased.Any() ?
                                   termBased : 
                                   termBased.Union(reportingTermBased);
            var cumulativeCreds = source.StudentGPAInfoList.Where(r => !string.IsNullOrEmpty(r.AcademicLevel) && r.StcAttCredit.HasValue)
                                                           .GroupBy(i => i.AcademicLevel)
                                                           .Select(c => c).ToList();

            var earnedDegreeCreds = source.StudentGPAInfoList.Where(i => i.MarkAcadCredentials != null && i.MarkAcadCredentials.Any())
                                                             .GroupBy(i => i.MarkAcadCredentials.Distinct())
                                                             .SelectMany(cr => cr).ToList();
            //Period Based GPA's
            if (periodBasedCreds != null && periodBasedCreds.Any())
            {
                List<StudentGradePointAveragesPeriodBasedDtoProperty> periodBasedList = new List<StudentGradePointAveragesPeriodBasedDtoProperty>();
                foreach (var periodBasedCred in periodBasedCreds)
                {
                    var institutionAcadCred = periodBasedCred.Where(inst => instCreditTypes.Contains(inst.CreditType)).ToList();
                    var allAcadCred = periodBasedCred.Select(all => all);

                    ConvertEntityToGpaValues(useAltCumFlag, out instAttemptedCredits, out instEarnedCredits, out instQualityPoints, out allAttemptedCredits, out allEarnedCredits, 
                        out allQualityPoints, institutionAcadCred, allAcadCred);

                    if (instQualityPoints.HasValue && instEarnedCredits.HasValue && institutionAcadCred != null && institutionAcadCred.Any())
                    {
                        if (dto.PeriodBased == null) dto.PeriodBased = new List<StudentGradePointAveragesPeriodBasedDtoProperty>();
                        periodBasedList.Add(await ConvertEntityToPeriodBasedDtoAsync(acadPeriods, instAttemptedCredits, instEarnedCredits, instQualityPoints, 
                            "institution", periodBasedCred, institutionAcadCred, source.PersonSTGuid, source.StudentId, bypassCache));
                    }

                    if (allQualityPoints.HasValue && allEarnedCredits.HasValue && allAcadCred != null && allAcadCred.Any())
                    {
                        if (dto.PeriodBased == null) dto.PeriodBased = new List<StudentGradePointAveragesPeriodBasedDtoProperty>();
                        periodBasedList.Add(await ConvertEntityToPeriodBasedDtoAsync(acadPeriods, allAttemptedCredits, allEarnedCredits, allQualityPoints, 
                            "all", periodBasedCred, allAcadCred, source.PersonSTGuid, source.StudentId, bypassCache));
                    }
                }
                dto.PeriodBased = periodBasedList;
            }
            //Cumulative GPA's
            if(cumulativeCreds != null & cumulativeCreds.Any())
            {
                List<StudentGradePointAveragesCumulativeDtoProperty> cumulativeList = new List<StudentGradePointAveragesCumulativeDtoProperty>();
                foreach (var cumulativeCred in cumulativeCreds)
                {
                    var institutionAcadCred = cumulativeCred.Where(inst => instCreditTypes.Contains(inst.CreditType) && inst.StcAttCredit.HasValue && !string.IsNullOrEmpty(inst.StcStudentCourseSec));
                    var allAcadCred = cumulativeCred.Where(all => all.StcAttCredit.HasValue);

                    ConvertEntityToGpaValues(useAltCumFlag, out instAttemptedCredits, out instEarnedCredits, out instQualityPoints, out allAttemptedCredits, out allEarnedCredits,
                        out allQualityPoints, institutionAcadCred, allAcadCred);

                    if (instQualityPoints.HasValue && instEarnedCredits.HasValue)
                    {
                        if (dto.Cumulative == null) dto.Cumulative = new List<StudentGradePointAveragesCumulativeDtoProperty>();
                        cumulativeList.Add(await ConvertEntityToCumulativeDtoAsync(acadPeriods, instAttemptedCredits, instEarnedCredits, instQualityPoints, "institution", institutionAcadCred, source.PersonSTGuid, source.StudentId, bypassCache));
                    }
                    if (allQualityPoints.HasValue && allEarnedCredits.HasValue)
                    {
                        if (dto.Cumulative == null) dto.Cumulative = new List<StudentGradePointAveragesCumulativeDtoProperty>();
                        cumulativeList.Add(await ConvertEntityToCumulativeDtoAsync(acadPeriods, allAttemptedCredits, allEarnedCredits, allQualityPoints, "all", allAcadCred, source.PersonSTGuid, source.StudentId, bypassCache));
                    }
                }
                dto.Cumulative = cumulativeList;
            }

            if (earnedDegreeCreds != null & earnedDegreeCreds.Any())
            {
                ConvertEntityToGpaValues(useAltCumFlag, out instAttemptedCredits, out instEarnedCredits, out instQualityPoints, out allAttemptedCredits, out allEarnedCredits,
                    out allQualityPoints, earnedDegreeCreds, earnedDegreeCreds);

                List<StudentGradePointAveragesEarnedDegreesDtoProperty> earnedDegreeList = new List<StudentGradePointAveragesEarnedDegreesDtoProperty>();

                if (instQualityPoints.HasValue && instEarnedCredits.HasValue)
                {
                    if (dto.EarnedDegrees == null) dto.EarnedDegrees = new List<StudentGradePointAveragesEarnedDegreesDtoProperty>();

                    earnedDegreeList.Add(ConvertEntityToEarnedDtoAsync(acadPeriods, instAttemptedCredits, instEarnedCredits, instQualityPoints, "institution", earnedDegreeCreds, studCredPrgInfo, source.PersonSTGuid, source.StudentId, bypassCache));
                }
                if (allQualityPoints.HasValue && allEarnedCredits.HasValue)
                {
                    if (dto.EarnedDegrees == null) dto.EarnedDegrees = new List<StudentGradePointAveragesEarnedDegreesDtoProperty>();

                    earnedDegreeList.Add(ConvertEntityToEarnedDtoAsync(acadPeriods, allAttemptedCredits, allEarnedCredits, allQualityPoints, "all", earnedDegreeCreds, studCredPrgInfo, source.PersonSTGuid, source.StudentId, bypassCache));
                }
                dto.EarnedDegrees = earnedDegreeList;
            }           
            return dto;
        }

        /// <summary>
        /// Converts entity gpa values to dto.
        /// </summary>
        /// <param name="useAltCumFlag"></param>
        /// <param name="instAttemptedCredits"></param>
        /// <param name="instEarnedCredits"></param>
        /// <param name="instQualityPoints"></param>
        /// <param name="allAttemptedCredits"></param>
        /// <param name="allEarnedCredits"></param>
        /// <param name="allQualityPoints"></param>
        /// <param name="institutionAcadCred"></param>
        /// <param name="allAcadCred"></param>
        private static void ConvertEntityToGpaValues(bool useAltCumFlag, out decimal? instAttemptedCredits, out decimal? instEarnedCredits, out decimal? instQualityPoints, out decimal? allAttemptedCredits, out decimal? allEarnedCredits, out decimal? allQualityPoints, 
            IEnumerable<StudentGPAInfo> institutionAcadCred, IEnumerable<StudentGPAInfo> allAcadCred)
        {
            if (useAltCumFlag)
            {
                instAttemptedCredits = institutionAcadCred.Sum(attCr => attCr.StcAltCumContribAttCredit.HasValue ? attCr.StcAltCumContribAttCredit.Value : 0.00m);
                instEarnedCredits = institutionAcadCred.Sum(eCr => eCr.StcAltCumContribGpaCredit.HasValue ? eCr.StcAltCumContribGpaCredit.Value : 0.00m);
                instQualityPoints = institutionAcadCred.Sum(qPt => qPt.StcAltcumContribGradePoint.HasValue ? qPt.StcAltcumContribGradePoint.Value : 0.00m);
            }
            else
            {
                instAttemptedCredits = institutionAcadCred.Sum(attCr => attCr.StcCumContribAttCredit.HasValue ? attCr.StcCumContribAttCredit.Value : 0.00m);
                instEarnedCredits = institutionAcadCred.Sum(eCr => eCr.StcCumContribGpaCredit.HasValue ? eCr.StcCumContribGpaCredit.Value : 0.00m);
                instQualityPoints = institutionAcadCred.Sum(qPt => qPt.StcCumContribGradePoint.HasValue ? qPt.StcCumContribGradePoint.Value : 0.00m);
            }

            if (useAltCumFlag)
            {
                allAttemptedCredits = allAcadCred.Sum(attCr => attCr.StcAltCumContribAttCredit.HasValue ? attCr.StcAltCumContribAttCredit.Value : 0.00m);
                allEarnedCredits = allAcadCred.Sum(eCr => eCr.StcAltCumContribGpaCredit.HasValue ? eCr.StcAltCumContribGpaCredit.Value : 0.00m);
                allQualityPoints = allAcadCred.Sum(qPt => qPt.StcAltcumContribGradePoint.HasValue ? qPt.StcAltcumContribGradePoint.Value : 0.00m);
            }
            else
            {
                allAttemptedCredits = allAcadCred.Sum(attCr => attCr.StcCumContribAttCredit.HasValue ? attCr.StcCumContribAttCredit.Value : 0.00m);
                allEarnedCredits = allAcadCred.Sum(eCr => eCr.StcCumContribGpaCredit.HasValue ? eCr.StcCumContribGpaCredit.Value : 0.00m);
                allQualityPoints = allAcadCred.Sum(qPt => qPt.StcCumContribGradePoint.HasValue ? qPt.StcCumContribGradePoint.Value : 0.00m);
            }
        }

        /// <summary>
        /// Converts entity to period based gpa dto.
        /// </summary>
        /// <param name="acadPeriods"></param>
        /// <param name="attemptedCredits"></param>
        /// <param name="earnedCredits"></param>
        /// <param name="qualityPoints"></param>
        /// <param name="academicSource"></param>
        /// <param name="periodBasedCred"></param>
        /// <param name="institutionAcadCred"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<StudentGradePointAveragesPeriodBasedDtoProperty> ConvertEntityToPeriodBasedDtoAsync(IEnumerable<AcademicPeriod> acadPeriods, decimal? attemptedCredits, decimal? earnedCredits, 
            decimal? qualityPoints, string academicSource, IGrouping<string, StudentGPAInfo> periodBasedCred, IEnumerable<StudentGPAInfo> institutionAcadCred,
            string personSTGuid, string studentId, bool bypassCache)
        {
            StudentGradePointAveragesPeriodBasedDtoProperty instPeriodDto = new StudentGradePointAveragesPeriodBasedDtoProperty();
            instPeriodDto.AcademicSource = academicSource;
            instPeriodDto.AttemptedCredits = attemptedCredits.HasValue ? attemptedCredits.Value : 0.00m;
            instPeriodDto.EarnedCredits = earnedCredits.Value;
            instPeriodDto.QualityPoints = qualityPoints.Value;
            instPeriodDto.Value = earnedCredits.Value == 0m? 0.000m : decimal.Round(decimal.Divide(qualityPoints.Value, earnedCredits.Value), 3, MidpointRounding.AwayFromZero);

            //Acad Level
            var acadLevels = await AcademicLevelsAsync(bypassCache);
            if (acadLevels != null && acadLevels.Any())
            {
                var templvl = institutionAcadCred.FirstOrDefault(i => !string.IsNullOrEmpty(i.AcademicLevel));
                if (templvl != null && !string.IsNullOrEmpty(templvl.AcademicLevel))
                {
                    var acadLevel = acadLevels.FirstOrDefault(lvl => lvl.Code.Equals(templvl.AcademicLevel, StringComparison.OrdinalIgnoreCase));
                    if (acadLevel == null || string.IsNullOrEmpty(acadLevel.Guid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level not found for level '{0}'.", templvl.AcademicLevel), "academic.level.invalid", personSTGuid, studentId);
                    }
                    else
                    {
                        instPeriodDto.AcademicLevel = new GuidObject2(acadLevel.Guid);
                    }
                }
            }
            //Terms
            if (acadPeriods != null && acadPeriods.Any())
            {
                var tempTerm = acadPeriods.FirstOrDefault(t => t.Code.Equals(periodBasedCred.Key, StringComparison.OrdinalIgnoreCase));
                if (tempTerm == null || string.IsNullOrEmpty(tempTerm.Guid))
                {
                    IntegrationApiExceptionAddError(string.Format("Term not found for '{0}'.", periodBasedCred.Key), "academic.periods.invalid", personSTGuid, studentId);
                }
                else
                {
                    instPeriodDto.AcademicPeriod = new GuidObject2(tempTerm.Guid);
                }
            }
            return instPeriodDto;
        }

        /// <summary>
        /// Converts entity to Cumulative dto.
        /// </summary>
        /// <param name="acadPeriods"></param>
        /// <param name="attemptedCredits"></param>
        /// <param name="earnedCredits"></param>
        /// <param name="qualityPoints"></param>
        /// <param name="academicSource"></param>
        /// <param name="institutionAcadCred"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<StudentGradePointAveragesCumulativeDtoProperty> ConvertEntityToCumulativeDtoAsync(IEnumerable<AcademicPeriod> acadPeriods, decimal? attemptedCredits, decimal? earnedCredits,
            decimal? qualityPoints, string academicSource, IEnumerable<StudentGPAInfo> institutionAcadCred,
            string personSTGuid, string studentId, bool bypassCache)
        {
            StudentGradePointAveragesCumulativeDtoProperty cumulativeDto = new StudentGradePointAveragesCumulativeDtoProperty();
            cumulativeDto.AcademicSource = academicSource;
            cumulativeDto.AttemptedCredits = attemptedCredits.HasValue ? attemptedCredits.Value : 0.00m;
            cumulativeDto.EarnedCredits = earnedCredits.Value;
            cumulativeDto.QualityPoints = qualityPoints.Value;
            cumulativeDto.Value = earnedCredits.Value == 0m? 0.000m : decimal.Round(decimal.Divide(qualityPoints.Value, earnedCredits.Value), 3, MidpointRounding.AwayFromZero);

            //Acad Level
            var acadLevels = await AcademicLevelsAsync(bypassCache);
            if (acadLevels != null && acadLevels.Any())
            {
                var templvl = institutionAcadCred.FirstOrDefault(i => !string.IsNullOrEmpty(i.AcademicLevel));
                if (templvl != null && !string.IsNullOrEmpty(templvl.AcademicLevel))
                {
                    var acadLevel = acadLevels.FirstOrDefault(lvl => lvl.Code.Equals(templvl.AcademicLevel, StringComparison.OrdinalIgnoreCase));
                    if (acadLevel == null || string.IsNullOrEmpty(acadLevel.Guid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level not found for level '{0}'.", templvl.AcademicLevel), "academic.level.invalid", personSTGuid, studentId);
                    }
                    else
                    {
                        cumulativeDto.AcademicLevel = new GuidObject2(acadLevel.Guid);
                    }
                }
            }
            return cumulativeDto;
        }

        /// <summary>
        /// Converts entity to earned degrees dto.
        /// </summary>
        /// <param name="acadPeriods"></param>
        /// <param name="attemptedCredits"></param>
        /// <param name="earnedCredits"></param>
        /// <param name="qualityPoints"></param>
        /// <param name="academicSource"></param>
        /// <param name="institutionAcadCred"></param>
        /// <param name="studCredPrgInfo"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private StudentGradePointAveragesEarnedDegreesDtoProperty ConvertEntityToEarnedDtoAsync(IEnumerable<AcademicPeriod> acadPeriods, decimal? attemptedCredits, decimal? earnedCredits,
            decimal? qualityPoints, string academicSource, IEnumerable<StudentGPAInfo> institutionAcadCred, IEnumerable<StudentAcademicCredentialProgramInfo> studCredPrgInfo,
            string personSTGuid, string StudentId, bool bypassCache)
        {
            StudentGradePointAveragesEarnedDegreesDtoProperty earnedDegreeDto = new StudentGradePointAveragesEarnedDegreesDtoProperty();
            earnedDegreeDto.AcademicSource = academicSource;
            earnedDegreeDto.AttemptedCredits = attemptedCredits.HasValue ? attemptedCredits.Value : 0.00m;
            earnedDegreeDto.EarnedCredits = earnedCredits.Value;
            earnedDegreeDto.QualityPoints = qualityPoints.Value;
            earnedDegreeDto.Value = earnedCredits.Value == 0m ? 0.000m : decimal.Round(decimal.Divide(qualityPoints.Value, earnedCredits.Value), 3, MidpointRounding.AwayFromZero);

            //Acad Credentials
            var id = institutionAcadCred.SelectMany(i => i.MarkAcadCredentials).Distinct().FirstOrDefault();

            var studAcadCredPrgInfo = studCredPrgInfo.FirstOrDefault(i => i.AcademicCredentialsId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (studAcadCredPrgInfo == null)
            {
                IntegrationApiExceptionAddError(string.Format("Academic Credit is marked as graduated but the associated academic credentials '{0}' is missing.", id), "acad.credentials.missing", personSTGuid, StudentId);
            }
            else
            {

                earnedDegreeDto.AcademicProgram = new GuidObject2(studAcadCredPrgInfo.StudentProgramGuid);
                if (!studAcadCredPrgInfo.AcadDegreeDate.HasValue && !studAcadCredPrgInfo.AcadCcdDate.HasValue)
                {
                    IntegrationApiExceptionAddError(string.Format("Earned on date is required for academic credentials student program '{0}' for person {1}.", studAcadCredPrgInfo.StudentProgramGuid,
                        studAcadCredPrgInfo.AcadPersonId), "earnedDegrees.earnedOn", personSTGuid, StudentId);
                }
                else
                {
                    earnedDegreeDto.EarnedOn = studAcadCredPrgInfo.AcadDegreeDate.HasValue ? studAcadCredPrgInfo.AcadDegreeDate.Value : studAcadCredPrgInfo.AcadCcdDate.Value;
                }
            }

            return earnedDegreeDto;
        }

        /// <summary>
        /// Converts to date string.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _studentGradePointAveragesRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get all terms
        /// </summary>
        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> TermsAsync(bool bypassCache = false)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        /// <summary>
        /// Get all academic periods
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> AcademicPeriodsAsync(bool bypassCache = false)
        {
            if (_academicPeriods == null)
            {
                _academicPeriods =  _termRepository.GetAcademicPeriods(await TermsAsync(bypassCache));
            }
            return _academicPeriods;
        }

        /// <summary>
        /// Get all academic periods
        /// </summary>
        private IEnumerable<AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<AcademicLevel>> AcademicLevelsAsync(bool bypassCache = false)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _referenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        /// <summary>
        /// Get all credit types
        /// </summary>
        private IEnumerable<CreditCategory> _creditTypes = null;

        private async Task<IEnumerable<CreditCategory>> CreditTypesAsync(bool bypassCache = false)
        {
            if (_creditTypes == null)
            {
                _creditTypes = await _referenceDataRepository.GetCreditCategoriesAsync(bypassCache);
            }
            return _creditTypes;
        }

        /// <summary>
        ///  Get default organization id from PID2
        /// </summary>
        /// <returns>default institution id</returns>
        private string _defaultInstitutionId = string.Empty;

        private string GetDefaultInstitutionId()
        {
            if (!string.IsNullOrEmpty(_defaultInstitutionId)) return _defaultInstitutionId;
            var hostId = string.Empty;
            var defaultsConfiguration = _configurationRepository.GetDefaultsConfiguration();
            if (defaultsConfiguration != null)
            {
                hostId = defaultsConfiguration.HostInstitutionCodeId;

            }
            if (string.IsNullOrEmpty(hostId))
                throw new KeyNotFoundException("Unable to determine default institution from PID2.");
            _defaultInstitutionId = hostId;

            return _defaultInstitutionId;
        }
        #endregion

        #region Permission Check
        /// <summary>
        /// Checks to see if the user has permissions
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HasStudentGradePointAveragesPermissionsAsync()
        {
            //VIEW.STUDENT.GRADE.POINT.AVERAGES
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.ViewStudentGradePointAverages))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}