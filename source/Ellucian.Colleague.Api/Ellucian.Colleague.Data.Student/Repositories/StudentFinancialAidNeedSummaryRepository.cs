// Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IStudentFinancialAidNeedSummaryRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentFinancialAidNeedSummaryRepository : BaseColleagueRepository, IStudentFinancialAidNeedSummaryRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        /// <summary>
        /// Constructor to instantiate a student financial aid need summary repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public StudentFinancialAidNeedSummaryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get the GUID for a CS Acyr using its id and year
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetIsirCalcResultsGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("ISIR.CALC.RESULTS", id);
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("IsirCalcResults.Guid.NotFound", "ISIR.CALC.RESULTS guid not found for ISIR.FAFSA id " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get Student Financial Aid Need Sumarries.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="faSuiteYears">List of all financial aid years</param>
        /// <returns>A list of Student Need Summaries in domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentNeedSummary>, int>> GetAsync(int offset, int limit, bool bypassCache, List<string> faSuiteYears)
        {
            var studentNeedSummaryEntities = new List<StudentNeedSummary>();
            int totalCount = 0;
            //var csAcyrIDs = new List<string>();
            var studentNeedSummaryIds = new List<string>();

            // Check CS.ACYR from 2006 and forward.
            for (int year = 2006; year <= DateTime.Today.Year; year += 1)
            {
                string stringYear = year.ToString();
                if (faSuiteYears.Contains(stringYear))
                {
                    string[] csAcyrIds = await DataReader.SelectAsync("CS." + year, "WITH (CS.NEED NE '' AND CS.FED.ISIR.ID NE '') OR WITH (CS.INST.NEED NE '' AND CS.INST.ISIR.ID NE '')");
                    totalCount += csAcyrIds.Count();
                    Array.Sort(csAcyrIds);
                    foreach (var id in csAcyrIds)
                    {
                        studentNeedSummaryIds.Add(string.Concat(year, '.', id));
                    }
                }
            }

            var subItems = studentNeedSummaryIds.Skip(offset).Take(limit).ToArray();
            List<string> years = subItems.GroupBy(s => s.Split('.')[0])
                           .Select(g => g.First().Split('.')[0]).Distinct()
                           .ToList();

            foreach (var year in years)
            {
                var csAcyrFile = string.Concat("CS.", year);
                var subList = subItems.Where(s => s.Split('.')[0] == year)
                           .Select(g => g.Split('.')[1]).ToArray();

                // Bulk read the CS.ACYRs
                var csAcyrDataContracts = await DataReader.BulkReadRecordAsync<CsAcyr>(csAcyrFile, subList);

                //Bulk read the ISIR.FAFSAs                                             
                var federalApplications = csAcyrDataContracts.Select(x => x.CsFedIsirId).Distinct().ToList();
                var institutionalApplications = csAcyrDataContracts.Select(x => x.CsInstIsirId).Distinct().ToList();
                var applications = federalApplications.Union(institutionalApplications);                
                var isirFafsaContracts = new List<IsirFafsa>();
                if (applications != null)
                {
                    var bulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>("ISIR.FAFSA", applications.ToArray());
                    if (bulkRecords != null)
                    {
                        isirFafsaContracts.AddRange(bulkRecords);
                    }
                }

                // Bulk read SA.ACYRs
                var saAcyrFile = string.Concat("SA.", year);
                var saAcyrDataContracts = await DataReader.BulkReadRecordAsync<SaAcyr>(saAcyrFile, subList);

                // Bulk read TA.ACYRs and their AWARDS
                var taAcyrIds = new List<string>();
                foreach (var dataContract in saAcyrDataContracts)
                {
                    if (dataContract.SaAward != null)
                    {
                        if (dataContract.SaTerms != null)
                        {
                            foreach (var award in dataContract.SaAward)
                            {
                                foreach (var term in dataContract.SaTerms)
                                {
                                    var taAcyrId = dataContract.Recordkey + "*" + award + "*" + term;
                                    taAcyrIds.Add(taAcyrId);
                                }
                            }
                        }
                    }
                }
                var taAcyrContracts = new List<TaAcyr>();
                var awardIds = new List<string>();
                var awardContracts = new List<Awards>();
                var taAcyrFile = string.Concat("TA.", year);
                if (taAcyrIds != null)
                {
                    var bulkRecords = await DataReader.BulkReadRecordAsync<TaAcyr>(taAcyrFile, taAcyrIds.ToArray());
                    if (bulkRecords != null)
                    {
                        taAcyrContracts.AddRange(bulkRecords);
                        awardIds = taAcyrContracts.Select(x => x.Recordkey.Split('*')[1]).Distinct().ToList();
                        var bulkAwardRecords = await DataReader.BulkReadRecordAsync<Awards>("AWARDS", awardIds.ToArray());
                        if (bulkAwardRecords != null)
                        {
                            awardContracts.AddRange(bulkAwardRecords);
                        }
                    }
                }
                
                foreach (var dataContract in csAcyrDataContracts)
                {
                    var studentId = dataContract.Recordkey;
                    var id = dataContract.RecordGuid;
                    var studentTaAcyrContracts = new List<TaAcyr>();
                    foreach (var taAcyrContract in taAcyrContracts)
                    {
                        if (taAcyrContract.Recordkey.Split('*')[0] == studentId)
                        {
                            studentTaAcyrContracts.Add(taAcyrContract);
                        }
                    }
                    
                    var notAwardedCategories = await GetNotAwardedCategoriesAsync();
                    
                    var studentNeedSummary = BuildStudentNeedSummary(id, studentId, year, 
                        dataContract,
                        isirFafsaContracts,
                        studentTaAcyrContracts,
                        awardContracts,
                        notAwardedCategories);                    

                    studentNeedSummaryEntities.Add(studentNeedSummary);
                }
            }
            return new Tuple<IEnumerable<StudentNeedSummary>, int>(studentNeedSummaryEntities, totalCount);
        }

        /// <summary>
        /// Get the Student Financial Aid Need Summary requested
        /// </summary>
        /// <param name="id">StudentFinancialAidBeedSummaries GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<StudentNeedSummary> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the CS.ACYR record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || string.IsNullOrEmpty(recordInfo.Entity) || recordInfo.Entity.Substring(0, 3) != "CS.")
            {
                throw new KeyNotFoundException(string.Format("No LDM.GUID record found for CS.ACYR record with guid {0}'. ", id));
            }
            var year = recordInfo.Entity.Substring(3, 4);
            int thisYear = Convert.ToInt32(year);
            if (thisYear < 2006)
            {
                var errorMessage = string.Format("Guid '{0}' is for invalid year {1}.  Only years 2006 and later are valid'. ", id, year);
                throw new ArgumentException(errorMessage);
            }
            var csAcyrFile = string.Concat("CS.", year);
            var csAcyrDataContract = await DataReader.ReadRecordAsync<CsAcyr>(csAcyrFile, recordInfo.PrimaryKey);
            {
                if (csAcyrDataContract == null)
                {
                    throw new KeyNotFoundException(string.Format("No CS.ACYR record found for guid {0}'. ", id));
                }
                var foundNeedAndApplication = false;
                if (csAcyrDataContract.CsNeed.HasValue && (!string.IsNullOrEmpty(csAcyrDataContract.CsFedIsirId))) 
                {
                    foundNeedAndApplication = true;
                }
                if (csAcyrDataContract.CsInstNeed.HasValue && (!string.IsNullOrEmpty(csAcyrDataContract.CsInstIsirId)))
                {
                    foundNeedAndApplication = true;
                }
                if (foundNeedAndApplication != true)
                {
                    var errorMessage = string.Format("CS.ACYR for guid '{0}' is not valid.  Must have federal need and application or institutional need and application.", id);
                    throw new ArgumentException(errorMessage);
                }
            }

            // Read SA.ACYR
            var saAcyrFile = string.Concat("SA.", year);
            var saAcyrContract = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, recordInfo.PrimaryKey);

            // Read TA.ACYR records and their AWARDS records
            var taAcyrIds = new List<string>();
            if (saAcyrContract.SaAward.Any())
            {
                if (saAcyrContract.SaTerms != null)
                {
                    foreach (var award in saAcyrContract.SaAward)
                    {
                        foreach (var term in saAcyrContract.SaTerms)
                        {
                            var taAcyrId = saAcyrContract.Recordkey + "*" + award + "*" + term;
                            taAcyrIds.Add(taAcyrId);
                        }
                    }
                }
            }
            var taAcyrContracts = new List<TaAcyr>();
            var awardContracts = new List<Awards>();
            var awardIds = new List<string>();
            var taAcyrFile = string.Concat("TA.", year);
            if (taAcyrIds != null)
            {
                var bulkRecords = await DataReader.BulkReadRecordAsync<TaAcyr>(taAcyrFile, taAcyrIds.ToArray());
                if (bulkRecords != null)
                {
                    taAcyrContracts.AddRange(bulkRecords);
                    awardIds = taAcyrContracts.Select(x => x.Recordkey.Split('*')[1]).Distinct().ToList();
                    var bulkAwardRecords = await DataReader.BulkReadRecordAsync<Awards>("AWARDS", awardIds.ToArray());
                    if (bulkAwardRecords != null)
                    {
                        awardContracts.AddRange(bulkAwardRecords);
                    }
                }
            }
       
            //Read the ISIR.FAFSA(s) records
            var applications = new List<string>();
            if (!string.IsNullOrEmpty(csAcyrDataContract.CsFedIsirId))
            {
                applications.Add(csAcyrDataContract.CsFedIsirId);
            }
            if (!string.IsNullOrEmpty(csAcyrDataContract.CsInstIsirId) && csAcyrDataContract.CsInstIsirId != csAcyrDataContract.CsFedIsirId)
            {
                applications.Add(csAcyrDataContract.CsInstIsirId);
            }
            var isirFafsaContracts = new List<IsirFafsa>();
            if (applications != null)
            {
                var bulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>("ISIR.FAFSA", applications.ToArray());
                if (bulkRecords != null)
                {
                    isirFafsaContracts.AddRange(bulkRecords);
                }
            }

            var notAwardedCategories = await GetNotAwardedCategoriesAsync();

            var studentId = recordInfo.PrimaryKey;
            var studentNeedSummary = BuildStudentNeedSummary(id, studentId, year,
                csAcyrDataContract,
                isirFafsaContracts,
                taAcyrContracts,
                awardContracts,
                notAwardedCategories);              

            return studentNeedSummary;
        }

        private StudentNeedSummary BuildStudentNeedSummary(string id, string studentId, string year, 
            CsAcyr csAcyrDataContract,
            List<IsirFafsa> isirFafsaContracts,
            List<TaAcyr> taAcyrContracts,
            List<Awards> awardContracts,
            IEnumerable<string> notAwardedCategories)
        {
            var studentNeedSummary = new StudentNeedSummary(studentId, year, id);

            try
            {
                // Save federal and institutional need values
                studentNeedSummary.FederalNeedAmount = csAcyrDataContract.CsNeed;
                studentNeedSummary.InstitutionalNeedAmount = csAcyrDataContract.CsInstNeed;

                // Save federal and institutional application keys (ISIR.FASFA keys)
                studentNeedSummary.CsFederalIsirId = csAcyrDataContract.CsFedIsirId;
                // Check if the fed ID has been corrected.
                var fedIsirId = csAcyrDataContract.CsFedIsirId;
                var fedOutcome = isirFafsaContracts.Where(fa => fa.Recordkey == fedIsirId).FirstOrDefault();
                if (fedOutcome != null)
                {
                    if (!string.IsNullOrEmpty(fedOutcome.IfafCorrectionId))
                    {
                        studentNeedSummary.CsFederalIsirId = fedOutcome.IfafCorrectionId;
                    }
                }           

                studentNeedSummary.CsInstitutionalIsirId = csAcyrDataContract.CsInstIsirId;
                // Check if the inst ID has been corrected.
                var instIsirId = csAcyrDataContract.CsInstIsirId;
                var instOutcome = isirFafsaContracts.Where(fa => fa.Recordkey == instIsirId).FirstOrDefault();
                if (instOutcome != null)
                {
                    if (!string.IsNullOrEmpty(instOutcome.IfafCorrectionId))
                    {
                        studentNeedSummary.CsInstitutionalIsirId = instOutcome.IfafCorrectionId;
                    }
                }                          
                
                // Save budget duration
                studentNeedSummary.BudgetDuration = csAcyrDataContract.CsBudgetDuration;

                // Save federal and institutional total expenses
                studentNeedSummary.FederalTotalExpenses = csAcyrDataContract.CsStdTotalExpenses;
                studentNeedSummary.InstitutionalTotalExpenses = csAcyrDataContract.CsInstTotalExpenses;

                //Save federal and institutional family contribution
                if (!string.IsNullOrEmpty(csAcyrDataContract.CsFc))
                {
                    studentNeedSummary.FederalFamilyContribution = Convert.ToInt32(csAcyrDataContract.CsFc);
                }
                studentNeedSummary.InstitutionalFamilyContribution = csAcyrDataContract.CsInstFc;

                // Calculate total need reduction
                if (taAcyrContracts.Any())
                {
                    studentNeedSummary.HasAward = true;
                    decimal familyContributionAwarded = 0;
                    decimal costAwarded = 0;
                    decimal needAwarded = 0;
                    foreach (var taAcyrContract in taAcyrContracts)
                    {
                        var awardId = taAcyrContract.Recordkey.Split('*')[1];
                        var awardRecord = awardContracts.Where(x => x.Recordkey == awardId).FirstOrDefault();
                        AddAwardAmounts(notAwardedCategories, ref familyContributionAwarded, ref costAwarded, ref needAwarded, taAcyrContract, awardRecord);
                    }
                    // Convert award values to integers
                    var familyContributionAwardedInteger = Convert.ToInt32(familyContributionAwarded);
                    var costAwardedInteger = Convert.ToInt32(costAwarded);
                    var needAwardedInteger = Convert.ToInt32(needAwarded);
                    //Adjust family contribution amount                   
                    int federalFamilyContributionAdjusted = 0;
                    int institutionalFamilyContributionAdjusted = 0;
                    int federalNeed = 0;
                    int institutionalNeed = 0;
                    if (csAcyrDataContract.CsNeed.HasValue)
                    {
                        federalNeed = csAcyrDataContract.CsNeed.GetValueOrDefault();
                    }
                    if (csAcyrDataContract.CsInstNeed.HasValue)
                    {
                        institutionalNeed = csAcyrDataContract.CsInstNeed.GetValueOrDefault();
                    }
                    if (familyContributionAwardedInteger >= 0)                        
                    {                        
                        federalFamilyContributionAdjusted = 0;
                        if (federalNeed >= 0)
                        {                            
                            var federalFamilyContribution = studentNeedSummary.FederalFamilyContribution ?? 0;     
                            if (familyContributionAwardedInteger > federalFamilyContribution)
                            {
                                federalFamilyContributionAdjusted = familyContributionAwardedInteger - federalFamilyContribution;
                            }                         
                        }
                        institutionalFamilyContributionAdjusted = 0;
                        if (institutionalNeed >= 0)
                        {
                            var institutionalFamilyContribution = studentNeedSummary.InstitutionalFamilyContribution ?? 0; 
                            if (familyContributionAwardedInteger > institutionalFamilyContribution)
                            {
                                institutionalFamilyContributionAdjusted = familyContributionAwardedInteger - institutionalFamilyContribution;
                            }
                        }
                    }
                    studentNeedSummary.FederalTotalNeedReduction = federalFamilyContributionAdjusted + costAwardedInteger + needAwardedInteger;
                    studentNeedSummary.InstitutionalTotalNeedReduction = institutionalFamilyContributionAdjusted + costAwardedInteger + needAwardedInteger;
                }
            }
            catch
            {
                var message = "Could not build Student Need Summary for ID " + id;
                throw new Exception(message);
            }
            return studentNeedSummary;
        }
        
        private static void AddAwardAmounts(IEnumerable<string> notAwardedCategories, ref decimal familyContributionAwarded, ref decimal costAwarded, ref decimal needAwarded, TaAcyr taAcyrContract, Awards awardRecord)
        {
            // Add to calculation if award category exists and not in the parameterized list of NOT awarded categories
            if (!string.IsNullOrEmpty(awardRecord.AwCategory))
            {
                if (!notAwardedCategories.Contains(taAcyrContract.TaTermAction))
                {
                    if (taAcyrContract.TaTermAmount != null)
                    {
                        decimal amount = (decimal)taAcyrContract.TaTermAmount;
                        if (taAcyrContract.TaTermAction != null)
                        {
                            switch (awardRecord.AwNeedCost)
                            {
                                case ("F"):
                                    familyContributionAwarded = familyContributionAwarded + amount;
                                    break;
                                case ("C"):
                                    costAwarded = costAwarded + amount;
                                    break;
                                case ("N"):
                                    needAwarded = needAwarded + amount;
                                    break;
                                case ("E"):
                                    // exclude for effecting student's budget
                                    break;
                                default:
                                    needAwarded = needAwarded + amount;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of award status categories that are excluded from 
        /// being considered as an awarded status.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetNotAwardedCategoriesAsync()
        {
            var sysParms = await GetSystemParametersAsync();
            return sysParms.FspNotAwardedCat;
        }

        private async Task<FaSysParams> GetSystemParametersAsync()
        {
            return await GetOrAddToCacheAsync<FaSysParams>("FinancialAidSystemParameters",
                            async () =>
                            {
                                return await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");

                            }, Level1CacheTimeoutValue);
        }
    }
}
