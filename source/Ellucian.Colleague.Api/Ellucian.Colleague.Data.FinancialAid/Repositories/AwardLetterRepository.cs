//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// AwardLetterRepository class exposes database access to Colleague Award Letters. It
    /// gathers data from numerous tables based on student data and creates AwardLetter
    /// objects.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AwardLetterRepository : BaseColleagueRepository, IAwardLetterRepository
    {
        public AwardLetterRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// This method gets award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <param name="studentId">The student form whom to generate award letters</param>
        /// <returns>A list of year-specific award letters for the given student.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the student has no Financial Aid records</exception>
        public IEnumerable<AwardLetter> GetAwardLetters(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Fafsa> fafsaRecords)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("availableAwardYears");
            }
            if (fafsaRecords == null)
            {
                throw new ArgumentNullException("fafsaRecords");
            }

            //if the student has no year-specific financial aid data, return an empty list
            if (studentAwardYears.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId));
                return new List<AwardLetter>();
            }

            //if the student has no fed flagged fafsa records, log a message
            if (fafsaRecords.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no federally flagged fafsa records", studentId));
            }

            //instantiate the return list
            var awardLetterEntities = new List<AwardLetter>();

            //loop through each of the student's years
            foreach (var year in studentAwardYears)
            {
                try
                {
                    var fafsaRecord = fafsaRecords.FirstOrDefault(fr => fr.AwardYear == year.Code);
                    var awardLetterEntity = BuildAwardLetter(studentId, year, fafsaRecord);
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
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <returns>An award letter object specific to the given award year</returns>
        public AwardLetter GetAwardLetter(string studentId, StudentAwardYear studentAwardYear, Fafsa fafsaRecord)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("awardYear");
            }

            return BuildAwardLetter(studentId, studentAwardYear, fafsaRecord);
        }

        /// <summary>
        /// This method updates an Award Letter, specifically the award letter's Accepted Date
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the data with which to update the database</param>
        /// <returns>An award letter object with updated data</returns>
        public AwardLetter UpdateAwardLetter(AwardLetter awardLetter, StudentAwardYear studentAwardYear, Fafsa fafsaRecord)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (awardLetter.StudentId != studentAwardYear.StudentId)
            {
                throw new ArgumentException("StudentIds of awardLetter and studentAwardYear do not match");
            }
            if (awardLetter.AwardYear.Code != studentAwardYear.Code)
            {
                throw new ArgumentException("AwardYears of awardLetter and studentAwardYear do not match");
            }

            //Verify that the award letter resource exists for this award year before updating it. We don't want to end up
            //creating db records if the resource doesn't exist. If it doesn't exist, this method call throws an exception.            
            var originalAwardLetter = BuildAwardLetter(awardLetter.StudentId, studentAwardYear, fafsaRecord);

            UpdateAwardLetterRequest request = new UpdateAwardLetterRequest();
            request.StudentId = awardLetter.StudentId;
            request.Year = awardLetter.AwardYear.Code;
            request.AcceptedDate = awardLetter.AcceptedDate;

            var transactionResponse = transactionInvoker.Execute<UpdateAwardLetterRequest, UpdateAwardLetterResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorMessage))
            {
                if (transactionResponse.ErrorMessage == "YS.ACYR record is locked")
                {
                    var message = string.Format("Award Letter update canceled because record id {0} in YS.{1} table is locked.", awardLetter.StudentId, awardLetter.AwardYear);
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
            }

            //at this point, we can assume the transaction db update was successful.
            //now simply update the original award letter's accepted date with the date used to update the db, and return it.
            //if the update transaction becomes more complicated, we may need to re-call BuildAwardLetter
            originalAwardLetter.AcceptedDate = awardLetter.AcceptedDate;
            return originalAwardLetter;
        }

        #region Helpers

        /// <summary>
        /// Build an award letter domain object for a given year 
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="studentAwardYear">Award Year</param>
        /// <returns>AwardLetter Domain object</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the year-specific award letter parameters record does not exist</exception>
        private AwardLetter BuildAwardLetter(string studentId, StudentAwardYear studentAwardYear, Fafsa fafsaRecord)
        {

            //Evaluate the award letter rule table to get the parameters record id
            var awardLetterParametersId = GetAwardLetterParametersRecordId(studentAwardYear.Code, studentId);
            var awardLetterParameterRecordsData = GetAwardLetterParametersRecordData();

            //get the parameters data record. if it doesn't exist, log a message and move on to the next year
            var awardLetterParametersData = awardLetterParameterRecordsData.FirstOrDefault(alp => alp.Recordkey == awardLetterParametersId);
            if (awardLetterParametersData == null)
            {
                var message = string.Format("Award Letter Parameters record {0} does not exist. Verify Award Letter Rule Table for {1} is setup correctly.",
                    awardLetterParametersId, studentAwardYear.Code);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            //we need the student's CS.ACYR record
            var csAcyrFile = "CS." + studentAwardYear.Code;
            var csDataRecord = DataReader.ReadRecord<CsAcyr>(csAcyrFile, studentId);
            if (csDataRecord == null)
            {
                logger.Info(string.Format("Student {0} has no {1} record", studentId, csAcyrFile));
            }

            //also need the student's YS.ACYR record
            var ysAcyrFile = "YS." + studentAwardYear.Code;
            var ysDataRecord = DataReader.ReadRecord<YsAcyr>(ysAcyrFile, studentId);
            if (ysDataRecord == null)
            {
                logger.Info(string.Format("Student {0} has no {1} record", studentId, ysAcyrFile));
            }

            var awardLetterEntity = new AwardLetter(studentId, studentAwardYear);

            //Paragraph spacing parameter
            var paragraphSpacing = awardLetterParametersData.AltrParaSpacing;

            awardLetterEntity.OpeningParagraph = ConvertValueMarks(awardLetterParametersData.AltrIntroText, paragraphSpacing);
            awardLetterEntity.ClosingParagraph = ConvertValueMarks(awardLetterParametersData.AltrClosingText, paragraphSpacing);

            if (ysDataRecord != null)
            {
                awardLetterEntity.AcceptedDate = ysDataRecord.YsAwardLtrAcceptedDt;
            }

            //Set FA Need info if indicated
            if (awardLetterParametersData.AltrNeedBlock.ToUpper() == "Y" && csDataRecord != null)
            {
                awardLetterEntity.IsNeedBlockActive = true;
                SetAwardLetterNeedBlock(awardLetterEntity, csDataRecord);
            }

            //Set FA Contact info if indicated
            if (awardLetterParametersData.AltrOfficeBlock.ToUpper() == "Y")
            {
                awardLetterEntity.IsContactBlockActive = true;
                SetAwardLetterContactBlock(studentId, studentAwardYear.CurrentOffice, awardLetterEntity);
            }

            //Set housing info if inidicated
            if (awardLetterParametersData.AltrHousingCode.ToUpper() == "Y")
            {
                awardLetterEntity.IsHousingCodeActive = true;
                SetHousingCode(studentAwardYear, fafsaRecord, awardLetterEntity);

            }

            awardLetterEntity.AwardNameTitle = !string.IsNullOrEmpty(awardLetterParametersData.AltrTitleAwdName) ? awardLetterParametersData.AltrTitleAwdName : "Awards";
            awardLetterEntity.AwardTotalTitle = !string.IsNullOrEmpty(awardLetterParametersData.AltrTitleAwdTotal) ? awardLetterParametersData.AltrTitleAwdTotal : "Total";

            //Get award categories groupings           
            var awardCategoriesTitleList = new List<string>()
            {
                awardLetterParametersData.AltrTitleGroup1,
                awardLetterParametersData.AltrTitleGroup2                
            };

            var awardCategoriesGroupList = new List<List<string>>()
            {
                awardLetterParametersData.AltrCategoryGroup1,
                awardLetterParametersData.AltrCategoryGroup2
            };

            for (var i = 0; i < awardCategoriesGroupList.Count; i++)
            {
                if (awardCategoriesGroupList[i].Count != 0)
                {
                    if (!awardLetterEntity.AddAwardCategoryGroup(awardCategoriesTitleList[i], i, GroupType.AwardCategories))
                    {
                        logger.Info(string.Format("Could not add {0} group to the award letter.", awardCategoriesTitleList[i]));
                    }
                    var currentGroup = awardLetterEntity.AwardCategoriesGroups.First(alg => alg.GroupType == GroupType.AwardCategories &&
                        alg.SequenceNumber == i);

                    if (currentGroup != null)
                    {
                        foreach (var member in awardCategoriesGroupList[i])
                        {
                            if (!currentGroup.AddGroupMember(member))
                            {
                                logger.Info(string.Format("Could not add {0} award category to the award category group.", member));
                            }
                        }
                    }
                }
            }

            //Last group gets the rest of award categories, list of award categories is not assigned here
            awardLetterEntity.NonAssignedAwardsGroup = new AwardLetterGroup(awardLetterParametersData.AltrTitleGroup3, awardCategoriesGroupList.Count, GroupType.AwardCategories);

            //Get award period columns groupings           
            var awardPeriodTitleList = new List<string>()
            {
                awardLetterParametersData.AltrTitleColumn1,
                awardLetterParametersData.AltrTitleColumn2,
                awardLetterParametersData.AltrTitleColumn3,
                awardLetterParametersData.AltrTitleColumn4,
                awardLetterParametersData.AltrTitleColumn5,
                awardLetterParametersData.AltrTitleColumn6
            };

            var awardPeriodGroupList = new List<List<string>>()
            {
                awardLetterParametersData.AltrAwdPerColumn1,
                awardLetterParametersData.AltrAwdPerColumn2,
                awardLetterParametersData.AltrAwdPerColumn3,
                awardLetterParametersData.AltrAwdPerColumn4,
                awardLetterParametersData.AltrAwdPerColumn5,
                awardLetterParametersData.AltrAwdPerColumn6
            };

            for (var i = 0; i < awardPeriodTitleList.Count; i++)
            {
                if (awardPeriodGroupList[i].Count != 0)
                {
                    if (!awardLetterEntity.AddAwardPeriodColumnGroup(awardPeriodTitleList[i], i, GroupType.AwardPeriodColumn))
                    {
                        logger.Info(string.Format("Could not add {0} group to the award letter.", awardPeriodTitleList[i]));
                    }
                    var currentGroup = awardLetterEntity.AwardPeriodColumnGroups.First(alg => alg.GroupType == GroupType.AwardPeriodColumn &&
                        alg.SequenceNumber == i);

                    if (currentGroup != null)
                    {
                        foreach (var member in awardPeriodGroupList[i])
                        {
                            if (!currentGroup.AddGroupMember(member))
                            {
                                logger.Info(string.Format("Could not add {0} award category to the award category group.", member));
                            }
                        }
                    }
                }
            }

            return awardLetterEntity;
        }

        /// <summary>
        /// Sets the award entity housing code to its value
        /// </summary>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="fafsaRecord">FAFSA record for the year</param>
        /// <param name="awardLetterEntity">award letter entity</param>
        private static void SetHousingCode(StudentAwardYear studentAwardYear, Fafsa fafsaRecord, AwardLetter awardLetterEntity)
        {
            if (fafsaRecord != null)
            {
                HousingCode? housingCode = null;
                fafsaRecord.HousingCodes.TryGetValue(studentAwardYear.CurrentOffice.TitleIVCode, out housingCode);
                awardLetterEntity.HousingCode = housingCode;
            }

        }

        /// <summary>
        /// Helper method that executes a Colleague transaction that evaluates a rule table to determine
        /// which award letter parameters record to use.
        /// </summary>
        /// <param name="year">The year of the award letter</param>
        /// <param name="studentId">The student Id of the award letter</param>
        /// <returns>The Award Letter Parameters record id to use to get data for an award letter</returns>
        private string GetAwardLetterParametersRecordId(string year, string studentId)
        {
            //Set up the request
            var request = new EvalAwardLetterParamsRuleTableRequest();
            request.Year = year;
            request.StudentId = studentId;

            //Execute
            var response = transactionInvoker.Execute<EvalAwardLetterParamsRuleTableRequest, EvalAwardLetterParamsRuleTableResponse>(request);

            //Log any messages. Messages returned here are not fatal. They probably indicate which rules from the rule table failed 
            foreach (var message in response.LogMessages)
            {
                logger.Info(message);
            }

            //Return the result of the transaction, which is the Award Letter Parameters record id
            return response.Result;
        }

        /// <summary>
        /// Helper method to set the Contact block information of the given award letter entity.
        /// </summary>
        /// <param name="studentId">StudentId</param>
        /// <param name="year">Award Year</param>
        /// <param name="awardLetterEntity">The AwardLetter object to set</param>
        /// <param name="csDataRecord">The CsAcyr DataContract - must be populated with data for this method call</param>
        private void SetAwardLetterContactBlock(string studentId, FinancialAidOffice currentOffice, AwardLetter awardLetterEntity)
        {
            //Get the static system parameters
            try
            {
                var systemParametersData = GetSystemParametersData();

                //default values for the contact block come from the system parameters
                awardLetterEntity.ContactName = systemParametersData.FspInstitutionName;
                awardLetterEntity.ContactAddress = new List<string>();
                systemParametersData.FspInstitutionAddress.ForEach(a => awardLetterEntity.ContactAddress.Add(a));
                awardLetterEntity.ContactAddress.Add(systemParametersData.FspInstitutionCsz);
                awardLetterEntity.ContactPhoneNumber = systemParametersData.FspPellPhoneNumber;
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
                    awardLetterEntity.ContactName = currentOffice.Name;
                }

                //if all parts of the address exist, update the address
                if (currentOffice.AddressLabel.Count() > 0)
                {
                    awardLetterEntity.ContactAddress = currentOffice.AddressLabel;
                }

                //update the phone number if it exists.
                if (!string.IsNullOrEmpty(currentOffice.PhoneNumber))
                {
                    awardLetterEntity.ContactPhoneNumber = currentOffice.PhoneNumber;
                }
            }

        }

        /// <summary>
        /// Helper method to set the Financial Need block information on an AwardLetter object
        /// </summary>
        /// <param name="awardLetterEntity">The AwardLetter object to update</param>
        /// <param name="csDataRecord">The CsAcyr DataContract object that contains the Financial need data</param>
        private void SetAwardLetterNeedBlock(AwardLetter awardLetterEntity, CsAcyr csDataRecord)
        {
            //set null values to 0
            if (!csDataRecord.CsStdTotalExpenses.HasValue) { csDataRecord.CsStdTotalExpenses = 0; }
            if (!csDataRecord.CsBudgetAdj.HasValue) { csDataRecord.CsBudgetAdj = 0; }
            if (!csDataRecord.CsInstAdj.HasValue) { csDataRecord.CsInstAdj = 0; }
            if (!csDataRecord.CsNeed.HasValue) { csDataRecord.CsNeed = 0; }

            //CS.FC is defined as a string in the db. Attempt to Parse the string value to an int.
            //if the original string value is empty, or the parse fails, set the int value to 0
            int efc;
            if (!string.IsNullOrEmpty(csDataRecord.CsFc))
            {
                var IsParsed = int.TryParse(csDataRecord.CsFc, out efc);
                if (!IsParsed)
                {
                    efc = 0;
                    logger.Info(string.Format("CsFc has invalid value {0}. CS.{1} with key {2} may be corrupt", csDataRecord.CsFc, awardLetterEntity.AwardYear, awardLetterEntity.StudentId));
                }
            }
            else
            {
                efc = 0;
            }

            //set the need block attributes
            awardLetterEntity.SetBudgetAmount(csDataRecord.CsStdTotalExpenses.Value, csDataRecord.CsBudgetAdj.Value);
            awardLetterEntity.SetEstimatedFamilyContributionAmount(efc, csDataRecord.CsInstAdj.Value);
            awardLetterEntity.NeedAmount = csDataRecord.CsNeed.Value;
        }

        private string ConvertValueMarks(string source, string spacing)
        {
            var replace = source;
            if (!string.IsNullOrEmpty(source))
            {
                char _vm = Convert.ToChar(DynamicArray.VM);
                string paragraphSpacing = "" + Environment.NewLine;
                if (spacing == "2")
                {
                    paragraphSpacing += Environment.NewLine;
                }
                //first replace two _vms with a new line, then replace remaining single vms with a space
                replace = source.Replace("" + _vm + _vm, paragraphSpacing).Replace(_vm, ' ');
            }
            return replace;
        }

        #endregion

        #region CacheDataRecords

        /// <summary>
        /// Get and cache System Parameters data
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private FaSysParams GetSystemParametersData()
        {
            return GetOrAddToCache<FaSysParams>("FinancialAidSystemParameters",
                () =>
                {
                    return DataReader.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                });
        }

        /// <summary>
        /// Get and cache Award Letter Parameters data
        /// </summary>
        /// <returns>Collection of <see cref="AltrParameters"/> DataContracts</returns>
        private Collection<AltrParameters> GetAwardLetterParametersRecordData()
        {
            return GetOrAddToCache<Collection<AltrParameters>>("AwardLetterParameters",
                () =>
                {
                    var awardLetterParameters = DataReader.BulkReadRecord<AltrParameters>("", false);
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
        #endregion


    }
}
