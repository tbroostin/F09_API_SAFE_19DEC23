// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Utility;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Service for tax form pdfs.
    /// </summary>
    [RegisterType]
    public class ColleagueFinanceTaxFormPdfService : BaseCoordinationService, IColleagueFinanceTaxFormPdfService
    {
        public const string ReportType = "PDF";
        public const string DeviceInfo = "<DeviceInfo>" +
                                         " <OutputFormat>PDF</OutputFormat>" +
                                         "</DeviceInfo>";

        private readonly IColleagueFinanceTaxFormPdfDataRepository taxFormPdfDataRepository;

        // Constructor TaxFormPdfService
        public ColleagueFinanceTaxFormPdfService(IColleagueFinanceTaxFormPdfDataRepository taxFormPdfDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormPdfDataRepository = taxFormPdfDataRepository;
        }

        /// <summary>
        /// Retrieves a FormT4aPdfData DTO.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>
        /// <returns>TaxFormT4APdfData domain entity</returns>
        public async Task<FormT4aPdfData> GetFormT4aPdfDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessToT4a(personId))
                throw new PermissionsException("Insufficient access to T4a data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetFormT4aPdfDataAsync(personId, recordId);

                // Validate that the domain entity recipient ID is the same as the person ID requested.
                if (taxFormPdfData.RecipientId != personId)
                {
                    throw new PermissionsException("Insufficient access to T4A data.");
                }

                return taxFormPdfData;
            }
            catch (Exception e)
            {
                // Log the error and throw the exception that was given
                logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a Form1099MIPdfData DTO.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>
        /// <returns>Form1099MIPdfData domain entity</returns>
        public async Task<Form1099MIPdfData> Get1099MiscPdfDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!(HasPermission(ColleagueFinancePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
            {
                throw new PermissionsException("Insufficient access to 1099-MISC data.");
            }

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetForm1099MiPdfDataAsync(personId, recordId);

                // Validate that the domain entity recipient ID is the same as the person ID requested.
                if (personId != taxFormPdfData.RecipientId)
                {
                    throw new PermissionsException("Insufficient access to 1099-MISC data.");
                }

                return taxFormPdfData;
            }
            catch (Exception e)
            {
                // Log the error and throw the exception that was given
                logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Populates the T4A PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">T4A PDF data</param>
        /// <param name="documentPath">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the T4A tax form</returns>
        public byte[] PopulateT4aPdf(FormT4aPdfData pdfData, string documentPath)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData");
            }
            if (string.IsNullOrEmpty(documentPath))
            {
                throw new ArgumentNullException("pathToReport");
            }

            byte[] renderedBytes;
            var report = new LocalReport();

            try
            {
                report.ReportPath = documentPath;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();
                parameters.Add(utility.BuildReportParameter("TaxYear", pdfData.TaxYear));
                parameters.Add(utility.BuildReportParameter("Amended", pdfData.Amended));
                parameters.Add(utility.BuildReportParameter("PayersName", pdfData.PayerName));
                parameters.Add(utility.BuildReportParameter("RecipientAccountNumber", pdfData.RecipientAccountNumber));
                parameters.Add(utility.BuildReportParameter("SocialInsuranceNumber", pdfData.Sin));
                parameters.Add(utility.BuildReportParameter("RecipientsName", pdfData.RecipientsName));
                parameters.Add(utility.BuildReportParameter("RecipientAddress1", pdfData.RecipientAddr1));
                parameters.Add(utility.BuildReportParameter("RecipientAddress2", pdfData.RecipientAddr2));
                parameters.Add(utility.BuildReportParameter("RecipientAddress3", pdfData.RecipientAddr3));
                parameters.Add(utility.BuildReportParameter("RecipientAddress4", pdfData.RecipientAddr4));

                parameters.Add(utility.BuildReportParameter("Pension", ConvertDecimalToString(pdfData.Pension)));
                parameters.Add(utility.BuildReportParameter("LumpSumPayment", ConvertDecimalToString(pdfData.LumpSumPayment)));
                parameters.Add(utility.BuildReportParameter("SelfEmployedCommissions", ConvertDecimalToString(pdfData.SelfEmployedCommissions)));
                parameters.Add(utility.BuildReportParameter("IncomeTaxDeducted", ConvertDecimalToString(pdfData.IncomeTaxDeducted)));
                parameters.Add(utility.BuildReportParameter("Annuities", ConvertDecimalToString(pdfData.Annuities)));
                parameters.Add(utility.BuildReportParameter("FeesForServices", ConvertDecimalToString(pdfData.FeesForServices)));

                // Populate the first 12 "dynamic" boxes.
                for (int i = 0; i < 12; i++)
                {
                    int boxNumber = i + 1;
                    var box = pdfData.TaxFormBoxesList.ElementAtOrDefault(i);
                    if (box != null)
                    {
                        parameters.Add(utility.BuildReportParameter("Box" + boxNumber, box.BoxNumber));
                        parameters.Add(utility.BuildReportParameter("Amount" + boxNumber, ConvertDecimalToString(box.Amount)));
                    }
                    else
                    {
                        parameters.Add(utility.BuildReportParameter("Box" + boxNumber, ""));
                        parameters.Add(utility.BuildReportParameter("Amount" + boxNumber, ""));
                    }
                }

                // Set the report parameters
                report.SetParameters(parameters);

                // Set up some options for the report
                string mimeType = string.Empty;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                // Render the report as a byte array
                renderedBytes = report.Render(
                    ReportType,
                    DeviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);
            }
            catch
            {
                // Rethrow exception
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
            return renderedBytes;
        }

        /// <summary>
        /// Populates the 1099-MISC PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">1099-MISC PDF data</param>
        /// <param name="documentPath">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the 1099-MISC tax form</returns>
        public byte[] Populate1099MiscPdf(Form1099MIPdfData pdfData, string documentPath)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData");
            }
            if (string.IsNullOrEmpty(documentPath))
            {
                throw new ArgumentNullException("pathToReport");
            }

            byte[] renderedBytes;
            var report = new LocalReport();

            try
            {
                report.ReportPath = documentPath;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();

                parameters.Add(utility.BuildReportParameter("Year2", pdfData.TaxYear.Substring(2)));
                parameters.Add(utility.BuildReportParameter("PayersName", pdfData.PayerName));
                parameters.Add(utility.BuildReportParameter("PayersNameAddr1", pdfData.PayerAddressLine1));
                parameters.Add(utility.BuildReportParameter("PayersNameAddr2", pdfData.PayerAddressLine2));
                parameters.Add(utility.BuildReportParameter("PayersNameAddr3", pdfData.PayerAddressLine3));
                parameters.Add(utility.BuildReportParameter("PayersNameAddr4", pdfData.PayerAddressLine4));
                parameters.Add(utility.BuildReportParameter("RecipientsName", pdfData.RecipientsName));
                parameters.Add(utility.BuildReportParameter("RecipientAddress1", pdfData.RecipientSecondName));
                parameters.Add(utility.BuildReportParameter("RecipientAddress2", pdfData.RecipientAddr1));
                parameters.Add(utility.BuildReportParameter("RecipientAddress3", pdfData.RecipientAddr2));
                parameters.Add(utility.BuildReportParameter("RecipientAddress4", pdfData.RecipientAddr3));

                parameters.Add(utility.BuildReportParameter("CorrectedForm", pdfData.IsCorrected ? "X" : ""));

                parameters.Add(utility.BuildReportParameter("PayersEIN", pdfData.PayersEin));
                parameters.Add(utility.BuildReportParameter("RecipientsEIN", pdfData.Ein));
                parameters.Add(utility.BuildReportParameter("FATCAFiling", ""));
                parameters.Add(utility.BuildReportParameter("AccountNumber", pdfData.AccountNumber));

                var taxFormBoxesList = pdfData.TaxFormBoxesList;
                parameters.Add(utility.BuildReportParameter("Box1Amt", GetAmountFromBoxList(taxFormBoxesList, "1")));
                parameters.Add(utility.BuildReportParameter("Box2Amt", GetAmountFromBoxList(taxFormBoxesList, "2")));
                parameters.Add(utility.BuildReportParameter("Box3Amt", GetAmountFromBoxList(taxFormBoxesList, "3")));
                parameters.Add(utility.BuildReportParameter("Box4Amt", GetAmountFromBoxList(taxFormBoxesList, "4")));
                parameters.Add(utility.BuildReportParameter("Box5Amt", GetAmountFromBoxList(taxFormBoxesList, "5")));
                parameters.Add(utility.BuildReportParameter("Box6Amt", GetAmountFromBoxList(taxFormBoxesList, "6")));
                parameters.Add(utility.BuildReportParameter("Box7Amt", GetAmountFromBoxList(taxFormBoxesList, "7")));
                parameters.Add(utility.BuildReportParameter("Box8Amt", GetAmountFromBoxList(taxFormBoxesList, "8")));
                parameters.Add(utility.BuildReportParameter("Box9", pdfData.IsDirectResale ? "X" : ""));
                parameters.Add(utility.BuildReportParameter("Box10Amt", GetAmountFromBoxList(taxFormBoxesList, "10")));
                parameters.Add(utility.BuildReportParameter("Box11Amt", pdfData.ForeignTaxPaid ?? ""));
                parameters.Add(utility.BuildReportParameter("Box12Amt", pdfData.BoxCountry ?? ""));
                parameters.Add(utility.BuildReportParameter("Box13Amt", GetAmountFromBoxList(taxFormBoxesList, "13")));
                parameters.Add(utility.BuildReportParameter("Box14Amt", GetAmountFromBoxList(taxFormBoxesList, "14")));
                parameters.Add(utility.BuildReportParameter("Box15aAmt", GetAmountFromBoxList(taxFormBoxesList, "15A")));
                parameters.Add(utility.BuildReportParameter("Box15bAmt", GetAmountFromBoxList(taxFormBoxesList, "15B")));
                parameters.Add(utility.BuildReportParameter("Box16Amt", GetAmountFromBoxList(taxFormBoxesList, "16")));
                parameters.Add(utility.BuildReportParameter("Box16bAmt", ""));
                parameters.Add(utility.BuildReportParameter("Box17", pdfData.StatePayerNumber == null ? "" : pdfData.StatePayerNumber));
                parameters.Add(utility.BuildReportParameter("Box17b", ""));
                parameters.Add(utility.BuildReportParameter("Box18Amt", GetAmountFromBoxList(taxFormBoxesList, "18")));
                parameters.Add(utility.BuildReportParameter("Box18bAmt", ""));


                // Set the report parameters
                report.SetParameters(parameters);

                // Set up some options for the report
                string mimeType = string.Empty;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                // Render the report as a byte array
                renderedBytes = report.Render(
                    ReportType,
                    DeviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);   
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate 1099-Misc report.");
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
            return renderedBytes;
        }

        /// <summary>
        /// Convert the input decimal value into a string without the '$'.
        /// </summary>
        /// <param name="amount">Input amount</param>
        /// <returns>String representation of input value.</returns>
        private string ConvertDecimalToString(decimal amount)
        {
            if (amount != 0)
            {
                return amount.ToString("N2").Replace(",", "").Replace(".", " ");
            }
            else
            {
                return " ";
            }
        }

        /// <summary>
        /// Get amount from taxFormBoxesList by passing box number and convert decimal value to string (N2) 
        /// </summary>
        /// <param name="taxFormBoxesList">Input list</param>
        /// <param name="boxNumber">Input box number</param>
        /// <returns>string value of the amount</returns>
        private string GetAmountFromBoxList(List<TaxFormBoxesPdfData> taxFormBoxesList, string boxNumber)
        {
            string value = string.Empty;
            if (!taxFormBoxesList.Any(x => x.BoxNumber == boxNumber))
            {
                return value;
            }
            var amount = Math.Abs(taxFormBoxesList.FirstOrDefault(x => x.BoxNumber == boxNumber).Amount);
            return amount == 0 ? value : amount.ToString("N2");
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's T4A tax form
        /// A person can only view a T4A form for:
        /// 1. Himself/herself with VIEW.T4A permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.T4A permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users T4A form with VIEW.RECIPIENT.T4A permission
        /// </summary>
        private bool HasAccessToT4a(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(ColleagueFinancePermissionCodes.ViewRecipientT4A);
            }
            else
            {
                return HasPermission(ColleagueFinancePermissionCodes.ViewT4A);
            }
        }
    }
}