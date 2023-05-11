// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Utility;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Service for tax form pdfs.
    /// </summary>
    [RegisterType]
    public class StudentTaxFormPdfService : BaseCoordinationService, IStudentTaxFormPdfService
    {
        public const string ReportType = "PDF";
        public const string DeviceInfo = "<DeviceInfo>" +
                                         " <OutputFormat>PDF</OutputFormat>" +
                                         "</DeviceInfo>";

        IStudentTaxFormPdfDataRepository taxFormPdfDataRepository;
        IPersonRepository personRepository;

        /// <summary>
        /// Constructor TaxFormPdfService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public StudentTaxFormPdfService(IStudentTaxFormPdfDataRepository taxFormPdfDataRepository,
            IPersonRepository personRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormPdfDataRepository = taxFormPdfDataRepository;
            this.personRepository = personRepository;
        }

        /// <summary>
        /// Returns the pdf data to print a 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <returns>TaxForm1098PdfData domain entity</returns>
        public async Task<Form1098PdfData> Get1098TaxFormData(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessTo1098(personId))
                throw new PermissionsException("Insufficient access to 1098 data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.Get1098PdfAsync(personId, recordId);

                // Validate that the domain entity student ID is the same as the person ID requested.
                if (personId != taxFormPdfData.StudentId)
                {
                    throw new PermissionsException("Insufficient access to 1098 data.");
                }

                var institutionAddressLines = await personRepository.Get1098HierarchyAddressAsync(taxFormPdfData.InstitutionId);

                // Assign the institution address lines.
                if (institutionAddressLines != null)
                {
                    // Combine address lines 1 and 2
                    taxFormPdfData.InstitutionAddressLine1 = institutionAddressLines.ElementAtOrDefault(0) ?? "";
                    if (!string.IsNullOrEmpty(institutionAddressLines.ElementAtOrDefault(1)))
                    {
                        taxFormPdfData.InstitutionAddressLine1 += " " + institutionAddressLines.ElementAtOrDefault(1);
                    }

                    // Combine address lines 3 and 4
                    taxFormPdfData.InstitutionAddressLine2 = institutionAddressLines.ElementAtOrDefault(2) ?? "";
                    if (!string.IsNullOrEmpty(institutionAddressLines.ElementAtOrDefault(3)))
                    {
                        taxFormPdfData.InstitutionAddressLine2 += " " + institutionAddressLines.ElementAtOrDefault(3);
                    }
                }

                // Put the phone number in address line 4 if it's empty.
                if (string.IsNullOrEmpty(taxFormPdfData.InstitutionAddressLine4))
                {
                    taxFormPdfData.InstitutionAddressLine4 = taxFormPdfData.InstitutionPhoneNumber;
                }

                return taxFormPdfData;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                // Log the error and throw the exception that was given
                logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Populates the 1098 PDF with the supplied data using an RDLC report.
        /// </summary>
        /// <param name="pdfData">1098 PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the 1098 tax form</returns>
        public byte[] Populate1098Report(Form1098PdfData pdfData, string pathToReport)
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
                //1098 Common Params
                parameters.Add(utility.BuildReportParameter("Year2", pdfData.TaxYear.Substring(2)));
                parameters.Add(utility.BuildReportParameter("CorrectedForm", pdfData.Correction ? "X" : ""));
                parameters.Add(utility.BuildReportParameter("InstNameAddr1", pdfData.InstitutionName));
                parameters.Add(utility.BuildReportParameter("InstNameAddr2", pdfData.InstitutionAddressLine1));
                parameters.Add(utility.BuildReportParameter("InstNameAddr3", pdfData.InstitutionAddressLine2));
                parameters.Add(utility.BuildReportParameter("InstNameAddr4", pdfData.InstitutionAddressLine3));
                parameters.Add(utility.BuildReportParameter("InstNameAddr5", pdfData.InstitutionAddressLine4));
                parameters.Add(utility.BuildReportParameter("InstEIN", pdfData.InstitutionEin));
                parameters.Add(utility.BuildReportParameter("StudentSSN", pdfData.SSN));
                parameters.Add(utility.BuildReportParameter("StudentName1", pdfData.StudentName));
                parameters.Add(utility.BuildReportParameter("StudentName2", pdfData.StudentName2));
                parameters.Add(utility.BuildReportParameter("StudentAddress", pdfData.StudentAddressLine1));
                parameters.Add(utility.BuildReportParameter("StudentCSZ", pdfData.StudentAddressLine2));
                parameters.Add(utility.BuildReportParameter("StudentID", pdfData.StudentId));
                //1098 T params
                if (pdfData.TaxFormName == "1098T")
                {
                    parameters.Add(utility.BuildReportParameter("AtLeastHalfTime", pdfData.AtLeastHalfTime ? "X" : ""));                            // Box 8
                    parameters.Add(utility.BuildReportParameter("Box1Amt", pdfData.AmountsPaidForTuitionAndExpenses ?? ""));
                    parameters.Add(utility.BuildReportParameter("Box2Amt", pdfData.AmountsBilledForTuitionAndExpenses ?? ""));
                    parameters.Add(utility.BuildReportParameter("Box4Amt", pdfData.AdjustmentsForPriorYear ?? ""));
                    parameters.Add(utility.BuildReportParameter("Box5Amt", pdfData.ScholarshipsOrGrants ?? ""));
                    parameters.Add(utility.BuildReportParameter("Box6Amt", pdfData.AdjustmentsToScholarshipsOrGrantsForPriorYear ?? ""));
                    parameters.Add(utility.BuildReportParameter("ChgRptgMethod", pdfData.ReportingMethodHasBeenChanged ? "X" : ""));                // Box 3
                    parameters.Add(utility.BuildReportParameter("GraduateStudent", pdfData.IsGradStudent ? "X" : ""));                              // Box 9
                    parameters.Add(utility.BuildReportParameter("First3Months", pdfData.AmountsBilledAndReceivedForQ1Period ? "X" : ""));           // Box 7
                }
                //1098 E params
                else
                {
                    parameters.Add(utility.BuildReportParameter("Box1Amt", pdfData.StudentInterestAmount ?? ""));
                    parameters.Add(utility.BuildReportParameter("IsIntrstExcld", pdfData.IsPriorInterestOrFeeExcluded ? "X" : ""));
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
        /// Returns the pdf data to print a T2202A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202A.</param>
        /// <param name="recordId">The record ID where the T2202A pdf data is stored</param>
        /// <returns>TaxFormT2202aPdfData domain entity</returns>
        public async Task<FormT2202aPdfData> GetT2202aTaxFormData(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID must be specified.");

            if (!HasAccessToT2202A(personId))
                throw new PermissionsException("Insufficient access to T2202A data.");

            try
            {
                // Call the repository to get all the data to print in the pdf.
                var taxFormPdfData = await this.taxFormPdfDataRepository.GetT2202aPdfAsync(personId, recordId);

                if (taxFormPdfData == null)
                {
                    throw new NullReferenceException("taxFormPdfData was null from repository.");
                }

                if (taxFormPdfData.Cancelled)
                {
                    throw new ApplicationException(string.Format("Attempt to access T2202 record {0} which is marked as cancelled.", recordId));
                }

                // Validate that the domain entity student ID is the same as the person ID requested.
                if (personId != taxFormPdfData.StudentId)
                {
                    throw new PermissionsException("Insufficient access to T2202A data.");
                }

                // Get the address using the 1098 hierarchy since that is also used for T2202As, and populate the 
                // two lines of address since the first line contains the institution name.
                var institutionAddressLines = await personRepository.Get1098HierarchyAddressAsync(taxFormPdfData.InstitutionId);
                if (institutionAddressLines != null)
                {
                    // Combine address lines 1 and 2
                    taxFormPdfData.InstitutionNameAddressLine2 = institutionAddressLines.ElementAtOrDefault(0) ?? "";
                    if (!string.IsNullOrEmpty(institutionAddressLines.ElementAtOrDefault(1)))
                    {
                        taxFormPdfData.InstitutionNameAddressLine2 += " " + institutionAddressLines.ElementAtOrDefault(1);
                    }

                    // Combine address lines 3 and 4
                    taxFormPdfData.InstitutionNameAddressLine3 = institutionAddressLines.ElementAtOrDefault(2) ?? "";
                    if (!string.IsNullOrEmpty(institutionAddressLines.ElementAtOrDefault(3)))
                    {
                        taxFormPdfData.InstitutionNameAddressLine3 += " " + institutionAddressLines.ElementAtOrDefault(3);
                    }
                }

                return taxFormPdfData;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                // Log the error and throw the exception that was given
                logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Populates the T2202A PDF with the supplied data using an RDLC report.
        /// </summary>
        /// <param name="pdfData">T2202A PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the T2202A tax form</returns>
        public byte[] PopulateT2202aReport(FormT2202aPdfData pdfData, string pathToReport)
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

                int taxYearInt;
                bool taxYearParseSuccess = int.TryParse(pdfData.TaxYear, out taxYearInt);
                
                if (taxYearParseSuccess && taxYearInt >= 2019)
                {
                    parameters.Add(utility.BuildReportParameter("TaxYear", pdfData.TaxYear));
                    parameters.Add(utility.BuildReportParameter("FlyingClub", pdfData.FlyingClub));
                    parameters.Add(utility.BuildReportParameter("SchoolType", pdfData.SchoolType));
                    parameters.Add(utility.BuildReportParameter("SocialInsuranceNumber", pdfData.SocialInsuranceNumber));
                }
                else
                {
                    parameters.Add(utility.BuildReportParameter("TaxYear", pdfData.TaxYear.Substring(2)));
                }

                parameters.Add(utility.BuildReportParameter("ProgramName", pdfData.ProgramName));
                parameters.Add(utility.BuildReportParameter("StudentIDNumber", pdfData.StudentId));

                parameters.Add(utility.BuildReportParameter("StuNameAddr1", pdfData.StudentNameAddressLine1));
                parameters.Add(utility.BuildReportParameter("StuNameAddr2", pdfData.StudentNameAddressLine2));
                parameters.Add(utility.BuildReportParameter("StuNameAddr3", pdfData.StudentNameAddressLine3));
                parameters.Add(utility.BuildReportParameter("StuNameAddr4", pdfData.StudentNameAddressLine4));
                parameters.Add(utility.BuildReportParameter("StuNameAddr5", pdfData.StudentNameAddressLine5));
                parameters.Add(utility.BuildReportParameter("StuNameAddr6", pdfData.StudentNameAddressLine6));

                // Populate the 4 "dynamic" session periods, boxes A (eligible tuition fees), B (number of part time months), and C (number of full time months).
                for (int i = 0; i < 4; i++)
                {
                    int sessionPeriodNumber = i + 1;
                    var sessionPeriod = pdfData.SessionPeriods.ElementAtOrDefault(i);
                    if (sessionPeriod != null)
                    {
                        if (taxYearParseSuccess && taxYearInt >= 2019)
                        {
                            parameters.Add(utility.BuildReportParameter("FromYear" + sessionPeriodNumber, sessionPeriod.StudentFromYear.Substring(2)));
                            parameters.Add(utility.BuildReportParameter("FromMonth" + sessionPeriodNumber, sessionPeriod.StudentFromMonth.Length > 1 ? sessionPeriod.StudentFromMonth : "0" + sessionPeriod.StudentFromMonth));
                            parameters.Add(utility.BuildReportParameter("ToYear" + sessionPeriodNumber, sessionPeriod.StudentToYear.Substring(2)));
                            parameters.Add(utility.BuildReportParameter("ToMonth" + sessionPeriodNumber, sessionPeriod.StudentToMonth.Length > 1 ? sessionPeriod.StudentToMonth : "0" + sessionPeriod.StudentToMonth));
                        }
                        else
                        {
                            parameters.Add(utility.BuildReportParameter("FromYear" + sessionPeriodNumber, sessionPeriod.StudentFromYear));
                            parameters.Add(utility.BuildReportParameter("FromMonth" + sessionPeriodNumber, sessionPeriod.StudentFromMonth));
                            parameters.Add(utility.BuildReportParameter("ToYear" + sessionPeriodNumber, sessionPeriod.StudentToYear));
                            parameters.Add(utility.BuildReportParameter("ToMonth" + sessionPeriodNumber, sessionPeriod.StudentToMonth));
                        }

                        parameters.Add(utility.BuildReportParameter("BoxAAmt" + sessionPeriodNumber, sessionPeriod.BoxAAmountString));
                        parameters.Add(utility.BuildReportParameter("BoxBAmt" + sessionPeriodNumber, sessionPeriod.BoxBHours));
                        parameters.Add(utility.BuildReportParameter("BoxCAmt" + sessionPeriodNumber, sessionPeriod.BoxCHours));
                    }
                    else
                    {
                        parameters.Add(utility.BuildReportParameter("FromYear" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("FromMonth" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("ToYear" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("ToMonth" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("BoxAAmt" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("BoxBAmt" + sessionPeriodNumber, string.Empty));
                        parameters.Add(utility.BuildReportParameter("BoxCAmt" + sessionPeriodNumber, string.Empty));
                    }
                }

                parameters.Add(utility.BuildReportParameter("BoxATotal", pdfData.StudentBoxATotal));
                parameters.Add(utility.BuildReportParameter("BoxBTotal", pdfData.StudentBoxBTotal));
                parameters.Add(utility.BuildReportParameter("BoxCTotal", pdfData.StudentBoxCTotal));

                parameters.Add(utility.BuildReportParameter("InstNameAddr1", pdfData.InstitutionNameAddressLine1));
                parameters.Add(utility.BuildReportParameter("InstNameAddr2", pdfData.InstitutionNameAddressLine2));
                parameters.Add(utility.BuildReportParameter("InstNameAddr3", pdfData.InstitutionNameAddressLine3));

                // Set the report parameters
                report.SetParameters(parameters);

                // Render the report as a byte array
                renderedBytes = report.Render(
                    ReportType,
                    DeviceInfo);
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
            return renderedBytes;
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's 1098 tax form
        /// A person can only view a 1098 form for:
        /// 1. Himself/herself with VIEW.1098 permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.1098 permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users 1098 form with VIEW.STUDENT.1098 permission
        /// </summary>
        private bool HasAccessTo1098(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(StudentPermissionCodes.ViewStudent1098);
            }
            else
            {
                return HasPermission(StudentPermissionCodes.View1098);
            }
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's T2202A tax form
        /// A person can only view a T2202A form for:
        /// 1. Himself/herself with VIEW.T2202A permission
        /// 2. Someone who currently has permission to proxy for the user with VIEW.T2202A permission
        /// 3. Someone (Tax form admin) who currently has permission to view all users T2202A form with VIEW.STUDENT.T2202A permission
        /// </summary>
        private bool HasAccessToT2202A(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                return HasPermission(StudentPermissionCodes.ViewStudentT2202A);
            }
            else
            {
                return HasPermission(StudentPermissionCodes.ViewT2202A);
            }
        }
    }
}
