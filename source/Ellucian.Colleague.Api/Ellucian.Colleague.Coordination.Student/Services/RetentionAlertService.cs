// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Repositories;
using slf4net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base;
using System.Linq;
using System.Text.RegularExpressions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for Retention Alert functionality
    /// </summary>
    [RegisterType]
    public class RetentionAlertService: StudentCoordinationService, IRetentionAlertService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IRetentionAlertRepository _retentionAlertRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for Retention Alert Service
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public RetentionAlertService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IStudentRepository studentRepository,IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IPersonBaseRepository personBaseRepository, ILogger logger, IRetentionAlertRepository retentionAlertRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository:studentRepository, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _retentionAlertRepository = retentionAlertRepository;
            _personBaseRepository = personBaseRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the List of case types
        /// </summary>
        /// <returns>A list of Case Types</returns>
        public async Task<IEnumerable<CaseType>> GetCaseTypesAsync()
        {
            var caseTypeDtoCollection = new List<CaseType>();
            var caseTypeCollection = await _referenceDataRepository.GetCaseTypesAsync();

            // Get the right adapter for the type mapping
            var caseTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseType, CaseType>();

            // Map the case Type entity to the case type DTO
            foreach (var caseType in caseTypeCollection)
            {
                caseTypeDtoCollection.Add(caseTypeDtoAdapter.MapToType(caseType));
            }
            return caseTypeDtoCollection;
        }

        /// <summary>
        /// Gets the List of case categories
        /// </summary>
        /// <returns>A list of Case categories</returns>
        public async Task<IEnumerable<CaseCategory>> GetCaseCategoriesAsync()
        {
            var caseCategoryDtoCollection = new List<CaseCategory>();
            var caseCategoryCollection = await _referenceDataRepository.GetCaseCategoriesAsync();

            // Get the right adapter for the type mapping
            var caseCategoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseCategory, CaseCategory>();

            // Map the case category entity to the case category DTO
            foreach (var casecategory in caseCategoryCollection)
            {
                caseCategoryDtoCollection.Add(caseCategoryDtoAdapter.MapToType(casecategory));
            }
            return caseCategoryDtoCollection;
        }

        /// <summary>
        /// Get the Org Role settings for Case Categories.
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids.</param>
        /// <returns>List of Retention Alert Case Category Org Roles</returns>
        public async Task<IEnumerable<RetentionAlertCaseCategoryOrgRoles>> GetRetentionAlertCaseCategoryOrgRolesAsync(List<string> caseCategoryIds)
        {
            if (caseCategoryIds == null || !caseCategoryIds.Any())
            {
                throw new ArgumentNullException("caseCategoryIds", "caseCategoryIds cannot be null or empty.");
            }
            var caseCategoryOrgRoleDtos = new List<RetentionAlertCaseCategoryOrgRoles>();
            var caseCategoryOrgRoleEntities = await _retentionAlertRepository.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);
            var caseCategoryOrgRoleAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, RetentionAlertCaseCategoryOrgRoles>();
            foreach (var item in caseCategoryOrgRoleEntities)
            {
                caseCategoryOrgRoleDtos.Add(caseCategoryOrgRoleAdapter.MapToType(item));
            }
            return caseCategoryOrgRoleDtos;
        }

        /// <summary>
        /// Gets the retention alert permissions asynchronous.
        /// </summary>
        /// <returns>Retention alert permissions for authenticated user</returns>
        public async Task<RetentionAlertPermissions> GetRetentionAlertPermissionsAsync()
        {
            try
            {
                IEnumerable<string> permissionCodes = await GetUserPermissionCodesAsync();
                Domain.Base.Entities.RetentionAlertPermissions permissionsEntity = new Domain.Base.Entities.RetentionAlertPermissions(permissionCodes);
                var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertPermissions, RetentionAlertPermissions>();
                RetentionAlertPermissions permissionsDto = entityToDtoAdapter.MapToType(permissionsEntity);
                return permissionsDto;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while retrieving retention alert permissions.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                string messge = "An error occurred while retrieving retention alert permissions.";
                logger.Error(ex, messge);
                throw new ApplicationException(messge);
            }
        }

        /// <summary>
        /// Gets the List of case closure reasons
        /// </summary>
        /// <returns>A list of Case closure reasons</returns>
        public async Task<IEnumerable<CaseClosureReason>> GetCaseClosureReasonsAsync()
        {
            var caseClosureReasonDtoCollection = new List<CaseClosureReason>();
            var caseClosureReasonCollection = await _referenceDataRepository.GetCaseClosureReasonsAsync();

            // Get the right adapter for the type mapping
            var caseClosureReasonDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseClosureReason, CaseClosureReason>();

            // Map the case closure reason entity to the case closure reason DTO
            foreach (var caseclosureReason in caseClosureReasonCollection)
            {
                caseClosureReasonDtoCollection.Add(caseClosureReasonDtoAdapter.MapToType(caseclosureReason));
            }
            return caseClosureReasonDtoCollection;
        }


        /// <summary>
        /// Gets the List of case priorities
        /// </summary>
        /// <returns>A list of Case Priorities</returns>
        public async Task<IEnumerable<CasePriority>> GetCasePrioritiesAsync()
        {
            var casePriorityDtoCollection = new List<CasePriority>();
            var casePriorityCollection = await _referenceDataRepository.GetCasePrioritiesAsync();

            // Get the right adapter for the type mapping
            var casePriorityDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CasePriority, CasePriority>();

            // Map the case Priority entity to the case Priority DTO
            foreach (var casePriority in casePriorityCollection)
            {
                casePriorityDtoCollection.Add(casePriorityDtoAdapter.MapToType(casePriority));
            }
            return casePriorityDtoCollection;
        }


        /// <summary>
        /// Retrieves retention alert contributions
        /// </summary>
        /// <param name="contributionsQueryCriteria">Retention Alert Contributions Query Criteria</param>
        /// <returns>A list of retention alert contributions</returns>
        public async Task<IEnumerable<RetentionAlertWorkCase>> GetRetentionAlertContributionsAsync(ContributionsQueryCriteria contributionsQueryCriteria)
        {
            if (contributionsQueryCriteria == null)
            {
                throw new ArgumentNullException("contributionsQueryCriteria", "Contributions Query Criteria must be specified.");
            }

            var retentionAlertTypeContributionsDtoCollection = new List<RetentionAlertWorkCase>();

            await CheckforRAPermissionsAsync();

            var dtoAdapter = _adapterRegistry.GetAdapter<ContributionsQueryCriteria, Domain.Base.Entities.ContributionsQueryCriteria>();
            var queryDto = dtoAdapter.MapToType(contributionsQueryCriteria);

            var retentionAlertContributionsCollection = await _retentionAlertRepository.GetRetentionAlertContributionsAsync(CurrentUser.PersonId, queryDto);

            // Get the right adapter for the type mapping
            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCase, RetentionAlertWorkCase>();

            foreach (var retentionAlertContribution in retentionAlertContributionsCollection)
            {
                retentionAlertTypeContributionsDtoCollection.Add(retentionAlertDtoAdapter.MapToType(retentionAlertContribution));
            }
            return retentionAlertTypeContributionsDtoCollection;
        }

        /// <summary>
        /// Retrieves retention alert open cases
        /// </summary>
        /// <returns>A list of retention alert open cases</returns>
        public async Task<IEnumerable<RetentionAlertOpenCase>> GetRetentionAlertOpenCasesAsync()
        {
            var retentionAlertOpenCasesDtoCollection = new List<RetentionAlertOpenCase>();

            await CheckforReportsPermission();

            var retentionAlertOpenCasesCollection = await _retentionAlertRepository.GetRetentionAlertOpenCasesAsync(CurrentUser.PersonId);

            // Get the right adapter for the type mapping
            var openCasesDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertOpenCase, RetentionAlertOpenCase>();

            foreach (var retentionAlertOpenCase in retentionAlertOpenCasesCollection)
            {
                retentionAlertOpenCasesDtoCollection.Add(openCasesDtoAdapter.MapToType(retentionAlertOpenCase));
            }
            return retentionAlertOpenCasesDtoCollection;
        }

        /// <summary>
        /// Retrieves retention alert closed cases grouped by closure reason
        /// </summary>
        /// <param name="categoryId">Retention Alert Case Category Id</param>
        /// <returns>A list of retention alert open cases</returns>
        public async Task<IEnumerable<RetentionAlertClosedCasesByReason>> GetRetentionAlertClosedCasesByReasonAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                throw new ArgumentNullException("categoryId", "categoryId cannot be null or empty to retrieve closed cases grouped by closure reason.");
            }
            var dtos = new List<RetentionAlertClosedCasesByReason>();

            await CheckforReportsPermission();

            var retentionAlertOpenCasesCollection = await _retentionAlertRepository.GetRetentionAlertClosedCasesByReasonAsync(categoryId);

            // Get the right adapter for the type mapping
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertClosedCasesByReason, RetentionAlertClosedCasesByReason>();

            foreach (var item in retentionAlertOpenCasesCollection)
            {
                dtos.Add(adapter.MapToType(item));
            }
            return dtos;
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Retention Alert Query Criteria</param>
        /// <returns>Retention Alert Work Case List</returns>
        public async Task<IEnumerable<RetentionAlertWorkCase>> GetRetentionAlertCasesAsync(RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            var retentionAlertTypeDtoCollection = new List<RetentionAlertWorkCase>();
            
            await CheckforWorkCasePermission();

            List<Domain.Base.Entities.RetentionAlertWorkCase> retentionAlertCollection;
            if (!string.IsNullOrEmpty(retentionAlertQueryCriteria.StudentSearchKeyword))
            {
                // Remove extra blank spaces
                var tempString = retentionAlertQueryCriteria.StudentSearchKeyword.Trim();
                Regex regEx = new Regex(@"\s+");
                retentionAlertQueryCriteria.StudentSearchKeyword = regEx.Replace(tempString, @" ");

                // If search string is a numeric ID and it is for a particular student, return only that
                double personId;
                bool isId = double.TryParse(retentionAlertQueryCriteria.StudentSearchKeyword, out personId);

                if (isId)
                {
                    retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCasesAsync(CurrentUser.PersonId, new List<string>() { retentionAlertQueryCriteria.StudentSearchKeyword }, retentionAlertQueryCriteria.CaseIds);
                }
                else
                {
                    // Otherwise, we are doing a name search of students - parse the search string into name parts
                    string lastName = null;
                    string firstName = null;
                    string middleName = null;
                    // Regular expression for all punctuation and numbers to remove from name string
                    Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
                    Regex regexNotSpace = new Regex(@"\s");

                    var nameStrings = retentionAlertQueryCriteria.StudentSearchKeyword.Split(',');
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
                        nameStrings = retentionAlertQueryCriteria.StudentSearchKeyword.Split(' ');
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

                    var matchingStudents = await _retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName, firstName, middleName);

                    if (matchingStudents.Any())
                    {

                        retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCasesAsync(CurrentUser.PersonId, matchingStudents, retentionAlertQueryCriteria.CaseIds);
                    }
                    else
                    {
                        retentionAlertQueryCriteria.CaseIds = new List<string>() { "" };
                        retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCasesAsync(CurrentUser.PersonId, null, retentionAlertQueryCriteria.CaseIds);
                    }

                }
            }
            else
            {
                retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCasesAsync(CurrentUser.PersonId, null, retentionAlertQueryCriteria.CaseIds);
            }

            // Get the right adapter for the type mapping
            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCase, RetentionAlertWorkCase>();

            foreach (var retentionAlertCase in retentionAlertCollection)
            {
                retentionAlertTypeDtoCollection.Add(retentionAlertDtoAdapter.MapToType(retentionAlertCase));
            }
            return retentionAlertTypeDtoCollection;
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Retention Alert Query Criteria</param>
        /// <returns>Retention Alert Work Case 2 List</returns>
        public async Task<IEnumerable<RetentionAlertWorkCase2>> GetRetentionAlertCases2Async(RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            var retentionAlertTypeDtoCollection = new List<RetentionAlertWorkCase2>();

            await CheckforWorkCasePermission();
            List<string> roles = CurrentUser.Roles.ToList();
            IEnumerable<Domain.Entities.Role> rolesMaster = (await _roleRepository.GetRolesAsync());
            IEnumerable<string> roleIds = rolesMaster.Where(r => roles.Contains(r.Title)).Select(r=>r.Id.ToString());
            List<Domain.Base.Entities.RetentionAlertWorkCase2> retentionAlertCollection;
            if (!string.IsNullOrEmpty(retentionAlertQueryCriteria.StudentSearchKeyword))
            {
                // Remove extra blank spaces
                var tempString = retentionAlertQueryCriteria.StudentSearchKeyword.Trim();
                Regex regEx = new Regex(@"\s+");
                retentionAlertQueryCriteria.StudentSearchKeyword = regEx.Replace(tempString, @" ");

                // If search string is a numeric ID and it is for a particular student, return only that
                double personId;
                bool isId = double.TryParse(retentionAlertQueryCriteria.StudentSearchKeyword, out personId);

                if (isId)
                {
                    retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCases2Async(CurrentUser.PersonId, new List<string>() { retentionAlertQueryCriteria.StudentSearchKeyword }, retentionAlertQueryCriteria.CaseIds, roleIds, retentionAlertQueryCriteria.IsIncludeClosedCases);
                }
                else
                {
                    // Otherwise, we are doing a name search of students - parse the search string into name parts
                    string lastName = null;
                    string firstName = null;
                    string middleName = null;
                    // Regular expression for all punctuation and numbers to remove from name string
                    Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
                    Regex regexNotSpace = new Regex(@"\s");

                    var nameStrings = retentionAlertQueryCriteria.StudentSearchKeyword.Split(',');
                    // If there was a comma, set the first item to last name
                    if (nameStrings.Count() > 1)
                    {
                        lastName = nameStrings.ElementAt(0).Trim();
                        if (nameStrings.Count() >= 2)
                        {
                            // parse the two items after  (last) or (first last) or (first middle last). 
                        // Blank values don't hurt anything.the comma using a space. Ignore anything else
                            var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                            if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                            if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                        }
                    }
                    else
                    {
                        // Parse entry using spaces, assume entered
                        nameStrings = retentionAlertQueryCriteria.StudentSearchKeyword.Split(' ');
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

                    var matchingStudents = await _retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName, firstName, middleName);

                    if (matchingStudents.Any())
                    {

                        retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCases2Async(CurrentUser.PersonId, matchingStudents, retentionAlertQueryCriteria.CaseIds, roleIds, retentionAlertQueryCriteria.IsIncludeClosedCases);
                    }
                    else
                    {
                        retentionAlertQueryCriteria.CaseIds = new List<string>() { "" };
                        retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCases2Async(CurrentUser.PersonId, null, retentionAlertQueryCriteria.CaseIds, roleIds, retentionAlertQueryCriteria.IsIncludeClosedCases);
                    }
                }
            }
            else
            {
                retentionAlertCollection = await _retentionAlertRepository.GetRetentionAlertCases2Async(CurrentUser.PersonId, null, retentionAlertQueryCriteria.CaseIds, roleIds, retentionAlertQueryCriteria.IsIncludeClosedCases);
            }

            // Get the right adapter for the type mapping
            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCase2, RetentionAlertWorkCase2>();

            List<string> roleOwners = (retentionAlertCollection != null && retentionAlertCollection.Any()) ? 
                                    retentionAlertCollection.Where(x => x.CaseOwnerIds != null && x.CaseOwnerIds.Any()).SelectMany(x => x.CaseOwnerIds).ToList() : new List<string>();

            var personBaseObjects = roleOwners.Any() ? await _personBaseRepository.GetPersonsBaseAsync(roleOwners.Distinct()) : null;

            foreach (var retentionAlertCase in retentionAlertCollection)
            {
                List<string> caseOwners = new List<string>();
                if(personBaseObjects != null && personBaseObjects.Any() && retentionAlertCase.CaseOwnerIds.Any())
                {
                    caseOwners.AddRange(personBaseObjects.Where(p => retentionAlertCase.CaseOwnerIds.Contains(p.Id)).Select(p => (p.FirstName + " " + p.LastName)).ToList());
                }
                if (retentionAlertCase.CaseRoleIds.Any())
                {
                    caseOwners.AddRange(rolesMaster.Where(r => retentionAlertCase.CaseRoleIds.Contains(r.Id)).Select(r => r.Title).ToList());
                }

                if (caseOwners.Any())
                    retentionAlertCase.CaseOwner = string.Join("; ", caseOwners);

                retentionAlertTypeDtoCollection.Add(retentionAlertDtoAdapter.MapToType(retentionAlertCase));
            }
            return retentionAlertTypeDtoCollection;
        }

        /// <summary>
        /// Creates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase"></param>
        /// <returns>Retention Alert Case Create Response</returns>
        public async Task<RetentionAlertCaseCreateResponse> AddRetentionAlertCaseAsync(RetentionAlertCase retentionAlertCase)
        {
            if (retentionAlertCase == null)
            {
                throw new ArgumentNullException("retentionAlertCase", "retention Alert Case information must be specified to add a case.");
            }
            
            await CheckforRAPermissionsAsync();

            var addCaseDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertCase, Domain.Base.Entities.RetentionAlertCase>();
            var addCaseDto = addCaseDtoAdapter.MapToType(retentionAlertCase);

            var createResponse = await _retentionAlertRepository.AddRetentionAlertCaseAsync(addCaseDto);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertCaseCreateResponse, RetentionAlertCaseCreateResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Updates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase"></param>
        /// <returns>Retention Alert Case Create Response</returns>
        public async Task<RetentionAlertCaseCreateResponse> UpdateRetentionAlertCaseAsync(string caseId, RetentionAlertCase retentionAlertCase)
        {
            if(string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "case id must be specified to update a case for student.");
            }
            if (retentionAlertCase == null)
            {
                throw new ArgumentNullException("retentionAlertCase", "retention Alert Case information must be specified to update a case.");
            }           

            var addCaseDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertCase, Domain.Base.Entities.RetentionAlertCase>();
            var addCaseDto = addCaseDtoAdapter.MapToType(retentionAlertCase);

            var createResponse = await _retentionAlertRepository.UpdateRetentionAlertCaseAsync(caseId, addCaseDto);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertCaseCreateResponse, RetentionAlertCaseCreateResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Add a note to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseNoteAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to add a Case Note.");
            }
            if (retentionAlertCaseNote == null)
            {
                throw new ArgumentNullException("retentionAlertCaseNote", "retention Alert Case Action information must be specified to update a case.");
            }
            
            await CheckforRAPermissionsAsync();

            var addCaseDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseNote, Domain.Base.Entities.RetentionAlertWorkCaseNote>();
            var addCaseEntity = addCaseDtoAdapter.MapToType(retentionAlertCaseNote);

            addCaseEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.AddRetentionAlertCaseNoteAsync(caseId, addCaseEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Add a follwoup to a Retention Alert Case, this will not add the user to the case owners list.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseFollowUpAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to add a Case FollowUp.");
            }
            if (retentionAlertCaseNote == null)
            {
                throw new ArgumentNullException("retentionAlertCaseNote", "retention Alert Case Action information must be specified to update a case.");
            }

            await CheckforRAPermissionsAsync();

            var addCaseDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseNote, Domain.Base.Entities.RetentionAlertWorkCaseNote>();
            var addCaseEntity = addCaseDtoAdapter.MapToType(retentionAlertCaseNote);

            addCaseEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.AddRetentionAlertCaseFollowUpAsync(caseId, addCaseEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Add a communication code to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCommCode"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseCommCodeAsync(string caseId, RetentionAlertWorkCaseCommCode retentionAlertCaseCommCode)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to add a Communication Code to a Case.");
            }
            if (retentionAlertCaseCommCode == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retention Alert Case Action information must be specified to add a communication code to a case.");
            }
            
            await CheckforWorkCasePermission();

            var caseNoteDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseCommCode, Domain.Base.Entities.RetentionAlertWorkCaseCommCode>();
            var caseNoteEntity = caseNoteDtoAdapter.MapToType(retentionAlertCaseCommCode);

            caseNoteEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync(caseId, caseNoteEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Add a case type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseTypeAsync(string caseId, RetentionAlertWorkCaseType retentionAlertCaseType)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to add a Type to a Case.");
            }
            if (retentionAlertCaseType == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retention Alert Case Action information must be specified to add a case type to a case.");
            }
            
            await CheckforWorkCasePermission();

            var caseCommCodeDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseType, Domain.Base.Entities.RetentionAlertWorkCaseType>();
            var caseCommCodeEntity = caseCommCodeDtoAdapter.MapToType(retentionAlertCaseType);

            caseCommCodeEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.AddRetentionAlertCaseTypeAsync(caseId, caseCommCodeEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Change the priority of a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCasePriority"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> ChangeRetentionAlertCasePriorityAsync(string caseId, RetentionAlertWorkCasePriority retentionAlertCasePriority)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to change the priority of a Case.");
            }
            if (retentionAlertCasePriority == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retention Alert Case Action information must be specified to add a case type to a case.");
            }
            
            await CheckforWorkCasePermission();

            var casePriorityDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCasePriority, Domain.Base.Entities.RetentionAlertWorkCasePriority>();
            var casePriorityEntity = casePriorityDtoAdapter.MapToType(retentionAlertCasePriority);

            casePriorityEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync(caseId, casePriorityEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }


        /// <summary>
        /// Close a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseClose"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> CloseRetentionAlertCaseAsync(string caseId, RetentionAlertWorkCaseClose retentionAlertCaseClose)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to close a Case.");
            }
            if (retentionAlertCaseClose == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retention Alert Case Action information must be specified to close a case.");
            }
            
            await CheckforWorkCasePermission();

            var closeCaseDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseClose, Domain.Base.Entities.RetentionAlertWorkCaseClose>();
            var closeCaseEntity = closeCaseDtoAdapter.MapToType(retentionAlertCaseClose);

            closeCaseEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.CloseRetentionAlertCaseAsync(caseId, closeCaseEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        /// <summary>
        /// Gets the retention alert case detail.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        /// <returns>Retention alert case detail</returns>
        /// <exception cref="PermissionsException"></exception>
        public async Task<RetentionAlertCaseDetail> GetRetentionAlertCaseDetailAsync(string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "case id must be specified to get retention alert case detail.");
            }
            
            await CheckforRAPermissionsAsync();

            var caseDetail = await _retentionAlertRepository.GetRetentionAlertCaseDetailAsync(caseId);

            // Get the right adapter for the type mapping
            var caseDetailDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertCaseDetail, RetentionAlertCaseDetail>();
            return caseDetailDtoAdapter.MapToType(caseDetail);
        }

        /// <summary>
        /// Sends a mail for the Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> SendRetentionAlertWorkCaseMailAsync(string caseId, RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to add a Type to a Case.");
            }
            if (retentionAlertWorkCaseSendMail == null)
            {
                throw new ArgumentNullException("retentionAlertWorkCaseSendMail", "retention Alert Mailing information must be specified to send mail for the case.");
            }
            
            await CheckforWorkCasePermission();

            var caseSendMailDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseSendMail, Domain.Base.Entities.RetentionAlertWorkCaseSendMail>();
            var caseSendMailEntity = caseSendMailDtoAdapter.MapToType(retentionAlertWorkCaseSendMail);

            caseSendMailEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.SendRetentionAlertWorkCaseMailAsync(caseId, caseSendMailEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);
        }

        // User cannot proceed without RA permissions
        protected async Task CheckforRAPermissionsAsync()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(RetentionAlertPermissionCodes.WorkAnyCase) || userPermissions.Contains(RetentionAlertPermissionCodes.WorkCases) || userPermissions.Contains(RetentionAlertPermissionCodes.ContributeToCases))
            {
                return;
            }
            throw new PermissionsException("User does not have permission to perform this operation");
        }

        // User cannot proceed without WorkAnyCase or WorkCases permissions
        protected async Task CheckforWorkCasePermission()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(RetentionAlertPermissionCodes.WorkAnyCase) || userPermissions.Contains(RetentionAlertPermissionCodes.WorkCases))
            {
                return;
            }
            throw new PermissionsException("User does not have permission to perform this operation");
        }

        // User cannot proceed without WorkAnyCase permissions
        protected async Task CheckforReportsPermission()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(RetentionAlertPermissionCodes.WorkAnyCase))
            {
                return;
            }
            throw new PermissionsException("User does not have permission to perform this operation");
        }

        /// <summary>
        /// Reassigns the retention alert work case
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="retentionAlertWorkCaseReassign"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> ReassignRetentionAlertWorkCaseAsync(string caseId, RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to reassign a Case.");
            }
            if (retentionAlertWorkCaseReassign == null)
            {
                throw new ArgumentNullException("retentionAlertWorkCaseReassign", "retention Alert reassignment information must be specified to reassign case.");
            }
            await CheckforWorkCasePermission();

            var caseReassignDtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseReassign, Domain.Base.Entities.RetentionAlertWorkCaseReassign>();
            var caseReassignEntity = caseReassignDtoAdapter.MapToType(retentionAlertWorkCaseReassign);

            caseReassignEntity.UpdatedBy = CurrentUser.PersonId;
            var createResponse = await _retentionAlertRepository.ReassignRetentionAlertWorkCaseAsync(caseId, caseReassignEntity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(createResponse);

        }

        /// <summary>
        /// Set a Case Reminder Date
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="reminder"></param>
        /// <returns>Retention alert work case action response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseReminderAsync(string caseId, RetentionAlertWorkCaseSetReminder reminder)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to reassign a Case.");
            }
            if (reminder == null)
            {
                throw new ArgumentNullException("reminder", "retention Alert set reminder information must be specified to reassign case.");
            }
            await CheckforWorkCasePermission();

            var dtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseSetReminder, Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>();
            var entity = dtoAdapter.MapToType(reminder);

            entity.UpdatedBy = CurrentUser.PersonId;
            var response = await _retentionAlertRepository.AddRetentionAlertCaseReminderAsync(caseId, entity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(response);
        }

        public async Task<RetentionAlertWorkCaseActionResponse> ManageRetentionAlertCaseRemindersAsync(string caseId, RetentionAlertWorkCaseManageReminders reminders)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case ID is required to reassign a Case.");
            }
            if (reminders == null)
            {
                throw new ArgumentNullException("reminders", "Reminder Information must be provided to manage retention alert reminder dates.");
            }
            await CheckforWorkCasePermission();

            var dtoAdapter = _adapterRegistry.GetAdapter<RetentionAlertWorkCaseManageReminders, Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>();
            var entity = dtoAdapter.MapToType(reminders);

            entity.UpdatedBy = CurrentUser.PersonId;
            var response = await _retentionAlertRepository.ManageRetentionAlertCaseRemindersAsync(caseId, entity);

            var retentionAlertDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, RetentionAlertWorkCaseActionResponse>();
            return retentionAlertDtoAdapter.MapToType(response);
        }

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="caseCategoryId">Retention Alert Case Category Id</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that category</returns> 
        public async Task<RetentionAlertGroupOfCasesSummary> GetRetentionAlertCaseOwnerSummaryAsync(string caseCategoryId)
        {
            if (string.IsNullOrEmpty(caseCategoryId))
            {
                throw new ArgumentNullException("caseCategoryIds", "caseCategoryIds are required to get a Category Summary.");
            }

            await CheckforReportsPermission();

            var resp = await _retentionAlertRepository.GetRetentionAlertCaseOwnerSummaryAsync(caseCategoryId);

            // Get the right adapter for the type mapping
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, RetentionAlertGroupOfCasesSummary>();
            return dtoAdapter.MapToType(resp);
        }

        /// <summary>
        /// Set the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <param name="sendEmailPreference"></param>
        /// <returns>Retention Alert Send Email Preference</returns>
        public async Task<RetentionAlertSendEmailPreference> SetRetentionAlertEmailPreferenceAsync(string orgEntityId, RetentionAlertSendEmailPreference sendEmailPreference)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                throw new ArgumentNullException("orgEntityId", "orgEntityId is required.");
            }
            if (sendEmailPreference == null)
            {
                throw new ArgumentNullException("sendEmailPreference", "sendemailPreference is required.");
            }

            await CheckforWorkCasePermission();

            var adapter = _adapterRegistry.GetAdapter<RetentionAlertSendEmailPreference, Domain.Base.Entities.RetentionAlertSendEmailPreference>();
            var entity = adapter.MapToType(sendEmailPreference);

            var response = await _retentionAlertRepository.SetRetentionAlertEmailPreferenceAsync(orgEntityId, entity);

            // Get the right adapter for the mapping
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertSendEmailPreference, RetentionAlertSendEmailPreference>();
            return dtoAdapter.MapToType(response);
        }

        /// <summary>
        /// Get the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        public async Task<RetentionAlertSendEmailPreference> GetRetentionAlertEmailPreferenceAsync(string orgEntityId)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                throw new ArgumentNullException("orgEntityId", "orgEntityId is required.");
            }

            await CheckforWorkCasePermission();

            var response = await _retentionAlertRepository.GetRetentionAlertEmailPreferenceAsync(orgEntityId);

            // Get the right adapter for the mapping
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RetentionAlertSendEmailPreference, RetentionAlertSendEmailPreference>();
            return dtoAdapter.MapToType(response);
        }
    }
}
