// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Utilities;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Service for tax form pdfs.
    /// </summary>
    [RegisterType]
    public class HumanResourcesTaxFormPdfService : BaseCoordinationService, IHumanResourcesTaxFormPdfService
    {
        public const string ReportType = "PDF";
        public const string DeviceInfo = "<DeviceInfo>" +
                                         " <OutputFormat>PDF</OutputFormat>" +
                                         "</DeviceInfo>";

        private IHumanResourcesTaxFormPdfDataRepository taxFormPdfDataRepository;

        /// <summary>
        /// Constructor TaxFormPdfService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public HumanResourcesTaxFormPdfService(IHumanResourcesTaxFormPdfDataRepository taxFormPdfDataRepository,
            IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormPdfDataRepository = taxFormPdfDataRepository;
        }

        /// <summary>
        /// Gets the boolean value that indicates if the client is set up to use the Guam version of the W2 form.
        /// </summary>
        /// <returns>Boolean value where true = Guam and false = USA</returns>
        public async Task<bool> GetW2GuamFlag()
        {
            return await taxFormPdfDataRepository.GetW2GuamFlag();
        }

        /// <summary>
        /// Returns the pdf data to print a W-2 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">The record ID where the W-2 pdf data is stored</param>
        /// <returns>Byte array containing PDF data for the W-2 tax form</returns>
        public async Task<FormW2PdfData> GetW2TaxFormDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessToW2(personId))
                throw new PermissionsException("Insufficient access to W-2 data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetW2PdfAsync(personId, recordId);

                // Validate that the domain entity employee ID is the same as the person ID requested.
                // Ensures record belongs to the specified person - specified above
                // Do not remove this check
                if (personId != taxFormPdfData.EmployeeId)
                {
                    throw new PermissionsException("Insufficient access to W2 data.");
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
        /// Returns the pdf data to print a W-2c tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2c.</param>
        /// <param name="recordId">The record ID where the W-2c pdf data is stored</param>
        /// <returns>Byte array containing PDF data for the W-2ctax form</returns>
        public async Task<FormW2cPdfData> GetW2cTaxFormData(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (personId != CurrentUser.PersonId && !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeW2))
                throw new PermissionsException("Insufficient access to W-2c data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetW2cPdfAsync(personId, recordId);

                // Validate that the domain entity employee ID is the same as the person ID requested.
                // Ensures record belongs to the specified person - specified above
                // Do not remove this check
                if (personId != taxFormPdfData.EmployeeId)
                {
                    throw new PermissionsException("Insufficient access to W2 data.");
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
        /// Populates the W-2c PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">W-2c PDF data</param>
        /// <param name="pathToReport">Path indicating the path to the report on the server</param>
        /// <returns>Byte array containing PDF data for the W-2c tax form</returns>
        public byte[] PopulateW2cPdfReport(FormW2cPdfData pdfData, string pathToReport)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }

            byte[] renderedBytes;
            var report = new LocalReport();

            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();
                parameters.Add(utility.BuildReportParameter("Tax_Year", pdfData.CorrectionYear));
                parameters.Add(utility.BuildReportParameter("Box_a_name", pdfData.EmployerName));
                parameters.Add(utility.BuildReportParameter("Box_a_addr_1", pdfData.EmployerAddressLine1));
                parameters.Add(utility.BuildReportParameter("Box_a_addr_2", pdfData.EmployerAddressLine2));
                parameters.Add(utility.BuildReportParameter("Box_a_addr_3", pdfData.EmployerAddressLine3));
                parameters.Add(utility.BuildReportParameter("Box_a_addr_4", pdfData.EmployerAddressLine4));
                parameters.Add(utility.BuildReportParameter("Box_e", pdfData.ChangesSsnOrName));
                parameters.Add(utility.BuildReportParameter("Box_h_addr_1", pdfData.EmployeeAddressLine1));
                parameters.Add(utility.BuildReportParameter("Box_h_addr_2", pdfData.EmployeeAddressLine2));
                parameters.Add(utility.BuildReportParameter("Box_h_addr_3", pdfData.EmployeeAddressLine3));
                parameters.Add(utility.BuildReportParameter("Box_h_addr_4", pdfData.EmployeeAddressLine4));
                parameters.Add(utility.BuildReportParameter("Box_h_first_and_initial", pdfData.EmployeeFirstName + ' ' + pdfData.EmployeeMiddleName));
                parameters.Add(utility.BuildReportParameter("Box_h_last_name", pdfData.EmployeeLastName));
                parameters.Add(utility.BuildReportParameter("Box_h_suffix", pdfData.EmployeeSuffix));
                parameters.Add(utility.BuildReportParameter("Box_1_correct", pdfData.FederalWages));
                parameters.Add(utility.BuildReportParameter("Box_2_correct", pdfData.FederalWithholding));
                parameters.Add(utility.BuildReportParameter("Box_3_correct", pdfData.SocialSecurityWages));
                parameters.Add(utility.BuildReportParameter("Box_4_correct", pdfData.SocialSecurityWithholding));
                parameters.Add(utility.BuildReportParameter("Box_5_correct", pdfData.MedicareWages));
                parameters.Add(utility.BuildReportParameter("Box_6_correct", pdfData.MedicareWithholding));
                parameters.Add(utility.BuildReportParameter("Box_7_correct", pdfData.SocialSecurityTips));
                parameters.Add(utility.BuildReportParameter("Box_8_correct", pdfData.AllocatedTips));
                parameters.Add(utility.BuildReportParameter("Box_10_correct", pdfData.DependentCare));
                parameters.Add(utility.BuildReportParameter("Box_11_correct", pdfData.NonqualifiedTotal));
                parameters.Add(utility.BuildReportParameter("Box_12a_Code_correct", pdfData.Box12aCode));
                parameters.Add(utility.BuildReportParameter("Box_12a_Amt_correct", pdfData.Box12aAmount));
                parameters.Add(utility.BuildReportParameter("Box_12b_Code_correct", pdfData.Box12bCode));
                parameters.Add(utility.BuildReportParameter("Box_12b_Amt_correct", pdfData.Box12bAmount));
                parameters.Add(utility.BuildReportParameter("Box_12c_Code_correct", pdfData.Box12cCode));
                parameters.Add(utility.BuildReportParameter("Box_12c_Amt_correct", pdfData.Box12cAmount));
                parameters.Add(utility.BuildReportParameter("Box_12d_Code_correct", pdfData.Box12dCode));
                parameters.Add(utility.BuildReportParameter("Box_12d_Amt_correct", pdfData.Box12dAmount));
                parameters.Add(utility.BuildReportParameter("Box_13_1_correct", pdfData.Box13CheckBox1));
                parameters.Add(utility.BuildReportParameter("Box_13_2_correct", pdfData.Box13CheckBox2));
                parameters.Add(utility.BuildReportParameter("Box_13_3_correct", pdfData.Box13CheckBox3));
                parameters.Add(utility.BuildReportParameter("Box_d", pdfData.EmployeeSsn));
                parameters.Add(utility.BuildReportParameter("Box_b", pdfData.EmployerEin));
                parameters.Add(utility.BuildReportParameter("Box_14_line_1", pdfData.Box14Line1));
                parameters.Add(utility.BuildReportParameter("Box_14_line_2", pdfData.Box14Line2));
                parameters.Add(utility.BuildReportParameter("Box_14_line_3", pdfData.Box14Line3));
                parameters.Add(utility.BuildReportParameter("Box_14_line_4", pdfData.Box14Line4));
                parameters.Add(utility.BuildReportParameter("Box_15_1_1", pdfData.Box15Line1Section1));
                parameters.Add(utility.BuildReportParameter("Box_15_1_2", pdfData.Box15Line1Section2));
                parameters.Add(utility.BuildReportParameter("Box_15_2_1", pdfData.Box15Line2Section1));
                parameters.Add(utility.BuildReportParameter("Box_15_2_2", pdfData.Box15Line2Section2));
                parameters.Add(utility.BuildReportParameter("Box_16_1", pdfData.Box16Line1));
                parameters.Add(utility.BuildReportParameter("Box_16_2", pdfData.Box16Line2));
                parameters.Add(utility.BuildReportParameter("Box_17_1", pdfData.Box17Line1));
                parameters.Add(utility.BuildReportParameter("Box_17_2", pdfData.Box17Line2));
                parameters.Add(utility.BuildReportParameter("Box_18_1", pdfData.Box18Line1));
                parameters.Add(utility.BuildReportParameter("Box_18_2", pdfData.Box18Line2));
                parameters.Add(utility.BuildReportParameter("Box_19_1", pdfData.Box19Line1));
                parameters.Add(utility.BuildReportParameter("Box_19_2", pdfData.Box19Line2));
                parameters.Add(utility.BuildReportParameter("Box_20_1", pdfData.Box20Line1));
                parameters.Add(utility.BuildReportParameter("Box_20_2", pdfData.Box20Line2));
                // Set previous parameters
                parameters.Add(utility.BuildReportParameter("Box_f", pdfData.EmployeeSsnPrev));
                parameters.Add(utility.BuildReportParameter("Box_g", pdfData.EmployeeNamePrev));
                parameters.Add(utility.BuildReportParameter("Box_1_previous", pdfData.FederalWagesPrev));
                parameters.Add(utility.BuildReportParameter("Box_2_previous", pdfData.FederalWithholdingPrev));
                parameters.Add(utility.BuildReportParameter("Box_3_previous", pdfData.SocialSecurityWagesPrev));
                parameters.Add(utility.BuildReportParameter("Box_4_previous", pdfData.SocialSecurityWithholdingPrev));
                parameters.Add(utility.BuildReportParameter("Box_5_previous", pdfData.MedicareWagesPrev));
                parameters.Add(utility.BuildReportParameter("Box_6_previous", pdfData.MedicareWithholdingPrev));
                parameters.Add(utility.BuildReportParameter("Box_7_previous", pdfData.SocialSecurityTipsPrev));
                parameters.Add(utility.BuildReportParameter("Box_8_previous", pdfData.AllocatedTipsPrev));
                parameters.Add(utility.BuildReportParameter("Box_10_previous", pdfData.DependentCarePrev));
                parameters.Add(utility.BuildReportParameter("Box_11_previous", pdfData.NonqualifiedTotalPrev));
                parameters.Add(utility.BuildReportParameter("Box_12a_Code_previous", pdfData.Box12aCodePrev));
                parameters.Add(utility.BuildReportParameter("Box_12a_Amt_previous", pdfData.Box12aAmountPrev));
                parameters.Add(utility.BuildReportParameter("Box_12b_Code_previous", pdfData.Box12bCodePrev));
                parameters.Add(utility.BuildReportParameter("Box_12b_Amt_previous", pdfData.Box12bAmountPrev));
                parameters.Add(utility.BuildReportParameter("Box_12c_Code_previous", pdfData.Box12cCodePrev));
                parameters.Add(utility.BuildReportParameter("Box_12c_Amt_previous", pdfData.Box12cAmountPrev));
                parameters.Add(utility.BuildReportParameter("Box_12d_Code_previous", pdfData.Box12dCodePrev));
                parameters.Add(utility.BuildReportParameter("Box_12d_Amt_previous", pdfData.Box12dAmountPrev));
                parameters.Add(utility.BuildReportParameter("Box_13_1_previous", pdfData.Box13CheckBox1Prev));
                parameters.Add(utility.BuildReportParameter("Box_13_2_previous", pdfData.Box13CheckBox2Prev));
                parameters.Add(utility.BuildReportParameter("Box_13_3_previous", pdfData.Box13CheckBox3Prev));
                parameters.Add(utility.BuildReportParameter("Box_14_line_1_previous", pdfData.Box14Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_14_line_2_previous", pdfData.Box14Line2Prev));
                parameters.Add(utility.BuildReportParameter("Box_14_line_3_previous", pdfData.Box14Line3Prev));
                parameters.Add(utility.BuildReportParameter("Box_14_line_4_previous", pdfData.Box14Line4Prev));
                parameters.Add(utility.BuildReportParameter("Box_15_1_1_previous", pdfData.Box15Line1Section1Prev));
                parameters.Add(utility.BuildReportParameter("Box_15_1_2_previous", pdfData.Box15Line1Section2Prev));
                parameters.Add(utility.BuildReportParameter("Box_15_2_1_previous", pdfData.Box15Line2Section1Prev));
                parameters.Add(utility.BuildReportParameter("Box_15_2_2_previous", pdfData.Box15Line2Section2Prev));
                parameters.Add(utility.BuildReportParameter("Box_16_1_previous", pdfData.Box16Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_16_2_previous", pdfData.Box16Line2Prev));
                parameters.Add(utility.BuildReportParameter("Box_17_1_previous", pdfData.Box17Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_17_2_previous", pdfData.Box17Line2Prev));
                parameters.Add(utility.BuildReportParameter("Box_18_1_previous", pdfData.Box18Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_18_2_previous", pdfData.Box18Line2Prev));
                parameters.Add(utility.BuildReportParameter("Box_19_1_previous", pdfData.Box19Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_19_2_previous", pdfData.Box19Line2Prev));
                parameters.Add(utility.BuildReportParameter("Box_20_1_previous", pdfData.Box20Line1Prev));
                parameters.Add(utility.BuildReportParameter("Box_20_2_previous", pdfData.Box20Line2Prev));

                // Set the report parameters
                report.SetParameters(parameters);

                // Render the report as a byte array
                renderedBytes = report.Render(
                    ReportType,
                    DeviceInfo);
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
        /// Populates the W-2 PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">W-2 PDF data</param>
        /// <param name="pathToReport">Path indicating the path to the report on the server</param>
        /// <returns>Byte array containing PDF data for the W-2 tax form</returns>
        public byte[] PopulateW2PdfReport(FormW2PdfData pdfData, string pathToReport)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }

            byte[] renderedBytes;
            var report = new LocalReport();

            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();
                parameters.Add(utility.BuildReportParameter("Tax_Year", pdfData.TaxYear));
                parameters.Add(utility.BuildReportParameter("Box_c_name", pdfData.EmployerName));
                parameters.Add(utility.BuildReportParameter("Box_c_addr_1", pdfData.EmployerAddressLine1));
                parameters.Add(utility.BuildReportParameter("Box_c_addr_2", pdfData.EmployerAddressLine2));
                parameters.Add(utility.BuildReportParameter("Box_c_addr_3", pdfData.EmployerAddressLine3));
                parameters.Add(utility.BuildReportParameter("Box_c_addr_4", pdfData.EmployerAddressLine4));
                parameters.Add(utility.BuildReportParameter("Box_e", pdfData.EmployeeName()));
                parameters.Add(utility.BuildReportParameter("Box_f_addr_1", pdfData.EmployeeAddressLine1));
                parameters.Add(utility.BuildReportParameter("Box_f_addr_2", pdfData.EmployeeAddressLine2));
                parameters.Add(utility.BuildReportParameter("Box_f_addr_3", pdfData.EmployeeAddressLine3));
                parameters.Add(utility.BuildReportParameter("Box_f_addr_4", pdfData.EmployeeAddressLine4));
                parameters.Add(utility.BuildReportParameter("Box_1", pdfData.FederalWages));
                parameters.Add(utility.BuildReportParameter("Box_2", pdfData.FederalWithholding));
                parameters.Add(utility.BuildReportParameter("Box_3", pdfData.SocialSecurityWages));
                parameters.Add(utility.BuildReportParameter("Box_4", pdfData.SocialSecurityWithholding));
                parameters.Add(utility.BuildReportParameter("Box_5", pdfData.MedicareWages));
                parameters.Add(utility.BuildReportParameter("Box_6", pdfData.MedicareWithholding));
                parameters.Add(utility.BuildReportParameter("Box_7", pdfData.SocialSecurityTips));
                parameters.Add(utility.BuildReportParameter("Box_8", pdfData.AllocatedTips));
                parameters.Add(utility.BuildReportParameter("Box_9", pdfData.AdvancedEic));
                parameters.Add(utility.BuildReportParameter("Box_10", pdfData.DependentCare));
                parameters.Add(utility.BuildReportParameter("Box_11", pdfData.NonqualifiedTotal));
                parameters.Add(utility.BuildReportParameter("Box_12a_Code", pdfData.Box12aCode));
                parameters.Add(utility.BuildReportParameter("Box_12a_Amt", pdfData.Box12aAmount));
                parameters.Add(utility.BuildReportParameter("Box_12b_Code", pdfData.Box12bCode));
                parameters.Add(utility.BuildReportParameter("Box_12b_Amt", pdfData.Box12bAmount));
                parameters.Add(utility.BuildReportParameter("Box_12c_Code", pdfData.Box12cCode));
                parameters.Add(utility.BuildReportParameter("Box_12c_Amt", pdfData.Box12cAmount));
                parameters.Add(utility.BuildReportParameter("Box_12d_Code", pdfData.Box12dCode));
                parameters.Add(utility.BuildReportParameter("Box_12d_Amt", pdfData.Box12dAmount));
                parameters.Add(utility.BuildReportParameter("Box_13_1", pdfData.Box13CheckBox1));
                parameters.Add(utility.BuildReportParameter("Box_13_2", pdfData.Box13CheckBox2));
                parameters.Add(utility.BuildReportParameter("Box_13_3", pdfData.Box13CheckBox3));
                parameters.Add(utility.BuildReportParameter("Box_a", pdfData.EmployeeSsn));
                parameters.Add(utility.BuildReportParameter("Box_b", pdfData.EmployerEin));
                parameters.Add(utility.BuildReportParameter("Box_14_line_1", pdfData.Box14Line1));
                parameters.Add(utility.BuildReportParameter("Box_14_line_2", pdfData.Box14Line2));
                parameters.Add(utility.BuildReportParameter("Box_14_line_3", pdfData.Box14Line3));
                parameters.Add(utility.BuildReportParameter("Box_14_line_4", pdfData.Box14Line4));
                parameters.Add(utility.BuildReportParameter("Box_15_1_1", pdfData.Box15Line1Section1));
                parameters.Add(utility.BuildReportParameter("Box_15_1_2", pdfData.Box15Line1Section2));
                parameters.Add(utility.BuildReportParameter("Box_15_2_1", pdfData.Box15Line2Section1));
                parameters.Add(utility.BuildReportParameter("Box_15_2_2", pdfData.Box15Line2Section2));
                parameters.Add(utility.BuildReportParameter("Box_16_1", pdfData.Box16Line1));
                parameters.Add(utility.BuildReportParameter("Box_16_2", pdfData.Box16Line2));
                parameters.Add(utility.BuildReportParameter("Box_17_1", pdfData.Box17Line1));
                parameters.Add(utility.BuildReportParameter("Box_17_2", pdfData.Box17Line2));
                parameters.Add(utility.BuildReportParameter("Box_18_1", pdfData.Box18Line1));
                parameters.Add(utility.BuildReportParameter("Box_18_2", pdfData.Box18Line2));
                parameters.Add(utility.BuildReportParameter("Box_19_1", pdfData.Box19Line1));
                parameters.Add(utility.BuildReportParameter("Box_19_2", pdfData.Box19Line2));
                parameters.Add(utility.BuildReportParameter("Box_20_1", pdfData.Box20Line1));
                parameters.Add(utility.BuildReportParameter("Box_20_2", pdfData.Box20Line2));

                // Set the report parameters
                report.SetParameters(parameters);

                // Render the report as a byte array
                renderedBytes = report.Render(
                    ReportType,
                    DeviceInfo);
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
        /// Returns the pdf data to print a 1095-C tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="recordId">The record ID where the 1095-C pdf data is stored</param>
        /// <returns>Byte array containing PDF data for the 1095-C tax form</returns>
        public async Task<Form1095cPdfData> Get1095cTaxFormDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessTo1095C(personId))
                throw new PermissionsException("Insufficient access to 1095-C data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.Get1095cPdfAsync(personId, recordId);

                // Validate that the domain entity employee ID is the same as the person ID requested.
                if (personId != taxFormPdfData.EmployeeId)
                {
                    throw new PermissionsException("Insufficient access to 1095-C data.");
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
        /// Populates the 1095-C PDF with the supplied data using an RDLC report.
        /// </summary>
        /// <param name="pdfData">1095-C PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the 1095-C tax form</returns>
        public byte[] Populate1095tReport(Form1095cPdfData pdfData, string pathToReport)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData", "Data to populate RDLC is required.");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport", "Path to RDLC report is required.");
            }

            byte[] reportBytes;
            var memoryStream = new MemoryStream();
            var report = new LocalReport();

            try
            {
                var empBox6 = string.Empty;
                if (!string.IsNullOrEmpty(pdfData.EmployeePostalCode))
                {
                    empBox6 = pdfData.EmployeePostalCode;
                }
                if (!string.IsNullOrEmpty(pdfData.EmployeeZipExtension))
                {
                    empBox6 = empBox6 + "-" + pdfData.EmployeeZipExtension;
                }
                if (!string.IsNullOrEmpty(pdfData.EmployeeCountry))
                {
                    if (!string.IsNullOrEmpty(empBox6))
                    {
                        empBox6 = empBox6 + " " + pdfData.EmployeeCountry;
                    }
                    else
                    {
                        empBox6 = pdfData.EmployeeCountry;
                    }
                }

                var isCorrected = pdfData.IsCorrected ? "X" : "";
                string isVoid = string.Empty;
                if (pdfData.IsVoided)
                {
                    isCorrected = string.Empty;
                    isVoid = "X";
                }

                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();
                var year = pdfData.TaxYear;
                parameters.Add(utility.BuildReportParameter("Year", pdfData.TaxYear));
                parameters.Add(utility.BuildReportParameter("Corrected", isCorrected));
                parameters.Add(utility.BuildReportParameter("Void", isVoid));
                parameters.Add(utility.BuildReportParameter("EmployeeName", pdfData.EmployeeName()));
                parameters.Add(utility.BuildReportParameter("EmployeeSsn", pdfData.EmployeeSsn));
                parameters.Add(utility.BuildReportParameter("EmployeeAddress", pdfData.EmployeeAddressLine1 + " " + pdfData.EmployeeAddressLine2));
                parameters.Add(utility.BuildReportParameter("EmployeeCity", pdfData.EmployeeCityName));
                parameters.Add(utility.BuildReportParameter("EmployeeState", pdfData.EmployeeStateCode));
                parameters.Add(utility.BuildReportParameter("EmployeeZipCountry", empBox6));

                parameters.Add(utility.BuildReportParameter("EmployerName", pdfData.EmployerName));
                parameters.Add(utility.BuildReportParameter("EmployerEin", pdfData.EmployerEin));
                parameters.Add(utility.BuildReportParameter("EmployerAddress", pdfData.EmployerAddressLine));
                parameters.Add(utility.BuildReportParameter("EmployerPhone", pdfData.EmployerContactPhoneNumber));
                parameters.Add(utility.BuildReportParameter("EmployerCity", pdfData.EmployerCityName));
                parameters.Add(utility.BuildReportParameter("EmployerState", pdfData.EmployerStateCode));
                parameters.Add(utility.BuildReportParameter("EmployerZip", pdfData.EmployerZipCode));

                parameters.Add(utility.BuildReportParameter("OfferOfCoverage12Month", pdfData.OfferOfCoverage12Month));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageJanuary", pdfData.OfferOfCoverageJanuary));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageFebruary", pdfData.OfferOfCoverageFebruary));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageMarch", pdfData.OfferOfCoverageMarch));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageApril", pdfData.OfferOfCoverageApril));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageMay", pdfData.OfferOfCoverageMay));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageJune", pdfData.OfferOfCoverageJune));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageJuly", pdfData.OfferOfCoverageJuly));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageAugust", pdfData.OfferOfCoverageAugust));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageSeptember", pdfData.OfferOfCoverageSeptember));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageOctober", pdfData.OfferOfCoverageOctober));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageNovember", pdfData.OfferOfCoverageNovember));
                parameters.Add(utility.BuildReportParameter("OfferOfCoverageDecember", pdfData.OfferOfCoverageDecember));

                // Include the '$' for 2015 because the template for that year does not include the '$'.
                if (pdfData.TaxYear == "2015")
                {
                    parameters.Add(utility.BuildReportParameter("LowestCostAmount12Month", ConvertDecimalToUsDollars(pdfData.LowestCostAmount12Month)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJanuary", ConvertDecimalToUsDollars(pdfData.LowestCostAmountJanuary)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountFebruary", ConvertDecimalToUsDollars(pdfData.LowestCostAmountFebruary)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountMarch", ConvertDecimalToUsDollars(pdfData.LowestCostAmountMarch)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountApril", ConvertDecimalToUsDollars(pdfData.LowestCostAmountApril)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountMay", ConvertDecimalToUsDollars(pdfData.LowestCostAmountMay)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJune", ConvertDecimalToUsDollars(pdfData.LowestCostAmountJune)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJuly", ConvertDecimalToUsDollars(pdfData.LowestCostAmountJuly)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountAugust", ConvertDecimalToUsDollars(pdfData.LowestCostAmountAugust)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountSeptember", ConvertDecimalToUsDollars(pdfData.LowestCostAmountSeptember)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountOctober", ConvertDecimalToUsDollars(pdfData.LowestCostAmountOctober)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountNovember", ConvertDecimalToUsDollars(pdfData.LowestCostAmountNovember)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountDecember", ConvertDecimalToUsDollars(pdfData.LowestCostAmountDecember)));
                }
                else
                {
                    parameters.Add(utility.BuildReportParameter("LowestCostAmount12Month", ConvertDecimalToUsString(pdfData.LowestCostAmount12Month)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJanuary", ConvertDecimalToUsString(pdfData.LowestCostAmountJanuary)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountFebruary", ConvertDecimalToUsString(pdfData.LowestCostAmountFebruary)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountMarch", ConvertDecimalToUsString(pdfData.LowestCostAmountMarch)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountApril", ConvertDecimalToUsString(pdfData.LowestCostAmountApril)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountMay", ConvertDecimalToUsString(pdfData.LowestCostAmountMay)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJune", ConvertDecimalToUsString(pdfData.LowestCostAmountJune)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountJuly", ConvertDecimalToUsString(pdfData.LowestCostAmountJuly)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountAugust", ConvertDecimalToUsString(pdfData.LowestCostAmountAugust)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountSeptember", ConvertDecimalToUsString(pdfData.LowestCostAmountSeptember)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountOctober", ConvertDecimalToUsString(pdfData.LowestCostAmountOctober)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountNovember", ConvertDecimalToUsString(pdfData.LowestCostAmountNovember)));
                    parameters.Add(utility.BuildReportParameter("LowestCostAmountDecember", ConvertDecimalToUsString(pdfData.LowestCostAmountDecember)));
                }


                parameters.Add(utility.BuildReportParameter("SafeHarborCode12Month", pdfData.SafeHarborCode12Month));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeJanuary", pdfData.SafeHarborCodeJanuary));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeFebruary", pdfData.SafeHarborCodeFebruary));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeMarch", pdfData.SafeHarborCodeMarch));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeApril", pdfData.SafeHarborCodeApril));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeMay", pdfData.SafeHarborCodeMay));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeJune", pdfData.SafeHarborCodeJune));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeJuly", pdfData.SafeHarborCodeJuly));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeAugust", pdfData.SafeHarborCodeAugust));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeSeptember", pdfData.SafeHarborCodeSeptember));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeOctober", pdfData.SafeHarborCodeOctober));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeNovember", pdfData.SafeHarborCodeNovember));
                parameters.Add(utility.BuildReportParameter("SafeHarborCodeDecember", pdfData.SafeHarborCodeDecember));

                parameters.Add(utility.BuildReportParameter("EmployeeIsSelfInsured", pdfData.EmployeeIsSelfInsured ? "X" : ""));
                parameters.Add(utility.BuildReportParameter("PlanStartMonthCode", pdfData.PlanStartMonthCode));

                int counter = 17;
                foreach (var dependant in pdfData.CoveredIndividuals)
                {
                    TaxFormPdfUtility.Populate1095CDependentRow(ref parameters, dependant, counter);
                    counter += 1;
                }

                if (counter < 34)
                {
                    Form1095cCoveredIndividualsPdfData emptyDependant = new Form1095cCoveredIndividualsPdfData()
                    {
                        CoveredIndividualFirstName = string.Empty,
                        CoveredIndividualMiddleName = string.Empty,
                        CoveredIndividualLastName = string.Empty,
                        CoveredIndividualSsn = string.Empty,
                        CoveredIndividualDateOfBirth = null,
                        Covered12Month = false,
                        CoveredApril = false,
                        CoveredAugust = false,
                        CoveredDecember = false,
                        CoveredFebruary = false,
                        CoveredJanuary = false,
                        CoveredJuly = false,
                        CoveredJune = false,
                        CoveredMarch = false,
                        CoveredMay = false,
                        CoveredNovember = false,
                        CoveredOctober = false,
                        CoveredSeptember = false,
                        IsEmployeeItself = false
                    };

                    for (int i = counter; i < 35; i++)
                    {
                        TaxFormPdfUtility.Populate1095CDependentRow(ref parameters, emptyDependant, i);
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
                reportBytes = report.Render(
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
                // rethrow exception
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
            return reportBytes;
        }

        /// <summary>
        /// Returns the pdf data to print a T4 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4.</param>
        /// <param name="recordId">The record ID where the T-4 pdf data is stored</param>
        /// <returns>FormT4PdfData domain entity</returns>
        public async Task<FormT4PdfData> GetT4TaxFormDataAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessToT4(personId))
                throw new PermissionsException("Insufficient access to T4 data.");

            try
            {
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetT4PdfAsync(personId, recordId);

                // Validate that the domain entity employee ID is the same as the person ID requested.
                if (personId != taxFormPdfData.EmployeeId)
                {
                    throw new PermissionsException("Insufficient access to T4 data.");
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
        /// Populates the T4 PDF with the supplied data using RDLC instead of PDF Sharp.
        /// </summary>
        /// <param name="pdfData">T4 PDF data</param>
        /// <param name="pathToReport">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the T4 tax form</returns>
        public byte[] PopulateT4PdfReport(FormT4PdfData pdfData, string pathToReport)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException("pdfData", "Data to populate RDLC is required.");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport", "Path to RDLC report is required.");
            }

            byte[] reportBytes;
            var memoryStream = new MemoryStream();
            var report = new LocalReport();

            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = new List<ReportParameter>();

                // Box Data
                parameters.Add(utility.BuildReportParameter("SocialInsuranceNumber", pdfData.SocialInsuranceNumber));
                parameters.Add(utility.BuildReportParameter("EmploymentIncome", pdfData.EmploymentIncome));
                parameters.Add(utility.BuildReportParameter("EmployeesCPPContributions", pdfData.EmployeesCPPContributions));
                parameters.Add(utility.BuildReportParameter("EmployeesQPPContributions", pdfData.EmployeesQPPContributions));
                parameters.Add(utility.BuildReportParameter("EmployeesEIPremiums", pdfData.EmployeesEIPremiums));
                parameters.Add(utility.BuildReportParameter("RPPContributions", pdfData.RPPContributions));
                parameters.Add(utility.BuildReportParameter("IncomeTaxDeducted", pdfData.IncomeTaxDeducted));
                parameters.Add(utility.BuildReportParameter("EIInsurableEarnings", pdfData.EIInsurableEarnings));
                parameters.Add(utility.BuildReportParameter("CPPQPPPensionableEarnings", pdfData.CPPQPPPensionableEarnings));
                parameters.Add(utility.BuildReportParameter("UnionDues", pdfData.UnionDues));
                parameters.Add(utility.BuildReportParameter("CharitableDonations", pdfData.CharitableDonations));
                parameters.Add(utility.BuildReportParameter("RPPorDPSPRegistrationNumber", pdfData.RPPorDPSPRegistrationNumber));
                parameters.Add(utility.BuildReportParameter("PensionAdjustment", pdfData.PensionAdjustment));
                parameters.Add(utility.BuildReportParameter("EmployeesPPIPPremiums", pdfData.EmployeesPPIPPremiums));
                parameters.Add(utility.BuildReportParameter("PPIPInsurableEarnings", pdfData.PPIPInsurableEarnings));

                // Other boxes
                for (var i = 0; i < 6; i++)
                {
                    var box = i + 1;
                    var boxData = pdfData.OtherBoxes.ElementAtOrDefault(i);
                    if (boxData != null)
                    {
                        parameters.Add(utility.BuildReportParameter("OtherBox" + box + "Code", boxData.Code));
                        parameters.Add(utility.BuildReportParameter("OtherBox" + box + "Amount", boxData.Amount));
                    }
                    else
                    {
                        parameters.Add(utility.BuildReportParameter("OtherBox" + box + "Code", string.Empty));
                        parameters.Add(utility.BuildReportParameter("OtherBox" + box + "Amount", string.Empty));
                    }

                }

                // Employee Info
                parameters.Add(utility.BuildReportParameter("EmployeeFirstName", pdfData.EmployeeFirstName));
                parameters.Add(utility.BuildReportParameter("EmployeeMiddleName", pdfData.EmployeeMiddleName));
                parameters.Add(utility.BuildReportParameter("EmployeeLastName", pdfData.EmployeeLastName));
                parameters.Add(utility.BuildReportParameter("EmployeeAddressLine1", pdfData.EmployeeAddressLine1));
                parameters.Add(utility.BuildReportParameter("EmployeeAddressLine2", pdfData.EmployeeAddressLine2));
                parameters.Add(utility.BuildReportParameter("EmployeeAddressLine3", pdfData.EmployeeAddressLine3));
                parameters.Add(utility.BuildReportParameter("EmployeeAddressLine4", pdfData.EmployeeAddressLine4));
                parameters.Add(utility.BuildReportParameter("ProvinceOfEmployment", pdfData.ProvinceOfEmployment));
                parameters.Add(utility.BuildReportParameter("ExemptCPPQPP", pdfData.ExemptCPPQPP));
                parameters.Add(utility.BuildReportParameter("ExemptEI", pdfData.ExemptEI));
                parameters.Add(utility.BuildReportParameter("ExemptPPIP", pdfData.ExemptPPIP));
                parameters.Add(utility.BuildReportParameter("EmploymentCode", pdfData.EmploymentCode));

                // Employer Info
                parameters.Add(utility.BuildReportParameter("EmployerAddressLine1", pdfData.EmployerAddressLine1));
                parameters.Add(utility.BuildReportParameter("EmployerAddressLine2", pdfData.EmployerAddressLine2));
                parameters.Add(utility.BuildReportParameter("EmployerAddressLine3", pdfData.EmployerAddressLine3));
                parameters.Add(utility.BuildReportParameter("EmployerAddressLine4", pdfData.EmployerAddressLine4));
                parameters.Add(utility.BuildReportParameter("EmployerAddressLine5", pdfData.EmployerAddressLine5));

                parameters.Add(utility.BuildReportParameter("TaxYear", pdfData.TaxYear));

                // Set the report parameters
                report.SetParameters(parameters);

                // Render the report as a byte array
                reportBytes = report.Render(
                    ReportType,
                    DeviceInfo);
            }
            catch
            {
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }

            return reportBytes;
        }

        /// <summary>
        /// Convert the input decimal value into a string without the '$'.
        /// </summary>
        /// <param name="amount">Input amount</param>
        /// <returns>String representation of input value.</returns>
        private string ConvertDecimalToUsString(decimal? amount)
        {
            if (amount.HasValue)
            {
                var amt = amount.Value;
                return amt.ToString("C", CultureInfo.GetCultureInfo("en-US")).Replace("$", "");
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Convert the input decimal value into a string with the '$'.
        /// </summary>
        /// <param name="amount">Input amount</param>
        /// <returns>String representation of input value.</returns>
        private string ConvertDecimalToUsDollars(decimal? amount)
        {
            if (amount.HasValue)
            {
                var amt = amount.Value;
                return amt.ToString("C", CultureInfo.GetCultureInfo("en-US"));
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's W-2 tax form.
        /// A person can only view a W-2 form for:
        /// 1. Himself/herself with VIEW.W2 permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.W2 permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users' W-2 forms with VIEW.EMPLOYEE.W2 permission
        /// </summary>
        private bool HasAccessToW2(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(HumanResourcesPermissionCodes.ViewEmployeeW2);
            }
            else
            {
                return HasPermission(HumanResourcesPermissionCodes.ViewW2);
            }
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's 1095-C tax form.
        /// A person can only view a 1095-C form for:
        /// 1. Himself/herself with VIEW.1095C permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.1095C permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users' 1095-C forms with VIEW.EMPLOYEE.1095C permission
        /// </summary>
        private bool HasAccessTo1095C(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(HumanResourcesPermissionCodes.ViewEmployee1095C);
            }
            else
            {
                return HasPermission(HumanResourcesPermissionCodes.View1095C);
            }
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's T4 tax form
        /// A person can only view a T4 form for:
        /// 1. Himself/herself with VIEW.T4 permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.T4 permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users T4 form with VIEW.EMPLOYEE.T4 permission
        /// </summary>
        private bool HasAccessToT4(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(HumanResourcesPermissionCodes.ViewEmployeeT4);
            }
            else
            {
                return HasPermission(HumanResourcesPermissionCodes.ViewT4);
            }
        }
    }
}