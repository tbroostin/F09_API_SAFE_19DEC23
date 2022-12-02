//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Student.Services
{

    [RegisterType]
    public class StudentAcademicStandingsService : StudentCoordinationService, IStudentAcademicStandingsService
    {
        private readonly IStudentStandingRepository _studentStandingRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;      
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentAcademicStandingsService(
            IPersonRepository personRepository,
            ITermRepository termRepository,
            IStudentStandingRepository studentStandingRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger, IStudentRepository studentRepository,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _studentStandingRepository = studentStandingRepository;
            _personRepository = personRepository;
            _termRepository = termRepository;
        }


        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }


        private IEnumerable<Domain.Student.Entities.Term> _terms = null;

        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }


        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcademicProgramsAsync(bool bypassCache)
        {
            if (_academicPrograms == null)
            {
                _academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _academicPrograms;
        }


        private IEnumerable<Domain.Student.Entities.AcademicStanding2> _academicStandings = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicStanding2>> GetAcademicStandingAsync(bool bypassCache)
        {
            if (_academicStandings == null)
            {
                _academicStandings = await _studentReferenceDataRepository.GetAcademicStandings2Async(bypassCache);
            }
            return _academicStandings;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get StudentAcademicStandings data.
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>List of <see cref="Dtos.StudentAcademicStandings">StudentAcademicStandings</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentAcademicStandings>, int>> GetStudentAcademicStandingsAsync(int offset, int limit, bool bypassCache = false)
        {
            var studentAcademicStandingsCollection = new List<Ellucian.Colleague.Dtos.StudentAcademicStandings>();
            int totalRecords = 0;
            var studentAcademicStandingsEntities = await _studentStandingRepository.GetStudentStandingsAsync(offset, limit);

            if (studentAcademicStandingsEntities != null && studentAcademicStandingsEntities.Item1 != null && studentAcademicStandingsEntities.Item1.Any())
            {
                totalRecords = studentAcademicStandingsEntities.Item2;
                var personIds = studentAcademicStandingsEntities.Item1.Select(sas => sas.StudentId);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                foreach (var studentAcademicStandingEntity in studentAcademicStandingsEntities.Item1)
                {
                    if (studentAcademicStandingEntity.Guid != null)
                    {
                        var studentAcademicStandingDto = await this.ConvertStudentAcademicStandingsEntityToDtoAsync(studentAcademicStandingEntity, personGuidCollection, bypassCache);
                        studentAcademicStandingsCollection.Add(studentAcademicStandingDto);
                    }
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.StudentAcademicStandings>, int>(studentAcademicStandingsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get StudentAcademicStandings data from a Guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.StudentAcademicStandings">StudentAcademicStandings</see></returns>
        public async Task<Dtos.StudentAcademicStandings> GetStudentAcademicStandingsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an Student Academic Standing.");
            }

            StudentAcademicStandings studentAcademicStandingDto = new StudentAcademicStandings();
            try
            {
                var studentAcademicStandingEntity = await _studentStandingRepository.GetStudentStandingByGuidAsync(guid);
                var personIds = new List<string>() { studentAcademicStandingEntity.StudentId };
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                studentAcademicStandingDto = await ConvertStudentAcademicStandingsEntityToDtoAsync(studentAcademicStandingEntity, personGuidCollection);
            }
            catch (KeyNotFoundException)
            {
                IntegrationApiExceptionAddError("No Student Academic Standings was found for guid " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
                //throw new KeyNotFoundException("No Student Academic Standings was found for guid  " + guid, ex);
            }
            catch (InvalidOperationException)
            {
                IntegrationApiExceptionAddError("No Student Academic Standings was found for guid " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
                //throw new InvalidOperationException("No Student Academic Standings was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                //throw new RepositoryException("No Student Academic Standings was found for guid " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError("No Student Academic Standings was found for guid " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
                //throw new ColleagueWebApiException("No Student Academic Standings was found for guid  " + guid, ex);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return studentAcademicStandingDto;
        }

        /// <summary>
        /// Converts a StudentAcademicStandings domain entity to its corresponding StudentAcademicStandings DTO
        /// </summary>
        /// <param name="source">StudentAcademicStandings domain entity</param>
        /// <returns>StudentAcademicStandings DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAcademicStandings> ConvertStudentAcademicStandingsEntityToDtoAsync(StudentStanding source, Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            var studentAcademicStandings = new Ellucian.Colleague.Dtos.StudentAcademicStandings();

            studentAcademicStandings.Id = source.Guid;
            if (!string.IsNullOrEmpty(source.Term))
            {
                var terms = await GetTermsAsync(bypassCache);
                if (terms != null)
                {
                    var term = terms.FirstOrDefault(t => t.Code == source.Term);
                    if (term != null && !string.IsNullOrEmpty(term.RecordGuid))
                    {
                        studentAcademicStandings.AcademicPeriod = new GuidObject2(term.RecordGuid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the AcademicPeriod '{0}'", source.Term), "Bad.Data", source.Guid, source.Id);
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.Level))
            {
                var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                if (academicLevels != null)
                {
                    var academicLevel = academicLevels.FirstOrDefault(al => al.Code == source.Level);
                    if (academicLevel != null && !string.IsNullOrEmpty(academicLevel.Guid))
                    {
                        studentAcademicStandings.Level = new GuidObject2(academicLevel.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the AcademicLevel '{0}'", source.Level), "Bad.Data", source.Guid, source.Id);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(source.OverrideReason))
            {
                studentAcademicStandings.OverrideReason = source.OverrideReason;
            }

            if (!string.IsNullOrEmpty(source.Program))
            {
                var academicPrograms = await GetAcademicProgramsAsync(bypassCache);
                if (academicPrograms != null)
                {
                    var academicStanding = academicPrograms.FirstOrDefault(ap => ap.Code == source.Program);
                    if (academicStanding != null && !string.IsNullOrEmpty(academicStanding.Guid))
                    {
                        studentAcademicStandings.Program = new GuidObject2(academicStanding.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the AcademicProgram '{0}'", source.Program), "Bad.Data", source.Guid, source.Id);
                    }
                }
            }
            var overrideStanding = ((!string.IsNullOrEmpty(source.CalcStandingCode)) && (!string.IsNullOrEmpty(source.StandingCode)))
                ? source.StandingCode : string.Empty;
            if (!string.IsNullOrEmpty(overrideStanding))
            {
                var academicStandings = await GetAcademicStandingAsync(bypassCache);
                if (academicStandings != null)
                {
                    var academicStanding = academicStandings.FirstOrDefault(ast => ast.Code == overrideStanding);
                    if (academicStanding != null && !string.IsNullOrEmpty(academicStanding.Guid))
                    {
                         studentAcademicStandings.OverrideStanding = new GuidObject2(academicStanding.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the AcademicStanding '{0}'", overrideStanding), "Bad.Data", source.Guid, source.Id);
                    }
                }
            }

            var standing = (!string.IsNullOrEmpty(source.CalcStandingCode)) ? source.CalcStandingCode : source.StandingCode;
            if (!string.IsNullOrEmpty(standing))
            {
                var academicStandings = await GetAcademicStandingAsync(bypassCache);
                if (academicStandings != null)
                {
                    var academicStanding = academicStandings.FirstOrDefault(ast => ast.Code == standing);
                    if (academicStanding != null && !string.IsNullOrEmpty(academicStanding.Guid))
                    {
                        studentAcademicStandings.Standing = new GuidObject2(academicStanding.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the AcademicStanding '{0}'", standing), "Bad.Data", source.Guid, source.Id);
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.StudentId))
            {
                try
                {
                    var personGuid = string.Empty;
                    if (personGuidCollection.TryGetValue(source.StudentId, out personGuid))
                    {
                        studentAcademicStandings.Student = new GuidObject2(personGuid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the person '{0}'", source.StudentId), "Bad.Data", source.Guid, source.Id);
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the person '{0}'", source.StudentId), "Bad.Data", source.Guid, source.Id);
                }
            }
            studentAcademicStandings.Type = ConvertStudentStandingsTypeDomainEnumToStudentAcademicStandingsTypeDtoEnum(source.Type);

            return studentAcademicStandings;
        }

        /// <summary>
        /// Converts a StudentAcademicStandingsType domain enumeration value to its corresponding StudentAcademicStandingsType DTO enumeration value
        /// </summary>
        /// <param name="source">StudentAcademicStandingsType domain enumeration value</param>
        /// <returns>StudentAcademicStandingsType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.StudentAcademicStandingsType ConvertStudentStandingsTypeDomainEnumToStudentAcademicStandingsTypeDtoEnum(StudentStandingType source)
        {
            switch (source)
            {
                case StudentStandingType.AcademicLevel:
                    return Dtos.EnumProperties.StudentAcademicStandingsType.Level;
                case StudentStandingType.Program:
                    return Dtos.EnumProperties.StudentAcademicStandingsType.Program;
                case StudentStandingType.Term:
                    return Dtos.EnumProperties.StudentAcademicStandingsType.Academicperiod;
                default:
                    return Dtos.EnumProperties.StudentAcademicStandingsType.Level;
            }
        }
    }
}