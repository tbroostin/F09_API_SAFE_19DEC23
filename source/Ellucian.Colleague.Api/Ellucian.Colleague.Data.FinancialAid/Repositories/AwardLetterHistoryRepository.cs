/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// AwardLetterHistoryRepository class exposes database access to Colleague Award Letters. It
    /// gathers data from numerous tables based on student data and creates AwardLetter
    /// objects.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AwardLetterHistoryRepository : BaseColleagueRepository, IAwardLetterHistoryRepository
    {
        public AwardLetterHistoryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region Obsolete methods
        /// <summary>
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <param name="allAwards">The full list of awards</param>
        /// <param name="createAwardLetterHistoryRecord">Boolean to be used to decide whether to create a new award letter history record</param>
        /// <returns>An award letter object specific to the given award year</returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetter2Async instead")]
        public async Task<AwardLetter2> GetAwardLetterAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            // Check to see if we need a new AwardLetterHistory record generated into Colleague.
            if (createAwardLetterHistoryRecord )
            {
                await CreateAwardLetterHistoryRecordAsync(studentId, studentAwardYear.Code);
            }

            return await BuildAwardLetter(studentId, studentAwardYear, allAwards);

        }

        /// <summary>
        /// Get the most recent award letter for all years
        /// </summary>
        /// <param name="studentId"The Colleague PERSON id of the student></param>
        /// <param name="studentAwardYears">The award year for which to get the award letter</param>
        /// <param name="allAwards">A list of all valid award codes used to retrieve the award description</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetters2Async instead")]
        public async Task<IEnumerable<AwardLetter2>> GetAwardLettersAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }

            //if the student has no year-specific financial aid data, return an empty list
            if (studentAwardYears.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId));
                return new List<AwardLetter2>();
            }

            //instantiate the return list
            var awardLetterEntities = new List<AwardLetter2>();

            foreach (var year in studentAwardYears)
            {
                try
                {
                    var awardLetterEntity = await BuildAwardLetter(studentId, year, allAwards);
                    awardLetterEntities.Add(awardLetterEntity);
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);
                }
            }
            return awardLetterEntities;
        }

        /// <summary>
        /// Gets the award letter entity for the specified record id
        /// </summary>
        /// <param name="recordId">award letter history record id</param>
        /// <param name="studentAwardYears">list of student award years</param>
        /// <param name="allAwards">list of all reference award data</param>
        /// <returns>AwardLetter2 entity</returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetterById2Async instead")]
        public async Task<AwardLetter2> GetAwardLetterByIdAsync(string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                throw new ArgumentNullException("studentAwardYears");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            AwardLetter2 awardLetterEntity;
            try
            {
                awardLetterEntity = await BuildAwardLetter(studentAwardYears, allAwards, recordId);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                awardLetterEntity = new AwardLetter2();
            }
            return awardLetterEntity;
        }

        [Obsolete("Obsolete as of Api version 1.22, use UpdateAwardLetter2Async instead")]
        public async Task<AwardLetter2> UpdateAwardLetterAsync(string studentId, AwardLetter2 awardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }
            if (awardLetter.AwardLetterYear == null)
            {
                throw new ArgumentNullException("AwardLetterYear");
            }

            if (awardLetter.StudentId != studentAwardYear.StudentId)
            {
                throw new ArgumentException("StudentIds of awardLetter and studentAwardYear do not match");
            }

            //Verify that the award letter resource exists for this award year before updating it. We don't want to end up
            //creating db records if the resource doesn't exist. If it doesn't exist, this method call throws an exception.            
            var originalAwardLetter = await BuildAwardLetter(awardLetter.StudentId, studentAwardYear, allAwards);

            UpdateAwardLetterSignedDateRequest request = new UpdateAwardLetterSignedDateRequest();
            request.AwardLetterHistId = awardLetter.Id;

            var transactionResponse = await transactionInvoker.ExecuteAsync<UpdateAwardLetterSignedDateRequest, UpdateAwardLetterSignedDateResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorMessage))
            {
                if (transactionResponse.ErrorMessage == "This award letter has already been signed. No update required.")
                {
                    var message = string.Format("The Award Letter has already been signed.");
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
                else
                {
                    var message = string.Format("Award Letter update canceled because record id {0} is locked.", awardLetter.StudentId);
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
            }

            //at this point, we can assume the transaction db update was successful.

            var studentAwardYears = new List<StudentAwardYear>();
            studentAwardYears.Add(studentAwardYear);
            var updatedAwardLetter = await GetAwardLetterByIdAsync(request.AwardLetterHistId, studentAwardYears, allAwards);

            return updatedAwardLetter;

        }

        #endregion

        /// <summary>
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <param name="allAwards">The full list of awards</param>
        /// <param name="createAwardLetterHistoryRecord">Boolean to be used to decide whether to create a new award letter history record</param>
        /// <returns>An AwardLetter3 object specific to the given award year</returns>
        public async Task<AwardLetter3> GetAwardLetter2Async(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            // Check to see if we need a new AwardLetterHistory record generated into Colleague.
            if (createAwardLetterHistoryRecord)
            {
                await CreateAwardLetterHistoryRecordAsync(studentId, studentAwardYear.Code);
            }

            return await BuildAwardLetter2(studentId, studentAwardYear, allAwards);

        }        

        /// <summary>
        /// Get the most recent award letter for all years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student></param>
        /// <param name="studentAwardYears">The award year for which to get the award letter</param>
        /// <param name="allAwards">A list of all valid award codes used to retrieve the award description</param>
        /// <returns></returns>
        public async Task<IEnumerable<AwardLetter3>> GetAwardLetters2Async(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }

            //if the student has no year-specific financial aid data, return an empty list
            if (!studentAwardYears.Any())
            {
                logger.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId));
                return new List<AwardLetter3>();
            }

            //instantiate the return list
            var awardLetterEntities = new List<AwardLetter3>();

            foreach (var year in studentAwardYears)
            {
                try
                {
                    var awardLetterEntity = await BuildAwardLetter2(studentId, year, allAwards);
                    awardLetterEntities.Add(awardLetterEntity);
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);
                }
            }
            return awardLetterEntities;
        }        

        /// <summary>
        /// Gets the award letter entity for the specified record id
        /// </summary>
        /// <param name="recordId">award letter history record id</param>
        /// <param name="studentAwardYears">a list of student award years</param>
        /// <param name="allAwards">list of all reference award data</param>
        /// <returns>AwardLetter3 entity</returns>
        public async Task<AwardLetter3> GetAwardLetterById2Async(string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                throw new ArgumentNullException("studentAwardYears");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            AwardLetter3 awardLetterEntity;
            try
            {
                awardLetterEntity = await BuildAwardLetter2(studentAwardYears, allAwards, recordId);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                awardLetterEntity = new AwardLetter3();
            }
            return awardLetterEntity;
        }
        

        /// <summary>
        /// Updates an award letter with a signed date
        /// </summary>
        /// <param name="studentId">studentId the award letter belongs to</param>
        /// <param name="awardLetter">award letter to update</param>
        /// <param name="studentAwardYear">student award year the award letter is for</param>
        /// <param name="allAwards">reference awards</param>
        /// <returns>Updated AwardLetter3 entity</returns>
        public async Task<AwardLetter3> UpdateAwardLetter2Async(string studentId, AwardLetter3 awardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }
            if (awardLetter.AwardLetterYear == null)
            {
                throw new ArgumentNullException("AwardLetterYear");
            }

            if (awardLetter.StudentId != studentAwardYear.StudentId)
            {
                throw new ArgumentException("StudentIds of awardLetter and studentAwardYear do not match");
            }

            //Verify that the award letter resource exists for this award year before updating it. We don't want to end up
            //creating db records if the resource doesn't exist. If it doesn't exist, this method call throws an exception.            
            var originalAwardLetter = await BuildAwardLetter2(awardLetter.StudentId, studentAwardYear, allAwards);

            UpdateAwardLetterSignedDateRequest request = new UpdateAwardLetterSignedDateRequest();
            request.AwardLetterHistId = awardLetter.Id;

            var transactionResponse = await transactionInvoker.ExecuteAsync<UpdateAwardLetterSignedDateRequest, UpdateAwardLetterSignedDateResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorMessage))
            {
                if (transactionResponse.ErrorMessage == "This award letter has already been signed. No update required.")
                {
                    var message = string.Format("The Award Letter has already been signed.");
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
                else
                {
                    var message = string.Format("Award Letter update canceled because record id {0} is locked.", awardLetter.StudentId);
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
            }

            //at this point, we can assume the transaction db update was successful.

            var studentAwardYears = new List<StudentAwardYear>();
            studentAwardYears.Add(studentAwardYear);
            var updatedAwardLetter = await GetAwardLetterById2Async(request.AwardLetterHistId, studentAwardYears, allAwards);

            return updatedAwardLetter;

        }


        #region Helpers        

        /// <summary>
        /// Build an award letter2 domain object for a given year 
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="studentAwardYear">Award Year</param>
        /// <returns>AwardLetter2 Domain object</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the year-specific award letter parameters record does not exist</exception>
        [Obsolete("Obsolete as of Api version 1.22, use BuildAwardLetter2 instead")]
        private async Task<AwardLetter2> BuildAwardLetter(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            // Get the most recent AwardLetterHistory record for this student and year
            string criteria = "WITH ALH.STUDENT.ID EQ '" + studentId + "' WITH ALH.AWARD.YEAR EQ '" + studentAwardYear.Code + "'"; 
            var awardLetterHistoryRecords = await DataReader.BulkReadRecordAsync<AwardLetterHistory>(criteria);

            //get the award letter history records for the year
            var awardLetterHistoryRecordsForYear = awardLetterHistoryRecords != null ?
                awardLetterHistoryRecords.Where(r => r.AlhAwardYear == studentAwardYear.Code).ToList().OrderByDescending(a => a.AlhAwardLetterDate).OrderByDescending(a => a.AwardLetterHistoryAddtime).FirstOrDefault() : null;
            
            var sortedAwardLetterHistoryRecords = awardLetterHistoryRecords != null ?
                awardLetterHistoryRecords.Where(r => r.AlhAwardYear == studentAwardYear.Code).ToList().OrderByDescending(a => a.AlhAwardLetterDate) : null;
            
            var mostRecentDate = sortedAwardLetterHistoryRecords.FirstOrDefault().AlhAwardLetterDate;
            
            var mostRecentAwardLetterHistoryRecords = sortedAwardLetterHistoryRecords.Where(a => a.AlhAwardLetterDate == mostRecentDate);

            var mostRecentRecord = mostRecentAwardLetterHistoryRecords.OrderByDescending(a => a.AwardLetterHistoryAddtime).FirstOrDefault();

            if (mostRecentRecord == null )
            {
                var message = string.Format("No Award Letter History records for student {0}", studentId);
                logger.Warn(message);
            }

            return await BuildAwardLetterEntity(studentAwardYear, allAwards, mostRecentRecord);
        }

        /// <summary>
        /// Retrieves award letter history record by id and returns an AwardLetter2 entity
        /// </summary>
        /// <param name="studentAwardYears">list of student award years</param>
        /// <param name="allAwards">list of reference awards</param>
        /// <param name="recordId">record id of the award letter history record</param>
        /// 
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.22, use BuildAwardLetter2 instead")]
        private async Task<AwardLetter2> BuildAwardLetter(IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards, string recordId)
        {
            //Get the award letter history record by the record id            
            var awardLetterHistoryRecord = await DataReader.ReadRecordAsync<AwardLetterHistory>(recordId);
            if (awardLetterHistoryRecord == null)
            {
                throw new KeyNotFoundException(string.Format("No award letter history record {0} was found for the student", recordId));
            }
            var awardYear = awardLetterHistoryRecord.AlhAwardYear;

            return await BuildAwardLetterEntity(studentAwardYears.FirstOrDefault(y => y.Code == awardYear), allAwards, awardLetterHistoryRecord);
        }

        /// <summary>
        /// Builds an AwardLetter2 entity
        /// </summary>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="allAwards">awards reference data</param>
        /// <param name="awardLetterHistoryRecord">award letter history record</param>
        /// <returns>AwardLetter2 entity</returns>
        [Obsolete("Obsolete as of Api version 1.22, use BuildAwardLetterEntity2 instead")]
        private async Task<AwardLetter2> BuildAwardLetterEntity(StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, AwardLetterHistory awardLetterHistoryRecord)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear cannot be null");
            }

            string studentId = awardLetterHistoryRecord != null ? awardLetterHistoryRecord.AlhStudentId : null;

            //Evaluate the award letter rule table to get the parameters record id
            AltrParameters awardLetterParametersData = await GetAwardLetterParametersData(studentId, studentAwardYear);

            var awardLetter2Entity = new AwardLetter2(studentId, studentAwardYear);

            awardLetter2Entity.AwardYearDescription = studentAwardYear.Description;

            await SetAwardLetterContactBlock(studentId, studentAwardYear.CurrentOffice, awardLetter2Entity);

            if (awardLetterHistoryRecord != null)
            {
                awardLetter2Entity.AwardLetterParameterId = awardLetterHistoryRecord.AlhAwardLetterParamsId;
                awardLetter2Entity.Id = awardLetterHistoryRecord.Recordkey;
                awardLetter2Entity.BudgetAmount = (awardLetterHistoryRecord.AlhCost.HasValue) ? awardLetterHistoryRecord.AlhCost.Value : 0;
                awardLetter2Entity.EstimatedFamilyContributionAmount = (awardLetterHistoryRecord.AlhEfc.HasValue) ? awardLetterHistoryRecord.AlhEfc.Value : 0;
                awardLetter2Entity.NeedAmount = (awardLetterHistoryRecord.AlhNeed.HasValue) ? awardLetterHistoryRecord.AlhNeed.Value : 0;
                awardLetter2Entity.AcceptedDate = awardLetterHistoryRecord.AlhAcceptedDate;
                awardLetter2Entity.CreatedDate = awardLetterHistoryRecord.AlhAwardLetterDate;
                awardLetter2Entity.StudentOfficeCode = awardLetterHistoryRecord.AlhOfficeId;

                //Paragraph spacing parameter
                var paragraphSpacing = awardLetterParametersData.AltrParaSpacing;

                awardLetter2Entity.OpeningParagraph = FormatParagraph(awardLetterHistoryRecord.AlhOpeningParagraph, paragraphSpacing);
                awardLetter2Entity.ClosingParagraph = FormatParagraph(awardLetterHistoryRecord.AlhClosingParagraph, paragraphSpacing);

                //Set housing info if inidicated
                awardLetter2Entity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.AlhHousingCode);

                var awardLetterGroups = new List<AwardLetterGroup2>();
                var origGroupInfo = awardLetterHistoryRecord.AlhGroupsEntityAssociation;
                foreach (var singleGroup in origGroupInfo)
                {
                    int groupNumber;
                    groupNumber = singleGroup.AlhGroupNumberAssocMember.Value;

                    var newGroup = new AwardLetterGroup2(singleGroup.AlhGroupNameAssocMember, groupNumber, GroupType.AwardCategories);
                    awardLetterGroups.Add(newGroup);
                }

                awardLetter2Entity.AwardLetterGroups = awardLetterGroups;

                //Get award letter annual awards
                awardLetter2Entity.AwardLetterAnnualAwards = GetAwardLetterAnnualAwards(allAwards, awardLetterHistoryRecord, awardLetterGroups);

                //Student name and address
                awardLetter2Entity.StudentName = awardLetterHistoryRecord.AlhStudentName;

                awardLetter2Entity.StudentAddress = new List<string>();
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefName))
                {
                    awardLetter2Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefName);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine1))
                {
                    awardLetter2Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine1);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine2))
                {
                    awardLetter2Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine2);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine3))
                {
                    awardLetter2Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine3);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine4))
                {
                    awardLetter2Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine4);
                }
            }

            return awardLetter2Entity;
        }

        /// <summary>
        /// Build an AwardLetter3 domain object for a given year 
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="studentAwardYear">Award Year</param>
        /// <returns>AwardLetter3 Domain object</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the year-specific award letter parameters record does not exist</exception>
        private async Task<AwardLetter3> BuildAwardLetter2(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            // Get the most recent AwardLetterHistory record for this student and year
            string criteria = "WITH ALH.STUDENT.ID EQ '" + studentId + "' WITH ALH.AWARD.YEAR EQ '" + studentAwardYear.Code + "'";
            var awardLetterHistoryRecords = await DataReader.BulkReadRecordAsync<AwardLetterHistory>(criteria);

            //get the most recent award letter history record for the year
            var mostRecentAwardLetterHistoryRecordForYear = awardLetterHistoryRecords != null ?
                awardLetterHistoryRecords.Where(r => r.AlhAwardYear == studentAwardYear.Code)
                .OrderByDescending(a => a.AlhAwardLetterDate)
                .ThenByDescending(a => a.AwardLetterHistoryAddtime).FirstOrDefault() : null;
            

            if (mostRecentAwardLetterHistoryRecordForYear == null)
            {
                var message = string.Format("No Award Letter History records for student {0}", studentId);
                logger.Warn(message);
                throw new KeyNotFoundException(message);
            }

            return await BuildAwardLetterEntity2(studentAwardYear, allAwards, mostRecentAwardLetterHistoryRecordForYear);
        }
        

        /// <summary>
        /// Retrieves award letter history record by id and returns an AwardLetter3 entity
        /// </summary>
        /// <param name="studentAwardYears">list of student award years</param>
        /// <param name="allAwards">list of reference awards</param>
        /// <param name="recordId">record id of the award letter history record</param>
        /// 
        /// <returns>AwardLetter3 entity</returns>
        private async Task<AwardLetter3> BuildAwardLetter2(IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards, string recordId)
        {
            //Get the award letter history record by the record id            
            var awardLetterHistoryRecord = await DataReader.ReadRecordAsync<AwardLetterHistory>(recordId);
            if (awardLetterHistoryRecord == null)
            {
                throw new KeyNotFoundException(string.Format("No award letter history record {0} was found for the student", recordId));
            }
            var awardYear = awardLetterHistoryRecord.AlhAwardYear;

            return await BuildAwardLetterEntity2(studentAwardYears.FirstOrDefault(y => y.Code == awardYear), allAwards, awardLetterHistoryRecord);
        }
        

        /// <summary>
        /// Builds an AwardLetter3 entity
        /// </summary>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="allAwards">awards reference data</param>
        /// <param name="awardLetterHistoryRecord">award letter history record</param>
        /// <returns>AwardLetter3 entity</returns>
        private async Task<AwardLetter3> BuildAwardLetterEntity2(StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, AwardLetterHistory awardLetterHistoryRecord)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear cannot be null");
            }

            string studentId = awardLetterHistoryRecord != null ? awardLetterHistoryRecord.AlhStudentId : null;


            //Evaluate the award letter rule table to get the parameters record id
            AltrParameters awardLetterParametersData = await GetAwardLetterParametersData(studentId, studentAwardYear, awardLetterHistoryRecord.AlhAwardLetterParamsId);

            var awardLetter3Entity = new AwardLetter3(studentId, studentAwardYear);

            awardLetter3Entity.AwardYearDescription = studentAwardYear.Description;

            await SetAwardLetterContactBlock2(studentId, studentAwardYear.CurrentOffice, awardLetter3Entity);

            if (awardLetterHistoryRecord != null)
            {
                awardLetter3Entity.AwardLetterParameterId = awardLetterHistoryRecord.AlhAwardLetterParamsId;
                awardLetter3Entity.Id = awardLetterHistoryRecord.Recordkey;
                awardLetter3Entity.BudgetAmount = (awardLetterHistoryRecord.AlhCost.HasValue) ? awardLetterHistoryRecord.AlhCost.Value : 0;
                awardLetter3Entity.EstimatedFamilyContributionAmount = awardLetterHistoryRecord.AlhEfc;
                awardLetter3Entity.NeedAmount = (awardLetterHistoryRecord.AlhNeed.HasValue) ? awardLetterHistoryRecord.AlhNeed.Value : 0;
                awardLetter3Entity.AcceptedDate = awardLetterHistoryRecord.AlhAcceptedDate;
                awardLetter3Entity.CreatedDate = awardLetterHistoryRecord.AlhAwardLetterDate;
                awardLetter3Entity.StudentOfficeCode = awardLetterHistoryRecord.AlhOfficeId;
                string alhLetterType = awardLetterHistoryRecord.AlhLetterType;

                if (string.IsNullOrEmpty(alhLetterType))
                {
                    alhLetterType = "ALTR";
                }

                awardLetter3Entity.AwardLetterHistoryType = alhLetterType;

                // Check to see if Award Letter History Type is anything other than old. If so, do new calculations
                if (alhLetterType == "OLTR")
                {
                    // Logic to assign 
                    awardLetter3Entity.PreAwardText = awardLetterHistoryRecord.AlhPreAwardText;
                    awardLetter3Entity.PostAwardText = awardLetterHistoryRecord.AlhPostAwardText;
                    awardLetter3Entity.PostClosingText = awardLetterHistoryRecord.AlhPostClosingText;

                    // Assignment of Direct cost information from Record to AwardLetter3 Entity.
                    var alhDirectCostAmount = new List<int>();
                    foreach (var amount in awardLetterHistoryRecord.AlhDirectCostAmount)
                    {
                        var nonNullAmount = amount.GetValueOrDefault();
                        alhDirectCostAmount.Add(nonNullAmount);
                    }
                    awardLetter3Entity.AlhDirectCostAmount = alhDirectCostAmount;
                    awardLetter3Entity.AlhDirectCostDesc = awardLetterHistoryRecord.AlhDirectCostDesc;
                    awardLetter3Entity.AlhDirectCostComp = awardLetterHistoryRecord.AlhDirectCostComp;

                    // Assignment of Indirect cost information from Record to AwardLetter3 Entity.
                    var alhIndirectCostAmount = new List<int>();
                    foreach (var amount in awardLetterHistoryRecord.AlhIndirectCostAmount)
                    {
                        var nonNullAmount = amount.GetValueOrDefault();
                        alhIndirectCostAmount.Add(nonNullAmount);
                    }
                    awardLetter3Entity.AlhIndirectCostAmount = alhIndirectCostAmount;
                    awardLetter3Entity.AlhIndirectCostDesc = awardLetterHistoryRecord.AlhIndirectCostDesc;
                    awardLetter3Entity.AlhIndirectCostComp = awardLetterHistoryRecord.AlhIndirectCostComp;

                    //Assignment of Enrollment Status & Housing Status information to AwardLetter3 Entity.
                    awardLetter3Entity.AlhHousingInd = awardLetterHistoryRecord.AlhHousingInd;
                    

                    //Assignment of Pell Entitlement Information
                    awardLetter3Entity.AlhPellEntitlementList = awardLetterHistoryRecord.AlhPellEntitlements;
                }

                //Paragraph spacing parameter
                var paragraphSpacing = awardLetterParametersData.AltrParaSpacing;

                awardLetter3Entity.OpeningParagraph = FormatParagraph(awardLetterHistoryRecord.AlhOpeningParagraph, paragraphSpacing);
                awardLetter3Entity.ClosingParagraph = FormatParagraph(awardLetterHistoryRecord.AlhClosingParagraph, paragraphSpacing);

                //Set housing info if inidicated
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhHousingCode))
                {
                    awardLetter3Entity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.AlhHousingCode);
                }
                else
                {
                    awardLetter3Entity.HousingCode = null;
                }

                var awardLetterGroups = new List<AwardLetterGroup2>();
                var origGroupInfo = awardLetterHistoryRecord.AlhGroupsEntityAssociation;
                foreach (var singleGroup in origGroupInfo)
                {
                    int groupNumber;
                    groupNumber = singleGroup.AlhGroupNumberAssocMember.Value;

                    var newGroup = new AwardLetterGroup2(singleGroup.AlhGroupNameAssocMember, groupNumber, GroupType.AwardCategories);
                    awardLetterGroups.Add(newGroup);
                }

                awardLetter3Entity.AwardLetterGroups = awardLetterGroups;

                //Get award letter annual awards
                var awardLetterAnnualAwards  = GetAwardLetterAnnualAwards(allAwards, awardLetterHistoryRecord, awardLetterGroups);
                awardLetter3Entity.AwardLetterAnnualAwards = awardLetterAnnualAwards;
                var awardPeriodAssociationTable = awardLetterHistoryRecord.AlhAwardPeriodTableEntityAssociation;

                var alhHousingDesc = new List<string>();
                var alhEnrollmentDesc = new List<string>();

                var awardPeriodsForYear = awardLetterAnnualAwards.Any() ? awardLetterAnnualAwards.SelectMany(a => a.AwardLetterAwardPeriods).ToList() : new List<Domain.FinancialAid.Entities.AwardLetterAwardPeriod>();
                if (awardPeriodsForYear != null && awardPeriodsForYear.Any())
                {
                    var distinctAwardPeriodColumnsForYear = awardPeriodsForYear.Any() ? awardPeriodsForYear.Select(ap => ap.ColumnNumber).Distinct().OrderBy(cn => cn).ToList() : new List<int>();

                    var distinctAwardPeriods = new List<AwardLetterHistoryAlhAwardPeriodTable>();

                    foreach (var columnNumber in distinctAwardPeriodColumnsForYear)
                    {

                        var distinctAwardPeriod = awardPeriodAssociationTable.Where(ap => ap.AlhColumnGroupNumberAssocMember == columnNumber.ToString()).FirstOrDefault();
                        distinctAwardPeriods.Add(distinctAwardPeriod);
                    }

                    if (distinctAwardPeriods.Any())
                    {
                        foreach (var awardPeriod in distinctAwardPeriods)
                        {
                            if (awardPeriod.AlhHousingDescAssocMember != null || awardPeriod.AlhHousingDescAssocMember != "")
                            {
                                alhHousingDesc.Add(awardPeriod.AlhHousingDescAssocMember);
                            }
                            if (awardPeriod.AlhEnrlDescAssocMember != null || awardPeriod.AlhEnrlDescAssocMember != "")
                            {
                                alhEnrollmentDesc.Add(awardPeriod.AlhEnrlDescAssocMember);
                            }
                        }
                    }
                }


                if (alhLetterType == "OLTR")
                {
                    awardLetter3Entity.AlhEnrollmentStatus = alhEnrollmentDesc;
                    awardLetter3Entity.AlhHousingDesc = alhHousingDesc;
                }
                else
                {
                    awardLetter3Entity.AlhEnrollmentStatus = awardLetterHistoryRecord.AlhEnrlDesc;
                    awardLetter3Entity.AlhHousingDesc = awardLetterHistoryRecord.AlhHousingDesc;
                }

                //Student name and address
                awardLetter3Entity.StudentName = awardLetterHistoryRecord.AlhStudentName;

                awardLetter3Entity.StudentAddress = new List<string>();
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefName))
                {
                    awardLetter3Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefName);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine1))
                {
                    awardLetter3Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine1);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine2))
                {
                    awardLetter3Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine2);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine3))
                {
                    awardLetter3Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine3);
                }
                if (!string.IsNullOrEmpty(awardLetterHistoryRecord.AlhPrefAddrLine4))
                {
                    awardLetter3Entity.StudentAddress.Add(awardLetterHistoryRecord.AlhPrefAddrLine4);
                }
            }

            return awardLetter3Entity;
        }

        /// <summary>
        /// Method to generate a new AwardLetterHistory record if one is needed
        /// </summary>
        /// <param name="studentId">student's Colleague Id</param>
        /// <param name="awardYear">Award Year to use for the award comparison</param>
        /// <returns>Nothing</returns>
        private async Task CreateAwardLetterHistoryRecordAsync(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            //Set up the request
            var request = new CreateAwardLetterHistoryRequest();
            request.Year = awardYear;
            request.StudentId = studentId;

            //Execute
            var response = await transactionInvoker.ExecuteAsync<CreateAwardLetterHistoryRequest, CreateAwardLetterHistoryResponse>(request);

            if (response == null)
            {
                var message = "Error getting CreateAwardLetterHistory transaction response from Colleague";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = string.Format("Error creating award letter history record");
                logger.Error(message);
                throw new ApplicationException(message);
            }

            return;
        }

        /// <summary>
        /// Helper method that executes a Colleague transaction that evaluates a rule table to determine
        /// which award letter parameters record to use.
        /// </summary>
        /// <param name="year">The year of the award letter</param>
        /// <param name="studentId">The student Id of the award letter</param>
        /// <returns>The Award Letter Parameters record id to use to get data for an award letter</returns>
        private async Task<string> GetAwardLetterParametersRecordIdAsync(string year, string studentId)
        {
            //Set up the request
            var request = new EvalAwardLetterParamsRuleTableRequest();
            request.Year = year;
            request.StudentId = studentId;

            //Execute
            var response = await transactionInvoker.ExecuteAsync<EvalAwardLetterParamsRuleTableRequest, EvalAwardLetterParamsRuleTableResponse>(request);

            //Log any messages. Messages returned here are not fatal. They probably indicate which rules from the rule table failed 
            foreach (var message in response.LogMessages)
            {
                logger.Info(message);
            }

            //Return the result of the transaction, which is the Award Letter Parameters record id
            return response.Result;
        }

        /// <summary>
        /// Get and cache Award Letter Parameters data
        /// </summary>
        /// <returns>Collection of <see cref="AltrParameters"/> DataContracts</returns>
        private async Task<Collection<AltrParameters>> GetAwardLetterParametersRecordDataAsync()
        {
            return await GetOrAddToCacheAsync<Collection<AltrParameters>>("AwardLetterParameters",
                async () =>
                {
                    var awardLetterParameters = await DataReader.BulkReadRecordAsync<AltrParameters>("", false);
                    if (awardLetterParameters != null)
                    {
                        return awardLetterParameters;
                    }
                    else
                    {
                        logger.Info("Null AltrParameters returned from database");
                        return new Collection<AltrParameters>();
                    }
                });
        }

       /// <summary>
       /// Paragraph formatting utility
       /// </summary>
       /// <param name="source">String to convert</param>
       /// <param name="spacing">Spacing to use when converting</param>
       /// <returns></returns>
        private string FormatParagraph(string source, string spacing)
        {
            var formattedString = source;
            if (!string.IsNullOrEmpty(source))
            {
                if (spacing == "2")
                {
                    formattedString = source.Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine);
                }

                //Remove spaces from urls (if any)
                if (formattedString.Contains("<a href="))
                {
                    int strLen = formattedString.Length;
                    int formattedStrLen = strLen;

                    //Difference in length between the initial string length and each time it is formatted
                    int difference = 0;

                    //Indices before and after a url
                    int urlStartIndex = 0;
                    int indexAfterUrlEnd = 0;

                    var urls = Regex.Matches(formattedString, @"href=[""']([^""'])+[""']");

                    foreach (Match url in urls)
                    {
                        foreach (Capture capture in url.Captures)
                        {
                            var formattedUrl = capture.Value.ToString().Replace(" ", "").Replace(Environment.NewLine, "");

                            urlStartIndex = capture.Index - difference;
                            indexAfterUrlEnd = capture.Index + capture.Length - difference;

                            if ((urlStartIndex >= 0 && urlStartIndex < formattedStrLen) && (indexAfterUrlEnd > 0 && indexAfterUrlEnd < formattedStrLen) && (urlStartIndex < indexAfterUrlEnd))
                            {
                                formattedString = formattedString.Substring(0, urlStartIndex) + formattedUrl + formattedString.Substring(indexAfterUrlEnd);
                                formattedStrLen = formattedString.Length;
                                difference = strLen - formattedStrLen;
                            }

                        }

                    }
                }
            }

            return formattedString;
        }

        /// <summary>
        /// Helper method to set the Contact block information of the given award letter entity.
        /// </summary>
        /// <param name="studentId">StudentId</param>
        /// <param name="year">Award Year</param>
        /// <param name="awardLetter2Entity">The AwardLetter object to set</param>
        /// <param name="csDataRecord">The CsAcyr DataContract - must be populated with data for this method call</param>
        private async Task SetAwardLetterContactBlock(string studentId, FinancialAidOffice currentOffice, AwardLetter2 awardLetter2Entity)
        {
            //Get the static system parameters
            try
            {
                var systemParametersData = await GetSystemParametersDataAsync();

                //default values for the contact block come from the system parameters
                awardLetter2Entity.ContactName = systemParametersData.FspInstitutionName;
                awardLetter2Entity.ContactAddress = new List<string>();
                systemParametersData.FspInstitutionAddress.ForEach(a => awardLetter2Entity.ContactAddress.Add(a));
                awardLetter2Entity.ContactAddress.Add(systemParametersData.FspInstitutionCsz);
                awardLetter2Entity.ContactPhoneNumber = systemParametersData.FspPellPhoneNumber;
            }
            catch (Exception e)
            {;
                logger.Info(e, "Error getting FaSysParams data and setting award letter entity default contact address");
            }

            //if the student has a current office, update the contact information with the office-specific contact info.
            if (currentOffice != null)
            {
                //update the name, if it exists
                if (!string.IsNullOrEmpty(currentOffice.Name))
                {
                    awardLetter2Entity.ContactName = currentOffice.Name;
                }

                //if all parts of the address exist, update the address
                if (currentOffice.AddressLabel.Count() > 0)
                {
                    awardLetter2Entity.ContactAddress = currentOffice.AddressLabel;
                }

                //update the phone number if it exists.
                if (!string.IsNullOrEmpty(currentOffice.PhoneNumber))
                {
                    awardLetter2Entity.ContactPhoneNumber = currentOffice.PhoneNumber;
                }
            }

        }

        /// <summary>
        /// Helper method to set the Contact block information of the given award letter entity.
        /// </summary>
        /// <param name="studentId">StudentId</param>
        /// <param name="year">Award Year</param>
        /// <param name="awardLetter3Entity">The AwardLetter3 object to set</param>
        /// <param name="csDataRecord">The CsAcyr DataContract - must be populated with data for this method call</param>
        private async Task SetAwardLetterContactBlock2(string studentId, FinancialAidOffice currentOffice, AwardLetter3 awardLetter3Entity)
        {
            //Get the static system parameters
            try
            {
                var systemParametersData = await GetSystemParametersDataAsync();

                //default values for the contact block come from the system parameters
                awardLetter3Entity.ContactName = systemParametersData.FspInstitutionName;
                awardLetter3Entity.ContactAddress = new List<string>();
                systemParametersData.FspInstitutionAddress.ForEach(a => awardLetter3Entity.ContactAddress.Add(a));
                awardLetter3Entity.ContactAddress.Add(systemParametersData.FspInstitutionCsz);
                awardLetter3Entity.ContactPhoneNumber = systemParametersData.FspPellPhoneNumber;
            }
            catch (Exception e)
            {
                logger.Info(e, "Error getting FaSysParams data and setting award letter entity default contact address");
            }

            //if the student has a current office, update the contact information with the office-specific contact info.
            if (currentOffice != null)
            {
                //update the name, if it exists
                if (!string.IsNullOrEmpty(currentOffice.Name))
                {
                    awardLetter3Entity.ContactName = currentOffice.Name;
                }

                //if all parts of the address exist, update the address
                if (currentOffice.AddressLabel.Count() > 0)
                {
                    awardLetter3Entity.ContactAddress = currentOffice.AddressLabel;
                }

                //update the phone number if it exists.
                if (!string.IsNullOrEmpty(currentOffice.PhoneNumber))
                {
                    awardLetter3Entity.ContactPhoneNumber = currentOffice.PhoneNumber;
                }
            }

        }

        /// <summary>
        /// Get and cache System Parameters data
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private async Task<FaSysParams> GetSystemParametersDataAsync()
        {
            return await GetOrAddToCacheAsync<FaSysParams>("FinancialAidSystemParameters",
                async () =>
                {
                    return await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                });
        }

        /// <summary>
        /// Helper method to translate a Housing Code. 
        /// Default code is OnCampus
        /// </summary>
        /// <param name="housingCode">Housing Code to translate</param>
        /// <returns></returns>
        private HousingCode TranslateHousingCode(string housingCode)
        {
            if (housingCode == null) housingCode = "";
            switch (housingCode.ToUpper())
            {
                case "1":
                    return HousingCode.OnCampus;

                case "2":
                    return HousingCode.WithParent;

                case "3":
                    return HousingCode.OffCampus;

                default:
                    return HousingCode.OnCampus;
            }
        }

        /// <summary>
        /// Get and cache Awards data
        /// </summary>
        /// <returns>Awards DataContract</returns>
        private string SetAwardDescription(string awardId, IEnumerable<Award> allAwards)
        {
            return allAwards.FirstOrDefault(a => a.Code == awardId).Description;
        }

        /// <summary>
        /// Get and cache Awards data
        /// </summary>
        /// <returns>Awards DataContract</returns>
        private string SetAwardRenewableFlag(string awardId, IEnumerable<Award> allAwards)
        {
            return allAwards.FirstOrDefault(a => a.Code == awardId).AwRenewableFlag;
        }

        /// <summary>
        /// Get and cache Awards data
        /// </summary>
        /// <returns>Awards DataContract</returns>
        private string SetAwardRenewableText(string awardId, IEnumerable<Award> allAwards)
        {
            var renewableText = allAwards.FirstOrDefault(a => a.Code == awardId).AwRenewableText;
            var formattedRenewableText = renewableText.Replace("ý", " ");
            return formattedRenewableText;
        }


        private List<AwardLetterAnnualAward> GetAwardLetterAnnualAwards(IEnumerable<Award> allAwards, AwardLetterHistory awardLetterHistoryRecord, List<AwardLetterGroup2> awardLetterGroups)
        {
            var awardLetterAnnualAwards = new List<AwardLetterAnnualAward>();
            var origAnnualAwards = awardLetterHistoryRecord.AlhAnnualAwardTableEntityAssociation;
            foreach (var singleAward in origAnnualAwards)
            {
                var newAnnualAward = new AwardLetterAnnualAward();
                newAnnualAward.AwardId = singleAward.AlhAnnualAwardIdAssocMember;
                newAnnualAward.AnnualAnnualAmount = singleAward.AlhAnnualAwardAmountsAssocMember;
                newAnnualAward.AwardDescription = SetAwardDescription(newAnnualAward.AwardId, allAwards);
                newAnnualAward.AwRenewableFlag = SetAwardRenewableFlag(newAnnualAward.AwardId, allAwards);
                newAnnualAward.AwRenewableText = SetAwardRenewableText(newAnnualAward.AwardId, allAwards);

                int groupNumber;
                groupNumber = singleAward.AlhAnnualGroupNumberAssocMember.HasValue ? singleAward.AlhAnnualGroupNumberAssocMember.Value : -1;
                newAnnualAward.GroupNumber = groupNumber;

                var group = awardLetterGroups.Where(alg => alg.GroupNumber == groupNumber).FirstOrDefault();
                newAnnualAward.GroupName = group != null ? group.GroupName : string.Empty;


                var AwardLetterAwardPeriods = new List<AwardLetterAwardPeriod>();
                var origAwardPeriodRecords = awardLetterHistoryRecord.AlhAwardPeriodTableEntityAssociation.Where(a => a.AlhAwardIdAssocMember == newAnnualAward.AwardId);

                foreach (var singleRecord in origAwardPeriodRecords)
                {
                    var newAwardPeriod = new AwardLetterAwardPeriod();
                    int columnNumber;
                    newAwardPeriod.AwardId = singleRecord.AlhAwardIdAssocMember;
                    newAwardPeriod.AwardDescription = newAnnualAward.AwardDescription;
                    newAwardPeriod.AwardPeriodAmount = singleRecord.AlhAmountAssocMember;
                    newAwardPeriod.ColumnName = singleRecord.AlhColumnGroupNameAssocMember;
                    newAwardPeriod.ColumnNumber = (Int32.TryParse(singleRecord.AlhColumnGroupNumberAssocMember, out columnNumber) ? columnNumber : -1);
                    newAwardPeriod.GroupName = newAnnualAward.GroupName;
                    newAwardPeriod.GroupNumber = groupNumber;

                    AwardLetterAwardPeriods.Add(newAwardPeriod);
                }

                newAnnualAward.AwardLetterAwardPeriods = AwardLetterAwardPeriods;
                awardLetterAnnualAwards.Add(newAnnualAward);
            }

            return awardLetterAnnualAwards;
        }

        private async Task<AltrParameters> GetAwardLetterParametersData(string studentId, StudentAwardYear studentAwardYear, string awardLetterParametersId = null)
        {
            var awardLetterParameterRecordsData = await GetAwardLetterParametersRecordDataAsync();

            if (string.IsNullOrEmpty(awardLetterParametersId))
            {
                awardLetterParametersId = await GetAwardLetterParametersRecordIdAsync(studentAwardYear.Code, studentId);
            }

            //get the parameters data record. if it doesn't exist, log a message and move on to the next year
            var awardLetterParametersData = awardLetterParameterRecordsData.FirstOrDefault(alp => alp.Recordkey == awardLetterParametersId);
            if (awardLetterParametersData == null)
            {
                var message = string.Format("Award Letter Parameters record {0} does not exist. Verify Award Letter Rule Table for {1} is setup correctly.",
                    awardLetterParametersId, studentAwardYear.Code);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return awardLetterParametersData;
        }

        #endregion

    }
}
