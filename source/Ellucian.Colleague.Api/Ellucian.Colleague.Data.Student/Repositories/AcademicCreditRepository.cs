// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AcademicCreditRepository : BaseColleagueRepository, IAcademicCreditRepository
    {
        private ICourseRepository CourseRepository;
        private ApplValcodes creditStatuses;
        private IDictionary<string, CreditType> types;
        private IDictionary<string, Grade> grades;
        private readonly IGradeRepository gradeRepository;
        private ITermRepository termRepository;
        private ISectionRepository sectionRepository;
        private IEnumerable<Term> termList;
        private string colleagueTimeZone;
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;
        const string AllStudentEquivEvalsRecordsCache = "AllStudentEquivEvalsRecordKeys";
        const int AllStudentEquivEvalsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        public AcademicCreditRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger,
            ICourseRepository courseRepository,
            IGradeRepository gradeRepository,
            ITermRepository termRepository,
            ISectionRepository sectionRepository,
            ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 5;
            CourseRepository = courseRepository;
            types = new Dictionary<string, CreditType>();
            this.gradeRepository = gradeRepository;
            this.termRepository = termRepository;
            this.sectionRepository = sectionRepository;
            termList = null;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Retrieves all academic credits for specific section Ids. This will return all academic credits regardless of status.
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <returns>List of AcademicCredit entities specific to this section.</returns>
        public async Task<IEnumerable<AcademicCredit>> GetAcademicCreditsBySectionIdsAsync(IEnumerable<string> sectionIds)
        {
            if (sectionIds == null || !sectionIds.Any())
            {
                throw new ArgumentNullException("sectionIds", "At least 1 Section Id is required to retrieve a section academic credits.");
            }
            IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();
            // Section Id is not a property of the STUDENT.ACAD.CRED. Select STUDENT.COURSE.SEC records and retrieve the appropriate academic credit Ids from those.
            List<string> consolidatedAcademicCreditIds = new List<string>();
            for (int i = 0; i < sectionIds.Count(); i += readSize)
            {
                var subList = sectionIds.Skip(i).Take(readSize).ToArray();
                string[] academicCreditIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '?' SAVING SCS.STUDENT.ACAD.CRED", subList);
                consolidatedAcademicCreditIds.AddRange(academicCreditIds);
            }

            if (consolidatedAcademicCreditIds != null && consolidatedAcademicCreditIds.Any())
            {
                // ask for all academic credit, unfiltered and without best fit. Called processes can filter as they need.
                academicCredits = await GetAsync(consolidatedAcademicCreditIds.Distinct().ToList(), false, false);
            }
            return academicCredits;
        }

        /// <summary>
        /// Retrieves all academic credits for specific section Ids. This will return all academic credits regardless of status.
        /// This also returns collection of invalid academic credit Ids, Ids that are missing from STUDENT.ACAD.CRED file.
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <returns>Tuple that contains List of AcademicCredit entities specific to the given sections and List of invalid academic credit Ids.</returns>
        public async Task<AcademicCreditsWithInvalidKeys> GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(IEnumerable<string> sectionIds)
        {
            if (sectionIds == null || !sectionIds.Any())
            {
                throw new ArgumentNullException("sectionIds", "At least 1 Section Id is required to retrieve a section academic credits.");
            }
            AcademicCreditsWithInvalidKeys academicCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(new List<AcademicCredit>(), new List<string>());
            //remove duplicates, null, blanks from the list
            IEnumerable<string> cleanedSectionIds = sectionIds.Where(s => !string.IsNullOrEmpty(s)).Distinct();
            // Section Id is not a property of the STUDENT.ACAD.CRED. Select STUDENT.COURSE.SEC records and retrieve the appropriate academic credit Ids from those.
            List<string> consolidatedAcademicCreditIds = new List<string>();
            for (int i = 0; i < cleanedSectionIds.Count(); i += readSize)
            {
                var subList = cleanedSectionIds.Skip(i).Take(readSize).ToArray();
                string[] academicCreditIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '?' SAVING SCS.STUDENT.ACAD.CRED", subList);
                consolidatedAcademicCreditIds.AddRange(academicCreditIds);
            }

            if (consolidatedAcademicCreditIds != null && consolidatedAcademicCreditIds.Any())
            {
                // ask for all academic credit, unfiltered and without best fit. Called processes can filter as they need.
                academicCreditsWithInvalidKeys = await GetWithInvalidKeysAsync(consolidatedAcademicCreditIds.Distinct().ToList(), false, false);
            }
            return academicCreditsWithInvalidKeys;
        }

        /// <summary>
        /// Sorts a list of academic credits according to one or more sort specifications and returns a dictionary of
        /// the sorted academic credits, keyed by their sort specification ID
        /// </summary>
        /// <param name="acadCredits">Collection of <see cref="AcademicCredit"/> objects.</param>
        /// <param name="sortSpecIds">Collection of sort specification IDs</param>
        /// <returns>Dictionary of sorted academic credits, keyed by their sort specification ID</returns>
        public async Task<Dictionary<string, List<AcademicCredit>>> GetSortedAcademicCreditsBySortSpecificationIdAsync(IEnumerable<AcademicCredit> acadCredits, IEnumerable<string> sortSpecIds)
        {
            if (acadCredits == null || !acadCredits.Any())
            {
                throw new ArgumentNullException("acadCredits", "At least 1 Academic Credit is required to retrieve sorted academic credits.");
            }

            var acadCredIds = acadCredits.Select(ac => ac.Id).Where(ac => !string.IsNullOrEmpty(ac)).Distinct().ToList();
            var dict = new Dictionary<string, List<AcademicCredit>>();

            if (acadCredIds.Any())
            {
                // Create SORT.STUDENT.ACAD.CREDS request
                var sortRequest = new SortStudentAcadCredsRequest()
                {
                    InStudentAcadCredIds = acadCredits.Select(ac => ac.Id).Where(ac => !string.IsNullOrEmpty(ac)).Distinct().ToList(),
                    InDaSortSpecsIds = sortSpecIds == null ? new List<string>() : sortSpecIds.Distinct().ToList()
                };

                // Call SORT.STUDENT.ACAD.CREDS
                var sortResponse = await transactionInvoker.ExecuteAsync<SortStudentAcadCredsRequest, SortStudentAcadCredsResponse>(sortRequest);

                // Handle null response from SORT.STUDENT.ACAD.CREDS call
                if (sortResponse == null)
                {
                    throw new ApplicationException("Call to CTX SORT.STUDENT.ACAD.CREDS returned null.");
                }

                // Handle errors from SORT.STUDENT.ACAD.CREDS call
                if (sortResponse.OutMessages != null && sortResponse.OutMessages.Any())
                {
                    throw new ApplicationException("Call to CTX SORT.STUDENT.ACAD.CREDS returned error(s):" + String.Join(Environment.NewLine, sortResponse.OutMessages));
                }

                // Handle null/empty sorted lists
                if (sortResponse.SortedStudentAcadCreditsBySortSpecId == null || !sortResponse.SortedStudentAcadCreditsBySortSpecId.Any())
                {
                    throw new ApplicationException("Call to CTX SORT.STUDENT.ACAD.CREDS did not return sorted list(s) of STUDENT.ACAD.CREDS");
                }

                // Build dictionary from valid SORT.STUDENT.ACAD.CREDS response
                foreach (var sortSpec in sortResponse.SortedStudentAcadCreditsBySortSpecId)
                {
                    if (!dict.ContainsKey(sortSpec.OutDaSortSpecsIds))
                    {
                        // Split the list of @SM-delimited STUDENT.ACAD.CRED.IDs
                        var sortedCreditIds = sortSpec.OutSortedStudentAcadCredIds.Split(DmiString._SM).ToList();

                        // Sort the list of AcademicCredit objects using the sort order
                        var sortedCredits =
                            (from id in sortedCreditIds
                             join academicCredit in acadCredits on id equals academicCredit.Id
                             select academicCredit).ToList();

                        // Add a dictionary entry for the sort spec using the sorted AcademicCredit list
                        dict.Add(sortSpec.OutDaSortSpecsIds, sortedCredits);
                    }
                }
            }
            else
            {
                logger.Info("No academic credits to sort; skipped call to SORT.STUDENT.ACAD.CREDS");
            }

            return dict;
        }

        /// <summary>
        /// Get a set of academic credits by ID
        /// </summary>
        /// <param name="academicCreditIds"></param>
        /// <param name="bestFit">Set to true if non-term credits should be given the term that most closely matches the credits date range </param>
        /// <param name="term">Term for filtering academic credit</param>
        /// <param name="filter">Flag indicating whether or not to filter credits based on status</param>
        /// <param name="includeDrops">Flag indicating whether or not to include dropped academic credits; can only be used when 'filter' is true</param>
        /// <returns>A set of academic credits</returns>
        public async Task<IEnumerable<AcademicCredit>> GetAsync(ICollection<string> academicCreditIds, bool bestFit = false, bool filter = true, bool includeDrops = false)
        {
            var creditList = new List<AcademicCredit>();
            if (academicCreditIds != null && academicCreditIds.Count() > 0)
            {
                // Not caching this information at this time. If we do decide to cash this info, we should cache all
                // the student information together (including grade viewing Restrictions).
                var academicCredits = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", academicCreditIds.ToArray());
                if (academicCredits == null || academicCreditIds.Count != academicCredits.Count)
                {
                    logger.Error("AcademicCreditRepository: ERROR: Failed to retrieve all credits from the database.  Id count " + academicCreditIds.Count + " Credits count " + academicCredits.Count);
                    var missingIds = academicCreditIds.Except(academicCredits.Select(c => c.Recordkey)).Distinct().ToList();
                    logger.Error("   Missing Ids :" + string.Join(",", missingIds));
                    throw new ArgumentOutOfRangeException("academicCreditIds", "Failed to retrieve all credits from the database.");
                }
                var academicCreditsCc = await DataReader.BulkReadRecordAsync<StudentAcadCredCc>("STUDENT.ACAD.CRED.CC", academicCreditIds.ToArray());
                var studentCourseSecIds = academicCredits.Select(sas => sas.StcStudentCourseSec).Distinct();
                var studentCourseSections = await DataReader.BulkReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray());
                var studentCourseSectionCc = await DataReader.BulkReadRecordAsync<StudentCourseSecCc>("STUDENT.COURSE.SEC.CC", studentCourseSecIds.ToArray());

                // Bulk read StudentEquivEvals, which is used to determine if credit was granted based on a non-course equivalency
                var studentEquivEvalIds = academicCredits.Select(sas => sas.StcStudentEquivEval).Distinct();
                var studentEquivEvals = await DataReader.BulkReadRecordAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", studentEquivEvalIds.ToArray());

                creditList = (await BuildCreditsAsync(academicCredits, academicCreditsCc, studentCourseSections, studentCourseSectionCc, studentEquivEvals, bestFit)).ToList();
            }
            if (filter)
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse
                };
                if (includeDrops)
                {
                    filteredStatuses.Add(CreditStatus.Dropped);
                }
                return creditList.Where(ac => filteredStatuses.Contains(ac.Status));
            }
            else
            {
                return creditList;
            }
        }

        /// <summary>
        /// Get a set of valid academic credits by ID as well as list of invalid academic credit Ids that are missing from STUDENT.ACAD.CRED file.
        /// </summary>
        /// <param name="academicCreditIds"></param>
        /// <param name="bestFit">Set to true if non-term credits should be given the term that most closely matches the credits date range </param>
        /// <param name="term">Term for filtering academic credit</param>
        /// <param name="filter">Flag indicating whether or not to filter credits based on status</param>
        /// <param name="includeDrops">Flag indicating whether or not to include dropped academic credits; can only be used when 'filter' is true</param>
        /// <returns>A Tuple that contains set of academic credits and list of invalid academic credit Ids</returns>
        public async Task<AcademicCreditsWithInvalidKeys> GetWithInvalidKeysAsync(ICollection<string> academicCreditIds, bool bestFit = false, bool filter = true, bool includeDrops = false)
        {
            var creditList = new List<AcademicCredit>();
            List<string> missingIds = new List<string>();

            if (academicCreditIds != null && academicCreditIds.Count() > 0)
            {
                List<string> distinctAcademicCreditIds = academicCreditIds.Distinct().ToList();
                // Not caching this information at this time. If we do decide to cash this info, we should cache all
                // the student information together (including grade viewing Restrictions).
                var academicCredits = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", distinctAcademicCreditIds.ToArray());
                if (academicCredits == null || distinctAcademicCreditIds.Count != academicCredits.Count)
                {
                    logger.Error("AcademicCreditRepository: ERROR: Failed to retrieve all credits from the database.  Id count " + distinctAcademicCreditIds.Count + " Credits count " + academicCredits.Count);
                    missingIds = distinctAcademicCreditIds.Except(academicCredits.Select(c => c.Recordkey)).Distinct().ToList();
                    logger.Error("   Missing Ids :" + string.Join(",", missingIds));
                }
                var academicCreditsCc = await DataReader.BulkReadRecordAsync<StudentAcadCredCc>("STUDENT.ACAD.CRED.CC", distinctAcademicCreditIds.ToArray());
                var studentCourseSecIds = academicCredits.Select(sas => sas.StcStudentCourseSec).Distinct();
                var studentCourseSections = await DataReader.BulkReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray());
                var studentCourseSectionCc = await DataReader.BulkReadRecordAsync<StudentCourseSecCc>("STUDENT.COURSE.SEC.CC", studentCourseSecIds.ToArray());

                // Bulk read StudentEquivEvals, which is used to determine if credit was granted based on a non-course equivalency
                var studentEquivEvalIds = academicCredits.Select(sas => sas.StcStudentEquivEval).Distinct();
                var studentEquivEvals = await DataReader.BulkReadRecordAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", studentEquivEvalIds.ToArray());

                creditList = (await BuildCreditsAsync(academicCredits, academicCreditsCc, studentCourseSections, studentCourseSectionCc, studentEquivEvals, bestFit)).ToList();
            }
            if (filter)
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse
                };
                if (includeDrops)
                {
                    filteredStatuses.Add(CreditStatus.Dropped);
                }
                return new AcademicCreditsWithInvalidKeys(creditList.Where(ac => filteredStatuses.Contains(ac.Status)), missingIds);
            }
            else
            {
                return new AcademicCreditsWithInvalidKeys(creditList, missingIds);
            }
        }



        public async Task<IEnumerable<AcademicCreditMinimum>> GetAcademicCreditMinimumAsync(ICollection<string> academicCreditIds, bool filter = true, bool includeDrops = false)
        {
            var creditList = new List<AcademicCreditMinimum>();
            if (academicCreditIds != null && academicCreditIds.Count() > 0)
            {
                // Not caching this information at this time. If we do decide to cash this info, we should cache all
                // the student information together (including grade viewing Restrictions).
                var academicCredits = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", academicCreditIds.ToArray());
                if (academicCredits == null || academicCreditIds.Count != academicCredits.Count)
                {
                    logger.Error("AcademicCreditRepository: ERROR: Failed to retrieve all credits from the database.  Id count " + academicCreditIds.Count + " Credits count " + academicCredits.Count);
                    var missingIds = academicCreditIds.Except(academicCredits.Select(c => c.Recordkey)).Distinct().ToList();
                    logger.Error("   Missing Ids :" + string.Join(",", missingIds));
                    throw new ArgumentOutOfRangeException("academicCreditIds", "Failed to retrieve all credits from the database.");
                }
                creditList = (await BuildCredits2Async(academicCredits)).ToList();
            }
            if (filter)
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse
                };
                if (includeDrops)
                {
                    filteredStatuses.Add(CreditStatus.Dropped);
                }
                return creditList.Where(ac => filteredStatuses.Contains(ac.Status));
            }
            else
            {
                return creditList;
            }
        }

        private async Task<IEnumerable<AcademicCreditMinimum>> BuildCredits2Async(Collection<StudentAcadCred> academicCreditData)
        {
            ICollection<AcademicCreditMinimum> credits = new List<AcademicCreditMinimum>();

            if (academicCreditData == null || academicCreditData.Count == 0) { return credits; }

            // Convert objects to dictionary values keyed by the record key.
            Dictionary<string, StudentAcadCred> dictCredits = null;

            if (academicCreditData != null)
            {
                dictCredits = academicCreditData.ToDictionary(f => f.Recordkey, f => f);
            }

            // Pre-fetch any courses from this set of academic credit data
            var courseIds = academicCreditData.Select(x => x.StcCourse).Distinct().ToList();
            //var courses = await CourseRepository.GetAsync(courseIds);
            //var coursesDictionary = courses.ToDictionary(x => x.Id, x => x);

            foreach (var cred in academicCreditData)
            {
                try
                {
                    AcademicCreditMinimum ac;

                    // if this is course credit, get course and section specific data                   
                    ac = new AcademicCreditMinimum(cred.Recordkey);
                    ac.GpaCredit = cred.StcGpaCred;
                    ac.GradePoints = cred.StcGradePts ?? 0;


                    // The credit status is in an association, get the FIRST one
                    // and translate to the CreditStatus enum we use in the domain
                    if (cred.StcStatus != null && cred.StcStatus.Any())
                    {
                        ac.Status = await ConvertCreditStatusAsync(cred.StcStatus.ElementAt(0));
                    }
                    else
                    {
                        ac.Status = CreditStatus.Unknown;
                    }

                    // Add academic credit statuses

                    if (cred.StcStatus != null && cred.StcStatus.Any())
                    {
                        foreach (var ss in cred.StcStatusesEntityAssociation)
                        {
                            try
                            {
                                var status = ss.StcStatusAssocMember;
                                var date = ss.StcStatusDateAssocMember;
                                var time = ss.StcStatusTimeAssocMember;
                                var reason = ss.StcStatusReasonAssocMember;
                                ac.AddStatus(status, date, time, reason);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, string.Format("An error occurred adding academic credit status for academic credit {0}", cred.Recordkey));
                            }
                        }
                    }

                    credits.Add(ac);
                }
                catch (Exception aex)
                {
                    // If any of the student's academic credits cannot be built throw an error and stop processing  
                    logger.Error(aex, string.Format("Unable to create all academic credits; error occurred building academic credit information for credit {0}", cred.Recordkey));
                    throw;
                }
            }

            return credits;
        }

        /// <summary>
        /// Gets non credits to return to EEDM StudentCourseTransferService
        /// </summary>
        /// <returns>A collection of AcademicCredit</returns>
        public async Task<Tuple<IEnumerable<StudentCourseTransfer>, int>> GetStudentCourseTransfersAsync(int offset, int limit, bool bypassCache)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllStudentEquivEvalsRecordsCache);
            List<StudentCourseTransfer> studentCourseTransfers = new List<StudentCourseTransfer>();

            if (limit == 0) limit = readSize;
            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "STUDENT.EQUIV.EVALS",
                offset,
                limit,
                AllStudentEquivEvalsRecordsCacheTimeout,
                async () =>
                {
                    selectionCriteria.Append("WITH STE.COURSE.ACAD.CRED NE '' AND WITH STE.INSTITUTION NE '' BY.EXP STE.COURSE.ACAD.CRED SAVING STE.COURSE.ACAD.CRED");

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = selectionCriteria.ToString()
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<StudentCourseTransfer>, int>(new List<StudentCourseTransfer>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();

            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<StudentCourseTransfer>, int>(new List<StudentCourseTransfer>(), 0);
            }

            // Bulk read StudentAcadCreds
            //var acadCredRecords = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", subList);
            var acadCredRecords = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", subList);

            if ((acadCredRecords.InvalidKeys != null && acadCredRecords.InvalidKeys.Any()) ||
                acadCredRecords.InvalidRecords != null && acadCredRecords.InvalidRecords.Any())
            {

                if (acadCredRecords.InvalidKeys.Any())
                {
                    exception.AddErrors(acadCredRecords.InvalidKeys
                        .Select(key => new RepositoryError("Bad.Data",
                        string.Format("Unable to locate the following STUDENT.ACAD.CRED key '{0}' from STE.COURSE.ACAD.CRED in STUDENT.EQUIV.EVALS.", key.ToString()))));
                }
                if (acadCredRecords.InvalidRecords.Any())
                {
                    exception.AddErrors(acadCredRecords.InvalidRecords
                       .Select(r => new RepositoryError("Bad.Data",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
            }

            // Bulk read StudentEquivEvals, which is used to determine if credit was granted based on a non-course equivalency
            string[] equivEvalIds = acadCredRecords.BulkRecordsRead.Select(stc => stc.StcStudentEquivEval).Distinct().ToArray();

            var equivRecords = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", equivEvalIds);

            if ((equivRecords.InvalidKeys != null && equivRecords.InvalidKeys.Any()) ||
                equivRecords.InvalidRecords != null && equivRecords.InvalidRecords.Any())
            {
                //var repositoryException = new RepositoryException();

                if (equivRecords.InvalidKeys.Any())
                {
                    exception.AddErrors(equivRecords.InvalidKeys
                        .Select(key => new RepositoryError("Bad.Data",
                        string.Format("Unable to locate the following key '{0}' from STUDENT.EQUIV.EVALS .", key.ToString()))));
                }
                if (equivRecords.InvalidRecords.Any())
                {
                    exception.AddErrors(equivRecords.InvalidRecords
                       .Select(r => new RepositoryError("Bad.Data",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
                //throw repositoryException;
            }
            // Combine each distinct acad cred/equiv eval combination to make a StudentCourseTransfer domain object
            // (acad cred/equiv evals have a possible many-to-one relationship)

            foreach (var acadCred in acadCredRecords.BulkRecordsRead)
            {

                StudentEquivEvals equivEval;

                try
                {
                    equivEval = equivRecords.BulkRecordsRead.First(ae => ae.Recordkey == acadCred.StcStudentEquivEval);
                    studentCourseTransfers.Add(await BuildStudentCourseTransfer(equivEval, acadCred));
                }
                catch (Exception e)
                {
                    exception.AddError(new RepositoryError("Bad.Data", e.Message)
                    {
                        SourceId = acadCred.StcStudentEquivEval

                    });
                }


            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return studentCourseTransfers.Any() ?
                new Tuple<IEnumerable<Domain.Student.Entities.StudentCourseTransfer>, int>(studentCourseTransfers, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.StudentCourseTransfer>, int>(new List<Domain.Student.Entities.StudentCourseTransfer>(), 0);
        }

        /// <summary>
        /// Gets one non credit by GUID to return to EEDM StudentCourseTransferService
        /// </summary>
        /// <returns>A collection of AcademicCredit</returns>
        public async Task<StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string guid, bool ignoreCache)
        {
            var recordKey = GetRecordInfoFromGuid(guid);
            string steKey;
            string stcKey;

            if (recordKey != null)
            {
                steKey = recordKey.PrimaryKey;
                stcKey = recordKey.SecondaryKey;
            }
            else
            {
                throw new KeyNotFoundException(string.Format("No student course transfer was found for guid [0]", guid));
            }
            StudentEquivEvals steRecord;
            StudentAcadCred stcRecord;

            try
            {
                steRecord = await DataReader.ReadRecordAsync<StudentEquivEvals>(steKey);
                if (steRecord == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate student equiv eval record with id of '{0}'.", steKey));
                }
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                var msg = string.Format("Unable to locate student equiv eval record with id of {0}: " + e.Message, steKey);
                logger.Error(msg);
                throw new KeyNotFoundException(msg);
            }


            try
            {
                stcRecord = await DataReader.ReadRecordAsync<StudentAcadCred>(stcKey);
                if (stcRecord == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate academic credit record with id of '{0}' that should exists for student equiv eval record with id of '{1}'.", stcKey, steKey));
                }
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                var msg = string.Format("Unable to locate academic credit record with id of {0} that should exists for student equiv eval" +
                                                                                    "record with id of {1}: " + e.Message
                                                                                        , stcKey, steKey);
                logger.Error(msg);
                throw new KeyNotFoundException(msg);
            }
            try
            {
                return await BuildStudentCourseTransfer(steRecord, stcRecord);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("Unable to locate student equiv eval record with id of '{0}'.", steKey));
            }

        }


        public async Task<StudentCourseTransfer> BuildStudentCourseTransfer(StudentEquivEvals equivEval, StudentAcadCred acadCred)
        {
            StudentCourseTransfer sct = new StudentCourseTransfer();



            sct.Guid = await GetGuidFromRecordInfoAsync("STUDENT.EQUIV.EVALS", equivEval.Recordkey, "STE.COURSE.ACAD.CRED", acadCred.Recordkey);
            sct.Id = equivEval.Recordkey;

            // Acad level guid
            sct.AcademicLevel = acadCred.StcAcadLevel;

            // Institution guid
            sct.TransferredFromInstitution = equivEval.SteInstitution;

            // Person guid
            sct.Student = acadCred.StcPersonId;

            // Course guid
            sct.Course = acadCred.StcCourse;

            // Academic program guids
            sct.AcademicPrograms = equivEval.SteAcadPrograms;

            // Term
            sct.AcademicPeriod = acadCred.StcTerm;

            // grade scheme
            sct.GradeScheme = acadCred.StcGradeScheme;

            // grade
            sct.Grade = acadCred.StcVerifiedGrade;

            // quality points
            if (acadCred.StcGradePts != null)
            {
                sct.QualityPoints = acadCred.StcGradePts;
            }

            // Credit Type
            sct.CreditType = acadCred.StcCredType;

            // Awarded credit
            if (acadCred.StcCred != null)
            {
                sct.AwardedCredit = acadCred.StcCred;
            }

            if (acadCred.StcEndDate != null)
            {
                sct.EquivalencyAppliedOn = acadCred.StcEndDate;
            }

            // status
            if (acadCred.StcStatus != null && acadCred.StcStatus.Any())
            {
                sct.Status = acadCred.StcStatus.FirstOrDefault();
            }

            return sct;

        }

        /// <summary>
        /// Gets PilotAcademicCredits, while doing a subset of the reads necessary to build an AcademicCredit
        /// </summary>
        /// <param name="academicCreditIds">A collection of academic credits to read</param>
        /// <param name="subset">A bitwise enum value representing which data to read</param>
        /// <param name="bestFit">Whether to fit non-term credit into terms</param>
        /// <param name="filter">Whether to filter out credits with certain statuses</param>
        /// <returns>A collection of PilotAcademicCredits</returns>
        public async Task<IEnumerable<PilotAcademicCredit>> GetPilotCreditsAsync(ICollection<string> academicCreditIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true)
        {
            var creditList = new List<PilotAcademicCredit>();
            if (academicCreditIds != null && academicCreditIds.Count() > 0)
            {
                // Not caching this information at this time. If we do decide to cash this info, we should cache all
                // the student information together (including grade viewing Restrictions).
                var academicCredits = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", academicCreditIds.ToArray());
                if (academicCredits == null || academicCreditIds.Count != academicCredits.Count)
                {
                    throw new ArgumentOutOfRangeException("academicCreditIds", "Failed to retrieve all credits from the database.");
                }
                var studentCourseSecIds = academicCredits.Select(sas => sas.StcStudentCourseSec).Distinct();

                Collection<StudentCourseSec> studentCourseSections;
                if ((subset & AcademicCreditDataSubset.StudentCourseSec) == AcademicCreditDataSubset.StudentCourseSec)
                    studentCourseSections = await DataReader.BulkReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray());
                else
                    studentCourseSections = new Collection<StudentCourseSec>();
                // Bulk read StudentEquivEvals, which is used to determine if credit was granted based on a non-course equivalency
                Collection<StudentEquivEvals> studentEquivEvals;
                if ((subset & AcademicCreditDataSubset.StudentEquivEvals) == AcademicCreditDataSubset.StudentEquivEvals)
                {
                    var studentEquivEvalIds = academicCredits.Select(sas => sas.StcStudentEquivEval).Distinct();
                    studentEquivEvals = await DataReader.BulkReadRecordAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", studentEquivEvalIds.ToArray());
                }
                else
                    studentEquivEvals = new Collection<StudentEquivEvals>();

                creditList = (await BuildPilotCreditsAsync(academicCredits, studentCourseSections, studentEquivEvals, bestFit)).ToList();
            }
            if (filter)
            {
                return creditList.Where(ac =>
                    (ac.Status == CreditStatus.Add ||
                    ac.Status == CreditStatus.New ||
                    ac.Status == CreditStatus.Preliminary ||
                    ac.Status == CreditStatus.Withdrawn ||
                    ac.Status == CreditStatus.TransferOrNonCourse));
            }
            else
            {
                return creditList;
            }
        }

        /// <summary>
        /// Gets PilotAcademicCredits, while doing a subset of the reads necessary to build an AcademicCredit.
        /// Includes dropped sections.
        /// </summary>
        /// <param name="academicCreditIds">A collection of academic credits to read</param>
        /// <param name="subset">A bitwise enum value representing which data to read</param>
        /// <param name="bestFit">Whether to fit non-term credit into terms</param>
        /// <param name="filter">Whether to filter out credits with certain statuses</param>
        /// <returns>A collection of PilotAcademicCredits</returns>
        public async Task<IEnumerable<PilotAcademicCredit>> GetPilotCredits2Async(ICollection<string> academicCreditIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true)
        {
            var creditList = new List<PilotAcademicCredit>();
            if (academicCreditIds != null && academicCreditIds.Count() > 0)
            {
                // Not caching this information at this time. If we do decide to cash this info, we should cache all
                // the student information together (including grade viewing Restrictions).
                var academicCredits = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", academicCreditIds.ToArray());
                if (academicCredits == null || academicCreditIds.Count != academicCredits.Count)
                {
                    throw new ArgumentOutOfRangeException("academicCreditIds", "Failed to retrieve all credits from the database.");
                }
                var studentCourseSecIds = academicCredits.Select(sas => sas.StcStudentCourseSec).Distinct();

                Collection<StudentCourseSec> studentCourseSections;
                if ((subset & AcademicCreditDataSubset.StudentCourseSec) == AcademicCreditDataSubset.StudentCourseSec)
                    studentCourseSections = await DataReader.BulkReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray());
                else
                    studentCourseSections = new Collection<StudentCourseSec>();
                // Bulk read StudentEquivEvals, which is used to determine if credit was granted based on a non-course equivalency
                Collection<StudentEquivEvals> studentEquivEvals;
                if ((subset & AcademicCreditDataSubset.StudentEquivEvals) == AcademicCreditDataSubset.StudentEquivEvals)
                {
                    var studentEquivEvalIds = academicCredits.Select(sas => sas.StcStudentEquivEval).Distinct();
                    studentEquivEvals = await DataReader.BulkReadRecordAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", studentEquivEvalIds.ToArray());
                }
                else
                    studentEquivEvals = new Collection<StudentEquivEvals>();

                creditList = (await BuildPilotCreditsAsync(academicCredits, studentCourseSections, studentEquivEvals, bestFit)).ToList();
            }
            if (filter)
            {
                return creditList.Where(ac =>
                    (ac.Status == CreditStatus.Add ||
                    ac.Status == CreditStatus.New ||
                    ac.Status == CreditStatus.Preliminary ||
                    ac.Status == CreditStatus.Withdrawn ||
                    ac.Status == CreditStatus.TransferOrNonCourse ||
                    ac.Status == CreditStatus.Dropped));
            }
            else
            {
                return creditList;
            }
        }

        /// <summary>
        /// get students AcademicCredit keys only.
        /// </summary>
        /// <param name="studentIds">List of Student IDs</param>
        /// <param name="bestFit">If set, all credit is put into a term that best fits within the dates</param>
        /// <param name="filter">if set, only return credits for the specified term.</param>
        /// <param name="includeDrops">Flag indicating whether or not to include dropped academic credits</param>
        /// <returns>Dictionary of Student Keys and AcademicCredit objects</returns>
        public async Task<Dictionary<string, List<AcademicCredit>>> GetAcademicCreditByStudentIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, bool includeDrops = false)
        {
            Dictionary<string, List<AcademicCredit>> studentAcadCredit = new Dictionary<string, List<AcademicCredit>>();

            try
            {
                if (studentIds != null && studentIds.Count() > 0)
                {
                    // Need to execute Select and Bulk Reads in chunks of no more than 5,000.
                    var academicCreditData = new List<AcademicCredit>();
                    for (int i = 0; i < studentIds.Count(); i += readSize)
                    {
                        var subList = studentIds.Skip(i).Take(readSize).ToArray();
                        string[] creditIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", "WITH STC.PERSON.ID = '?'", subList);
                        if (creditIds != null && creditIds.Count() > 0)
                        {
                            IEnumerable<AcademicCredit> creditData = await GetAsync(creditIds, bestFit, filter, includeDrops);
                            academicCreditData.AddRange(creditData);
                        }
                        else
                        {
                            // Log that we could not select data for this list of students
                            var message = "Could not select data from STUDENT.ACAD.CRED.  Index = '" + i + "', Readsize = '" + readSize + "'.";
                            logger.Warn(message);
                        }
                    }
                    // Put into dictionary to return to calling process.
                    if (academicCreditData != null)
                    {
                        foreach (var creditData in academicCreditData)
                        {
                            if (studentAcadCredit.ContainsKey(creditData.StudentId))
                            {
                                var creditDataList = studentAcadCredit[creditData.StudentId];
                                creditDataList.Add(creditData);
                                studentAcadCredit[creditData.StudentId] = creditDataList;
                            }
                            else
                            {
                                List<AcademicCredit> creditDataList = new List<AcademicCredit>();
                                creditDataList.Add(creditData);
                                studentAcadCredit.Add(creditData.StudentId, creditDataList);
                            }
                        }
                    }
                }
                return studentAcadCredit;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving academic credit for student with id {0}", studentIds);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occurred while retrieving academic credits");
                throw;
            }
        }

        /// <summary>
        /// Gets PilotAcademicCredits, while doing a subset of the reads necessary to build an AcademicCredit
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="subset">A bitwise enum value representing which data to read</param>
        /// <param name="bestFit">Whether to fit non-term credit into terms</param>
        /// <param name="filter">Whether to filter out credits with certain statuses</param>
        /// <param name="term">The optional term to filter the academic credit read (and returned data) to</param>
        /// <returns>A dictionary of PilotAcademicCredits keyed by student id</returns>
        public async Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIdsAsync(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null)
        {
            Dictionary<string, List<PilotAcademicCredit>> studentAcadCredit = new Dictionary<string, List<PilotAcademicCredit>>();

            AcademicCreditDataSubset dataToRead = AcademicCreditDataSubset.None;
            dataToRead = dataToRead | subset;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // Need to execute Select and Bulk Reads in chunks of no more than 5,000.
                var academicCreditData = new List<PilotAcademicCredit>();
                for (int i = 0; i < studentIds.Count(); i += readSize)
                {
                    var subList = studentIds.Skip(i).Take(readSize).ToArray();
                    string[] creditIds;
                    if (!string.IsNullOrEmpty(term))
                    {
                        string searchString = "WITH STC.TERM = '" + term + "' AND STC.PERSON.ID = '?'";
                        creditIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", searchString, subList);
                    }
                    else
                        creditIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", "WITH STC.PERSON.ID = '?'", subList);
                    if (creditIds != null && creditIds.Count() > 0)
                    {
                        IEnumerable<PilotAcademicCredit> creditData = await GetPilotCreditsAsync(creditIds, dataToRead, bestFit, filter);
                        academicCreditData.AddRange(creditData);
                    }
                    else
                    {
                        // Log that we could not select data for this list of students
                        var message = "Could not select data from STUDENT.ACAD.CRED.  Index = '" + i + "', Readsize = '" + readSize + "'.";
                        logger.Warn(message);
                    }
                }
                // Put into dictionary to return to calling process.
                if (academicCreditData != null)
                {
                    foreach (var creditData in academicCreditData)
                    {
                        if (studentAcadCredit.ContainsKey(creditData.StudentId))
                        {
                            var creditDataList = studentAcadCredit[creditData.StudentId];
                            creditDataList.Add(creditData);
                            studentAcadCredit[creditData.StudentId] = creditDataList;
                        }
                        else
                        {
                            List<PilotAcademicCredit> creditDataList = new List<PilotAcademicCredit>();
                            creditDataList.Add(creditData);
                            studentAcadCredit.Add(creditData.StudentId, creditDataList);
                        }
                    }
                }
            }
            return studentAcadCredit;
        }

        /// <summary>
        /// Gets PilotAcademicCredits, while doing a subset of the reads necessary to build an AcademicCredit
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="subset">A bitwise enum value representing which data to read</param>
        /// <param name="bestFit">Whether to fit non-term credit into terms</param>
        /// <param name="filter">Whether to filter out credits with certain statuses</param>
        /// <param name="term">The optional term to filter the academic credit read (and returned data) to</param>
        /// <returns>A dictionary of PilotAcademicCredits keyed by student id</returns>
        public async Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIds2Async(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null)
        {
            Dictionary<string, List<PilotAcademicCredit>> studentAcadCredit = new Dictionary<string, List<PilotAcademicCredit>>();

            AcademicCreditDataSubset dataToRead = AcademicCreditDataSubset.None;
            dataToRead = dataToRead | subset;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // Need to execute Select and Bulk Reads in chunks of no more than 5,000.
                var academicCreditData = new List<PilotAcademicCredit>();
                for (int i = 0; i < studentIds.Count(); i += readSize)
                {
                    var subList = studentIds.Skip(i).Take(readSize).ToArray();
                    string[] creditIds;
                    if (!string.IsNullOrEmpty(term))
                    {
                        string searchString = "WITH STC.TERM = '" + term + "' AND STC.PERSON.ID = '?'";
                        creditIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", searchString, subList);
                    }
                    else
                        creditIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", "WITH STC.PERSON.ID = '?'", subList);
                    if (creditIds != null && creditIds.Count() > 0)
                    {
                        IEnumerable<PilotAcademicCredit> creditData = await GetPilotCredits2Async(creditIds, dataToRead, bestFit, filter);
                        academicCreditData.AddRange(creditData);
                    }
                    else
                    {
                        // Log that we could not select data for this list of students
                        var message = "Could not select data from STUDENT.ACAD.CRED.  Index = '" + i + "', Readsize = '" + readSize + "'.";
                        logger.Warn(message);
                    }
                }
                // Put into dictionary to return to calling process.
                if (academicCreditData != null)
                {
                    foreach (var creditData in academicCreditData)
                    {
                        if (studentAcadCredit.ContainsKey(creditData.StudentId))
                        {
                            var creditDataList = studentAcadCredit[creditData.StudentId];
                            creditDataList.Add(creditData);
                            studentAcadCredit[creditData.StudentId] = creditDataList;
                        }
                        else
                        {
                            List<PilotAcademicCredit> creditDataList = new List<PilotAcademicCredit>();
                            creditDataList.Add(creditData);
                            studentAcadCredit.Add(creditData.StudentId, creditDataList);
                        }
                    }
                }
            }
            return studentAcadCredit;
        }

        private async Task<IEnumerable<AcademicCredit>> BuildCreditsAsync(Collection<StudentAcadCred> academicCreditData, Collection<StudentAcadCredCc> academicCreditCcData, Collection<StudentCourseSec> studentCourseSecData, Collection<StudentCourseSecCc> studentCourseSecCcData, Collection<StudentEquivEvals> studentEquivEvals, bool bestFit = false)
        {
            ICollection<AcademicCredit> credits = new List<AcademicCredit>();

            if (academicCreditData == null || academicCreditData.Count == 0) { return credits; }

            // Convert objects to dictionary values keyed by the record key.
            Dictionary<string, StudentAcadCred> dictCredits = null;
            Dictionary<string, StudentAcadCredCc> dictCreditsCc = null;
            Dictionary<string, StudentCourseSec> dictCourseSections = null;
            Dictionary<string, StudentCourseSecCc> dictCourseSectionsCc = null;
            Dictionary<string, StudentEquivEvals> dictEquivEvals = null;

            if (academicCreditData != null)
            {
                dictCredits = academicCreditData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (academicCreditCcData != null)
            {
                dictCreditsCc = academicCreditCcData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (studentCourseSecData != null)
            {
                dictCourseSections = studentCourseSecData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (studentCourseSecCcData != null)
            {
                dictCourseSectionsCc = studentCourseSecCcData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (studentEquivEvals != null && studentEquivEvals.Count > 0)
            {
                dictEquivEvals = studentEquivEvals.ToDictionary(f => f.Recordkey, f => f);
            }

            // Pre-fetch any courses from this set of academic credit data
            var courseIds = academicCreditData.Select(x => x.StcCourse).Distinct().ToList();
            var courses = await CourseRepository.GetAsync(courseIds);
            var coursesDictionary = courses.ToDictionary(x => x.Id, x => x);

            foreach (var cred in academicCreditData)
            {
                try
                {
                    AcademicCredit ac;

                    // if this is course credit, get course and section specific data
                    if (!string.IsNullOrEmpty(cred.StcCourse))
                    {
                        // It is also possible, in STAC-created credits, to have no SCS.
                        string sectionId = null;
                        string studentCourseSectionId = null;
                        GradingType creditGradingType = GradingType.Graded;
                        string location = null;
                        DateTime? dateLastAttended = null;
                        bool? neverAttended = null;
                        // capture the midterm grades from SCS, along with times from SCSCC
                        List<MidTermGrade> mtgrades = new List<MidTermGrade>();
                        if (!string.IsNullOrEmpty(cred.StcStudentCourseSec))
                        {
                            // var scs = studentCourseSecData.Where(s => s.Recordkey == cred.StcStudentCourseSec).FirstOrDefault();
                            StudentCourseSec scs = null;
                            StudentCourseSecCc scscc = null;
                            if (dictCourseSections != null && dictCourseSections.ContainsKey(cred.StcStudentCourseSec) && dictCourseSections[cred.StcStudentCourseSec] != null)
                            {
                                scs = dictCourseSections[cred.StcStudentCourseSec];
                            }
                            // var scscc = studentCourseSecCcData.Where(s => s.Recordkey == cred.StcStudentCourseSec).FirstOrDefault();
                            if (dictCourseSectionsCc != null && dictCourseSectionsCc.ContainsKey(cred.StcStudentCourseSec) && dictCourseSectionsCc[cred.StcStudentCourseSec] != null)
                            {
                                scscc = dictCourseSectionsCc[cred.StcStudentCourseSec];
                            }

                            if (scs != null)
                            {
                                sectionId = scs.ScsCourseSection;
                                studentCourseSectionId = scs.Recordkey;
                                if (scs.ScsPassAudit == "A")
                                {
                                    creditGradingType = GradingType.Audit;
                                }
                                if (scs.ScsPassAudit == "P")
                                {
                                    creditGradingType = GradingType.PassFail;
                                }
                                location = scs.ScsLocation;
                                dateLastAttended = scs.ScsLastAttendDate;
                                neverAttended = !string.IsNullOrEmpty(scs.ScsNeverAttendedFlag) && (scs.ScsNeverAttendedFlag.ToUpper() == "Y");
                                // get the midterm grades here? we have the scs at hand
                                DateTime? timePart = null;
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade1))
                                {
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime1; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate1, timePart);
                                    mtgrades.Add(new MidTermGrade(1, scs.ScsMidTermGrade1, gdt));
                                }
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade2))
                                {
                                    timePart = null;
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime2; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate2, timePart);
                                    mtgrades.Add(new MidTermGrade(2, scs.ScsMidTermGrade2, gdt));
                                }
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade3))
                                {
                                    timePart = null;
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime3; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate3, timePart);
                                    mtgrades.Add(new MidTermGrade(3, scs.ScsMidTermGrade3, gdt));
                                }
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade4))
                                {
                                    timePart = null;
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime4; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate4, timePart);
                                    mtgrades.Add(new MidTermGrade(4, scs.ScsMidTermGrade4, gdt));
                                }
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade5))
                                {
                                    timePart = null;
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime5; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate5, timePart);
                                    mtgrades.Add(new MidTermGrade(5, scs.ScsMidTermGrade5, gdt));
                                }
                                if (!string.IsNullOrEmpty(scs.ScsMidTermGrade6))
                                {
                                    if (scscc != null) { timePart = scscc.ScsccMidGradeTime6; }
                                    DateTimeOffset? gdt = GetCompositeDateTime(scs.ScsMidGradeDate6, timePart);
                                    mtgrades.Add(new MidTermGrade(6, scs.ScsMidTermGrade6, gdt));
                                }
                            }
                            else
                            {
                                var errorMessage = "Unable to access STUDENT.COURSE.SEC with ID " + cred.StcStudentCourseSec + " referenced by STUDENT.ACAD.CRED " + cred.Recordkey + " for student ID " + cred.StcPersonId;
                                logger.Error(errorMessage);
                            }
                        }
                        Course c = null;
                        // Get the course record out of the dictionary
                        if (coursesDictionary.TryGetValue(cred.StcCourse, out c))
                        {
                            ac = new AcademicCredit(cred.Recordkey, c, sectionId) { StudentCourseSectionId = studentCourseSectionId };
                        }
                        else
                        {
                            // Academic credit had a link to a course that is not good.  Build the credit anyway.
                            var errorMessage = "Unable to access COURSES record for " + cred.StcCourse;
                            logger.Error(errorMessage);
                            ac = new AcademicCredit(cred.Recordkey) { StudentCourseSectionId = studentCourseSectionId };
                        }

                        ac.GradingType = creditGradingType;
                        foreach (MidTermGrade mtg in mtgrades)
                        {
                            ac.AddMidTermGrade(mtg);
                        }
                        ac.Location = location;
                        ac.LastAttendanceDate = dateLastAttended;
                        ac.NeverAttended = neverAttended;
                    }
                    else
                    {
                        // This is a noncourse, transfer, or other kind of credit
                        ac = new AcademicCredit(cred.Recordkey);
                    }

                    ac.AdjustedCredit = cred.StcAltcumContribCmplCred;
                    ac.AdjustedGpaCredit = cred.StcAltcumContribGpaCred ?? 0;
                    ac.AdjustedGradePoints = cred.StcAltcumContribGradePts ?? 0;
                    ac.CompletedCredit = cred.StcCmplCred;
                    ac.AttemptedCredit = cred.StcAttCred ?? 0;
                    ac.CanBeReplaced = cred.StcAllowReplFlag == "Y" ? true : false;
                    ac.ContinuingEducationUnits = cred.StcCeus ?? 0;
                    ac.CourseName = cred.StcCourseName;
                    ac.Title = cred.StcTitle;
                    ac.Credit = cred.StcCred ?? 0;
                    ac.EndDate = cred.StcEndDate;
                    ac.StartDate = cred.StcStartDate;
                    ac.GpaCredit = cred.StcGpaCred;
                    ac.GradePoints = cred.StcGradePts ?? 0;
                    ac.GradeSchemeCode = cred.StcGradeScheme;
                    ac.CourseLevelCode = cred.StcCourseLevel;
                    ac.AcademicLevelCode = cred.StcAcadLevel;
                    ac.SectionNumber = cred.StcSectionNo;
                    ac.Mark = cred.StcMark;
                    ac.StudentId = cred.StcPersonId;
                    ac.FinalGradeId = cred.StcFinalGrade;
                    ac.FinalGradeExpirationDate = cred.StcGradeExpireDate;


                    // Use the departments associated to the specific student academic credit
                    foreach (var dept in cred.StcDepts)
                    {
                        try
                        {
                            ac.AddDepartment(dept);
                        }
                        catch (Exception ex)
                        {
                            // If the department is already in the list just keep going. 
                            logger.Error(ex, "Department is already in the list.");
                        }

                    }
                    // If the academic credit has a course but ends up having no departments then use the departments from the course
                    if (ac.Course != null && ac.DepartmentCodes.Count() == 0)
                    {
                        foreach (var d in ac.Course.DepartmentCodes)
                        {
                            try
                            {
                                ac.AddDepartment(d);
                            }
                            catch (Exception ex)
                            {
                                // If the department is already in the list just keep going. 
                                logger.Error(ex, "Department is already in the list.");
                            }
                        }
                    }


                    //add divisions and schools
                    if (cred.StcSchools != null && cred.StcSchools.Count > 0)
                    {
                        ac.Schools.AddRange(cred.StcSchools.Where(s => !string.IsNullOrEmpty(s)));
                    }
                    if (cred.StcDivisions != null && cred.StcDivisions.Count > 0)
                    {
                        ac.Divisions.AddRange(cred.StcDivisions.Where(d => !string.IsNullOrEmpty(d)));
                    }


                    // The credit status is in an association, get the FIRST one
                    // and translate to the CreditStatus enum we use in the domain
                    if (cred.StcStatus != null && cred.StcStatus.Count > 0)
                    {
                        ac.Status = await ConvertCreditStatusAsync(cred.StcStatus.ElementAt(0));
                        if (cred.StcStatusDate != null && cred.StcStatusDate.Count > 0)
                        {
                            ac.StatusDate = cred.StcStatusDate.ElementAt(0);
                        }
                    }
                    else
                    {
                        ac.Status = CreditStatus.Unknown;
                    }

                    // Add academic credit statuses

                    if (cred.StcStatus != null && cred.StcStatus.Count > 0)
                    {
                        foreach (var ss in cred.StcStatusesEntityAssociation)
                        {
                            try
                            {
                                var status = ss.StcStatusAssocMember;
                                var date = ss.StcStatusDateAssocMember;
                                var time = ss.StcStatusTimeAssocMember;
                                var reason = ss.StcStatusReasonAssocMember;
                                ac.AddStatus(status, date, time, reason);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, string.Format("An error occurred adding academic credit status for academic credit {0}", cred.Recordkey));
                            }
                        }
                    }

                    // Determine if a credit was granted based on a non-course equivalency
                    ac.IsNonCourse = false;
                    // If StcStudentEquivEval has a value, use it to hop over to STUDENT.EQUIV.EVALS
                    if (!string.IsNullOrEmpty(cred.StcStudentEquivEval) && dictEquivEvals != null)
                        if (dictEquivEvals.ContainsKey(cred.StcStudentEquivEval) && dictEquivEvals[cred.StcStudentEquivEval] != null)
                        {
                            {
                                // A collection of StudentEquivEvals was passed in as a parameter, check if the StudentEquivEval for this credit is in the collection
                                var studentEquivEval = dictEquivEvals[cred.StcStudentEquivEval];
                                // If the StudentEquivEvals record has SteStudentNonCourse set, then this is a non-course
                                if (studentEquivEval != null && !string.IsNullOrEmpty(studentEquivEval.SteStudentNonCourse))
                                {
                                    ac.IsNonCourse = true;
                                }
                            }
                        }

                    ac.SubjectCode = cred.StcSubject;
                    ac.TermCode = cred.StcTerm;

                    // to bestFit, we need a termList, and the cred start date needs a value. Otherwise the item will not be placed into a best fit term and the term will be left blank.

                    if (bestFit)
                    {
                        if (string.IsNullOrEmpty(ac.TermCode) && cred.StcStartDate.HasValue)
                        {
                            // Fetch this once, and only once needed.  Planning terms take precedence within each year. 
                            // Then the terms are ordered by reporting year, then planning vs nonplanning, then by equence so that the earliest matching terms will be first in list
                            if (termList == null) termList = termRepository.Get().OrderBy(t => t.ReportingYear).ThenByDescending(t => t.ForPlanning).ThenBy(t => t.Sequence);
                            if (termList != null && termList.Count() > 0)
                            {
                                // Collect all terms that meet one of the following criterias:
                                // A. Select terms where the term’s start date is on or before the credit start date and the term’s end date is on or after the credit start date.
                                // B. If the credit has an end date - select the terms where the term's start date is on or AFTER the credit's start date and the term's start date is on or before the credit end date.
                                // C. If the credit does not have an end date - select any term that starts on or after the credit's start date. This will pick up future terms but these
                                //    will generally only come into play when the credit's start date doesn't directly fall into any other term's date range and is just a last resort to try to find
                                //    a term that this can be placed into.
                                // Because the terms are ordered above those terms that are "planning terms" will take precedence. And the term with the earliest reporting year and sequence will
                                // be chosen.
                                var testTerms = termList.Where(t => ((t.StartDate.CompareTo(cred.StcStartDate.Value) <= 0 && t.EndDate.CompareTo(cred.StcStartDate.Value) >= 0) ||
                                        (t.StartDate.CompareTo(cred.StcStartDate.Value) >= 0 && (cred.StcEndDate.HasValue && t.StartDate.CompareTo(cred.StcEndDate.Value) <= 0)) ||
                                        (t.StartDate.CompareTo(cred.StcStartDate.Value) >= 0 && !cred.StcEndDate.HasValue)));
                                if (testTerms != null && testTerms.Count() > 0)
                                {
                                    ac.TermCode = testTerms.First().Code;
                                }
                            }
                        }
                    }

                    // Credit type ("I"nstitutional, "T"ransfer, etc.) are rendered unto the CreditType enum
                    ac.Type = await GetCreditTypeAsync(cred.StcCredType);
                    ac.LocalType = cred.StcCredType;
                    if (!string.IsNullOrEmpty(cred.StcVerifiedGrade))
                    {
                        try
                        {
                            ac.VerifiedGrade = (await GetGradesAsync())[cred.StcVerifiedGrade];
                            if (ac.VerifiedGrade != null)
                            {
                                DateTime? timePart = null;
                                if (dictCreditsCc != null && dictCreditsCc.ContainsKey(cred.Recordkey) && dictCreditsCc[cred.Recordkey] != null)
                                {
                                    // var stccc = academicCreditCcData.Where(a => a.Recordkey == cred.Recordkey).FirstOrDefault();
                                    var stccc = dictCreditsCc[cred.Recordkey];

                                    if (stccc != null)
                                    {
                                        timePart = stccc.StcccVerifiedGradeTime;
                                    }
                                }
                                ac.VerifiedGradeTimestamp = GetCompositeDateTime(cred.StcVerifiedGradeDate, timePart);

                            }
                        }
                        catch (ColleagueSessionExpiredException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // Log the message only.
                            var errorMessage = string.Format("ERROR: Grade Code '{0}' on Academic Credit record '{1}' has no match in the grades file.", cred.StcVerifiedGrade, cred.Recordkey);
                            logger.Error(ex, errorMessage);
                        }
                    }

                    // Registration billing needs access to all credits, regardless of status, so any filtering of credits is done
                    // by the Coordination services that use this

                    // Indicate if this academic credit is replaced.
                    if (!String.IsNullOrEmpty(cred.StcReplCode)) { ac.ReplacedStatus = ReplacedStatus.Replaced; }
                    // List of other academic credits involved in the replacement
                    ac.RepeatAcademicCreditIds = cred.StcRepeatedAcadCred;

                    credits.Add(ac);
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception aex)
                {
                    // If any of the student's academic credits cannot be built throw an error and stop processing  
                    logger.Error(aex, string.Format("Unable to create all academic credits; error occurred building academic credit information for credit {0}", cred.Recordkey));
                    throw;
                }
            }

            return credits;
        }

        /// <summary>
        /// Builds PilotAcademicCredits, which don't require all of the data usually provided in an AcademicCredit
        /// </summary>
        /// <param name="academicCreditData">StudentAcadCred records to process</param>
        /// <param name="studentCourseSecData">Optional records from StudentCourseSec</param>
        /// <param name="studentEquivEvals">Optional records from StudentEquivEvals</param>
        /// <param name="bestFit">Whether to fit non-term credits into a term</param>
        /// <returns>A collection of PilotAcademicCredits</returns>
        private async Task<IEnumerable<PilotAcademicCredit>> BuildPilotCreditsAsync(Collection<StudentAcadCred> academicCreditData, Collection<StudentCourseSec> studentCourseSecData, Collection<StudentEquivEvals> studentEquivEvals, bool bestFit = false)
        {
            ICollection<PilotAcademicCredit> credits = new List<PilotAcademicCredit>();

            if (academicCreditData == null || academicCreditData.Count == 0) { return credits; }

            // Convert objects to dictionary values keyed by the record key.
            Dictionary<string, StudentAcadCred> dictCredits = null;
            Dictionary<string, StudentCourseSec> dictCourseSections = null;
            Dictionary<string, StudentEquivEvals> dictEquivEvals = null;
            if (academicCreditData != null)
            {
                dictCredits = academicCreditData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (studentCourseSecData != null)
            {
                dictCourseSections = studentCourseSecData.ToDictionary(f => f.Recordkey, f => f);
            }
            if (studentEquivEvals != null && studentEquivEvals.Count > 0)
            {
                dictEquivEvals = studentEquivEvals.ToDictionary(f => f.Recordkey, f => f);
            }

            foreach (var cred in academicCreditData)
            {
                PilotAcademicCredit ac;
                ac = new PilotAcademicCredit(cred.Recordkey);

                // if this is course credit, get course and section specific data
                if (!string.IsNullOrEmpty(cred.StcCourse))
                {
                    // It is also possible, in STAC-created credits, to have no SCS.
                    GradingType creditGradingType = GradingType.Graded;
                    List<MidTermGrade> mtgrades = new List<MidTermGrade>();
                    if (!string.IsNullOrEmpty(cred.StcStudentCourseSec))
                    {
                        StudentCourseSec scs = null;
                        if (dictCourseSections != null && dictCourseSections.ContainsKey(cred.StcStudentCourseSec) && dictCourseSections[cred.StcStudentCourseSec] != null)
                        {
                            scs = dictCourseSections[cred.StcStudentCourseSec];
                        }

                        if (scs != null)
                        {
                            ac.SectionId = scs.ScsCourseSection;
                            if (scs.ScsPassAudit == "A")
                            {
                                creditGradingType = GradingType.Audit;
                            }
                            if (scs.ScsPassAudit == "P")
                            {
                                creditGradingType = GradingType.PassFail;
                            }
                            ac.Location = scs.ScsLocation;
                        }
                        ac.GradingType = creditGradingType;

                        #region Midterm Grades
                        if (scs != null)
                        {
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade1))
                            {
                                mtgrades.Add(new MidTermGrade(1, scs.ScsMidTermGrade1, null));
                            }
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade2))
                            {
                                mtgrades.Add(new MidTermGrade(2, scs.ScsMidTermGrade2, null));
                            }
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade3))
                            {
                                mtgrades.Add(new MidTermGrade(3, scs.ScsMidTermGrade3, null));
                            }
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade4))
                            {
                                mtgrades.Add(new MidTermGrade(4, scs.ScsMidTermGrade4, null));
                            }
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade5))
                            {
                                mtgrades.Add(new MidTermGrade(5, scs.ScsMidTermGrade5, null));
                            }
                            if (!string.IsNullOrEmpty(scs.ScsMidTermGrade6))
                            {
                                mtgrades.Add(new MidTermGrade(6, scs.ScsMidTermGrade6, null));
                            }
                            foreach (MidTermGrade mtg in mtgrades)
                            {
                                ac.AddMidTermGrade(mtg);
                            }
                        }
                        #endregion
                    }
                }

                // Verified grades can exist even with no midterms / student course sec
                ac.HasVerifiedGrade = false;
                if (!string.IsNullOrEmpty(cred.StcVerifiedGrade))
                {
                    ac.VerifiedGradeId = cred.StcVerifiedGrade;
                    ac.HasVerifiedGrade = true;
                }

                ac.AdjustedCredit = cred.StcAltcumContribCmplCred ?? 0;
                ac.AdjustedGpaCredit = cred.StcAltcumContribGpaCred ?? 0;
                ac.AdjustedGradePoints = cred.StcAltcumContribGradePts ?? 0;
                ac.CompletedCredit = cred.StcCmplCred ?? 0;
                ac.AttemptedCredit = cred.StcAttCred ?? 0;
                ac.CanBeReplaced = cred.StcAllowReplFlag == "Y" ? true : false;
                ac.ContinuingEducationUnits = cred.StcCeus ?? 0;
                ac.CourseName = cred.StcCourseName;
                ac.Title = cred.StcTitle;
                ac.Credit = cred.StcCred ?? 0;
                ac.EndDate = cred.StcEndDate;
                ac.StartDate = cred.StcStartDate;
                ac.GpaCredit = cred.StcGpaCred ?? 0;
                ac.GradePoints = cred.StcGradePts ?? 0;
                ac.CumulativeCompletedCredit = cred.StcCumContribCmplCred ?? 0;
                ac.CumulativeGpaCredit = cred.StcCumContribGpaCred ?? 0;
                ac.CumulativeGradePoints = cred.StcCumContribGradePts ?? 0;
                ac.GradeSchemeCode = cred.StcGradeScheme;
                ac.CourseLevelCode = cred.StcCourseLevel;
                ac.AcademicLevelCode = cred.StcAcadLevel;
                ac.SectionNumber = cred.StcSectionNo;
                ac.Mark = cred.StcMark;
                ac.StudentId = cred.StcPersonId;

                // Use the departments associated to the specific student academic credit
                foreach (var dept in cred.StcDepts)
                {
                    ac.AddDepartment(dept);
                }
                // If the academic credit has a course but ends up having no departments then use the departments from the course
                if (ac.Course != null && ac.DepartmentCodes.Count() == 0)
                {
                    foreach (var d in ac.Course.DepartmentCodes) { ac.AddDepartment(d); }
                }

                // The credit status is in an association, get the FIRST one
                // and translate to the CreditStatus enum we use in the domain
                if (cred.StcStatus != null && cred.StcStatus.Count > 0)
                {
                    ac.Status = await ConvertCreditStatusAsync(cred.StcStatus.ElementAt(0));
                    if (cred.StcStatusDate != null && cred.StcStatusDate.Count > 0)
                    {
                        ac.StatusDate = cred.StcStatusDate.ElementAt(0);
                    }
                }
                else
                {
                    ac.Status = CreditStatus.Unknown;
                }

                // Add academic credit statuses
                if (cred.StcStatus != null && cred.StcStatus.Count > 0)
                {
                    foreach (var ss in cred.StcStatusesEntityAssociation)
                    {
                        try
                        {
                            var status = ss.StcStatusAssocMember;
                            var date = ss.StcStatusDateAssocMember;
                            var time = ss.StcStatusTimeAssocMember;
                            var reason = ss.StcStatusReasonAssocMember;
                            ac.AddStatus(status, date, time, reason);
                        }
                        catch (Exception e)
                        {
                            LogDataError("Student Academic Credit", cred.Recordkey, ss, e);
                        }
                    }
                }

                // Determine if a credit was granted based on a non-course equivalency
                ac.IsNonCourse = false;
                // If StcStudentEquivEval has a value, use it to hop over to STUDENT.EQUIV.EVALS
                if (!string.IsNullOrEmpty(cred.StcStudentEquivEval) && dictEquivEvals != null)
                    if (dictEquivEvals.ContainsKey(cred.StcStudentEquivEval) && dictEquivEvals[cred.StcStudentEquivEval] != null)
                    {
                        {
                            // A collection of StudentEquivEvals was passed in as a parameter, check if the StudentEquivEval for this credit is in the collection
                            var studentEquivEval = dictEquivEvals[cred.StcStudentEquivEval];
                            // If the StudentEquivEvals record has SteStudentNonCourse set, then this is a non-course
                            if (studentEquivEval != null && !string.IsNullOrEmpty(studentEquivEval.SteStudentNonCourse))
                            {
                                ac.IsNonCourse = true;
                            }
                        }
                    }

                ac.SubjectCode = cred.StcSubject;
                ac.TermCode = cred.StcTerm;

                // to bestFit, we need a termList, and the cred start date needs a value. Otherwise the item will not be placed into a best fit term and the term will be left blank.

                if (bestFit)
                {
                    if (string.IsNullOrEmpty(ac.TermCode) && cred.StcStartDate.HasValue)
                    {
                        // Fetch this once, and only once needed.  Planning terms take precedence within each year. 
                        // Then the terms are ordered by reporting year, then planning vs nonplanning, then by equence so that the earliest matching terms will be first in list
                        if (termList == null) termList = termRepository.Get().OrderBy(t => t.ReportingYear).ThenByDescending(t => t.ForPlanning).ThenBy(t => t.Sequence);
                        if (termList != null && termList.Count() > 0)
                        {
                            // Collect all terms that meet one of the following criterias:
                            // A. Select terms where the term’s start date is on or before the credit start date and the term’s end date is on or after the credit start date.
                            // B. If the credit has an end date - select the terms where the term's start date is on or AFTER the credit's start date and the term's start date is on or before the credit end date.
                            // C. If the credit does not have an end date - select any term that starts on or after the credit's start date. This will pick up future terms but these
                            //    will generally only come into play when the credit's start date doesn't directly fall into any other term's date range and is just a last resort to try to find
                            //    a term that this can be placed into.
                            // Because the terms are ordered above those terms that are "planning terms" will take precedence. And the term with the earliest reporting year and sequence will
                            // be chosen.
                            var testTerms = termList.Where(t => ((t.StartDate.CompareTo(cred.StcStartDate.Value) <= 0 && t.EndDate.CompareTo(cred.StcStartDate.Value) >= 0) ||
                                    (t.StartDate.CompareTo(cred.StcStartDate.Value) >= 0 && (cred.StcEndDate.HasValue && t.StartDate.CompareTo(cred.StcEndDate.Value) <= 0)) ||
                                    (t.StartDate.CompareTo(cred.StcStartDate.Value) >= 0 && !cred.StcEndDate.HasValue)));
                            if (testTerms != null && testTerms.Count() > 0)
                            {
                                ac.TermCode = testTerms.First().Code;
                            }
                        }
                    }
                }

                // Credit type ("I"nstitutional, "T"ransfer, etc.) are rendered unto the CreditType enum
                ac.Type = await GetCreditTypeAsync(cred.StcCredType);
                ac.LocalType = cred.StcCredType;

                // Indicate if this academic credit is replaced.
                if (!String.IsNullOrEmpty(cred.StcReplCode)) { ac.ReplacedStatus = ReplacedStatus.Replaced; }
                // List of other academic credits involved in the replacement
                ac.RepeatAcademicCreditIds = cred.StcRepeatedAcadCred;

                credits.Add(ac);

            }

            return credits;
        }

        private DateTimeOffset? GetCompositeDateTime(DateTime? datePart, DateTime? timePart)
        {
            // If date and time are provided, calculate the offset using the colleague time zone
            DateTimeOffset? dateTimeOffset = null;
            if (timePart != null && datePart != null)
            {
                dateTimeOffset = timePart.ToPointInTimeDateTimeOffset(datePart, colleagueTimeZone);
            }
            else
            {
                // Neither date nor time returns null.
                // Date provided with no time returns a DateTimeOffset with zero time
                if (datePart != null)
                {
                    int iYr = datePart.Value.Year;
                    int iMo = datePart.Value.Month;
                    int iDa = datePart.Value.Day;
                    DateTime dateOnly = new DateTime(iYr, iMo, iDa);
                    dateTimeOffset = new DateTimeOffset(dateOnly, new TimeSpan(0, 0, 0));
                    // Did not use this code because it results in the same value as above, but with the Colleague offset,
                    // which will be converted to another time by the serializer. We want dead midnight to be sent when no time
                    // is provided.
                    //dateTimeOffset = dateOnly.ToPointInTimeDateTimeOffset(dateOnly, colleagueTimeZone);
                }
            }
            return dateTimeOffset;
        }


        public async Task<CreditType> GetCreditTypeAsync(string typecode)
        {
            if (types.Count() == 0)
            {
                // STC.TYPE => CRED.TYPES => CRTP.CODE, CRTP.CATEGORY

                var creditTypes = await GetOrAddToCacheAsync<List<CredTypes>>("AllCreditTypes",
                    async () =>
                    {
                        var credTypes = await DataReader.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "");
                        return credTypes.ToList();
                    }
                , Level1CacheTimeoutValue);



                // There is a Valcode CRED.TYPE.TYPES but it's not used for anything.  In Colleague, things
                // either have a CRTP.CATEGORY of "I" or they don't.

                if (creditTypes != null)
                {
                    foreach (var ct in creditTypes)
                    {
                        CreditType type;

                        switch (ct.CrtpCategory)
                        {
                            case "I":
                                {
                                    type = CreditType.Institutional;
                                    break;
                                }
                            case "C":
                                {
                                    type = CreditType.ContinuingEducation;
                                    break;
                                }
                            case "T":
                                {
                                    type = CreditType.Transfer;
                                    break;
                                }
                            default:
                                {
                                    type = CreditType.Other;
                                    break;
                                }

                        }
                        types.Add(ct.Recordkey, type);
                    }
                }
                else
                {
                    throw new ColleagueWebApiException("Transaction failed to return credit types");
                }
            }

            if (typecode != null && typecode != "" && types.Keys.Contains(typecode))
            {
                return types[typecode];
            }
            else
            {
                return CreditType.Other;
            }

        }

        // Translate the status string to an enum 
        //private async Task<CreditStatus> ConvertCreditStatusAsync(string statusCode)
        public async Task<CreditStatus> ConvertCreditStatusAsync(string statusCode)
        {
            if (!string.IsNullOrEmpty(statusCode))
            {
                var codeItem = (await GetStudentAcademicCreditStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == statusCode).FirstOrDefault();
                CreditStatus cs;
                if (codeItem == null)
                {
                    cs = CreditStatus.Unknown;
                    return cs;
                }
                switch (codeItem.ValActionCode1AssocMember)
                {

                    case "1":
                        cs = CreditStatus.New;
                        break;
                    case "2":
                        cs = CreditStatus.Add;
                        break;
                    case "3":
                        cs = CreditStatus.Dropped;
                        break;
                    case "4":
                        cs = CreditStatus.Withdrawn;
                        break;
                    case "5":
                        cs = CreditStatus.Deleted;
                        break;
                    case "6":
                        cs = CreditStatus.Cancelled;
                        break;
                    case "7":
                        cs = CreditStatus.TransferOrNonCourse;
                        break;
                    case "8":
                        cs = CreditStatus.Preliminary;
                        break;
                    default:
                        cs = CreditStatus.Unknown;
                        break;
                }
                return cs;
            }
            else
            {
                return CreditStatus.Unknown;
            }
        }
        /// <summary>
        /// Filters and return academic credits based upon the criteria passed.
        /// Criteria is passed as unidata_uniquery syntax.
        /// Criteria will only be applied to STUDENT.ACAD.CRED records
        /// Criteria syntax is like "WITH <STUDENT.ACAD.CRED><FIELD NAME>"
        /// </summary>
        /// <param name="academicCredits">List of academic credits</param>
        /// <param name="criteria">criteria as per unidata_uniquery syntax</param>
        /// <returns>Filtered Academic credits as per criteria
        /// If no criteria is passed, returns all the academic credits
        /// </returns>
        public async Task<IEnumerable<AcademicCredit>> FilterAcademicCreditsAsync(IEnumerable<AcademicCredit> academicCredits, string criteria)
        {
            if (academicCredits == null)
            {
                throw new ArgumentNullException("acadCredits", "academic credits cannot be null");
            }
            if (string.IsNullOrEmpty(criteria))
            {
                return academicCredits;
            }
            string[] fiteredAcadCredIds = null;
            IEnumerable<AcademicCredit> filteredAcadCredits = new List<AcademicCredit>();
            string[] academicCreditsIds = academicCredits.Select(a => a.Id).ToArray<string>();
            if (academicCreditsIds != null && academicCreditsIds.Length > 0)
            {
                try
                {
                    fiteredAcadCredIds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", academicCreditsIds, criteria);

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception occured while filtering academic credits, criteria passed is :" + criteria);
                    return academicCredits;
                }
                if (fiteredAcadCredIds != null && fiteredAcadCredIds.Any())
                {
                    filteredAcadCredits = academicCredits.Where(a => fiteredAcadCredIds.Contains(a.Id));
                }

            }
            return filteredAcadCredits;
        }

        /// <summary>
        /// GetCreditStatuses gets the STUDENT.ACAD.CRED.STATUSES valcode table 
        /// </summary>
        /// <returns>ApplValcodes<string, CreditStatus></returns>
        private async Task<ApplValcodes> GetStudentAcademicCreditStatusesAsync()
        {
            if (creditStatuses != null)
            {
                return creditStatuses;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            creditStatuses = await GetOrAddToCacheAsync<ApplValcodes>("StudentAcademicCreditStatuses",
               async () =>
               {
                   ApplValcodes studentAcadCredStatuses = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES");

                   if (studentAcadCredStatuses == null)
                   {
                       var errorMessage = "Unable to access STUDENT.ACAD.CRED.STATUSES valcode table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   return studentAcadCredStatuses;
               }, Level1CacheTimeoutValue);
            return creditStatuses;
        }

        private async Task<IDictionary<string, Grade>> GetGradesAsync()
        {
            if (grades != null)
            {
                return grades;
            }

            grades = new Dictionary<string, Grade>();
            var gradeEntities = (await gradeRepository.GetAsync()).ToList();
            foreach (var grade in gradeEntities)
            {
                grades.Add(grade.Id, grade);
            }
            return grades;
        }
        public async Task<bool> GetPilotCensusBooleanAsync()
        {
            Ellucian.Colleague.Data.Base.DataContracts.PilotParms pilotDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PilotParms>("ST.PARMS", "PILOT.DEFAULTS");
            bool UseCensusDate = false;
            if (pilotDefaults != null && pilotDefaults.PilCensusFteFlag != null)
            {
                UseCensusDate = pilotDefaults.PilCensusFteFlag.ToUpper() == "Y";
            }
            return UseCensusDate;
        }

        public async Task<IEnumerable<StudentAnonymousGrading>> GetAnonymousGradingIdsAsync(AnonymousGradingType anonymousGradingType,
            string studentId, List<string> termIds, List<string> sectionIds)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                var errorMessage = "a student id is required in order to retrieve grading ids for a student.";
                logger.Info(errorMessage);
                throw new ArgumentNullException("studentId", errorMessage);
            }

            if ((termIds != null && termIds.Count > 0) && (sectionIds != null && sectionIds.Count > 0))
            {
                throw new ArgumentException("either term ids or course section ids may be provided but not both");
            }

            var studentAnonymousGradingIds = new List<StudentAnonymousGrading>();

            // this is a valid condition in which Anonymous Grading has not been configured (school is not doing any anonymous grading)
            if (anonymousGradingType == AnonymousGradingType.None)
            {
                logger.Info("Anonymous Grading Type configuration has not been configured and is null or empty");
                return studentAnonymousGradingIds;
            }

            // get student course sections for the given student id
            var sectionCriteria = "WITH SCS.STUDENT EQ '" + studentId + "'";
            var studentCourseSectionsData = await DataReader.BulkReadRecordAsync<StudentCourseSec>(sectionCriteria);

            if (studentCourseSectionsData == null || !studentCourseSectionsData.Any())
            {
                return studentAnonymousGradingIds;
            }

            List<StudentCourseSec> studentCourseSections = studentCourseSectionsData.ToList();

            //check that student has registered for sections in filter list
            if (sectionIds != null && sectionIds.Count > 0)
            {
                //all the ids in sectionIds that are not in studentCourseSections.ScsCourseSection
                var notRregisterdSectionIds = sectionIds.Where(sectionId => !studentCourseSections.Any(scs => scs.ScsCourseSection == sectionId));
                foreach (var id in notRregisterdSectionIds)
                {
                    var message = "Student " + studentId + " is not registered for course section " + id + ".";
                    logger.Error(message);
                    studentAnonymousGradingIds.Add(new StudentAnonymousGrading(string.Empty, string.Empty, string.Empty, id, message));
                }

                //filter out sections if specified in the criteria
                studentCourseSections = studentCourseSections.Where(scs => sectionIds.Contains(scs.ScsCourseSection)).ToList();
            }

            if (studentCourseSections == null || studentCourseSections.Count == 0)
            {
                logger.Error("No student course sections found for the student: " + studentId);
                return studentAnonymousGradingIds;
            }

            // filter out any sections that are not configured for Grading by Random ID (flag on ASCI) 
            studentCourseSections = await FilterRandomGradingSections(studentId, studentCourseSections);

            //student has no sections that are configured for random grading
            if (studentCourseSections.Count == 0)
            {
                var logMsg = "Student " + studentId + " has no student course sections that are configured for random grading.";
                logger.Error(logMsg);
                return studentAnonymousGradingIds;
            }

            //get student academic credits for sections that are configured for random grading
            var acadCreditIds = studentCourseSections.Select(scs => scs.ScsStudentAcadCred).ToList();
            if (acadCreditIds == null || acadCreditIds.Count == 0)
            {
                logger.Error("No student academic credit record ids found for the student: " + studentId);
                return studentAnonymousGradingIds;
            }

            // read academic credits to find terms and filter out any credits that are dropped or deleted
            var studentAcademicCreditsWithInvalidKeys = await GetWithInvalidKeysAsync(academicCreditIds: acadCreditIds);
            var studentAcademicCredits = studentAcademicCreditsWithInvalidKeys.AcademicCredits.ToList();

            //log any missing academic credits
            if (studentAcademicCreditsWithInvalidKeys.InvalidAcademicCreditIds != null && studentAcademicCreditsWithInvalidKeys.InvalidAcademicCreditIds.Any())
            {
                logger.Error("AcademicCreditRepository: Failed to retrieve all academic credits for the student course sections.  Id count " + acadCreditIds.Count() + " Credits count " + studentAcademicCredits.Count);
                logger.Error("   Missing Ids (includes dropped credits):" + string.Join(",", studentAcademicCreditsWithInvalidKeys.InvalidAcademicCreditIds));
            }

            //Only active academic credits should be used - filter credits to exclude dropped & withdrawn so they do not show on the student grading ids tab
            var activeStudentAcademicCredits = studentAcademicCredits.Where(ac => ac.Status == CreditStatus.Add || ac.Status == CreditStatus.New).ToList();

            //check that student has registered sections in the specified in the term criteria when present
            if (termIds != null && termIds.Count > 0)
            {

                //all the ids in termIds that are not in studentAcademicCredits
                var notRregisterdTermIds = termIds.Where(termId => !studentAcademicCredits.Any(sac => sac.TermCode == termId));
                foreach (var id in notRregisterdTermIds)
                {
                    var message = "Student " + studentId + " is not registered for course sections that are configured for random grading in term: " + id + ".";
                    logger.Error(message);
                    studentAnonymousGradingIds.Add(new StudentAnonymousGrading(string.Empty, string.Empty, string.Empty, id, message));
                }

                // filter out terms if terms where specified in the query criteria
                activeStudentAcademicCredits = activeStudentAcademicCredits.Where(sac => termIds.Contains(sac.TermCode)).ToList();
            }

            if (activeStudentAcademicCredits == null || activeStudentAcademicCredits.Count == 0)
            {
                logger.Error("No student academic credit records found with random grading configured sections for the student: " + studentId + " and terms: " + string.Join(",", termIds));
                return studentAnonymousGradingIds;
            }

            //at this point we can build grading ids when configured for section based generation of of anonymous grading ids
            if (anonymousGradingType == AnonymousGradingType.Section)
            {
                //return await BuildSectionAnonymousGradingIdsAsync(studentId, studentCourseSections, activeStudentAcademicCredits);
                return BuildSectionAnonymousGradingIdsAsync(studentId, studentCourseSections, activeStudentAcademicCredits);
            }

            //retrieving student terms is only required when we are getting grading ids for term based generation of anonymous grading ids
            if (anonymousGradingType == AnonymousGradingType.Term)
            {
                return await BuildTermAnonymousGradingIdsAsync(studentId, acadCreditIds, activeStudentAcademicCredits);
            }

            return studentAnonymousGradingIds;
        }

        /// <summary>
        /// filter out any section that is not congifugred for random grading
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="studentCourseSections"></param>
        /// <returns>A List of Student Course Sections that are configured for random grading</returns>
        private async Task<List<StudentCourseSec>> FilterRandomGradingSections(string studentId, List<StudentCourseSec> studentCourseSections)
        {
            var randomGradingStudenCourseSections = new List<StudentCourseSec>();


            foreach (var studentCourseSection in studentCourseSections)
            {
                var section = (await sectionRepository.GetCachedSectionsAsync(new List<string>() { studentCourseSection.ScsCourseSection })).FirstOrDefault();

                // log any missing sections, student course section record has an invalid section pointer 
                if (section == null)
                {
                    var logMsg = "Section: " + studentCourseSection.ScsCourseSection + " for student: " + studentId + " was not found in course catalog.";
                    logger.Info(logMsg);
                    continue;
                }

                //check configuration of course section - only return sections that are configured for random grading
                if (section.GradeByRandomId)
                {
                    randomGradingStudenCourseSections.Add(studentCourseSection);
                }
            }

            return randomGradingStudenCourseSections;
        }

        private IEnumerable<StudentAnonymousGrading> BuildSectionAnonymousGradingIdsAsync(string studentId,
            List<StudentCourseSec> studentCourseSections, List<AcademicCredit> studentAcademicCredits)
        {
            var studentAnonymousGradingIds = new List<StudentAnonymousGrading>();

            foreach (var acadCredit in studentAcademicCredits)
            {
                string message = null;

                //find the course section from the filtered acad credit list
                var studentSection = studentCourseSections.Where(scs => scs.ScsStudentAcadCred == acadCredit.Id).FirstOrDefault();
                if (studentSection == null || string.IsNullOrEmpty(studentSection.ScsCourseSection)) //ensure student course section has a section assigned
                {
                    var logMsg = "Student " + studentId + " has a student course section " + studentSection.Recordkey + " missing a section id.";
                    logger.Error(logMsg);
                    continue;
                }

                var gradingId = (studentSection.ScsRandomId.HasValue) ? studentSection.ScsRandomId.Value.ToString() : string.Empty;
                var midTermGradingId = (studentSection.ScsMidRandomId.HasValue) ? studentSection.ScsMidRandomId.Value.ToString() : string.Empty;
                var termId = (acadCredit.TermCode == null) ? string.Empty : acadCredit.TermCode;

                //skip sections that are missing anonymous grading ids
                if (string.IsNullOrEmpty(gradingId)) // ensure student course section has a random grading id assigned
                {
                    var logMsg = "Student " + studentId + " does not have a anonymous grading ID for course section: " + studentSection.ScsCourseSection;
                    logger.Error(logMsg);
                    continue;
                }

                studentAnonymousGradingIds.Add(new StudentAnonymousGrading(gradingId, midTermGradingId, termId, studentSection.ScsCourseSection, message));
            }

            return studentAnonymousGradingIds;
        }

        private async Task<IEnumerable<StudentAnonymousGrading>> BuildTermAnonymousGradingIdsAsync(string studentId,
            IEnumerable<string> acadCreditIds, List<AcademicCredit> studentAcademicCredits)
        {
            var studentAnonymousGradingIds = new List<StudentAnonymousGrading>();

            var studentTermKeys = studentAcademicCredits.Select(sac => studentId + "*" + sac.TermCode + "*" + sac.AcademicLevelCode).Distinct();
            if (studentTermKeys == null || !studentTermKeys.Any())
            {
                logger.Info("error occurred while creating student term keys");
                return studentAnonymousGradingIds;
            }

            var studentTermsOutput = await DataReader.BulkReadRecordWithInvalidRecordsAsync<StudentTerms>("STUDENT.TERMS", studentTermKeys.ToArray());

            if (studentTermsOutput.InvalidKeys != null && studentTermsOutput.InvalidKeys.Any())
            {
                logger.Error("AcademicCreditRepository: Failed to retrieve all Student Terms for the student academic credits.  Id count " + acadCreditIds.Count() + " Credits count " + studentTermsOutput.BulkRecordsRead.Count);
                logger.Error("   Missing Ids :" + string.Join(",", studentTermsOutput.InvalidKeys));
            }

            //at this point we can build grading ids when configured for terms based generation of of anonymous grading ids
            foreach (var studentTerm in studentTermsOutput.BulkRecordsRead)
            {
                var gradingId = (studentTerm.SttrRandomId.HasValue) ? studentTerm.SttrRandomId.Value.ToString() : string.Empty;
                var midTermGradingId = (studentTerm.SttrMidRandomId.HasValue) ? studentTerm.SttrMidRandomId.Value.ToString() : string.Empty;
                var recordKeyValues = studentTerm.Recordkey.Split('*');
                var termId = (recordKeyValues == null || recordKeyValues.Length != 3) ? string.Empty : recordKeyValues[1];
                string message = null;

                //skip terms that are missing an anonymous grading
                if (string.IsNullOrEmpty(gradingId)) // ensure student term has a random grading id assigned
                {
                    var logMsg = "Student " + studentId + " does not have a anonymous grading ID for term: " + termId;
                    logger.Error(logMsg);
                    continue;
                }

                studentAnonymousGradingIds.Add(new StudentAnonymousGrading(gradingId, midTermGradingId, termId, null, message));
            }

            return studentAnonymousGradingIds;
        }

    }
}