//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentTranscriptGradesOptionsService : BaseCoordinationService, IStudentTranscriptGradesOptionsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;   
        private readonly IGradeRepository _gradeRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentTranscriptGradesOptionsRepository _StudentTranscriptGradesOptionsRepository;

        public StudentTranscriptGradesOptionsService(
            IStudentTranscriptGradesOptionsRepository StudentTranscriptGradesOptionsRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IPersonRepository personRepository,
            IGradeRepository gradeRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _StudentTranscriptGradesOptionsRepository = StudentTranscriptGradesOptionsRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;           
            _personRepository = personRepository;
            _gradeRepository = gradeRepository;
        }

        #region GET Methods

        /// <summary>
        /// Gets all StudentTranscriptGrade
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentTranscriptGradesOptions">StudentTranscriptGrade</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions>, int>> GetStudentTranscriptGradesOptionsAsync(int offset,
            int limit, Dtos.Filters.StudentFilter studentFilter, bool bypassCache = false)
        {
         
            string studentGuid = string.Empty;
            if (studentFilter != null)
            {
                studentGuid = studentFilter.Student != null ? studentFilter.Student.Id : string.Empty;
            }

            string studentId = string.Empty;
            if (!string.IsNullOrEmpty(studentGuid))
            {
                try
                {
                    studentId = await _personRepository.GetPersonIdFromGuidAsync(studentGuid);
                    if (string.IsNullOrEmpty(studentId))
                        return new Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>(new List<Dtos.StudentTranscriptGradesOptions>(), 0);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>(new List<Dtos.StudentTranscriptGradesOptions>(), 0);
                }

            }

            var StudentTranscriptGradesOptions = new List<Dtos.StudentTranscriptGradesOptions>();

            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentTranscriptGradesOptions>, int> StudentTranscriptGradesOptionsEntities = null;

            try
            {
                StudentTranscriptGradesOptionsEntities = await _StudentTranscriptGradesOptionsRepository.GetStudentTranscriptGradesOptionsAsync(offset, limit, studentId, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (StudentTranscriptGradesOptionsEntities != null && StudentTranscriptGradesOptionsEntities.Item1.Any())
            {
                StudentTranscriptGradesOptions = (await BuildStudentTranscriptGradesOptionsDtoAsync(StudentTranscriptGradesOptionsEntities.Item1, bypassCache)).ToList();
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return StudentTranscriptGradesOptions.Any() ? new Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>(StudentTranscriptGradesOptions, StudentTranscriptGradesOptionsEntities.Item2) :
                new Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>(new List<Dtos.StudentTranscriptGradesOptions>(), 0);
        }

        /// <summary>
        /// Get a StudentTranscriptGrade by guid.
        /// </summary>
        /// <param name="guid">Guid of the StudentTranscriptGrade in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGradesOptions">StudentTranscriptGrade</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a student transcript grade.");
            }
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.StudentTranscriptGradesOptions StudentTranscriptGradesOptionsEntity = null;

                try
                {
                    StudentTranscriptGradesOptionsEntity = await _StudentTranscriptGradesOptionsRepository.GetStudentTranscriptGradesOptionsByGuidAsync(guid);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, guid: guid);
                    throw IntegrationApiException;
                }

                IEnumerable<Dtos.StudentTranscriptGradesOptions> StudentTranscriptGradesOptions = null;

                StudentTranscriptGradesOptions = (await BuildStudentTranscriptGradesOptionsDtoAsync(new List<Domain.Student.Entities.StudentTranscriptGradesOptions>()
                { StudentTranscriptGradesOptionsEntity }, bypassCache));

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return StudentTranscriptGradesOptions != null ? StudentTranscriptGradesOptions.FirstOrDefault() : null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        private async Task<IEnumerable<Dtos.StudentTranscriptGradesOptions>> BuildStudentTranscriptGradesOptionsDtoAsync(IEnumerable<Domain.Student.Entities.StudentTranscriptGradesOptions> sources, 
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var studentTranscriptGradeOptions = new List<Dtos.StudentTranscriptGradesOptions>();            

            foreach (var source in sources)
            {
                
                studentTranscriptGradeOptions.Add(await ConvertStudentTranscriptGradesOptionsEntityToDtoAsync(source, bypassCache));

            }
            return studentTranscriptGradeOptions;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTranscriptGradesOptions domain entity to its corresponding StudentTranscriptGradesOptions DTO
        /// </summary>
        /// <param name="source">StudentTranscriptGradesOptions domain entity</param>
        /// <returns>StudentTranscriptGradesOptions DTO</returns>
                private async Task<StudentTranscriptGradesOptions> ConvertStudentTranscriptGradesOptionsEntityToDtoAsync(Domain.Student.Entities.StudentTranscriptGradesOptions source,
            bool bypassCache = false)
        {                       
            var studentTranscriptGradeOptions = new Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions();
            studentTranscriptGradeOptions.Id = source.Guid;            
            studentTranscriptGradeOptions.GradeScheme = await ConvertEntityToGradeSchemeObjectAsync(source, bypassCache);
            studentTranscriptGradeOptions.Grades = await ConvertEntityToGradesObjectAsync(source, bypassCache);
            return studentTranscriptGradeOptions;
        }

        #region Convert Methods

        private async Task<StudentTranscriptGradesOptionsGradeSchemeDtoProperty> ConvertEntityToGradeSchemeObjectAsync(Domain.Student.Entities.StudentTranscriptGradesOptions source,
            bool bypassCache = false)
        {
            if (source != null && string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                return null;
            }
            var gradeSchemeObject = new StudentTranscriptGradesOptionsGradeSchemeDtoProperty();
            var entity = await this.GetGradeSchemesAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                //throw new InvalidOperationException("Grade schemes are not defined.");
                IntegrationApiExceptionAddError("Grade schemes are not defined.", "Bad.Data", source.Id, source.Guid);
            }
            else
            {
                var gradeSchemes = entity.FirstOrDefault(i => i.Code.Equals(source.GradeSchemeCode, StringComparison.OrdinalIgnoreCase));
                if (gradeSchemes == null)
                {
                    //throw new KeyNotFoundException(string.Format("Grade Scheme not found for key: {0}. Guid: {1}", source.GradeSchemeCode, source.Guid));
                    IntegrationApiExceptionAddError(string.Format("Grade Scheme not found for key: '{0}'.", source.GradeSchemeCode), "Record.Not.Found", source.Id, source.Guid);
                }
                else
                {
                    gradeSchemeObject.Detail = new GuidObject2(gradeSchemes.Guid);
                    gradeSchemeObject.Title = gradeSchemes.Description;
                }
            }
            
            return gradeSchemeObject;
        }

        private async Task<List<StudentTranscriptGradesOptionsGradesDtoProperty>> ConvertEntityToGradesObjectAsync(Domain.Student.Entities.StudentTranscriptGradesOptions source,
            bool bypassCache = false)
        {
            if (source != null && string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                return null;
            }
            var gradesCollection = new List<StudentTranscriptGradesOptionsGradesDtoProperty>();

            var entity = await this.GetGradeDefinitionsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                //throw new InvalidOperationException("Grades are not defined.");
                IntegrationApiExceptionAddError("Grades are not defined.", "Bad.Data", source.Id, source.Guid);
            }
            else
            {
                var grades = entity.Where(i => i.GradeSchemeCode.Equals(source.GradeSchemeCode, StringComparison.OrdinalIgnoreCase));
                if (grades == null || !grades.Any())
                {
                    //throw new KeyNotFoundException(string.Format("Grades not found for grade scheme: {0}. Guid: {1}", source.GradeSchemeCode, source.Guid));
                    IntegrationApiExceptionAddError(string.Format("Grades not found for grade scheme: '{0}'.", source.GradeSchemeCode), "Record.Not.Found", source.Id, source.Guid);
                }
                else
                {
                    foreach (var grade in grades)
                    {
                        var gradesObject = new StudentTranscriptGradesOptionsGradesDtoProperty();
                        if (!string.IsNullOrEmpty(grade.Guid))
                        {
                            gradesObject.Grade = new GuidObject2(grade.Guid);
                            gradesObject.Value = grade.LetterGrade;
                            gradesCollection.Add(gradesObject);
                            if (!string.IsNullOrEmpty(grade.IncompleteGrade))
                            {
                                var incompleteGrade = entity.FirstOrDefault(i => i.Id.Equals(grade.IncompleteGrade));
                                if (incompleteGrade != null && !string.IsNullOrEmpty(incompleteGrade.Guid))
                                {
                                    gradesObject.IncompleteGrade = new StudentTranscriptGradesIncompleteGrade()
                                    {
                                        FinalGrade = new GuidObject2(incompleteGrade.Guid)
                                    };
                                }
                            }
                        }
                    }
                }
            }
            return gradesCollection;
        }        

        #endregion

        #region All Reference Data

        /// <summary>
        /// Grade definitions
        /// </summary>
        IEnumerable<Domain.Student.Entities.Grade> _gradeDefinitions = null;
        private async Task<IEnumerable<Domain.Student.Entities.Grade>> GetGradeDefinitionsAsync(bool bypassCache)
        {
            return _gradeDefinitions ?? (_gradeDefinitions = await _gradeRepository.GetHedmAsync(bypassCache));
        }
                
        /// <summary>
        /// Grade schemes
        /// </summary>
        IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool bypassCache)
        {
            return _gradeSchemes ?? (_gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache));
        }


        #endregion

   
    }
}