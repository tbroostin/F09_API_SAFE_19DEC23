// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IFinancialAidApplicationRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentFinancialAidApplicationRepository : BaseColleagueRepository, IStudentFinancialAidApplicationRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        private Collection<FaLocations> faLocationDataContracts;
        private Collection<FaOffices> faOfficeDataContracts;
        private FaSysParams faSysParamsDataContracts;
        //private Collection<IsirFafsa> isirFafsaDataContacts;

        /// <summary>
        /// Constructor to instantiate a student financial aid applications repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public StudentFinancialAidApplicationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get the FinancialAidApplications requested
        /// </summary>
        /// <param name="id">FinancialAidApplications GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Fafsa> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the ISIR.FAFSA record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || string.IsNullOrEmpty(recordInfo.Entity) || recordInfo.Entity != "ISIR.FAFSA")
            {
                throw new KeyNotFoundException(string.Format("No FA application was found for guid {0}'. ", id));
            }
            var isirFafsa = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", recordInfo.PrimaryKey);
            {
                if (isirFafsa == null)
                {
                    throw new KeyNotFoundException(string.Format("No FA application was found for guid {0}'. ", id));
                }
            }

            // Read the CS.ACYR record for this student/year.
            var year = isirFafsa.IfafImportYear;
            var csAcyrFile = string.Concat("CS.", year);
            var csAcyr = await DataReader.ReadRecordAsync<CsAcyr>(csAcyrFile, isirFafsa.IfafStudentId);

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
                        isirFafsaNcp = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", pncpId);
                        {
                            if (isirFafsaNcp == null)
                            {
                                throw new KeyNotFoundException(string.Format("No PNCP fafsa record was found for guid {0}. Entity:'ISIR.FAFSA', Record Id :'{1}''. ", id, pncpId));
                            }
                            else
                            {
                                profileNcp = await DataReader.ReadRecordAsync<ProfileNcp>("PROFILE.NCP", isirFafsaNcp.Recordkey);
                                isirCalcResultsNcp = await DataReader.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", isirFafsaNcp.Recordkey);
                                isirProfileNcp = await DataReader.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", isirFafsaNcp.Recordkey);
                            }
                        }
                    }
                }

                // Read the ISIR.CALC.RESULTS record (if one exists for this FAFSA record)
                var isirCalcResults = new IsirCalcResults();
                isirCalcResults = await DataReader.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", isirFafsa.Recordkey);

                // Read the ISIR.PROFILE record (if one exists for this FAFSA record)          
                var isirProfile = new IsirProfile();
                isirProfile = await DataReader.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", isirFafsa.Recordkey);

                // Read the corrected-from ISIR.FAFSA and ISIR.CALC.RESULTS record for when this is a corection record from type of ISIR or CPPSG.
                var origIsirFafsa = new IsirFafsa();
                var origIsirCalcResults = new IsirCalcResults();
                if (isirFafsa.IfafCorrectedFromId != null && isirFafsa.IfafCorrectedFromId.Any())
                {
                    origIsirFafsa = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", isirFafsa.IfafCorrectedFromId);
                    origIsirCalcResults = await DataReader.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", isirFafsa.IfafCorrectedFromId);
                }

                // Read FA.SYS.PARAMS
                var faSysParams = await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");

                fafsaEntity = await (BuildFinancialAidApplication(isirFafsa,
                    csAcyr,
                    isirCalcResults,
                    isirProfile,
                    isirFafsaNcp,
                    isirCalcResultsNcp,
                    isirProfileNcp,
                    profileNcp,
                    origIsirFafsa,
                    origIsirCalcResults,
                    faSysParams));
            }
            else
            {
                var errorMessage = string.Format("Fafsa record with ID : '{0}' is an invalid application.  Must be an uncorrected federal, uncorrected institutional, correction federal, or correction institutional application.", isirFafsa.Recordkey);
                throw new ArgumentException(errorMessage);
            }
            return fafsaEntity;
        }

        /// <summary>
        /// Get student financial aid applications
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="faSuiteYears">List of all financial aid years</param>
        /// <returns>A list of FinancialAidApplication domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<Fafsa>, int>> GetAsync(int offset, int limit, bool bypassCache, string studentId, string aidYear, List<string> faSuiteYears)
        {
            var applicationIds = new List<string>();
            var aidYears = new List<string>();
            //if there is a Aid year then use that otherwise get it from 2006
            if (!string.IsNullOrEmpty(aidYear))
                //if no suite file for that year then return empty
                if (faSuiteYears.Contains(aidYear))
                    aidYears.Add(aidYear);
                else
                    return new Tuple<IEnumerable<Fafsa>, int>(null, 0);
            else
                // Check CS.ACYR from 2006 and forward.
                for (int year = 2006; year <= DateTime.Today.Year; year += 1)
                {
                    string stringYear = year.ToString();
                    if (faSuiteYears.Contains(stringYear))
                        aidYears.Add(stringYear);
                }
            //call to get the appropriate application id
            applicationIds = await GetApplicationIds(aidYears, studentId);
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
                var correctedIsirFafsaRecords = new List<IsirFafsa>();
                var correctedIsirCalcResultsRecords = new List<IsirCalcResults>();
                var csAcyrApplicants = new Dictionary<string, List<string>>();
                var csAcyrRecords = new Dictionary<string, List<CsAcyr>>();
                var isirProfileRecords = new List<IsirProfile>();
                var isirFafsaNcpRecords = new List<IsirFafsa>();
                var isirCalcResultsNcpRecords = new List<IsirCalcResults>();
                var isirProfileNcpRecords = new List<IsirProfile>();
                var profileNcpIds = new List<string>();
                var profileNcpRecords = new List<ProfileNcp>();

                // Bulk read ISIR.FAFSA         
                var bulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(applicationSubList);

                // Bulk read any cofile records in ISIR.CALC.RESULTS for the sublist of applications.
                var bulkIsirCalcResultsRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(applicationSubList);
                if (bulkIsirCalcResultsRecords != null)
                {
                    isirCalcResultsRecords.AddRange(bulkIsirCalcResultsRecords);
                }

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

                                // Store the original record that has been corrected
                                correctedIsirFafsaRecords.Add(record);

                                // Store the original calc results record that has been corrected
                                var calcResultsRecord = bulkIsirCalcResultsRecords.Where(cr => cr.Recordkey == applicationIds.ElementAt(i));
                                correctedIsirCalcResultsRecords.AddRange(calcResultsRecord);

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

                //  If Profile NCP records were found from select while processing each ISIR.FAFSA, 
                //  bulk read and add them to the list of fafsa records.   
                if (profileNcpIds.Any())
                {
                    var ncpBulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(profileNcpIds.ToArray());
                    if (ncpBulkRecords == null)
                    {
                        logger.Error("Unexpected null from bulk read of Isir Fafsa records for PNCP");
                    }
                    else
                    {
                        isirFafsaNcpRecords.AddRange(ncpBulkRecords);
                    }
                    //  And bulk read their PROFILE.NCP records.
                    var profileNcpBulkRecords = await DataReader.BulkReadRecordAsync<ProfileNcp>(profileNcpIds.ToArray());
                    if (profileNcpBulkRecords != null)
                    {
                        profileNcpRecords.AddRange(profileNcpBulkRecords);
                    }

                    // Add bulk read of ISIR.CALC.RESULTS for PNCP-type
                    var isirCalcResultsNcpBulkRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(profileNcpIds.ToArray());
                    if (isirCalcResultsNcpBulkRecords != null)
                    {
                        isirCalcResultsNcpRecords.AddRange(isirCalcResultsNcpBulkRecords);
                    }

                    // Add bulk red of ISIR.PROFILE for PNCP-type
                    var isirProfileNcpBulkRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(profileNcpIds.ToArray());
                    if (isirProfileNcpBulkRecords != null)
                    {
                        isirProfileNcpRecords.AddRange(isirProfileNcpBulkRecords);
                    }
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

                // Bulk read any cofile records in ISIR.PROFILE for the sublist of applications.
                var IsirProfileRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(effectiveApplicationSubList);
                if (IsirProfileRecords != null)
                {
                    isirProfileRecords.AddRange(IsirProfileRecords);
                }

                // Read FA.SYS.PARAMS
                var faSysParams = await GetFaSysParms();

                var isirFafsaEntities = await BuildFinancialAidApplicationsAsync(effectiveApplicationSubList.ToList(),
                    isirFafsaRecords,
                    csAcyrRecords,
                    isirCalcResultsRecords,
                    isirProfileRecords,
                    isirFafsaNcpRecords,
                    isirCalcResultsNcpRecords,
                    isirProfileNcpRecords,
                    profileNcpRecords,
                    correctedIsirFafsaRecords,
                    correctedIsirCalcResultsRecords,
                    faSysParams);

                return new Tuple<IEnumerable<Fafsa>, int>(isirFafsaEntities, totalCount);
            }
            else
            {

                logger.Error("No federal or institutional application found in 2005 and later");
                IEnumerable<Fafsa> isirFafsaEntities = null;
                return new Tuple<IEnumerable<Fafsa>, int>(isirFafsaEntities, 0);
            }
        }

        /// <summary>
        /// Get the Applications Ids
        /// </summary>
        /// <param name="year">Financial Aid Year</param>
        /// <returns>A list of FinancialAidApplication Ids</returns>
        private async Task<List<string>> GetApplicationIds(List<string> aidyears, string studentId)
        {
            // Build a list of application IDs. They must be either the federal or institutional
            // ISIR record defined in the CS.ACYR file.
            var unvalidatedApplicationIds = new List<string>();
            var applicationIds = new List<string>();
            var fedCriteria = "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE '' SAVING UNIQUE CS.FED.ISIR.ID";
            var instCriteria = "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE '' SAVING UNIQUE CS.INST.ISIR.ID";
            var stuCriteria = string.Empty;
            if (!string.IsNullOrEmpty(studentId))
                stuCriteria = string.Concat("WITH CS.STUDENT.ID EQ '", studentId, "'");
            if (aidyears != null && aidyears.Any())
            {
                foreach ( var year in aidyears)
                {
                    var limitingKeys = new List<string>();
                    string[] csAcyrIdsFed = new string[] { };
                    string[] csAcyrIdsInst = new string[] { };
                    //if there is a student Id then we just care of CS.ACYR record that belong to the student.
                    if (!string.IsNullOrEmpty(stuCriteria))
                    {
                        limitingKeys = (await DataReader.SelectAsync("CS." + year, stuCriteria)).ToList();
                        if (limitingKeys != null && limitingKeys.Any())
                        {
                            csAcyrIdsFed = await DataReader.SelectAsync("CS." + year, limitingKeys.ToArray(), fedCriteria);
                            unvalidatedApplicationIds.AddRange(csAcyrIdsFed);
                            csAcyrIdsInst = await DataReader.SelectAsync("CS." + year, limitingKeys.ToArray(), instCriteria);
                            unvalidatedApplicationIds.AddRange(csAcyrIdsInst);

                        }
                    }
                    else
                    {
                        csAcyrIdsFed = await DataReader.SelectAsync("CS." + year, fedCriteria);
                        unvalidatedApplicationIds.AddRange(csAcyrIdsFed);
                        csAcyrIdsInst = await DataReader.SelectAsync("CS." + year, instCriteria);
                        unvalidatedApplicationIds.AddRange(csAcyrIdsInst);

                    }                
                }
                unvalidatedApplicationIds = unvalidatedApplicationIds.Distinct().ToList();
            }
            if (unvalidatedApplicationIds != null && unvalidatedApplicationIds.Any())
            {
                var IsirCriteria = "WITH IFAF.IMPORT.YEAR NE ''";
                // check if there is just one year then use that as a criteria
                var validApplicationIds = await DataReader.SelectAsync("ISIR.FAFSA", unvalidatedApplicationIds.ToArray(), IsirCriteria);
                applicationIds = validApplicationIds.ToList();
            }
            return applicationIds;
        }

        private async Task<IEnumerable<Fafsa>> BuildFinancialAidApplicationsAsync(List<string> fafsaIds,
            List<IsirFafsa> isirFafsasData,
            Dictionary<string, List<CsAcyr>> csAcyrsData,
            List<IsirCalcResults> isirCalcResultsData,
            List<IsirProfile> isirProfilesData,
            List<IsirFafsa> isirFafsaNcpsData,
            List<IsirCalcResults> isirCalcResultsNcpsData,
            List<IsirProfile> isirProfileNcpsData,
            List<ProfileNcp> profileNcpsData,
            List<IsirFafsa> correctedIsirFafsasData,
            List<IsirCalcResults> correctedIsirCalcResultsData,
            FaSysParams faSysParams)
        {

            var fafsaList = new List<Fafsa>();

            foreach (var fafsaId in fafsaIds)
            {
                var isirFafsaData = isirFafsasData.Where(ifd => ifd.Recordkey == fafsaId).FirstOrDefault();
                var thisCsAcryData = csAcyrsData[isirFafsaData.IfafImportYear];

                try
                {
                    var csAcyr = thisCsAcryData.Where(ca => ca.Recordkey == isirFafsaData.IfafStudentId).FirstOrDefault();
                    var isirCalcResultData = isirCalcResultsData.Where(icr => icr.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();
                    var isirProfileData = isirProfilesData.Where(ip => ip.Recordkey == isirFafsaData.Recordkey).FirstOrDefault();

                    var studentId = isirFafsaData.IfafStudentId;
                    var year = isirFafsaData.IfafImportYear;
                    var isirFafsaNcpData = isirFafsaNcpsData.Where(x => x.IfafStudentId == studentId && x.IfafImportYear == year && x.IfafIsirType == "PNCP").FirstOrDefault();

                    var isirCalcResultNcpData = new IsirCalcResults();
                    var isirProfileNcpData = new IsirProfile();
                    var profileNcpData = new ProfileNcp();
                    if (isirFafsaNcpData != null)
                    {
                        if (!string.IsNullOrEmpty(isirFafsaNcpData.Recordkey))
                        {
                            profileNcpData = profileNcpsData.Where(pn => pn.Recordkey == isirFafsaNcpData.Recordkey).FirstOrDefault();
                            isirCalcResultNcpData = isirCalcResultsNcpsData.Where(x => x.Recordkey == isirFafsaNcpData.Recordkey).FirstOrDefault();
                            isirProfileNcpData = isirProfileNcpsData.Where(x => x.Recordkey == isirFafsaNcpData.Recordkey).FirstOrDefault();
                        }
                    }

                    var correctedIsirFafsaData = new IsirFafsa();
                    var correctedIsirCalcResultData = new IsirCalcResults();
                    if (!string.IsNullOrEmpty(isirFafsaData.IfafCorrectedFromId))
                    {
                        correctedIsirFafsaData = correctedIsirFafsasData.Where(ic => ic.Recordkey == isirFafsaData.IfafCorrectedFromId).FirstOrDefault();
                        correctedIsirCalcResultData = correctedIsirCalcResultsData.Where(ic => ic.Recordkey == isirFafsaData.IfafCorrectedFromId).FirstOrDefault();
                    }

                    var fafsaEntity = await (BuildFinancialAidApplication(isirFafsaData,
                        csAcyr,
                        isirCalcResultData,
                        isirProfileData,
                        isirFafsaNcpData,
                        isirCalcResultNcpData,
                        isirProfileNcpData,
                        profileNcpData,
                        correctedIsirFafsaData,
                        correctedIsirCalcResultData,
                        faSysParams));

                    fafsaList.Add(fafsaEntity);
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch
                {
                    var errorMessage = string.Format("Unable to build data processing FA application. Entity:'ISIR.FAFSA', Record Id :'{0}'", fafsaId);
                    throw new ArgumentException(errorMessage);
                }
            }
            return fafsaList;
        }


        private async Task<Fafsa> BuildFinancialAidApplication(IsirFafsa isirFafsaData,
            CsAcyr csAcyrData,
            IsirCalcResults isirCalcResultData,
            IsirProfile isirProfileData,
            IsirFafsa isirFafsaNcpData,
            IsirCalcResults isirCalcResultNcpData,
            IsirProfile isirProfileNcpData,
            ProfileNcp profileNcpData,
            IsirFafsa origIsirFafsaData,
            IsirCalcResults origIsirCalcResultData,
            FaSysParams faSysParams)
        {
            Fafsa fafsa = new Fafsa(isirFafsaData.Recordkey, isirFafsaData.IfafStudentId, isirFafsaData.IfafImportYear, isirFafsaData.RecordGuid);
            fafsa.CsFederalIsirId = csAcyrData.CsFedIsirId;
            fafsa.CsInstitutionalIsirId = csAcyrData.CsInstIsirId;
            fafsa.ApplicationCompletedOn = isirFafsaData.IfafDateSign;
            fafsa.StateOfLegalResidence = isirFafsaData.IfafSLegalRes;
            if (isirProfileData != null)
            {
                fafsa.ApplicationCompletedOnProfile = isirProfileData.IproAppReceiptDate;
                fafsa.StateOfLegalResidenceProfile = isirProfileData.IproStuStLegalRes;
            }
            fafsa.Type = isirFafsaData.IfafIsirType;
            fafsa.WorkStudyInterest = isirFafsaData.IfafSIntWorkStudy;
            fafsa.CorrectedFromId = isirFafsaData.IfafCorrectedFromId;

            fafsa.FafsaPrimaryId = isirFafsaData.Recordkey;
            fafsa.FafsaPrimaryType = isirFafsaData.IfafIsirType;
            if (origIsirFafsaData != null)
            {
                if (origIsirFafsaData.Recordkey != null)
                {
                    fafsa.FafsaPrimaryIdCorrected = origIsirFafsaData.Recordkey;
                }
            }

            SaveIndependenceCriteriaData(isirFafsaData, fafsa);

            await SaveHousingCode(isirFafsaData, csAcyrData, faSysParams, fafsa);

            SaveIncomeData(isirFafsaData, isirCalcResultData, isirProfileData, isirCalcResultNcpData, isirProfileNcpData , profileNcpData, origIsirFafsaData, origIsirCalcResultData, fafsa);

            return fafsa;
        }

        private static void SaveIncomeData(IsirFafsa isirFafsaData, IsirCalcResults isirCalcResultData, IsirProfile isirProfileData, IsirCalcResults isirCalcResultNcpData, IsirProfile isirProfileNcpData, ProfileNcp profileNcpData, IsirFafsa origIsirFafsaData, IsirCalcResults origIsirCalcResultsData, Fafsa fafsa)
        {
            // Save applicant/parent income information from fafsa record and cofiles
            fafsa.StudentTaxReturnStatus = isirFafsaData.IfafSTaxReturnFiled;
            fafsa.StudentAdjustedGrossIncome = isirFafsaData.IfafSAgi;
            fafsa.StudentEarnedIncome = isirFafsaData.IfafSStudentInc;
            fafsa.SpouseEarnedIncome = isirFafsaData.IfafSpouseInc;

            fafsa.ParentTaxReturnStatus = isirFafsaData.IfafPTaxReturnFiled;
            fafsa.ParentAdjustedGrossIncome = isirFafsaData.IfafPAgi;
            fafsa.Parent1EarnedIncome = isirFafsaData.IfafPFatherInc;
            fafsa.Parent2EarnedIncome = isirFafsaData.IfafPMotherInc;
            fafsa.Parent1EducationLevel = isirFafsaData.IfafFatherGradeLvl;
            fafsa.Parent2EducationLevel = isirFafsaData.IfafMotherGradeLvl;

            // Save information from calc results cofile
            if (isirCalcResultData != null)
            {
                fafsa.StudentDependencyStatus = isirCalcResultData.IcresDependency;
                fafsa.StudentTotalIncomeProfileCorrected = isirCalcResultData.IcresInasStuTi;
                fafsa.StudentTotalIncomeProfileOrig = isirCalcResultData.IcresImOptStuTi;
                fafsa.StudentTotalIncome = isirCalcResultData.IcresPriSti;
                fafsa.PrimaryTotalIncome = isirCalcResultData.IcresPriTi;
                fafsa.ParentTotalIncomeProfileCorrected = isirCalcResultData.IcresInasParTi;
                fafsa.ParentTotalIncomeProfileOrig = isirCalcResultData.IcresImOptParTi;
                fafsa.ParentPrimaryTotalIncome = isirCalcResultData.IcresPriTi;                
            }

            // Save information from profile cofile
            if (fafsa.Type == "PROF")
            {
                if (isirProfileData != null)
                {
                    // Save independendence criteria from profile record.
                    if (isirProfileData.IproStuLegalDep != null)
                    {
                        if (isirProfileData.IproStuLegalDep.ToUpper() == "Y")
                        {
                            fafsa.HasDependentChildrenProfile = true;
                        }
                        else
                        {
                            fafsa.HasDependentChildrenProfile = false;
                        }
                    }

                    if (isirProfileData.IproStuWardCourt != null)
                    {
                        if (isirProfileData.IproStuWardCourt.ToUpper() == "Y")
                        {
                            fafsa.IsWardProfile = true;
                        }
                        else
                        {
                            fafsa.IsWardProfile = false;
                        }
                    }

                    if (isirProfileData.IproSHomeless != null)
                    {
                        if (isirProfileData.IproSHomeless.ToUpper() == "Y")
                        {
                            fafsa.IsHomelessProfile = true;
                        }
                        else
                        {
                            fafsa.IsHomelessProfile = false;
                        }
                    }

                    if (isirProfileData.IproVeteran != null)
                    {
                        if (isirProfileData.IproVeteran.ToUpper() == "Y")
                        {
                            fafsa.IsVeteranProfile = true;
                        }
                        else
                        {
                            fafsa.IsVeteranProfile = false;
                        }
                    }

                    fafsa.MaritalStatusProfile = isirProfileData.IproStuMaritalStatus;

                    fafsa.StudentTaxReturnStatusProfile = isirProfileData.IproStuTaxRetStat07;
                    fafsa.StudentAdjustedGrossIncomeProfile = isirProfileData.IproStuAdjGrossInc;
                    fafsa.StudentEarnedIncomeProfile = isirProfileData.IproStuWorkInc;
                    fafsa.SpouseEarnedIncomeProfile = isirProfileData.IproSpsWorkInc;

                    fafsa.ParentTaxReturnStatusProfile = isirProfileData.IproParTaxRetStat07;
                    fafsa.ParentTaxReturnStatusProfileNcp = isirProfileNcpData.IproParTaxRetStat07;
                    fafsa.ParentAdjustedGrossIncomeProfile = isirProfileData.IproParAgi;
                    fafsa.ParentAdjustedGrossIncomeProfileNcp = isirProfileNcpData.IproParAgi;
                    fafsa.Parent1EarnedIncomeProfile = isirProfileData.IproFatherWorkInc;
                    fafsa.Parent2EarnedIncomeProfile = isirProfileData.IproMotherWorkInc;
                    fafsa.Parent1EducationLevelProfile = isirProfileData.IproFatherEduLevel;
                    fafsa.Parent2EducationLevelProfile = isirProfileData.IproMotherEduLevel;
                }

                if (isirCalcResultNcpData != null)
                {
                    fafsa.NoncustodialParentTotalIncomeProfile = isirCalcResultNcpData.IcresNcpImOptTi;
                }

                if (profileNcpData != null)
                {
                    fafsa.NoncustodialParent1EarnedIncomeProfile = profileNcpData.PncpEarnedIncome;
                    fafsa.NoncustodialParent2EarnedIncomeProfile = profileNcpData.PncpSpouseEarnedInc;
                }
            }
        }

        private async Task SaveHousingCode(IsirFafsa isirFafsaData, CsAcyr csAcyrData, FaSysParams faSysParams, Fafsa fafsa)
        {
            // Save housing code
            var housingCode = "";
            var office = "";
            var schoolCode = "";
            if (string.IsNullOrEmpty(csAcyrData.CsLocation))
            {
                office = faSysParams.FspMainFaOffice;
            }
            else
            {
                var location = csAcyrData.CsLocation;
                try
                {
                    var faLocationData = (await GetFaLocations()).FirstOrDefault(fal => fal.Recordkey == location);
                    office = faLocationData.FalocFaOffice;
                }
                catch
                {
                    var errorMessage = string.Format("No FA Location '{0}' found for FA office '{1}'. Entity:'ISIR.FAFSA', Record Id :'{2}'", location, office, fafsa.Id);
                    throw new ArgumentException(errorMessage);
                }
            }
            if (!string.IsNullOrEmpty(office))
            {
                try
                {
                    var faOfficeData = (await GetFaOffices()).FirstOrDefault(fao => fao.Recordkey == office);
                    if (faOfficeData != null)
                    {
                        schoolCode = faOfficeData.FaofcTitleIvCode;
                    }
                    if (string.IsNullOrEmpty(schoolCode))
                    {
                        schoolCode = faSysParams.FspTitleIvCode;
                    }
                }
                catch
                {
                    var errorMessage = string.Format("Unable to get FA Office information for '{0}'. Entity:'ISIR.FAFSA', Record Id :'{1}'", office, fafsa.Id);
                    throw new ArgumentException(errorMessage);
                }

            }

            if (!string.IsNullOrEmpty(schoolCode))
            {
                if (schoolCode == isirFafsaData.IfafTitleiv1)
                {
                    housingCode = isirFafsaData.IfafHousing1;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv2)
                {
                    housingCode = isirFafsaData.IfafHousing2;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv3)
                {
                    housingCode = isirFafsaData.IfafHousing3;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv4)
                {
                    housingCode = isirFafsaData.IfafHousing4;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv5)
                {
                    housingCode = isirFafsaData.IfafHousing5;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv6)
                {
                    housingCode = isirFafsaData.IfafHousing6;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv7)
                {
                    housingCode = isirFafsaData.IfafHousing7;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv8)
                {
                    housingCode = isirFafsaData.IfafHousing8;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv9)
                {
                    housingCode = isirFafsaData.IfafHousing9;
                }
                if (schoolCode == isirFafsaData.IfafTitleiv10)
                {
                    housingCode = isirFafsaData.IfafHousing10;
                }
            }
            fafsa.HousingCode = housingCode;
        }

        private static void SaveIndependenceCriteriaData(IsirFafsa isirFafsaData, Fafsa fafsa)
        {
            // Save independence criteria from fafsa record.
            if (isirFafsaData.IfafHomelessAtRisk != null)
            {
                if (isirFafsaData.IfafHomelessAtRisk.ToUpper() == "Y")
                {
                    fafsa.IsAtRiskHomeless = true;
                }
                else
                {
                    fafsa.IsAtRiskHomeless = false;
                }
            }

            if (isirFafsaData.IfafGradProf != null)
            {
                if (isirFafsaData.IfafGradProf.ToUpper() == "Y")
                {
                    fafsa.IsAdvancedDegreeStudent = true;
                }
                else
                {
                    fafsa.IsAdvancedDegreeStudent = false;
                }
            }

            if (isirFafsaData.IfafDependChildren != null)
            {
                if (isirFafsaData.IfafDependChildren.ToUpper() == "Y")
                {
                    fafsa.HasDependentChildren = true;
                }
                else
                {
                    fafsa.HasDependentChildren = false;
                }
            }

            if (isirFafsaData.IfafOtherDepend != null)
            {
                if (isirFafsaData.IfafOtherDepend.ToUpper() == "Y")
                {
                    fafsa.HasOtherDependents = true;
                }
                else
                {
                    fafsa.HasOtherDependents = false;
                }
            }

            if (isirFafsaData.IfafOrphanWard != null)
            {
                if (isirFafsaData.IfafOrphanWard.ToUpper() == "Y")
                {
                    fafsa.IsOrphanOrWard = true;
                }
                else
                {
                    fafsa.IsOrphanOrWard = false;
                }
            }

            if (isirFafsaData.IfafEmancipatedMinor != null)
            {
                if (isirFafsaData.IfafEmancipatedMinor.ToUpper() == "Y")
                {
                    fafsa.IsEmancipatedMinor = true;
                }
                else
                {
                    fafsa.IsEmancipatedMinor = false;
                }
            }

            if (isirFafsaData.IfafLegalGuardianship != null)
            {
                if (isirFafsaData.IfafLegalGuardianship.ToUpper() == "Y")
                {
                    fafsa.HasGuardian = true;
                }
                else
                {
                    fafsa.HasGuardian = false;
                }
            }

            if (isirFafsaData.IfafHomelessBySchool != null)
            {
                if (isirFafsaData.IfafHomelessBySchool.ToUpper() == "Y")
                {
                    fafsa.IsHomelessBySchool = true;
                }
                else
                {
                    fafsa.IsHomelessBySchool = false;
                }
            }

            if (isirFafsaData.IfafHomelessByHud != null)
            {
                if (isirFafsaData.IfafHomelessByHud.ToUpper() == "Y")
                {
                    fafsa.IsHomelessByHud = true;
                }
                else
                {
                    fafsa.IsHomelessByHud = false;
                }
            }

            if (isirFafsaData.IfafBornB4Dt != null)
            {
                if (isirFafsaData.IfafBornB4Dt.ToUpper() == "Y")
                {
                    fafsa.IsBornBeforeDate = true;
                }
                else
                {
                    fafsa.IsBornBeforeDate = false;
                }
            }

            if (isirFafsaData.IfafMarried != null)
            {
                if (isirFafsaData.IfafMarried.ToUpper() == "Y")
                {
                    fafsa.IsMarried = true;
                }
                else
                {
                    fafsa.IsMarried = false;
                }
            }

            if (isirFafsaData.IfafVeteran != null)
            {
                if (isirFafsaData.IfafVeteran.ToUpper() == "Y")
                {
                    fafsa.IsVeteran = true;
                }
                else
                {
                    fafsa.IsVeteran = false;
                }
            }

            if (isirFafsaData.IfafActiveDuty != null)
            {
                if (isirFafsaData.IfafActiveDuty.ToUpper() == "Y")
                {
                    fafsa.IsActiveDuty = true;
                }
                else
                {
                    fafsa.IsActiveDuty = false;
                }
            }
        }

        /// <summary>
        /// Read the FA location used to get FA office for determining housing preference 
        /// </summary>
        /// <returns>fa locations data contract</returns>


        // bma needs to honor bypasscache
        private async Task<Collection<FaLocations>> GetFaLocations()
        {
            if (faLocationDataContracts != null)
            {
                return faLocationDataContracts;
            }
            faLocationDataContracts = await DataReader.BulkReadRecordAsync<FaLocations>("");
            return faLocationDataContracts;
        }

        /// <summary>
        /// Read the FA office used to get school code for determining housing preference 
        /// </summary>
        /// <returns>fa offices data contract</returns>

        // bma needs to honor bypasscache

        private async Task<Collection<FaOffices>> GetFaOffices()
        {
            if (faOfficeDataContracts != null)
            {
                return faOfficeDataContracts;
            }
            faOfficeDataContracts = await DataReader.BulkReadRecordAsync<FaOffices>("");
            return faOfficeDataContracts;
        }


        /// <summary>
        /// Read the FA Sys Parms 
        /// </summary>
        /// <returns>fa offices data contract</returns>

        // bma needs to honor bypasscache

        private async Task<FaSysParams> GetFaSysParms()
        {
            if (faSysParamsDataContracts != null)
            {
                return faSysParamsDataContracts;
            }
            faSysParamsDataContracts = await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
            return faSysParamsDataContracts;
        }
    }
}
