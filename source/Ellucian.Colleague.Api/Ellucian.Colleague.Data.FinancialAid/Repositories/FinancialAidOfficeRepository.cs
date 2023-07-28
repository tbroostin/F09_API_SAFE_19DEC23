//Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// OfficeRepository class builds Office objects from Colleague database records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidOfficeRepository : BaseColleagueRepository, IFinancialAidOfficeRepository
    {

        /// <summary>
        /// Constructor instantiates OfficeRepository object
        /// </summary>
        /// <param name="cacheProvider">CacheProvider object</param>
        /// <param name="transactionFactory">ColleagueTransactionFactory object</param>
        /// <param name="logger">Logger object</param>
        public FinancialAidOfficeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get a list of all Financial Aid Office objects 
        /// </summary>
        /// <returns>A list of Financial Aid Office objects</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no office records are found in the database.</exception>
        public async Task<IEnumerable<FinancialAidOffice>> GetFinancialAidOfficesAsync()
        {
            try
            {
                return await GetOrAddToCacheAsync<IEnumerable<FinancialAidOffice>>("AllFinancialAidOffices",
                        async() =>
                        {
                            var faLocationRecords = await DataReader.BulkReadRecordAsync<FaLocations>("");
                            var officeRecords = await DataReader.BulkReadRecordAsync<FaOffices>("");
                            var defaultSystemParameters = await GetSystemParametersAsync();
                            var faOfficeParamRecords = await DataReader.BulkReadRecordAsync<FaOfficeParameters>("");
                            var faOfficeSapParamRecords = await DataReader.BulkReadRecordAsync<FaOfficeSapParameters>("");
                            var shoppingSheetParamRecords = await DataReader.BulkReadRecordAsync<FaShopsheetParams>("");

                            if (officeRecords == null || !officeRecords.Any())
                            {
                                var message = "Office records not found in database";
                                logger.Info(message);
                                return new List<FinancialAidOffice>();
                            }

                            if (faOfficeParamRecords == null || !faOfficeParamRecords.Any())
                            {
                                var message = "FA Office parameter records not found in database";
                                logger.Info(message);
                                faOfficeParamRecords = new Collection<FaOfficeParameters>();
                            }

                            if (shoppingSheetParamRecords == null || !shoppingSheetParamRecords.Any())
                            {
                                var message = "Shopping Sheet Parameter records not found in database";
                                logger.Info(message);
                                shoppingSheetParamRecords = new Collection<FaShopsheetParams>();
                            }

                            if (faOfficeSapParamRecords == null || !faOfficeSapParamRecords.Any())
                            {
                                var message = "FA Office SAP Parameter records not found in database";
                                logger.Info(message);
                                faOfficeSapParamRecords = new Collection<FaOfficeSapParameters>();
                            }


                            var officeList = new List<FinancialAidOffice>();
                            foreach (var officeRecord in officeRecords)
                            {
                                try
                                {
                                    var officeAddress = BuildOfficeAddress(officeRecord);

                                    var locationRecords = (faLocationRecords != null) ?
                                        faLocationRecords.Where(loc => loc.FalocFaOffice == officeRecord.Recordkey) :
                                        new List<FaLocations>();

                                    var office = new FinancialAidOffice(officeRecord.Recordkey)
                                    {
                                        Name = officeRecord.FaofcName,
                                        AddressLabel = officeAddress,
                                        DirectorName = officeRecord.FaofcPellFaDirector,
                                        PhoneNumber = officeRecord.FaofcPellPhoneNumber,
                                        EmailAddress = officeRecord.FaofcPellInternetAddress,
                                        LocationIds = locationRecords.Select(l => l.Recordkey).ToList(),
                                        IsDefault = (defaultSystemParameters != null && defaultSystemParameters.FspMainFaOffice == officeRecord.Recordkey),
                                        OpeId = (!string.IsNullOrEmpty(officeRecord.FaofcOpeId)) ? officeRecord.FaofcOpeId : defaultSystemParameters.FspOpeId,
                                        TitleIVCode = (!string.IsNullOrEmpty(officeRecord.FaofcTitleIvCode)) ? officeRecord.FaofcTitleIvCode : defaultSystemParameters.FspTitleIvCode,
                                        DefaultDisplayYearCode = officeRecord.FaofcSsStartDisplayYear
                                    };

                                //extract the office, shopping sheet and SAP parameters specific to this office.
                                var officeParameters = faOfficeParamRecords.Where(p => p.FopFaOfficeCode == office.Id);
                                    var shoppingSheetParameters = shoppingSheetParamRecords.Where(p => p.FsspOpeId == office.OpeId);
                                    var academicProgressParameterRecord = faOfficeSapParamRecords.FirstOrDefault(p => p.FospFaOfficeCode == office.Id);

                                //build a list of award years for which parameter records exist
                                var parameterYears = officeParameters.Select(p => p.FopYear).Concat(shoppingSheetParameters.Select(p => p.FsspFaYear)).Distinct();

                                //for each year, build a configuration object
                                var configurations = parameterYears.Select(year =>
                                            BuildOfficeConfiguration(office.Id, year, officeParameters.FirstOrDefault(p => p.FopYear == year), shoppingSheetParameters.FirstOrDefault(p => p.FsspFaYear == year), defaultSystemParameters)
                                        );

                                    office.AddConfigurationRange(configurations);

                                //build the AcademicProgressConfiguration
                                try
                                    {
                                        office.AcademicProgressConfiguration = BuildAcademicProgressConfiguration(office.Id, academicProgressParameterRecord);
                                    }
                                    catch (Exception e)
                                    {
                                        LogDataError("FaOfficeSapParameters", office.Id, academicProgressParameterRecord, e, "Unable to create Academic Progress Parameters");
                                    }

                                    officeList.Add(office);
                                }
                                catch (Exception e)
                                {
                                    LogDataError("FaOffices", officeRecord.Recordkey, officeRecord, e, "Also check FaOfficeParameters and FaShopsheetParams");
                                }
                            }

                            return officeList;
                        });
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a collection of financial aid offices
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid offices</returns>
        public async Task<IEnumerable<FinancialAidOfficeItem>> GetFinancialAidOfficesAsync(bool ignoreCache = false)
        {
            var defaultSystemParameters = await GetSystemParametersAsync();

            return await GetGuidCodeItemAsync<FaOffices, FinancialAidOfficeItem>("AllFinancialAidOfficeItems", "FA.OFFICES",
                (fa, g) => new FinancialAidOfficeItem(g, fa.Recordkey, fa.Recordkey, fa.FaofcName)
                {
                    addressLines = BuildOfficeAddress2(fa, defaultSystemParameters),
                    city = !string.IsNullOrEmpty(fa.FaofcCity) ? fa.FaofcCity : null,
                    state = !string.IsNullOrEmpty(fa.FaofcState) ? fa.FaofcState : null,
                    postalCode = !string.IsNullOrEmpty(fa.FaofcZip) ? fa.FaofcZip : null,
                    aidAdministrator = !string.IsNullOrEmpty(fa.FaofcPellFaDirector) ? fa.FaofcPellFaDirector : defaultSystemParameters.FspFaDirectorName,
                    phoneNumber = !string.IsNullOrEmpty(fa.FaofcPellPhoneNumber) ? fa.FaofcPellPhoneNumber : defaultSystemParameters.FspPellPhoneNumber,
                    faxNumber = !string.IsNullOrEmpty(fa.FaofcPellFaxNumber) ? fa.FaofcPellFaxNumber : defaultSystemParameters.FspPellFaxNumber,
                    emailAddress = !string.IsNullOrEmpty(fa.FaofcPellInternetAddress) ? fa.FaofcPellInternetAddress : defaultSystemParameters.FspPellInternetAddress,
                }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Helper to build an AcademicProgressConfiguration object. Even if parameterRecord input is null, method
        /// still returns a default configuration object where Academic Progress is turned off.
        /// </summary>
        /// <param name="officeId"></param>
        /// <param name="parameterRecord"></param>
        /// <returns></returns>>
        private AcademicProgressConfiguration BuildAcademicProgressConfiguration(string officeId, FaOfficeSapParameters parameterRecord)
        {
            if (parameterRecord == null)
            {
                return new AcademicProgressConfiguration(officeId)
                {
                    IsSatisfactoryAcademicProgressActive = false,
                    IsSatisfactoryAcademicProgressHistoryActive = false,
                    DetailPropertyConfigurations = new List<AcademicProgressPropertyConfiguration>(),
                    AcademicProgressTypesToDisplay = new List<string>()
                };
            }

            // We want to default a value of 5 whenever the Number of Academic Progress History Records to display is null.

            const int defaultNumberOfHistoryRecordsToDisplay = 5;

            var config = new AcademicProgressConfiguration(officeId)
            {
                IsSatisfactoryAcademicProgressActive = !string.IsNullOrEmpty(parameterRecord.FospSapAvail) ? parameterRecord.FospSapAvail.ToUpper() == "Y" : false,
                IsSatisfactoryAcademicProgressHistoryActive = !string.IsNullOrEmpty(parameterRecord.FospSapHistoryAvail) ? parameterRecord.FospSapHistoryAvail.ToUpper() == "Y" : false,
                NumberOfAcademicProgressHistoryRecordsToDisplay = (parameterRecord.FospNumSapHistToDisp.HasValue) ? parameterRecord.FospNumSapHistToDisp.Value : defaultNumberOfHistoryRecordsToDisplay,
                DetailPropertyConfigurations = new List<AcademicProgressPropertyConfiguration>(),
               
            };

           
            config.AcademicProgressTypesToDisplay = parameterRecord.FospDisplaySapTypes == null ? new List<string>() : parameterRecord.FospDisplaySapTypes;         

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodAttemptedCredits)
            {                
                Description = parameterRecord.FospAttEvalPdExpl,
                Label = parameterRecord.FospAttEvalPdLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospAttEvalPdOpt) || parameterRecord.FospAttEvalPdOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodCompletedCredits)
            {
                Description = parameterRecord.FospCmplEvalPdExpl,
                Label = parameterRecord.FospCmplEvalPdLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospCmplEvalPdOpt) || parameterRecord.FospCmplEvalPdOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodOverallGpa)
            {
                Description = parameterRecord.FospGpaEvalPdExpl,
                Label = parameterRecord.FospGpaEvalPdLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospGpaEvalPdOpt) || parameterRecord.FospGpaEvalPdOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodRateOfCompletion)
            {
                Description = parameterRecord.FospPaceEvalPdExpl,
                Label = parameterRecord.FospPaceEvalPdLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospPaceEvalPdOpt) || parameterRecord.FospPaceEvalPdOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCredits)
            {
                Description = parameterRecord.FospAttWithRemExpl,
                Label = parameterRecord.FospAttWithRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospAttWithRemOpt) || parameterRecord.FospAttWithRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeCompletedCredits)
            {
                Description = parameterRecord.FospCmplWithRemExpl,
                Label = parameterRecord.FospCmplWithRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospCmplWithRemOpt) || parameterRecord.FospCmplWithRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeOverallGpa)
            {
                Description = parameterRecord.FospGpaWithRemExpl,
                Label = parameterRecord.FospGpaWithRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospGpaWithRemOpt) || parameterRecord.FospGpaWithRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeRateOfCompletion)
            {
                Description = parameterRecord.FospPaceWithRemExpl,
                Label = parameterRecord.FospPaceWithRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospPaceWithRemOpt) || parameterRecord.FospPaceWithRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCreditsExcludingRemedial)
            {
                Description = parameterRecord.FospAttWoRemExpl,
                Label = parameterRecord.FospAttWoRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospAttWoRemOpt) || parameterRecord.FospAttWoRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeCompletedCreditsExcludingRemedial)
            {
                Description = parameterRecord.FospCmplWoRemExpl,
                Label = parameterRecord.FospCmplWoRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospCmplWoRemOpt) || parameterRecord.FospCmplWoRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeRateOfCompletionExcludingRemedial)
            {
                Description = parameterRecord.FospPaceWoRemExpl,
                Label = parameterRecord.FospPaceWoRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospPaceWoRemOpt) || parameterRecord.FospPaceWoRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeGpaIncludingRemedial)
            {
                Description = parameterRecord.FospGpaWoRemExpl,
                Label = parameterRecord.FospGpaWoRemLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospGpaWoRemOpt) || parameterRecord.FospGpaWoRemOpt.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.MaximumProgramCredits)
            {
                Description = parameterRecord.FospMaxCredExpl,
                Label = parameterRecord.FospMaxCredLabel,
                IsHidden = string.IsNullOrEmpty(parameterRecord.FospMaxCredOpt) || parameterRecord.FospMaxCredOpt.ToUpper() != "Y"
            });

            return config;

        }

        /// <summary>
        /// Build the Configuration objects for the Office
        /// </summary>
        /// <param name="officeId">The Id of the Office for which to build Configurations</param>
        /// <param name="faOfficeParamRecords">Collection of OfficeParameter records</param>
        /// <param name="shoppingSheetParametersRecord">Collection of ShoppingSheetParameter records</param>
        /// <param name="faSysParams">Collection of FaSysParam records</param>
        /// <returns>A list of Financial Aid Configuration objects specific to the office code passed in.</returns>
        private FinancialAidConfiguration BuildOfficeConfiguration(string officeId, string awardYear, FaOfficeParameters officeParametersRecord, FaShopsheetParams shoppingSheetParametersRecord, FaSysParams faSysParams)
        {

            if (officeParametersRecord == null && shoppingSheetParametersRecord == null)
            {
                return null;
            }

            var singleConfiguration = new FinancialAidConfiguration(officeId, awardYear);

            if (officeParametersRecord != null)
            {
                singleConfiguration.AwardYearDescription = officeParametersRecord.FopYearDescription;

                try
                {
                    singleConfiguration.UndergraduatePackage = new AverageAwardPackage(officeParametersRecord.FopUgAvgGrantAmt, officeParametersRecord.FopUgAvgLoanAmt, officeParametersRecord.FopUgAvgScholarshipAmt, officeParametersRecord.FopYear);
                }

                catch (Exception e)
                {
                    singleConfiguration.UndergraduatePackage = null;
                    logger.Info(e, "Error creating undergraduate average award package for {0} award year", officeParametersRecord.FopYear);
                }

                try
                {
                    singleConfiguration.GraduatePackage = new AverageAwardPackage(officeParametersRecord.FopGrAvgGrantAmt, officeParametersRecord.FopGrAvgLoanAmt, officeParametersRecord.FopGrAvgScholarshipAmt, officeParametersRecord.FopYear);
                }
                catch (Exception e)
                {
                    singleConfiguration.GraduatePackage = null;
                    logger.Info(e, "Error creating graduate average award package for {0} award year", officeParametersRecord.FopYear);
                }

                singleConfiguration.IsSelfServiceActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSelfServiceAvail) && officeParametersRecord.FopSelfServiceAvail.ToUpper() == "Y");

                singleConfiguration.IsAwardingActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAwardingAvail) && officeParametersRecord.FopAwardingAvail.ToUpper() == "Y");

                singleConfiguration.AreAwardChangesAllowed =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAwardChangesAvail) && officeParametersRecord.FopAwardChangesAvail.ToUpper() == "Y");

                singleConfiguration.AllowAnnualAwardUpdatesOnly =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAnnualAccrejOnly) && officeParametersRecord.FopAnnualAccrejOnly.ToUpper() == "Y");

                singleConfiguration.IsAwardLetterActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAwardLetterAvail) && officeParametersRecord.FopAwardLetterAvail.ToUpper() == "Y");

                singleConfiguration.IsProfileActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopProfileAvail) && officeParametersRecord.FopProfileAvail.ToUpper() == "Y");

                singleConfiguration.IsShoppingSheetActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopShoppingSheetAvail) && officeParametersRecord.FopShoppingSheetAvail.ToUpper() == "Y");

                singleConfiguration.AreLoanRequestsAllowed =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopLoanRequestsAvail) && officeParametersRecord.FopLoanRequestsAvail.ToUpper() == "Y");

                singleConfiguration.IsAwardLetterHistoryActive =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAwdLtrHistAvail) && officeParametersRecord.FopAwdLtrHistAvail.ToUpper() == "Y");

                singleConfiguration.FaBlankStatusText = officeParametersRecord.FopBlankStatusText;
                singleConfiguration.FaBlankDueDateText = officeParametersRecord.FopBlankDueDateText;

                //Adding FA Credits Page Parameters
                singleConfiguration.SuppressCourseCredits = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressCourseCredits) && officeParametersRecord.FopSuppressCourseCredits.ToUpper() == "Y");
                singleConfiguration.SuppressInstCredits = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressInstCredits) && officeParametersRecord.FopSuppressInstCredits.ToUpper() == "Y");
                singleConfiguration.SuppressTivCredits = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressTivCredits) && officeParametersRecord.FopSuppressTivCredits.ToUpper() == "Y");
                singleConfiguration.SuppressPellCredits = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressPellCredits) && officeParametersRecord.FopSuppressPellCredits.ToUpper() == "Y");
                singleConfiguration.SuppressDlCredits = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressDlCredits) && officeParametersRecord.FopSuppressDlCredits.ToUpper() == "Y");

                singleConfiguration.CourseCreditsExplanation = officeParametersRecord.FopCourseCredExplanation;
                singleConfiguration.InstCreditsExplanation = officeParametersRecord.FopInstCredExplanation;
                singleConfiguration.TivCreditsExplanation = officeParametersRecord.FopTivCredExplanation;
                singleConfiguration.PellCreditsExplanation = officeParametersRecord.FopPellCredExplanation;
                singleConfiguration.DlCreditsExplanation = officeParametersRecord.FopDlCredExplanation;

                singleConfiguration.FaCreditsVisibilityRule = officeParametersRecord.FopCreditsDisplayRule;
                singleConfiguration.SuppressProgramDisplay = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressProgram) && officeParametersRecord.FopSuppressProgram.ToUpper() == "Y");
                singleConfiguration.SuppressDegreeAudit = (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressDegreeAudit) && officeParametersRecord.FopSuppressDegreeAudit.ToUpper() == "Y");
                singleConfiguration.DegreeAuditExplanation = officeParametersRecord.FopDegreeAuditExplanation;
                singleConfiguration.EnrolledCreditsPageExplanation = officeParametersRecord.FopEnrolledCreditsExpl;
                //Done adding FA Credits Page Parameters

                singleConfiguration.ExcludeAwardStatusCategoriesView = TranslateCodeToAwardStatusCategory(officeParametersRecord.FopExclActCatFromView).ToList();

                singleConfiguration.ExcludeAwardCategoriesView = officeParametersRecord.FopExclAwdCatFromView == null ? new List<string>() : officeParametersRecord.FopExclAwdCatFromView;
                singleConfiguration.ExcludeAwardPeriods = officeParametersRecord.FopExclAwdPdsFromView == null ? new List<string>() : officeParametersRecord.FopExclAwdPdsFromView;
                singleConfiguration.ExcludeAwardsView = officeParametersRecord.FopExclAwardsFromView == null ? new List<string>() : officeParametersRecord.FopExclAwardsFromView;

                singleConfiguration.ExcludeAwardStatusCategoriesFromChange = TranslateCodeToAwardStatusCategory(officeParametersRecord.FopExclActCatFromChg).ToList();
                singleConfiguration.ExcludeAwardCategoriesFromChange = officeParametersRecord.FopExclAwdCatFromChg == null ? new List<string>() : officeParametersRecord.FopExclAwdCatFromChg;
                singleConfiguration.ExcludeAwardsFromChange = officeParametersRecord.FopExclAwardsFromChg == null ? new List<string>() : officeParametersRecord.FopExclAwardsFromChg;
                singleConfiguration.ExcludeAwardStatusesFromChange = officeParametersRecord.FopExclActStFromChg == null ? new List<string>() : officeParametersRecord.FopExclActStFromChg;

                //Assign checklist item codes, their control statuses, and default flags
                singleConfiguration.ChecklistItemCodes = officeParametersRecord.FopChecklistItems == null ? new List<string>() : officeParametersRecord.FopChecklistItems;
                singleConfiguration.ChecklistItemControlStatuses = officeParametersRecord.FopChecklistDisplayAction == null ? new List<string>() : officeParametersRecord.FopChecklistDisplayAction;
                singleConfiguration.ChecklistItemDefaultFlags = officeParametersRecord.FopChecklistAssignByDflt == null ? new List<string>() : officeParametersRecord.FopChecklistAssignByDflt;

                // Award Letter Parameters
                singleConfiguration.IgnoreAwardStatusesFromEval = officeParametersRecord.FopIgnoreActStFromEval == null ? new List<string>() : officeParametersRecord.FopIgnoreActStFromEval;
                singleConfiguration.IgnoreAwardCategoriesFromEval = officeParametersRecord.FopIgnoreAwdCatFromEval == null ? new List<string>() : officeParametersRecord.FopIgnoreAwdCatFromEval;
                singleConfiguration.IgnoreAwardsFromEval = officeParametersRecord.FopIgnoreAwardsFromEval == null ? new List<string>() : officeParametersRecord.FopIgnoreAwardsFromEval;

                //Award package checlist parameters
                singleConfiguration.IgnoreAwardCategoriesOnChecklist = officeParametersRecord.FopIgnoreAwdCatOnChklst == null ? new List<string>() : officeParametersRecord.FopIgnoreAwdCatOnChklst;
                singleConfiguration.IgnoreAwardsOnChecklist = officeParametersRecord.FopIgnoreAwardsOnChklst == null ? new List<string>() : officeParametersRecord.FopIgnoreAwardsOnChklst;
                singleConfiguration.IgnoreAwardStatusesOnChecklist = officeParametersRecord.FopIgnoreActStOnChklst == null ? new List<string>() : officeParametersRecord.FopIgnoreActStOnChklst;

                // Accept/Reject Parameters
                singleConfiguration.AcceptedAwardStatusCode = officeParametersRecord.FopAccAwdsActSt;
                singleConfiguration.AcceptedAwardCommunicationCode = officeParametersRecord.FopAccAwdsCcCode;
                singleConfiguration.AcceptedAwardCommunicationStatus = officeParametersRecord.FopAccAwdsCcSt;

                singleConfiguration.RejectedAwardStatusCode = officeParametersRecord.FopRejAwdsActSt;
                singleConfiguration.RejectedAwardCommunicationCode = officeParametersRecord.FopRejAwdsCcCode;
                singleConfiguration.RejectedAwardCommunicationStatus = officeParametersRecord.FopRejAwdsCcSt;

                try
                {
                    var allHousingOptions = GetHousingOptions();
                    singleConfiguration.HousingOptions = (allHousingOptions.ValInternalCode.Except(officeParametersRecord.FopExclHousingOptions)).Select(ho => ho.ToString()).ToList();
                }
                catch
                {//do nothing
                }
                
                singleConfiguration.AllowNegativeUnmetNeedBorrowing =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopNegUnmetNeed) && officeParametersRecord.FopNegUnmetNeed.ToUpper() == "Y");

                singleConfiguration.AllowLoanChanges =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopLoanAmtChanges) && officeParametersRecord.FopLoanAmtChanges.ToUpper() == "Y");

                if (singleConfiguration.AllowLoanChanges)
                {
                    singleConfiguration.AllowLoanDecreaseOnly =
                        (!string.IsNullOrEmpty(officeParametersRecord.FopDecrLoanAmtsOnly) && officeParametersRecord.FopDecrLoanAmtsOnly.ToUpper() == "Y");
                }
                else { singleConfiguration.AllowLoanDecreaseOnly = false; }

                singleConfiguration.AllowLoanChangeIfAccepted =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopChangeAcceptedLoans) && officeParametersRecord.FopChangeAcceptedLoans.ToUpper() == "Y");

                singleConfiguration.AllowDeclineZeroOfAcceptedLoans =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopDeclineZeroAccLoans) && officeParametersRecord.FopDeclineZeroAccLoans.ToUpper() == "Y");

                singleConfiguration.SuppressInstanceData =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressInstanceData) && officeParametersRecord.FopSuppressInstanceData.ToUpper() == "Y");

                singleConfiguration.NewLoanCommunicationCode = officeParametersRecord.FopNewLoanCcCode;
                singleConfiguration.NewLoanCommunicationStatus = officeParametersRecord.FopNewLoanCcStatus;
                singleConfiguration.LoanChangeCommunicationCode = officeParametersRecord.FopChgLoanCcCode;
                singleConfiguration.LoanChangeCommunicationStatus = officeParametersRecord.FopChgLoanCcStatus;

                singleConfiguration.IsLoanAmountChangeRequestRequired =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopReviewLoanChanges) && officeParametersRecord.FopReviewLoanChanges.ToUpper() == "Y");

                singleConfiguration.IsDeclinedStatusChangeRequestRequired =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopReviewDeclinedAwards) && officeParametersRecord.FopReviewDeclinedAwards.ToUpper() == "Y");

                singleConfiguration.PaperCopyOptionText = officeParametersRecord.FopPaperCopyOptionDesc;

                singleConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = TranslateCodeToAwardStatusCategory(officeParametersRecord.FopExclActCatFromAwdltr).ToList();
                singleConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = officeParametersRecord.FopExclAwardsFromAwdltr == null ? new List<string>()
                    : officeParametersRecord.FopExclAwardsFromAwdltr;
                singleConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = officeParametersRecord.FopExclAwdPdsFromAwdltr == null ? new List<string>()
                    : officeParametersRecord.FopExclAwdPdsFromAwdltr;
                singleConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = officeParametersRecord.FopExclAwdCatFromAwdltr == null ? new List<string>()
                    : officeParametersRecord.FopExclAwdCatFromAwdltr;

                singleConfiguration.ShowBudgetDetailsOnAwardLetter = (!string.IsNullOrEmpty(officeParametersRecord.FopShowBudgetDetails)
                    && officeParametersRecord.FopShowBudgetDetails.ToUpper() == "Y");
                singleConfiguration.StudentAwardLetterBudgetDetailsDescription = officeParametersRecord.FopBudgetDtlDesc;

                singleConfiguration.CounselorPhoneType = officeParametersRecord.FopCounselorPhoneType;

                singleConfiguration.CreateChecklistItemsForNewStudent =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopChecklistNoFinAid) && officeParametersRecord.FopChecklistNoFinAid.ToUpper() == "Y");

                singleConfiguration.AnnualAcceptRejectOnlyFlag =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopAnnualAccrejOnly) && officeParametersRecord.FopAnnualAccrejOnly.ToUpper() == "Y");

                singleConfiguration.UseDefaultContact =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopUseDefaultContact) && officeParametersRecord.FopUseDefaultContact.ToUpper() == "Y");
                singleConfiguration.SuppressMaximumLoanLimits =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSupressLoanLimit) && officeParametersRecord.FopSupressLoanLimit.ToUpper() == "Y");
                singleConfiguration.UseDocumentStatusDescription =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopUseMailingCodeDesc) && officeParametersRecord.FopUseMailingCodeDesc.ToUpper() == "Y");

                singleConfiguration.DisplayPellLifetimeEarningsUsed =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopDisplayPellLeu) && officeParametersRecord.FopDisplayPellLeu.ToUpper() == "Y");

                singleConfiguration.SuppressAccountSummaryDisplay =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressActSumDisplay) && officeParametersRecord.FopSuppressActSumDisplay.ToUpper() == "Y");
                singleConfiguration.SuppressAverageAwardPackageDisplay =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressAvgPkgDisplay) && officeParametersRecord.FopSuppressAvgPkgDisplay.ToUpper() == "Y");
                singleConfiguration.SuppressAwardLetterAcceptance =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressAwdLtrAccept) && officeParametersRecord.FopSuppressAwdLtrAccept.ToUpper() == "Y");
                singleConfiguration.SuppressDisbursementInfoDisplay =
                    (!string.IsNullOrEmpty(officeParametersRecord.FopSuppressDisbInfoDispl) && officeParametersRecord.FopSuppressDisbInfoDispl.ToUpper() == "Y");

                //Checklist item configuration
                singleConfiguration.ShowParentLoanInfo = (!string.IsNullOrEmpty(officeParametersRecord.FopShowChkParLoanInfo) && officeParametersRecord.FopShowChkParLoanInfo.ToUpper() == "Y");
                singleConfiguration.ShowPlusApplicationInfo = (!string.IsNullOrEmpty(officeParametersRecord.FopShowChkPlusAppInfo) && officeParametersRecord.FopShowChkPlusAppInfo.ToUpper() == "Y");
                singleConfiguration.ShowStudentLoanInfo = (!string.IsNullOrEmpty(officeParametersRecord.FopShowChkStuLoanInfo) && officeParametersRecord.FopShowChkStuLoanInfo.ToUpper() == "Y");
                singleConfiguration.ShowAslaInfo = (!string.IsNullOrEmpty(officeParametersRecord.FopShowChkAslaInfo) && officeParametersRecord.FopShowChkAslaInfo.ToUpper() == "Y");
            }

            //shopping sheet
            if (shoppingSheetParametersRecord != null)
            {
                var officeType = ShoppingSheetOfficeType.BachelorDegreeGranting;
                if (!string.IsNullOrEmpty(shoppingSheetParametersRecord.FsspInstitutionType))
                {
                    switch (shoppingSheetParametersRecord.FsspInstitutionType.ToUpper())
                    {
                        case "1":
                            officeType = ShoppingSheetOfficeType.BachelorDegreeGranting;
                            break;
                        case "2":
                            officeType = ShoppingSheetOfficeType.AssociateDegreeGranting;
                            break;
                        case "3":
                            officeType = ShoppingSheetOfficeType.CertificateGranting;
                            break;
                        case "4":
                            officeType = ShoppingSheetOfficeType.GraduateDegreeGranting;
                            break;
                        case "5":
                            officeType = ShoppingSheetOfficeType.NonDegreeGranting;
                            break;
                    }
                }

                var efcOption = ShoppingSheetEfcOption.IsirEfc;
                var useProfileImEfc =
                    (!string.IsNullOrEmpty(shoppingSheetParametersRecord.FsspEfcOption) && shoppingSheetParametersRecord.FsspEfcOption.ToUpper() == "Y");
                var useProfileImEfcUntilIsirExists =
                    (!string.IsNullOrEmpty(shoppingSheetParametersRecord.FsspEfcOptionExt) && shoppingSheetParametersRecord.FsspEfcOptionExt.ToUpper() == "Y");
                if (singleConfiguration.IsProfileActive && useProfileImEfc)
                {
                    if (useProfileImEfcUntilIsirExists)
                    {
                        efcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                    }
                    else
                    {
                        efcOption = ShoppingSheetEfcOption.ProfileEfc;
                    }
                }

                singleConfiguration.ShoppingSheetConfiguration = new ShoppingSheetConfiguration()
                {
                    CustomMessageRuleTableId = shoppingSheetParametersRecord.FsspCustomMessageRtId,
                    GraduationRate = shoppingSheetParametersRecord.FsspGraduationRate,
                    LoanDefaultRate = shoppingSheetParametersRecord.FsspInstLoanDefaultRate,
                    NationalLoanDefaultRate = shoppingSheetParametersRecord.FsspUsLoanDefaultRate,
                    MedianBorrowingAmount = shoppingSheetParametersRecord.FsspMedianBorrowAmount,
                    MedianMonthlyPaymentAmount = shoppingSheetParametersRecord.FsspMedianPayment,
                    OfficeType = officeType,
                    EfcOption = efcOption,
                    LowToMediumBoundary = shoppingSheetParametersRecord.FsspGradRateLowToMed,
                    MediumToHighBoundary = shoppingSheetParametersRecord.FsspGradRateMedToHigh,
                    InstitutionRepaymentRate = shoppingSheetParametersRecord.FsspInstRepaymentRate,
                    NationalRepaymentRateAverage = shoppingSheetParametersRecord.FsspNatRepaymentRateAvg,
                    TuitionAndFees = shoppingSheetParametersRecord.FsspTuitionAndFees,
                    HousingAndMealsOn = shoppingSheetParametersRecord.FsspHmOnCampus,
                    HousingAndMealsOff = shoppingSheetParametersRecord.FsspHmOffCampus,
                    BooksAndSupplies = shoppingSheetParametersRecord.FsspBooksAndSupplies,
                    Transportation = shoppingSheetParametersRecord.FsspTransportation,
                    OtherEducationCosts = shoppingSheetParametersRecord.FsspOtherEducationCosts,
                    SchoolScholarships = shoppingSheetParametersRecord.FsspSchoolSchol,
                    StateScholarships = shoppingSheetParametersRecord.FsspStateSchol,
                    OtherScholarships = shoppingSheetParametersRecord.FsspOtherSchol,
                    PellGrants = shoppingSheetParametersRecord.FsspPellGrants,
                    SchoolGrants = shoppingSheetParametersRecord.FsspSchoolGrants,
                    StateGrants = shoppingSheetParametersRecord.FsspStateGrants,
                    OtherGrants = shoppingSheetParametersRecord.FsspOtherGrants,
                    DlSubLoans = shoppingSheetParametersRecord.FsspDlSubLoans,
                    DlUnsubLoans = shoppingSheetParametersRecord.FsspDlUnsubLoans,
                    PrivateLoans = shoppingSheetParametersRecord.FsspPrivateLoans,
                    SchoolLoans = shoppingSheetParametersRecord.FsspSchoolLoans,
                    OtherLoans = shoppingSheetParametersRecord.FsspOtherLoans,
                    ParentPlusLoans = shoppingSheetParametersRecord.FsspParentPlusLoans,
                    WorkStudy = shoppingSheetParametersRecord.FsspWorkStudy,
                    OtherJobs = shoppingSheetParametersRecord.FsspOtherJobs,
                    LoanAmountTextRuleId = shoppingSheetParametersRecord.FsspLoanAmtTextRuleId,
                    EducationBenTextRuleId = shoppingSheetParametersRecord.FsspEducBenTextRuleId,
                    NextStepsRuleId = shoppingSheetParametersRecord.FsspNextStepsRuleId,
                    EmployeeTuitionBenefits = shoppingSheetParametersRecord.FsspEmplPdTuitBen,
                    DisadvantagedStudentScholarship = shoppingSheetParametersRecord.FsspScholDisStu,
                    GraduateUndergraduateRuleId = shoppingSheetParametersRecord.FsspGrUgRuleId,
                    SubInterestRate = shoppingSheetParametersRecord.FsspSubInterestRate,
                    SubOriginationFee = shoppingSheetParametersRecord.FsspSubOrigFee,
                    UnsubInterestRate = shoppingSheetParametersRecord.FsspUnsubInterestRate,
                    UnsubOriginationFee = shoppingSheetParametersRecord.FsspUnsubOrigFee,
                    GradUnsubInterestRate = shoppingSheetParametersRecord.FsspUnsubGrIntRt,
                    GradUnsubOriginationFee = shoppingSheetParametersRecord.FsspUnsubGrOrigFee,
                    PrivateInterestRate = shoppingSheetParametersRecord.FsspPrivInterestRate,
                    PrivateOriginationFee = shoppingSheetParametersRecord.FsspPrivOrigFee,
                    InstitutionInterestRate = shoppingSheetParametersRecord.FsspInstInterestRate,
                    InstitutionOriginationFee = shoppingSheetParametersRecord.FsspInstOrigFee,
                    GradPlusInterestRate = shoppingSheetParametersRecord.FsspGplusInterestRate,
                    GradPlusOriginationFee = shoppingSheetParametersRecord.FsspGplusOrigFee,
                    HRSAInterestRate = shoppingSheetParametersRecord.FsspHrsaInterestRate,
                    HRSAOriginationFee = shoppingSheetParametersRecord.FsspHrsaOrigFee,
                    GradPlusLoans = shoppingSheetParametersRecord.FsspGplusLoans,
                    HrsaLoans = shoppingSheetParametersRecord.FsspHrsaLoans,
                    PlusInterestRate = shoppingSheetParametersRecord.FsspPlusInterestRate,
                    PlusOriginationFee = shoppingSheetParametersRecord.FsspPlusOrigFee,
                    SchoolPaidTuitionBenefits = shoppingSheetParametersRecord.FsspSchoolPdTuitBen,
                    TuitionRemWaiver = shoppingSheetParametersRecord.FsspTuitionRemWaiver,
                    Assistantships = shoppingSheetParametersRecord.FsspAssistantships,
                    IncomeShare = shoppingSheetParametersRecord.FsspIncomeShareAgreements,
                    VetsAwards = shoppingSheetParametersRecord.FsspVeteransBenefits,
                    UseVetsData = shoppingSheetParametersRecord.FsspUseVetsData
                };
            }

            if (faSysParams != null)
            {
                var faAlertText = GetMiscText(faSysParams.FspNotificationText);
                if (faAlertText != null)
                {
                    singleConfiguration.FspNotificationText = faAlertText;
                }
            }


            return singleConfiguration;
        }

        /// <summary>
        /// Get and Cache the FaSysParams record
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private FaSysParams GetSystemParameters()
        {
            return GetOrAddToCache<FaSysParams>("FinancialAidSystemParameters",
                            () =>
                            {
                                var sysParams = DataReader.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                                if (sysParams == null)
                                {
                                    logger.Info("Unable to read FA.SYS.PARAMS from database");
                                    sysParams = new FaSysParams();
                                }
                                return sysParams;
                            }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get and Cache the FaSysParams record asynchronously
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private async Task<FaSysParams> GetSystemParametersAsync()
        {
            try
            {
                return await GetOrAddToCacheAsync<FaSysParams>("FinancialAidSystemParameters",
                                async () =>
                                {
                                    return await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                                }, Level1CacheTimeoutValue);
            }
            catch (Exception)
            {
                var errorMessage = "Unable to access FA.SYS.PARAMS in ST.PARMS.";
                logger.Info(errorMessage);
                throw new ColleagueWebApiException(errorMessage);
            }
        }

        private List<string> BuildOfficeAddress(FaOffices officeRecord)
        {
            var officeAddress = new List<string>();
            if (officeRecord.FaofcAddress != null &&
                officeRecord.FaofcAddress.Any(a => !string.IsNullOrEmpty(a)) &&
                !string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                !string.IsNullOrEmpty(officeRecord.FaofcState) &&
                !string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                officeAddress.AddRange(officeRecord.FaofcAddress);

                var csz = string.Format("{0}, {1} {2}", officeRecord.FaofcCity, officeRecord.FaofcState, officeRecord.FaofcZip);
                officeAddress.Add(csz);
            }
            return officeAddress;
        }

        private List<string> BuildOfficeAddress2(FaOffices officeRecord, FaSysParams defaultSystemParameters)
        {
            var officeAddress = new List<string>();
            if (officeRecord.FaofcAddress != null &&
                officeRecord.FaofcAddress.Any(a => !string.IsNullOrEmpty(a)) &&
                !string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                !string.IsNullOrEmpty(officeRecord.FaofcState) &&
                !string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                officeAddress.AddRange(officeRecord.FaofcAddress);

                var csz = string.Format("{0}, {1} {2}", officeRecord.FaofcCity, officeRecord.FaofcState, officeRecord.FaofcZip);
                officeAddress.Add(csz);
            }
            else if (officeRecord.FaofcAddress == null || officeRecord.FaofcAddress.Any(a => string.IsNullOrEmpty(a)) &&
                !string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                !string.IsNullOrEmpty(officeRecord.FaofcState) &&
                !string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                var csz = string.Format("{0}, {1} {2}", officeRecord.FaofcCity, officeRecord.FaofcState, officeRecord.FaofcZip);
                officeAddress.Add(csz);
            }
            else if (officeRecord.FaofcAddress != null && officeRecord.FaofcAddress.Any() &&
                string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                string.IsNullOrEmpty(officeRecord.FaofcState) &&
                string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                officeRecord.FaofcAddress.ForEach(a=>{ if(!string.IsNullOrEmpty(a)) {officeAddress.Add(a);} });
                //officeAddress.AddRange(officeRecord.FaofcAddress);
            }
            else
            {
                officeAddress.AddRange(defaultSystemParameters.FspInstitutionAddress);
                officeAddress.Add(defaultSystemParameters.FspInstitutionCsz);
            }
            return officeAddress;
        }

        private IEnumerable<AwardStatusCategory> TranslateCodeToAwardStatusCategory(IEnumerable<string> codes)
        {
            var awardStatusCategories = new List<AwardStatusCategory>();
            if (codes == null) return awardStatusCategories;

            foreach (var code in codes)
            {
                var category = TranslateCodeToAwardStatusCategory(code);
                if (category.HasValue)
                    awardStatusCategories.Add(category.Value);
            }

            return awardStatusCategories;
        }

        private AwardStatusCategory? TranslateCodeToAwardStatusCategory(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            switch (code.ToUpper())
            {
                case "A":
                    return AwardStatusCategory.Accepted;
                case "P":
                    return AwardStatusCategory.Pending;
                case "E":
                    return AwardStatusCategory.Estimated;
                case "R":
                    return AwardStatusCategory.Rejected;
                case "D":
                    return AwardStatusCategory.Denied;
                default:
                    return null;
            }
        }

        private string GetMiscText(string miscTextId)
        {
            string text = String.Empty;
            if (!string.IsNullOrEmpty(miscTextId))
            {
                Base.DataContracts.MiscText miscText = DataReader.ReadRecord<Base.DataContracts.MiscText>("MISC.TEXT", miscTextId);
                if (miscText != null)
                {
                    text = miscText.MtxtText.Replace(DmiString._VM, ' ');
                }
            }
            return text;
        }

        /// <summary>
        /// Helper method to get housing options from valcode table
        /// </summary>
        /// <returns>valcode values for housing options</returns>
        private ApplValcodes GetHousingOptions()
        {
            return GetOrAddToCache<ApplValcodes>("HousingOptions",
                () =>
                {
                    var housingTable = DataReader.ReadRecord<ApplValcodes>("ST.VALCODES", "FAFSA.HOUSING.CODES09");
                    if (housingTable == null)
                    {
                        var message = "Unable to get ST->FAFSA.HOUSING.CODES09 valcode table";
                        logger.Error(message);
                        throw new ColleagueWebApiException(message);
                    }
                    return housingTable;
                }, Level1CacheTimeoutValue);
        }
    }
}
