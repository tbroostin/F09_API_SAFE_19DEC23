//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
            CheckViewStudentAcadStandingsPermission();

            var studentAcademicStandingsCollection = new List<Ellucian.Colleague.Dtos.StudentAcademicStandings>();

            var studentAcademicStandingsEntities = await _studentStandingRepository.GetStudentStandingsAsync(offset, limit);
            var totalRecords = studentAcademicStandingsEntities.Item2;

            foreach (var studentAcademicStandingEntity in studentAcademicStandingsEntities.Item1)
            {
                if (studentAcademicStandingEntity.Guid != null)
                {
                    var studentAcademicStandingDto = await this.ConvertStudentAcademicStandingsEntityToDtoAsync(studentAcademicStandingEntity, bypassCache);
                    studentAcademicStandingsCollection.Add(studentAcademicStandingDto);
                }
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
            CheckViewStudentAcadStandingsPermission();

            try
            {
                return await ConvertStudentAcademicStandingsEntityToDtoAsync(await _studentStandingRepository.GetStudentStandingByGuidAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No Student Academic Standings was found for guid  " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No Student Academic Standings was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException("No Student Academic Standings was found for guid  " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("No Student Academic Standings was found for guid  " + guid, ex);
            }
        }

        /// <summary>
        /// Converts a StudentAcademicStandings domain entity to its corresponding StudentAcademicStandings DTO
        /// </summary>
        /// <param name="source">StudentAcademicStandings domain entity</param>
        /// <returns>StudentAcademicStandings DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAcademicStandings> ConvertStudentAcademicStandingsEntityToDtoAsync(StudentStanding source, bool bypassCache = false)
        {
            var studentAcademicStandings = new Ellucian.Colleague.Dtos.StudentAcademicStandings();

            studentAcademicStandings.Id = source.Guid;
            if (!string.IsNullOrEmpty(source.Term))
            {
                var terms = await GetTermsAsync(bypassCache);
                if (terms != null)
                {
                    var term = terms.FirstOrDefault(t => t.Code == source.Term);
                    if (term != null)
                    {
                        studentAcademicStandings.AcademicPeriod = new GuidObject2(term.RecordGuid);
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.Level))
            {
                var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                if (academicLevels != null)
                {
                    var academicLevel = academicLevels.FirstOrDefault(al => al.Code == source.Level);
                    if (academicLevel != null)
                    {
                        studentAcademicStandings.Level = new GuidObject2(academicLevel.Guid);
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
                    if (academicStanding != null)
                    {
                        studentAcademicStandings.Program = new GuidObject2(academicStanding.Guid);
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
                    if (academicStanding != null)
                    {
                         studentAcademicStandings.OverrideStanding = new GuidObject2(academicStanding.Guid);
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
                    if (academicStanding != null)
                    {
                        studentAcademicStandings.Standing = new GuidObject2(academicStanding.Guid);
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.StudentId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.StudentId);
                if (!string.IsNullOrEmpty(personGuid))
                {
                    studentAcademicStandings.Student = new GuidObject2(personGuid);
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

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. 
        /// This API will integrate information related to students that could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentAcadStandingsPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewStudentAcadStandings);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Student Academic Standings.");
            }
        }

    }
}