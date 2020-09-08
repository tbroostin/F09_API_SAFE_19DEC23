// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository for the data to be printed in a pdf for any tax form
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class HumanResourcesTaxFormPdfDataRepository : BaseColleagueRepository, IHumanResourcesTaxFormPdfDataRepository
    {
        /// <summary>
        /// Tax Form PDF data repository constructor.
        /// </summary>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public HumanResourcesTaxFormPdfDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // nothing to do
        }

        /// <summary>
        /// Gets the boolean value that indicates if the client is set up to use the Guam version of the W2 form.
        /// </summary>
        /// <returns>Boolean value where true = Guam and false = USA</returns>
        public async Task<bool> GetW2GuamFlag()
        {
            var qtdYtdParameter = await DataReader.ReadRecordAsync<QtdYtdParameterW2Pdf>("HR.PARMS", "QTD.YTD.PARAMETER");
            if (qtdYtdParameter != null && !string.IsNullOrWhiteSpace(qtdYtdParameter.QypW2UseGuamTemplate))
            {
                return qtdYtdParameter.QypW2UseGuamTemplate.ToUpper() == "Y";
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the pdf data for tax form W-2
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a W-2 tax form</param>
        /// <returns>The pdf data for tax form W-2</returns>
        public async Task<FormW2PdfData> GetW2PdfAsync(string personId, string recordId)
        {
            // Throw an exception if there is no record id to get the W-2 tax form data
            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "The record ID is required.");

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "The person ID is required.");

            // Read the record where the W-2 tax form data is stored
            string criteria = "WITH WEB.W2.ONLINE.ID EQ '" + recordId + "'" + " AND WITH WW2O.EMPLOYEE.ID EQ '" + personId + "'";
            var w2Ids = await DataReader.SelectAsync("WEB.W2.ONLINE", criteria);

            if (w2Ids == null)
                throw new ApplicationException("One WEB.W2.ONLINE ID expected but null returned for record ID: " + recordId);

            if (w2Ids.Count() == 0)
                throw new ApplicationException("One WEB.W2.ONLINE ID expected but zero returned for record ID: " + recordId);

            if (w2Ids.Count() > 1)
                throw new ApplicationException("One WEB.W2.ONLINE ID expected but more than one returned for record ID: " + recordId);

            var dataContractW2 = await DataReader.ReadRecordAsync<WebW2Online>(w2Ids.FirstOrDefault());

            // Validate that we found the record and that it contains required fields for the constructor
            if (dataContractW2 == null)
                throw new ApplicationException("WebW2Online record " + recordId + " does not exist.");

            if (string.IsNullOrEmpty(dataContractW2.Ww2oYear))
                throw new ApplicationException("WebW2Online record " + dataContractW2.Recordkey + "must have a tax year.");

            if (string.IsNullOrEmpty(dataContractW2.Ww2oEmplyrId))
                throw new ApplicationException("WebW2Online record " + dataContractW2.Recordkey + "must have an employer tax ID (EIN).");

            // Initialize the SSN
            string ssn = "";
            if (dataContractW2 != null && !string.IsNullOrEmpty(dataContractW2.Ww2oSsn))
            {
                ssn = dataContractW2.Ww2oSsn;
            }

            var hrWebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.HumanResources.DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
            if (hrWebDefaults != null)
            {
                // Mask the SSN if necessary.
                if (!string.IsNullOrEmpty(hrWebDefaults.HrwebW2oMaskSsn) && hrWebDefaults.HrwebW2oMaskSsn.ToUpper() == "Y")
                {
                    if (!string.IsNullOrEmpty(ssn))
                    {
                        // Mask SSN
                        if (ssn.Length >= 4)
                        {
                            ssn = "XXX-XX-" + ssn.Substring(ssn.Length - 4);
                        }
                        else
                        {
                            ssn = "XXX-XX-" + ssn;
                        }
                    }
                }
            }

            // Instantiate a W-2 domain entity
            var domainEntityW2 = new FormW2PdfData(dataContractW2.Ww2oYear, dataContractW2.Ww2oEmplyrId, ssn);

            // Get the employer information
            domainEntityW2.EmployerName = dataContractW2.Ww2oEmplyrName;
            domainEntityW2.EmployerAddressLine1 = dataContractW2.Ww2oEmplyrAddrLine1;
            domainEntityW2.EmployerAddressLine2 = dataContractW2.Ww2oEmplyrAddrLine2;
            domainEntityW2.EmployerAddressLine3 = dataContractW2.Ww2oEmplyrAddrLine3;
            domainEntityW2.EmployerAddressLine4 = dataContractW2.Ww2oEmplyrAddrLine4;

            // Get the employee information
            domainEntityW2.EmployeeId = dataContractW2.Ww2oEmployeeId;
            domainEntityW2.EmployeeFirstName = dataContractW2.Ww2oFirstName;
            domainEntityW2.EmployeeLastName = dataContractW2.Ww2oLastName;
            domainEntityW2.EmployeeMiddleName = dataContractW2.Ww2oMiddleName;
            domainEntityW2.EmployeeSuffix = dataContractW2.Ww2oSuffix;
            domainEntityW2.EmployeeAddressLine1 = dataContractW2.Ww2oEmplyeAddrLine1;
            domainEntityW2.EmployeeAddressLine2 = dataContractW2.Ww2oEmplyeAddrLine2;
            domainEntityW2.EmployeeAddressLine3 = dataContractW2.Ww2oEmplyeAddrLine3;
            domainEntityW2.EmployeeAddressLine4 = dataContractW2.Ww2oEmplyeAddrLine4;

            domainEntityW2.FederalWages = W2AmountStringToDecimal(dataContractW2.Ww2oFederalWages, recordId, dataContractW2);
            domainEntityW2.FederalWithholding = W2AmountStringToDecimal(dataContractW2.Ww2oFederalWithholding, recordId, dataContractW2);
            domainEntityW2.SocialSecurityWages = W2AmountStringToDecimal(dataContractW2.Ww2oSocSecWages, recordId, dataContractW2);
            domainEntityW2.SocialSecurityWithholding = W2AmountStringToDecimal(dataContractW2.Ww2oSocSecWithholding, recordId, dataContractW2);
            domainEntityW2.MedicareWages = W2AmountStringToDecimal(dataContractW2.Ww2oMedicareWages, recordId, dataContractW2);
            domainEntityW2.MedicareWithholding = W2AmountStringToDecimal(dataContractW2.Ww2oMedicareWithholding, recordId, dataContractW2);
            domainEntityW2.SocialSecurityTips = W2AmountStringToDecimal(dataContractW2.Ww2oSocSecTips, recordId, dataContractW2);
            domainEntityW2.AllocatedTips = W2AmountStringToDecimal(dataContractW2.Ww2oAllocatedTips, recordId, dataContractW2);
            if (dataContractW2.Ww2oYear == "2010")
            {
                domainEntityW2.AdvancedEic = W2AmountStringToDecimal(dataContractW2.Ww2oAdvanceEic, recordId, dataContractW2);
            }
            domainEntityW2.DependentCare = W2AmountStringToDecimal(dataContractW2.Ww2oDependentCare, recordId, dataContractW2);
            domainEntityW2.NonqualifiedTotal = W2AmountStringToDecimal(dataContractW2.Ww2oNonqualTotal, recordId, dataContractW2);

            // Populate the data for the appropriate boxes on the W-2 pdf

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oCodeBoxCodeE))
            {
                domainEntityW2.Box12aCode = dataContractW2.Ww2oCodeBoxCodeE;
                //domainEntityW2.Box12aAmount = dataContractW2.Ww2oCodeBoxAmountE;
                domainEntityW2.Box12aAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountE, recordId, dataContractW2);
            }
            else
            {
                domainEntityW2.Box12aCode = dataContractW2.Ww2oCodeBoxCodeA;
                //domainEntityW2.Box12aAmount = dataContractW2.Ww2oCodeBoxAmountA;
                domainEntityW2.Box12aAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oCodeBoxCodeF))
            {
                domainEntityW2.Box12bCode = dataContractW2.Ww2oCodeBoxCodeF;
                domainEntityW2.Box12bAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountF, recordId, dataContractW2);
            }
            else
            {
                domainEntityW2.Box12bCode = dataContractW2.Ww2oCodeBoxCodeB;
                domainEntityW2.Box12bAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountB, recordId, dataContractW2);
            }

            domainEntityW2.Box12cCode = dataContractW2.Ww2oCodeBoxCodeC;
            domainEntityW2.Box12cAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountC, recordId, dataContractW2);
            domainEntityW2.Box12dCode = dataContractW2.Ww2oCodeBoxCodeD;
            domainEntityW2.Box12dAmount = W2AmountStringToDecimal(dataContractW2.Ww2oCodeBoxAmountD, recordId, dataContractW2);

            domainEntityW2.Box13CheckBox1 = dataContractW2.Ww2oCheckBox1;
            domainEntityW2.Box13CheckBox2 = dataContractW2.Ww2oCheckBox3;
            domainEntityW2.Box13CheckBox3 = dataContractW2.Ww2oCheckBox6;

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeE))
            {
                domainEntityW2.Box14Line1 = dataContractW2.Ww2oOtherBoxCodeE + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountE, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeA))
            {
                domainEntityW2.Box14Line1 = dataContractW2.Ww2oOtherBoxCodeA + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeF))
            {
                domainEntityW2.Box14Line2 = dataContractW2.Ww2oOtherBoxCodeF + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountF, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeB))
            {
                domainEntityW2.Box14Line2 = dataContractW2.Ww2oOtherBoxCodeB + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountB, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeC))
                domainEntityW2.Box14Line3 = dataContractW2.Ww2oOtherBoxCodeC + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountC, recordId, dataContractW2);

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oOtherBoxCodeD))
                domainEntityW2.Box14Line4 = dataContractW2.Ww2oOtherBoxCodeD + " - " + W2AmountStringToDecimal(dataContractW2.Ww2oOtherBoxAmountD, recordId, dataContractW2);

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateCodeC))
            {
                domainEntityW2.Box15Line1Section1 = dataContractW2.Ww2oStateCodeC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateCodeA))
            {
                domainEntityW2.Box15Line1Section1 = dataContractW2.Ww2oStateCodeA;
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateCodeD))
            {
                domainEntityW2.Box15Line2Section1 = dataContractW2.Ww2oStateCodeD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateCodeB))
            {
                domainEntityW2.Box15Line2Section1 = dataContractW2.Ww2oStateCodeB;
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateIdC))
            {
                domainEntityW2.Box15Line1Section2 = dataContractW2.Ww2oStateIdC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateIdA))
            {
                domainEntityW2.Box15Line1Section2 = dataContractW2.Ww2oStateIdA;
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateIdD))
            {
                domainEntityW2.Box15Line2Section2 = dataContractW2.Ww2oStateIdD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateIdB))
            {
                domainEntityW2.Box15Line2Section2 = dataContractW2.Ww2oStateIdB;
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWagesC))
            {
                domainEntityW2.Box16Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWagesC, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWagesA))
            {
                domainEntityW2.Box16Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWagesA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWagesD))
            {
                domainEntityW2.Box16Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWagesD, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWagesB))
            {
                domainEntityW2.Box16Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWagesB, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWithheldC))
            {
                domainEntityW2.Box17Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWithheldC, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWithheldA))
            {
                domainEntityW2.Box17Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWithheldA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWithheldD))
            {
                domainEntityW2.Box17Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWithheldD, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oStateWithheldB))
            {
                domainEntityW2.Box17Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oStateWithheldB, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWagesC))
            {
                domainEntityW2.Box18Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWagesC, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWagesA))
            {
                domainEntityW2.Box18Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWagesA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWagesD))
            {
                domainEntityW2.Box18Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWagesD, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWagesB))
            {
                domainEntityW2.Box18Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWagesB, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWithheldC))
            {
                domainEntityW2.Box19Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWithheldC, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWithheldA))
            {
                domainEntityW2.Box19Line1 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWithheldA, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWithheldD))
            {
                domainEntityW2.Box19Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWithheldD, recordId, dataContractW2);
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalWithheldB))
            {
                domainEntityW2.Box19Line2 = W2AmountStringToDecimal(dataContractW2.Ww2oLocalWithheldB, recordId, dataContractW2);
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalNameC))
            {
                domainEntityW2.Box20Line1 = dataContractW2.Ww2oLocalNameC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalNameA))
            {
                domainEntityW2.Box20Line1 = dataContractW2.Ww2oLocalNameA;
            }

            if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalNameD))
            {
                domainEntityW2.Box20Line2 = dataContractW2.Ww2oLocalNameD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2.Ww2oLocalNameB))
            {
                domainEntityW2.Box20Line2 = dataContractW2.Ww2oLocalNameB;
            }

            // Call the PDF accessed CTX to send an email notification
            TxNotifyHrPdfAccessRequest pdfRequest = new TxNotifyHrPdfAccessRequest();
            pdfRequest.AFormType = "W2";
            pdfRequest.APersonId = dataContractW2.Ww2oEmployeeId;
            pdfRequest.ARecordId = dataContractW2.Recordkey;

            var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyHrPdfAccessRequest, TxNotifyHrPdfAccessResponse>(pdfRequest);


            return domainEntityW2;
        }

        /// <summary>
        /// Get the pdf data for tax form W-2C
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2C.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a W-2C tax form</param>
        /// <returns>The pdf data for tax form W-2C</returns>
        public async Task<FormW2cPdfData> GetW2cPdfAsync(string personId, string recordId)
        {
            // Throw an exception if there is no record id to get the W-2 tax form data
            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "The record ID is required.");

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "The person ID is required.");

            // Read the record where the W-2C tax form data is stored
            string criteria = "WITH WEB.W2C.ONLINE.ID EQ '" + recordId + "'" + " AND WITH WW2CO.EMPLOYEE.ID EQ '" + personId + "'";
            var w2CIds = await DataReader.SelectAsync("WEB.W2C.ONLINE", criteria);

            if (w2CIds == null)
                throw new ApplicationException("One WEB.W2C.ONLINE ID expected but null returned for record ID: " + recordId);

            if (w2CIds.Count() == 0)
                throw new ApplicationException("One WEB.W2C.ONLINE ID expected but zero returned for record ID: " + recordId);

            if (w2CIds.Count() > 1)
                throw new ApplicationException("One WEB.W2C.ONLINE ID expected but more than one returned for record ID: " + recordId);

            var dataContractW2c = await DataReader.ReadRecordAsync<WebW2cOnline>(w2CIds.FirstOrDefault());

            // Validate that we found the record and that it contains required fields for the constructor
            if (dataContractW2c == null)
                throw new ApplicationException("WebW2cOnline record " + recordId + " does not exist.");

            if (string.IsNullOrEmpty(dataContractW2c.Ww2coCorrectionYear))
                throw new ApplicationException("WebW2cOnline record " + dataContractW2c.Recordkey + "must have a tax year.");

            if (string.IsNullOrEmpty(dataContractW2c.Ww2coEmplyrId))
                throw new ApplicationException("WebW2cOnline record " + dataContractW2c.Recordkey + "must have an employer tax ID (EIN).");

            // Initialize the SSN
            string ssn = "";
            if (dataContractW2c != null && !string.IsNullOrEmpty(dataContractW2c.Ww2coSsn))
            {
                ssn = dataContractW2c.Ww2coSsn;
            }

            var hrWebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.HumanResources.DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
            if (hrWebDefaults != null)
            {
                // Mask the SSN if necessary.
                if (!string.IsNullOrEmpty(hrWebDefaults.HrwebW2oMaskSsn) && hrWebDefaults.HrwebW2oMaskSsn.ToUpper() == "Y")
                {
                    if (!string.IsNullOrEmpty(ssn))
                    {
                        // Mask SSN
                        if (ssn.Length >= 4)
                        {
                            ssn = "XXX-XX-" + ssn.Substring(ssn.Length - 4);
                        }
                        else
                        {
                            ssn = "XXX-XX-" + ssn;
                        }
                    }
                }
            }

            // Instantiate a W-2c domain entity
            var domainEntityW2c = new FormW2cPdfData(dataContractW2c.Ww2coCorrectionYear, dataContractW2c.Ww2coEmplyrId, ssn);

            // Get the employer information
            domainEntityW2c.EmployerName = dataContractW2c.Ww2coEmplyrName;
            domainEntityW2c.EmployerAddressLine1 = dataContractW2c.Ww2coEmplyrAddrLine1;
            domainEntityW2c.EmployerAddressLine2 = dataContractW2c.Ww2coEmplyrAddrLine2;
            domainEntityW2c.EmployerAddressLine3 = dataContractW2c.Ww2coEmplyrAddrLine3;
            domainEntityW2c.EmployerAddressLine4 = dataContractW2c.Ww2coEmplyrAddrLine4;

            // Get the employee information
            domainEntityW2c.EmployeeId = dataContractW2c.Ww2coEmployeeId;
            domainEntityW2c.EmployeeFirstName = dataContractW2c.Ww2coFirstName;
            domainEntityW2c.EmployeeLastName = dataContractW2c.Ww2coLastName;
            domainEntityW2c.EmployeeMiddleName = !string.IsNullOrEmpty(dataContractW2c.Ww2coMiddleName) ? dataContractW2c.Ww2coMiddleName.Substring(0, 1) : string.Empty;
            domainEntityW2c.EmployeeSuffix = dataContractW2c.Ww2coSuffix;
            domainEntityW2c.EmployeeAddressLine1 = dataContractW2c.Ww2coEmplyeAddrLine1;
            domainEntityW2c.EmployeeAddressLine2 = dataContractW2c.Ww2coEmplyeAddrLine2;
            domainEntityW2c.EmployeeAddressLine3 = dataContractW2c.Ww2coEmplyeAddrLine3;
            domainEntityW2c.EmployeeAddressLine4 = dataContractW2c.Ww2coEmplyeAddrLine4;

            domainEntityW2c.FederalWages = W2cAmountStringToDecimal(dataContractW2c.Ww2coFederalWages, recordId, dataContractW2c);
            domainEntityW2c.FederalWithholding = W2cAmountStringToDecimal(dataContractW2c.Ww2coFederalWithholding, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityWages = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecWages, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityWithholding = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecWithholding, recordId, dataContractW2c);
            domainEntityW2c.MedicareWages = W2cAmountStringToDecimal(dataContractW2c.Ww2coMedicareWages, recordId, dataContractW2c);
            domainEntityW2c.MedicareWithholding = W2cAmountStringToDecimal(dataContractW2c.Ww2coMedicareWithholding, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityTips = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecTips, recordId, dataContractW2c);
            domainEntityW2c.AllocatedTips = W2cAmountStringToDecimal(dataContractW2c.Ww2coAllocatedTips, recordId, dataContractW2c);

            if (dataContractW2c.Ww2coCorrectionYear == "2010")
            {
                domainEntityW2c.AdvancedEic = W2cAmountStringToDecimal(dataContractW2c.Ww2coAdvanceEic, recordId, dataContractW2c);
            }
            domainEntityW2c.DependentCare = W2cAmountStringToDecimal(dataContractW2c.Ww2coDependentCare, recordId, dataContractW2c);
            domainEntityW2c.NonqualifiedTotal = W2cAmountStringToDecimal(dataContractW2c.Ww2coNonqualTotal, recordId, dataContractW2c);

            // Populate the data for the appropriate boxes on the W-2c pdf

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coCodeBoxCodeE))
            {
                domainEntityW2c.Box12aCode = dataContractW2c.Ww2coCodeBoxCodeE;
                domainEntityW2c.Box12aAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountE, recordId, dataContractW2c);
            }
            else
            {
                domainEntityW2c.Box12aCode = dataContractW2c.Ww2coCodeBoxCodeA;
                domainEntityW2c.Box12aAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coCodeBoxCodeF))
            {
                domainEntityW2c.Box12bCode = dataContractW2c.Ww2coCodeBoxCodeF;
                domainEntityW2c.Box12bAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountF, recordId, dataContractW2c);
            }
            else
            {
                domainEntityW2c.Box12bCode = dataContractW2c.Ww2coCodeBoxCodeB;
                domainEntityW2c.Box12bAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountB, recordId, dataContractW2c);
            }

            domainEntityW2c.Box12cCode = dataContractW2c.Ww2coCodeBoxCodeC;
            domainEntityW2c.Box12cAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountC, recordId, dataContractW2c);
            domainEntityW2c.Box12dCode = dataContractW2c.Ww2coCodeBoxCodeD;
            domainEntityW2c.Box12dAmount = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmountD, recordId, dataContractW2c);

            if (dataContractW2c.Ww2coCheckBox1 == "A")
            {
                domainEntityW2c.Box13CheckBox1 = "x";
                domainEntityW2c.Box13CheckBox1Prev = null;

            }
            else if (dataContractW2c.Ww2coCheckBox1 == "R")
            {
                domainEntityW2c.Box13CheckBox1 = null;
                domainEntityW2c.Box13CheckBox1Prev = "x";
            }
            else
            {
                domainEntityW2c.Box13CheckBox1 = null;
                domainEntityW2c.Box13CheckBox1Prev = null;
            }

            if (dataContractW2c.Ww2coCheckBox3 == "A")
            {
                domainEntityW2c.Box13CheckBox2 = "x";
                domainEntityW2c.Box13CheckBox2Prev = null;

            }
            else if (dataContractW2c.Ww2coCheckBox3 == "R")
            {
                domainEntityW2c.Box13CheckBox2 = null;
                domainEntityW2c.Box13CheckBox2Prev = "x";
            }
            else
            {
                domainEntityW2c.Box13CheckBox2 = null;
                domainEntityW2c.Box13CheckBox3Prev = null;
            }

            if (dataContractW2c.Ww2coCheckBox6 == "A")
            {
                domainEntityW2c.Box13CheckBox3 = "x";
                domainEntityW2c.Box13CheckBox3Prev = null;

            }
            else if (dataContractW2c.Ww2coCheckBox6 == "R")
            {
                domainEntityW2c.Box13CheckBox3 = null;
                domainEntityW2c.Box13CheckBox3Prev = "x";
            }
            else
            {
                domainEntityW2c.Box13CheckBox3 = null;
                domainEntityW2c.Box13CheckBox3Prev = null;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeE))
            {
                domainEntityW2c.Box14Line1 = dataContractW2c.Ww2coOtherBoxCodeE + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountE, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeA))
            {
                domainEntityW2c.Box14Line1 = dataContractW2c.Ww2coOtherBoxCodeA + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeF))
            {
                domainEntityW2c.Box14Line2 = dataContractW2c.Ww2coOtherBoxCodeF + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountF, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeB))
            {
                domainEntityW2c.Box14Line2 = dataContractW2c.Ww2coOtherBoxCodeB + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountB, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeC))
                domainEntityW2c.Box14Line3 = dataContractW2c.Ww2coOtherBoxCodeC + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountC, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeD))
                domainEntityW2c.Box14Line4 = dataContractW2c.Ww2coOtherBoxCodeD + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmountD, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeC))
            {
                domainEntityW2c.Box15Line1Section1 = dataContractW2c.Ww2coStateCodeC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeA))
            {
                domainEntityW2c.Box15Line1Section1 = dataContractW2c.Ww2coStateCodeA;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeD))
            {
                domainEntityW2c.Box15Line2Section1 = dataContractW2c.Ww2coStateCodeD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeB))
            {
                domainEntityW2c.Box15Line2Section1 = dataContractW2c.Ww2coStateCodeB;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdC))
            {
                domainEntityW2c.Box15Line1Section2 = dataContractW2c.Ww2coStateIdC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdA))
            {
                domainEntityW2c.Box15Line1Section2 = dataContractW2c.Ww2coStateIdA;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdD))
            {
                domainEntityW2c.Box15Line2Section2 = dataContractW2c.Ww2coStateIdD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdB))
            {
                domainEntityW2c.Box15Line2Section2 = dataContractW2c.Ww2coStateIdB;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesC))
            {
                domainEntityW2c.Box16Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesC, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesA))
            {
                domainEntityW2c.Box16Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesD))
            {
                domainEntityW2c.Box16Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesD, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesB))
            {
                domainEntityW2c.Box16Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesB, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldC))
            {
                domainEntityW2c.Box17Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldC, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldA))
            {
                domainEntityW2c.Box17Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldD))
            {
                domainEntityW2c.Box17Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldD, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldB))
            {
                domainEntityW2c.Box17Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldB, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesC))
            {
                domainEntityW2c.Box18Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesC, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesA))
            {
                domainEntityW2c.Box18Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesD))
            {
                domainEntityW2c.Box18Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesD, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesB))
            {
                domainEntityW2c.Box18Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesB, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldC))
            {
                domainEntityW2c.Box19Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldC, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldA))
            {
                domainEntityW2c.Box19Line1 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldA, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldD))
            {
                domainEntityW2c.Box19Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldD, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldB))
            {
                domainEntityW2c.Box19Line2 = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldB, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameC))
            {
                domainEntityW2c.Box20Line1 = dataContractW2c.Ww2coLocalNameC;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameA))
            {
                domainEntityW2c.Box20Line1 = dataContractW2c.Ww2coLocalNameA;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameD))
            {
                domainEntityW2c.Box20Line2 = dataContractW2c.Ww2coLocalNameD;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameB))
            {
                domainEntityW2c.Box20Line2 = dataContractW2c.Ww2coLocalNameB;
            }

            // Assign the previous W2 values
            if (dataContractW2c.Ww2coChangedSsnOrName == "Y")
            {
                domainEntityW2c.ChangesSsnOrName = "x";
                string ssnPrev = "";
                if (dataContractW2c != null && !string.IsNullOrEmpty(dataContractW2c.Ww2coSsnPrev))
                {
                    ssnPrev = dataContractW2c.Ww2coSsnPrev;
                }

                if (hrWebDefaults != null)
                {
                    // Mask the SSN if necessary.
                    if (!string.IsNullOrEmpty(hrWebDefaults.HrwebW2oMaskSsn) && hrWebDefaults.HrwebW2oMaskSsn.ToUpper() == "Y")
                    {
                        if (!string.IsNullOrEmpty(ssnPrev))
                        {
                            // Mask SSN
                            if (ssnPrev.Length >= 4)
                            {
                                ssnPrev = "XXX-XX-" + ssnPrev.Substring(ssnPrev.Length - 4);
                            }
                            else
                            {
                                ssnPrev = "XXX-XX-" + ssnPrev;
                            }
                        }
                    }
                }
                domainEntityW2c.EmployeeSsnPrev = ssnPrev;
                domainEntityW2c.EmployeeNamePrev = dataContractW2c.Ww2coEmployeeNamePrev;
            }
            domainEntityW2c.FederalWagesPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coFederalWagesPrev, recordId, dataContractW2c);
            domainEntityW2c.FederalWithholdingPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coFederalWthPrev, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityWagesPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecWagesPrev, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityWithholdingPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecWthPrev, recordId, dataContractW2c);
            domainEntityW2c.MedicareWagesPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coMedicareWagesPrev, recordId, dataContractW2c);
            domainEntityW2c.MedicareWithholdingPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coMedicareWthPrev, recordId, dataContractW2c);
            domainEntityW2c.SocialSecurityTipsPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coSocSecTipsPrev, recordId, dataContractW2c);
            domainEntityW2c.AllocatedTipsPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coAllocatedTipsPrev, recordId, dataContractW2c);
            if (dataContractW2c.Ww2coCorrectionYear == "2010")
            {
                domainEntityW2c.AdvancedEicPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coAdvanceEicPrev, recordId, dataContractW2c);
            }
            domainEntityW2c.DependentCarePrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coDependentCarePrev, recordId, dataContractW2c);
            domainEntityW2c.NonqualifiedTotalPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coNonqualTotalPrev, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coCodeBoxCodeEPrev))
            {
                domainEntityW2c.Box12aCodePrev = dataContractW2c.Ww2coCodeBoxCodeEPrev;
                domainEntityW2c.Box12aAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntEPrev, recordId, dataContractW2c);
            }
            else
            {
                domainEntityW2c.Box12aCodePrev = dataContractW2c.Ww2coCodeBoxCodeAPrev;
                domainEntityW2c.Box12aAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coCodeBoxCodeFPrev))
            {
                domainEntityW2c.Box12bCodePrev = dataContractW2c.Ww2coCodeBoxCodeFPrev;
                domainEntityW2c.Box12bAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntFPrev, recordId, dataContractW2c);
            }
            else
            {
                domainEntityW2c.Box12bCodePrev = dataContractW2c.Ww2coCodeBoxCodeBPrev;
                domainEntityW2c.Box12bAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntBPrev, recordId, dataContractW2c);
            }


            domainEntityW2c.Box12cCodePrev = dataContractW2c.Ww2coCodeBoxCodeCPrev;
            domainEntityW2c.Box12cAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntCPrev, recordId, dataContractW2c);
            domainEntityW2c.Box12dCodePrev = dataContractW2c.Ww2coCodeBoxCodeDPrev;
            domainEntityW2c.Box12dAmountPrev = W2cAmountStringToDecimal(dataContractW2c.Ww2coCodeBoxAmntDPrev, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeEPrev))
            {
                domainEntityW2c.Box14Line1Prev = dataContractW2c.Ww2coOtherBoxCodeEPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntEPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeAPrev))
            {
                domainEntityW2c.Box14Line1Prev = dataContractW2c.Ww2coOtherBoxCodeAPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeFPrev))
            {
                domainEntityW2c.Box14Line2Prev = dataContractW2c.Ww2coOtherBoxCodeFPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntFPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeBPrev))
            {
                domainEntityW2c.Box14Line2Prev = dataContractW2c.Ww2coOtherBoxCodeBPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntBPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeCPrev))
                domainEntityW2c.Box14Line3Prev = dataContractW2c.Ww2coOtherBoxCodeCPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntCPrev, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coOtherBoxCodeDPrev))
                domainEntityW2c.Box14Line4Prev = dataContractW2c.Ww2coOtherBoxCodeDPrev + " - " + W2cAmountStringToDecimal(dataContractW2c.Ww2coOtherBoxAmntDPrev, recordId, dataContractW2c);

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeCPrev))
            {
                domainEntityW2c.Box15Line1Section1Prev = dataContractW2c.Ww2coStateCodeCPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeAPrev))
            {
                domainEntityW2c.Box15Line1Section1Prev = dataContractW2c.Ww2coStateCodeAPrev;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeDPrev))
            {
                domainEntityW2c.Box15Line2Section1Prev = dataContractW2c.Ww2coStateCodeDPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateCodeBPrev))
            {
                domainEntityW2c.Box15Line2Section1Prev = dataContractW2c.Ww2coStateCodeBPrev;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdCPrev))
            {
                domainEntityW2c.Box15Line1Section2Prev = dataContractW2c.Ww2coStateIdCPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdAPrev))
            {
                domainEntityW2c.Box15Line1Section2Prev = dataContractW2c.Ww2coStateIdAPrev;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdDPrev))
            {
                domainEntityW2c.Box15Line2Section2Prev = dataContractW2c.Ww2coStateIdDPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateIdBPrev))
            {
                domainEntityW2c.Box15Line2Section2Prev = dataContractW2c.Ww2coStateIdBPrev;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesCPrev))
            {
                domainEntityW2c.Box16Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesCPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesAPrev))
            {
                domainEntityW2c.Box16Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesDPrev))
            {
                domainEntityW2c.Box16Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesDPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWagesBPrev))
            {
                domainEntityW2c.Box16Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWagesBPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldCPrev))
            {
                domainEntityW2c.Box17Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldCPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldAPrev))
            {
                domainEntityW2c.Box17Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldDPrev))
            {
                domainEntityW2c.Box17Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldDPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coStateWithheldBPrev))
            {
                domainEntityW2c.Box17Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coStateWithheldBPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesCPrev))
            {
                domainEntityW2c.Box18Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesCPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesAPrev))
            {
                domainEntityW2c.Box18Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesDPrev))
            {
                domainEntityW2c.Box18Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesDPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWagesBPrev))
            {
                domainEntityW2c.Box18Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWagesBPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldCPrev))
            {
                domainEntityW2c.Box19Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldCPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldAPrev))
            {
                domainEntityW2c.Box19Line1Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldAPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldDPrev))
            {
                domainEntityW2c.Box19Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldDPrev, recordId, dataContractW2c);
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalWithheldBPrev))
            {
                domainEntityW2c.Box19Line2Prev = W2cAmountStringToDecimal(dataContractW2c.Ww2coLocalWithheldBPrev, recordId, dataContractW2c);
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameCPrev))
            {
                domainEntityW2c.Box20Line1Prev = dataContractW2c.Ww2coLocalNameCPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameAPrev))
            {
                domainEntityW2c.Box20Line1Prev = dataContractW2c.Ww2coLocalNameAPrev;
            }

            if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameDPrev))
            {
                domainEntityW2c.Box20Line2Prev = dataContractW2c.Ww2coLocalNameDPrev;
            }
            else if (!string.IsNullOrEmpty(dataContractW2c.Ww2coLocalNameBPrev))
            {
                domainEntityW2c.Box20Line2Prev = dataContractW2c.Ww2coLocalNameBPrev;
            }
            domainEntityW2c.CorrectionYear = dataContractW2c.Ww2coCorrectionYear;
            // Call the PDF accessed CTX to send an email notification
            TxNotifyHrPdfAccessRequest pdfRequest = new TxNotifyHrPdfAccessRequest();
            pdfRequest.AFormType = "W2C";
            pdfRequest.APersonId = dataContractW2c.Ww2coEmployeeId;
            pdfRequest.ARecordId = dataContractW2c.Recordkey;

            var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyHrPdfAccessRequest, TxNotifyHrPdfAccessResponse>(pdfRequest);


            return domainEntityW2c;
        }

        /// <summary>
        /// Get the pdf data for tax form 1095-C
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a 1095-C tax form</param>
        /// <returns>The pdf data for tax form 1095-C</returns>
        public async Task<Form1095cPdfData> Get1095cPdfAsync(string personId, string recordId)
        {
            // Throw an exception if there is no record id to get the 1095-C tax form data
            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "The record ID is required.");

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "The person ID is required.");

            string selectCriteria = "WITH TAX.FORM.1095C.WHIST.ID EQ '" + recordId + "'" + " AND WITH TFCWH.HRPER.ID EQ '" + personId + "'";
            var form1095cIds = await DataReader.SelectAsync("TAX.FORM.1095C.WHIST", selectCriteria);

            if (form1095cIds == null)
                throw new ApplicationException("One TAX.FORM.1095C.WHIST ID expected but null returned for record ID: " + recordId);

            if (form1095cIds.Count() == 0)
                throw new ApplicationException("One TAX.FORM.1095C.WHIST ID expected but zero returned for record ID: " + recordId);

            if (form1095cIds.Count() > 1)
                throw new ApplicationException("One TAX.FORM.1095C.WHIST ID expected but more than one returned for record ID: " + recordId);

            // Read the 1095-C record containing the data for the pdf
            var dataContract1095cWhist = await DataReader.ReadRecordAsync<TaxForm1095cWhist>(form1095cIds.FirstOrDefault());

            // Throw an exception if there is no record data
            if (dataContract1095cWhist == null)
            {
                throw new ApplicationException("TaxForm1095cWhist record " + recordId + " does not exist.");
            }

            // Throw an exception if there is no tax year
            if (string.IsNullOrEmpty(dataContract1095cWhist.TfcwhTaxYear))
            {
                throw new ApplicationException("Missing tax year for 1095-C pdf data: " + dataContract1095cWhist.Recordkey);
            }

            bool usePaymaster = false;
            string employerEin = string.Empty;
            string hostOrganizationId = null;
            Paymstr paymasterContract = null;

            // Some clients may not have payroll licensed/installed, so they would not have employer
            // information in the PAYROLL.MASTER record even though they may have the record itself.
            // Find out if the payroll module, PR, is installed in the environment.
            var installedApplsContract = await DataReader.ReadRecordAsync<InstalledAppls>("SYSDEFS", "INSTALLED.APPLICATIONS");
            if (installedApplsContract != null)
            {
                // Check if PR is present in the list of installed modules.
                var installedPayrollModule = installedApplsContract.IaModulesEntityAssociation.FirstOrDefault(x => x.IaModuleNamesAssocMember == "PR");
                if (installedPayrollModule != null)
                {
                    // Read the PAYMSTR record from payroll to get the employer information
                    paymasterContract = await DataReader.ReadRecordAsync<Paymstr>("ACCOUNT.PARAMETERS", "PAYROLL.MASTER");
                    if (paymasterContract != null)
                    {
                        employerEin = paymasterContract.PmInstitutionEin;
                        logger.Warn("Paymaster EIN: " + employerEin);
                        if (!string.IsNullOrEmpty(employerEin))
                        {
                            usePaymaster = true;
                        }
                    }
                }
            }

            logger.Warn("Use paymaster: " + usePaymaster.ToString());

            if (!usePaymaster)
            {
                // If the EIN is not in this record, obtain the employer information from the Host Organization.
                var defaultsContract = await GetDefaults();
                hostOrganizationId = defaultsContract.DefaultHostCorpId;
                logger.Warn("Default host corp ID: " + hostOrganizationId);

                // Read the CORP.FOUNDS record for the host organization ID to get the employer EIN.
                if (string.IsNullOrEmpty(hostOrganizationId))
                {
                    throw new ApplicationException("Host Organization ID is missing");
                }

                CorpFounds corpFounds = await DataReader.ReadRecordAsync<CorpFounds>(hostOrganizationId);

                if (corpFounds == null)
                {
                    throw new ApplicationException("CORP.FOUNDS record " + hostOrganizationId + " does not exist.");
                }

                // Throw an exception if the host organization EIN is missing.
                if (string.IsNullOrEmpty(corpFounds.CorpTaxId))
                {
                    throw new ApplicationException("Missing employer EIN from the host organization record " + hostOrganizationId);
                }
                employerEin = corpFounds.CorpTaxId;
            }

            // Format the EIN if it hasn't already been formatted.
            if (!employerEin.Contains("-") && employerEin.Length > 2)
            {
                employerEin = employerEin.Insert(2, "-");
            }

            // Get the employee SSN from the Person record.
            var personIdForPersonContract = dataContract1095cWhist.TfcwhHrperId;
            if (string.IsNullOrEmpty(personIdForPersonContract))
            {
                throw new ApplicationException("Employee ID " + personIdForPersonContract + " is required.");
            }

            Data.Base.DataContracts.Person personContract = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Person>(personIdForPersonContract);

            // Initialize the SSN
            string ssn = "";
            if (personContract != null && !string.IsNullOrEmpty(personContract.Ssn))
            {
                ssn = personContract.Ssn;
            }

            var hrWebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.HumanResources.DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
            if (hrWebDefaults != null)
            {
                // Mask the SSN if necessary.
                if (!string.IsNullOrEmpty(hrWebDefaults.Hrweb1095cMaskSsn) && hrWebDefaults.Hrweb1095cMaskSsn.ToUpper() == "Y")
                {
                    if (!string.IsNullOrEmpty(ssn))
                    {
                        // Mask SSN
                        if (ssn.Length >= 4)
                        {
                            ssn = "XXX-XX-" + ssn.Substring(ssn.Length - 4);
                        }
                        else
                        {
                            ssn = "XXX-XX-" + ssn;
                        }
                    }
                }
            }

            // Create a new 1095-C pdf data domain entity.
            var domainEntity1095c = new Form1095cPdfData(dataContract1095cWhist.TfcwhTaxYear, employerEin, ssn);

            // Read the record that contains the employer's contact phone number.
            var qtdYtdParameter1095CPDFContract = await DataReader.ReadRecordAsync<Data.HumanResources.DataContracts.QtdYtdParameter1095CPDF>("HR.PARMS", "QTD.YTD.PARAMETER");
            if (qtdYtdParameter1095CPDFContract == null)
            {
                throw new ApplicationException("Unable to access QTD.YTD.PARAMETER from HR.PARMS table.");
            }

            // Get the employer's contact phone number and extension.
            domainEntity1095c.EmployerContactPhoneNumber = qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone;
            if (!qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone.Contains("-") && qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone.Length == 10)
            {
                domainEntity1095c.EmployerContactPhoneNumber = String.Format("{0}-{1}-{2}",
                    qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone.Substring(0, 3),
                    qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone.Substring(3, 3),
                    qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone.Substring(6, 4));
            }

            domainEntity1095c.EmployerContactPhoneExtension = qtdYtdParameter1095CPDFContract.Qyp1095cContactExt;

            // Set the plan start month
            domainEntity1095c.PlanStartMonthCode = qtdYtdParameter1095CPDFContract.Qyp1095cPlanStartMonth;
            if (domainEntity1095c.PlanStartMonthCode.Length == 1)
                domainEntity1095c.PlanStartMonthCode = "0" + domainEntity1095c.PlanStartMonthCode;

            // Employer's demographic data. Use data from PAYMSTR if it is present.
            if (usePaymaster == true)
            {
                domainEntity1095c.EmployerName = paymasterContract.PmInstitutionName;
                domainEntity1095c.EmployerAddressLine = paymasterContract.PmInstitutionAddress.FirstOrDefault();
                domainEntity1095c.EmployerCityName = paymasterContract.PmInstitutionCity;
                domainEntity1095c.EmployerStateCode = paymasterContract.PmInstitutionState;
                domainEntity1095c.EmployerZipCode = paymasterContract.PmInstitutionZipcode;
            }
            else if (!string.IsNullOrEmpty(hostOrganizationId))
            {
                // The PAYROLL module is not licensed. Get the employer's name and address from the host organization ID using the PREFERRED hierarchy.

                TxGetHierarchyNameRequest nameRequest = new TxGetHierarchyNameRequest()
                {
                    IoPersonId = hostOrganizationId,
                    InHierarchy = "PREFERRED"
                };

                TxGetHierarchyNameResponse nameResponse = transactionInvoker.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(nameRequest);

                if (nameResponse != null)
                {
                    if (nameResponse.OutPersonName != null && nameResponse.OutPersonName.Count >= 1)
                    {
                        var employerName = nameResponse.OutPersonName.ToArray();
                        domainEntity1095c.EmployerName = employerName.FirstOrDefault();
                    }
                }

                TxGetHierarchyAddressRequest addressRequest = new TxGetHierarchyAddressRequest()
                {
                    IoPersonId = hostOrganizationId,
                    InHierarchy = "PREFERRED",
                    InDate = DateTime.Today
                };

                TxGetHierarchyAddressResponse addressResponse = transactionInvoker.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(addressRequest);

                if (addressResponse != null)
                {
                    if (!((addressResponse.OutAddressLines == null) || (addressResponse.OutAddressLines.Count < 1)))
                    {
                        domainEntity1095c.EmployerAddressLine = addressResponse.OutAddressLines.FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(addressResponse.OutAddressCity))
                    {
                        domainEntity1095c.EmployerCityName = addressResponse.OutAddressCity;
                    }
                    if (!string.IsNullOrEmpty(addressResponse.OutAddressState))
                    {
                        domainEntity1095c.EmployerStateCode = addressResponse.OutAddressState;
                    }
                    if (!string.IsNullOrEmpty(addressResponse.OutAddressZip))
                    {
                        domainEntity1095c.EmployerZipCode = addressResponse.OutAddressZip;
                    }
                }
            }

            // Assign the status
            domainEntity1095c.IsCorrected = dataContract1095cWhist.TfcwhStatus == "COR";

            // Set the void field
            domainEntity1095c.IsVoided = dataContract1095cWhist.TfcwhVoidInd == "Y";

            // Assign the employee demographic data
            domainEntity1095c.EmployeeId = dataContract1095cWhist.TfcwhHrperId;
            domainEntity1095c.EmployeeFirstName = dataContract1095cWhist.TfcwhFirstName;
            domainEntity1095c.EmployeeLastName = dataContract1095cWhist.TfcwhLastName;
            domainEntity1095c.EmployeeMiddleName = dataContract1095cWhist.TfcwhMiddleName;
            domainEntity1095c.EmployeeAddressLine1 = dataContract1095cWhist.TfcwhAddressLine1Text;
            domainEntity1095c.EmployeeAddressLine2 = dataContract1095cWhist.TfcwhAddressLine2Text;
            domainEntity1095c.EmployeeCityName = dataContract1095cWhist.TfcwhCityName;
            domainEntity1095c.EmployeeStateCode = dataContract1095cWhist.TfcwhStateProvCode;
            domainEntity1095c.EmployeePostalCode = dataContract1095cWhist.TfcwhPostalCode;
            domainEntity1095c.EmployeeZipExtension = dataContract1095cWhist.TfcwhZipExtension;
            domainEntity1095c.EmployeeCountry = dataContract1095cWhist.TfcwhCountryName;

            // Assign the offer of coverage codes
            domainEntity1095c.OfferOfCoverage12Month = dataContract1095cWhist.TfcwhOfferCode12mnth;
            domainEntity1095c.OfferOfCoverageJanuary = dataContract1095cWhist.TfcwhOfferCodeJan;
            domainEntity1095c.OfferOfCoverageFebruary = dataContract1095cWhist.TfcwhOfferCodeFeb;
            domainEntity1095c.OfferOfCoverageMarch = dataContract1095cWhist.TfcwhOfferCodeMar;
            domainEntity1095c.OfferOfCoverageApril = dataContract1095cWhist.TfcwhOfferCodeApr;
            domainEntity1095c.OfferOfCoverageMay = dataContract1095cWhist.TfcwhOfferCodeMay;
            domainEntity1095c.OfferOfCoverageJune = dataContract1095cWhist.TfcwhOfferCodeJun;
            domainEntity1095c.OfferOfCoverageJuly = dataContract1095cWhist.TfcwhOfferCodeJul;
            domainEntity1095c.OfferOfCoverageAugust = dataContract1095cWhist.TfcwhOfferCodeAug;
            domainEntity1095c.OfferOfCoverageSeptember = dataContract1095cWhist.TfcwhOfferCodeSep;
            domainEntity1095c.OfferOfCoverageOctober = dataContract1095cWhist.TfcwhOfferCodeOct;
            domainEntity1095c.OfferOfCoverageNovember = dataContract1095cWhist.TfcwhOfferCodeNov;
            domainEntity1095c.OfferOfCoverageDecember = dataContract1095cWhist.TfcwhOfferCodeDec;

            // Assign the employee share of lowest cost monthly premium
            domainEntity1095c.LowestCostAmount12Month = dataContract1095cWhist.TfcwhLowestCostAmt12mnth;
            domainEntity1095c.LowestCostAmountJanuary = dataContract1095cWhist.TfcwhLowestCostAmtJan;
            domainEntity1095c.LowestCostAmountFebruary = dataContract1095cWhist.TfcwhLowestCostAmtFeb;
            domainEntity1095c.LowestCostAmountMarch = dataContract1095cWhist.TfcwhLowestCostAmtMar;
            domainEntity1095c.LowestCostAmountApril = dataContract1095cWhist.TfcwhLowestCostAmtApr;
            domainEntity1095c.LowestCostAmountMay = dataContract1095cWhist.TfcwhLowestCostAmtMay;
            domainEntity1095c.LowestCostAmountJune = dataContract1095cWhist.TfcwhLowestCostAmtJun;
            domainEntity1095c.LowestCostAmountJuly = dataContract1095cWhist.TfcwhLowestCostAmtJul;
            domainEntity1095c.LowestCostAmountAugust = dataContract1095cWhist.TfcwhLowestCostAmtAug;
            domainEntity1095c.LowestCostAmountSeptember = dataContract1095cWhist.TfcwhLowestCostAmtSep;
            domainEntity1095c.LowestCostAmountOctober = dataContract1095cWhist.TfcwhLowestCostAmtOct;
            domainEntity1095c.LowestCostAmountNovember = dataContract1095cWhist.TfcwhLowestCostAmtNov;
            domainEntity1095c.LowestCostAmountDecember = dataContract1095cWhist.TfcwhLowestCostAmtDec;

            // Assign safe harbor codes
            domainEntity1095c.SafeHarborCode12Month = dataContract1095cWhist.TfcwhSafeHarborCd12mnth;
            domainEntity1095c.SafeHarborCodeJanuary = dataContract1095cWhist.TfcwhSafeHarborCodeJan;
            domainEntity1095c.SafeHarborCodeFebruary = dataContract1095cWhist.TfcwhSafeHarborCodeFeb;
            domainEntity1095c.SafeHarborCodeMarch = dataContract1095cWhist.TfcwhSafeHarborCodeMar;
            domainEntity1095c.SafeHarborCodeApril = dataContract1095cWhist.TfcwhSafeHarborCodeApr;
            domainEntity1095c.SafeHarborCodeMay = dataContract1095cWhist.TfcwhSafeHarborCodeMay;
            domainEntity1095c.SafeHarborCodeJune = dataContract1095cWhist.TfcwhSafeHarborCodeJun;
            domainEntity1095c.SafeHarborCodeJuly = dataContract1095cWhist.TfcwhSafeHarborCodeJul;
            domainEntity1095c.SafeHarborCodeAugust = dataContract1095cWhist.TfcwhSafeHarborCodeAug;
            domainEntity1095c.SafeHarborCodeSeptember = dataContract1095cWhist.TfcwhSafeHarborCodeSep;
            domainEntity1095c.SafeHarborCodeOctober = dataContract1095cWhist.TfcwhSafeHarborCodeOct;
            domainEntity1095c.SafeHarborCodeNovember = dataContract1095cWhist.TfcwhSafeHarborCodeNov;
            domainEntity1095c.SafeHarborCodeDecember = dataContract1095cWhist.TfcwhSafeHarborCodeDec;

            // Is the employee self-insured
            domainEntity1095c.EmployeeIsSelfInsured = false;
            if (dataContract1095cWhist.TfcwhCoveredIndivInd.Equals("1"))
            {
                domainEntity1095c.EmployeeIsSelfInsured = true;
            }

            // If the employee is self-insured, they may have additional covered individuals
            if (domainEntity1095c.EmployeeIsSelfInsured == true)
            {
                var criteria = "WITH TFCCH.1095C.ID EQ '" + dataContract1095cWhist.Recordkey + "'";
                var coveredIndividualsRecords = await DataReader.BulkReadRecordAsync<TaxForm1095cChist>(criteria);

                if (coveredIndividualsRecords != null)
                {
                    // First loop through the covered individual records to obtain the person IDs to do a bulkRead
                    List<string> coveredIndividualsPersonIds = new List<string>();
                    foreach (var coveredIndividual in coveredIndividualsRecords)
                    {
                        if (coveredIndividual != null)
                        {
                            coveredIndividualsPersonIds.Add(coveredIndividual.TfcchPersonId);
                        }
                    }

                    Collection<Person> personContracts = new Collection<Person>();
                    if (coveredIndividualsPersonIds != null && coveredIndividualsPersonIds.Count() > 0)
                    {
                        personContracts = await DataReader.BulkReadRecordAsync<Person>(coveredIndividualsPersonIds.ToArray());
                    }

                    // Now loop again through the covered individual records and populate the necessary data for the pdf.
                    foreach (var coveredIndividual in coveredIndividualsRecords)
                    {
                        if (coveredIndividual != null)
                        {
                            try
                            {
                                // Create a new 1095-C covered covered individual pdf data domain entity
                                var coveredIndividualDomainEntity = new Form1095cCoveredIndividualsPdfData();

                                // Indicate whether this object is for the employee itself or one of its covered individuals
                                coveredIndividualDomainEntity.IsEmployeeItself = false;
                                if (coveredIndividual.TfcchCoverageCode.ToUpper() == "S")
                                {
                                    coveredIndividualDomainEntity.IsEmployeeItself = true;
                                }

                                // Get the covered individual person record
                                Person coveredIndividualPersonContract = personContracts.Where(p => p != null && p.Recordkey == coveredIndividual.TfcchPersonId).FirstOrDefault();

                                if (coveredIndividualPersonContract != null)
                                {
                                    // Covered individual's name
                                    coveredIndividualDomainEntity.CoveredIndividualFirstName = coveredIndividualPersonContract.FirstName;
                                    coveredIndividualDomainEntity.CoveredIndividualMiddleName = coveredIndividualPersonContract.MiddleName;
                                    coveredIndividualDomainEntity.CoveredIndividualLastName = coveredIndividualPersonContract.LastName;

                                    // If the covered individual does not have an SSN use the date of birth
                                    coveredIndividualDomainEntity.CoveredIndividualSsn = coveredIndividualPersonContract.Ssn ?? "";
                                    if (string.IsNullOrEmpty(coveredIndividualPersonContract.Ssn))
                                    {
                                        coveredIndividualDomainEntity.CoveredIndividualDateOfBirth = coveredIndividualPersonContract.BirthDate;
                                    }
                                    else
                                    {
                                        if (hrWebDefaults != null)
                                        {
                                            if (!string.IsNullOrEmpty(hrWebDefaults.Hrweb1095cMaskSsn) && hrWebDefaults.Hrweb1095cMaskSsn.ToUpper() == "Y")
                                            {
                                                // Mask SSN
                                                if (coveredIndividualDomainEntity.CoveredIndividualSsn.Length >= 4)
                                                {
                                                    coveredIndividualDomainEntity.CoveredIndividualSsn = "XXX-XX-" +
                                                        coveredIndividualDomainEntity.CoveredIndividualSsn
                                                        .Substring(coveredIndividualDomainEntity.CoveredIndividualSsn.Length - 4);
                                                }
                                                else
                                                {
                                                    coveredIndividualDomainEntity.CoveredIndividualSsn = "XXX-XX-" +
                                                        coveredIndividualDomainEntity.CoveredIndividualSsn;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Assign the covered individual's coverage
                                coveredIndividualDomainEntity.Covered12Month = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredInd12mnth))
                                {
                                    coveredIndividualDomainEntity.Covered12Month = true;
                                }
                                coveredIndividualDomainEntity.CoveredJanuary = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndJan))
                                {
                                    coveredIndividualDomainEntity.CoveredJanuary = true;
                                }
                                coveredIndividualDomainEntity.CoveredFebruary = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndFeb))
                                {
                                    coveredIndividualDomainEntity.CoveredFebruary = true;
                                }
                                coveredIndividualDomainEntity.CoveredMarch = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndMar))
                                {
                                    coveredIndividualDomainEntity.CoveredMarch = true;
                                }
                                coveredIndividualDomainEntity.CoveredApril = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndApr))
                                {
                                    coveredIndividualDomainEntity.CoveredApril = true;
                                }
                                coveredIndividualDomainEntity.CoveredMay = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndMay))
                                {
                                    coveredIndividualDomainEntity.CoveredMay = true;
                                }
                                coveredIndividualDomainEntity.CoveredJune = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndJun))
                                {
                                    coveredIndividualDomainEntity.CoveredJune = true;
                                }
                                coveredIndividualDomainEntity.CoveredJuly = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndJul))
                                {
                                    coveredIndividualDomainEntity.CoveredJuly = true;
                                }
                                coveredIndividualDomainEntity.CoveredAugust = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndAug))
                                {
                                    coveredIndividualDomainEntity.CoveredAugust = true;
                                }
                                coveredIndividualDomainEntity.CoveredSeptember = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndSep))
                                {
                                    coveredIndividualDomainEntity.CoveredSeptember = true;
                                }
                                coveredIndividualDomainEntity.CoveredOctober = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndOct))
                                {
                                    coveredIndividualDomainEntity.CoveredOctober = true;
                                }
                                coveredIndividualDomainEntity.CoveredNovember = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndNov))
                                {
                                    coveredIndividualDomainEntity.CoveredNovember = true;
                                }
                                coveredIndividualDomainEntity.CoveredDecember = false;
                                if (!string.IsNullOrEmpty(coveredIndividual.TfcchCoveredIndDec))
                                {
                                    coveredIndividualDomainEntity.CoveredDecember = true;
                                }

                                // Add the covered individual to the employee's object
                                domainEntity1095c.AddCoveredIndividual(coveredIndividualDomainEntity);
                            }
                            catch (Exception e)
                            {
                                LogDataError("TaxForm1095cChist", dataContract1095cWhist.TfcwhHrperId, new Object(), e, e.Message);
                            }

                        }
                    }
                }
            }

            // Call the PDF accessed CTX to trigger an email notification
            TxUpdt1095cAccessTriggerRequest request = new TxUpdt1095cAccessTriggerRequest();
            request.TaxFormPdfId = recordId;
            var response = await transactionInvoker.ExecuteAsync<TxUpdt1095cAccessTriggerRequest, TxUpdt1095cAccessTriggerResponse>(request);

            // Call the PDF accessed CTX to send an email notification
            TxNotifyHrPdfAccessRequest pdfRequest = new TxNotifyHrPdfAccessRequest();
            pdfRequest.AFormType = "1095C";
            pdfRequest.APersonId = dataContract1095cWhist.TfcwhHrperId;
            pdfRequest.ARecordId = dataContract1095cWhist.Recordkey;

            var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyHrPdfAccessRequest, TxNotifyHrPdfAccessResponse>(pdfRequest);

            return domainEntity1095c;
        }

        public async Task<FormT4PdfData> GetT4PdfAsync(string personId, string recordId)
        {
            // Throw an exception if there is no record id to get the T4 tax form data
            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "The record ID is required.");

            // Support overflow
            var parsedRecordId = recordId.Split('-')[0];
            var parsedSlipId = Convert.ToInt32(recordId.Split('-').Count() > 1 ? recordId.Split('-')[1] : "0");

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "The person ID is required.");

            // Read the record where the T4 tax form data is stored
            string criteria = "WITH WEB.T4.ONLINE.ID EQ '" + parsedRecordId + "'" + " AND WITH WT4O.EMPLOYEE.ID EQ '" + personId + "'";
            var t4Ids = await DataReader.SelectAsync("WEB.T4.ONLINE", criteria);

            if (t4Ids == null)
                throw new ApplicationException("One WEB.T4.ONLINE ID expected but null returned for record ID: " + recordId);

            if (t4Ids.Count() == 0)
                throw new ApplicationException("One WEB.T4.ONLINE ID expected but zero returned for record ID: " + recordId);

            if (t4Ids.Count() > 1)
                throw new ApplicationException("One WEB.T4.ONLINE ID expected but more than one returned for record ID: " + recordId);

            var dataContractT4 = await DataReader.ReadRecordAsync<WebT4Online>(t4Ids.FirstOrDefault());

            var t4DomainEntity = new FormT4PdfData();

            #region Data on all slips

            string countryDesc = string.Empty;
            if (dataContractT4.Wt4oCountryCode != null)
            {
                if (!string.IsNullOrEmpty(dataContractT4.Wt4oCountryCode))
                {
                    var countryContract = await DataReader.ReadRecordAsync<Countries>(dataContractT4.Wt4oCountryCode);
                    if (countryContract != null)
                    {
                        if (!string.IsNullOrEmpty(countryContract.CtryDesc))
                        {
                            countryDesc = countryContract.CtryDesc;
                        }
                    }
                }
            }

            // Employee info
            t4DomainEntity.EmployeeId = dataContractT4.Wt4oEmployeeId;
            t4DomainEntity.EmployeeFirstName = dataContractT4.Wt4oFirstName;
            t4DomainEntity.EmployeeMiddleName = !string.IsNullOrEmpty(dataContractT4.Wt4oInitial) ? dataContractT4.Wt4oInitial.Substring(0, 1) : string.Empty;
            t4DomainEntity.EmployeeLastName = dataContractT4.Wt4oSurname.ToUpper();

            t4DomainEntity.SocialInsuranceNumber = dataContractT4.Wt4oSin;
            if (!string.IsNullOrEmpty(t4DomainEntity.SocialInsuranceNumber) && t4DomainEntity.SocialInsuranceNumber.Length > 8)
            {
                t4DomainEntity.SocialInsuranceNumber = t4DomainEntity.SocialInsuranceNumber.Substring(0, 3) + " " + t4DomainEntity.SocialInsuranceNumber.Substring(3, 3) + " " + t4DomainEntity.SocialInsuranceNumber.Substring(6);
            }

            if (dataContractT4.Wt4oPensionRgstNo != null)
            {
                t4DomainEntity.RPPorDPSPRegistrationNumber = dataContractT4.Wt4oPensionRgstNo.FirstOrDefault();
            }

            // get AMENDED flag
            if (dataContractT4.Wt4oFormText != null)
            {
                t4DomainEntity.Amended = dataContractT4.Wt4oFormText.FirstOrDefault();
            }

            t4DomainEntity.EmployeeAddressLine1 = dataContractT4.Wt4oAddr1;
            if (string.IsNullOrWhiteSpace(dataContractT4.Wt4oAddr2))
            {
                t4DomainEntity.EmployeeAddressLine2 = (dataContractT4.Wt4oCity ?? string.Empty) + ", " + (dataContractT4.Wt4oProvinceCode ?? string.Empty) + " " + (dataContractT4.Wt4oPostalCode ?? string.Empty);
                t4DomainEntity.EmployeeAddressLine3 = countryDesc;
            }
            else
            {
                t4DomainEntity.EmployeeAddressLine2 = dataContractT4.Wt4oAddr2;
                t4DomainEntity.EmployeeAddressLine3 = (dataContractT4.Wt4oCity ?? string.Empty) + ", " + (dataContractT4.Wt4oProvinceCode ?? string.Empty) + " " + (dataContractT4.Wt4oPostalCode ?? string.Empty);
                t4DomainEntity.EmployeeAddressLine4 = countryDesc;
            }
            t4DomainEntity.EmploymentCode = dataContractT4.Wt4oEmploymentCode;
            t4DomainEntity.ExemptCPPQPP = dataContractT4.Wt4oCppExempt;
            t4DomainEntity.ExemptEI = dataContractT4.Wt4oEiExempt;
            t4DomainEntity.ExemptPPIP = dataContractT4.Wt4oPpipExempt;
            t4DomainEntity.ProvinceOfEmployment = dataContractT4.Wt4oEmploymentProvince;

            // Employer info
            t4DomainEntity.EmployerAddressLine1 = dataContractT4.Wt4oPayerName1;
            if (string.IsNullOrWhiteSpace(dataContractT4.Wt4oPayerName2))
            {
                if (dataContractT4.Wt4oYear != "2010")
                {
                    t4DomainEntity.EmployerAddressLine2 = dataContractT4.Wt4oPayerAddr1;
                    if (string.IsNullOrWhiteSpace(dataContractT4.Wt4oPayerAddr2))
                    {
                        t4DomainEntity.EmployerAddressLine3 = (dataContractT4.Wt4oPayerCity ?? string.Empty) + ", " + (dataContractT4.Wt4oPayerProvCode ?? string.Empty) + " " + (dataContractT4.Wt4oPayerPostalCode ?? string.Empty);
                    }
                    else
                    {
                        t4DomainEntity.EmployerAddressLine3 = dataContractT4.Wt4oPayerAddr2;
                        t4DomainEntity.EmployerAddressLine4 = (dataContractT4.Wt4oPayerCity ?? string.Empty) + ", " + (dataContractT4.Wt4oPayerProvCode ?? string.Empty) + " " + (dataContractT4.Wt4oPayerPostalCode ?? string.Empty);
                    }
                }
            }
            else
            {
                t4DomainEntity.EmployerAddressLine2 = dataContractT4.Wt4oPayerName2;
                t4DomainEntity.EmployerAddressLine3 = dataContractT4.Wt4oPayerAddr1;
                if (string.IsNullOrWhiteSpace(dataContractT4.Wt4oPayerAddr2))
                {
                    t4DomainEntity.EmployerAddressLine4 = (dataContractT4.Wt4oPayerCity ?? string.Empty) + ", " + (dataContractT4.Wt4oPayerProvCode ?? string.Empty) + " " + (dataContractT4.Wt4oPayerPostalCode ?? string.Empty);
                }
                else
                {
                    t4DomainEntity.EmployerAddressLine4 = dataContractT4.Wt4oPayerAddr2;
                    t4DomainEntity.EmployerAddressLine5 = (dataContractT4.Wt4oPayerCity ?? string.Empty) + ", " + (dataContractT4.Wt4oPayerProvCode ?? string.Empty) + " " + (dataContractT4.Wt4oPayerPostalCode ?? string.Empty);
                }
            }

            t4DomainEntity.TaxYear = dataContractT4.Wt4oYear;

            #endregion

            #region Box data

            var otherBoxes = dataContractT4.T4BoxInformationEntityAssociation.Where(x => x != null && x.Wt4oOtherInfoFlagsAssocMember != null && x.Wt4oOtherInfoFlagsAssocMember.ToUpperInvariant() == "Y").OrderBy(x => x.Wt4oBoxCodesAssocMember).ToList();

            // "Other" boxes - The primary slip will have all box data (including "other" boxes) but overflow slips will only have "other" box data
            for (int i = parsedSlipId * 6; i < Math.Min(otherBoxes.Count, parsedSlipId * 6 + 6); i++)
            {
                if (otherBoxes[i].Wt4oBoxCodesAssocMember != null)
                {
                    t4DomainEntity.OtherBoxes.Add(new OtherBoxData(otherBoxes[i].Wt4oBoxCodesAssocMember, (Convert.ToDecimal(otherBoxes[i].Wt4oBoxFnAmtAssocMember ?? "0") / 100m).ToString("N2")));
                }
            }

            // Only the first slip will have standard box data
            if (parsedSlipId == 0)
            {
                var standardBoxes = dataContractT4.T4BoxInformationEntityAssociation.Where(x => x != null && x.Wt4oOtherInfoFlagsAssocMember != null && x.Wt4oOtherInfoFlagsAssocMember.ToUpperInvariant() != "Y").OrderBy(x => x.Wt4oBoxCodesAssocMember).ToList();
                for (int i = 0; i < standardBoxes.Count; i++)
                {
                    var currentAssociation = standardBoxes[i];
                    if (currentAssociation != null)
                    {
                        var code = currentAssociation.Wt4oBoxCodesAssocMember;
                        var amount = (Convert.ToDecimal(currentAssociation.Wt4oBoxFnAmtAssocMember ?? "0") / 100m).ToString("N2");
                        // Employment Income - Box 14
                        if (code == "14")
                        {
                            t4DomainEntity.EmploymentIncome = amount;
                        }

                        // CPP Contributions - Box 16
                        if (code == "16")
                        {
                            t4DomainEntity.EmployeesCPPContributions = amount;
                        }

                        // QPP Contributions - Box 17
                        if (code == "17")
                        {
                            t4DomainEntity.EmployeesQPPContributions = amount;
                        }

                        // EI Premiums - Box 18
                        if (code == "18")
                        {
                            t4DomainEntity.EmployeesEIPremiums = amount;
                        }

                        // RPP Contributions - Box 20
                        if (code == "20")
                        {
                            t4DomainEntity.RPPContributions = amount;
                        }

                        // Income Tax Deducted - Box 22
                        if (code == "22")
                        {
                            t4DomainEntity.IncomeTaxDeducted = amount;
                        }

                        // EI Insurable Earnings - Box 24
                        if (code == "24")
                        {
                            t4DomainEntity.EIInsurableEarnings = amount;
                        }

                        // CPP/QPP Pensionable Earnings - Box 26
                        if (code == "26")
                        {
                            t4DomainEntity.CPPQPPPensionableEarnings = amount;
                        }

                        // Union Dues - Box 44
                        if (code == "44")
                        {
                            t4DomainEntity.UnionDues = amount;
                        }

                        // Charitable Donations - Box 46
                        if (code == "46")
                        {
                            t4DomainEntity.CharitableDonations = amount;
                        }

                        // Pension Adjustment - Box 52
                        if (code == "52")
                        {
                            t4DomainEntity.PensionAdjustment = amount;
                        }

                        // PPIP Premiums - Box 55
                        if (code == "55")
                        {
                            t4DomainEntity.EmployeesPPIPPremiums = amount;
                        }

                        // PPIP Insurable Earnings - Box 56
                        if (code == "56")
                        {
                            t4DomainEntity.PPIPInsurableEarnings = amount;
                        }
                    }
                }
            }

            #endregion

            // Call the PDF accessed CTX to send an email notification
            TxNotifyHrPdfAccessRequest pdfRequest = new TxNotifyHrPdfAccessRequest();
            pdfRequest.AFormType = "T4";
            pdfRequest.APersonId = dataContractT4.Wt4oEmployeeId;
            pdfRequest.ARecordId = dataContractT4.Recordkey;

            var pdfResponse = await transactionInvoker.ExecuteAsync<TxNotifyHrPdfAccessRequest, TxNotifyHrPdfAccessResponse>(pdfRequest);


            return t4DomainEntity;
        }

        /// <summary>
        /// Get the Defaults from CORE to get the default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private async Task<Defaults> GetDefaults()
        {
            return await GetOrAddToCacheAsync<Data.Base.DataContracts.Defaults>("CoreDefaults",
                async () =>
                {
                    Defaults coreDefaults = await DataReader.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Converts a numeric string into a string with two decimals
        /// </summary>
        /// <param name="amount">String containing numbers</param>
        /// <param name="recordId">W2 record key</param>
        /// <param name="dataContract">W2 data contract</param>
        /// <returns>String that represents an amount with dollars and cents or the original string.</returns>
        private string W2AmountStringToDecimal(string amount, string recordId, WebW2Online dataContract)
        {
            // Convert the string amount into a decimal, divide by 100, and then convert back into a
            // string that always has two decimal digits. If the incoming string cannot be converted into
            // a decimal, catch the exception and log it in the logfile and return the original string.
            string newAmount = amount;
            try
            {
                newAmount = (Convert.ToDecimal(amount) / 100).ToString("N2", CultureInfo.InvariantCulture);
            }
            catch (System.FormatException fe)
            {
                LogDataError("WebW2Online", recordId, dataContract, fe, fe.Message);
            }
            catch (System.OverflowException se)
            {
                LogDataError("WebW2Online", recordId, dataContract, se, se.Message);
            }
            return newAmount;
        }

        /// <summary>
        /// Converts a numeric string into a string with two decimals
        /// </summary>
        /// <param name="amount">String containing numbers</param>
        /// <param name="recordId">W2c record key</param>
        /// <param name="dataContract">W2c data contract</param>
        /// <returns>String that represents an amount with dollars and cents or the original string.</returns>
        private string W2cAmountStringToDecimal(string amount, string recordId, WebW2cOnline dataContract)
        {
            // Convert the string amount into a decimal, divide by 100, and then convert back into a
            // string that always has two decimal digits. If the incoming string cannot be converted into
            // a decimal, catch the exception and log it in the logfile and return the original string.
            string newAmount = amount;
            try
            {
                newAmount = (Convert.ToDecimal(amount) / 100).ToString("N2", CultureInfo.InvariantCulture);
            }
            catch (System.FormatException fe)
            {
                LogDataError("WebW2cOnline", recordId, dataContract, fe, fe.Message);
            }
            catch (System.OverflowException se)
            {
                LogDataError("WebW2cOnline", recordId, dataContract, se, se.Message);
            }
            return newAmount;
        }
    }
}
