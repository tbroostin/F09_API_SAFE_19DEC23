/*Copyright 2014-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// StudentLoanSummaryRepository creates StudentLoanSummary objects by pulling data from various Student Loan records in Colleague
    /// 
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentLoanSummaryRepository : BaseColleagueRepository, IStudentLoanSummaryRepository
    {
        //subsidized interview and mpn type code account for both sub and unsub loans (direct loans)
        private const string subsidizedInterviewCategoryCode = "S";
        private const string plusInterviewCategoryCode = "P";
        private const string subsidizedMpnTypeCode = "M";
        private const string plusMpnTypeCode = "N";

        //Arrays of acceptable loan types for direct and plus loans (when looking through NSLDS records for interview dates)
        private readonly string[] directLoanTypes = new string[14] { "CS", "CU", "D0", "D1", "D2", "D5", "D6", "D8", "D9", "RF", "SF", "SL", "SN", "SU" };
        private readonly string[] plusLoanTypes = new string[2] { "GB", "D3" };

        /// <summary>
        /// StudentLoanSummaryRepository constructor for injection-framework
        /// </summary>
        /// <param name="cacheProvider">ICacheProvider object</param>
        /// <param name="transactionFactory">IColleagueTransactionFactory object</param>
        /// <param name="logger">ILogger object</param>
        public StudentLoanSummaryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get a StudentLoanSummary object for the given student id
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>StudentLoanSummary object containing data derived from the Colleague database</returns>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when student does not have Financial Aid records</exception>
        public async Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            //Read the FIN.AID record. If no FIN.AID record exists, throw an exception Use this to validate
            //that the FA module "knows" this student.
            var studentRecordData = await DataReader.ReadRecordAsync<FinAid>(studentId);
            if (studentRecordData == null)
            {
                throw new KeyNotFoundException(string.Format("Student Id {0} does not have Financial Aid data", studentId));
            }

            var studentLoanSummary = new StudentLoanSummary(studentId);

            var isirNsldsRecords = await GetNsldsRecordsAsync(studentRecordData);

            var systemParametersData = await GetSystemParametersDataAsync();
            var UseMyInterviewsOnly = systemParametersData.FspUseFaInterviewsOnly;

            //Get interview dates
            if (studentRecordData.FaInterviews != null && studentRecordData.FaInterviews.Count() > 0)
            {
                var studentInterviewRecords = await DataReader.BulkReadRecordAsync<FaInterview>(studentRecordData.FaInterviews.ToArray());

                if (studentInterviewRecords != null && studentInterviewRecords.Count() > 0)
                {
                    studentLoanSummary.DirectLoanEntranceInterviewDate = await GetInterviewDateAsync(studentInterviewRecords, subsidizedInterviewCategoryCode);
                    studentLoanSummary.GraduatePlusLoanEntranceInterviewDate = await GetInterviewDateAsync(studentInterviewRecords, plusInterviewCategoryCode);
                }
            }

            // If we have the parameter set to look only at LEEI Interviews skip looking at the NSLDS info.
            if (UseMyInterviewsOnly != "Y")
            {
                // If we don't have Interview date for direct loans
                if (studentLoanSummary.DirectLoanEntranceInterviewDate == null)
                {
                    if (isirNsldsRecords != null && isirNsldsRecords.Any())
                    {
                        studentLoanSummary.DirectLoanEntranceInterviewDate = await GetNsldsDateAsync(isirNsldsRecords, subsidizedInterviewCategoryCode);
                    }
                }

                //If we do not have interview date for plus loans
                if (studentLoanSummary.GraduatePlusLoanEntranceInterviewDate == null)
                {
                    if (isirNsldsRecords != null && isirNsldsRecords.Any())
                    {
                        studentLoanSummary.GraduatePlusLoanEntranceInterviewDate = await GetNsldsDateAsync(isirNsldsRecords, plusInterviewCategoryCode);
                    }
                }
            }
            //Get Mpn expiration dates
            var plusLoanMpnItems = new List<PlusMpnItem>();
            if (!string.IsNullOrEmpty(studentRecordData.FaCodPersonId))
            {
                var codPersonRecord = await DataReader.ReadRecordAsync<CodPerson>(studentRecordData.FaCodPersonId);
                if (codPersonRecord != null && codPersonRecord.CodpCodMpnIds != null && codPersonRecord.CodpCodMpnIds.Count() > 0)
                {
                    var studentMpnRecords = await DataReader.BulkReadRecordAsync<CodMpn>(codPersonRecord.CodpCodMpnIds.ToArray());
                    if (studentMpnRecords != null && studentMpnRecords.Count() > 0)
                    {
                        var directLoanMpnExpirationDate = GetActiveMpnExpirationDate(studentId, studentMpnRecords, subsidizedMpnTypeCode);
                        var graduatePlusLoanMpnExpirationDate = GetActiveMpnExpirationDate(studentId, studentMpnRecords, plusMpnTypeCode, "GRAD");
                        if (directLoanMpnExpirationDate != null && directLoanMpnExpirationDate.Any())
                        {
                            studentLoanSummary.DirectLoanMpnExpirationDate = directLoanMpnExpirationDate.FirstOrDefault();
                        }
                        if (graduatePlusLoanMpnExpirationDate != null && graduatePlusLoanMpnExpirationDate.Any())
                        {
                            studentLoanSummary.GraduatePlusLoanMpnExpirationDate = graduatePlusLoanMpnExpirationDate.FirstOrDefault();
                        }
                        var plusLoanDates = GetActiveMpnExpirationDate(studentId, studentMpnRecords, plusMpnTypeCode, "PLUS");
                        var plusLoanPersonIDs = GetPlusLoanMpnNames(studentId, studentMpnRecords, plusMpnTypeCode, "PLUS");
                        if (plusLoanDates != null && plusLoanDates.Count > 0 && plusLoanPersonIDs != null && plusLoanPersonIDs.Count > 0)
                        {
                            if (plusLoanDates[0] != null && plusLoanPersonIDs[0] != null)
                            {
                                var plusLoanMpnItem = new PlusMpnItem();
                                plusLoanMpnItem.PlusLoanMpnExpirationDate = plusLoanDates[0];
                                var translatedPerson = await DataReader.ReadRecordAsync<CodPerson>(plusLoanPersonIDs[0]);
                                plusLoanMpnItem.PlusLoanMpnPersonId = translatedPerson.CodpColleagueId;
                                plusLoanMpnItems.Add(plusLoanMpnItem);
                            }
                            if (plusLoanDates.Count > 1)
                            {
                                if (plusLoanDates[1] != null && plusLoanPersonIDs[1] != null)
                                {
                                    var plusLoanMpnItem = new PlusMpnItem();
                                    plusLoanMpnItem.PlusLoanMpnExpirationDate = plusLoanDates[1];
                                    var translatedPerson = await DataReader.ReadRecordAsync<CodPerson>(plusLoanPersonIDs[1]);
                                    plusLoanMpnItem.PlusLoanMpnPersonId = translatedPerson.CodpColleagueId;
                                    plusLoanMpnItems.Add(plusLoanMpnItem);
                                }
                            }
                        }
                        studentLoanSummary.PlusMpnItems = plusLoanMpnItems;
                        //studentLoanSummary.PlusLoanMpnExpirationDate = GetActiveMpnExpirationDate(studentId, studentMpnRecords, plusMpnTypeCode, "PLUS");
                    }
                }
            }

            //Get InformedBorrower status
            // call transaction
            var informedBorrowerItems = new List<InformedBorrowerItem>();
            var plusLoanItems = new List<PlusLoanItem>();
            var plusAppItems = new List<PlusApplicationItem>();
            foreach (var faYear in studentRecordData.FaSaYears)
            {

                var request = new GetInformedBorrowerRequest();
                request.StudentId = studentId;
                request.FaYear = faYear;
                var informedBorrowerItem = new InformedBorrowerItem();
                informedBorrowerItem.FaYear = faYear;

                var request2 = new GetPlusApplInfoRequest();
                request2.StudentId = studentId;
                request2.FaYear = faYear;
                var response2 = await transactionInvoker.ExecuteAsync<GetPlusApplInfoRequest, GetPlusApplInfoResponse>(request2);

                var response = await transactionInvoker.ExecuteAsync<GetInformedBorrowerRequest, GetInformedBorrowerResponse>(request);

                if (response != null)
                {
                    if (response.AInfBorrResult == "Y")
                    {
                        informedBorrowerItem.IsInformedBorrowerComplete = true;
                        informedBorrowerItems.Add(informedBorrowerItem);
                    }
                    if (string.IsNullOrEmpty(response.AInfBorrResult) || response.AInfBorrResult == "N")
                    {
                        informedBorrowerItem.IsInformedBorrowerComplete = false;
                        informedBorrowerItems.Add(informedBorrowerItem);
                    }
                    if (!string.IsNullOrEmpty(response.PlusResult1))
                    {
                        if (response.PlusResult1 == "Y" && !string.IsNullOrEmpty(response.AP1LastName))
                        {
                            var plusLoanItem = new PlusLoanItem();
                            plusLoanItem.FaYear = faYear;
                            if (!string.IsNullOrEmpty(response.AP1LastName))
                            {
                                plusLoanItem.PlusLoanLastName = response.AP1LastName;
                            }
                            if (!string.IsNullOrEmpty(response.AP1FirstName))
                            {
                                plusLoanItem.PlusLoanFirstName = response.AP1FirstName;
                            }
                            plusLoanItem.IsPlusLoanItemComplete = true;
                            plusLoanItem.FaYear = faYear;
                            plusLoanItems.Add(plusLoanItem);
                        }
                        if (response.PlusResult1 == "N" && !string.IsNullOrEmpty(response.AP1FirstName))
                        {
                            var plusLoanItem = new PlusLoanItem();
                            plusLoanItem.IsPlusLoanItemComplete = false;
                            plusLoanItem.FaYear = faYear;
                            plusLoanItem.PlusLoanFirstName = response.AP1FirstName;
                            plusLoanItem.PlusLoanLastName = response.AP1LastName;
                            plusLoanItems.Add(plusLoanItem);
                        }
                        if (response.PlusResult1 == "N" && string.IsNullOrEmpty(response.AP1FirstName))
                        {
                            var plusLoanItem = new PlusLoanItem();
                            plusLoanItem.IsPlusLoanItemComplete = false;
                            plusLoanItem.FaYear = faYear;
                            plusLoanItems.Add(plusLoanItem);
                        }
                    }
                    if (!string.IsNullOrEmpty(response.APlusRslt2))
                    {
                        if (response.APlusRslt2 == "Y" && !string.IsNullOrEmpty(response.AP2FirstName))
                        {
                            var plusLoanItem2 = new PlusLoanItem();
                            if (!string.IsNullOrEmpty(response.AP2LastName))
                            {
                                plusLoanItem2.PlusLoanLastName = response.AP2LastName;
                            }
                            if (!string.IsNullOrEmpty(response.AP2FirstName))
                            {
                                plusLoanItem2.PlusLoanFirstName = response.AP2FirstName;
                            }
                            plusLoanItem2.FaYear = faYear;
                            plusLoanItem2.IsPlusLoanItemComplete = true;
                            plusLoanItems.Add(plusLoanItem2);
                        }
                        if (response.APlusRslt2 == "N" && !string.IsNullOrEmpty(response.AP2FirstName))
                        {
                            var plusLoanItem2 = new PlusLoanItem();
                            plusLoanItem2.IsPlusLoanItemComplete = false;
                            plusLoanItem2.PlusLoanFirstName = response.AP2FirstName;
                            plusLoanItem2.PlusLoanLastName = response.AP2LastName;
                            plusLoanItem2.FaYear = faYear;
                            plusLoanItems.Add(plusLoanItem2);
                        }
                        if (response.APlusRslt2 == "N" && string.IsNullOrEmpty(response.AP2FirstName))
                        {
                            var plusLoanItem2 = new PlusLoanItem();
                            plusLoanItem2.IsPlusLoanItemComplete = false;
                            plusLoanItem2.FaYear = faYear;
                            plusLoanItems.Add(plusLoanItem2);
                        }
                    }
                }

                if (response2 != null)
                {
                    if (!string.IsNullOrEmpty(response2.PlusResult1))
                    {
                        if (response2.PlusResult1 == "Y" && !string.IsNullOrEmpty(response2.AP1FirstName))
                        {
                            var plusAppItem = new PlusApplicationItem();
                            plusAppItem.FaYear = faYear;
                            if (!string.IsNullOrEmpty(response2.AP1LastName))
                            {
                                plusAppItem.PlusApplicationLastName = response.AP1LastName;
                            }
                            if (!string.IsNullOrEmpty(response2.AP1FirstName))
                            {
                                plusAppItem.PlusApplicationFirstName = response.AP1FirstName;
                            }
                            plusAppItem.IsLoanApplicationItemComplete = true;
                            plusAppItem.FaYear = faYear;
                            plusAppItem.IsGPLUS = false;
                            plusAppItems.Add(plusAppItem);
                        }
                        if (response2.PlusResult1 == "N")
                        {
                            var plusAppItem = new PlusApplicationItem();
                            plusAppItem.IsLoanApplicationItemComplete = false;
                            plusAppItem.FaYear = faYear;
                            plusAppItem.IsGPLUS = false;
                            plusAppItems.Add(plusAppItem);
                        }
                    }
                    if (!string.IsNullOrEmpty(response2.APlusRslt2))
                    {
                        if (response2.APlusRslt2 == "Y" && !string.IsNullOrEmpty(response2.AP1FirstName))
                        {
                            var plusAppItem2 = new PlusApplicationItem();
                            if (!string.IsNullOrEmpty(response2.AP2LastName))
                            {
                                plusAppItem2.PlusApplicationLastName = response2.AP2LastName;
                            }
                            if (!string.IsNullOrEmpty(response2.AP2FirstName))
                            {
                                plusAppItem2.PlusApplicationFirstName = response2.AP2FirstName;
                            }
                            plusAppItem2.FaYear = faYear;
                            plusAppItem2.IsLoanApplicationItemComplete = true;
                            plusAppItem2.IsGPLUS = false;
                            plusAppItems.Add(plusAppItem2);
                        }
                        if (response2.APlusRslt2 == "N")
                        {
                            var plusAppItem2 = new PlusApplicationItem();
                            plusAppItem2.IsLoanApplicationItemComplete = false;
                            plusAppItem2.FaYear = faYear;
                            plusAppItem2.IsGPLUS = false;
                            plusAppItems.Add(plusAppItem2);
                        }
                    }
                    if (!string.IsNullOrEmpty(response2.GplusRslt))
                    {
                        if (response2.GplusRslt == "Y")
                        {
                            var plusAppItem3 = new PlusApplicationItem();
                            plusAppItem3.PlusApplicationFirstName = "";
                            plusAppItem3.PlusApplicationLastName = "";
                            plusAppItem3.FaYear = faYear;
                            plusAppItem3.IsLoanApplicationItemComplete = true;
                            plusAppItem3.IsGPLUS = true;
                            plusAppItems.Add(plusAppItem3);
                        }
                        if (response2.GplusRslt == "N")
                        {
                            var plusAppItem3 = new PlusApplicationItem();
                            plusAppItem3.PlusApplicationFirstName = "";
                            plusAppItem3.PlusApplicationLastName = "";
                            plusAppItem3.FaYear = faYear;
                            plusAppItem3.IsLoanApplicationItemComplete = false;
                            plusAppItem3.IsGPLUS = true;
                            plusAppItems.Add(plusAppItem3);
                        }
                    }
                }
            }

            studentLoanSummary.PlusLoanItems = plusLoanItems;
            studentLoanSummary.PlusApplicationItems = plusAppItems;
            studentLoanSummary.InformedBorrowerItem = informedBorrowerItems;


            //Get StudentLoanHistory
            // Do I have any ISIR.NSLDS records already read?

            if (isirNsldsRecords != null && isirNsldsRecords.Any())
            //if (studentRecordData.FaIsirNsldsIds != null && studentRecordData.FaIsirNsldsIds.Count() > 0)
            {
                //Get the first ISIR.NSLDS record from the list in Fin.Aid
                var isirNsldsRecord = isirNsldsRecords.FirstOrDefault();
                if (isirNsldsRecord != null)
                {
                    //Assign the aggregate principal loan amount
                    studentLoanSummary.StudentLoanCombinedTotalAmount = isirNsldsRecord.InsdAgCombPrBal.HasValue ? isirNsldsRecord.InsdAgCombPrBal.Value : 0;

                    if (isirNsldsRecord.InsdLoanIds != null && isirNsldsRecord.InsdLoanIds.Count() > 0)
                    {
                        var isirNsldsLoanRecords = await DataReader.BulkReadRecordAsync<IsirNsldsLoan>(isirNsldsRecord.InsdLoanIds.ToArray());
                        if (isirNsldsLoanRecords != null && isirNsldsLoanRecords.Count() > 0)
                        {
                            foreach (var singleLoanRecord in isirNsldsLoanRecords)
                            {
                                if (singleLoanRecord != null &&
                                    !string.IsNullOrEmpty(singleLoanRecord.InsdlSchoolCode) &&
                                    singleLoanRecord.InsdlAggrPrinBal.HasValue)
                                {

                                    studentLoanSummary.AddOrUpdateLoanHistory(singleLoanRecord.InsdlSchoolCode, singleLoanRecord.InsdlAggrPrinBal.Value);
                                }
                            }
                        }
                    }
                }
            }

            return studentLoanSummary;
        }

        /// <summary>
        /// Helper method to retrieve a student's active Master Promissory Note expiration date for the given mpnType code
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON Id used to report errors</param>
        /// <param name="studentMpnRecords">Collection of the student's CodMpn data contract objects</param>
        /// <param name="mpnTypeCode">Indicates the type of Master Promissory Note for which to get the expiration date. M - Subsidized/Unsubsidized Master Promissory Notes; N - PLUS Master Promissory Notes</param>
        /// <returns>A DateTime object representing the student's active Master Promissory Note expiration date for the given MPN type. If student has
        /// no active Master Promissory Notes, or there was an error retrieving the Master Promissory Note, this method returns null.</returns>        
        private List<DateTime?> GetActiveMpnExpirationDate(string studentId, Collection<CodMpn> studentMpnRecords, string mpnTypeCode, string plusFlag = null)
        {

            //Select Mpns that match the given mpnTypeCode (the 10th character in the RecordKey indiciates the type - M=SUB/UNSUB, N=PLUS)
            //and that are active
            //and that have non-null/empty date/time stamps

            //This next logic mimics the logic of the Colleague computed column S.FA.CC.POTENTIAL.MPN.STATUS
            // Check for an Active Status first, then Pending, then use the Old Active Status

            //Find the MPNs of the correct Loan Type and are Active

            IEnumerable<CodMpn> typeSpecificActiveMpnsFiltered = null;
            var activeMpnExpirationDates = new List<DateTime?>();

            if (plusFlag != null && plusFlag == "PLUS")
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (plusFlag != null && plusFlag == "GRAD")
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (plusFlag == null)
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (typeSpecificActiveMpnsFiltered != null && typeSpecificActiveMpnsFiltered.Count() > 0)
            {
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificActiveMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnExpirationDates.Add(mpn.CodmMpnExpDate);
                        }
                    }
                    return activeMpnExpirationDates;
                }
            }

            //If this is a PLUS or GPLUS and there is an Endorser status use this

            //Find the MPNs of the correct Loan Type and are Endorser
            IEnumerable<CodMpn> typeSpecificEndorserMpnsFiltered = null;

            if (plusFlag == "PLUS")
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;

            }
            if (plusFlag == null)
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;
            }

            if (mpnTypeCode == plusMpnTypeCode && typeSpecificEndorserMpnsFiltered != null && typeSpecificEndorserMpnsFiltered.Count() > 0)
            {
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificEndorserMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnExpirationDates.Add(mpn.CodmMpnExpDate);
                        }
                    }
                    return activeMpnExpirationDates;
                }
            }

            //Find the MPNs of the correct Loan Type and are Pending
            IEnumerable<CodMpn> typeSpecificPendingMpnsFiltered = null;
            if (plusFlag == "PLUS")
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (plusFlag == null)
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (typeSpecificPendingMpnsFiltered != null && typeSpecificPendingMpnsFiltered.Count() > 0)
            {
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificPendingMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnExpirationDates.Add(mpn.CodmMpnExpDate);
                        }
                    }
                    return activeMpnExpirationDates;
                }
            }

            //Find the MPNs of the correct Loan Type and are Old Active Status
            IEnumerable<CodMpn> typeSpecificOldActiveMpnsFiltered = null;
            if (plusFlag == "PLUS")
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (plusFlag == null)
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (typeSpecificOldActiveMpnsFiltered != null && typeSpecificOldActiveMpnsFiltered.Count() > 0)
            {
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificOldActiveMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnExpirationDates.Add(mpn.CodmMpnExpDate);
                        }
                    }
                    return activeMpnExpirationDates;
                }
            }
            
            //If none return null
            return null;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="studentMpnRecords"></param>
        /// <param name="mpnTypeCode"></param>
        /// <param name="plusFlag"></param>
        /// <returns></returns>
        private List<string> GetPlusLoanMpnNames(string studentId, Collection<CodMpn> studentMpnRecords, string mpnTypeCode, string plusFlag = null)
        {

            IEnumerable<CodMpn> typeSpecificActiveMpnsFiltered = null;
            var activeMpnPersonIDs = new List<string>();

            if (plusFlag != null && plusFlag == "PLUS")
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (plusFlag != null && plusFlag == "GRAD")
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (plusFlag == null)
            {
                var typeSpecificActiveMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "A" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpns;
            }

            if (typeSpecificActiveMpnsFiltered != null && typeSpecificActiveMpnsFiltered.Count() > 0)
            {
                typeSpecificActiveMpnsFiltered = typeSpecificActiveMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificActiveMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnPersonIDs.Add(mpn.CodmNonstuCodPersonId);
                        }
                    }
                    return activeMpnPersonIDs;
                }
            }

            //If this is a PLUS or GPLUS and there is an Endorser status use this

            //Find the MPNs of the correct Loan Type and are Endorser
            IEnumerable<CodMpn> typeSpecificEndorserMpnsFiltered = null;

            if (plusFlag == "PLUS")
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;

            }
            if (plusFlag == null)
            {
                var typeSpecificEndorserMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "E" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpns;
            }

            if (mpnTypeCode == plusMpnTypeCode && typeSpecificEndorserMpnsFiltered != null && typeSpecificEndorserMpnsFiltered.Count() > 0)
            {
                typeSpecificEndorserMpnsFiltered = typeSpecificEndorserMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificEndorserMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnPersonIDs.Add(mpn.CodmNonstuCodPersonId);
                        }
                    }
                    return activeMpnPersonIDs;
                }
            }

            //Find the MPNs of the correct Loan Type and are Pending
            IEnumerable<CodMpn> typeSpecificPendingMpnsFiltered = null;
            if (plusFlag == "PLUS")
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (plusFlag == null)
            {
                var typeSpecificPendingMpns = studentMpnRecords.Where(
                    m =>
                        m.Recordkey.Length >= 10 &&
                        m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                        m.CodmMpnStatus == "P" &&
                        m.CodmMpnExpDate.HasValue
                    );
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpns;
            }
            if (typeSpecificPendingMpnsFiltered != null && typeSpecificPendingMpnsFiltered.Count() > 0)
            {
                typeSpecificPendingMpnsFiltered = typeSpecificPendingMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificPendingMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnPersonIDs.Add(mpn.CodmNonstuCodPersonId);
                        }
                    }
                    return activeMpnPersonIDs;
                }
            }

            //Find the MPNs of the correct Loan Type and are Old Active Status
            IEnumerable<CodMpn> typeSpecificOldActiveMpnsFiltered = null;
            if (plusFlag == "PLUS")
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    !string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (plusFlag == "GRAD")
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    string.IsNullOrEmpty(m.CodmNonstuCodPersonId) &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (plusFlag == null)
            {
                var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Recordkey.Length >= 10 &&
                    m.Recordkey.Substring(9, 1) == mpnTypeCode &&
                    m.CodmMpnStatus == "X" &&
                    m.CodmMpnExpDate.HasValue
                );
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpns;
            }
            if (typeSpecificOldActiveMpnsFiltered != null && typeSpecificOldActiveMpnsFiltered.Count() > 0)
            {
                typeSpecificOldActiveMpnsFiltered = typeSpecificOldActiveMpnsFiltered.OrderByDescending(m => m.CodmMpnExpDate);
                var mpnToUse = typeSpecificOldActiveMpnsFiltered;
                if (mpnToUse != null)
                {
                    foreach (var mpn in mpnToUse)
                    {
                        if (mpn.CodmMpnExpDate >= DateTime.Today)
                        {
                            activeMpnPersonIDs.Add(mpn.CodmNonstuCodPersonId);
                        }
                    }
                    return activeMpnPersonIDs;
                }
            }

            //If none return null
            return null;
        }

        /// <summary>
        /// Helper method to get the student's interview date based on the given loan code.
        /// </summary>
        /// <param name="studentInterviewRecords">Collection of the student's FaInterview data contract objects</param>
        /// <param name="interviewCategoryCode">Indicates the category of interview from which to get the interview date. 
        /// S - Subsidized/Unsubsidized interview; P - GradPlus interview</param>
        /// <returns>DateTime object representing the interview date for the given interviewTypeCode. If the student has no
        /// interview records, this method returns null.</returns>
        private async Task<DateTime?> GetInterviewDateAsync(Collection<FaInterview> studentInterviewRecords, string interviewCategoryCode)
        {
            var interviewCodes = (await GetFaInterviewCodesAsync()).Where(code => code.FicCategory.ToUpper() == interviewCategoryCode).Select(code => code.Recordkey);
            var loanCodeInterviews = studentInterviewRecords.Where(i => interviewCodes.Contains(i.FainLoanCode) && i.FainEntranceCmplDate.HasValue);
            if (loanCodeInterviews.Count() > 0)
            {
                loanCodeInterviews = loanCodeInterviews.OrderByDescending(i => i.FainEntranceCmplDate.Value);
                return loanCodeInterviews.First().FainEntranceCmplDate;
            }
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Isir Nslds records from the list in Fin.Aid.
        /// </summary>
        /// <param name="studentFinaidRecord">Fin.Aid record for the student.</param>
        /// <returns>List of ISIR Nslds records.</returns>
        private async Task<IEnumerable<IsirNslds>> GetNsldsRecordsAsync(FinAid studentFinaidRecord)
        {
            if (studentFinaidRecord.FaIsirNsldsIds != null && studentFinaidRecord.FaIsirNsldsIds.Any())
            {
                var isirNsldsRecords = await DataReader.BulkReadRecordAsync<IsirNslds>(studentFinaidRecord.FaIsirNsldsIds.ToArray());
                return isirNsldsRecords;
            }
            else
            {
                return new List<IsirNslds>();
            }
        }

        /// <summary>
        /// Gets NSLDS stored interview date for the requested loan type if any
        /// </summary>
        /// <param name="studentNsldsRecords">list of student NSLDS records</param>
        /// <param name="loanType">direct or plus loan type string: 'S' or 'P'</param>
        /// <returns>interview date or null</returns>
        private async Task<DateTime?> GetNsldsDateAsync(IEnumerable<IsirNslds> studentNsldsRecords, string loanType)
        {
            // Loop thru ISIR.NSLDS records
            foreach (var singleNsldsRecord in studentNsldsRecords)
            {
                if (singleNsldsRecord.InsdLoanIds != null && singleNsldsRecord.InsdLoanIds.Any())
                {
                    // Get the ISIR.NSLDS.LOAN records
                    var nsldsLoanRecords = await DataReader.BulkReadRecordAsync<IsirNsldsLoan>(singleNsldsRecord.InsdLoanIds.ToArray());
                    if (nsldsLoanRecords != null && nsldsLoanRecords.Any())
                    {
                        foreach (var singleLoanRecord in nsldsLoanRecords)
                        {
                            var typeOfLoan = singleLoanRecord.InsdlProgramCode;
                            if (!string.IsNullOrEmpty(typeOfLoan))
                            {
                                if ((loanType == "S" && directLoanTypes.Contains(typeOfLoan.ToUpper())) 
                                    ||(loanType == "P" && plusLoanTypes.Contains(typeOfLoan.ToUpper())))
                                {
                                    return singleLoanRecord.InsdlCurrentStatusDate; 
                                }                                
                            } 
                        }
                    }
                }
                //I can stop looping once I've processed a TM record. Keep going with a CPS type since they only give
                // us the last 6 loans at a time.
                // CPS - isir record. Not CPS - nslds record ( we need to look at one nslds record only)
                if (singleNsldsRecord.InsdType != "CPS")
                {
                    return null;
                }
            } 
            return null;
        }


        /// <summary>
        /// Helper to get FaInterviewCodes records from cache or db.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<FaInterviewCodes>> GetFaInterviewCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<FaInterviewCodes>>("AllFaInterviewCodesRecords",
                async() =>
                {
                    return await DataReader.BulkReadRecordAsync<FaInterviewCodes>("");
                });
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
        /// Helper to get IsiNsldsLoan records from cache or db.
        /// </summary>
        /// <returns></returns>
        //private async Task<IEnumerable<IsirNsldsLoan>> GetIsirNsldsLoanRecordsAsync(List<string> loanIds)
        //{
        //    return await GetOrAddToCacheAsync<IEnumerable<IsirNsldsLoan>>("AllNsldsLoanRecords",
        //        async () =>
        //        {
        //            return await DataReader.BulkReadRecordAsync<IsirNsldsLoan>(loanIds.ToArray());
        //        });
        //}

    }
}
