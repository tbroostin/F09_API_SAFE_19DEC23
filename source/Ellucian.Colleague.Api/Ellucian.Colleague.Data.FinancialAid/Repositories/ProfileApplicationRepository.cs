/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository for Profile Applications
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ProfileApplicationRepository : BaseColleagueRepository, IProfileApplicationRepository
    {
        /// <summary>
        /// Constructor for ProfileApplicationRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public ProfileApplicationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }


        /// <summary>
        /// Get all Profile Applications for the given student
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id for whom to get profile applications</param>
        /// <param name="studentAwardYears">The list of studentAwardYears for which to get profile applications for the student</param>
        /// <returns>A list of profile applications for the given student</returns>
        public async Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Info("No awardYears passed to GetProfileApplications in ProfileApplicationsRepository");
                return new List<ProfileApplication>();
            }

            //get CS.ACYR records
            //key is awardYear
            //value is CsAcyr record
            var csAcyrDictionary = new Dictionary<string, CsAcyr>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var acyrFile = "CS." + studentAwardYear.Code;
                var csAcyrRecord = await DataReader.ReadRecordAsync<CsAcyr>(acyrFile, studentId);
                if (csAcyrRecord != null)
                {
                    csAcyrDictionary.Add(studentAwardYear.Code, csAcyrRecord);
                }
            }

            //get the list of ISIR.FAFSA ids from the CS.ACYR records
            var isirFafsaIds = csAcyrDictionary.Where(c => c.Value.CsIsirTransIds != null).SelectMany(c => c.Value.CsIsirTransIds).Distinct();
            if (isirFafsaIds == null || isirFafsaIds.Count() == 0)
            {
                var message = string.Format("No CsIsirTransIds exist in any award year for student {0}", studentId);
                logger.Debug(message);
                return new List<ProfileApplication>();
            }

            //read ISIR.FAFSA
            var isirFafsaRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(isirFafsaIds.ToArray());
            if (isirFafsaRecords == null || isirFafsaRecords.Count() == 0)
            {
                var message = string.Format("Record ids in CS.ISIR.TRANS.IDS for all award years do not exist in ISIR.FASFA for student {0}", studentId);
                logger.Debug(message);
                return new List<ProfileApplication>();
            }

            //select all the Profile records
            var profileRecords = isirFafsaRecords.Where(i => i.IfafIsirType.ToUpper() == "PROF").ToList();

            //loop through the records, grab the linked CsAcyr record and build the ProfileApplication object
            var profileApplications = new List<ProfileApplication>();
            foreach (var profileRecord in profileRecords)
            {
                CsAcyr csAcyrRecord;
                if (csAcyrDictionary.TryGetValue(profileRecord.IfafImportYear, out csAcyrRecord) && profileRecord.IfafStudentId == studentId)
                {
                    try
                    {

                        var profileApplication = new ProfileApplication(profileRecord.Recordkey, profileRecord.IfafImportYear, studentId)
                        {
                            IsFederallyFlagged = (csAcyrRecord.CsFedIsirId == profileRecord.Recordkey),
                            IsInstitutionallyFlagged = (csAcyrRecord.CsInstIsirId == profileRecord.Recordkey),
                            InstitutionalFamilyContribution = (csAcyrRecord.CsInstIsirId == profileRecord.Recordkey) ? csAcyrRecord.CsInstFc : null
                        };

                        if (profileApplication.IsFederallyFlagged && !string.IsNullOrEmpty(csAcyrRecord.CsFc))
                        {
                            int familyContribution;
                            if (int.TryParse(csAcyrRecord.CsFc, out familyContribution))
                            {
                                profileApplication.FamilyContribution = familyContribution;
                            }
                            else
                            {
                                logger.Debug(string.Format("Unable to parse CsFc - {0} - for studentId {1}, awardYear {2}", csAcyrRecord.CsFc, studentId, profileRecord.IfafImportYear));
                            }
                        }

                        profileApplications.Add(profileApplication);
                    }
                    catch (Exception e)
                    {
                        var message = string.Format("Unable to build ProfileApplication object with record id {0}, studentId {1}, awardYear {2}", profileRecord.Recordkey, profileRecord.IfafStudentId, profileRecord.IfafImportYear);
                        logger.Debug(e, message);
                    }
                }
                else
                {
                    logger.Debug(string.Format("Error getting CsAcyr record for student {0}, awardYear {1}. Possible data corruption between CsAcyr and ProfileRecord Id {2}", studentId, profileRecord.IfafImportYear, profileRecord.Recordkey));
                }
            }

            return profileApplications;
        }
    }
}
