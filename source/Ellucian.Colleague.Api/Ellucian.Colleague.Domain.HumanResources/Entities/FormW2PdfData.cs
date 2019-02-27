// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents a W-2 tax for PDF data
    /// </summary>
    [Serializable]
    public class FormW2PdfData
    {
        #region Required Attributes
      
        /// <summary>
        /// Tax year for the tax form
        /// </summary>
        public string TaxYear { get { return this.taxYear;} }
        private readonly string taxYear;

        /// <summary>
        /// Employer's tax identification number
        /// </summary>
        public string EmployerEin { get { return this.employerEin; } }
        private readonly string employerEin;

        /// <summary>
        /// Employee's social security number
        /// </summary>
        public string EmployeeSsn { get { return this.employeeSsn; } }
        private readonly string employeeSsn;

        #endregion

        #region Employer Attributes

        /// <summary>
        /// Employer name
        /// </summary>
        public string EmployerName { get; set; }

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

        #endregion

        #region Employee Attributes

        /// <summary>
        /// The person ID for the employee.
        /// </summary>
        public string EmployeeId { get; set; }

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

        /// <summary>
        /// Employee's suffix
        /// </summary>
        public string EmployeeSuffix { get; set; }

        /// <summary>
        /// Employee's full name
        /// </summary>
        /// <returns></returns>
        public string EmployeeName()
        {
            string name = EmployeeFirstName;
            if (!string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(EmployeeMiddleName))
                {
                    name = name + " " + EmployeeMiddleName.Substring(0, 1);
                }
                if (!string.IsNullOrEmpty(EmployeeLastName))
                {
                    name = name + " " + EmployeeLastName;
                }
                if (!string.IsNullOrEmpty(EmployeeSuffix))
                {
                    name = name + ", " + EmployeeSuffix;
                }
            }
            return name;
        }

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

        /// <summary>
        ///Box 1 wages, tips, other compensation
        /// </summary>
        public string FederalWages { get; set; }

        /// <summary>
        /// Box 2 federal income tax withheld
        /// </summary>
        public string FederalWithholding { get; set; }

        /// <summary>
        /// Box 3 social security wages
        /// </summary>
        public string SocialSecurityWages { get; set; }

        /// <summary>
        /// Box 4 social security tax withheld
        /// </summary>
        public string SocialSecurityWithholding { get; set; }

        /// <summary>
        /// Box 5 medicare wages and tips
        /// </summary>
        public string MedicareWages { get; set; }

        /// <summary>
        /// Box 6 medicare tax withheld
        /// </summary>
        public string MedicareWithholding { get; set; }

        /// <summary>
        /// Box 7 social security tips
        /// </summary>
        public string SocialSecurityTips { get; set; }

        /// <summary>
        /// Box 8 allocated tips
        /// </summary>
        public string AllocatedTips { get; set; }

        /// <summary>
        /// Box 9 Advanced EIC for 2010 and prior
        /// </summary>
        public string AdvancedEic { get; set; }

        /// <summary>
        /// Box 10 dependent care benefits
        /// </summary>
        public string DependentCare { get; set; }

        /// <summary>
        /// Box 11 non qualified plans
        /// </summary>
        public string NonqualifiedTotal { get; set; }

        /// <summary>
        /// Box 12a code
        /// </summary>
        public string Box12aCode { get; set; }

        /// <summary>
        /// Box 12a amount
        /// </summary>
        public string Box12aAmount { get; set; }

        /// <summary>
        /// Box 12b code
        /// </summary>
        public string Box12bCode { get; set; }

        /// <summary>
        /// Box 12b amount
        /// </summary>
        public string Box12bAmount { get; set; }

        /// <summary>
        /// Box 12c code
        /// </summary>
        public string Box12cCode { get; set; }

        /// <summary>
        /// Box 12c amount
        /// </summary>
        public string Box12cAmount { get; set; }

        /// <summary>
        /// Box 12d code
        /// </summary>
        public string Box12dCode { get; set; }

        /// <summary>
        /// Box 12d amount
        /// </summary>
        public string Box12dAmount { get; set; }

        /// <summary>
        /// Box 13 Statutory employee 
        /// </summary>
        public string Box13CheckBox1 { get; set; }
        /// <summary>
        /// Box 13 Retirement plan
        /// </summary>
        public string Box13CheckBox2 { get; set; }

        /// <summary>
        /// Box 13 Third-party sick pay
        /// </summary>
        public string Box13CheckBox3 { get; set; }

        /// <summary>
        /// Box 14 first line
        /// </summary>
        public string Box14Line1 { get; set; }

        /// <summary>
        /// Box 14 second line
        /// </summary>
        public string Box14Line2 { get; set; }

        /// <summary>
        /// Box 14 third line
        /// </summary>
        public string Box14Line3 { get; set; }

        /// <summary>
        /// Box 14 fourth line
        /// </summary>
        public string Box14Line4 { get; set; }

        /// <summary>
        /// Box 15 first line state code
        /// </summary>
        public string Box15Line1Section1 { get; set; }

        /// <summary>
        /// Box 15 second line state code
        /// </summary>
        public string Box15Line2Section1 { get; set; }

        /// <summary>
        /// Box 15 first line employer's state ID number
        /// </summary>
        public string Box15Line1Section2 { get; set; }

        /// <summary>
        /// Box 15 second line employer's state ID number
        /// </summary>
        public string Box15Line2Section2 { get; set; }

        /// <summary>
        /// Box 16 first line state wages, tips, etc
        /// </summary>
        public string Box16Line1 { get; set; }

        /// <summary>
        /// Box 16 second line state wages, tips etc 
        /// </summary>
        public string Box16Line2 { get; set; }

        /// <summary>
        /// Box 17 first line state income tax
        /// </summary>
        public string Box17Line1 { get; set; }

        /// <summary>
        /// Box 17 second line state income tax
        /// </summary>
        public string Box17Line2 { get; set; }

        /// <summary>
        /// Box 18 first line local wages, tips, etc
        /// </summary>
        public string Box18Line1 { get; set; }

        /// <summary>
        /// Box 18 second line local wages, tips, etc
        /// </summary>
        public string Box18Line2 { get; set; }

        /// <summary>
        /// Box 19 first line local income tax
        /// </summary>
        public string Box19Line1 { get; set; }

        /// <summary>
        /// Box 19 second line local income tax
        /// </summary>
        public string Box19Line2 { get; set; }

        /// <summary>
        /// Box 20 first line locality name
        /// </summary>
        public string Box20Line1 { get; set; }

        /// <summary>
        /// Box 20 second line locality name
        /// </summary>
        public string Box20Line2 { get; set; }

        #endregion

        /// <summary>
        /// Constructor to instantiate a W-2 tax form PDF for an employee
        /// </summary>
        /// <param name="taxYear">The tax year</param>
        /// <param name="employerEin">The Employer's EIN</param>
        /// <param name="employeeSsn">The employee's SSN</param>
        public FormW2PdfData(string taxYear, string employerEin, string employeeSsn)
        {
            if (string.IsNullOrEmpty(taxYear))
            {
                throw new ArgumentNullException("taxYear", "The tax year on the W-2 PDF record cannot be null.");
            }
            if (string.IsNullOrEmpty(employerEin))
            {
                throw new ArgumentNullException("employerEin", "The employer tax identification numbeer (EIN) on the W-2 PDF record cannot be null.");
            }

            this.taxYear = taxYear;
            this.employerEin = employerEin;
            this.employeeSsn = employeeSsn ?? "";
        }
    }
}
