// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for ColleagueFinance tax forms
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ColleagueFinanceTaxFormStatementRepository : BaseColleagueRepository, IColleagueFinanceTaxFormStatementRepository
    {
        /// <summary>
        /// ColleagueFinance Tax Form Statement repository constructor.
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public ColleagueFinanceTaxFormStatementRepository(ApiSettings settings, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Retrieve set of tax form statements assigned to the specified person for the tax form type.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            var statements = new List<TaxFormStatement2>();

            // Based on the type of tax form, we will obtain the statements from different entities.
            switch (taxForm)
            {
                case TaxForms.FormT4A:
                    statements = (await GetT4ATaxStatements(personId)).ToList();
                    break;

                case TaxForms.Form1099MI:
                    statements = (await Get1099MiTaxStatements(personId)).ToList();
                    break;
                default:
                    throw new ArgumentException(taxForm.ToString() + " is not accessible within the Colleague Finance module.", "taxForm");
            }

            return statements;
        }

        /// <summary>
        /// Obtain the available list of T4A statements for this person.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>List of T4A statement objects.</returns>
        private async Task<IEnumerable<TaxFormStatement2>> GetT4ATaxStatements(string personId)
        {
            var statements = new List<TaxFormStatement2>();

            var parmT4aContract = await DataReader.ReadRecordAsync<ParmT4a>("CF.PARMS", "T4A");
            if (parmT4aContract == null)
            {
                throw new NullReferenceException("PARM.T4A cannot be null.");
            }

            // Read the tax year records which will be needed later.
            Collection<TaxFormT4aYears> taxYearRecords = await DataReader.BulkReadRecordAsync<TaxFormT4aYears>("TAX.FORM.T4A.YEARS", "");

            // Find the all the T4A tax forms for the recipient.
            var taxFormCriteria = "WITH TFTBR.BIN.ID EQ '" + personId + "'";
            var taxFormRecords = await DataReader.BulkReadRecordAsync<TaxFormT4aBinRepos>(taxFormCriteria);
            if (taxFormRecords != null)
            {
                // Sort the statements.
                var sortedT4aStatementRecords = taxFormRecords.Where(x => x != null).OrderByDescending(x => x.TftbrYear);

                // Loop through each tax year. We do not need to remove duplicate years because there may be multiple institutions issuing T4As.
                foreach (var taxRecord in sortedT4aStatementRecords)
                {
                    try
                    {
                        if (taxRecord.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (taxRecord != null && !string.IsNullOrEmpty(taxRecord.TftbrBinId) && taxRecord.TftbrBinId == personId && taxRecord.TftbrYear.HasValue)
                        {
                            string statusFlag = null;
                            string refLevel = "0";
                            string level = null;

                            // Check the year being processed.
                            if (taxRecord.TftbrYear.ToString() == parmT4aContract.Pt4aYear)
                            {
                                // They are requesting the current year.
                                // Find the corrected level or the new data level.
                                if (taxRecord.TftbrStatus.ToUpperInvariant() == "U" || taxRecord.TftbrStatus.ToUpperInvariant() == "")
                                {
                                    // The data is in a frozen or submitted status. Find the level were the
                                    // data was corrected (C or G status) or were it was new (N status).
                                    for (int i = 0; i < taxRecord.TftbrCertEntityAssociation.Count(); i++)
                                    {
                                        var assocMember = taxRecord.TftbrCertEntityAssociation[i];
                                        statusFlag = assocMember.TftbrCertStatusAssocMember;
                                        refLevel = i.ToString();
                                        if (statusFlag != "U")
                                        {
                                            break;
                                        }
                                    }

                                    // Get the record for the year and obtain the list of submitted levels
                                    var taxYearRecord = taxYearRecords.Where(x => x.TftyTaxYear == taxRecord.TftbrYear).FirstOrDefault();

                                    if (taxYearRecord != null)
                                    {
                                        // Find the level to use for this tax form.
                                        var submittedAssoc = taxYearRecord.TftySubmittedEntityAssociation[Convert.ToInt32(refLevel)];
                                        level = submittedAssoc.TftySubmitSeqNosAssocMember;
                                    }
                                }
                                else
                                {
                                    // Use the M level.
                                    level = "M";
                                    refLevel = "M";
                                    statusFlag = taxRecord.TftbrStatus;
                                }
                            }
                            else
                            {
                                // Not the current year. We ignore the verified level if the data is in a frozen status, even if it
                                // was corrected. Find the corrected (C or G) level or the new (N) data level in the submitted data.
                                for (int i = 0; i < taxRecord.TftbrCertEntityAssociation.Count(); i++)
                                {
                                    var assocMember = taxRecord.TftbrCertEntityAssociation[i];
                                    statusFlag = assocMember.TftbrCertStatusAssocMember;
                                    refLevel = i.ToString();
                                    if (statusFlag != "U")
                                    {
                                        break;
                                    }
                                }

                                // Get the record for the year and obtain the list of submitted levels
                                var taxYearRecord = taxYearRecords.Where(x => x.TftyTaxYear == taxRecord.TftbrYear).FirstOrDefault();

                                if (taxYearRecord != null)
                                {
                                    // Find the level to use for this tax form.
                                    var submittedAssoc = taxYearRecord.TftySubmittedEntityAssociation[Convert.ToInt32(refLevel)];
                                    level = submittedAssoc.TftySubmitSeqNosAssocMember;
                                }
                            }

                            // Get the box number/footnotes and associated amounts for the CF/HR/ST TAX.T4A.DETAIL records for the level.
                            var detailCriteria = "WITH TTDR.YEAR EQ '" + taxRecord.TftbrYear + "' AND WITH TTDR.ID EQ '" + taxRecord.TftbrBinId +
                                "' AND WITH TTDR.BIN.ID EQ '" + taxRecord.TftbrBinCode + "' AND WITH TTDR.REF.ID EQ '" + level + "'";
                            var detailTaxRecords = await DataReader.BulkReadRecordAsync<TaxT4aDetailRepos>(detailCriteria);

                            List<string> boxNumberSub = new List<string>();
                            List<decimal?> boxNumberAmounts = new List<decimal?>();

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
                                            boxNumberAmounts[loc] += boxAssocMember.TtdrAmtAssocMember.HasValue ? boxAssocMember.TtdrAmtAssocMember : 0m;
                                        }
                                        else
                                        {
                                            boxNumberSub.Add(boxAssocMember.TtdrBoxNumberSubAssocMember);
                                            boxNumberAmounts.Add(boxAssocMember.TtdrAmtAssocMember);
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

                                //If any RESP accumulated income payments totaling
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
                                if (boxNumberAmounts.Sum() >= parmT4aContract.Pt4aMinTotAmt)
                                {
                                    isIncluded = true;
                                }

                                if (isIncluded)
                                {
                                    if (!string.IsNullOrEmpty(taxRecord.TftbrBinId) && taxRecord.TftbrYear.HasValue)
                                    {
                                        var statementT4a = new TaxFormStatement2(taxRecord.TftbrBinId,
                                            taxRecord.TftbrYear.ToString(), TaxForms.FormT4A, taxRecord.Recordkey);

                                        // Add the statement to the list
                                        statements.Add(statementT4a);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                    }
                }
            }

            // Update the tax form notation depending on whether the institution has enabled viewing on the web.
            // If processing for the current year has not been at least frozen once, the current year will not
            // be in the repository, and will not be available for display.

            foreach (var statement in statements)
            {
                // Get the record for the year and obtain the list of submitted levels
                var taxYearRecord = taxYearRecords.Where(x => x.TftyTaxYear.ToString() == statement.TaxYear).FirstOrDefault();
                if (taxYearRecord != null)
                {
                    if (taxYearRecord.TftyWebEnabled.ToUpperInvariant() != "Y")
                    {
                        statement.Notation = TaxFormNotations.NotAvailable;
                    }
                }
            }
            return statements;
        }

        /// <summary>
        /// Obtain the available list of 1099MI statements for this person.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>List of 1099MI statement objects.</returns>
        private async Task<IEnumerable<TaxFormStatement2>> Get1099MiTaxStatements(string personId)
        {
            // Initialize the statements to return.
            var statements = new List<TaxFormStatement2>();

            var parm1099MIContract = await DataReader.ReadRecordAsync<Parm1099mi>("CF.PARMS", "PARM.1099MI");
            if (parm1099MIContract == null)
            {
                throw new NullReferenceException("PARM.1099MI cannot be null.");
            }

            var taxFormStatus = await DataReader.ReadRecordAsync<TaxFormStatus>("TAX.FORM.STATUS", "1099MI");
            if (taxFormStatus == null)
            {
                throw new NullReferenceException("TAX.FORM.STATUS cannot be null.");
            }

            // Read the tax year records for 1099MI.
            Collection<TaxForm1099miYears> taxYearRecords = await DataReader.BulkReadRecordAsync<TaxForm1099miYears>("TAX.FORM.1099MI.YEARS", "");

            // Find the all the 1099MI tax forms detail records for the recipient.
            var taxFormDetailCriteria = "WITH TMIDTLR.VENDOR.ID EQ '" + personId + "'";
            var taxFormDetailRepos = await DataReader.BulkReadRecordAsync<Tax1099miDetailRepos>(taxFormDetailCriteria);

            // If the recipient does not have any records, return an empty list.
            // We do not want to return an exception, simply present them with no statements in the page.
            if (taxFormDetailRepos == null || !taxFormDetailRepos.Any())
            {
                return statements;
            }
            // A temporary filter the recipient detail records to get only those for which we have tax years template is ready in the product
            // This will be removed once all the tax year templates are ready for 1099MI tax forms
            string[] includedYearsFor1099Mi = ColleagueFinanceConstants.IncludedYearsFor1099mi.Split(',');
            var filteredTaxFormDetailRepos = taxFormDetailRepos.Where(taxform => includedYearsFor1099Mi.Contains(taxform.TmidtlrYear));
            Tax1099miDetailRepos miLatestRecord = null;
            try
            {
                // Get a distinct list of tax forms by Year-State-Ein by descending tax year.
                var list = filteredTaxFormDetailRepos.Select(t => new { t.TmidtlrYear, t.TmidtlrStateId, t.TmidtlrEin }).Distinct().OrderByDescending(x => x.TmidtlrYear);

                foreach (var taxFormData in list)
                {
                    // Get the TAX.FORM.1099MI.YEARS record for the tax year in this tax form.
                    var miYear = taxYearRecords.FirstOrDefault(x => x.Recordkey == taxFormData.TmidtlrYear);
                    // We need the tax year record to see if the web enabled flag is set to Yes for the notation.
                    if (miYear != null)
                    {
                        // Get the recipient's detail records for this tax year, state and EIN.
                        var miRecords = filteredTaxFormDetailRepos.Where(x => x.TmidtlrYear == taxFormData.TmidtlrYear && x.TmidtlrStateId == taxFormData.TmidtlrStateId && x.TmidtlrEin == taxFormData.TmidtlrEin && x.TmidtlrQualifiedFlag == "Y");
                        if (miRecords != null && miRecords.Any())
                        {
                            // Check if the record is for the current tax year.
                            if (parm1099MIContract.P1099miYear == taxFormData.TmidtlrYear)
                            {
                                if (miRecords.Any(x => x.TmidtlrRefId == "M" && x.TmidtlrQualifiedFlag == "Y" && x.TmidtlrStatus != "U"))
                                {
                                    miLatestRecord = miRecords.Where(x => x.TmidtlrRefId == "M").FirstOrDefault();
                                }
                                else
                                {
                                    // The tax form year is current year which do not have valid M Record
                                    // Get the record that has the most recent submit sequence number with valid status 
                                    miLatestRecord = GetMostRecentSubmittedValidRecord(miLatestRecord, miYear, miRecords);
                                }
                            }
                            else
                            {
                                // The tax form year is a past year
                                // Get the record that has the most recent submit sequence number with valid status 
                                miLatestRecord = GetMostRecentSubmittedValidRecord(miLatestRecord, miYear, miRecords);
                            }
                        }
                        if (miLatestRecord != null)
                        {
                            if (miLatestRecord.Recordkey == null)
                            {
                                throw new NullReferenceException("RecordKey is a required field");
                            }
                            //Add the record to the statement
                            AddRecordToStatements(statements, miLatestRecord, miYear);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
            }
            return statements;
        }

        private static Tax1099miDetailRepos GetMostRecentSubmittedValidRecord(Tax1099miDetailRepos miLatestRecord, TaxForm1099miYears miYear, IEnumerable<Tax1099miDetailRepos> miRecords)
        {
            if (miYear.TfmySubmitSeqNos.Any())
            {
                foreach (var seqNo in miYear.TfmySubmitSeqNos)
                {
                    miLatestRecord = miRecords.Where(x => x.TmidtlrRefId == seqNo && x.TmidtlrStatus != "U").FirstOrDefault();
                    if (miLatestRecord != null)
                        break;
                }
            }
            return miLatestRecord;
        }

        private static void AddRecordToStatements(List<TaxFormStatement2> statements, Tax1099miDetailRepos miLatestRecord, TaxForm1099miYears miYear)
        {
            var latestStatement = new TaxFormStatement2(miLatestRecord.TmidtlrVendorId,
               miLatestRecord.TmidtlrYear.ToString(), TaxForms.Form1099MI, miLatestRecord.Recordkey);
            //Set the notation for the statement
            latestStatement.Notation = miYear.TfmyWebEnabled == "Y" && miLatestRecord.TmidtlrQualifiedFlag == "Y" ? TaxFormNotations.None : TaxFormNotations.NotAvailable;
            statements.Add(latestStatement);
        }
    }
}



