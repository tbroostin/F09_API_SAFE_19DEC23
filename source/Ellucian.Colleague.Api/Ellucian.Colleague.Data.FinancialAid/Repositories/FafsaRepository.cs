//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository for Fafsa data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FafsaRepository : BaseColleagueRepository, IFafsaRepository
    {
        private readonly int bulkReadSize;
        /// <summary>
        /// Constructor for the Fafsa Repository
        /// </summary>
        /// <param name="cacheProvider">cacheProvider</param>
        /// <param name="transactionFactory">transactionFactory</param>
        /// <param name="logger">logger</param>
        public FafsaRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.bulkReadSize = (apiSettings != null && apiSettings.BulkReadSize > 0) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a list of all FAFSAs (and any association correction data) for all given students and award years.
        /// Initial Fafsa applications are not returned by this method. The application must have been submitted to, received by
        /// and processed by the Department of Education's Central Processing System.
        /// </summary>
        /// <param name="studentIds">The Colleague PERSON ids of the students for whom to get fafsas</param>
        /// <param name="studentAwardYears">The award years for which to get FAFSA data</param>
        /// <returns>A list of all FAFSAs from the given student ids and award years</returns>
        public async Task<IEnumerable<Fafsa>> GetFafsasAsync(IEnumerable<string> studentIds, IEnumerable<string> awardYears)
        {
            if (studentIds == null || studentIds.Count() == 0)
            {
                logger.Info("No studentIds passed to GetFafsas in FafsaRepository");
                return new List<Fafsa>();
            }

            if (awardYears == null || awardYears.Count() == 0)
            {
                logger.Info("No awardYears passed to GetFafsas in FafsaRepository");
                return new List<Fafsa>();
            }

            //remove duplicates
            var distinctStudentIds = studentIds.Distinct();
            var distinctAwardYears = awardYears.Distinct();

            //get CS.ACYR records
            //key is Tuple(awardYear, studentId)
            //value is csAcyr record
            var csAcyrDictionary = new Dictionary<Tuple<string, string>, CsAcyr>();
            foreach (var awardYear in distinctAwardYears)
            {
                var acyrFile = "CS." + awardYear;
                for (int i = 0; i < distinctStudentIds.Count(); i += bulkReadSize)
                {
                    var subList = distinctStudentIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkData = await DataReader.BulkReadRecordAsync<CsAcyr>(acyrFile, subList);
                    if (bulkData != null && bulkData.Count > 0)
                    {
                        foreach (var csRecord in bulkData)
                        {
                            if (csRecord != null)
                            {
                                csAcyrDictionary.Add(new Tuple<string, string>(awardYear, csRecord.Recordkey), csRecord);
                            }
                        }
                    }
                }
            }

            //get the list of ISIR.FAFSA ids from the CS.ACYR records
            var isirFafsaIds = csAcyrDictionary.Where(c => c.Value.CsIsirTransIds != null).SelectMany(c => c.Value.CsIsirTransIds).Distinct();
            if (isirFafsaIds == null || isirFafsaIds.Count() == 0)
            {
                var message = string.Format("No CsIsirTransIds exist for the given lists of students and awardYears");
                logger.Info(message);
                return new List<Fafsa>();
            }

            //read ISIR.FAFSA. 
            var allIsirFafsaRecords = await LimitBulkReadAsync<IsirFafsa>(isirFafsaIds.ToArray());
            if (allIsirFafsaRecords == null || allIsirFafsaRecords.Count() == 0)
            {
                var message = string.Format("Record Ids in CS.ISIR.TRANS.IDS for students and award years do not exist in ISIR.FAFSA");
                logger.Info(message);
                return new List<Fafsa>();
            }

            //filter out any records with an IAPP (Initial Application) or PROF (profile) isir type
            allIsirFafsaRecords = allIsirFafsaRecords.Where(i => i.IfafIsirType.ToUpper() != "PROF" && i.IfafIsirType.ToUpper() != "IAPP").AsEnumerable();

            //read the non-profile ISIR.RESULTS, but we don't care if they don't exist - just log a message. ISIR.FAFSA is the driver
            var allIsirResultsRecords = await LimitBulkReadAsync<IsirResults>(allIsirFafsaRecords.Select(i => i.Recordkey).ToArray());
            if (allIsirResultsRecords == null || allIsirResultsRecords.Count() == 0)
            {
                var message = string.Format("Record Ids in CS.ISIR.TRANS.IDS for for students and award years do not exist in ISIR.RESULTS");
                logger.Info(message);
                allIsirResultsRecords = new List<IsirResults>();
            }

            //get the correction records from the FAFSAs that have corrections
            var correctionIds = allIsirFafsaRecords.Where(i => !string.IsNullOrEmpty(i.IfafCorrectionId)).Select(i => i.IfafCorrectionId).ToArray();
            var allIsirFafsaCorrectionRecords = await LimitBulkReadAsync<IsirFafsa>(correctionIds);
            //var allIsirResultCorrectionRecords = LimitBulkRead<IsirResults>(correctionIds);

            if (allIsirFafsaCorrectionRecords == null) { allIsirFafsaCorrectionRecords = new List<IsirFafsa>(); }

            //create a FAFSA object for each isirFafsa record
            var fafsaEntities = new List<Fafsa>();

            foreach (var isirFafsaRecord in allIsirFafsaRecords)
            {
                //get the CS.ACYR record for this ISIR's studentId and award year
                CsAcyr csAcyrRecord;
                csAcyrDictionary.TryGetValue(new Tuple<string, string>(isirFafsaRecord.IfafImportYear, isirFafsaRecord.IfafStudentId), out csAcyrRecord);

                var isirFafsaToUse = isirFafsaRecord;
                var isirResultToUse = allIsirResultsRecords.FirstOrDefault(i => i.Recordkey == isirFafsaRecord.Recordkey);

                //does this record have a correction?
                if (!string.IsNullOrEmpty(isirFafsaRecord.IfafCorrectionId))
                {
                    isirFafsaToUse = allIsirFafsaCorrectionRecords.FirstOrDefault(i => i.Recordkey == isirFafsaRecord.IfafCorrectionId);
                }

                //finally build the fafsa object
                if (isirFafsaToUse != null)
                {
                    try
                    {
                        fafsaEntities.Add(
                            BuildFafsa(csAcyrRecord, isirFafsaToUse, isirResultToUse));
                    }
                    catch (Exception e)
                    {
                        var message = string.Format("Unable to build Fafsa object with record id {0}, studentId {1}, awardYear {2}.", isirFafsaToUse.Recordkey, isirFafsaToUse.IfafStudentId, isirFafsaToUse.IfafImportYear);
                        logger.Info(e, message);
                    }
                }
            }

            return fafsaEntities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYears"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProfileEFC>> GetEfcAsync(string studentId, IEnumerable<StudentAwardYear> awardYears)
        {
            if (studentId == null)
            {
                logger.Info("No studentIds passed to GetFafsas in FafsaRepository");
                throw new ArgumentNullException("studentIds");
            }

            if (awardYears == null || awardYears.Count() == 0)
            {
                logger.Info("No awardYears passed to GetFafsas in FafsaRepository");
                throw new ArgumentNullException("awardYears");
            }

            var distinctAwardYears = awardYears.Distinct();
            var profileEfcList = new List<ProfileEFC>();
            var isirFafsaListProf = new List<IsirFafsa>();

            foreach (var awardYear in distinctAwardYears)
            {
                var acyrFile = "CS." + awardYear;
                var csRecord = await DataReader.ReadRecordAsync<CsAcyr>(acyrFile, studentId);
                if (csRecord != null)
                {
                    if (csRecord != null)
                    {
                        if (!string.IsNullOrEmpty(csRecord.CsInstIsirId))
                        {
                            var isirFafsas = await DataReader.ReadRecordAsync<IsirFafsa>(csRecord.CsInstIsirId);
                            if (isirFafsas != null)
                            {
                                if (isirFafsas.IfafIsirType == "PROF")
                                {
                                    isirFafsaListProf.Add(isirFafsas);
                                    if (isirFafsaListProf != null && isirFafsaListProf.Any())
                                    {
                                        profileEfcList.Add(new ProfileEFC(studentId, awardYear.Code, csRecord.CsInstFc));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return profileEfcList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYears"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProfileEFC>> GetFafsaEfcAsync(string studentId, IEnumerable<StudentAwardYear> awardYears)
        {
            if (studentId == null)
            {
                logger.Info("No studentIds passed to GetFafsas in FafsaRepository");
                throw new ArgumentNullException("studentIds");
            }

            if (awardYears == null || awardYears.Count() == 0)
            {
                logger.Info("No awardYears passed to GetFafsas in FafsaRepository");
                throw new ArgumentNullException("awardYears");
            }

            var distinctAwardYears = awardYears.Distinct();
            var fafsaEfcList = new List<ProfileEFC>();
            var isirFafsaList = new List<IsirFafsa>();

            foreach (var awardYear in distinctAwardYears)
            {
                var acyrFile = "CS." + awardYear;
                var csRecord = await DataReader.ReadRecordAsync<CsAcyr>(acyrFile, studentId);
                if (csRecord != null)
                {
                    if (csRecord != null)
                    {
                        if (!string.IsNullOrEmpty(csRecord.CsFedIsirId))
                        {
                            var isirFafsas = await DataReader.ReadRecordAsync<IsirFafsa>(csRecord.CsFedIsirId);
                            if (isirFafsas != null)
                            {
                                if (isirFafsas.IfafIsirType == "ISIR" || isirFafsas.IfafIsirType == "CPSSG")
                                {
                                    isirFafsaList.Add(isirFafsas);
                                    if (isirFafsaList != null && isirFafsaList.Any())
                                    {
                                        fafsaEfcList.Add(new ProfileEFC(studentId, awardYear.Code, Convert.ToInt32(csRecord.CsFc)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return fafsaEfcList;
        }

        /// <summary>
        /// Helper method to bulk read records using a read size limiter.
        /// </summary>
        /// <typeparam name="T">IColleagueEntity as a DataContract</typeparam>
        /// <param name="recordIds">Array of record ids</param>
        /// <returns>An enumeration of DataContracts of type T</returns>
        private async Task<IEnumerable<T>> LimitBulkReadAsync<T>(string[] recordIds)
            where T : class, IColleagueEntity
        {
            var returnList = new List<T>();
            for (int i = 0; i < recordIds.Count(); i += bulkReadSize)
            {
                var subList = recordIds.Skip(i).Take(bulkReadSize).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<T>(subList);
                if (bulkData != null && bulkData.Count > 0)
                {
                    returnList.AddRange(bulkData);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Get the federally flagged fafsa data for a group of students.
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="awardYear">Award Year to get FAFSA data for.</param>
        /// <param name="term">Term to find award year if year is not specified.</param>
        /// <returns>List of Fafsa Objects for each Student</returns>
        public async Task<IEnumerable<Fafsa>> GetFafsaByStudentIdsAsync(IEnumerable<string> studentIds, string awardYear)
        {
            if (studentIds == null || studentIds.Count() <= 0)
            {
                throw new ArgumentNullException("studentIds");
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "The award year must be specified for Fafsa Data retrieval.");
            }

            List<Fafsa> fafsaDataRecords = new List<Fafsa>();

            ICollection<IsirFafsa> isirFafsaData = new List<IsirFafsa>();
            ICollection<IsirResults> isirResultsData = new List<IsirResults>();

            var csAcyrFile = "CS." + awardYear;

            //get CS.ACYR records
            //key is Tuple(awardYear, studentId)
            //value is csAcyr record
            var csAcyrDictionary = new Dictionary<Tuple<string, string>, CsAcyr>();
            var acyrFile = "CS." + awardYear;
            for (int i = 0; i < studentIds.Count(); i += bulkReadSize)
            {
                var subList = studentIds.Skip(i).Take(bulkReadSize).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<CsAcyr>(acyrFile, subList);
                if (bulkData != null && bulkData.Count > 0)
                {
                    foreach (var csRecord in bulkData)
                    {
                        if (csRecord != null)
                        {
                            csAcyrDictionary.Add(new Tuple<string, string>(awardYear, csRecord.Recordkey), csRecord);
                        }
                    }
                }
            }
            bool error = false;
            var isirFafsaIds = csAcyrDictionary.Where(c => !string.IsNullOrEmpty(c.Value.CsFedIsirId)).Select(c => c.Value.CsFedIsirId);
            if (isirFafsaIds != null && isirFafsaIds.Count() > 0)
            {
                var allIsirFafsaRecords = await LimitBulkReadAsync<IsirFafsa>(isirFafsaIds.ToArray());
                var allIsirResults = await LimitBulkReadAsync<IsirResults>(isirFafsaIds.ToArray());

                var allCorrectionIds = allIsirFafsaRecords.Where(i => !string.IsNullOrEmpty(i.IfafCorrectionId)).Select(i => i.IfafCorrectionId);
                var allIsirFafsaCorrections = await LimitBulkReadAsync<IsirFafsa>(allCorrectionIds.ToArray());

                if (allIsirFafsaRecords != null && allIsirFafsaRecords.Any())
                {
                    foreach (var studentId in studentIds)
                    {
                        try
                        {
                            //get the CS.ACYR record for this ISIR's studentId and award year
                            CsAcyr studentCsRecord;
                            csAcyrDictionary.TryGetValue(new Tuple<string, string>(awardYear, studentId), out studentCsRecord);

                            if (studentCsRecord != null)
                            {
                                IsirFafsa isirFafsa = allIsirFafsaRecords.FirstOrDefault(f => f.Recordkey == studentCsRecord.CsFedIsirId);
                                IsirResults isirResult = null;
                                if (allIsirResults != null && allIsirResults.Any())
                                {
                                    isirResult = allIsirResults.FirstOrDefault(f => f.Recordkey == studentCsRecord.CsFedIsirId);
                                }

                                // Read in the Corrected Fafsa Record if we have one
                                if (isirFafsa != null && !string.IsNullOrEmpty(isirFafsa.IfafCorrectionId))
                                {
                                    var correctionId = isirFafsa.IfafCorrectionId;
                                    isirFafsa = allIsirFafsaCorrections.FirstOrDefault(i => i.Recordkey == correctionId);
                                }
                                try
                                {
                                    Fafsa fafsaData = BuildFafsa(studentId, awardYear, studentCsRecord, isirFafsa, isirResult);
                                    fafsaDataRecords.Add(fafsaData);
                                }
                                catch (Exception e)
                                {
                                    var message =
                                        (isirFafsa == null) ?
                                        string.Format("IsirFafsa record is null. Possible database corruption with correction record for studentId {0}", studentId) :
                                        string.Format("Unable to build Fafsa object with record id {0}, studentId {1}, awardYear {2}", isirFafsa.Recordkey, studentId, isirFafsa.IfafImportYear);
                                    logger.Info(e, message);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to build student {0} for fafsa", studentId));
                            logger.Error(e.GetBaseException().Message);
                            logger.Error(e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && fafsaDataRecords.Count() == 0)
                throw new Exception("Unexpected errors occurred. No fafsa records returned. Check API error log.");
            return fafsaDataRecords;
        }

        private Fafsa BuildFafsa(CsAcyr studentCsRecord, IsirFafsa isirFafsaRecord, IsirResults isirResultsRecord)
        {
            return BuildFafsa(isirFafsaRecord.IfafStudentId, isirFafsaRecord.IfafImportYear, studentCsRecord, isirFafsaRecord, isirResultsRecord);
        }

        private Fafsa BuildFafsa(string studentId, string awardYear, CsAcyr studentCsRecord, IsirFafsa isirFafsaRecord, IsirResults isirResultRecord)
        {
            Fafsa fafsaEntity = new Fafsa(isirFafsaRecord.Recordkey, awardYear, studentId);

            fafsaEntity.ParentsAdjustedGrossIncome = isirFafsaRecord.IfafPAgi;
            fafsaEntity.StudentsAdjustedGrossIncome = isirFafsaRecord.IfafSAgi;

            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv1, isirFafsaRecord.IfafHousing1, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv2, isirFafsaRecord.IfafHousing2, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv3, isirFafsaRecord.IfafHousing3, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv4, isirFafsaRecord.IfafHousing4, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv5, isirFafsaRecord.IfafHousing5, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv6, isirFafsaRecord.IfafHousing6, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv7, isirFafsaRecord.IfafHousing7, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv8, isirFafsaRecord.IfafHousing8, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv9, isirFafsaRecord.IfafHousing9, fafsaEntity);
            AddHousingCodesKeyValuePair(isirFafsaRecord.IfafTitleiv10, isirFafsaRecord.IfafHousing10, fafsaEntity);

            if (isirResultRecord != null)
            {
                if (isirResultRecord.IresCpsPellElig.ToUpper() == "Y") fafsaEntity.IsPellEligible = true;
            }

            //if this is the federally flagged isir, or this is the correction of the federally flagged isir,
            //indicate that this ISIR is federally flagged.
            if (studentCsRecord != null && !string.IsNullOrEmpty(studentCsRecord.CsFedIsirId) &&
                (studentCsRecord.CsFedIsirId == isirFafsaRecord.Recordkey || studentCsRecord.CsFedIsirId == isirFafsaRecord.IfafCorrectedFromId))
            {
                fafsaEntity.IsFederallyFlagged = true;

                if (!string.IsNullOrEmpty(studentCsRecord.CsFc))
                {
                    int familyContribution;
                    if (int.TryParse(studentCsRecord.CsFc, out familyContribution))
                    {
                        fafsaEntity.FamilyContribution = familyContribution;
                    }
                    else
                    {
                        logger.Info("Unable to parse CsFc - {0} - for studentId {1}, awardYear {2}", studentCsRecord.CsFc, studentId, awardYear);
                    }
                }
            }

            //if this is the institutionally flagged isir, or this is a correction of the institutionally flagged isir, 
            //indicate that this ISIR is institutionally flagged
            if (studentCsRecord != null && !string.IsNullOrEmpty(studentCsRecord.CsInstIsirId) &&
                (studentCsRecord.CsInstIsirId == isirFafsaRecord.Recordkey || studentCsRecord.CsInstIsirId == isirFafsaRecord.IfafCorrectedFromId))
            {
                fafsaEntity.IsInstitutionallyFlagged = true;
                fafsaEntity.InstitutionalFamilyContribution = studentCsRecord.CsInstFc;
            }
            return fafsaEntity;
        }

        /// <summary>
        /// Adds key value pairs to the HousingCodes dictionary
        /// </summary>
        /// <param name="isirFafsa">received ISIR FAFSA record</param>
        /// <param name="fafsaEntity">created student's FAFSA record</param>
        private static void AddHousingCodesKeyValuePair(string titleIV, string housingCode, Fafsa fafsaEntity)
        {
            if (!string.IsNullOrEmpty(titleIV) && !fafsaEntity.HousingCodes.ContainsKey(titleIV))
            {
                switch (housingCode)
                {
                    case "1":
                        fafsaEntity.HousingCodes.Add(titleIV, HousingCode.OnCampus);
                        break;
                    case "2":
                        fafsaEntity.HousingCodes.Add(titleIV, HousingCode.WithParent);
                        break;
                    case "3":
                        fafsaEntity.HousingCodes.Add(titleIV, HousingCode.OffCampus);
                        break;
                }
            }
        }
    }
}
