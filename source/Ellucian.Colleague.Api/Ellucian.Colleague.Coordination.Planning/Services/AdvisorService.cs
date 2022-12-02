// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Planning.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    [RegisterType]
    public class AdvisorService : StudentCoordinationService, IAdvisorService
    {
        private readonly IDegreePlanRepository _degreePlanRepository;
        private readonly IStudentDegreePlanRepository _studentDegreePlanRepository;
        private readonly IAdvisorRepository _advisorRepository;
        private readonly ITermRepository _termRepository;
        private readonly IAdviseeRepository _adviseeRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPersonBaseRepository _personBaseRepository;

        public AdvisorService(IAdapterRegistry adapterRegistry, IAdvisorRepository advisorRepository, IDegreePlanRepository degreePlanRepository, 
            ITermRepository termRepository, IAdviseeRepository adviseeRepository, IStudentConfigurationRepository studentConfigurationRepository, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository, 
            IStaffRepository staffRepository, IConfigurationRepository configurationRepository, IPersonBaseRepository personBaseRepository,
            IStudentDegreePlanRepository studentDegreePlanRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            _configurationRepository = configurationRepository;
            _degreePlanRepository = degreePlanRepository;
            _studentDegreePlanRepository = studentDegreePlanRepository;
            _advisorRepository = advisorRepository;
            _termRepository = termRepository;
            _adviseeRepository = adviseeRepository;
            _studentConfigurationRepository = studentConfigurationRepository;
            _studentRepository = studentRepository;
            _personBaseRepository = personBaseRepository;
        }

        /// <summary>
        /// Intended only to return name, email and ID of a current or former Faculty or Staff Advisor.
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// </summary>
        /// <param name="advisorId">Id of the advisor/staff to get</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Planning.Advisor">Advisor</see> object</returns>
        public async Task<Dtos.Planning.Advisor> GetAdvisorAsync(string advisorId)
        {
            var advisorDto = new Dtos.Planning.Advisor();
            Domain.Planning.Entities.Advisor advisorEntity;
            try
            {
                // No need to return advisees for the advisors - only need name email info.
                advisorEntity = await _advisorRepository.GetAsync(advisorId, AdviseeInclusionType.NoAdvisees);
                // Get the custom adapter

                advisorDto = await BuildAdvisorDtoAsync(advisorEntity);
            }
            catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception)
            {
                logger.Error("Advisor Id " + advisorId + " is neither a Faculty nor a Staff");
                var message = "Cannot retrieve information for Advisor Id " + advisorId;
                throw new KeyNotFoundException(message);
            }

            return advisorDto;
        }

        /// <summary>
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve merely reference information (name, email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// </summary>
        /// <param name="advisorQueryCriteria">Criteria of the advisors to retrieve</param>
        /// <returns>A list of <see cref="Advisor">Advisors</see> object containing advisor name</returns>
        [Obsolete("Obsolete as API 1.19, user QueryAdvisorsByPostAsync instead.")]
        public async Task<IEnumerable<Dtos.Planning.Advisor>> GetAdvisorsAsync(Dtos.Planning.AdvisorQueryCriteria advisorQueryCriteria)
        {
            var advisorDtos = new List<Dtos.Planning.Advisor>();

            IEnumerable<Domain.Planning.Entities.Advisor> advisorEntities;

            try
            {
                if (advisorQueryCriteria.OnlyActiveAdvisees)
                {
                    advisorEntities = await _advisorRepository.GetAdvisorsAsync(advisorQueryCriteria.AdvisorIds, AdviseeInclusionType.ExcludeFormerAdvisees);
                }
                else
                {
                    advisorEntities = await _advisorRepository.GetAdvisorsAsync(advisorQueryCriteria.AdvisorIds, AdviseeInclusionType.AllAdvisees);
                }
                foreach (var advisorEntity in advisorEntities)
                {
                    advisorDtos.Add(await BuildAdvisorDtoAsync(advisorEntity));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving data for specified advisors. " + ex.Message);
                var message = "Cannot retrieve information for specified advisors";
                throw new ColleagueWebApiException(message);
            }

            return advisorDtos;
        }

        /// <summary>
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve merely reference information (name, email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// </summary>
        /// <param name="advisorIds">Advisor IDs for whom data will be retrieved</param>
        /// <returns>A list of <see cref="Advisor">Advisors</see> object containing advisor name</returns>
        public async Task<IEnumerable<Dtos.Planning.Advisor>> QueryAdvisorsByPostAsync(IEnumerable<string> advisorIds)
        {
            if (advisorIds == null || !advisorIds.Any())
            {
                throw new ArgumentNullException("advisorIds", "At least one advisor ID must be provided to search for advisors.");
            }
            try
            { 
                IEnumerable<Dtos.Planning.Advisor> advisorDtos;
                IEnumerable<Domain.Planning.Entities.Advisor> advisorEntities;

                advisorEntities = await _advisorRepository.GetAdvisorsAsync(advisorIds, AdviseeInclusionType.NoAdvisees);
                advisorDtos = await BuildAdvisorDtosAsync(advisorEntities);
                return advisorDtos;
            }
            catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving data for specified advisors. " + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// Verifies that the Advisor with the given ID is a current advisor with all the permissions 
        /// and setup required to function as an Advisor.
        /// </summary>
        /// <param name="advisorId">Id of advisor</param>
        /// <param name="adviseeInclusionType">This indicates which types of assigned advisees are to be returned.</param>
        /// <returns>Advisor object</returns>
        private async Task<Domain.Planning.Entities.Advisor> GetAuthorizedAdvisorAsync(string advisorId, AdviseeInclusionType adviseeInclusionType = AdviseeInclusionType.AllAdvisees)
        {
            Domain.Planning.Entities.Advisor advisorEntity = null;

            // Make sure current user is the specified advisor
            if (CurrentUser.PersonId != advisorId)
            {
                throw new ColleagueWebApiException("Requested advisor " + advisorId + " is not the current user.");
            }

            // Either Faculty Advisor or Staff, cannot proceed without at least "view" permissions
            await CheckViewAdviseePermissionsAsync();

            try
            {
                // Specifying which type of assigned advisees are to be returned. 
                advisorEntity = await _advisorRepository.GetAsync(CurrentUser.PersonId, adviseeInclusionType);
            }
            catch (KeyNotFoundException)
            {
                // This catches the repository exception for a missing person.
                var message = "ID " + advisorId + " is not a Faculty Advisor or Staff Advisor.";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // Only active advisors are authorized
            if (!advisorEntity.IsActive)
            {
                var message = "ID " + advisorId + " is not an active Faculty Advisor or Staff Advisor.";
                logger.Error(message);
                throw new PermissionsException(message);

            }

            // If the advisor has no assigned advisees make sure they at least have ViewAnyAdvisee permission,
            // otherwise they are not authorized.
            if (advisorEntity.Advisees.Count() == 0)
            {
                if (!await CanViewAnyAdviseeAsync())
                {
                    throw new PermissionsException("Staff is not authorized to be an Advisor.");
                }
            }

            return advisorEntity;
        }


        /// <summary>
        /// Returns a list of Advisee Dtos for the advisees assigned to this advisor.
        /// </summary>
        /// <param name="advisorId"></param>
        /// <returns>List of Advisee objects</returns>
        public async Task<PrivacyWrapper<List<Dtos.Planning.Advisee>>> GetAdviseesAsync(string advisorId, int pageSize, int pageIndex, bool activeAdviseesOnly = false )
        {
            logger.Info("STEPPING through advisorservice GetAdvisees... " + DateTime.Now);

            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            var watch = new Stopwatch();

            //STEP1 ===================================================
            watch.Restart();

            var advisees = new PrivacyWrapper<List<Dtos.Planning.Advisee>>(new List<Dtos.Planning.Advisee>(), false);
            Advisor advisorEntity;
            // This method returns the advisor entity, or throws an exception if the advisor does not have needed permissions
            if (activeAdviseesOnly)
            {
                advisorEntity = await GetAuthorizedAdvisorAsync(advisorId, AdviseeInclusionType.CurrentAdviseesOnly);
            }
            else
            {
                advisorEntity = await GetAuthorizedAdvisorAsync(advisorId, AdviseeInclusionType.AllAdvisees);
            }


            //===================================================
            watch.Stop();
            logger.Info("STEP1 AdvisorRepository Get... completed in " + watch.ElapsedMilliseconds.ToString());

            // Get advisee degree plan information only if there are advisees present
            if (advisorEntity.Advisees != null && advisorEntity.Advisees.Count() > 0)
            {
                //STEP2 ===================================================
                watch.Restart();

                // Using the advisor's list of assigned advisees - return those that are planning students using the Advisee Repository GetAsync command.  
                // This will only return planning students for those that are students.
                var adviseeStudentEntities = await _adviseeRepository.GetAsync(advisorEntity.Advisees, pageSize, pageIndex);

                //===================================================
                watch.Stop();
                logger.Info("STEP2 AdviseeRepository Get... completed in " + watch.ElapsedMilliseconds.ToString());

                //STEP3 ===================================================
                watch.Restart();

                var adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(adviseeStudentEntities.Select(s => s.Id));

                //===================================================
                watch.Stop();
                logger.Info("STEP3 DegreePlanRepository Get... completed in " + watch.ElapsedMilliseconds.ToString());

                advisees = BuildAdviseeDtos(adviseeDegreePlans, adviseeStudentEntities, advisorEntity.Advisees);
            }
            return advisees;
        }

        /// <summary>
        /// Returns an Advisee Dtos for the advisee assigned to this advisor.
        /// </summary>
        /// <param name="advisorId"></param>
        /// <returns>List of Advisee objects</returns>
        public async Task<PrivacyWrapper<Dtos.Planning.Advisee>> GetAdviseeAsync(string advisorId, string adviseeId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId");
            }
            if (string.IsNullOrEmpty(adviseeId))
            {
                throw new ArgumentNullException("adviseeId");
            }

            Dtos.Planning.Advisee advisee = new Dtos.Planning.Advisee();

            // Returns advisor entity complete with all advisees former and future, or throws an error if the advisor does not have permissions
            var advisorEntity = await GetAuthorizedAdvisorAsync(advisorId);

            var assignedAdvisees = advisorEntity.Advisees;
            if (!(await CanViewAnyAdviseeAsync()) && (!assignedAdvisees.Contains(adviseeId)))
            {
                throw new PermissionsException("User does not have permission to view the requested advisee");
            }
            var adviseeStudentEntity = await _adviseeRepository.GetAsync(adviseeId);
            if (adviseeStudentEntity == null)
            {
                throw new KeyNotFoundException("Advisee with this Id is not found.");
            }
            var adviseeDegreePlans = new List<Domain.Student.Entities.DegreePlans.DegreePlan>();
            if (adviseeStudentEntity.DegreePlanId != null)
            {
                var adviseeDegreePlan = await _studentDegreePlanRepository.GetAsync(adviseeStudentEntity.DegreePlanId.GetValueOrDefault(0));
                adviseeDegreePlans.Add(adviseeDegreePlan);
            }

            // This method builds a privacy wrapper with a list of advisees. Convert to a privacy wrapper with a single advisee.
            var privacyWrapperWithList = BuildAdviseeDtos(adviseeDegreePlans, new List<Domain.Student.Entities.PlanningStudent>() { adviseeStudentEntity }, advisorEntity.Advisees);
            var adviseeDto = privacyWrapperWithList.Dto.FirstOrDefault();
            var privacyWrapper = new PrivacyWrapper<Dtos.Planning.Advisee>(adviseeDto, privacyWrapperWithList.HasPrivacyRestrictions);
            return privacyWrapper;
        }

        /// <summary>
        /// OBSOLETE - see Search2()
        /// Process the string handed from UI to search for students. Expected possible inputs:
        ///    ID (checks student file, throws error if not valid, returns if valid student)
        ///    last, first
        ///    last, first middle (anything more ignored)
        ///    last
        ///    first last
        ///    first middle last (anything more ignored)
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>A list of student Ids</returns>
        [Obsolete("Obsolete as of API version 1.2. Use Search3 instead.")]
        public async Task<IEnumerable<string>> SearchAsync(string searchString, int pageSize, int pageIndex)
        {
            if (!(await GetUserPermissionCodesAsync()).Contains(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                throw new PermissionsException("User does not have permission to search for unassigned advisees");
            }
            if (searchString == null || (searchString.Trim().Length < 2))
            {
                throw new ArgumentException("A search string of at least two characters must be supplied", "searchString");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            // If search string is a numeric ID, return only that
            double studentId;
            bool isId = double.TryParse(searchString, out studentId);
            if (isId == true)
            {
                // Validate the ID - if invalid, error will be thrown
                var student = await _adviseeRepository.GetAsync(searchString);
                // If valid, return the ID. If not found, return null
                if (student == null)
                {
                    return new List<string>() { };
                }
                else
                {
                    return new List<string>() { searchString };
                }
            }

            // Otherwise, parse the search string into name parts
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
            // Remove all spaces
            // Call repository method to query students based on name strings
            return (await _adviseeRepository.SearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex)).Select(s => s.Id).ToList();
        }

        /// <summary>
        /// Process the string handed from UI to search for students. Expected possible inputs:
        ///    ID (checks student file, throws error if not valid, returns if valid student)
        ///    last, first
        ///    last, first middle (anything more ignored)
        ///    last
        ///    first last
        ///    first middle last (anything more ignored)
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>A set of advisees</returns>
        [Obsolete("Obsolete as of API version 1.5. Use Search3 instead.")]
        public async Task<IEnumerable<Dtos.Planning.Advisee>> Search2Async(string searchString, int pageSize, int pageIndex)
        {
            //STEP1 ===================================================
            logger.Info("STEPPING through AdvisorService Search2... " + DateTime.Now);
            var watch = new Stopwatch();
            watch.Start();

            // If the user is limited to his own advisees, get their IDs so we can limit the return set.
            // In this version advisees are not limited to active.
            var advisor = await GetAuthorizedAdvisorAsync(CurrentUser.PersonId);

            ////===================================================
            watch.Stop();
            logger.Info("STEP1 GetAdvisorWithPermissions... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP 2 ===================================================
            watch.Restart();

            if (searchString == null || (searchString.Trim().Length < 2))
            {
                throw new ArgumentException("A search string of at least two characters must be supplied", "searchString");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;


            //===================================================
            watch.Stop();
            logger.Info("STEP2 AdvisorRepositoryGet... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP3 ===================================================
            watch.Restart();

            List<Domain.Student.Entities.PlanningStudent> assignedAdvisees = new List<Domain.Student.Entities.PlanningStudent>();
            if (advisor.Advisees.Count() > 0)
            {
                assignedAdvisees.AddRange(await _adviseeRepository.GetAsync(advisor.Advisees, pageSize, pageIndex));
            }

            //===================================================
            watch.Stop();
            logger.Info("STEP3 GetAssignedAdvisees... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP4 ===================================================
            watch.Restart();

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            var privacyRestriction = false;

            // If search string is a numeric ID, return only that
            double studentId;
            bool isId = double.TryParse(searchString, out studentId);
            if (isId == true)
            {
                // If the requested ID is not an assigned advisee but the advisor can only view assigned advisees, then return an empty list
                if (!(await CanViewAnyAdviseeAsync()) && (!assignedAdvisees.Select(x => x.Id).Contains(searchString)))
                {
                    return new List<Dtos.Planning.Advisee>() { };
                }

                // Validate the ID - if invalid, error will be thrown
                var student = await _adviseeRepository.GetAsync(searchString);
                // If valid, return the ID. If not found, return null
                if (student == null)
                {
                    return new List<Dtos.Planning.Advisee>() { };
                }
                else
                {
                    var adviseeList = new List<Domain.Student.Entities.PlanningStudent>() { student };
                    var adviseeDegreePlansList = await _studentDegreePlanRepository.GetAsync(adviseeList.Select(s => s.Id));

                    var advisee = BuildAdviseeDtos(adviseeDegreePlansList, adviseeList, advisor.Advisees);
                    return advisee.Dto as List<Dtos.Planning.Advisee>;
                }
            }

            // Otherwise, parse the search string into name parts
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
            //===================================================
            watch.Stop();
            logger.Info("STEP4 ParseSearchString... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP5 ===================================================
            watch.Restart();

            // Call repository method to query students based on name strings
            IEnumerable<Domain.Student.Entities.PlanningStudent> advisees = null;
            var canViewAnyAdvisee = await CanViewAnyAdviseeAsync();
            if (!canViewAnyAdvisee && advisor.Advisees.Count() > 0)
            {
                advisees = (await _adviseeRepository.SearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex, advisor.Advisees)).ToList();
            }
            else if (canViewAnyAdvisee)
            {
                advisees = (await _adviseeRepository.SearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex, null)).ToList();
            }
            else
            {
                advisees = new List<Domain.Student.Entities.PlanningStudent>();
            }
            //===================================================
            watch.Stop();
            logger.Info("STEP5 GetAdviseesWithSearchString... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP6 ===================================================
            watch.Restart();

            var adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(advisees.Select(s => s.Id));

            //===================================================
            watch.Stop();
            logger.Info("STEP6 GetDegreePlans... completed in " + watch.ElapsedMilliseconds.ToString());

            //STEP7 ===================================================
            watch.Restart();

            var adviseeDtos = BuildAdviseeDtos(adviseeDegreePlans, advisees, advisor.Advisees);

            //===================================================
            watch.Stop();
            logger.Info("STEP7 BuildAdviseeDtos... completed in " + watch.ElapsedMilliseconds.ToString());

            return adviseeDtos.Dto as List<Dtos.Planning.Advisee>;
        }

        // Create Advisor Dto to return
        private PrivacyWrapper<List<Dtos.Planning.Advisee>> BuildAdviseeDtos(
            IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan> degreePlans, 
            IEnumerable<Domain.Student.Entities.PlanningStudent> adviseeStudentEntities, IEnumerable<string> assignedAdvisees)
        {
            var hasPrivacyRestriction = false; // Default to false, check later for each advisee

            List<Dtos.Planning.Advisee> adviseeDtos = new List<Dtos.Planning.Advisee>();
            var adviseeAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>();
            // loop through each advisee id
            foreach (var advisee in adviseeStudentEntities)
            {
                try
                {
                    // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                    // against any privacy code on the student's record
                    var adviseeHasPrivacyRestriction = string.IsNullOrEmpty(advisee.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(advisee.PrivacyStatusCode);

                    Dtos.Planning.Advisee adviseeDto;

                    // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                    // then blank out the record, except for name, id, and privacy code
                    if (adviseeHasPrivacyRestriction)
                    {
                        hasPrivacyRestriction = true;
                        adviseeDto = new Dtos.Planning.Advisee()
                        {
                            LastName = advisee.LastName,
                            FirstName = advisee.FirstName,
                            MiddleName = advisee.MiddleName,
                            Id = advisee.Id,
                            PrivacyStatusCode = advisee.PrivacyStatusCode,
                            PhoneTypesHierarchy = advisee.PhoneTypesHierarchy
                        };
                    }
                    else
                    {
                        adviseeDto = adviseeAdapter.MapToType(advisee);
                        // get advisee's degree plan - if more than one are returned, always take the one with the lowest Id for consistency.
                        var dp = degreePlans.Where(d => d.PersonId == advisee.Id).OrderBy(d => d.Id).FirstOrDefault();
                        if (dp != null)
                        {
                            adviseeDto.DegreePlanId = dp.Id;      // Just in case they don't match...
                                                                  // Add approval requested indicator
                            adviseeDto.ApprovalRequested = dp.ReviewRequested;
                        }
                        else
                        {
                            adviseeDto.DegreePlanId = null;
                        }
                        // If the advisee is in the list of assignedAdviseeIds (ie, assigned to the current advisor using the system), set the flag
                        if (assignedAdvisees.Contains(adviseeDto.Id))
                        {
                            adviseeDto.IsAdvisee = true;
                        }
                        else
                        {
                            adviseeDto.IsAdvisee = false;
                        }
                        if (advisee.PreferredEmailAddress != null)
                        {
                            adviseeDto.PreferredEmailAddress = advisee.PreferredEmailAddress.Value;
                        }
                        if (advisee.EmailAddresses !=null)
                        {
                            if (advisee.EmailAddresses.Any())
                            {
                                adviseeDto.EmailAddresses = new List<EmailAddress>();
                                foreach (var email in advisee.EmailAddresses)
                                {
                                    EmailAddress emailAddress = new EmailAddress()
                                    {
                                        Value = email.Value,
                                        TypeCode = email.TypeCode,
                                        IsPreferred = email.IsPreferred
                                    };
                                    adviseeDto.EmailAddresses.Add(emailAddress);
                                }
                            }
                        }

                    }
                    adviseeDtos.Add(adviseeDto);
                }
                catch(Exception ex)
                {
                    logger.Error("Failed to build advisee dto for advisee - " + advisee.Id);
                    logger.Error(ex, ex.Message);
                }
            }

            return new PrivacyWrapper<List<Dtos.Planning.Advisee>>(adviseeDtos, hasPrivacyRestriction);
        }

        // Create Advisor Dto to return
        private PrivacyWrapper<Dtos.Planning.Advisee> BuildAdviseeDto(Domain.Student.Entities.PlanningStudent adviseeStudentEntity, IEnumerable<string> assignedAdvisees)
        {
            var hasPrivacyRestriction = false; // Default to false, check later for each advisee

            List<Dtos.Planning.Advisee> adviseeDtos = new List<Dtos.Planning.Advisee>();
            var adviseeAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>();
            var completedAdvisementsAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.CompletedAdvisement, Dtos.Student.CompletedAdvisement>();

            // Before doing anything, check the current advisor's privacy code settings (on their staff record)
            // against any privacy code on the student's record
            var adviseeHasPrivacyRestriction = string.IsNullOrEmpty(adviseeStudentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(adviseeStudentEntity.PrivacyStatusCode);

            Dtos.Planning.Advisee adviseeDto;

            // If a privacy restriction exists (staff record doesn't contain student's privacy code)
            // then blank out the record, except for name, id, privacy code, and completed advisements
            if (adviseeHasPrivacyRestriction)
            {
                hasPrivacyRestriction = true;
                adviseeDto = new Dtos.Planning.Advisee()
                {
                    LastName = adviseeStudentEntity.LastName,
                    FirstName = adviseeStudentEntity.FirstName,
                    MiddleName = adviseeStudentEntity.MiddleName,
                    Id = adviseeStudentEntity.Id,
                    PrivacyStatusCode = adviseeStudentEntity.PrivacyStatusCode,
                    PhoneTypesHierarchy = adviseeStudentEntity.PhoneTypesHierarchy
                };
                if (adviseeStudentEntity.CompletedAdvisements != null && adviseeStudentEntity.CompletedAdvisements.Any())
                {
                    List<Dtos.Student.CompletedAdvisement> completedAdvisements = new List<Dtos.Student.CompletedAdvisement>();
                    foreach(var completedAdvisement in adviseeStudentEntity.CompletedAdvisements)
                    {
                        completedAdvisements.Add(completedAdvisementsAdapter.MapToType(completedAdvisement));
                    }
                    adviseeDto.CompletedAdvisements = completedAdvisements;
                }
            }
            else
            {
                adviseeDto = adviseeAdapter.MapToType(adviseeStudentEntity);
                // If the advisee is in the list of assignedAdviseeIds (ie, assigned to the current advisor using the system), set the flag
                if (assignedAdvisees.Contains(adviseeDto.Id))
                {
                    adviseeDto.IsAdvisee = true;
                }
                else
                {
                    adviseeDto.IsAdvisee = false;
                }
                if (adviseeStudentEntity.PreferredEmailAddress != null)
                {
                    adviseeDto.PreferredEmailAddress = adviseeStudentEntity.PreferredEmailAddress.Value;
                }
            }
            adviseeDtos.Add(adviseeDto);

            return new PrivacyWrapper<Dtos.Planning.Advisee>(adviseeDto, hasPrivacyRestriction);
        }

        /// <summary>
        /// Returns the advisor specific permissions pertinent to this user.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of Colleague Web API 1.21. Use GetAdvisingPermissions2Async")]
        public async Task<IEnumerable<string>> GetAdvisorPermissionsAsync()
        {
            var permissions = new List<string>();
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAssignedAdvisees))
            {
                permissions.Add(PlanningPermissionCodes.ViewAssignedAdvisees);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                permissions.Add(PlanningPermissionCodes.ViewAnyAdvisee);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.UpdateAdviseeDegreePlan))
            {
                permissions.Add(PlanningPermissionCodes.UpdateAdviseeDegreePlan);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAdviseeDegreePlan))
            {
                permissions.Add(PlanningPermissionCodes.ViewAdviseeDegreePlan);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee))
            {
                permissions.Add(PlanningPermissionCodes.ReviewAnyAdvisee);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees))
            {
                permissions.Add(PlanningPermissionCodes.ReviewAssignedAdvisees);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee))
            {
                permissions.Add(PlanningPermissionCodes.UpdateAnyAdvisee);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees))
            {
                permissions.Add(PlanningPermissionCodes.UpdateAssignedAdvisees);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee))
            {
                permissions.Add(PlanningPermissionCodes.AllAccessAnyAdvisee);
            }
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees))
            {
                permissions.Add(PlanningPermissionCodes.AllAccessAssignedAdvisees);
            }
            return permissions;

        }

        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Advising permissions for the authenticated user.</returns>
        public async Task<Dtos.Planning.AdvisingPermissions> GetAdvisingPermissions2Async()
        {
            try
            {
                IEnumerable<string> permissionCodes = await GetUserPermissionCodesAsync();
                AdvisingPermissions permissionsEntity = new AdvisingPermissions(permissionCodes, logger);
                ITypeAdapter<AdvisingPermissions, Dtos.Planning.AdvisingPermissions> entityToDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Planning.Entities.AdvisingPermissions, Dtos.Planning.AdvisingPermissions>();
                Dtos.Planning.AdvisingPermissions permissionsDto = entityToDtoAdapter.MapToType(permissionsEntity);
                return permissionsDto;
            }
            catch (Exception ex)
            {
                if (CurrentUser != null)
                {
                    logger.Error(ex, string.Format("An error occurred while retrieving user {0}'s advising permissions.", CurrentUser.PersonId));
                }
                else
                {
                    logger.Error(ex, "An error occurred while retrieving advising permissions.");
                }
                throw new ApplicationException("An error occurred while retrieving advising permissions.");
            }
        }

        /// <summary>
        /// Process the criteria handed from UI to search for students. The criteria must have either an AdviseeName or AdvisorName. 
        /// 
        /// Expected possible inputs for the name values:
        ///    ID (checks student file, throws error if not valid, returns if valid student)
        ///    last, first
        ///    last, first middle (anything more ignored)
        ///    last
        ///    first last
        ///    first middle last (anything more ignored)
        /// </summary>
        /// <param name="criteria">Criteria which describes how the search is to be performed by name or advisor name.</param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="activeAdviseesOnly">If the result set should be limited to active advisees (not former or future) for advisors with assigned advisee access only.</param>
        /// <returns>A set of advisees</returns>
        public async Task<PrivacyWrapper<List<Dtos.Planning.Advisee>>> Search3Async(Dtos.Planning.AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            var hasPrivacyRestriction = false; // Default to false, will be set properly when retreiving advisees

            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "A search criteria object must be supplied");
            }
            if ((string.IsNullOrEmpty(criteria.AdviseeKeyword) && string.IsNullOrEmpty(criteria.AdvisorKeyword)) || (!string.IsNullOrEmpty(criteria.AdviseeKeyword) && !string.IsNullOrEmpty(criteria.AdvisorKeyword)))
            {
                throw new ArgumentException("Search criteria must contain either an Advisee name or an Advisor name but not both.", "criteria");
            }
            if ((!string.IsNullOrEmpty(criteria.AdviseeKeyword) && criteria.AdviseeKeyword.Trim().Length < 2) || (!string.IsNullOrEmpty(criteria.AdvisorKeyword) && criteria.AdvisorKeyword.Trim().Length < 2))
            {
                throw new ArgumentException("A search string of at least two characters must be supplied for either advisee name or advisor name.", "criteria");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            logger.Info("STEPPING through AdvisorService Search3... " + DateTime.Now);

            ///////////////////////  STEP1: Get authorized advisor /////////////////////////////////////////////

            var watch = new Stopwatch();
            watch.Start();

            // If the user is limited to his own advisees, get their IDs so we can limit the return set.
            Advisor advisor;
            if (criteria.IncludeActiveAdviseesOnly)
            {
                advisor = await GetAuthorizedAdvisorAsync(CurrentUser.PersonId, AdviseeInclusionType.CurrentAdviseesOnly);
            } else
            {
                advisor = await GetAuthorizedAdvisorAsync(CurrentUser.PersonId, AdviseeInclusionType.AllAdvisees);
            }


            ////===================================================
            watch.Stop();
            logger.Info("GetAdvisorWithPermissions... completed in " + watch.ElapsedMilliseconds.ToString());

            ///////////////////////  STEP2: Get Advisor's assigned advisees  ////////////////////////////////////
            watch.Restart();

            List<Domain.Student.Entities.PlanningStudent> assignedAdvisees = new List<Domain.Student.Entities.PlanningStudent>();
            if (advisor.Advisees.Count() > 0)
            {
                assignedAdvisees.AddRange(await _adviseeRepository.GetAsync(advisor.Advisees, pageSize, pageIndex));
            }

            //===================================================
            watch.Stop();
            logger.Info("GetAssignedAdvisees... completed in " + watch.ElapsedMilliseconds.ToString());

            string searchString = !string.IsNullOrEmpty(criteria.AdviseeKeyword) ? criteria.AdviseeKeyword : criteria.AdvisorKeyword;

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            // If search string is a numeric ID and it is for a particular advisee, return only that
            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(criteria.AdviseeKeyword))
            {
                // If the requested ID is not an assigned advisee but the advisor can only view assigned advisees, then return an empty list
                if (!(await CanViewAnyAdviseeAsync()) && (!advisor.Advisees.Contains(searchString)))
                {
                    return new PrivacyWrapper<List<Dtos.Planning.Advisee>>(new List<Dtos.Planning.Advisee>(), hasPrivacyRestriction);
                }

                // Validate the ID - if invalid, error will be thrown
                var student = await _adviseeRepository.GetAsync(searchString);
                // If valid, return the ID. If not found, return null
                if (student == null)
                {
                    return new PrivacyWrapper<List<Dtos.Planning.Advisee>>(new List<Dtos.Planning.Advisee>(), hasPrivacyRestriction);
                }
                else
                {
                    var adviseeList = new List<Domain.Student.Entities.PlanningStudent>() { student };
                    var adviseeDegreePlansList = await _studentDegreePlanRepository.GetAsync(adviseeList.Select(s => s.Id));

                    var advisee = BuildAdviseeDtos(adviseeDegreePlansList, adviseeList, advisor.Advisees);
                    return advisee;
                }
            }
            IEnumerable<Domain.Student.Entities.PlanningStudent> advisees = null;
            IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan> adviseeDegreePlans = null;
            PrivacyWrapper<List<Dtos.Planning.Advisee>> privacyWrapper = null;

            // If we have just an advisor ID then select the advisees based on that
            if (isId && !string.IsNullOrEmpty(criteria.AdvisorKeyword))
            {

                IEnumerable<string> advisorIds = new List<string>() { searchString };
                // Do the search based on advisor's permissions
                if (!(await CanViewAnyAdviseeAsync()) && advisor.Advisees.Count() > 0)
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (await CanViewAnyAdviseeAsync())
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
                adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(advisees.Select(s => s.Id));
                privacyWrapper = BuildAdviseeDtos(adviseeDegreePlans, advisees, advisor.Advisees);
                return privacyWrapper;
            }

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

            // Now that the search string is parsed into the appropriate pieces, do the appropriate search based on which keyword was supplied
            if (!string.IsNullOrEmpty(criteria.AdviseeKeyword))
            {

                // Advisee Name Search: Call repository method to query students based on name strings
                // Do the search based on the advisor's permissions
                var canViewAnyAdvisee = await CanViewAnyAdviseeAsync();
                if (!canViewAnyAdvisee && advisor.Advisees.Count() > 0)
                {

                    advisees = (await _adviseeRepository.SearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (canViewAnyAdvisee)
                {
                    advisees = (await _adviseeRepository.SearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
            }
            else
            {
                IEnumerable<string> advisorIds = await _advisorRepository.SearchAdvisorByNameAsync(lastName, firstName, middleName);

                // Do the search based on advisor's permissions - if advisorIds list is empty the repo will return an empty list of advisees
                if (!(await CanViewAnyAdviseeAsync()) && advisor.Advisees.Count() > 0)
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (await CanViewAnyAdviseeAsync())
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
            }

            watch.Restart();

            adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(advisees.Select(s => s.Id));

            //===================================================
            watch.Stop();
            logger.Info("Get Advisee Plans... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();

            privacyWrapper = BuildAdviseeDtos(adviseeDegreePlans, advisees, advisor.Advisees);

            //===================================================
            watch.Stop();
            logger.Info("Build Advisee DTOs... completed in " + watch.ElapsedMilliseconds.ToString());

            return privacyWrapper;
        }


        /// <summary>
        /// Process the criteria handed from UI to search for students. The criteria must have either an AdviseeName or AdvisorName. 
        /// 
        /// Expected possible inputs for the name values:
        ///    ID (checks student file, throws error if not valid, returns if valid student)
        ///    last, first
        ///    last, first middle (anything more ignored)
        ///    last
        ///    first last
        ///    first middle last (anything more ignored)
        /// </summary>
        /// <param name="criteria">Criteria which describes how the search is to be performed by name or advisor name.</param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="activeAdviseesOnly">If the result set should be limited to active advisees (not former or future) for advisors with assigned advisee access only.</param>
        /// <returns>A set of advisees</returns>
        public async Task<PrivacyWrapper<List<Dtos.Planning.Advisee>>> SearchForExactMatchAsync(Dtos.Planning.AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            var hasPrivacyRestriction = false; // Default to false, will be set properly when retreiving advisees

            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "A search criteria object must be supplied");
            }
            if ((string.IsNullOrEmpty(criteria.AdviseeKeyword) && string.IsNullOrEmpty(criteria.AdvisorKeyword)) || (!string.IsNullOrEmpty(criteria.AdviseeKeyword) && !string.IsNullOrEmpty(criteria.AdvisorKeyword)))
            {
                throw new ArgumentException("Search criteria must contain either an Advisee name or an Advisor name but not both.", "criteria");
            }
            if ((!string.IsNullOrEmpty(criteria.AdviseeKeyword) && criteria.AdviseeKeyword.Trim().Length < 2) || (!string.IsNullOrEmpty(criteria.AdvisorKeyword) && criteria.AdvisorKeyword.Trim().Length < 2))
            {
                throw new ArgumentException("A search string of at least two characters must be supplied for either advisee name or advisor name.", "criteria");
            }
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            logger.Info("STEPPING through AdvisorService Search3... " + DateTime.Now);

            ///////////////////////  STEP1: Get authorized advisor /////////////////////////////////////////////

            var watch = new Stopwatch();
            watch.Start();

            // If the user is limited to his own advisees, get their IDs so we can limit the return set.
            Advisor advisor;
            if (criteria.IncludeActiveAdviseesOnly)
            {
                advisor = await GetAuthorizedAdvisorAsync(CurrentUser.PersonId, AdviseeInclusionType.CurrentAdviseesOnly);
            }
            else
            {
                advisor = await GetAuthorizedAdvisorAsync(CurrentUser.PersonId, AdviseeInclusionType.AllAdvisees);
            }


            ////===================================================
            watch.Stop();
            logger.Info("GetAdvisorWithPermissions... completed in " + watch.ElapsedMilliseconds.ToString());

            ///////////////////////  STEP2: Get Advisor's assigned advisees  ////////////////////////////////////
            watch.Restart();

            List<Domain.Student.Entities.PlanningStudent> assignedAdvisees = new List<Domain.Student.Entities.PlanningStudent>();
            if (advisor.Advisees.Count() > 0)
            {
                assignedAdvisees.AddRange(await _adviseeRepository.GetAsync(advisor.Advisees, pageSize, pageIndex));
            }

            //===================================================
            watch.Stop();
            logger.Info("GetAssignedAdvisees... completed in " + watch.ElapsedMilliseconds.ToString());

            string searchString = !string.IsNullOrEmpty(criteria.AdviseeKeyword) ? criteria.AdviseeKeyword : criteria.AdvisorKeyword;

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            // If search string is a numeric ID and it is for a particular advisee, return only that
            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(criteria.AdviseeKeyword))
            {
                // If the requested ID is not an assigned advisee but the advisor can only view assigned advisees, then return an empty list
                if (!(await CanViewAnyAdviseeAsync()) && (!advisor.Advisees.Contains(searchString)))
                {
                    return new PrivacyWrapper<List<Dtos.Planning.Advisee>>(new List<Dtos.Planning.Advisee>(), hasPrivacyRestriction);
                }

                // Validate the ID - if invalid, error will be thrown
                var student = await _adviseeRepository.GetAsync(searchString);
                // If valid, return the ID. If not found, return null
                if (student == null)
                {
                    return new PrivacyWrapper<List<Dtos.Planning.Advisee>>(new List<Dtos.Planning.Advisee>(), hasPrivacyRestriction);
                }
                else
                {
                    var adviseeList = new List<Domain.Student.Entities.PlanningStudent>() { student };
                    var adviseeDegreePlansList = await _studentDegreePlanRepository.GetAsync(adviseeList.Select(s => s.Id));

                    var advisee = BuildAdviseeDtos(adviseeDegreePlansList, adviseeList, advisor.Advisees);
                    return advisee;
                }
            }
            IEnumerable<Domain.Student.Entities.PlanningStudent> advisees = null;
            IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan> adviseeDegreePlans = null;
            PrivacyWrapper<List<Dtos.Planning.Advisee>> privacyWrapper = null;

            // If we have just an advisor ID then select the advisees based on that
            if (isId && !string.IsNullOrEmpty(criteria.AdvisorKeyword))
            {

                IEnumerable<string> advisorIds = new List<string>() { searchString };
                // Do the search based on advisor's permissions
                if (!(await CanViewAnyAdviseeAsync()) && advisor.Advisees.Count() > 0)
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (await CanViewAnyAdviseeAsync())
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
                adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(advisees.Select(s => s.Id));
                privacyWrapper = BuildAdviseeDtos(adviseeDegreePlans, advisees, advisor.Advisees);
                return privacyWrapper;
            }

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

            // Now that the search string is parsed into the appropriate pieces, do the appropriate search based on which keyword was supplied
            if (!string.IsNullOrEmpty(criteria.AdviseeKeyword))
            {

                // Advisee Name Search: Call repository method to query students based on name strings
                // Do the search based on the advisor's permissions
                var canViewAnyAdvisee = await CanViewAnyAdviseeAsync();
                if (!canViewAnyAdvisee && advisor.Advisees.Count() > 0)
                {

                    advisees = (await _adviseeRepository.SearchByNameForExactMatchAsync(lastName, firstName, middleName, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (canViewAnyAdvisee)
                {
                    advisees = (await _adviseeRepository.SearchByNameForExactMatchAsync(lastName, firstName, middleName, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
            }
            else
            {
                IEnumerable<string> advisorIds = await _advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName, firstName, middleName);

                // Do the search based on advisor's permissions - if advisorIds list is empty the repo will return an empty list of advisees
                if (!(await CanViewAnyAdviseeAsync()) && advisor.Advisees.Count() > 0)
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, advisor.Advisees)).ToList();
                }
                else if (await CanViewAnyAdviseeAsync())
                {
                    advisees = (await _adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, pageSize, pageIndex, null)).ToList();
                }
                else
                {
                    advisees = new List<Domain.Student.Entities.PlanningStudent>();
                }
            }

            watch.Restart();

            adviseeDegreePlans = await _studentDegreePlanRepository.GetAsync(advisees.Select(s => s.Id));

            //===================================================
            watch.Stop();
            logger.Info("Get Advisee Plans... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();

            privacyWrapper = BuildAdviseeDtos(adviseeDegreePlans, advisees, advisor.Advisees);

            //===================================================
            watch.Stop();
            logger.Info("Build Advisee DTOs... completed in " + watch.ElapsedMilliseconds.ToString());

            return privacyWrapper;
        }

        /// <summary>
        /// Posts a <see cref="Dtos.Student.CompletedAdvisement">completed advisement</see>
        /// </summary>
        /// <param name="studentId">ID of the student whose advisement is being marked complete</param>
        /// <param name="completeAdvisement">A <see cref="Dtos.Student.CompletedAdvisement">completed advisement</see></param>
        /// <returns>An <see cref="Dtos.Planning.Advisee">advisee</see></returns>
        public async Task<PrivacyWrapper<Dtos.Planning.Advisee>> PostCompletedAdvisementAsync(string studentId, Dtos.Student.CompletedAdvisement completeAdvisement)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID must be specified.");
            }
            if (completeAdvisement == null)
            {
                throw new ArgumentNullException("completeAdvisement", "A complete advisement object must be supplied.");
            }
            if (string.IsNullOrEmpty(completeAdvisement.AdvisorId))
            {
                throw new ArgumentNullException("completeAdvisement.AdvisorId", "Complete advisement object must have an advisor ID specified.");
            }
            if (studentId == completeAdvisement.AdvisorId)
            {
                throw new ApplicationException("Student ID and Advisor ID cannot be the same.");
            }

            // Check complete advisement permissions for student for the advisor
            await CheckCompleteAdvisementPermissionsAsync(studentId, completeAdvisement.AdvisorId);

            // Post complete advisement
            var advisementCompletedStudent = await _adviseeRepository.PostCompletedAdvisementAsync(studentId, completeAdvisement.CompletionDate, completeAdvisement.CompletionTime, completeAdvisement.AdvisorId);
            if (advisementCompletedStudent == null)
            {
                throw new ApplicationException("Unable to retrieve data for student " + studentId + " after posting advisement completion.");
            }

            // Get advisor data; if the user is limited to his own advisees, get their IDs so we can limit the return set.
            var advisor = await GetAuthorizedAdvisorAsync(completeAdvisement.AdvisorId);

            // Before doing anything, check the current advisor's privacy code settings (on their staff record)
            // against any privacy code on the student's record
            return BuildAdviseeDto(advisementCompletedStudent, advisor.Advisees);
        }

        /// <summary>
        /// Determines if the user has permission to create a degree plan for this student - will throw a PermissionException if not permitted.
        /// </summary>
        /// <param name="student">Student for whom the advisement is being marked complete</param>
        /// <param name="advisorId">ID of the advisor marking the advisement complete</param>
        protected async Task CheckCompleteAdvisementPermissionsAsync(string studentId, string advisorId)
        {
            // User does not have permission if they do not have the specified advisor ID
            if (!UserIsSelf(advisorId))
            {
                // User does not have permissions and error needs to be thrown and logged
                var error = "User " + CurrentUser.PersonId + " does not match given advisor ID " + advisorId + " and may not complete advisements for student " + studentId;
                logger.Error(error);
                throw new PermissionsException(error);
            }

            // Get student from repository
            Ellucian.Colleague.Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            if (student == null)
            {
                throw new ApplicationException("Unable to retrieve student data for student " + studentId);
            }

            bool userIsAssignedAdvisor = await UserIsAssignedAdvisorAsync(student.Id, student.ConvertToStudentAccess());

            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            // Access is Ok if this is an advisor with all access, update access, or review access for any student.  
            // Access is also OK if this is an advisor with all access, update access, or review access to their assigned advisees and this is an assigned advisee.
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee) || 
                userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) || 
                userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee) || 
                (userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees) && userIsAssignedAdvisor) || 
                (userPermissions.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees) && userIsAssignedAdvisor) || 
                (userPermissions.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees) && userIsAssignedAdvisor))
            {
                return;
            }

            // User does not have permissions and error needs to be thrown and logged
            logger.Error("User " + CurrentUser.PersonId + " does not have permissions to complete advisements for student " + student.Id);
            throw new PermissionsException();
        }

        protected async Task CheckViewAdviseePermissionsAsync()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.ViewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees))
            {
                return;
            }
            throw new PermissionsException("User does not have permission to perform this operation");
        }

        protected async Task<bool> CanViewAnyAdviseeAsync()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<Ellucian.Colleague.Dtos.Planning.Advisor> BuildAdvisorDtoAsync(Domain.Planning.Entities.Advisor advisor)
        {
            StudentConfiguration studentConfiguration = await _studentConfigurationRepository.GetStudentConfigurationAsync();
            string facultyEmailTypeCode = studentConfiguration.FacultyEmailTypeCode;

            // Get the right adapter for the type mapping
            var advisorDtoAdapter = new AdvisorEntityAdapter(_adapterRegistry, logger);

            // Use the customized adapter to map the advisor entity to the advisor DTO - makes use of the facultyEmailTypeCode.
            Ellucian.Colleague.Dtos.Planning.Advisor advisorDto = advisorDtoAdapter.MapToType(advisor, facultyEmailTypeCode);


            return advisorDto;
        }

        private async Task<IEnumerable<Ellucian.Colleague.Dtos.Planning.Advisor>> BuildAdvisorDtosAsync(IEnumerable<Domain.Planning.Entities.Advisor> advisors)
        {
            StudentConfiguration studentConfiguration = await _studentConfigurationRepository.GetStudentConfigurationAsync();
            string facultyEmailTypeCode = studentConfiguration.FacultyEmailTypeCode;

            // Get the right adapter for the type mapping
            var advisorDtoAdapter = new AdvisorEntityAdapter(_adapterRegistry, logger);

            List<Dtos.Planning.Advisor> dtos = new List<Dtos.Planning.Advisor>();

            if (advisors != null && advisors.Any())
            {
                foreach (var advisor in advisors)
                {
                    try
                    {
                        // Use the customized adapter to map the advisor entity to the advisor DTO - makes use of the facultyEmailTypeCode.
                        dtos.Add(advisorDtoAdapter.MapToType(advisor, facultyEmailTypeCode));
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error converting Advisor entity to Advisor DTO. " + ex.Message);
                    }
                }
            }

            return dtos;
        }
    }
}