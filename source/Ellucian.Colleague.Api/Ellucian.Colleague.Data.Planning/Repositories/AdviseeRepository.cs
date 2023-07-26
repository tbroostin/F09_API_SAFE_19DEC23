// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Planning.Transactions;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AdviseeRepository : PlanningStudentRepository, IAdviseeRepository
    {
        private readonly string _colleagueTimeZone;
        public AdviseeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get the advisee specified by id
        /// </summary>
        /// <param name="id">An advisee (student) id</param>
        /// <returns>A student domain entity for that specified advisee</returns>
        new public async Task<Domain.Student.Entities.PlanningStudent> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Advisee ID may not be null or empty");
            }
            else
            {
                id = await PadIdPerPid2ParamsAsync(id);

                // An advisee is really just a student, so call the base (PlanningStudentRepository) method
                return await base.GetAsync(id);
            }
        }

        /// <summary>
        /// Reads the advisee information from Colleague and returns an IEnumerable of Student entities.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list of none are found.  The set of advisees/students
        /// is sorted first by degree plan request status, then by advisee/student name (PERSON-SORT.NAME).  We have to do the sorting and paging here,
        /// and not in a domain service, because we need to be able to limit the number of people retrieved from Colleague for performance reasons
        /// (this is unlike the Course search results, where all courses are cached and indexed, and the sort/paging is done somewhere closer to the business 
        /// domain)</returns>
        public async Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> GetAsync(IEnumerable<string> adviseeIds, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (adviseeIds == null || adviseeIds.Count() == 0)
            {
                throw new ArgumentException("adviseeIds", "You must specify at least 1 id to retrieve.");
            }
            var watch = new Stopwatch();
            watch.Start();

            var currentPageAdvisees = await this.GetCurrentPageAsync(adviseeIds, pageSize, pageIndex);

            watch.Stop();
            logger.Info(String.Format("  STEP3.1 GetCurrentPage... completed in {0} ", watch.ElapsedMilliseconds.ToString()));
            if (currentPageAdvisees != null)
            {
                logger.Info(String.Format("  STEP3.1.1 GetCurrentPage... returned {0} advisees out of {1} advisees passed", currentPageAdvisees.Count(), adviseeIds.Count()));
            }
            watch.Restart();

            var advisees = currentPageAdvisees.Count() > 0 ? (await base.GetAsync(currentPageAdvisees)).ToList() : new List<Domain.Student.Entities.PlanningStudent>();

            watch.Stop();
            logger.Info(String.Format("  STEP3.2 Get(currentPageAdvisees)(base)... completed in {0}", watch.ElapsedMilliseconds.ToString()));
            if (advisees != null)
            {
                logger.Info(String.Format("  STEP3.2.1 Get(currentPageAdvisees)(base)... returned {0} planning students records", advisees.Count));
            }
            return advisees;
        }

        /// <summary>
        /// Finds students given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against STUDENTS.
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Student Ids</returns>
        public async Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> SearchByNameAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<Domain.Student.Entities.PlanningStudent>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var adviseeIds = await base.SearchByNameAsync(lastName, firstName, middleName);

            watch.Stop();
            logger.Info("  STEP5.1 SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (adviseeIds != null)
            {
                logger.Info("  STEP5.1.1 Returned searched Person Ids count: " + adviseeIds.Count());
            }
            watch.Restart();
            // Filter to only return students
            adviseeIds = await base.FilterByEntityAsync("STUDENTS", adviseeIds);
            watch.Stop();
            logger.Info("  STEP5.2 FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (adviseeIds != null)
            {
                logger.Info("  STEP5.2.1 Filtered PERSONS to " + adviseeIds.Count() + " STUDENTS.");
            }

            // If there are assigned advisees (only set if the user can only view assigned advisees), limit the list
            if (assignedAdvisees != null && assignedAdvisees.Count() > 0)
            {
                logger.Info("Filtering student Ids to only assigned advisees");
                adviseeIds = adviseeIds.Where(x => assignedAdvisees.Contains(x));
                if (adviseeIds != null)
                {
                    logger.Info("  STEP5.2.2 Filtered advisee Ids to only assigned advisees and the count is " + adviseeIds.Count());
                }
            }

            watch.Restart();

            var currentPageAdvisees = await this.GetCurrentPageAsync(adviseeIds, pageSize, pageIndex);

            watch.Stop();
            logger.Info("  STEP5.3 GetCurrentPage... completed in " + watch.ElapsedMilliseconds.ToString());
            if (currentPageAdvisees != null)
            {
                logger.Info(String.Format("  STEP3.2 Get(currentPageAdvisees)(base)... returned {0} advisees ids", currentPageAdvisees.Count()));
            }
            watch.Restart();

            var adviseesUnsorted = currentPageAdvisees.Count() > 0 ? (await base.GetAsync(currentPageAdvisees)).ToList() : new List<Domain.Student.Entities.PlanningStudent>();

            // GetCurrentPage returns the advisee IDs sorted.  base.Get does not.
            // So, sort advisee entities in the order supplied by GetCurrentPage
            var advisees = new List<Domain.Student.Entities.PlanningStudent>();
            foreach (var id in currentPageAdvisees)
            {
                var matchedAdvisee = adviseesUnsorted.FirstOrDefault(x => x.Id == id);
                if (matchedAdvisee != null)
                {
                    advisees.Add(matchedAdvisee);
                }
                else
                {
                    logger.Info(string.Format("Could not retrieve details for the Advisee {0} therefore this particular advisee is not returned", id));
                }
            }

            watch.Stop();
            logger.Info("  STEP5.4 Get(currentPageAdvisees)(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            logger.Info(String.Format("  STEP5.4.1 Get(currentPageAdvisees)(base)... returned {0} planningStudent records out of {1} advisee ids on current page", advisees.Count(), currentPageAdvisees.Count()));
            return advisees;
        }


        /// <summary>
        /// Finds students given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against STUDENTS.
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Student Ids</returns>
        public async Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> SearchByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<Domain.Student.Entities.PlanningStudent>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var adviseeIds = await base.SearchByNameForExactMatchAsync(lastName, firstName, middleName);

            watch.Stop();
            logger.Info("  STEP5.1 SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (adviseeIds != null)
            {
                logger.Info("  STEP5.1.1 Returned searched Person Ids count: " + adviseeIds.Count());
            }
            watch.Restart();
            // Filter to only return students
            adviseeIds = await base.FilterByEntityAsync("STUDENTS", adviseeIds);
            watch.Stop();
            logger.Info("  STEP5.2 FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (adviseeIds != null)
            {
                logger.Info("  STEP5.2.1 Filtered PERSONS to " + adviseeIds.Count() + " STUDENTS.");
            }

            // If there are assigned advisees (only set if the user can only view assigned advisees), limit the list
            if (assignedAdvisees != null && assignedAdvisees.Count() > 0)
            {
                logger.Info("Filtering student Ids to only assigned advisees");
                adviseeIds = adviseeIds.Where(x => assignedAdvisees.Contains(x));
                if (adviseeIds != null)
                {
                    logger.Info("  STEP5.2.2 Filtered advisee Ids to only assigned advisees and the count is " + adviseeIds.Count());
                }
            }

            watch.Restart();

            var currentPageAdvisees = await this.GetCurrentPageAsync(adviseeIds, pageSize, pageIndex);

            watch.Stop();
            logger.Info("  STEP5.3 GetCurrentPage... completed in " + watch.ElapsedMilliseconds.ToString());
            if (currentPageAdvisees != null)
            {
                logger.Info(String.Format("  STEP3.2 Get(currentPageAdvisees)(base)... returned {0} advisees ids", currentPageAdvisees.Count()));
            }
            watch.Restart();

            var adviseesUnsorted = currentPageAdvisees.Count() > 0 ? (await base.GetAsync(currentPageAdvisees)).ToList() : new List<Domain.Student.Entities.PlanningStudent>();

            // GetCurrentPage returns the advisee IDs sorted.  base.Get does not.
            // So, sort advisee entities in the order supplied by GetCurrentPage
            var advisees = new List<Domain.Student.Entities.PlanningStudent>();
            foreach (var id in currentPageAdvisees)
            {
                var matchedAdvisee = adviseesUnsorted.FirstOrDefault(x => x.Id == id);
                if (matchedAdvisee != null)
                {
                    advisees.Add(matchedAdvisee);
                }
                else
                {
                    logger.Info(string.Format("Could not retrieve details for the Advisee {0} therefore this particular advisee is not returned", id));
                }

            }

            watch.Stop();
            logger.Info("  STEP5.4 Get(currentPageAdvisees)(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            logger.Info(String.Format("  STEP5.4.1 Get(currentPageAdvisees)(base)... returned {0} planningStudent records out of {1} advisee ids on current page", advisees.Count(), currentPageAdvisees.Count()));
            return advisees;
        }
        /// <summary>
        /// Given a list of advisee IDs, get the IDs on the current page, sorted by degree plan request status, then by name
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetCurrentPageAsync(IEnumerable<string> adviseeIds, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            // Get the student IDs for any advisees that have requested review/approval,
            // then sort those IDs by PERSON-SORT.NAME
            // Advisee performance diagnostics
            logger.Info("Start AdviseeRepository GetCurrentPage ");

            // Returns empty list if no ids passed in
            if (adviseeIds == null || adviseeIds.Count() == 0)
            {
                return new List<string>();
            }

            var watch = new Stopwatch();
            watch.Start();

            string[] studentIdsReviewRequested = new string[] { };
            string searchString = "DP.REVIEW.REQUESTED EQ 'Y' AND WITH DP.STUDENT.ID EQ '?' SAVING DP.STUDENT.ID";
            studentIdsReviewRequested = await DataReader.SelectAsync("DEGREE_PLAN", searchString, adviseeIds.ToArray());

            watch.Stop();
            logger.Info("    STEPX.3.1 SELECT DEGREE_PLAN WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());

            string[] studentIdsReviewRequestedSorted = new string[] { };
            if (studentIdsReviewRequested != null && studentIdsReviewRequested.Count() > 0)
            {
                logger.Info("Count of student Ids requested for degree plan review is: " + studentIdsReviewRequested.Count());
                watch.Restart();

                searchString = "BY LAST.NAME BY FIRST.NAME BY MIDDLE.NAME";
                studentIdsReviewRequestedSorted = await DataReader.SelectAsync("PERSON", studentIdsReviewRequested, searchString);

                watch.Stop();
                logger.Info("    STEPX.3.2 SELECT PERSON WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());
                if (studentIdsReviewRequestedSorted != null)
                {
                    logger.Info(string.Format("Count of PERSON records read for the list of students requested for degree plan review is {0}", studentIdsReviewRequestedSorted.Count()));
                }
                else
                {
                    logger.Info("No PERSON records were read for student ids requested for degree plan review");
                }
            }
            else
            {
                logger.Info("There are no students waiting for degree plan review");
            }

            watch.Restart();

            // Now get the IDs for all student advisees, sorted by name.
            // First, narrow the list to just students because applicants can also be assigned to advisors.
            string[] studentIds = await DataReader.SelectAsync("STUDENTS", adviseeIds.ToArray(), string.Empty);
            watch.Stop();
            logger.Info("    STEPX.3.3 SELECT STUDENTS " + "... completed in " + watch.ElapsedMilliseconds.ToString());
            logger.Info(String.Format("Count of Student records are {0} for the list of advisees passed {1} ", studentIds.Count(), adviseeIds.Count()));
            // Next sort them by name
            watch.Restart();
            searchString = "BY LAST.NAME BY FIRST.NAME BY MIDDLE.NAME";
            string[] studentIdsSorted = await DataReader.SelectAsync("PERSON", studentIds, searchString);

            watch.Stop();
            logger.Info("    STEPX.3.3.1 SELECT PERSON WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());
            logger.Info(String.Format("Count of PERSON records are {0}", studentIdsSorted.Count()));
            // Finally, merge the two lists of IDs, removing duplicates.
            var studentIdsMerged = studentIdsReviewRequestedSorted != null && studentIdsReviewRequestedSorted.Count() != 0 ? studentIdsReviewRequestedSorted.Concat(studentIdsSorted) : studentIdsSorted.ToList();
            var studentIdsMergedNoDupes = new List<string>();
            foreach (var studentId in studentIdsMerged)
            {
                if (!studentIdsMergedNoDupes.Contains(studentId))
                {
                    studentIdsMergedNoDupes.Add(studentId);
                }
            }

            var totalItems = studentIdsMergedNoDupes.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            return studentIdsMergedNoDupes.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        /// <summary>
        /// Searches for advisees who are currently advised by the specified advisor Ids
        /// </summary>
        /// <param name="advisorIds">List of advisor Ids</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="assignedAdvisees">List of Ids currently assigned to the person requesting the search (populated if the person is ONLY allowed to view specific advisees. List will be limited based on this.</param>
        /// <returns>Student entities for any matching advisees</returns>
        public async Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> SearchByAdvisorIdsAsync(IEnumerable<string> advisorIds, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null)
        {
            if (advisorIds == null || advisorIds.Count() == 0)
            {
                return new List<Domain.Student.Entities.PlanningStudent>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Select the adviseeIds that are currently assigned to these advisor Ids.

            string todaysDate = UniDataFormatter.UnidataFormatDate(DateTime.Today, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter);

            string searchString = "STAD.FACULTY EQ '?' AND WITH STAD.START.DATE LE '" + todaysDate + "' AND WITH STAD.END.DATE EQ '' OR STAD.END.DATE GT '" + todaysDate + "' SAVING STAD.STUDENT";

            string[] matchingAdviseeIds = await DataReader.SelectAsync("STUDENT.ADVISEMENT", searchString, advisorIds.ToArray());

            IEnumerable<string> adviseeIds = matchingAdviseeIds.AsEnumerable().Distinct();
            if (adviseeIds != null)
            {
                logger.Info(String.Format("Count of STUDENT.ADVISEMENT records found for given advisor Ids are {0} ", adviseeIds.Count()));
            }

            // If there are assigned advisees (only set if the user can only view assigned advisees), limit the list
            if (assignedAdvisees != null && assignedAdvisees.Count() > 0)
            {
                adviseeIds = adviseeIds.Where(x => assignedAdvisees.Contains(x));
            }

            watch.Stop();
            logger.Info("Student Advisement select... completed in " + watch.ElapsedMilliseconds.ToString());
            if (adviseeIds != null)
            {
                logger.Info(String.Format("Count of assigned STUDENT.ADVISEMENT records found for given advisor Ids are {0} ", adviseeIds.Count()));
            }
            watch.Restart();

            var currentPageAdvisees = await this.GetCurrentPageAsync(adviseeIds, pageSize, pageIndex);

            watch.Stop();
            logger.Info("GetCurrentPage... completed in " + watch.ElapsedMilliseconds.ToString());
            if (currentPageAdvisees != null)
            {
                logger.Info(String.Format("Count of Current page advisees for advisor Ids search is {0} ", currentPageAdvisees.Count()));
            }
            watch.Restart();

            var advisees = currentPageAdvisees.Count() > 0 ? (await base.GetAsync(currentPageAdvisees)).ToList() : new List<Domain.Student.Entities.PlanningStudent>();

            watch.Stop();
            logger.Info("Get(currentPageAdvisees)(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (advisees != null)
            {
                logger.Info(String.Format("Count of Current page advisees planningStudent records for advisor Ids search is {0} ", advisees.Count()));
            }
            return advisees;
        }

        /// <summary>
        /// Posts advisement complete data for a student
        /// </summary>
        /// <param name="studentId">ID of the student whose advisement is being marked complete</param>
        /// <param name="completionDate">Date on which an advisor is marking the student's advisement complete</param>
        /// <param name="completionTime">Time of day at which an advisor is marking the student's advisement complete</param>
        /// <param name="advisorId">ID of the advisor who is marking the student's advisement complete</param>
        /// <returns>An updated <see cref="Domain.Planning.Entities.PlanningStudent">planning student</see></returns>
        public async Task<Domain.Student.Entities.PlanningStudent> PostCompletedAdvisementAsync(string studentId, DateTime completionDate, DateTimeOffset completionTime, string advisorId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Must provide a student ID to mark a student as 'advisement complete.'");
            }
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "Must provide an advisor ID to mark a student as 'advisement complete.'");
            }

            CreateStudentAdvisementRequest request = new CreateStudentAdvisementRequest()
            {
                AStudentsId = studentId,
                AStuAdviseCompleteDate = completionDate,
                AStuAdviseCompleteTime = completionTime.ToLocalDateTime(_colleagueTimeZone),
                AStuAdviseCompleteAdvisor = advisorId
            };

            CreateStudentAdvisementResponse response = await transactionInvoker.ExecuteAsync<CreateStudentAdvisementRequest, CreateStudentAdvisementResponse>(request);
            if (response == null)
            {
                throw new ApplicationException("Null response returned by application listener when executing COMPLETE.ADVISEMENT.FOR.STUDENT transaction");
            }
            if (response.AErrorOccurred)
            {
                string error = "One or more errors occurred when executing COMPLETE.ADVISEMENT.FOR.STUDENT transaction. ";
                if (response.AlErrorMessages != null)
                {
                    error += Environment.NewLine;
                    error += String.Join(Environment.NewLine, response.AlErrorMessages);
                    logger.Error(error);
                }
                throw new ApplicationException(error);
            }
            return await base.GetAsync(response.AStudentsId, false);
        }
    }
}
