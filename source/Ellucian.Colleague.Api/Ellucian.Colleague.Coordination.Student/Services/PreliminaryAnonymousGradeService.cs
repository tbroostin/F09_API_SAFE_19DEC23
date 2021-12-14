// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.AnonymousGrading;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for actions related to course section preliminary anonymous grading
    /// </summary>
    [RegisterType]
    public class PreliminaryAnonymousGradeService : StudentCoordinationService, IPreliminaryAnonymousGradeService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly IPreliminaryAnonymousGradeRepository _preliminaryAnonymousGradeRepository;

        /// <summary>
        /// Creates a new <see cref="PreliminaryAnonymousGradeService"/>
        /// </summary>
        /// <param name="adapterRegistry">Interface to adapter registry</param>
        /// <param name="currentUserFactory">Interface to current user factory</param>
        /// <param name="roleRepository">Interface to role repository</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="studentRepository">Interface to student repository</param>
        /// <param name="configurationRepository">Interface to configuration repository</param>
        /// <param name="sectionRepository">Interface to section repository</param>
        /// <param name="studentConfigurationRepository">Interface to student configuration repository</param>
        /// <param name="preliminaryAnonymousGradeRepository">Interface to preliminary anonymous grade repository</param>
        public PreliminaryAnonymousGradeService(IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, 
            IStudentRepository studentRepository, IConfigurationRepository configurationRepository, ISectionRepository sectionRepository,
            IStudentConfigurationRepository studentConfigurationRepository, IPreliminaryAnonymousGradeRepository preliminaryAnonymousGradeRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _sectionRepository = sectionRepository;
            _studentConfigurationRepository = studentConfigurationRepository;
            _preliminaryAnonymousGradeRepository = preliminaryAnonymousGradeRepository;
        }

        /// <summary>
        /// Get preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to retrieve preliminary anonymous grade information</param>
        /// <returns>Preliminary anonymous grade information for the specified course section</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when retrieving preliminary anonymous grade information.</exception>
        public async Task<SectionPreliminaryAnonymousGrading> GetPreliminaryAnonymousGradesBySectionIdAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A course section ID is required when retrieving preliminary anonymous grade information.");
            }

            // Verify that academic record configuration is valid
            await VerifyAcademicRecordConfigurationAsync();

            // Retrieve non-cached course section information for further validations
            var courseSection = await RetrieveCourseSectionInformation(sectionId);

            // The authenticated user is only permitted to retrieve preliminary anonymous grades for a course section if they are an assigned faculty member for the course section
            VerifyAuthenticatedUserIsAssignedFacultyForSectionAsync(courseSection);

            // The specified course section must be set up for grading by random ID
            VerifyCourseSectionUsesGradingByRandomIdAsync(courseSection);

            // Initialize the collection of data to return
            SectionPreliminaryAnonymousGrading sectionPreliminaryAnonymousGradingDto = new SectionPreliminaryAnonymousGrading();

            // Build a list of course sections to retrieve preliminary anonymous grading for;
            // this includes data for the specified course section and could also include crosslisted sections' data
            List<string> crosslistedSectionIds = await BuildListOfCrosslistedSectionsToRetrievePreliminaryAnonymousGradesAsync(courseSection);

            // Verify that any crosslisted course sections are set up for grading by random ID;
            // only cross-listed sections that use grading by random ID will be used. Any others will be logged and omitted from the results.
            List<string> crossListedSectionsToProcess = new List<string>();
            foreach(var crosslistedSectionId in crosslistedSectionIds)
            {
                try 
                {
                    var crosslistedSection = await RetrieveCourseSectionInformation(crosslistedSectionId);
                    VerifyCourseSectionUsesGradingByRandomIdAsync(crosslistedSection);
                    crossListedSectionsToProcess.Add(crosslistedSectionId);
                }
                catch (Exception ex)
                {
                    logger.Debug(string.Format("Course section {0} is crosslisted with course section {1}: " + Environment.NewLine + "" +
                        "{2}" + Environment.NewLine +
                        "Course section {1} preliminary anonymous grade information will not be included.",
                        sectionId,
                        crosslistedSectionId,
                        ex.Message));
                }
            }

            // Retrieve preliminary anonymous grade information
            var sectionPreliminaryAnonymousGradingEntity = await _preliminaryAnonymousGradeRepository.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, crossListedSectionsToProcess);

            // Convert entity to DTO
            var sectionPreliminaryAnonymousGradingAdapter = new SectionPreliminaryAnonymousGradingAdapter(_adapterRegistry, logger);
            sectionPreliminaryAnonymousGradingDto = sectionPreliminaryAnonymousGradingAdapter.MapToType(sectionPreliminaryAnonymousGradingEntity);

            // Return DTO collection
            return sectionPreliminaryAnonymousGradingDto;
        }

        /// <summary>
        /// Update preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to process preliminary anonymous grade updates</param>
        /// <param name="preliminaryAnonymousGrades">Preliminary anonymous grade updates to process</param>
        /// <returns>Preliminary anonymous grade update results</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when updating preliminary anonymous grade information.</exception>
        /// <exception cref="ArgumentNullException">At least one preliminary anonymous grade is required when updating preliminary anonymous grade information.</exception>
        public async Task<IEnumerable<PreliminaryAnonymousGradeUpdateResult>> UpdatePreliminaryAnonymousGradesBySectionIdAsync(string sectionId, 
            IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A course section ID is required when updating preliminary anonymous grade information.");
            }
            if (preliminaryAnonymousGrades == null || !preliminaryAnonymousGrades.Any())
            {
                throw new ArgumentNullException("preliminaryAnonymousGrades", "At least one grade is required when updating preliminary anonymous grade information.");
            }

            // Verify that academic record configuration is valid
            await VerifyAcademicRecordConfigurationAsync();

            // Retrieve non-cached course section information for further validations
            var courseSection = await RetrieveCourseSectionInformation(sectionId);

            // The authenticated user is only permitted to update preliminary anonymous grades for a course section if they are an assigned faculty member for the course section
            VerifyAuthenticatedUserIsAssignedFacultyForSectionAsync(courseSection);

            // The specified course section must be set up for grading by random ID
            VerifyCourseSectionUsesGradingByRandomIdAsync(courseSection);

            // Convert inbound preliminary anonymous grades to update
            var adapter = new PreliminaryAnonymousGradeDtoToEntityAdapter(_adapterRegistry, logger);
            var entities = new List<Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade>();
            foreach(var dto in preliminaryAnonymousGrades)
            {
                try
                {
                    entities.Add(adapter.MapToType(dto));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occurred while converting one of the provided preliminary anonymous grade DTOs to a preliminary anonymous grade entity.");
                    throw;
                }
            }

            // Process the attempted updates
            var updateResultEntities = await _preliminaryAnonymousGradeRepository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, entities);

            // Build the result DTOs
            var dtos = new List<PreliminaryAnonymousGradeUpdateResult>();
            var dtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult, Ellucian.Colleague.Dtos.Student.AnonymousGrading.PreliminaryAnonymousGradeUpdateResult>();
            foreach (var entity in updateResultEntities)
            {
                dtos.Add(dtoAdapter.MapToType(entity));
            }

            // Return the results
            return dtos;
        }


        /// <summary>
        /// Retrieves course section information for the specified course section ID
        /// </summary>
        /// <param name="sectionId">ID of the course section whose data will be retrieved</param>
        /// <returns>Course section information for the specified course section ID</returns>
        /// <exception cref="KeyNotFoundException">Course section data could not be retrieved.</exception>
        private async Task<Domain.Student.Entities.Section> RetrieveCourseSectionInformation(string sectionId)
        {
            var courseSections = await _sectionRepository.GetSectionAsync(sectionId);
            if (courseSections == null)
            {
                throw new KeyNotFoundException(string.Format("Could not retrieve course section information for course section {0}.", sectionId));
            }
            return courseSections;
        }

        /// <summary>
        /// Verifies that the authenticated user is an assigned faculty member for the specified course section
        /// </summary>
        /// <param name="courseSection">Course section to check</param>
        /// <exception cref="PermissionsException">Authenticated user is not an assigned faculty member for course section.</exception>
        private void VerifyAuthenticatedUserIsAssignedFacultyForSectionAsync(Domain.Student.Entities.Section courseSection)
        {
            // The current user must be a faculty member of the section
            if (courseSection.FacultyIds == null || !courseSection.FacultyIds.Contains(CurrentUser.PersonId))
            {
                throw new PermissionsException(string.Format("Authenticated user is not an assigned faculty member for course section {0}.", courseSection.Id));
            }
        }

        /// <summary>
        /// Determine if preliminary anonymous grades data for a course section should include data for its crosslisted course section(s) 
        /// based on grading configuration settings and, if so, identify those crosslisted course sections' IDs.
        /// </summary>
        /// <param name="courseSection">Course section to check</param>
        /// <returns>List of crosslisted course section IDs for the specified course section (when appropriate)</returns>
        private async Task<List<string>> BuildListOfCrosslistedSectionsToRetrievePreliminaryAnonymousGradesAsync(Domain.Student.Entities.Section courseSection)
        {
            var gradingConfiguration = await _studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
            List<string> crossListedCourseSectionIds = new List<string>();
            if (gradingConfiguration != null && gradingConfiguration.IncludeCrosslistedStudents)
            {
                if (courseSection.CrossListedSections != null)
                {
                    var crossListedSectionIds = courseSection.CrossListedSections.Select(x => x.Id);
                    if (crossListedSectionIds.Any())
                    {
                        logger.Debug(string.Format("Grading configuration specifies that cross-listed sections are included for grading. Preliminary anonymous grade data for course section {0} will include data for the following crosslisted sections: {1}",
                            courseSection.Id,
                            string.Join(",", crossListedSectionIds)));

                        crossListedCourseSectionIds.AddRange(crossListedSectionIds);
                        crossListedCourseSectionIds = crossListedCourseSectionIds.Distinct().ToList();
                    }
                }
            }
            return crossListedCourseSectionIds;
        }

        /// <summary>
        /// Verifies that the specified course section uses grading by random ID
        /// </summary>
        /// <param name="courseSection">Course section to check</param>
        /// <exception cref="ConfigurationException">Course section does not use grading by random ID.</exception>
        private void VerifyCourseSectionUsesGradingByRandomIdAsync(Domain.Student.Entities.Section courseSection)
        {
            // The current user must be configured for grading by random ID
            if (!courseSection.GradeByRandomId)
            {
                throw new ConfigurationException(string.Format("Course section {0} does not use grading by random ID.", courseSection.Id));
            }
        }

        /// <summary>
        /// Verifies that academic record configuration is valid
        /// </summary>
        /// <exception cref="ConfigurationException">Academic record configuration from AC.DEFAULTS is null.</exception>
        /// <exception cref="ConfigurationException">Generate Random IDs field from ACPR is not set. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.</exception>
        private async Task VerifyAcademicRecordConfigurationAsync()
        {
            // Read academic record configuration to determine how the institution generates random IDs
            var config = await _studentConfigurationRepository.GetAcademicRecordConfigurationAsync();
            if (config == null)
            {
                string acadRecordConfigNullMessage = "Academic record configuration from AC.DEFAULTS could not be retrieved.";
                logger.Error(acadRecordConfigNullMessage);
                throw new ConfigurationException(acadRecordConfigNullMessage);
            }
            if (config.AnonymousGradingType == Domain.Student.Entities.AnonymousGradingType.None)
            {
                string acadRecordConfigMessage = "Generate Random IDs field from ACPR is not set. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.";
                logger.Error(acadRecordConfigMessage);
                throw new ConfigurationException(acadRecordConfigMessage);
            }
        }
    }
}
