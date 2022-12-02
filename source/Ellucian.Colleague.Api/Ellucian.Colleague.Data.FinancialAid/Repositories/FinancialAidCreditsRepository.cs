/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class gets FinancialAidCredits objects from the Colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidCreditsRepository : BaseColleagueRepository, IFinancialAidCreditsRepository
    {
        /// <summary>
        /// Constructor for FinancialAidCreditsRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public FinancialAidCreditsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// This gets all of the student's credits from Colleague for all (or active only) years and details how they apply to the various Financial Aid credit types
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve credit data</param>
        /// <param name="studentAwardYears">The student's award years from which to retrieve credit data</param>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when studentId does not exist in the FinancialAid system - Student has no FIN.AID record</exception>
        /// <returns>A list of StudentAward objects</returns>
        public async Task<List<AwardYearCredits>> GetStudentFinancialAidCreditsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                logger.Debug(string.Format("Cannot get AwardYearCredits for student {0} with no studentAwardYears", studentId));
                return new List<AwardYearCredits>();
            }
            if (!studentAwardYears.All(y => y.StudentId == studentId))
            {
                throw new InvalidOperationException("StudentId of studentAwardYears must match student Id argument: " + studentId);
            }


            List<AwardYearCredits> allStuFaCreds = new List<AwardYearCredits>();

            foreach (var saw in studentAwardYears)
            {
                try
                {
                    allStuFaCreds.Add(await RetrieveCreditsAsync(saw));
                }
                catch (Exception e)
                {
                    var message = string.Format("Failed RetrieveCreditsAsync for StudentAwardYear year {0}", saw.Code);
                    logger.Debug(e, message);
                }
            }
            return allStuFaCreds;
        }

        /// <summary>
        /// This gets the course credits for each specific award year, querying each award period asynchronously
        /// </summary>
        /// <param name="studentAwardYear">The year currently being processed</param>
        /// <returns>Returns an AwardYearCredits object containing all course credits for the provided FA year</returns>
        public async Task<AwardYearCredits> RetrieveCreditsAsync(StudentAwardYear studentAwardYear)
        {
            logger.Debug(string.Format("Entering RetrieveCreditsAsync for {0}*{1}", studentAwardYear.StudentId, studentAwardYear.Code));
            string awardYear = studentAwardYear.Code;
            string studentId = studentAwardYear.StudentId;

            //Create a new AwardYearCredits object for the year which will contain any AwardPeriodCredits objects within the year
            var awardYearCoursework = new AwardYearCredits(studentId, awardYear);
            var containsCoursework = false;
            var awpdTasks = new List<Task>();

            string saAcyrFile = "SA." + awardYear;
            try
            {
                var studentAwardData = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, studentId);

                if (studentAwardData != null && studentAwardData.SaTerms.Count >= 1)
                {
                    for (var awdPd = 0; awdPd < studentAwardData.SaTerms.Count; awdPd++)
                    {
                        //Need to store the index pointer as a temporary value as it can change during asynchronous operation and lead to index out of range runtime errors
                        var tempValue = awdPd;
                        //Create a new AwardPeriodCredits object for each award period with an empty list of AwardPeriodCoursework objects
                        var awardPeriodCoursework = new AwardPeriodCredits(studentId, awardYear, studentAwardData.SaTerms[tempValue]);
                        //Get the student's program for this award period first (if one exists)
                        var awpdProgram = await GetAwpdProgram(studentAwardData.SaTerms[tempValue], studentAwardYear);
                        if (!string.IsNullOrEmpty(awpdProgram))
                        {
                            awardPeriodCoursework.ProgramName = awpdProgram;
                        }
                        var awpdDescription = await GetAwpdDescription(studentAwardData.SaTerms[tempValue]);
                        if (!string.IsNullOrEmpty(awpdDescription))
                        {
                            awardPeriodCoursework.AwardPeriodDescription = awpdDescription;
                        }
                        //Add each AwardPeriodCredits object to awardYearCoursework in order in case Async returns out of index order
                        awardYearCoursework.AwardPeriodCoursework.Add(awardPeriodCoursework);

                        //Create a set of tasks to be executed asynchronously which will get each award period's course credits
                        awpdTasks.Add(Task.Run(async () =>
                        {
                            var awpdCoursework = await GetAwpdValues(studentAwardData.SaTerms[tempValue], studentAwardYear);
                            //If coursework is found for this award period, add it to the associated AwardPeriodCredits object
                            if (awpdCoursework != null)
                            {
                                awardYearCoursework.AwardPeriodCoursework[tempValue].Coursework = awpdCoursework;
                                //awardPeriodCoursework.Coursework = awpdCoursework;
                                if (awpdCoursework.Count >= 1)
                                {
                                    containsCoursework = true;
                                    //All elements contain the same flag so just grab the first one
                                    awardYearCoursework.AwardPeriodCoursework[tempValue].DegreeAuditActive = awpdCoursework[0].DegreeAuditActiveFlag;
                                    //awardPeriodCoursework.DegreeAuditActive = awpdCoursework[0].DegreeAuditActiveFlag;
                                }
                            }
                            //Add all AwardPeriodCredits objects to the current year's AwardYearCredits object
                            //awardYearCoursework.AwardPeriodCoursework.Add(awardPeriodCoursework);
                        }));
                    }
                }

                await Task.WhenAll(awpdTasks);
            }
            catch(Exception e)
            {
                logger.Debug(e, e.Message);
            }
            awardYearCoursework.ContainsCourseCredits = containsCoursework;
            return awardYearCoursework;
        }

        /// <summary>
        /// Returns the associated course credits for an award period and its associated FA year
        /// </summary>
        /// <param name="awardPeriodId"></param>
        /// <param name="studentAwardYear"></param>
        /// <returns>Returns a list of CourseCreditAssocation objects for the provided award period</returns>
        public async Task<List<CourseCreditAssociation>> GetAwpdValues(string awardPeriodId, StudentAwardYear studentAwardYear)
        {
            logger.Debug(string.Format("Entering GetAwpdValues for {0}*{1}*{2}", studentAwardYear.StudentId, awardPeriodId, studentAwardYear.Code));
            try
            {
                //Create an empty list of CourseCreditAssocation objects to add to (if necessary) 
                var awpdCourses = new List<CourseCreditAssociation>();
                GetFaCreditsRequest crsRequest = new GetFaCreditsRequest();
                crsRequest.StudentId = studentAwardYear.StudentId;
                crsRequest.Year = studentAwardYear.Code;
                crsRequest.AwardPeriodId = awardPeriodId;

                //Allow asynchronous execution so that multiple award periods can be queried for the student at once
                var courses = await transactionInvoker.ExecuteAsync<GetFaCreditsRequest, GetFaCreditsResponse>(crsRequest);

                //If the GET.FA.CREDITS transaction returns credits for this award period, add them one by one to a list of CourseCreditAssociation objects
                if (courses != null && courses.CourseNames.Any())
                {
                    //Since additional credits are at the top, this just sorts them to the bottom
                    for (int i = 0; i < courses.CourseNames.Count; i++)
                    {
                        var tempIndex = i;
                        awpdCourses.Add(new CourseCreditAssociation(
                            studentAwardYear.Code,
                            studentAwardYear.StudentId,
                            awardPeriodId,
                            courses.CourseNames[tempIndex],
                            courses.CourseTitles[tempIndex],
                            courses.CourseSections[tempIndex],
                            courses.CourseTerms[tempIndex],
                            courses.CourseCreds[tempIndex],
                            courses.CourseInstCreds[tempIndex],
                            courses.CourseTivCreds[tempIndex],
                            courses.CoursePellCreds[tempIndex],
                            courses.CourseDlCreds[tempIndex],
                            courses.CourseProgramFlags[tempIndex],
                            courses.DegreeAuditActiveFlag));
                    }
                }
                return awpdCourses;
            }
            catch (Exception e) {
                logger.Debug(e,string.Format("GetAwpdValues failed for {0}*{1}*{2}", studentAwardYear.StudentId, awardPeriodId, studentAwardYear.Code));
                return null;
            }           
        }

        /// <summary>
        /// Retrieve the student's program for each award period
        /// </summary>
        /// <param name="awardPeriodId"></param>
        /// <param name="studentAwardYear"></param>
        /// <returns></returns>
        public async Task<string> GetAwpdProgram (string awardPeriodId, StudentAwardYear studentAwardYear)
        {
            logger.Debug(string.Format("Entering GetAwpdProgram for  {0}*{1}*{2}", studentAwardYear.StudentId, awardPeriodId, studentAwardYear.Code));
            //Program retrieval for each award period
            GetFaProgramRequest progRequest = new GetFaProgramRequest();
            progRequest.StudentId = studentAwardYear.StudentId;
            progRequest.AwardPeriodId = awardPeriodId;
            progRequest.AwardYear = studentAwardYear.Code;

            try
            {
                GetFaProgramResponse progResponse = await transactionInvoker.ExecuteAsync<GetFaProgramRequest, GetFaProgramResponse>(progRequest);
                return progResponse.ProgramName;
            }
            catch
            {
                logger.Debug("GET.FA.PROGRAM failed");
                return null;
            }
        }


        /// <summary>
        /// Retrieve the description for each award period
        /// </summary>
        /// <param name="awardPeriodId"></param>
        /// <param name="studentAwardYear"></param>
        /// <returns></returns>
        public async Task<string> GetAwpdDescription(string awardPeriodId)
        {
            try
            {
                var awardPeriodInfo = await DataReader.ReadRecordAsync<AwardPeriods>("AWARD.PERIODS", awardPeriodId);
                return awardPeriodInfo.AwdpDesc;
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }
    }
}
