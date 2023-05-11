/*Copyright 2014-2022 Ellucian Company L.P. and its affiliates.*/
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
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Dmi.Runtime;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class gets and updates StudentAward objects to the colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAwardRepository : BaseColleagueRepository, IStudentAwardRepository
    {
        /// <summary>
        /// Constructor for StudentAwardRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public StudentAwardRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// This gets all of the student's awards from Colleague for all years the student has award data.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award data</param>
        /// <param name="studentAwardYears">The student's award years from which to retrieve award data</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when studentId does not exist in the FinancialAid system - Student has no FIN.AID record</exception>
        /// <returns>A list of StudentAward objects</returns>
        public async Task<IEnumerable<StudentAward>> GetAllStudentAwardsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }
            if (!studentAwardYears.All(y => y.StudentId == studentId))
            {
                throw new InvalidOperationException("StudentId of studentAwardYears must match student Id argument: " + studentId);
            }
            if (allAwards == null)
            {
                throw new ArgumentNullException("allAwards");
            }
            if (allAwardStatuses == null)
            {
                throw new ArgumentNullException("allAwardStatuses");
            }

            var studentAwards = new List<StudentAward>();

            //Get all the student award data from each year.
            foreach (var year in studentAwardYears)
            {
                //get the student awards for the year
                //if any exceptions are thrown, log them, and skip this year.
                try
                {
                    var annualStudentAwards = await GetStudentAwardsForYearAsync(studentId, year, allAwards, allAwardStatuses);

                    studentAwards.AddRange(annualStudentAwards);
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);
                }
            }

            return studentAwards;
        }

        /// <summary>
        /// Gets a list of StudentAward objects for a given award year.
        /// </summary>
        /// <param name="studentId">The StudentId</param>
        /// <param name="studentAwardYear">The StudentAwardYear for which to get awards</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <returns>A list of StudentAward objects for the given awardYear parameter</returns>
        /// <exception cref="Exception">Thrown if an error is returned from the GetLoanChangeRestriction transaction</exception>
        /// <exception cref="Exception">Thrown if the student has no awards for the given awardYear</exception>
        /// <exception cref="Exception">Thrown if the student is missing all award period data for the given awardYear</exception>
        public async Task<IEnumerable<StudentAward>> GetStudentAwardsForYearAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("awardYear");
            }
            if (allAwards == null)
            {
                throw new ArgumentNullException("allAwards");
            }
            if (allAwardStatuses == null)
            {
                throw new ArgumentNullException("allAwardStatuses");
            }
            
            //Initialize the return var
            var studentAwardList = new List<StudentAward>();

            //read SA.ACYR for list of award codes and list of frozen award periods
            string saAcyrFile = "SA." + studentAwardYear.Code;
            
            var studentAwardData = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, studentId);
            
            if (studentAwardData == null || studentAwardData.SaAward == null || studentAwardData.SaAward.Count() == 0)
            {
                var message = string.Format("Student {0} has no award data in {1}", studentId, saAcyrFile);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var frozenAwardPeriods = studentAwardData.StatusForTermEntityAssociation
                .Where(t => t.SaAtpFreezeAssocMember.ToUpper() == "Y")
                .Select(t => t.SaTermsAssocMember);

            //project the award codes into a list of SL.ACYR record ids
            var slAcyrRecordIds = studentAwardData.SaAward.Select(awardCode => string.Format("{0}*{1}", studentId, awardCode));

            //read SL.ACYR to get disbursement records for loans
            var slAcyrFile = "SL." + studentAwardYear.Code;
            
            var loanDisbursementRecordData = await DataReader.BulkReadRecordAsync<SlAcyr>(slAcyrFile, slAcyrRecordIds.ToArray());
            
            if (loanDisbursementRecordData == null || loanDisbursementRecordData.Count() == 0)
            {
                //report an info message, but don't throw an exception. loans without disbursements
                //won't be modifiable, or the student has no loans
                loanDisbursementRecordData = new Collection<SlAcyr>();
                var message = string.Format("No {0} record ids for student {1} award year {2}", slAcyrFile, studentId, studentAwardYear.Code);
                logger.Debug(message);
            }

            //Get the year's awardPeriod level data from Colleague
            string acyrFile = "TA." + studentAwardYear.Code;
            string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "'";
            
            var studentAwardPeriodData = await DataReader.BulkReadRecordAsync<TaAcyr>(acyrFile, criteria);            
            
            //Verify the student has awardPeriod level data - they should if we're at this point, so throw an Exception if they don't
            if (studentAwardPeriodData == null || studentAwardPeriodData.Count() < 1)
            {
                var message = string.Format("All Award Period data missing from Colleague for Year {0} and Student {1}", studentAwardYear.Code, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            //for each award the student has for this year, retreive the particular awardPeriodData objects and build a StudentAward
            foreach (var awardCode in studentAwardData.SaAward)
            {

                //get the award entity for the award Code.
                var awardEntity = allAwards.FirstOrDefault(a => a.Code == awardCode);
                if (awardEntity == null)
                {
                    logger.Error(string.Format("Award {0} is missing from Colleague", awardCode));
                }
                else
                {
                    //build the studentAward
                    var studentAward = new StudentAward(studentAwardYear, studentId, awardEntity, true);

                    try
                    {
                        //build the studentAwardPeriods
                        //Just building StudentAwardPeriods for a particular StudentAward
                        BuildStudentAwardPeriods(studentAward, studentAwardPeriodData, loanDisbursementRecordData, allAwardStatuses, frozenAwardPeriods);

                        //Add the StudentAward to the list of StudentAwards
                        studentAwardList.Add(studentAward);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, e.Message);
                    }
                }
            }
            return studentAwardList;
        }

        /// <summary>
        /// Get single award
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="awardCode">award code for award to retrieve</param>
        /// <param name="allAwards">reference awards</param>
        /// <param name="allAwardStatuses">reference award statuses</param>
        /// <returns></returns>
        public async Task<StudentAward> GetStudentAwardAsync(string studentId, StudentAwardYear studentAwardYear, string awardCode, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            if (string.IsNullOrEmpty(awardCode))
            {
                throw new ArgumentNullException("awardCode");
            }
            if (allAwards == null)
            {
                throw new ArgumentNullException("allAwards");
            }
            if (allAwardStatuses == null)
            {
                throw new ArgumentNullException("allAwardStatuses");
            }
            
            #region Read SA.ACYR for the year
            //read SA.ACYR for list of award codes and list of frozen award periods
            string saAcyrFile = "SA." + studentAwardYear.Code;
            
            var studentAwardData = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, studentId);
            
            if (studentAwardData == null || studentAwardData.SaAward == null || !studentAwardData.SaAward.Any() || !studentAwardData.SaAward.Contains(awardCode))
            {
                var message = string.Format("Student {0} has no award data for {1} in {2}", studentId, awardCode, saAcyrFile);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var frozenAwardPeriods = studentAwardData.StatusForTermEntityAssociation
                .Where(t => t.SaAtpFreezeAssocMember.ToUpper() == "Y")
                .Select(t => t.SaTermsAssocMember);
            #endregion

            var currentAward = allAwards.FirstOrDefault(a => a.Code == awardCode);
            StudentAward studentAward = null;

            if(currentAward != null){
                //Is it a loan - if no category is specified, we assume it is a loan
                bool isLoan = !currentAward.AwardCategoryType.HasValue || currentAward.AwardCategoryType.Value == AwardCategoryType.Loan;
                SlAcyr loanDisbursementRecord;
                var loanDisbursementRecordData = new List<SlAcyr>();

                #region Read Sl.ACYR if loan
                if (isLoan)
                {
                    //project the award code into a SL.ACYR record id
                    var slAcyrRecordId = string.Format("{0}*{1}", studentId, awardCode);

                    //read SL.ACYR to get disbursement record for the loan
                    var slAcyrFile = "SL." + studentAwardYear.Code;
                    
                    loanDisbursementRecord = await DataReader.ReadRecordAsync<SlAcyr>(slAcyrFile, slAcyrRecordId);                    
                    
                    if (loanDisbursementRecord == null)
                    {
                        //report an info message, but don't throw an exception. loans without disbursements
                        //won't be modifiable, or it is not a loan                    
                        var message = string.Format("No loan disbursement data for {0} for student {1} {2} award year", awardCode, studentId, studentAwardYear.Code);
                        logger.Debug(message);
                    }
                    else
                    {
                        loanDisbursementRecordData.Add(loanDisbursementRecord);
                    }
                }
                #endregion

                #region Read TA.ACYR for the specified award
                //Get the year's awardPeriod level data for the award from Colleague
                string acyrFile = "TA." + studentAwardYear.Code;
                string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "' AND TA.AW.ID EQ '" + awardCode +"'";
                
                var studentAwardPeriodData = await DataReader.BulkReadRecordAsync<TaAcyr>(acyrFile, criteria);                
                
                //Verify the student has awardPeriod level data - they should if we're at this point, so throw an Exception if they don't
                if (studentAwardPeriodData == null || !studentAwardPeriodData.Any())
                {
                    var message = string.Format("Award Period data for {0} award is missing from Colleague for Year {1} and Student {2}", awardCode, studentAwardYear.Code, studentId);
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }
                #endregion
                
                //build the studentAward
                studentAward = new StudentAward(studentAwardYear, studentId, currentAward, true);

                try
                {
                    //build the studentAwardPeriods
                    //Just building StudentAwardPeriods for a particular StudentAward
                    BuildStudentAwardPeriods(studentAward, studentAwardPeriodData, loanDisbursementRecordData, allAwardStatuses, frozenAwardPeriods);                    
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);
                }
            }
            return studentAward;
        }

        public async Task<IEnumerable<string>> GetCFPVersionAsync(string studentId, string awardYear)
        {
            if (studentId == null)
            {
                throw new ArgumentNullException("studentId");
            }
            if (awardYear == null)
            {
                throw new ArgumentNullException("awardYear");
            }

            var cfpVersion = new List<string>();
            var request = new GetFaCfpVersionRequest();
            request.FaYear = awardYear;
            request.StudentId = studentId;
            var response = await transactionInvoker.ExecuteAsync<GetFaCfpVersionRequest, GetFaCfpVersionResponse>(request);
            if (response.CfpVersion != null)
            {
                cfpVersion.Add(response.CfpVersion);
                return cfpVersion;
            }
            else
            {
                return response.ErrorMsgs;
            }

        }


        public async Task<IEnumerable<StudentAward>> UpdateStudentAwardsAsync(StudentAwardYear studentAwardYear, IEnumerable<StudentAward> studentAwards, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            if (studentAwards == null)
            {
                throw new ArgumentNullException("studentAwards");
            }

            if (studentAwards.Any(sa => !sa.StudentAwardYear.Equals(studentAwardYear)))
            {
                throw new InvalidOperationException(string.Format("All studentAwards must apply to the same studentAwardYear", studentAwardYear.ToString()));

            }

            var request = new UpdateStudentAwardRequest()
            {
                Year = studentAwardYear.Code,
                StudentId = studentAwardYear.StudentId,
                UpdateAward = studentAwards.Select(sa =>
                    new UpdateAward()
                    {
                        AwardId = sa.Award.Code,
                        AwardPeriodIds = string.Join(DmiString.sSM, sa.StudentAwardPeriods.Select(p => p.AwardPeriodId)),
                        AwardPeriodAmounts = string.Join(DmiString.sSM, sa.StudentAwardPeriods.Select(p => p.AwardAmount)),
                        AwardPeriodStatuses = string.Join(DmiString.sSM, sa.StudentAwardPeriods.Select(p => p.AwardStatus.Code))
                    }).ToList()
            };
            try
            {
                var response = await transactionInvoker.ExecuteAsync<UpdateStudentAwardRequest, UpdateStudentAwardResponse>(request);

                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    //Whatever the error was, log it
                    logger.Error(response.ErrorMessage);
                    throw new ApplicationException(response.ErrorMessage);
                }

                return (await GetStudentAwardsForYearAsync(studentAwardYear.StudentId, studentAwardYear, allAwards, allAwardStatuses)).Where(sa => studentAwards.Contains(sa));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
        }

        /// <summary>
        /// This method builds a list of StudentAwardPeriod Entities.
        /// </summary>
        /// <param name="studentAward">The StudentAward object that is the parent of the StudentAwardPeriod entities built by this method</param>
        /// <param name="studentAwardPeriodData">A list of TaAcyr DataContracts containing the data to build StudentAwardPeriods</param>
        /// <param name="allAwardStatuses">A list of all AwardStatus objects from Colleague</param>
        /// <param name="frozenAwardPeriodIds">A list of award period codes from Colleague that are frozen, i.e. cannot be modified).</param>
        /// <returns>A list of StudentAwardPeriod Entities.</returns>
        private IEnumerable<StudentAwardPeriod> BuildStudentAwardPeriods(StudentAward studentAward, IEnumerable<TaAcyr> studentAwardPeriodData, IEnumerable<SlAcyr> disbusrementData, IEnumerable<AwardStatus> allAwardStatuses, IEnumerable<string> frozenAwardPeriodIds)
        {
            if (frozenAwardPeriodIds == null) { frozenAwardPeriodIds = new List<string>(); }
            if (disbusrementData == null) { disbusrementData = new List<SlAcyr>(); }
            if (studentAwardPeriodData == null) { studentAwardPeriodData = new List<TaAcyr>(); }

            var studentAwardPeriodList = new List<StudentAwardPeriod>();

            //extract disbursement records from disbursementRecords list
            var disbursementRecordKey = studentAward.StudentId + "*" + studentAward.Award.Code;
            var awardPeriodsWithLoanDisbursements = disbusrementData
                .Where(disb => disb.Recordkey.ToUpper() == disbursementRecordKey.ToUpper())
                .SelectMany(disb => disb.SlAntDisbTerm);

            //extract StudentAwardPeriod records specific to this StudentAward.Award.Code
            string studentAwardPeriodPartialRecordKey = disbursementRecordKey + "*";
            var awardSpecificPeriodData = studentAwardPeriodData
                .Where(period => period.Recordkey.ToUpper().StartsWith(studentAwardPeriodPartialRecordKey.ToUpper()));

            //throw an exception if there are no award periods. calling process should log this error.
            if (awardSpecificPeriodData == null || awardSpecificPeriodData.Count() == 0)
            {
                var message = string.Format("Award Period data missing from Colleague for Year {0}, Student {1} and Award {2}", studentAward.StudentAwardYear.Code, studentAward.StudentId, studentAward.Award.Code);
                throw new ApplicationException(message);
            }
            else
            {
                //otherwise, loop through and build
                foreach (var studentAwardPeriodRecord in awardSpecificPeriodData)
                {
                    try
                    {
                        var awardPeriodId = studentAwardPeriodRecord.Recordkey.Split('*')[2];
                        var awardStatusEntity = allAwardStatuses.FirstOrDefault(s => s.Code == studentAwardPeriodRecord.TaTermAction);
                        var isFrozen = frozenAwardPeriodIds.Contains(awardPeriodId);

                        var studentAwardPeriodEntity = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatusEntity, isFrozen, studentAwardPeriodRecord.TaTermXmitAmt.HasValue);
                        studentAwardPeriodEntity.AwardAmount = studentAwardPeriodRecord.TaTermAmount;
                        studentAwardPeriodEntity.HasLoanDisbursement = awardPeriodsWithLoanDisbursements.Contains(awardPeriodId);

                        studentAwardPeriodList.Add(studentAwardPeriodEntity);
                    }
                    catch (Exception e)
                    {
                        logger.Debug(e, string.Format("Unable to create StudentAwardPeriod from data. AwardYear: {0} RecordKey {1}", studentAward.StudentAwardYear, studentAwardPeriodRecord.Recordkey));
                    }
                }
            }

            return studentAwardPeriodList;
        }



        /// <summary>
        /// Gets a list of StudentAward objects for a given award year. This method is different than
        /// <see cref="GetStudentAwardsForYear"/> in that this method does not execute Colleague Transactions
        /// to determine loan change restrictions and eligibility, thus reducing processing time.
        /// Use this method when you need read-only StudentAward objects
        /// </summary>
        /// <param name="studentId">The StudentId</param>
        /// <param name="awardYear">The AwardYear</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <returns>A list of StudentAward objects for the given awardYear parameter</returns>
        /// <exception cref="Exception">Thrown if an error is returned from the GetLoanChangeRestriction transaction</exception>
        /// <exception cref="Exception">Thrown if the student has no awards for the given awardYear</exception>
        /// <exception cref="Exception">Thrown if the student is missing all award period data for the given awardYear</exception>
        public IEnumerable<StudentAward> GetStudentAwardSummaryForYear(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("awardYear");
            }
            if (allAwards == null)
            {
                throw new ArgumentNullException("allAwards");
            }
            if (allAwardStatuses == null)
            {
                throw new ArgumentNullException("allAwardStatuses");
            }

            //Initialize the return var
            var studentAwardList = new List<StudentAward>();

            //Get all award data from the SA.year tables
            string saAcyrFile = "SA." + studentAwardYear.Code;
            var studentAwardData = DataReader.ReadRecord<SaAcyr>(saAcyrFile, studentId);

            if (studentAwardData != null)
            {
                List<string> studentAwards = new List<string>();
                studentAwards.AddRange(studentAwardData.SaAward);

                //if the student has no awards for this year, throw an Exception
                if (studentAwardData.SaAward == null || studentAwardData.SaAward.Count() < 1)
                {
                    logger.Error(string.Format("Student {0} has no awards for year {1}", studentId, studentAwardYear.Code));
                }

                //Get the year's awardPeriod level data from Colleague
                string acyrFile = "TA." + studentAwardYear.Code;
                string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "'";
                var studentAwardPeriodData = DataReader.BulkReadRecord<TaAcyr>(acyrFile, criteria);

                //Verify the student has awardPeriod level data - they should if we're at this point, so throw an Exception if they don't
                if (studentAwardPeriodData == null || studentAwardPeriodData.Count() < 1)
                {
                    logger.Error(string.Format("All Award Period data missing from Colleague for Year {0} and Student {1}", studentAwardYear.Code, studentId));
                }

                //for each award the student has for this year, retreive the particular awardPeriodData objects and build a StudentAward
                foreach (var award in studentAwards)
                {

                    //Create the StudentAward object
                    bool isAwardEligible = true;
                    var awardEntity = allAwards.FirstOrDefault(a => a.Code == award);
                    if (awardEntity == null)
                    {
                        logger.Error(string.Format("Award {0} is missing from Colleague", award));
                    }
                    else
                    {

                        var studentAward = new StudentAward(studentAwardYear, studentId, awardEntity, isAwardEligible);

                        //Begin creating award periods
                        //Extract the award period data from the data collection for the current award
                        string taPartialKey = studentId + "*" + award + "*";
                        var periodData = studentAwardPeriodData.Where(s => s.Recordkey.StartsWith(taPartialKey));

                        //Verify the student has awardPeriod level data for this particular award - they should if we're at this point, so log an error 
                        //and skip the award
                        if (periodData != null && periodData.Count() > 0)
                        {
                            //Just building StudentAwardPeriods for a particular StudentAward
                            var studentAwardPeriodList = BuildStudentAwardPeriods(studentAward, periodData, new List<SlAcyr>(), allAwardStatuses, null);


                            //Add the StudentAward to the list of StudentAwards
                            studentAwardList.Add(studentAward);
                        }
                        else
                        {
                            logger.Error(string.Format("Award Period data missing from Colleague for Year {0}, Student {1} and Award {2}", studentAwardYear.Code, studentId, award));
                        }
                    }

                }
            }
            return studentAwardList;
        }
        public int? GetVetBenAmount(string studentId, string awardYear, IEnumerable <string> cfpVersion)
        {
            logger.Debug(string.Format("Entering the GetVenBenAmount method to pull student VETS award information from their STUDENT.TERMS records for {0}*{1}",studentId, awardYear));

            if (cfpVersion == null)
            {
                throw new ArgumentNullException("cfpVersion");
            }

            int? vetBenAmount = 0;
            var saYear = "SA." + awardYear;
            try
            {
                var saAcyrData = DataReader.ReadRecord<SaAcyr>(saYear, studentId);
                
                //Get list of all academic levels for the student in this year of either UG or GR level
                var stuData = DataReader.ReadRecord<Students>(studentId);
                var acadLevels = new List<string>();
                if (stuData != null && stuData.StuAcadLevels.Count > 0)
                {
                    logger.Debug(string.Format("Retrieved acad levels {0} from STUDENTS record and reducing them to those of type {1}", string.Join(", ", stuData.StuAcadLevels), cfpVersion.FirstOrDefault()));
                    for (var c = 0; c < stuData.StuAcadLevels.Count; c++)
                    {
                        var acadLevelsData = DataReader.ReadRecord<AcadLevels>(stuData.StuAcadLevels[c]);
                        if (acadLevelsData != null)
                        {
                            //If we are looking for UG acad levels and the GR flag is not set, add to list of acad levels to check
                            if (cfpVersion.FirstOrDefault() == "UG" && acadLevelsData.AclvGradLevelFlag.ToUpper() != "Y")
                            {
                                acadLevels.Add(acadLevelsData.Recordkey);
                            }
                            //If we are looking for GR acad levels and the GR flag is set, add to list of acad levels to check
                            if (cfpVersion.FirstOrDefault() == "GR" && acadLevelsData.AclvGradLevelFlag.ToUpper() == "Y")
                            {
                                acadLevels.Add(acadLevelsData.Recordkey);
                            }
                        }
                    }
                }


                var stuTerms = new List<string>();

                if (saAcyrData != null)
                {
                    //Using for instead of foreach for performance
                    //Create list of all terms for the student in this FA year

                    for (var x = 0; x < saAcyrData.SaTerms.Count; x++)
                    {
                        var awardPeriod = DataReader.ReadRecord<AwardPeriods>("AWARD.PERIODS", saAcyrData.SaTerms[x]);
                        if (awardPeriod != null)
                        {
                            for (var y = 0; y < awardPeriod.AwdpAcadTerms.Count; y++)
                            {
                                stuTerms.Add(awardPeriod.AwdpAcadTerms[y]);
                            }
                        }
                    }

                    logger.Debug(string.Format("Checking all STUDENT.TERMS records for terms {0} and acad levels {1} for student {2}", string.Join(", ", stuTerms), string.Join(", ", acadLevels), studentId));

                    for (var z = 0; z < stuTerms.Count; z++)
                    {
                        for (var y = 0; y < acadLevels.Count; y++)
                        {
                            //Updating logic to account for all acad levels
                            //var stuTermsId = studentId + "*" + stuTerms[z] + "*" + cfpVersion.First().ToString();
                            var stuTermsId = studentId + "*" + stuTerms[z] + "*" + acadLevels[y];
                            var studentTermsData = DataReader.ReadRecord<StudentTerms>("STUDENT.TERMS", stuTermsId);
                            if (studentTermsData != null && studentTermsData.SttrVetNetAmtCertified != null)
                            {
                                vetBenAmount += (int?)studentTermsData.SttrVetNetAmtCertified;
                            }
                        }
                    }
                }
                logger.Debug(string.Format("Returning VET amount of {0}", vetBenAmount));
                return (vetBenAmount != 0 ? vetBenAmount : null);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                return null;
            }
        }
    }
}
