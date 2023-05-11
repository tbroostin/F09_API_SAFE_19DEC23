// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class DepartmentalOversightService : BaseCoordinationService, IDepartmentalOversightService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly ITermRepository _termRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger _logger;

        public DepartmentalOversightService(IAdapterRegistry adapterRegistry, ISectionRepository sectionRepository, IFacultyRepository facultyRepository, IPersonBaseRepository personBaseRepository, ITermRepository termRepository, IReferenceDataRepository referenceDataRepository, IConfigurationRepository configurationRepository,
            IStudentConfigurationRepository studentConfigurationRepository, IStudentReferenceDataRepository studentReferenceDataRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _sectionRepository = sectionRepository;
            _facultyRepository = facultyRepository;
            _personBaseRepository = personBaseRepository;
            _termRepository = termRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentConfigurationRepository = studentConfigurationRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the departmental oversight permissions asynchronous.
        /// </summary>
        /// <returns>Departmental oversight permissions for authenticated user</returns>
        public async Task<Dtos.Student.DepartmentalOversightPermissions> GetDepartmentalOversightPermissionsAsync()
        {
            try
            {
                IEnumerable<string> permissionCodes = await GetUserPermissionCodesAsync();
                Domain.Base.Entities.DepartmentalOversightPermissions permissionsEntity = new Domain.Base.Entities.DepartmentalOversightPermissions(permissionCodes);
                var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.DepartmentalOversightPermissions, Dtos.Student.DepartmentalOversightPermissions>();
                Dtos.Student.DepartmentalOversightPermissions permissionsDto = entityToDtoAdapter.MapToType(permissionsEntity);
                return permissionsDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string messge = "An error occurred while retrieving departmental oversight permissions.";
                logger.Error(ex, messge);
                throw;
            }
        }

        /// <summary>
        /// Service method to retrieve the  result based on section/faculty search for Departmental oversight person
        /// </summary>
        /// <param name="criteria">DeptOversightSearchCriteria object</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>IEnumerable<Dtos.Student.DeptOversightSearchResult></returns>
        public async Task<IEnumerable<Dtos.Student.DeptOversightSearchResult>> SearchAsync(DeptOversightSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            List<DeptOversightSearchResult> deptOversightSearchResults = new List<DeptOversightSearchResult>();
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "A search criteria object must be supplied");
            }
            if ((string.IsNullOrEmpty(criteria.FacultyKeyword) && string.IsNullOrEmpty(criteria.SectionKeyword)) || (!string.IsNullOrEmpty(criteria.FacultyKeyword) && !string.IsNullOrEmpty(criteria.SectionKeyword)))
            {
                throw new ArgumentException("Search criteria must contain either a Section name or a Faculty Name/Id but not both.", "criteria");
            }
            if (!HasPermission(StudentPermissionCodes.ViewPersonInformation))
            {
                throw new PermissionsException("User does not have permissions to access any person information");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            var allDepartments = await _referenceDataRepository.GetDepartmentsAsync(false);
            var departmentsOfDO = allDepartments.Where(d => d.DepartmentalOversightIds != null && d.DepartmentalOversightIds.Contains(CurrentUser.PersonId)).Select(d => d.Code);
            //check if the departmentsOfDO had value
            if (departmentsOfDO != null && departmentsOfDO.Any())
            {
                //get the list of terms available for registration in RGWP form
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> registrationTerms = await _termRepository.GetRegistrationTermsAsync();

                // Gather nonRegistrationTermsToCheck - these are any beyond the terms listed on RGWP.
                var nonRegistrationTermsToCheck = new List<Domain.Student.Entities.Term>();

                //get the list of all terms
                var allTerms = await _termRepository.GetAsync();

                // get the list of non registration terms to check based on CSWP and GRWP form
                var gradingConfiguration = await _studentConfigurationRepository.GetFacultyGradingConfiguration2Async();
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
                //combine the list of both registration terms and non registration terms
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> terms = registrationTerms.Union(nonRegistrationTermsToCheck).Distinct().ToList();
                if (!string.IsNullOrEmpty(criteria.FacultyKeyword))
                {
                    return await FacultySearch(criteria.FacultyKeyword, departmentsOfDO, terms, pageSize, pageIndex);
                }
                else
                {
                    IEnumerable<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult> deptOversightData = new List<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult>();
                    deptOversightData = await _sectionRepository.GetDeptOversightSectionDetails(criteria.SectionKeyword, terms, departmentsOfDO);
                    var departmentalOversightDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult, Ellucian.Colleague.Dtos.Student.DeptOversightSearchResult>();
                    foreach (var deptOverSightDetail in deptOversightData)
                    {
                        deptOversightSearchResults.Add(departmentalOversightDtoAdapter.MapToType(deptOverSightDetail));
                    }
                }
            }
            int totalItems = 0;
            int totalPages = 0;
            totalItems = deptOversightSearchResults.Count();
            totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            return deptOversightSearchResults.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        /// <summary>
        /// Gets the departmental oversight and faculty details
        /// </summary>
        /// <param name="id">list of departmental oversight and faculty ids</param>
        /// <returns>list of departmental oversight and faculty dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.DepartmentalOversight>> QueryDepartmentalOversightAsync(IEnumerable<string> Ids, string SectionId, bool useCache = true)
        {
            if (!HasPermission(StudentPermissionCodes.ViewPersonInformation))
            {
                throw new PermissionsException("User does not have permissions to access any person information");
            }

            Domain.Student.Entities.Section section = null;
            var id = new List<string>() { SectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = null;
            if (useCache)
            {
                sections = await _sectionRepository.GetCachedSectionsAsync(id);
            }
            else
            {
                sections = await _sectionRepository.GetNonCachedSectionsAsync(id);
            }

            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                var departments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();

                if ((section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) || CheckDepartmentalOversightAccessForSection(section, departments))
                {
                    var results = new List<Dtos.Student.DepartmentalOversight>();
                    IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonBase> personBases = new List<Ellucian.Colleague.Domain.Base.Entities.PersonBase>();
                    personBases = await _personBaseRepository.SearchByIdsOrNamesAsync(Ids, null);

                    var departmentalOversightDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonBase, Ellucian.Colleague.Dtos.Student.DepartmentalOversight>();

                    // for departmental oversight using the same faculty name hierarchy for showing Person Display Name. 
                    string deptOversightNameHierarchy = await _facultyRepository.GetFacultyNameHierarchy();
                    foreach (var personBase in personBases)
                    {
                        if (!string.IsNullOrEmpty(deptOversightNameHierarchy))
                        {
                            NameAddressHierarchy hierarchy = null;
                            try
                            {
                                hierarchy = await _personBaseRepository.GetCachedNameAddressHierarchyAsync(deptOversightNameHierarchy);
                            }
                            catch (ColleagueSessionExpiredException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Unable to find name address hierarchy with ID " + deptOversightNameHierarchy + ". Not calculating hierarchy name.");

                            }
                            if (hierarchy != null)
                            {
                                if (personBase != null)
                                {
                                    personBase.PersonDisplayName = PersonNameService.GetHierarchyName(personBase, hierarchy);
                                }
                            }

                        }
                        results.Add(departmentalOversightDtoAdapter.MapToType(personBase));
                    }

                    return results;
                }

                string error = "Current user is not authorized to view person detail information for section : " + section.Id;
                logger.Error(error);
                throw new PermissionsException(error);
            }
            else
            {
                throw new KeyNotFoundException("Invalid ID for section: " + id);
            }
        }

        private async Task<IEnumerable<Dtos.Student.DeptOversightSearchResult>> FacultySearch(string facultyKeyword, IEnumerable<string> departmentsOfDO, IEnumerable<Domain.Student.Entities.Term> terms, int pageSize, int pageIndex)
        {
            List<DeptOversightSearchResult> deptOversightSearchResults = new List<DeptOversightSearchResult>();
            int totalItems = 0;
            int totalPages = 0;
            double facultyId;
            bool isId = double.TryParse(facultyKeyword, out facultyId);

            IEnumerable<string> facultyIds = new List<string>();
            if (isId)
            {
                var paddedFacultyId = await _facultyRepository.GetPID2FacultyIdAsync(facultyKeyword);
                facultyIds = new List<string> { paddedFacultyId };
            }
            else
            {
                string searchString = facultyKeyword;
                // Remove extra blank spaces
                var tempString = searchString.Trim();
                Regex regEx = new Regex(@"\s+");
                searchString = regEx.Replace(tempString, @" ");

                // Otherwise, we are doing a name search of advisees or the advisors - parse the search string into name parts
                string lastName = null;
                string firstName = null;
                string middleName = null;
                // Regular expression for all punctuation and numbers to remove from name string
                Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
                Regex regexNotSpace = new Regex(@"\s");

                var nameStrings = searchString.Split(',');
                // If there was a comma, set the first item to last name
                if (nameStrings.Count() > 1)
                {
                    lastName = nameStrings.ElementAt(0).Trim();
                    if (nameStrings.Count() >= 2)
                    {
                        // parse the two items after the comma using a space. Ignore anything else
                        var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                        if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                        if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                    }
                }
                else
                {
                    // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                    // Blank values don't hurt anything.
                    nameStrings = searchString.Split(' ');
                    switch (nameStrings.Count())
                    {
                        case 1:
                            lastName = nameStrings.ElementAt(0).Trim();
                            break;
                        case 2:
                            firstName = nameStrings.ElementAt(0).Trim();
                            lastName = nameStrings.ElementAt(1).Trim();
                            break;
                        default:
                            firstName = nameStrings.ElementAt(0).Trim();
                            middleName = nameStrings.ElementAt(1).Trim();
                            lastName = nameStrings.ElementAt(2).Trim();
                            break;
                    }
                }
                // Remove characters that won't make sense for each name part, including all punctuation and numbers 
                if (lastName != null)
                {
                    lastName = regexNotPunc.Replace(lastName, "");
                    lastName = regexNotSpace.Replace(lastName, "");
                }
                if (firstName != null)
                {
                    firstName = regexNotPunc.Replace(firstName, "");
                    firstName = regexNotSpace.Replace(firstName, "");
                }
                if (middleName != null)
                {
                    middleName = regexNotPunc.Replace(middleName, "");
                    middleName = regexNotSpace.Replace(middleName, "");
                }

                facultyIds = await _facultyRepository.SearchFacultyByNameAsync(lastName, firstName, middleName);
            }

            var facultyList = await _facultyRepository.GetFacultyByIdsAsync(facultyIds);
            if (facultyList != null && facultyList.Any())
            {
                List<string> distFacultyIds = facultyList.Select(fc => fc.Id).Distinct().ToList();
                var sectionsDetails = await _sectionRepository.GetFacultySectionsAsync(distFacultyIds);

                if (sectionsDetails != null && sectionsDetails.Any())
                {
                    //1: Get all depeartments Code from the sectionsDetails (Distinct)                 
                    //2: Loop through all the department and filter sectiondetails Ex: two section
                    //3: Loop through the filtered sections and construct the first object
                    //4: Terms can be retried from the sectionsDetails

                    //filter out the departments assigned to Departmental oversight in DEPT form
                    List<Domain.Student.Entities.Section> deptFilteredSections = new List<Domain.Student.Entities.Section>();
                    foreach (var dept in departmentsOfDO)
                    {
                        var deptSections = sectionsDetails.Where(f => f.Departments != null && f.Departments.Any(d => d.AcademicDepartmentCode == dept));
                        if (deptSections != null)
                        {
                            deptFilteredSections.AddRange(deptSections);
                        }
                    }
                    if (deptFilteredSections.Any())
                    {
                        //filter out the terms in RGWP form, CSWP form and GRWP form
                        List<Domain.Student.Entities.Section> termFilteredSections = new List<Domain.Student.Entities.Section>();
                        foreach (var term in terms)
                        {
                            var termSections = deptFilteredSections.Where(f => f.TermId == term.Code || string.IsNullOrEmpty(f.TermId));
                            if (termSections != null)
                            {
                                termFilteredSections.AddRange(termSections);
                            }
                        }
                        if (termFilteredSections.Any())
                        {
                            foreach (var facId in distFacultyIds)
                            {
                                var facSection = termFilteredSections.Where(s => s.FacultyIds.Any(f => f == facId)).ToList();
                                if (facSection != null && facSection.Any())
                                {
                                    var departmentsDistinct = facSection.Where(x => x.Departments != null).SelectMany(x => x.Departments.Select(dp => dp.AcademicDepartmentCode)).Distinct().ToList();
                                    // to prevent other departments data from being displayed in self service
                                    var finalListOfDepartments = departmentsOfDO.Intersect(departmentsDistinct).ToList();
                                    foreach (var depCode in finalListOfDepartments)
                                    {
                                        List<string> sectionIds = new List<string>();

                                        var finalFilteredSections = termFilteredSections.Where(s => s.Departments.Any(d => d.AcademicDepartmentCode == depCode));

                                        if (finalFilteredSections != null && finalFilteredSections.Any())
                                        {
                                            // Add Section Ids
                                            sectionIds.AddRange(finalFilteredSections.Where(fs => fs.IsActive).Select(s => s.Id.ToString()).Distinct().ToList());

                                            if (sectionIds.Count > 0)
                                            {
                                                DeptOversightSearchResult deptOversightSearchResult = new DeptOversightSearchResult()
                                                {
                                                    FacultyId = facId,
                                                    Department = depCode,
                                                    SectionIds = sectionIds
                                                };
                                                deptOversightSearchResults.Add(deptOversightSearchResult);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            totalItems = deptOversightSearchResults.Count();
            totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            return deptOversightSearchResults.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }
    }
}