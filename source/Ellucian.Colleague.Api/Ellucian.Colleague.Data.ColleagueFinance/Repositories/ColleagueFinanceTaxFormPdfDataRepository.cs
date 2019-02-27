// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for the data to be printed in a PDF for any tax form
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ColleagueFinanceTaxFormPdfDataRepository : BaseColleagueRepository, IColleagueFinanceTaxFormPdfDataRepository
    {
        /// <summary>
        /// Instantiate a new instance of the FormT4aPdfDataRepository.
        /// </summary>
        /// <param name="cacheProvider">ICacheProvider object.</param>
        /// <param name="transactionFactory">IColleagueTransactionFactory object.</param>
        /// <param name="logger">ILogger object.</param>
        public ColleagueFinanceTaxFormPdfDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // nothing to do
        }

        /// <summary>
        /// Get the pdf data for tax form T4A.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a T4A tax form</param>
        /// <returns>The pdf data for tax form T4A</returns>
        public async Task<FormT4aPdfData> GetFormT4aPdfDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            // Get T4A demographic information and box numbers.
            var t4aBinReposContract = await DataReader.ReadRecordAsync<TaxFormT4aBinRepos>(recordId);

            try
            {
                if (t4aBinReposContract == null)
                {
                    throw new NullReferenceException("pdfDataContract cannot be null.");
                }

                if (personId != t4aBinReposContract.TftbrBinId)
                {
                    throw new ApplicationException("Person ID from request is not the same as the Person ID of the current user.");
                }

                var parmT4aContract = await DataReader.ReadRecordAsync<ParmT4a>("CF.PARMS", "T4A");
                if (parmT4aContract == null)
                {
                    throw new NullReferenceException("PARM.T4A cannot be null.");
                }

                if (t4aBinReposContract.TftbrYear == null)
                {
                    throw new NullReferenceException("TftbrYear cannot be null.");
                }

                string statusFlag = null;
                string qualifyFlag = null;
                string refLevel = "0";
                string level = null;

                // Check the year being processed.
                if (t4aBinReposContract.TftbrYear.ToString() == parmT4aContract.Pt4aYear)
                {
                    // They are requesting the current year.
                    // Find the corrected level or the new data level.
                    if (t4aBinReposContract.TftbrStatus != null && (t4aBinReposContract.TftbrStatus.ToUpperInvariant() == "U" || t4aBinReposContract.TftbrStatus.ToUpperInvariant() == ""))
                    {
                        // The data is in a frozen or submitted status. Find the level where the
                        // data was corrected (C or G status) or where it was new (N status).
                        if (t4aBinReposContract.TftbrCertEntityAssociation != null)
                        {
                            for (int i = 0; i < t4aBinReposContract.TftbrCertEntityAssociation.Count(); i++)
                            {
                                var assocMember = t4aBinReposContract.TftbrCertEntityAssociation[i];
                                statusFlag = assocMember.TftbrCertStatusAssocMember;
                                refLevel = i.ToString();
                                if (statusFlag != "U")
                                {
                                    qualifyFlag = assocMember.TftbrCertQualifiedFlagAssocMember;
                                    break;
                                }
                            }
                        }

                        // Get the record for the year and obtain the list of submitted levels.
                        var taxYearCriteria = "WITH TFTY.TAX.YEAR EQ '" + t4aBinReposContract.TftbrYear + "'";
                        var taxYearIds = await DataReader.SelectAsync("TAX.FORM.T4A.YEARS", taxYearCriteria);

                        if (taxYearIds == null)
                            throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but null returned for record ID: " + t4aBinReposContract.TftbrYear);

                        if (taxYearIds.Count() == 0)
                            throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but zero returned for record ID: " + t4aBinReposContract.TftbrYear);

                        if (taxYearIds.Count() > 1)
                            throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but more than one returned for record ID: " + t4aBinReposContract.TftbrYear);

                        var taxYearContract = await DataReader.ReadRecordAsync<TaxFormT4aYears>(taxYearIds.FirstOrDefault());

                        // Throw an exception if there is no record data
                        if (taxYearContract == null)
                        {
                            throw new ApplicationException("TaxFormT4aYears record " + taxYearIds.FirstOrDefault() + " does not exist.");
                        }

                        // Find the level to use for this tax form.
                        int refLevelInt = Convert.ToInt32(refLevel);
                        if (refLevelInt < taxYearContract.TftySubmittedEntityAssociation.Count)
                        {
                            var submittedAssoc = taxYearContract.TftySubmittedEntityAssociation[refLevelInt];
                            level = submittedAssoc.TftySubmitSeqNosAssocMember;
                        }
                    }
                    else
                    {
                        // Use the M level.
                        level = "M";
                        refLevel = "M";
                        statusFlag = t4aBinReposContract.TftbrStatus;
                        qualifyFlag = t4aBinReposContract.TftbrQualifiedFlag;
                    }
                }
                else
                {
                    // Not the current year. We ignore the verified level if the data is in a frozen status, even if it
                    // was corrected. Find the corrected (C or G) level or the new (N) data level in the submitted data.
                    if (t4aBinReposContract.TftbrCertEntityAssociation != null)
                    {
                        for (int i = 0; i < t4aBinReposContract.TftbrCertEntityAssociation.Count(); i++)
                        {
                            var assocMember = t4aBinReposContract.TftbrCertEntityAssociation[i];
                            statusFlag = assocMember.TftbrCertStatusAssocMember;
                            refLevel = i.ToString();
                            if (statusFlag != "U")
                            {
                                qualifyFlag = assocMember.TftbrCertQualifiedFlagAssocMember;
                                break;
                            }
                        }
                    }

                    // Get the record for the year and obtain the list of submitted levels.
                    var taxYearCriteria = "WITH TFTY.TAX.YEAR EQ '" + t4aBinReposContract.TftbrYear + "'";
                    var taxYearIds = await DataReader.SelectAsync("TAX.FORM.T4A.YEARS", taxYearCriteria);

                    if (taxYearIds == null)
                        throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but null returned for record ID: " + t4aBinReposContract.TftbrYear);

                    if (taxYearIds.Count() == 0)
                        throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but zero returned for record ID: " + t4aBinReposContract.TftbrYear);

                    if (taxYearIds.Count() > 1)
                        throw new ApplicationException("One TAX.FORM.T4A.YEARS ID expected but more than one returned for record ID: " + t4aBinReposContract.TftbrYear);

                    var taxYearContract = await DataReader.ReadRecordAsync<TaxFormT4aYears>(taxYearIds.FirstOrDefault());

                    // Throw an exception if there is no record data
                    if (taxYearContract == null)
                    {
                        throw new ApplicationException("TaxFormT4aYears record " + taxYearIds.FirstOrDefault() + " does not exist.");
                    }

                    // Find the level to use for this tax form.
                    int refLevelInt = Convert.ToInt32(refLevel);
                    if (refLevelInt < taxYearContract.TftySubmittedEntityAssociation.Count)
                    {
                        var submittedAssoc = taxYearContract.TftySubmittedEntityAssociation[refLevelInt];
                        level = submittedAssoc.TftySubmitSeqNosAssocMember;
                    }
                }

                if (statusFlag != "U" && qualifyFlag == "Y")
                {
                    // Get the box number/footnotes and associated amounts for the CF/HR/ST TAX.T4A.DETAIL records for the level.
                    var detailCriteria = "WITH TTDR.YEAR EQ '" + t4aBinReposContract.TftbrYear + "' AND WITH TTDR.ID EQ '" + t4aBinReposContract.TftbrBinId +
                        "' AND WITH TTDR.BIN.ID EQ '" + t4aBinReposContract.TftbrBinCode + "' AND WITH TTDR.REF.ID EQ '" + level + "'";
                    var detailTaxRecords = await DataReader.BulkReadRecordAsync<TaxT4aDetailRepos>(detailCriteria);

                    List<string> boxNumberSub = new List<string>();
                    List<decimal> boxNumberAmounts = new List<decimal>();

                    if (detailTaxRecords != null && detailTaxRecords.Any())
                    {
                        // Loop through each detail record to summarize box codes and their amounts.
                        foreach (var record in detailTaxRecords)
                        {
                            foreach (var boxAssocMember in record.TtdrSubEntityAssociation)
                            {
                                int loc = boxNumberSub.IndexOf(boxAssocMember.TtdrBoxNumberSubAssocMember);
                                if (loc != -1)
                                {
                                    boxNumberAmounts[loc] += boxAssocMember.TtdrAmtAssocMember.HasValue ? boxAssocMember.TtdrAmtAssocMember.Value : 0m;
                                }
                                else
                                {
                                    boxNumberSub.Add(boxAssocMember.TtdrBoxNumberSubAssocMember ?? "");
                                    boxNumberAmounts.Add(boxAssocMember.TtdrAmtAssocMember.HasValue ? boxAssocMember.TtdrAmtAssocMember.Value : 0m);
                                }
                            }
                        }
                    }

                    // Verify whether this tax form meets the minimum requirements for a T4A.
                    bool isIncluded = false;

                    // If group term life insurance was provided in any amount
                    int pos = boxNumberSub.IndexOf("119");
                    if (pos != -1)
                    {
                        isIncluded = true;
                    }

                    // If you deducted tax from any payment
                    pos = boxNumberSub.IndexOf("022");
                    if (pos != -1)
                    {
                        isIncluded = true;
                    }

                    // If Tax - Free Savings Account (TFSA)taxable amounts paid
                    // to a recipient for the year is more than $50.
                    pos = boxNumberSub.IndexOf("134");
                    if (pos != -1 && boxNumberAmounts[pos] > 50)
                    {
                        isIncluded = true;
                    }

                    // If any RESP accumulated income payments totaling
                    // $50 or more are made in the calendar year.
                    // Box 122 rolls up into 040, so check for 040122 also.
                    pos = boxNumberSub.IndexOf("040");
                    if (pos != -1 && boxNumberAmounts[pos] >= 50)
                    {
                        isIncluded = true;
                    }
                    pos = boxNumberSub.IndexOf("040122");
                    if (pos != -1 && boxNumberAmounts[pos] >= 50)
                    {
                        isIncluded = true;
                    }

                    // If any RESP educational assistance payments totaling
                    // $50 or more are made in the calendar year.
                    pos = boxNumberSub.IndexOf("042");
                    if (pos != -1 && boxNumberAmounts[pos] >= 50)
                    {
                        isIncluded = true;
                    }

                    // Also check the minimum total amount
                    if (!parmT4aContract.Pt4aMinTotAmt.HasValue || boxNumberAmounts.Sum() >= parmT4aContract.Pt4aMinTotAmt.Value)
                    {
                        isIncluded = true;
                    }

                    // Only create a T4A pdf if it meets the criteria
                    if (isIncluded)
                    {
                        List<string> payerIds = new List<string>();
                        List<string> hierarchies = new List<string>();
                        List<string> names = new List<string>();

                        var payerId = t4aBinReposContract.TftbrCorpId;
                        string payerName = "";

                        // Call a colleague transaction to get the institution name based on the hierarchy.
                        if (string.IsNullOrEmpty(payerId))
                        {
                            throw new ApplicationException("TaxFormT4aBinRepos record Corp ID does not exist.");
                        }
                        else
                        {
                            payerIds.Add(payerId);
                            hierarchies.Add(parmT4aContract.Pt4aNameAddrHierarchy);

                            GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                            {
                                IoPersonIds = payerIds,
                                IoHierarchies = hierarchies
                            };
                            GetHierarchyNamesForIdsResponse response = await transactionInvoker.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                            // The transaction returns the hierarchy names. If the name is multivalued, 
                            // the transaction only returns the first value of the name.
                            if (response != null)
                            {
                                if (!((response.OutPersonNames == null) || (response.OutPersonNames.Count < 1)))
                                {
                                    payerName = response.OutPersonNames.FirstOrDefault();
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(payerName))
                        {
                            throw new ApplicationException("payerName is a required field for a Tax form T4A pdf.");
                        }

                        // Instantiate the domain entity.
                        string taxYear = t4aBinReposContract.TftbrYear.ToString();
                        FormT4aPdfData domainEntityT4a = new FormT4aPdfData(taxYear, payerName);

                        // Populate the person ID in the domain for the service layer security check.
                        domainEntityT4a.RecipientId = t4aBinReposContract.TftbrBinId;

                        // The Payer's account number is only printed when sending forms to CRA.

                        // Three possible scenarios exist for each BIN record.
                        // Depending on the status of the BIN record, the following occurs:
                        // 1. Status of G(1 part correction)
                        //    The BIN record is printed with the word "AMENDED" on the top of the form.
                        // 2. Status of C(2 part correction) which means that the recipient's name, SIN, or address information has been changed.
                        //    The BIN record is printed if the amounts are greater than 0.00.The level prior to the requested level is 
                        //    determined and the BIN form for the prior level is printed with the word "Cancel" on the top of the form.
                        //    Therefore, two forms are printed for the BIN record.
                        // 3. Status of N(new). The BIN record is printed.
                        if (statusFlag.ToUpperInvariant() == "G")
                            domainEntityT4a.Amended = "AMENDED";

                        if (statusFlag.ToUpperInvariant() == "C")
                            // Only print the amended level, not the cancel level
                            domainEntityT4a.Amended = "AMENDED";


                        // Read the TAX.FORM.T4A.REPOS record for the demographic information
                        var taxFormT4aContract = await DataReader.ReadRecordAsync<TaxFormT4aRepos>(t4aBinReposContract.TftbrReposId);

                        // We need to determine is this recipient is an individual or a business.
                        // If there is a PERSON record and the indicator is yes, it is a business.
                        // Otherwise we will treat it as an individual as it has been up until now.
                        var personContract = await DataReader.ReadRecordAsync<Person>("PERSON", taxFormT4aContract.TftrId);

                        if (level == "M")
                        {
                            domainEntityT4a.RecipientsName = taxFormT4aContract.TftrSurname + ' ' + taxFormT4aContract.TftrFirstName + ' ' + taxFormT4aContract.TftrMiddleInitial;

                            string countryDesc = string.Empty;
                            if (!string.IsNullOrEmpty(taxFormT4aContract.TftrCountry))
                            {
                                var countryContract = await DataReader.ReadRecordAsync<Countries>(taxFormT4aContract.TftrCountry);
                                if (countryContract != null)
                                {
                                    if (!string.IsNullOrEmpty(countryContract.CtryDesc))
                                    {
                                        countryDesc = countryContract.CtryDesc;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(taxFormT4aContract.TftrAddress2))
                            {
                                domainEntityT4a.RecipientAddr1 = taxFormT4aContract.TftrAddress;
                                domainEntityT4a.RecipientAddr2 = taxFormT4aContract.TftrCity + ' ' + taxFormT4aContract.TftrProvince + ' ' + taxFormT4aContract.TftrPostalCode;
                                // Only print country description if TFTR.COUNTRY is populated
                                if (!string.IsNullOrEmpty(countryDesc))
                                {
                                    domainEntityT4a.RecipientAddr3 = countryDesc;
                                }
                            }
                            else
                            {
                                domainEntityT4a.RecipientAddr1 = taxFormT4aContract.TftrAddress;
                                domainEntityT4a.RecipientAddr2 = taxFormT4aContract.TftrAddress2;
                                domainEntityT4a.RecipientAddr3 = taxFormT4aContract.TftrCity + ' ' + taxFormT4aContract.TftrProvince + ' ' + taxFormT4aContract.TftrPostalCode;
                                // Only print country description if TFTR.COUNTRY is populated
                                if (!string.IsNullOrEmpty(countryDesc))
                                {
                                    domainEntityT4a.RecipientAddr4 = countryDesc;
                                }
                            }

                            if (personContract.PersonCorpIndicator.ToUpperInvariant() == "Y")
                            {
                                domainEntityT4a.RecipientAccountNumber = taxFormT4aContract.TftrRecipientBn.Replace(" ", "");
                            }
                            else
                            {
                                domainEntityT4a.Sin = taxFormT4aContract.TftrSin.Replace("-", "");
                            }
                        }

                        else
                        {
                            var index = Convert.ToInt32(refLevel);
                            if (index < taxFormT4aContract.TftrCertifySin.Count)
                            {
                                string surname = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifySurname.Count())
                                {
                                    surname = taxFormT4aContract.TftrCertifySurname[index];
                                }

                                string firstName = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyFirstName.Count)
                                {
                                    firstName = taxFormT4aContract.TftrCertifyFirstName[index];
                                }

                                string middleInitial = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyMiddleInitial.Count)
                                {
                                    middleInitial = taxFormT4aContract.TftrCertifyMiddleInitial[index];
                                }

                                domainEntityT4a.RecipientsName = (surname + ' ' + firstName + ' ' + middleInitial).Trim();

                                string countryCode = string.Empty;
                                string countryDesc = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyCountry.Count)
                                {
                                    countryCode = taxFormT4aContract.TftrCertifyCountry[index];
                                }

                                string addressLine1 = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyAddress.Count)
                                {
                                    addressLine1 = taxFormT4aContract.TftrCertifyAddress[index];
                                }

                                string addressLine2 = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyAddress2.Count)
                                {
                                    addressLine2 = taxFormT4aContract.TftrCertifyAddress2[index];
                                }

                                string city = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyCity.Count)
                                {
                                    city = taxFormT4aContract.TftrCertifyCity[index];
                                }

                                string province = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyProvince.Count)
                                {
                                    province = taxFormT4aContract.TftrCertifyProvince[index];
                                }

                                string postalCode = string.Empty;
                                if (index < taxFormT4aContract.TftrCertifyPostalCode.Count)
                                {
                                    postalCode = taxFormT4aContract.TftrCertifyPostalCode[index];
                                }
                                if (!string.IsNullOrEmpty(countryCode))
                                {
                                    var countryContract = await DataReader.ReadRecordAsync<Countries>(countryCode);
                                    if (countryContract != null)
                                    {
                                        if (!string.IsNullOrEmpty(countryContract.CtryDesc))
                                        {
                                            countryDesc = countryContract.CtryDesc;
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(addressLine2))
                                {
                                    domainEntityT4a.RecipientAddr1 = addressLine1;
                                    domainEntityT4a.RecipientAddr2 = city + ' ' + province + ' ' + postalCode;
                                    // Only print country description if TFTR.COUNTRY is populated
                                    if (!string.IsNullOrEmpty(countryDesc))
                                    {
                                        domainEntityT4a.RecipientAddr3 = countryDesc;
                                    }
                                }
                                else
                                {
                                    domainEntityT4a.RecipientAddr1 = addressLine1;
                                    domainEntityT4a.RecipientAddr2 = addressLine2;
                                    domainEntityT4a.RecipientAddr3 = city + ' ' + province + ' ' + postalCode;
                                    // Only print country description if TFTR.COUNTRY is populated
                                    if (!string.IsNullOrEmpty(countryDesc))
                                    {
                                        domainEntityT4a.RecipientAddr4 = countryDesc;
                                    }
                                }

                                if (personContract.PersonCorpIndicator.ToUpperInvariant() == "Y")
                                {
                                    domainEntityT4a.RecipientAccountNumber = taxFormT4aContract.TftrCertifyRecipientBn[index].Replace(" ", "");
                                }
                                else
                                {
                                    domainEntityT4a.Sin = taxFormT4aContract.TftrCertifySin[index].Replace("-", "");
                                }
                            }
                        }

                        // Process the boxes and their amounts.
                        var boxNumber = string.Empty;
                        decimal boxAmount = 0;
                        List<TaxFormBoxesPdfData> taxFormBoxesList = new List<TaxFormBoxesPdfData>();
                        TaxFormBoxesPdfData existingBox = null;
                        TaxFormBoxesPdfData boxNumberDomain = null;

                        for (var i = 0; i < boxNumberSub.Count; i++)
                        {
                            boxNumber = boxNumberSub[i];
                            boxAmount = boxNumberAmounts[i];

                            // Check that each box number is setup in the parameter form.
                            pos = parmT4aContract.Pt4aBoxNoSub.IndexOf(boxNumber);
                            if (pos == -1)
                            {
                                logger.Warn("Box Number " + boxNumber + " is not setup in TASU. Amount ignored.");
                                //throw new ApplicationException("Box Number " + boxNumber + " is not setup in TASU. Amount ignored.");
                            }
                            else
                            {
                                // Only process the box if there is an amount.
                                switch (boxNumber)
                                {
                                    // These next 6 boxes have a pre-printed box in the form.
                                    case "016":
                                        domainEntityT4a.Pension = boxAmount;
                                        break;
                                    case "018":
                                        domainEntityT4a.LumpSumPayment = boxAmount;
                                        break;
                                    case "020":
                                        domainEntityT4a.SelfEmployedCommissions = boxAmount;
                                        break;
                                    case "022":
                                        // No negatives are printed on the form.
                                        domainEntityT4a.IncomeTaxDeducted = boxAmount;
                                        if (domainEntityT4a.IncomeTaxDeducted < 0)
                                        {
                                            domainEntityT4a.IncomeTaxDeducted = boxAmount * -1;
                                        }
                                        break;
                                    case "024":
                                        domainEntityT4a.Annuities = boxAmount;
                                        break;
                                    case "048":
                                        domainEntityT4a.FeesForServices = boxAmount;
                                        break;
                                    default:
                                        switch (boxNumber)
                                        {
                                            // Assoc boxes for 016 print their own amounts in the Other Information section
                                            // of the form, and are added to the 016 box in the fixed section of the form.
                                            case "016128":
                                                domainEntityT4a.Pension += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("128", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            // Assoc boxes for 018 print their own amounts in the Other Information section
                                            // of the form, and are added to the 018 box in the fixed section of the form.
                                            case "018102":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("102", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "018108":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("108", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "018110":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("110", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "018158":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("158", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "018180":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("180", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "018190":
                                                domainEntityT4a.LumpSumPayment += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("190", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;

                                            // Assoc boxes for 024 print their own amounts in the Other Information section
                                            // of the form, and are added to the 024 box in the fixed section of the form.
                                            case "024111":
                                                domainEntityT4a.Annuities += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("111", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;
                                            case "024115":
                                                domainEntityT4a.Annuities += boxAmount;
                                                boxNumberDomain = new TaxFormBoxesPdfData("115", boxAmount);
                                                taxFormBoxesList.Add(boxNumberDomain);
                                                break;

                                            case "032":
                                                // Box 032 has an assoc boxes 126 and 162, so 032 may already be in the list.
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == boxNumber);
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData(boxNumber, boxAmount));
                                                }
                                                break;
                                            // The assoc box for 032 prints its own amount in the Other Information section
                                            // of the form, and causes '032' to print in that section also (if there is no
                                            // amount already just for '032', otherwise it is added to the '032' amount).
                                            case "032126":
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "032");
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData("032", boxAmount));
                                                }
                                                taxFormBoxesList.Add(new TaxFormBoxesPdfData("126", boxAmount));
                                                break;
                                            case "032162":
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "032");
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData("032", boxAmount));
                                                }
                                                taxFormBoxesList.Add(new TaxFormBoxesPdfData("162", boxAmount));
                                                break;

                                            case "040":
                                                // Box 040 has an assoc box 122, so 040 may already in the list.
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == boxNumber);
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData(boxNumber, boxAmount));
                                                }
                                                break;
                                            case "040122":
                                                // The assoc box for 040 prints its own amount in the Other Information section
                                                // of the form, and causes '040' to print in that section also (if there is no
                                                // amount already just for '040', otherwise it is added to the '040' amount).
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "040");
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData("040", boxAmount));
                                                }
                                                taxFormBoxesList.Add(new TaxFormBoxesPdfData("122", boxAmount));
                                                break;

                                            case "105":
                                                // Box 105 has an assoc box 196, so 105 may already be in the list
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == boxNumber);
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData(boxNumber, boxAmount));
                                                }
                                                break;
                                            case "105196":
                                                // The assoc box for 105 prints its own amount in the Other Information section
                                                // of the form, and causes '105' to print in that section also (if there is no
                                                // amount already just for '105', otherwise it is added to the '105' amount).
                                                existingBox = taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "105");
                                                if (existingBox != null)
                                                {
                                                    existingBox.AddAmount(boxAmount);
                                                }
                                                else
                                                {
                                                    taxFormBoxesList.Add(new TaxFormBoxesPdfData("105", boxAmount));
                                                }
                                                taxFormBoxesList.Add(new TaxFormBoxesPdfData("196", boxAmount));
                                                break;
                                            default:
                                                taxFormBoxesList.Add(new TaxFormBoxesPdfData(boxNumber, boxAmount));
                                                break;
                                        }
                                        break;
                                }
                            }
                        }

                        domainEntityT4a.TaxFormBoxesList = taxFormBoxesList.OrderBy(x => x.BoxNumber).ToList();

                        // Call the PDF accessed CTX to send an email notification
                        TxNotifyCfPdfAccessRequest pdfRequest = new TxNotifyCfPdfAccessRequest();
                        pdfRequest.AFormType = "T4A";
                        pdfRequest.APersonId = t4aBinReposContract.TftbrBinId;
                        pdfRequest.ARecordId = t4aBinReposContract.TftbrReposId;

                        var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyCfPdfAccessRequest, TxNotifyCfPdfAccessResponse>(pdfRequest);

                        return domainEntityT4a;
                    }
                    else
                    {
                        throw new ApplicationException("No T4A form can be produced for tax year " + t4aBinReposContract.TftbrYear.ToString() + " for recipient " + t4aBinReposContract.TftbrBinId);
                    }
                }
                else
                {
                    throw new ApplicationException("No T4A form can be produced for tax year " + t4aBinReposContract.TftbrYear.ToString() + " for recipient " + t4aBinReposContract.TftbrBinId);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Get the pdf data for tax form 1099-MISC.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a 1099-MISC tax form</param>
        /// <returns>The pdf data for tax form 1099-MISC</returns>
        public async Task<Form1099MIPdfData> GetForm1099MiPdfDataAsync(string personId, string recordId)
        {

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            Form1099MIPdfData domainEntity1099Mi = null;

            // Get 1099MI detail record information, like year, state, ein, vendorid
            var currentMiDetailRecord = await DataReader.ReadRecordAsync<Tax1099miDetailRepos>(recordId);

            try
            {
                if (currentMiDetailRecord == null)
                {
                    throw new ApplicationException("1099Mi Detail Record cannot be null.");
                }
                var parm1099MIContract = await DataReader.ReadRecordAsync<Parm1099mi>("CF.PARMS", "PARM.1099MI");
                if (parm1099MIContract == null)
                {
                    throw new ApplicationException("PARM.1099MI cannot be null.");
                }
                // Read the tax year record for 1099MI selected statement.
                TaxForm1099miYears taxYearRecord = await DataReader.ReadRecordAsync<TaxForm1099miYears>("TAX.FORM.1099MI.YEARS", currentMiDetailRecord.TmidtlrYear);
                if (taxYearRecord == null)
                {
                    throw new ApplicationException("TAX.FORM.1099MI.YEARS cannot be null.");
                }

                string transmitterId = null, transmitterName = null, transmitterName2 = null, transmitterAddress1 = null, transmitterCity = null, transmitterState = null, transmitterZip = null,
                    transmitterAddress2 = null, transmitterAddress3 = null, transmitterPhone = null;
                // Read the TAX.FORM.1099MI.REPOS record for the demographic information
                var miReposContract = await DataReader.ReadRecordAsync<TaxForm1099miRepos>(currentMiDetailRecord.TmidtlrReposId);
                if (miReposContract == null)
                {
                    throw new ApplicationException("1099Mi Repos cannot be null.");
                }
                //Get the corp details from EIN
                var corpCriteria = "WITH CORP.TAX.ID EQ '" + miReposContract.TfmirEin + "'";
                var corpData = await DataReader.BulkReadRecordAsync<CorpFounds>(corpCriteria);
                if (corpData != null && corpData.Any())
                {
                    transmitterId = corpData.Select(x => x.Recordkey).FirstOrDefault();
                };

                if(transmitterId == null)
                {
                    var transmitterEntity = parm1099MIContract.TransmitterDmEntityAssociation.FirstOrDefault(x => x.P1099miTTransmitterTinAssocMember == miReposContract.TfmirEin);
                    if(transmitterEntity!=null)
                    {
                        transmitterId = transmitterEntity.P1099miTTransmitterIdAssocMember;
                    }
                }
                //Get the Transmitter Default Name
                if (transmitterId != null)
                {
                    var corp = await DataReader.ReadRecordAsync<Corp>("PERSON", transmitterId);
                    if (corp != null)
                    {
                        transmitterName = corp.CorpName.FirstOrDefault();
                    }
                }

                // Get the employer's contact phone number and extension.
                transmitterPhone = parm1099MIContract.P1099miPhoneNumber;
                if (!transmitterPhone.Contains("-") && transmitterPhone.Length == 10)
                {
                    transmitterPhone = String.Format("{0}-{1}-{2}",
                        transmitterPhone.Substring(0, 3),
                        transmitterPhone.Substring(3, 3),
                        transmitterPhone.Substring(6, 4));
                }
                //appending extension                    
                transmitterPhone = transmitterPhone + " " + parm1099MIContract.P1099miPhoneExtension;
                // Set the Payer Name and Address details
                if (parm1099MIContract.P1099miTTransmitterId.Any(x => x == transmitterId))
                {
                    var currentTransmitter = parm1099MIContract.TransmitterDmEntityAssociation.FirstOrDefault(x => x.P1099miTTransmitterIdAssocMember == transmitterId);
                    if (currentTransmitter != null)
                    {
                        transmitterName = currentTransmitter.P1099miTCoNameAssocMember ?? transmitterName;
                        transmitterName2 = currentTransmitter.P1099miTCoName2AssocMember ?? "";
                        transmitterAddress1 = currentTransmitter.P1099miTCoAddrAssocMember ?? "";
                        transmitterCity = currentTransmitter.P1099miTCoCityAssocMember ?? "";
                        transmitterState = currentTransmitter.P1099miTStateAssocMember ?? "";
                        transmitterZip = currentTransmitter.P1099miTZipAssocMember ?? "";
                        transmitterAddress2 = transmitterCity + " " + transmitterState + " " + transmitterZip;
                        transmitterAddress3 = string.Empty;
                    }
                }
                else
                {
                    TxGetHierarchyAddressResponse response = await transactionInvoker.ExecuteAsync<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                    new TxGetHierarchyAddressRequest()
                    {
                        IoPersonId = transmitterId,
                        InHierarchy = "TN99",
                        InDate = DateTime.Today
                    });

                    var transmitterAddressLines = response == null || (String.IsNullOrEmpty(response.OutAddressId)) ? null : GetAddressLabel(response);

                    if (transmitterAddressLines != null)
                    {
                        // Combine address lines 1 and 2
                        transmitterAddress1 = transmitterAddressLines.ElementAtOrDefault(0) ?? "";
                        if (!string.IsNullOrEmpty(transmitterAddressLines.ElementAtOrDefault(1)))
                        {
                            transmitterAddress1 += " " + transmitterAddressLines.ElementAtOrDefault(1);
                        }

                        // Combine address lines 3 and 4
                        transmitterAddress2 = transmitterAddressLines.ElementAtOrDefault(2) ?? "";
                        if (!string.IsNullOrEmpty(transmitterAddressLines.ElementAtOrDefault(3)))
                        {
                            transmitterAddress2 += " " + transmitterAddressLines.ElementAtOrDefault(3);
                        }
                    }
                }
                //Assigning Recipient Address details
                //set the recipients name and address lines
                //get the details from TFMIR, if it is an M record, else get the certify level parameters
                string recipientName = null, recipientSecondName = null, recipientAddress1 = null, recipientAddress2 = null, recipientAddress3 = null, recipientPhone = null, recipientIdentificationNumber = null, directResale = null;
                if (currentMiDetailRecord.TmidtlrRefId == "M")
                {
                    recipientName = miReposContract.TfmirName;
                    recipientSecondName = miReposContract.TfmirSecondName;
                    recipientAddress1 = miReposContract.TfmirAddress;
                    recipientAddress2 = miReposContract.TfmirAddressLine2 + " " + miReposContract.TfmirAddressLine3;
                    recipientAddress3 = miReposContract.TfmirCity + ", " + miReposContract.TfmirState + " " + miReposContract.TfmirZip;
                    recipientIdentificationNumber = miReposContract.TfmirTin;
                    directResale = miReposContract.TfmirDirectResale;
                }
                else
                {
                    //Get the latest certified name
                    recipientName = miReposContract.TfmirCertifyName.FirstOrDefault() ?? "";
                    recipientSecondName = miReposContract.TfmirCertifySecondName.FirstOrDefault() ?? "";
                    recipientAddress1 = miReposContract.TfmirCertifyAddress.FirstOrDefault() ?? "";
                    recipientAddress2 = (miReposContract.TfmirCertifyAddressLine2.FirstOrDefault() ?? "") + " " + (miReposContract.TfmirCertifyAddressLine3.FirstOrDefault() ?? "");
                    recipientAddress3 = (miReposContract.TfmirCertifyCity.FirstOrDefault() ?? "") + ", " + (miReposContract.TfmirCertifyState.FirstOrDefault() ?? "") + " " + (miReposContract.TfmirCertifyZip.FirstOrDefault() ?? "");
                    recipientIdentificationNumber = miReposContract.TfmirCertifyTin.FirstOrDefault() ?? "";
                    directResale = miReposContract.TfmirCertifyDirectResale.FirstOrDefault() ?? "";
                }
                //Assigning the State-Payer Ein 
                string statePayerEin = currentMiDetailRecord.TmidtlrStateId;
                string stateInstitutionRecordKey = currentMiDetailRecord.TmidtlrStateId + "*" + transmitterId;
                var states1099OrgContract = await DataReader.ReadRecordAsync<States1099Org>(stateInstitutionRecordKey);
                if (states1099OrgContract != null)
                {
                    if (!string.IsNullOrWhiteSpace(states1099OrgContract.St1099OrgEin))
                    {
                        statePayerEin = statePayerEin + " " + states1099OrgContract.St1099OrgEin;
                    }
                }
                // Instantiate the domain entity.
                string taxYear = currentMiDetailRecord.TmidtlrYear.ToString();
                domainEntity1099Mi = new Form1099MIPdfData(taxYear, transmitterName);
                domainEntity1099Mi.IsCorrected = currentMiDetailRecord.TmidtlrStatus == "G" || currentMiDetailRecord.TmidtlrStatus == "C";
                domainEntity1099Mi.IsDirectResale = directResale == "Y" ? true :false;
                //payers ein
                domainEntity1099Mi.PayersEin = miReposContract.TfmirEin;

                if (string.IsNullOrWhiteSpace(transmitterName2))
                {
                    transmitterName2 = transmitterAddress1;
                    transmitterAddress1 = transmitterAddress2;
                    transmitterAddress2 = transmitterAddress3;
                }

                // Assign the payer address lines.
                domainEntity1099Mi.PayerAddressLine1 = transmitterName2 ?? "";
                domainEntity1099Mi.PayerAddressLine2 = transmitterAddress1 ?? "";
                domainEntity1099Mi.PayerAddressLine3 = transmitterAddress2 ?? "";
                domainEntity1099Mi.PayerAddressLine4 = transmitterPhone ?? "";

                if (string.IsNullOrWhiteSpace(recipientSecondName))
                {
                    recipientSecondName = recipientAddress1;
                    if (string.IsNullOrWhiteSpace(recipientAddress2))
                    {
                        recipientAddress1 = recipientAddress3;
                        recipientAddress2 = "";
                        recipientAddress3 = "";
                    }
                    else
                    {
                        recipientAddress1 = recipientAddress2;
                        recipientAddress2 = recipientAddress3;
                        recipientAddress3 = "";
                    }
                    
                }
                //assign recipient details
                domainEntity1099Mi.RecipientId = currentMiDetailRecord.TmidtlrVendorId;
                domainEntity1099Mi.RecipientsName = recipientName;
                domainEntity1099Mi.RecipientSecondName = recipientSecondName;
                domainEntity1099Mi.RecipientAddr1 = recipientAddress1 ?? "";
                domainEntity1099Mi.RecipientAddr2 = recipientAddress2 ?? "";
                domainEntity1099Mi.RecipientAddr3 = recipientAddress3 ?? "";
                domainEntity1099Mi.RecipientAddr4 = recipientPhone ?? "";
                domainEntity1099Mi.Ein = recipientIdentificationNumber ?? "";
                domainEntity1099Mi.RecipientAccountNumber = currentMiDetailRecord.TmidtlrVendorId;
                domainEntity1099Mi.State = currentMiDetailRecord.TmidtlrStateId;
                domainEntity1099Mi.StatePayerNumber = string.IsNullOrEmpty(statePayerEin) ? string.Empty : statePayerEin;

                // Mask the SSN if necessary.
                //First identify the masking is needed or not
                bool isCorpVendor = false;
                if (!string.IsNullOrEmpty(taxYearRecord.TfmyMaskSsn) && taxYearRecord.TfmyMaskSsn.ToUpper() == "Y")
                {
                    var personContract = await DataReader.ReadRecordAsync<Person>("PERSON", currentMiDetailRecord.TmidtlrVendorId);
                    if (personContract.PersonCorpIndicator.ToUpperInvariant() == "Y")
                    {
                        isCorpVendor = true;
                    }
                    if (!string.IsNullOrEmpty(domainEntity1099Mi.Ein) && !isCorpVendor)
                    {
                        // Mask SSN
                        if (domainEntity1099Mi.Ein.Length >= 4)
                        {
                            domainEntity1099Mi.Ein = "XXX-XX-" + domainEntity1099Mi.Ein.Substring(domainEntity1099Mi.Ein.Length - 4);
                        }
                        else
                        {
                            domainEntity1099Mi.Ein = "XXX-XX-" + domainEntity1099Mi.Ein;
                        }
                    }
                }

                //Get the box data
                TaxFormBoxesPdfData boxData = null;
                foreach (var boxInfoEntity in currentMiDetailRecord.TmidtlrBoxInfoEntityAssociation)
                {
                    boxData = new TaxFormBoxesPdfData(boxInfoEntity.TmidtlrBoxNumberAssocMember, boxInfoEntity.TmidtlrAmtAssocMember ?? default(decimal));
                    domainEntity1099Mi.TaxFormBoxesList.Add(boxData);
                }
                //Tax Year 2013 specific logic for Foreign Tax Paid and Box Country
                if (currentMiDetailRecord.TmidtlrYear == "2013")
                {
                    var box11 = domainEntity1099Mi.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "11");
                    if (box11 != null)
                    {
                        domainEntity1099Mi.ForeignTaxPaid = Math.Abs(box11.Amount).ToString("N2");
                    }
                    var boxCodeCriteria = "WITH BXC.TAX.FORM EQ '1099MI' AND WITH BXC.BOX.NUMBER EQ '11'";
                    var boxCountry = await DataReader.BulkReadRecordAsync<BoxCodes>(boxCodeCriteria);
                    if(boxCountry != null && boxCountry.Any())
                    {
                        domainEntity1099Mi.BoxCountry = boxCountry.Select(x => x.BxcCountry).FirstOrDefault();
                    }
                }
                // Process the boxes and their amounts.
                var boxNumber = string.Empty;
                domainEntity1099Mi.TaxFormBoxesList = domainEntity1099Mi.TaxFormBoxesList.OrderBy(x => x.BoxNumber).ToList();
                // Call the PDF accessed CTX to send an email notification
                TxNotifyCfPdfAccessRequest pdfRequest = new TxNotifyCfPdfAccessRequest();
                pdfRequest.AFormType = "1099MI";
                pdfRequest.APersonId = currentMiDetailRecord.TmidtlrVendorId;
                pdfRequest.ARecordId = recordId;
                var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyCfPdfAccessRequest, TxNotifyCfPdfAccessResponse>(pdfRequest);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return domainEntity1099Mi;
        }


        /// <summary>
        /// Get an address label from an Address entity
        /// </summary>
        /// <param name="address">Address domain entity</param>
        /// <returns>IEnumerable of type string</returns>
        private List<string> GetAddressLabel(TxGetHierarchyAddressResponse address)
        {
            List<string> label = new List<string>();

            if (address.OutAddressLabel.Count > 0)
            {
                label.AddRange(address.OutAddressLabel);
            }
            else
            {
                // Build address label
                if (!String.IsNullOrEmpty(address.OutAddressModifier))
                {
                    label.Add(address.OutAddressModifier);
                }
                if (address.OutAddressLines.Count > 0)
                {
                    label.AddRange(address.OutAddressLines);
                }
                string cityStatePostalCode = AddressProcessor.GetCityStatePostalCode(address.OutAddressCity, address.OutAddressState, address.OutAddressZip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(address.OutAddressCountryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(address.OutAddressCountryDesc.ToUpper());
                }
            }
            return label;
        }
    }
}