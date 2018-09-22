// Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Student.DataContracts;
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IFinancialAidApplicationOutcomeRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidApplicationOutcomeRepository : BaseColleagueRepository, IFinancialAidApplicationOutcomeRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;
        
        /// <summary>
        /// Constructor to instantiate a student FinancialAidApplicationOutcome repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public FinancialAidApplicationOutcomeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get the FinancialAidApplicationOutcomes requested
        /// </summary>
        /// <param name="id">FinancialAidApplicationOutcomes GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Fafsa> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the ISIR.CALC.RESULTS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || string.IsNullOrEmpty(recordInfo.Entity) || recordInfo.Entity != "ISIR.CALC.RESULTS")
            {
                throw new KeyNotFoundException(string.Format("No FA application outcome was found for guid {0}'. ", id));
            }
            var isirCalcResults = await DataReader.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", recordInfo.PrimaryKey);
            {
                if (isirCalcResults == null)
                {
                    throw new KeyNotFoundException(string.Format("No FA application outcome records was found for guid {0}'. ", id));
                }
            }

            // Read the ISIR.FAFSA record.
            var isirFafsa = new IsirFafsa();
            isirFafsa = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", recordInfo.PrimaryKey);

            // Read the ISIR.RESULTS record.
            var isirResults = new IsirResults();
            isirResults = await DataReader.ReadRecordAsync<IsirResults>("ISIR.RESULTS", recordInfo.PrimaryKey);


            // Read the ISIR.RESULTS of original (corrected-from) record if necessary.
            var isirResultsOriginal = new IsirResults();
            if (!string.IsNullOrEmpty(isirFafsa.IfafCorrectedFromId))
            {
                isirResultsOriginal = await DataReader.ReadRecordAsync<IsirResults>("ISIR.RESULTS", isirFafsa.IfafCorrectedFromId);
            }

            // Read the CS.ACYR record for this student/year.
            var year = isirFafsa.IfafImportYear;
            var csAcyrFile = string.Concat("CS.", year);
            var csAcyr = await DataReader.ReadRecordAsync<CsAcyr>(csAcyrFile, isirFafsa.IfafStudentId);

            // Read the SA.ACYR record for this student/year.
            var saAcyrFile = string.Concat("SA.", year);
            var saAcyr = await DataReader.ReadRecordAsync<SaAcyr>(saAcyrFile, isirFafsa.IfafStudentId);

            var fafsaEntity = new Fafsa(isirFafsa.Recordkey, isirFafsa.IfafStudentId, isirFafsa.IfafImportYear, recordInfo.PrimaryKey);

            //  Only include this FAFSA record if its:
            //     - a federal application with no correction
            //     - an institutional application with no correction
            //     - a correction to a federal application
            //     - a correction to an institutional application.
            bool validRecord = false;
            if (isirFafsa.Recordkey == csAcyr.CsFedIsirId && string.IsNullOrEmpty(isirFafsa.IfafCorrectionId))
            {
                validRecord = true;
            }
            if (isirFafsa.Recordkey == csAcyr.CsInstIsirId && string.IsNullOrEmpty(isirFafsa.IfafCorrectionId))
            {
                validRecord = true;
            }
            if (!string.IsNullOrEmpty(csAcyr.CsFedIsirId) && isirFafsa.IfafCorrectedFromId == csAcyr.CsFedIsirId)
            {
                validRecord = true;
            }
            if (!string.IsNullOrEmpty(csAcyr.CsInstIsirId) && isirFafsa.IfafCorrectedFromId == csAcyr.CsInstIsirId)
            {
                validRecord = true;
            }
            if (validRecord == true)
            {
                // Read the ISIR.PROFILE record (if one exists for this FAFSA record)          
                var isirProfile = new IsirProfile();
                isirProfile = await DataReader.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", isirFafsa.Recordkey);

                var profileNcp = new ProfileNcp();
                var isirFafsaNcp = new IsirFafsa();
                var isirCalcResultsNcp = new IsirCalcResults();
                var isirProfileNcp = new IsirProfile();

                if (isirFafsa.IfafIsirType == "PROF")
                {
                    // Check if this fafsa record is for noncustodial parents and we need to use a different "PNCP" type fafsa record instead.  
                    var criteria = new StringBuilder();
                    criteria.AppendFormat("WITH IFAF.STUDENT.ID = '{0}'", isirFafsa.IfafStudentId);
                    criteria.AppendFormat("WITH IFAF.IMPORT.YEAR = '{0}'", isirFafsa.IfafImportYear);
                    criteria.AppendFormat("WITH IFAF.ISIR.TYPE = 'PNCP'");
                    var pncpIds = await DataReader.SelectAsync("ISIR.FAFSA", criteria.ToString());
                    Array.Reverse(pncpIds);
                    var pncpId = pncpIds.FirstOrDefault();
                    if (pncpId != null)
                    {
                        // read the ISIR.FAFSA of the PNCP cofile record.  
                        isirFafsaNcp = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", pncpId);

                        if (isirFafsaNcp == null)
                        {
                            throw new KeyNotFoundException(string.Format("No PNCP fafsa record was found for guid {0}, using ISIR.FAFSA.ID {1}'. ", id, pncpId));
                        }
                        else
                        {
                            // Read the PROFILE.NCP record
                            profileNcp = await DataReader.ReadRecordAsync<ProfileNcp>("PROFILE.NCP", pncpId);

                            // read the ISIR.CALC.RESULTS of the PNCP cofile record.  
                            isirCalcResultsNcp = await DataReader.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", pncpId);

                            // read the ISIR.PROFILE of the PNCP cofile record.  
                            isirProfileNcp = await DataReader.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", pncpId);
                        }
                    }
                }
                                
                fafsaEntity = BuildFinancialAidApplication(isirFafsa,
                    csAcyr,
                    saAcyr,
                    isirCalcResults,
                    isirProfile,
                    isirResults,
                    profileNcp,
                    isirFafsaNcp,
                    isirCalcResultsNcp,
                    isirProfileNcp,
                    isirResultsOriginal);
            }
            else
            {
                var errorMessage = string.Format("Fafsa record with ID : '{0}' is not a federal, institutional, or correction of a federal or institutional record.", isirFafsa.RecordGuid);
                throw new ArgumentException(errorMessage);
            }
            return fafsaEntity;
        }

        /// <summary>
        /// Get all FinancialAidApplicationOutcomes.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="faSuiteYears">List of all financial aid years</param>
        /// <returns>A list of FinancialAidApplication domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<Fafsa>, int>> GetAsync(int offset, int limit, bool bypassCache, List<string> faSuiteYears)
        {
            // Build a list of application IDs. They must be either the federal or institutional
            // ISIR record defined in the CS.ACYR file.
            var unvalidatedApplicationIds = new List<string>();
            var applicationIds = new List<string>();
            // Check CS.ACYR from 2006 and forward.
            for (int year = 2006; year <= DateTime.Today.Year; year += 1)
            {
                string stringYear = year.ToString();
                if (faSuiteYears.Contains(stringYear))
                {
                    var csAcyrIds = await DataReader.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''");
                    for (int i = 0; i < csAcyrIds.Count(); i += readSize)
                    {
                        var subList = csAcyrIds.Skip(i).Take(readSize);
                        var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                        if (records != null)
                        {
                            foreach (var record in records)
                            {
                                // Build combined list of federal and institutional applications
                                if (!string.IsNullOrEmpty(record.CsFedIsirId))
                                {
                                    // Add federal application
                                    unvalidatedApplicationIds.Add(record.CsFedIsirId);
                                }

                                if (!string.IsNullOrEmpty(record.CsInstIsirId))
                                {
                                    if (string.IsNullOrEmpty(record.CsFedIsirId))
                                    {
                                        // Add institutional application since we know thre is no federal application
                                        // so can't be a duplicate.
                                        unvalidatedApplicationIds.Add(record.CsInstIsirId);
                                    }
                                    else
                                    {
                                        if (record.CsFedIsirId != record.CsInstIsirId)
                                        {
                                            // Add institutional application since it differs from the federal application
                                            unvalidatedApplicationIds.Add(record.CsInstIsirId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (unvalidatedApplicationIds != null)
            {
                var validApplicationIds = await DataReader.SelectAsync("ISIR.FAFSA", "WITH IFAF.STUDENT.ID NE '' AND WITH IFAF.IMPORT.YEAR NE ''");
                validApplicationIds = validApplicationIds.Intersect(unvalidatedApplicationIds).ToArray();
                applicationIds = validApplicationIds.ToList();
            }
            //
            //  Gather supporting data for the applicationIds
            //
            var totalCount = 0;
            if (applicationIds != null)
            {
                totalCount = applicationIds.Count();
                applicationIds.Sort();
                var applicationSubList = applicationIds.Skip(offset).Take(limit).ToArray();
                // We will never change the count of applicationSubList, but we want to replace 
                // each corrected application ID with its associated correction application ID
                // in a new "effective" list.  Default it to the original list initially.
                var effectiveApplicationSubList = applicationSubList;
                var correctionIDs = new List<string>();

                var isirFafsaRecords = new List<IsirFafsa>();
                var isirCalcResultsRecords = new List<IsirCalcResults>();
                var isirResultsRecords = new List<IsirResults>();
                //var correctedIsirFafsaRecords = new List<IsirFafsa>();
                //var correctedIsirCalcResultsRecords = new List<IsirCalcResults>();
                var csAcyrApplicants = new Dictionary<string, List<string>>();
                var csAcyrRecords = new Dictionary<string, List<CsAcyr>>();
                var saAcyrRecords = new Dictionary<string, List<SaAcyr>>();
                var isirProfileRecords = new List<IsirProfile>();
                var profileNcpIds = new List<string>();
                var profileNcpRecords = new List<ProfileNcp>();
                var isirFafsaNcpRecords = new List<IsirFafsa>();
                var isirCalcResultsNcpRecords = new List<IsirCalcResults>();
                var isirProfileNcpRecords = new List<IsirProfile>();
                var isirResultsOriginalRecords = new List<IsirResults>();

                // Bulk read ISIR.FAFSA         
                var bulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(applicationSubList);
                if (bulkRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of Isir Fafsa records");
                }
                else
                {
                    isirFafsaRecords.AddRange(bulkRecords);

                    // Loop through each fafsa record.  Need to:
                    // - build dictionary of CS.ACYR years and associated student IDs in each year.
                    // - identify if it has a corresponding PNCP (profile non-custodial parent) record.
                    // - identify if the record has been corrected.

                    for (int i = 0; i <= applicationSubList.Count() - 1; i++)
                    {
                        try
                        {
                            var record = bulkRecords.ElementAt(i);
                            var csFileYear = record.IfafImportYear;
                            if (csAcyrApplicants.ContainsKey(csFileYear))
                            {
                                // New entry in dictionary for csFileYear.  Add the one student ID.
                                var applicantList = csAcyrApplicants[csFileYear];
                                applicantList.Add(record.IfafStudentId);
                                csAcyrApplicants[csFileYear] = applicantList;
                            }
                            else
                            {
                                // Existing entry in dictionary for csFileYear.  Append the student ID to the list.
                                List<string> applicantList = new List<string>();
                                applicantList.Add(record.IfafStudentId);
                                csAcyrApplicants.Add(csFileYear, applicantList);
                            }
                            //
                            // Check if this fafsa record is for noncustodial parents and we need to use a different "PNCP" type fafsa record instead.  
                            // If so, add to a list of profile NCP Ids to bulk read later.
                            //
                            if (record.IfafIsirType == "PROF")
                            {
                                var criteria = new StringBuilder();
                                criteria.AppendFormat("WITH IFAF.STUDENT.ID = '{0}'", record.IfafStudentId);
                                criteria.AppendFormat("WITH IFAF.IMPORT.YEAR = '{0}'", record.IfafImportYear);
                                criteria.AppendFormat("WITH IFAF.ISIR.TYPE = 'PNCP'");
                                var pncpIds = await DataReader.SelectAsync("ISIR.FAFSA", criteria.ToString());
                                Array.Reverse(pncpIds);
                                var pncpId = pncpIds.FirstOrDefault();
                                if (!string.IsNullOrEmpty(pncpId))
                                {
                                    // Replace the original ID in the effective ID list with the PNCP Id
                                    //var originalFafsaId = effectiveApplicationSubList.ElementAt(i);
                                    //effectiveApplicationSubList = effectiveApplicationSubList.Select(s => s.Replace(originalFafsaId, pncpId)).ToArray();

                                    // Append a list of these profile NCP Ids.  We'll need to bulk read
                                    // the corresponding ISIR.FAFSAs and their PROFILE.NCPs
                                    profileNcpIds.Add(pncpId);
                                }
                            }
                            //
                            // Check if this application was corrected. 
                            //
                            if (!string.IsNullOrEmpty(record.IfafCorrectionId))
                            {
                                //  Replace the original ID in the effective ID list with the correction ID
                                effectiveApplicationSubList[i] = record.IfafCorrectionId;
                                
                                // Append a list of correction records for bulk read later.
                                correctionIDs.Add(record.IfafCorrectionId);
                            }
                        }
                        catch
                        {
                            var record = bulkRecords.ElementAt(i);
                            var errorMessage = string.Format("Unable to process application '{0}' for GUID '{1}'", applicationSubList.ElementAt(i), record.RecordGuid);
                            throw new ArgumentException(errorMessage);
                        }
                    }
                }

                // Bulk read any cofile records in ISIR.CALC.RESULTS for the sublist of applications.
                var bulkIsirCalcResultsRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(effectiveApplicationSubList);
                if (bulkIsirCalcResultsRecords != null)
                {
                    isirCalcResultsRecords.AddRange(bulkIsirCalcResultsRecords);
                }

                //  If correction records were found, bulk read and add them to the list of fafsa records
                if (correctionIDs.Any())
                {
                    var correctionBulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(correctionIDs.ToArray());
                    if (correctionBulkRecords == null)
                    {
                        logger.Error("Unexpected null from bulk read of correction Isir Fafsa records");
                    }
                    else
                    {
                        isirFafsaRecords.AddRange(correctionBulkRecords);
                    }

                    // Also add any calc results records from correction IDs

                    var correctionBulkResultsRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(correctionIDs.ToArray());
                    if (correctionBulkResultsRecords != null)
                    {
                        isirCalcResultsRecords.AddRange(correctionBulkResultsRecords);
                    }
                }

                // Bulk read any cofile records in ISIR.RESULTS for the sublist of applications.
                var bulkIsirResultsRecords = await DataReader.BulkReadRecordAsync<IsirResults>(effectiveApplicationSubList);
                if (bulkIsirResultsRecords != null)
                {
                    isirResultsRecords.AddRange(bulkIsirResultsRecords);
                }

                // Bulk read any cofile records in ISIR.PROFILE for the sublist of applications.
                var IsirProfileRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(effectiveApplicationSubList);
                if (IsirProfileRecords != null)
                {
                    isirProfileRecords.AddRange(IsirProfileRecords);
                }

                //  If Profile NCP records were found from select while processing each ISIR.FAFSA, 
                //  bulk read and add them to the list of fafsa records.   
                if (profileNcpIds != null)
                {
                    if (profileNcpIds.Any())
                    {
                        var bulkIsirFafsaNcpRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(profileNcpIds.ToArray());
                        if (bulkIsirFafsaNcpRecords == null)
                        {
                            logger.Error("Unexpected null from bulk read of Isir Fafsa records for PNCP");
                        }
                        else
                        {
                            isirFafsaNcpRecords.AddRange(bulkIsirFafsaNcpRecords);
                        }

                        //  And bulk read their PROFILE.NCP records.
                        var bulkProfileNcpRecords = await DataReader.BulkReadRecordAsync<ProfileNcp>(profileNcpIds.ToArray());
                        if (bulkProfileNcpRecords != null)
                        {
                            profileNcpRecords.AddRange(bulkProfileNcpRecords);
                        }

                        // Bulk read NCP Isir Calc Results
                        var bulkIsirCalcResultsNcpRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(profileNcpIds.ToArray());
                        if (bulkIsirCalcResultsNcpRecords != null)
                        {
                            isirCalcResultsNcpRecords.AddRange(bulkIsirCalcResultsNcpRecords);
                        }

                        // Bulk read NCP isir Profile
                        var bulkIsirProfileNcpRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(profileNcpIds.ToArray());
                        if (bulkIsirProfileNcpRecords != null)
                        {
                            isirProfileNcpRecords.AddRange(bulkIsirProfileNcpRecords);
                        }
                    }
                }

                // Do a bulk read for each CS.ACYR year and populate a dictionary of the results
                foreach (var csFileYear in csAcyrApplicants.Keys)
                {
                    var yearRecords = new List<CsAcyr>();
                    var applicantsList = csAcyrApplicants[csFileYear];
                    applicantsList.Sort();
                    for (int i = 0; i < applicantsList.Count(); i += readSize)
                    {
                        var subList = applicantsList.Skip(i).Take(readSize);
                        var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + csFileYear, subList.ToArray());
                        if (records != null)
                        {
                            yearRecords.AddRange(records);
                        }
                    }
                    //  Add the bulk read results of each year to the dictionary row with the appropriate year. 
                    var csYearRecords = new List<CsAcyr>();
                    csYearRecords.AddRange(yearRecords);
                    csAcyrRecords.Add(csFileYear, csYearRecords);
                }

                // Do a bulk read for each SA.ACYR year and populate a dictionary of the results
                foreach (var saFileYear in csAcyrApplicants.Keys)
                {
                    var yearRecords = new List<SaAcyr>();
                    var applicantsList = csAcyrApplicants[saFileYear];
                    applicantsList.Sort();
                    for (int i = 0; i < applicantsList.Count(); i += readSize)
                    {
                        var subList = applicantsList.Skip(i).Take(readSize);
                        var records = await DataReader.BulkReadRecordAsync<SaAcyr>("SA." + saFileYear, subList.ToArray());
                        if (records != null)
                        {
                            yearRecords.AddRange(records);
                        }
                    }
                    //  Add the bulk read results of each year to the dictionary row with the appropriate year. 
                    var saYearRecords = new List<SaAcyr>();
                    saYearRecords.AddRange(yearRecords);
                    saAcyrRecords.Add(saFileYear, saYearRecords);
                }

                // Bulk read any ISIR.RESULTS for original (corrected-from) records
                var originalProfileIds = isirFafsaRecords.Select(x => x.IfafCorrectedFromId).Distinct().ToList();
                if (originalProfileIds.Any())
                {
                    var bulkIsirResultsOriginalRecords = await DataReader.BulkReadRecordAsync<IsirResults>(originalProfileIds.ToArray());
                    if (bulkIsirResultsOriginalRecords != null)
                    {
                        isirResultsOriginalRecords.AddRange(bulkIsirResultsOriginalRecords);
                    }
                }

                // Read FA.SYS.PARAMS
                var faSysParams = await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");

                var isirFafsaEntities = await BuildFinancialAidApplicationsAsync(effectiveApplicationSubList.ToList(),
                    isirFafsaRecords,
                    csAcyrRecords,
                    saAcyrRecords,
                    isirCalcResultsRecords,
                    isirProfileRecords,
                    isirResultsRecords,
                    profileNcpRecords,
                    isirFafsaNcpRecords,
                    isirCalcResultsNcpRecords,
                    isirProfileNcpRecords,
                    isirResultsOriginalRecords);

                return new Tuple<IEnumerable<Fafsa>, int>(isirFafsaEntities, totalCount);
            }
            else
            {

                logger.Error("No federal or institutional application found in 2005 and later");
                IEnumerable<Fafsa> isirFafsaEntities = null;
                return new Tuple<IEnumerable<Fafsa>, int>(isirFafsaEntities, 0);
            }
        }

        private async Task<IEnumerable<Fafsa>> BuildFinancialAidApplicationsAsync(List<string> fafsaIds,
            List<IsirFafsa> isirFafsasData,
            Dictionary<string, List<CsAcyr>> csAcyrsData,
            Dictionary<string, List<SaAcyr>> saAcyrsData,
            List<IsirCalcResults> isirCalcResultsData,
            List<IsirProfile> isirProfilesData,
            List<IsirResults> isirResultsData,
            List<ProfileNcp> profileNcpsData,
            List<IsirFafsa> isirFafsasNcpData,
            List<IsirCalcResults> isirCalcResultsNcpData,
            List<IsirProfile> isirProfilesNcpData,
            List<IsirResults> isirResultsOriginalData)
        {

            var fafsaList = new List<Fafsa>();

            foreach (var fafsaId in fafsaIds)
            {
                var isirFafsaData = isirFafsasData.Where(ifd => ifd.Recordkey == fafsaId).FirstOrDefault();
                if (isirFafsaData == null)
                {
                    var errorMessage = string.Format("Unable to find previously read data for ISIR.FAFSA record '{0}'", isirFafsaData.Recordkey);
                    throw new ArgumentException(errorMessage);
                }
                
                var studentId = isirFafsaData.IfafStudentId;
                var year = isirFafsaData.IfafImportYear;
                var isirType = isirFafsaData.IfafIsirType;
                var thisCsAcyrData = csAcyrsData[isirFafsaData.IfafImportYear];
                var thisSaAcyrData = saAcyrsData[isirFafsaData.IfafImportYear];
                    
                try
                {
                    var csAcyr = thisCsAcyrData.Where(ca => ca.Recordkey == isirFafsaData.IfafStudentId).FirstOrDefault();
                    var saAcyr = thisSaAcyrData.Where(ca => ca.Recordkey == isirFafsaData.IfafStudentId).FirstOrDefault();
                    var isirCalcResultData = isirCalcResultsData.Where(x => x.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();
                    var isirProfileData = isirProfilesData.Where(x => x.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();
                    var isirResultData = isirResultsData.Where(x => x.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();
                    var profileNcpData = profileNcpsData.Where(x => x.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();
                    var isirFafsaNcpData = isirFafsasNcpData.Where(x => x.IfafStudentId == studentId && x.IfafImportYear == year && x.IfafIsirType == "PNCP").FirstOrDefault();                    
                    var isirCalcResultNcpData = new IsirCalcResults();
                    var isirProfileNcpData = new IsirProfile();
                    if (isirFafsaNcpData != null)
                    {
                        if (!string.IsNullOrEmpty(isirFafsaNcpData.Recordkey))
                        {
                            isirCalcResultNcpData = isirCalcResultsNcpData.Where(x => x.Recordkey == isirFafsaNcpData.Recordkey).FirstOrDefault();
                            isirProfileNcpData = isirProfilesNcpData.Where(x => x.Recordkey == isirFafsaNcpData.Recordkey).FirstOrDefault();
                        }
                    }
                    var isirResultOriginalData = new IsirResults();
                    if (isirFafsaData.IfafIsirType == "CORR")
                    {
                        isirResultOriginalData = isirResultsOriginalData.Where(x => x.Recordkey == isirFafsaData.IfafCorrectedFromId).FirstOrDefault();
                    }
                    
                    var fafsaEntity = BuildFinancialAidApplication(isirFafsaData,
                        csAcyr,
                        saAcyr,
                        isirCalcResultData,
                        isirProfileData,
                        isirResultData,
                        profileNcpData,
                        isirFafsaNcpData,
                        isirCalcResultNcpData,
                        isirProfileNcpData,
                        isirResultOriginalData
                        );

                    fafsaList.Add(fafsaEntity);
                }
                catch
                {
                    var errorMessage = string.Format("Unable to build data processing FA application '{0}'", isirFafsaData.Recordkey);
                    throw new ArgumentException(errorMessage);
                }
            }
            return fafsaList;
        }
        
        private Fafsa BuildFinancialAidApplication(IsirFafsa isirFafsaData,
            CsAcyr csAcyrData,
            SaAcyr saAcyrData,
            IsirCalcResults isirCalcResultData,
            IsirProfile isirProfileData,
            IsirResults isirResultsData,
            ProfileNcp profileNcpData,
            IsirFafsa isirFafsaNcpData,
            IsirCalcResults isirCalcResultNcpData,
            IsirProfile isirProfileNcpData,
            IsirResults isirResultsOriginalData)
        {
            Fafsa fafsa = new Fafsa(isirFafsaData.Recordkey, isirFafsaData.IfafStudentId, isirFafsaData.IfafImportYear, isirFafsaData.RecordGuid);
            fafsa.CalcResultsGuid = isirCalcResultData.RecordGuid;
            fafsa.CsFederalIsirId = csAcyrData.CsFedIsirId;
            fafsa.CsInstitutionalIsirId = csAcyrData.CsInstIsirId;
            fafsa.ApplicationCompletedOn = isirFafsaData.IfafDateSign;
            fafsa.StateOfLegalResidence = isirFafsaData.IfafSLegalRes;
            fafsa.Type = isirFafsaData.IfafIsirType;
            fafsa.WorkStudyInterest = isirFafsaData.IfafInterestCws;
            fafsa.CorrectedFromId = isirFafsaData.IfafCorrectedFromId;

            fafsa.FafsaPrimaryId = isirFafsaData.Recordkey;
            fafsa.FafsaPrimaryType = isirFafsaData.IfafIsirType;


            fafsa.HasNonCustodialParentProfile = false;

            if (profileNcpData != null)
            {
                if (profileNcpData.Recordkey != null)
                {
                    fafsa.HasNonCustodialParentProfile = true;
                }
            }          

            // dependency override
            fafsa.StudentDependencyOverride = isirFafsaData.IfafDependOverride;
            
            // professional judgement
            fafsa.FinancialAidAAministratorAdjustment = isirFafsaData.IfafFaaAdj;


            fafsa.HasIsirResults = false;                        
            if (isirResultsData != null)
            {
                fafsa.HasIsirResults = true;
                // Student Aid Report C flag
                if (isirResultsData.IresSarCFlag != null)
                {
                    if (isirResultsData.IresSarCFlag.ToUpper() == "Y")
                    {
                        fafsa.HasStudentAidReportC = true;
                    }
                }

                // Pell eligibility
                if (!string.IsNullOrEmpty(isirResultsData.IresCpsPellElig))
                {
                    if (isirResultsData.IresCpsPellElig.ToUpper() == "Y")
                    {
                        fafsa.IsPellEligible = true;
                    }
                    else
                    {
                        fafsa.IsPellEligible = false;
                    }
                }

                // verification selection
                if (isirResultsData.IresVerifFlag != null)
                {                        
                    if (isirResultsData.IresVerifFlag.ToUpper() == "Y")
                    {
                        fafsa.HasVerificationSelection = true;
                    }
                    else
                    {
                        if (isirResultsData.IresVerifFlag == "*")
                        {
                            fafsa.HasVerificationSelection = true;
                        }
                        else
                        {
                            fafsa.HasVerificationSelection = false;
                        }
                    }
                }
                
                // verification tracking
                int importYear;
                int minYear = 2013;
                Int32.TryParse(isirFafsaData.IfafImportYear, out importYear);
                if (importYear >= minYear)
                {
                    fafsa.VerificationTracking = isirResultsData.IresVerifTracking;
                }                
            }

            fafsa.HasIsirResultsOriginal = false; 
            if (isirResultsOriginalData != null)
            {
                fafsa.HasIsirResultsOriginal = true;
                // verification selection for corrected record.  Read from original ISIR.RESULTS
                if (isirResultsOriginalData.IresVerifFlag != null)
                {
                    if (isirResultsOriginalData.IresVerifFlag.ToUpper() == "Y")
                    {
                        fafsa.HasVerificationSelectionOriginal = true;
                    }
                    else
                    {
                        if (isirResultsOriginalData.IresVerifFlag == "*")
                        {
                            fafsa.HasVerificationSelectionOriginal = true;
                        }
                        else
                        {
                            fafsa.HasVerificationSelectionOriginal = false;
                        }
                    }
                }

                if (isirFafsaData.IfafIsirType == "CORR")
                {
                    // Pell eligibility
                    if (saAcyrData.SaPellEntitlement != null)
                    {
                        if (saAcyrData.SaPellEntitlement.Any())
                        {
                            var pellEntitlements = saAcyrData.SaPellEntitlement;
                            bool checkPellEligibile = false;
                            foreach (var pellEntitlement in pellEntitlements)
                            {
                                int pellEntitlementInteger;                                
                                Int32.TryParse(pellEntitlement, out pellEntitlementInteger);
                                if (pellEntitlementInteger > 0)
                                {
                                    checkPellEligibile = true;
                                }
                            }
                            if (checkPellEligibile == true)
                            {
                                if (!string.IsNullOrEmpty(isirResultsOriginalData.IresCpsPellElig))
                                {
                                    if (isirResultsOriginalData.IresCpsPellElig.ToUpper() == "Y")
                                    {
                                        fafsa.IsPellEligibleOriginal = true;
                                    }
                                    else
                                    {
                                        fafsa.IsPellEligibleOriginal = false;
                                    }
                                }
                            }
                        }
                    }
                    // verification tracking
                    int importYear;
                    int minYear = 2013;
                    Int32.TryParse(isirFafsaData.IfafImportYear, out importYear);
                    if (importYear >= minYear)
                    {
                        fafsa.VerificationTrackingOriginal = isirResultsOriginalData.IresVerifTracking;
                    }   
                }
            }

            if (isirCalcResultData != null)
            {
                // rejection codes
                fafsa.RejectionCodes = isirCalcResultData.IcresRejectCodes;

                // depedency status
                fafsa.StudentDepdendencyStatusInas = isirCalcResultData.IcresInasDependency;
                fafsa.StudentDependencyStatus = isirCalcResultData.IcresDependency;                    

                // Automatic Zero Expected EFC indicator
                if (isirCalcResultData.IcresAzeInd != null)
                {
                    if (isirCalcResultData.IcresAzeInd.ToUpper() == "Y")
                    {
                        fafsa.HasAutomaticZeroExpectedFamilyContribution = true;
                    }
                    else
                    {
                        fafsa.HasAutomaticZeroExpectedFamilyContribution = false;
                    }
                }

                // Simplified needs indicator
                if (isirCalcResultData.IcresSimpleNeedInd != null)
                {
                    if (isirCalcResultData.IcresSimpleNeedInd.ToUpper() == "Y")
                    {
                        fafsa.HasMetSimpleNeed = true;
                    }
                    else
                    {
                        fafsa.HasMetSimpleNeed = false;
                    }
                }     

                fafsa.InstitutionalNeedAnalysisParentsContribution = isirCalcResultData.IcresInasParPc;
                fafsa.InstitutionalNeedAnalysisStudentContribution = isirCalcResultData.IcresInasStuSc;
                fafsa.InstitutionFamilyContributionOverrideAmount = isirCalcResultData.IcresInstEfcOvrAmt;
                fafsa.NonCustodialParentOverrideAmount = isirCalcResultData.IcresNcpOverrideAmt;
                fafsa.FamilyContribution = isirCalcResultData.IcresPriEfc;
                fafsa.FisapTotalIncome = isirCalcResultData.IcresPriFti;
                fafsa.ParentContribution = isirCalcResultData.IcresPriPc;
                fafsa.StudentContribution = isirCalcResultData.IcresPriSc;
                fafsa.CfsParentOptionalImCalculation = isirCalcResultData.IcresImOptParCfs;
                fafsa.CfsStudentOptionalImCalculation = isirCalcResultData.IcresImOptStuCfs;                
            }

            if (isirProfileData != null)
            {
                fafsa.NonCustodialParentContribution = isirProfileData.IproNonCustParContr;
                fafsa.ParentHomeDebt = isirProfileData.IproParHomeDebt;
                fafsa.StudentHomeDebt = isirProfileData.IproStuHomeDebt;
                fafsa.ParentHomeValue = isirProfileData.IproParHomeValue;
                fafsa.StudentHomeValue = isirProfileData.IproStuHomeValue;
            }                  

            if (profileNcpData != null)
            {                
                if (isirCalcResultNcpData != null)
                {
                    fafsa.NonCustodialParentCalculatedContributionNcp = isirCalcResultNcpData.IcresNcpImOptCfs;
                    fafsa.NonCustodialParentOverrideAmountNcp = isirCalcResultNcpData.IcresNcpOverrideAmt;
                }
                if (isirProfileNcpData != null)
                {
                    fafsa.ParentHomeDebtNcp = isirProfileNcpData.IproParHomeDebt;
                    fafsa.ParentHomeValueNcp = isirProfileNcpData.IproParHomeValue;
                }
            }

            return fafsa;
        }
    }
}
