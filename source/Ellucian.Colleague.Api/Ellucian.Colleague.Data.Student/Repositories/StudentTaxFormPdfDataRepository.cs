// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for the data to be printed in a PDF for any tax form
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTaxFormPdfDataRepository : BaseColleagueRepository, IStudentTaxFormPdfDataRepository
    {
        /// <summary>
        /// Tax Form PDF data repository constructor.
        /// </summary>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public StudentTaxFormPdfDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // nothing to do
        }

        /// <summary>
        /// Get the 1098-T data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098-T.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a 1098-T tax form</param>
        /// <param name="suppressNotification">Optional parameter to suppress PDf access email notifications</param>
        /// <returns>1098 T/E data for a pdf</returns>
        public async Task<Form1098PdfData> Get1098PdfAsync(string personId, string recordId, bool suppressNotification = false)
        {
            try
            {
                // Get student demographic information and box numbers.
                var pdfDataContract = await DataReader.ReadRecordAsync<TaxForm1098Forms>(recordId);

                // Read person record to get SSN.
                var personContract = await DataReader.ReadRecordAsync<Person>(personId);

                // Get Parm1098 
                var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
                
                string taxFormName = string.Empty;
                if (pdfDataContract == null)
                {
                    throw new NullReferenceException("pdfDataContract cannot be null.");
                }

                if (pdfDataContract.Tf98fStudent != personId)
                {
                    throw new ApplicationException("Person ID from request is not the same as the Person ID of the current user and the requesting user is not a Proxy.");
                }
                // Read CORP to get the institution information.
                var corpContract = await DataReader.ReadRecordAsync<Corp>("PERSON", pdfDataContract.Tf98fInstitution);

                var corpFoundsContract = await DataReader.ReadRecordAsync<CorpFounds>(pdfDataContract.Tf98fInstitution);

                if (pdfDataContract.Tf98fTaxForm1098Boxes == null || !pdfDataContract.Tf98fTaxForm1098Boxes.Any())
                {
                    throw new ApplicationException("There are no boxes on the 1098 tax form.");
                }
                var form1098BoxDataContracts = await DataReader.BulkReadRecordAsync<TaxForm1098Boxes>(pdfDataContract.Tf98fTaxForm1098Boxes.ToArray());

                var boxCodesDataContracts = await DataReader.BulkReadRecordAsync<BoxCodes>(form1098BoxDataContracts.Select(x => x.Tf98bBoxCode).ToArray());


                if (corpFoundsContract == null)
                {
                    throw new NullReferenceException("corpFoundsContract cannot be null.");
                }
                if (pdfDataContract.Tf98fTaxYear == null)
                {
                    throw new NullReferenceException("Tf98fTaxYear cannot be null.");
                }
                if (string.IsNullOrEmpty(corpFoundsContract.CorpTaxId))
                {
                    throw new NullReferenceException("CorpTaxId cannot be null.");
                }

                Form1098PdfData entity = new Form1098PdfData(pdfDataContract.Tf98fTaxYear.ToString(), corpFoundsContract.CorpTaxId);
                entity.Correction = pdfDataContract.Tf98fCorrectionInd.ToUpper() == "Y";
                // Student data
                entity.StudentId = pdfDataContract.Tf98fStudent;
                entity.StudentName = pdfDataContract.Tf98fName;
                entity.StudentName2 = pdfDataContract.Tf98fName2;
                entity.StudentAddressLine1 = pdfDataContract.Tf98fAddress;
                entity.StudentAddressLine2 = pdfDataContract.Tf98fCity + ", " + pdfDataContract.Tf98fState + " " + pdfDataContract.Tf98fZip;
                if (pdfDataContract.Tf98fCountry.ToUpper() != "USA" && pdfDataContract.Tf98fCountry.ToUpper() != "US")
                {
                    entity.StudentAddressLine2 += " " + pdfDataContract.Tf98fCountry;
                }

                // Initialize the SSN
                entity.SSN = "";
                if (personContract != null && !string.IsNullOrEmpty(personContract.Ssn))
                {
                    entity.SSN = personContract.Ssn;
                }

                if (parm1098Contract != null)
                {
                    //Set the current tax form to identify the T or E
                    if (parm1098Contract.P1098TTaxForm == pdfDataContract.Tf98fTaxForm)
                    {
                        taxFormName = StudentConstants.TaxForm1098TName;
                    }
                    else if (parm1098Contract.P1098ETaxForm == pdfDataContract.Tf98fTaxForm)
                    {
                        taxFormName = StudentConstants.TaxForm1098EName;
                    }
                    else
                    {
                        throw new ColleagueWebApiException("The tax form should be either 1098T or 1098E");
                    }

                    entity.TaxFormName = taxFormName;

                    // Mask the SSN if necessary.
                    if (!string.IsNullOrEmpty(parm1098Contract.P1098MaskSsn) && parm1098Contract.P1098MaskSsn.ToUpper() == "Y")
                    {
                        if (!string.IsNullOrEmpty(entity.SSN))
                        {
                            // Mask SSN
                            if (entity.SSN.Length >= 4)
                            {
                                entity.SSN = "XXX-XX-" + entity.SSN.Substring(entity.SSN.Length - 4);
                            }
                            else
                            {
                                entity.SSN = "XXX-XX-" + entity.SSN;
                            }
                        }
                    }

                    entity.AtLeastHalfTime = false;

                    // Institution data
                    entity.InstitutionId = pdfDataContract.Tf98fInstitution;

                    if (corpContract.CorpName == null || !corpContract.CorpName.Any())
                    {
                        throw new ApplicationException("Institution must have a name.");
                    }

                    entity.InstitutionName = String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)));

                    if (entity.TaxFormName == StudentConstants.TaxForm1098TName)
                    {
                        SetInstitutionPhone(parm1098Contract.P1098TInstPhone, parm1098Contract.P1098TInstPhoneExt, entity);
                    }
                    else if (entity.TaxFormName == StudentConstants.TaxForm1098EName)
                    {
                        SetInstitutionPhone(parm1098Contract.P1098EInstPhone, parm1098Contract.P1098EInstPhoneExt, entity);
                    }

                    // Box 1 - for 1098E
                    // Get the box codes where the box number is 1 and the box code tax form is the tax form for 1098-Es.
                    var box1BoxCode1098e = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "1" && x.BxcTaxForm == parm1098Contract.P1098ETaxForm);
                    if (box1BoxCode1098e != null)
                    {
                        //last value is being taken from the boxes filtered out
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box1BoxCode1098e.Recordkey).LastOrDefault();
                        entity.StudentInterestAmount = (boxData != null && boxData.Tf98bAmt != null && boxData.Tf98bAmt.HasValue ? boxData.Tf98bAmt.Value : 0).ToString("N2");
                    }

                    // Box 1 - for 1098T
                    // Get the box codes where the box number is 1 and the box code tax form is the tax form for 1098-Ts.
                    var box1BoxCode1098t = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "1" && x.BxcTaxForm == parm1098Contract.P1098TTaxForm);
                    if (box1BoxCode1098t != null)
                    {
                        //last value is being taken from the boxes filtered out
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box1BoxCode1098t.Recordkey).LastOrDefault();
                        entity.AmountsPaidForTuitionAndExpenses = (boxData != null && boxData.Tf98bAmt != null && boxData.Tf98bAmt.HasValue ? boxData.Tf98bAmt.Value : 0).ToString("N2");
                    }

                    // Box 2 - for 1098T
                    var box2BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "2");
                    if (box2BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box2BoxCode.Recordkey).LastOrDefault();
                        if (boxData != null)
                        {
                            entity.AmountsBilledForTuitionAndExpenses = (boxData.Tf98bAmt ?? 0).ToString("N2");
                        }
                    }

                    // Box 2 - 1098E
                    var priorInterestOrFeeExcluded = parm1098Contract.P1098EYearInfoEntityAssociation.FirstOrDefault(x =>
                        x.P1098EYearsAssocMember.HasValue && x.P1098EYearsAssocMember.ToString() == entity.TaxYear);
                    if (priorInterestOrFeeExcluded != null && !string.IsNullOrEmpty(priorInterestOrFeeExcluded.P1098EFeeFlagsAssocMember))
                    {
                        entity.IsPriorInterestOrFeeExcluded = priorInterestOrFeeExcluded.P1098EFeeFlagsAssocMember.ToUpper() == "Y";
                    }

                    // Box 3 - for 1098T
                    entity.ReportingMethodHasBeenChanged = false;
                    var changedReportingMethod = parm1098Contract.P1098TYearInfoEntityAssociation.First(x =>
                        x.P1098TYearsAssocMember.HasValue && x.P1098TYearsAssocMember.ToString() == entity.TaxYear);
                    if (changedReportingMethod != null && !string.IsNullOrEmpty(changedReportingMethod.P1098TYrChgRptMethsAssocMember))
                    {
                        entity.ReportingMethodHasBeenChanged = changedReportingMethod.P1098TYrChgRptMethsAssocMember.ToUpper() == "Y";
                    }

                    // Box 4 - for 1098T
                    var box4BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "4");
                    if (box4BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box4BoxCode.Recordkey
                            && x.Tf98bBoxCode == parm1098Contract.P1098TRefundBoxCode).LastOrDefault();
                        if (boxData != null)
                        {
                            entity.AdjustmentsForPriorYear = (boxData.Tf98bAmt ?? 0).ToString("N2");
                        }
                    }

                    // Box 5 - for 1098T
                    var box5BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "5");
                    if (box5BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box5BoxCode.Recordkey
                            && x.Tf98bBoxCode == parm1098Contract.P1098TFaBoxCode).LastOrDefault();
                        if (boxData != null)
                        {
                            entity.ScholarshipsOrGrants = (boxData.Tf98bAmt ?? 0).ToString("N2");
                        }
                    }

                    // Box 6 - for 1098T
                    var box6BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "6");
                    if (box6BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.Where(x => x.Tf98bBoxCode == box6BoxCode.Recordkey
                            && x.Tf98bBoxCode == parm1098Contract.P1098TFaRefBoxCode).LastOrDefault();
                        if (boxData != null)
                        {
                            entity.AdjustmentsToScholarshipsOrGrantsForPriorYear = (boxData.Tf98bAmt ?? 0).ToString("N2");
                        }
                    }

                    // Box 7 - for 1098T
                    if (pdfDataContract.Tf98fTaxYear >= 2018)
                    {
                        if (string.IsNullOrEmpty(parm1098Contract.P1098TNewYrPayBoxCode))
                        {
                            logger.Error("P1098TNewYrPayBoxCode must have a value set up on T9TE.");
                            throw new ApplicationException("P1098TNewYrPayBoxCode must have a value set up.");
                        }
                        var boxData = form1098BoxDataContracts.LastOrDefault(x => x.Tf98bBoxCode == parm1098Contract.P1098TNewYrPayBoxCode);
                        if (boxData != null && boxData.Tf98bValue != null)
                        {
                            entity.AmountsBilledAndReceivedForQ1Period = boxData.Tf98bValue.ToUpper() == "X";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(parm1098Contract.P1098TNewYrBoxCode))
                        {
                            logger.Error("P1098TNewYrBoxCode must have a value set up on T9TE.");
                            throw new ApplicationException("P1098TNewYrBoxCode must have a value set up.");
                        }
                        var boxData = form1098BoxDataContracts.LastOrDefault(x => x.Tf98bBoxCode == parm1098Contract.P1098TNewYrBoxCode);
                        if (boxData != null && boxData.Tf98bValue != null)
                        {
                            entity.AmountsBilledAndReceivedForQ1Period = boxData.Tf98bValue.ToUpper() == "X";
                        }
                    }

                    // Box 8 - for 1098T
                    var box8BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "8");
                    if (box8BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.LastOrDefault(x => x.Tf98bBoxCode == box8BoxCode.Recordkey);
                        if (boxData != null && parm1098Contract.P1098TLoadBoxCode == boxData.Tf98bBoxCode)
                        {
                            entity.AtLeastHalfTime = boxData.Tf98bValue.ToUpper() == "X";
                        }
                    }

                    // Box 9 - for 1098T
                    var box9BoxCode = boxCodesDataContracts.FirstOrDefault(x => x.BxcBoxNumber == "9");
                    if (box9BoxCode != null)
                    {
                        var boxData = form1098BoxDataContracts.LastOrDefault(x => x.Tf98bBoxCode == box9BoxCode.Recordkey);
                        if (boxData != null && parm1098Contract.P1098TGradBoxCode == boxData.Tf98bBoxCode)
                        {
                            entity.IsGradStudent = boxData.Tf98bValue.ToUpper() == "X";
                        }
                    }

                    // Do not display 1098-T forms that do not have non-zero amounts for box 1, 4, 5, and 6,
                    // and are for tax year 2019 and beyond, that are not a correction.
                    if (parm1098Contract.P1098TTaxForm == pdfDataContract.Tf98fTaxForm)
                    {
                        if (pdfDataContract.Tf98fTaxYear >= 2019)
                        {
                            if (!(entity.AmountsPaidForTuitionAndExpenses != null && entity.AmountsPaidForTuitionAndExpenses != "0.00") &&
                                !(entity.AdjustmentsForPriorYear != null && entity.AdjustmentsForPriorYear != "0.00") &&
                                !(entity.ScholarshipsOrGrants != null && entity.ScholarshipsOrGrants != "0.00") &&
                                !(entity.AdjustmentsToScholarshipsOrGrantsForPriorYear != null && entity.AdjustmentsToScholarshipsOrGrantsForPriorYear != "0.00") &&
                                !(entity.Correction))
                            {
                                throw new ApplicationException("1098-T tax form must have values for box 1, 4, 5, or 6.");
                            }
                        }
                    }
                }

                // Call the PDF accessed CTX to trigger an email notification
                if (!suppressNotification)
                {
                    TxNotifyStPdfAccessRequest request = new TxNotifyStPdfAccessRequest();
                    request.TaxFormPdfId = recordId;
                    request.PdfPersonId = entity.StudentId;
                    request.TaxForm = "1098";
                    var response = await transactionInvoker.ExecuteAsync<TxNotifyStPdfAccessRequest, TxNotifyStPdfAccessResponse>(request);
                }
                return entity;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Couldn't retrieve 1098 tax form data.");
                return null;
            }
        }

        private static void SetInstitutionPhone(string p1098InstPhone, string p1098TInstPhoneExt, Form1098PdfData entity)
        {
            // Institution phone number
            if (!string.IsNullOrEmpty(p1098InstPhone))
            {
                entity.InstitutionPhoneNumber = p1098InstPhone;

                if (!string.IsNullOrEmpty(p1098TInstPhoneExt))
                {
                    entity.InstitutionPhoneNumber += " Ext. " + p1098TInstPhoneExt;
                }
            }
        }

        /// <summary>
        /// Get the T2202A data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202A.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a T2202A tax form.</param>
        /// <returns>T2202A data for a pdf.</returns>
        public async Task<FormT2202aPdfData> GetT2202aPdfAsync(string personId, string recordId)
        {
            try
            {
                // Get student demographic information and box numbers.
                var pdfDataContract = await DataReader.ReadRecordAsync<CnstT2202aRepos>(recordId);
                var rptParmsDataContract = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
                var personDataContract = await DataReader.ReadRecordAsync<Person>("PERSON", personId);
                
                // Get the default institution ID and read the default institution name.
                var institutionName = string.Empty;
                var defaultCorpId = string.Empty;
            
                if (pdfDataContract == null)
                {
                    throw new NullReferenceException("pdfDataContract cannot be null.");
                }

                if (pdfDataContract.T2ReposStudent != personId)
                {
                    throw new ApplicationException("Person ID from request is not the same as the Person ID of the current user and the requesting user is not a Proxy.");
                }

                if (pdfDataContract.T2ReposYear == null)
                {
                    throw new NullReferenceException("T2ReposYear cannot be null.");
                }
                if (pdfDataContract.T2ReposStudent == null)
                {
                    throw new NullReferenceException("T2ReposStudent cannot be null.");
                }

                // Get the institution information from default institution.
                var defaults = this.GetDefaults();
                if (defaults != null)
                {
                    defaultCorpId = defaults.DefaultHostCorpId;
                    var corpContract = await DataReader.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", defaultCorpId);
                    if (corpContract.CorpName == null || !corpContract.CorpName.Any())
                    {
                        throw new ApplicationException("Institution must have a name.");
                    }

                    institutionName = corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
                }

                FormT2202aPdfData entity = new FormT2202aPdfData(pdfDataContract.T2ReposYear.ToString(), pdfDataContract.T2ReposStudent);

                // Determine if the T2202 is cancelled.
                if ((pdfDataContract.T2CancelFlag != null && pdfDataContract.T2CancelFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    || (pdfDataContract.T2Status != null && pdfDataContract.T2Status.Equals("C", StringComparison.InvariantCultureIgnoreCase) && !(pdfDataContract.T2AmendedFlag != null && pdfDataContract.T2AmendedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))))
                {
                    entity.Cancelled = true;
                }
                else
                {
                    entity.Cancelled = false;
                }

                // Add institution ID and name to domain entity. The ID will be used in the service to get the address information.
                entity.InstitutionId = defaultCorpId;
                entity.InstitutionNameAddressLine1 = institutionName;

                if (rptParmsDataContract != null)
                {
                    entity.FlyingClub = rptParmsDataContract.CnstT2202FlyingClub;
                    entity.SchoolType = rptParmsDataContract.CnstT2202SchoolType;
                }

                if (personDataContract != null)
                {
                    entity.SocialInsuranceNumber = personDataContract.Ssn;
                }

                // Add Student name and address lines
                entity.StudentNameAddressLine1 = pdfDataContract.T2ReposStudentName;
                if (pdfDataContract.T2ReposStudentAddress != null)
                {
                    entity.StudentNameAddressLine2 = pdfDataContract.T2ReposStudentAddress.ElementAtOrDefault(0);
                    entity.StudentNameAddressLine3 = pdfDataContract.T2ReposStudentAddress.ElementAtOrDefault(1);
                    entity.StudentNameAddressLine4 = pdfDataContract.T2ReposStudentAddress.ElementAtOrDefault(2);
                    entity.StudentNameAddressLine5 = pdfDataContract.T2ReposStudentAddress.ElementAtOrDefault(3);
                    entity.StudentNameAddressLine6 = pdfDataContract.T2ReposStudentAddress.ElementAtOrDefault(4);
                }

                // Add Student's program
                entity.ProgramName = pdfDataContract.T2ReposStuProgramTitle;

                // Add Session periods
                foreach (var enrollment in pdfDataContract.T2ReposEnrollmentEntityAssociation)
                {
                    if (enrollment.T2ReposEnrollStartDatesAssocMember == null)
                    {
                        throw new ArgumentNullException("T2ReposEnrollStartDatesAssocMember", "Enrollment start date is required.");
                    }
                    if (enrollment.T2ReposEnrollEndDatesAssocMember == null)
                    {
                        throw new ArgumentNullException("T2ReposEnrollEndDatesAssocMember", "Enrollment end date is required.");
                    }
                    if (enrollment.T2ReposEnrollTuitionAmtsAssocMember == null && enrollment.T2ReposEnrollPtMthsAssocMember == null && enrollment.T2ReposEnrollFtMthsAssocMember == null)
                    {
                        throw new ArgumentNullException("T2ReposEnrollTuitionAmtsAssocMember", "Enrollment tuition fees, part-time hours, or full-time hours are required.");
                    }

                    var fromYear = enrollment.T2ReposEnrollStartDatesAssocMember.Value.Year.ToString();
                    var fromMonth = enrollment.T2ReposEnrollStartDatesAssocMember.Value.Month.ToString();
                    var toYear = enrollment.T2ReposEnrollEndDatesAssocMember.Value.Year.ToString();
                    var toMonth = enrollment.T2ReposEnrollEndDatesAssocMember.Value.Month.ToString();
                    entity.SessionPeriods.Add(new FormT2202aSessionPeriod(fromYear, fromMonth, toYear, toMonth, enrollment.T2ReposEnrollTuitionAmtsAssocMember, enrollment.T2ReposEnrollPtMthsAssocMember, enrollment.T2ReposEnrollFtMthsAssocMember));

                }

                // Call the PDF accessed CTX to trigger an email notification
                TxNotifyStPdfAccessRequest request = new TxNotifyStPdfAccessRequest();
                request.TaxFormPdfId = recordId;
                request.PdfPersonId = entity.StudentId;
                request.TaxForm = "T2202A";
                var response = await transactionInvoker.ExecuteAsync<TxNotifyStPdfAccessRequest, TxNotifyStPdfAccessResponse>(request);
                return entity;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
                () =>
                {
                    var coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Base.DataContracts.Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }
    }
}
