// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
    /// FacultyService is an application that responds to a request for information about a faculty member
    /// </summary>
    [RegisterType]
    public class FacultyService : BaseCoordinationService, IFacultyService
    {
        private readonly IFacultyRepository _facultyRepository;
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ITermRepository _termRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;

        public FacultyService(IAdapterRegistry adapterRegistry, IFacultyRepository facultyRepository, IStudentConfigurationRepository configurationRepository, ISectionRepository sectionRepository, ITermRepository termRepository, IReferenceDataRepository referenceDataRepository, IStudentReferenceDataRepository studentReferenceDataRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _facultyRepository = facultyRepository;
            _configurationRepository = configurationRepository;
            _sectionRepository = sectionRepository;
            _termRepository = termRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        /// <summary>
        /// OBSOLETE AS OF API 1.3, REPLACED BY GetFacultySections2.
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd, defaults to today</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>List of <see cref="Section">Section</see> objects></returns>
        [Obsolete("Obsolete as of API 1.3")]
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section>>> GetFacultySectionsAsync(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today;
            }
            if (!endDate.HasValue || endDate.Value < startDate)
            {
                endDate = startDate.Value.AddDays(90.0);
            }
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            var allTerms = await _termRepository.GetAsync();
            var sectionEntities = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms)).Where(cs => ((cs.FacultyIds.Contains(facultyId)) && (cs.StartDate.CompareTo(endDate.Value) <= 0) && (!cs.EndDate.HasValue || (cs.EndDate.Value.CompareTo(startDate.Value) >= 0))));
            List<Dtos.Student.Section> sectionDtos = new List<Dtos.Student.Section>();
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>();
            foreach (var section in sectionEntities)
            {
                if (bestFit && string.IsNullOrEmpty(section.TermId))
                {
                    if (allTerms.Count() > 0)
                    {
                        var testTerms = allTerms.Where(t => ((t.StartDate.CompareTo(section.StartDate) <= 0 && t.EndDate.CompareTo(section.StartDate) >= 0) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && (section.EndDate.HasValue && t.StartDate.CompareTo(section.EndDate) <= 0)) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && !section.EndDate.HasValue)));
                        if (testTerms.Count() > 0)
                        {
                            section.TermId = testTerms.First().Code;
                        }
                    }
                }
            }
            return BuildPrivacyWrappedSectionDto(sectionEntities);
        }

        /// <summary>
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, defaults to today</param>
        /// <param name="endDate">Optional, endDate, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>List of <see cref="Section2">Section</see> objects></returns>
        [Obsolete("Obsolete as of API 1.5")]
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>>> GetFacultySections2Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today;
            }
            if (!endDate.HasValue || endDate.Value < startDate)
            {
                endDate = startDate.Value.AddDays(90.0);
            }
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            var allTerms = await _termRepository.GetAsync();
            var sectionEntities = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms)).Where(cs => ((cs.FacultyIds.Contains(facultyId)) && (cs.StartDate.CompareTo(endDate.Value) <= 0) && (!cs.EndDate.HasValue || (cs.EndDate.Value.CompareTo(startDate.Value) >= 0))));
            List<Dtos.Student.Section2> sectionDtos = new List<Dtos.Student.Section2>();
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section2>();
            foreach (var section in sectionEntities)
            {
                if (bestFit && string.IsNullOrEmpty(section.TermId))
                {
                    if (allTerms.Count() > 0)
                    {
                        var testTerms = allTerms.Where(t => ((t.StartDate.CompareTo(section.StartDate) <= 0 && t.EndDate.CompareTo(section.StartDate) >= 0) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && (section.EndDate.HasValue && t.StartDate.CompareTo(section.EndDate) <= 0)) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && !section.EndDate.HasValue)));
                        if (testTerms.Count() > 0)
                        {
                            section.TermId = testTerms.First().Code;
                        }
                    }
                }
            }
            return BuildPrivacyWrappedSection2Dto(sectionEntities);
        }

        /// <summary>
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd, defaults to today</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>Collection of requested Section3 DTOs</returns>
        [Obsolete("Obsolete as of API 1.13.1")]
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>>> GetFacultySections3Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today;
            }
            if (!endDate.HasValue || endDate.Value < startDate)
            {
                endDate = startDate.Value.AddDays(90.0);
            }
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            var allTerms = await _termRepository.GetAsync();
            List<Domain.Student.Entities.Section> sectionEntities = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms)).Where(cs => (cs.FacultyIds.Contains(facultyId)) && (cs.StartDate.CompareTo(endDate.Value) <= 0) && (!cs.EndDate.HasValue || (cs.EndDate.Value.CompareTo(startDate.Value) >= 0))).ToList();

            // catch non-Registration Term sections
            // possibly the current term, which could have been removed from RGWP if no futher reg actions are allowed (past Drop period)
            var nonRegistrationTermsToCheck = new List<Domain.Student.Entities.Term>();
            foreach (var term in allTerms)
            {
                if (!(term.EndDate.CompareTo(startDate) < 0 || term.StartDate.CompareTo(endDate) > 0))
                {
                    if (!registrationTerms.Contains(term))
                    {
                        nonRegistrationTermsToCheck.Add(term);
                    }
                }
            }
            if (nonRegistrationTermsToCheck.Count() > 0)
            {
                List<Domain.Student.Entities.Section> nonRegTermSectionEntities = (await _sectionRepository.GetNonCachedFacultySectionsAsync(nonRegistrationTermsToCheck, facultyId)).ToList();
                sectionEntities.AddRange(nonRegTermSectionEntities);
                sectionEntities = sectionEntities.Distinct().ToList();
            }

            List<Dtos.Student.Section3> sectionDtos = new List<Dtos.Student.Section3>();
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section3>();
            foreach (var section in sectionEntities)
            {
                if (bestFit && string.IsNullOrEmpty(section.TermId))
                {
                    if (allTerms.Count() > 0)
                    {
                        var testTerms = allTerms.Where(t => ((t.StartDate.CompareTo(section.StartDate) <= 0 && t.EndDate.CompareTo(section.StartDate) >= 0) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && (section.EndDate.HasValue && t.StartDate.CompareTo(section.EndDate) <= 0)) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && !section.EndDate.HasValue)));
                        if (testTerms.Count() > 0)
                        {
                            section.TermId = testTerms.First().Code;
                        }
                    }
                }
            }
            return BuildPrivacyWrappedSection3Dto(sectionEntities);
        }

        /// <summary>
        /// Get a list of sections taught by faculty ID based on a date range or system parameters. If a start date is not specified sections will be returned based on 
        /// the allowed terms specified on Registration Web Parameters (RGWP), Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP).
        /// </summary>
        /// <param name="facultyId">A faculty ID - if not supplied an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd. If a start date is specified but end date is not, it will default to 90 days past start date. It must be greater than start date if specified, otherwise it will default to 90 days past start.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached <see cref="Section3">course section</see> data. Defaults to true.</param>
        /// <returns>Collection of requested Section3 DTOs</returns>
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>>> GetFacultySections4Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit, bool useCache = true)
        {
            // Note - there is no permissions or current user check.  Anyone can see section information that is filtered by a faculty Id and either a date range or the default limiting parameters. 
            // Which means it is really a query of sections...

            if (string.IsNullOrEmpty(facultyId))
            {
                return new PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>(), false);
            }
            if (startDate.HasValue && (!endDate.HasValue || endDate.Value < startDate))
            {
                endDate = startDate.Value.AddDays(90.0);
            }
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            var allTerms = await _termRepository.GetAsync();

            List<Domain.Student.Entities.Section> sectionEntities = new List<Domain.Student.Entities.Section>();
            if (registrationTerms != null && registrationTerms.Any())
            {
                if (useCache)
                {
                    // Limit the registration sections to only those taught by this faculty Id.
                    sectionEntities = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms)).Where(cs => (cs.FacultyIds.Contains(facultyId))).ToList();
                    var sectionIds = String.Join(",", sectionEntities.Select(s => s.Id));
                }
                else
                {
                    sectionEntities = (await _sectionRepository.GetNonCachedFacultySectionsAsync(registrationTerms, facultyId)).ToList();
                    var sectionIds = String.Join(",", sectionEntities.Select(s => s.Id));
                }
            }

            // Gather nonRegistrationTermsToCheck - these are any beyond the terms listed on RGWP.
            var nonRegistrationTermsToCheck = new List<Domain.Student.Entities.Term>();
            // If a date range was supplied determine which terms are in the date range but are not on RGWP. For example, this may return the current term, which could have been removed from RGWP if no futher reg actions are allowed (past Drop period)
            if (startDate.HasValue && endDate.HasValue)
            {
                // First reduce the list of registration sections to those within this date range.
                sectionEntities = sectionEntities.Where(cs => cs.StartDate.CompareTo(endDate.Value) <= 0 && (!cs.EndDate.HasValue || (cs.EndDate.Value.CompareTo(startDate.Value) >= 0))).ToList();
                // Next find the list of other terms that need to be considered in this date range.
                foreach (var term in allTerms)
                {
                    if (!(term.EndDate.CompareTo(startDate) < 0 || term.StartDate.CompareTo(endDate) > 0))
                    {
                        if (!registrationTerms.Contains(term))
                        {
                            nonRegistrationTermsToCheck.Add(term);
                        }
                    }
                }
            }
            else
            {
                // If a date range is not specified figure out which terms are on  
                // Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP) but that are not on RGWP.
                var gradingConfiguration = await _configurationRepository.GetFacultyGradingConfigurationAsync();
                var gradingTerms = gradingConfiguration != null && gradingConfiguration.AllowedGradingTerms != null ? gradingConfiguration.AllowedGradingTerms.ToList() : new List<string>();
                var scheduleTermsValcodes = await _studentReferenceDataRepository.GetAllScheduleTermsAsync(false);
                var scheduleTerms = scheduleTermsValcodes != null ? scheduleTermsValcodes.Select(st => st.Code).ToList() : new List<string>();
                var otherTerms = gradingTerms.Union(scheduleTerms).Except(registrationTerms.Select(r => r.Code)).ToList();
                nonRegistrationTermsToCheck = (from termcode in otherTerms
                                               join term in allTerms
                                                   on termcode equals term.Code
                                                   into joinNonRegTerms
                                                   from resultTerm in joinNonRegTerms
                                               select resultTerm).ToList();


            }
            if (nonRegistrationTermsToCheck.Count() > 0)
            {
                List<Domain.Student.Entities.Section> nonRegTermSectionEntities = (await _sectionRepository.GetNonCachedFacultySectionsAsync(nonRegistrationTermsToCheck, facultyId, bestFit)).ToList();
                var sectionIds = String.Join(",", nonRegTermSectionEntities.Select(s => s.Id));
                sectionEntities.AddRange(nonRegTermSectionEntities);
                sectionEntities = sectionEntities.Distinct().ToList();
            }

            List<Dtos.Student.Section3> sectionDtos = new List<Dtos.Student.Section3>();
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section3>();
            var finalSectionIds = String.Join(",", sectionEntities.Select(s => s.Id));
            foreach (var section in sectionEntities)
            {
                if (bestFit && string.IsNullOrEmpty(section.TermId))
                {
                    if (allTerms.Count() > 0)
                    {
                        var testTerms = allTerms.Where(t => ((t.StartDate.CompareTo(section.StartDate) <= 0 && t.EndDate.CompareTo(section.StartDate) >= 0) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && (section.EndDate.HasValue && t.StartDate.CompareTo(section.EndDate) <= 0)) ||
                                (t.StartDate.CompareTo(section.StartDate) >= 0 && !section.EndDate.HasValue)));
                        if (testTerms.Count() > 0)
                        {
                            section.TermId = testTerms.First().Code;
                        }
                    }
                }
            }
            return BuildPrivacyWrappedSection3Dto(sectionEntities);
        }

        /// <summary>
        /// Get a single faculty by Id
        /// </summary>
        /// <param name="facultyid">Id of faculty.</param>
        /// <returns>Faculty dto</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.Faculty> GetAsync(string facultyid)
        {
            Domain.Student.Entities.Faculty faculty = await _facultyRepository.GetAsync(facultyid);
            return await BuildFacultyDtoAsync(faculty);
        }

        /// <summary>
        /// Get a list of faculty by Id
        /// </summary>
        /// <param name="id">list of faculty (person) ids</param>
        /// <returns>list of faculty dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Faculty>> QueryFacultyAsync(FacultyQueryCriteria criteria)
        {
            if (criteria.FacultyIds == null || criteria.FacultyIds.Count() == 0)
            {
                string errorText = "At least one item must be provided in list of facultyIds.";
                throw new ArgumentNullException("criteria", errorText);
            }

            List<Dtos.Student.Faculty> facultyDtos = new List<Dtos.Student.Faculty>();

            var facultyEntities = await _facultyRepository.GetFacultyByIdsAsync(criteria.FacultyIds);
            if (facultyEntities != null && facultyEntities.Count() > 0)
            {
                foreach (var faculty in facultyEntities)
                {
                    try
                    {
                        facultyDtos.Add(await BuildFacultyDtoAsync(faculty));
                    }
                    catch
                    {
                        // do not throw for a dto conversion error, simply move on
                    }
                }
            }
            return facultyDtos;
        }

        /// <summary>
        /// Get a list of faculty by Id
        /// </summary>
        /// <param name="id">list of faculty (person) ids</param>
        /// <returns>list of faculty dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Faculty>> GetFacultyByIdsAsync(IEnumerable<string> facultyIds)
        {
            List<Dtos.Student.Faculty> facultyDtos = new List<Dtos.Student.Faculty>();

            var facultyEntities = await _facultyRepository.GetFacultyByIdsAsync(facultyIds);
            if (facultyEntities != null && facultyEntities.Count() > 0)
            {
                foreach (var faculty in facultyEntities)
                {
                    facultyDtos.Add(await BuildFacultyDtoAsync(faculty));
                }
            }
            return facultyDtos;
        }
        /// <summary>
        /// Returns a list of Faculty Keys
        /// </summary>
        /// <param name="facultyOnlyFlag">Set to true to return only faculty and no advisors</param>
        /// <param name="advisorOnlyFlag">Set to true to return advisors and no faculty</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> SearchFacultyIdsAsync(bool facultyOnlyFlag, bool advisorOnlyFlag)
        {
            return await _facultyRepository.SearchFacultyIdsAsync(facultyOnlyFlag, advisorOnlyFlag);
        }

        private async Task<Ellucian.Colleague.Dtos.Student.Faculty> BuildFacultyDtoAsync(Domain.Student.Entities.Faculty faculty)
        {
            StudentConfiguration studentConfiguration = await _configurationRepository.GetStudentConfigurationAsync();
            IEnumerable<string> facultyEmailAddresses = faculty.GetFacultyEmailAddresses(studentConfiguration.FacultyEmailTypeCode);
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> facultyPhoneEntities = faculty.GetFacultyPhones(studentConfiguration.FacultyPhoneTypeCode);
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> facultyAddressEntities = new List<Ellucian.Colleague.Domain.Base.Entities.Address>();
            var adrelType = GetAddressRelationType();
            if (adrelType != null && !string.IsNullOrEmpty(adrelType.SpecialProcessingAction2))
            {
                facultyAddressEntities = faculty.GetFacultyAddresses(adrelType.SpecialProcessingAction2);
            }

            // Get the right adapter for the type mapping
            var facultyDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>();
            var phoneDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>();
            var addressDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Address, Dtos.Base.Address>();
            // Map the Phone Entities
            List<Ellucian.Colleague.Dtos.Base.Phone> facultyPhoneDtos = new List<Dtos.Base.Phone>();
            if (facultyPhoneEntities != null)
            {
                foreach (var phoneEntity in facultyPhoneEntities)
                {
                    Dtos.Base.Phone phoneDto = phoneDtoAdapter.MapToType(phoneEntity);
                    facultyPhoneDtos.Add(phoneDto);
                }
            }
            // Map the Address Entities
            List<Ellucian.Colleague.Dtos.Base.Address> facultyAddressDtos = new List<Dtos.Base.Address>();
            if (facultyAddressEntities != null)
            {
                foreach (var addressEntity in facultyAddressEntities)
                {
                    Dtos.Base.Address addressDto = addressDtoAdapter.MapToType(addressEntity);
                    facultyAddressDtos.Add(addressDto);
                }
            }
            // Map the person entity to the person DTO
            Ellucian.Colleague.Dtos.Student.Faculty facultyDto = facultyDtoAdapter.MapToType(faculty);

            // Add in the phone, email and address information to the Dto.
            facultyDto.EmailAddresses = facultyEmailAddresses;
            facultyDto.Phones = facultyPhoneDtos;
            facultyDto.Addresses = facultyAddressDtos;

            return facultyDto;
        }

        private AddressRelationType GetAddressRelationType()
        {
            IEnumerable<AddressRelationType> AdrelTypes = new List<AddressRelationType>();
            try
            {
                AdrelTypes = _referenceDataRepository.AddressRelationTypes.Where(adt => adt.SpecialProcessingAction2 == "FAC");
            }
            catch
            {
                // No Code found with FAC in special processing
            }
            if (AdrelTypes.Count() > 0)
            {
                return AdrelTypes.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the faculty specific permissions pertinent to this user.
        /// </summary>
        /// <returns>A list of permission codes</returns>
        public async Task<IEnumerable<string>> GetFacultyPermissionsAsync()
        {
            var permissions = new List<string>();
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.CreatePrerequisiteWaiver))
            {
                permissions.Add(StudentPermissionCodes.CreatePrerequisiteWaiver);
            }
            if (userPermissions.Contains(StudentPermissionCodes.CreateFacultyConsent))
            {
                permissions.Add(StudentPermissionCodes.CreateFacultyConsent);
            }
            if (userPermissions.Contains(StudentPermissionCodes.CreateStudentPetition))
            {
                permissions.Add(StudentPermissionCodes.CreateStudentPetition);
            }
            if (userPermissions.Contains(StudentPermissionCodes.UpdateGrades))
            {
                permissions.Add(StudentPermissionCodes.UpdateGrades);
            }
            return permissions;
        }

        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Advising permissions for the authenticated user.</returns>
        public async Task<Dtos.Student.FacultyPermissions> GetFacultyPermissions2Async()
        {
            try
            {
                IEnumerable<string> permissionCodes = await GetUserPermissionCodesAsync();
                logger.Info("GetFacultyPermissions2: GetUserPermissionCodesAsync returned " + String.Join(", ", permissionCodes.ToArray()));

                Domain.Student.Entities.FacultyPermissions permissionsEntity = new Domain.Student.Entities.FacultyPermissions(permissionCodes);
                ITypeAdapter<Domain.Student.Entities.FacultyPermissions, Dtos.Student.FacultyPermissions> entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.FacultyPermissions, Dtos.Student.FacultyPermissions>();
                Dtos.Student.FacultyPermissions permissionsDto = entityToDtoAdapter.MapToType(permissionsEntity);
                return permissionsDto;
            }
            catch (Exception ex)
            {
                if (CurrentUser != null)
                {
                    logger.Error(ex, string.Format("An error occurred while retrieving user {0}'s faculty permissions.", CurrentUser.PersonId));
                }
                else
                {
                    logger.Error(ex, "An error occurred while retrieving faculty permissions.");
                }
                throw new ApplicationException("An error occurred while retrieving faculty permissions.");
            }
        }

        /// <summary>
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section>> BuildPrivacyWrappedSectionDto(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section> sectionDtos = new List<Dtos.Student.Section>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>> BuildPrivacyWrappedSection2Dto(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section2>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section2> sectionDtos = new List<Dtos.Student.Section2>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section2 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section2>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>> BuildPrivacyWrappedSection3Dto(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section3>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section3> sectionDtos = new List<Dtos.Student.Section3>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section3 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section3>>(sectionDtos, hasPrivacyRestriction);
        }
    }
}
