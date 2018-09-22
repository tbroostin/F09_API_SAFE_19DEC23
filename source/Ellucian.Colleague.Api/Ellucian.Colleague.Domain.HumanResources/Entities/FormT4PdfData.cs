// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class FormT4PdfData
    {
        public FormT4PdfData()
        {
            this.OtherBoxes = new List<OtherBoxData>();
        }

        public string ProvinceOfEmployment { get; set; }

        public string EmploymentCode { get; set; }

        public string ExemptCPPQPP { get; set; }

        public string ExemptEI { get; set; }

        public string ExemptPPIP { get; set; }

        public string SocialInsuranceNumber { get; set; }

        public string EmploymentIncome { get; set; }

        public string EmployeesCPPContributions { get; set; }

        public string EmployeesQPPContributions { get; set; }

        public string EmployeesEIPremiums { get; set; }

        public string RPPContributions { get; set; }

        public string PensionAdjustment { get; set; }

        public string EmployeesPPIPPremiums { get; set; }

        public string IncomeTaxDeducted { get; set; }

        public string EIInsurableEarnings { get; set; }

        public string CPPQPPPensionableEarnings { get; set; }

        public string UnionDues { get; set; }

        public string CharitableDonations { get; set; }

        public string RPPorDPSPRegistrationNumber { get; set; }

        public string PPIPInsurableEarnings { get; set; }

        public List<OtherBoxData> OtherBoxes { get; set; }

        public string TaxYear { get; set; }

        #region Employer Attributes

        /// <summary>
        /// Employer's first address line
        /// </summary>
        public string EmployerAddressLine1 { get; set; }

        /// <summary>
        /// Employer's second address line
        /// </summary>
        public string EmployerAddressLine2 { get; set; }


        /// <summary>
        /// Employer's third address line
        /// </summary>
        public string EmployerAddressLine3 { get; set; }

        /// <summary>
        /// Employer's fourth address line
        /// </summary>
        public string EmployerAddressLine4 { get; set; }

        /// <summary>
        /// Employer's fifth address line
        /// </summary>
        public string EmployerAddressLine5 { get; set; }

        #endregion

        #region Employee Attributes

        /// <summary>
        /// Employee's first name
        /// </summary>
        public string EmployeeFirstName { get; set; }

        /// <summary>
        /// Employee's middle name
        /// </summary>
        public string EmployeeMiddleName { get; set; }

        /// <summary>
        /// Employee's last name
        /// </summary>
        public string EmployeeLastName { get; set; }

        // Only 4 lines of employee address are supported.

        /// <summary>
        /// Employee's first address line
        /// </summary>
        public string EmployeeAddressLine1 { get; set; }

        /// <summary>
        /// Employee's second address line
        /// </summary>
        public string EmployeeAddressLine2 { get; set; }

        /// <summary>
        /// Employee's third address line
        /// </summary>
        public string EmployeeAddressLine3 { get; set; }

        /// <summary>
        /// Employee's fourth address line
        /// </summary>
        public string EmployeeAddressLine4 { get; set; }

        #endregion
    }

    [Serializable]
    public class OtherBoxData
    {
        public OtherBoxData(string code, string amount)
        {
            this.Code = code;
            this.Amount = amount;
        }

        public string Code { get; set; }

        public string Amount { get; set; }
    }
}
