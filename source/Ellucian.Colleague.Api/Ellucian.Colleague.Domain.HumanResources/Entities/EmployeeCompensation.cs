/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class containing Employee Compensation Information.
    /// It includes Benefits- Deduction enrrolled by employee,associated tax benefits and stipend information
    /// </summary>
    [Serializable]
    public class EmployeeCompensation
    {
        /// <summary>
        /// employee id of the user
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// Information about other benefits that institution provides at no cost.
        /// </summary>
        public string OtherBenefits { get; set; }
        /// <summary>
        /// Flag that decided if employee costs(contribution) information should be available
        /// </summary>
        public string DisplayEmployeeCosts { get; set; }
        /// <summary>
        /// Infomative message regarding Total compensation Page
        /// </summary>
        public string TotalCompensationPageHeader { get; set; }
        /// <summary>
        ///  estimated annual value of the total compensation provided by institution
        /// </summary>
        public decimal? SalaryAmount { get; set; }
        /// <summary>
        /// List of Benefits-Deduction enrolled by an employee(if any)
        /// </summary>
        public List<EmployeeBended> Bended { get; set; }
        /// <summary>
        /// List of Tax Benefits associated with an employee(if any)
        /// </summary>
        public List<EmployeeTax> Taxes { get; set; }
        /// <summary>
        /// List of stipends associated with an employee(if any)
        /// </summary>
        public List<EmployeeStipend> Stipends { get; set; }

        /// <summary>
        /// Contains Error code and message returned by CTX
        /// </summary>
        public EmployeeCompensationError EmployeeCompensationError { get; set; }

        public EmployeeCompensation(string effectivePersonId, string otherBenefits, string displayEmployeeCosts, string totalCompPageHeader, decimal? salaryAmount, List<EmployeeBended> lstEmpBended, List<EmployeeTax> lstEmpTaxes, List<EmployeeStipend> lstEmpStipends)
        {
            if (string.IsNullOrEmpty(effectivePersonId))
                throw new ArgumentNullException("PersonId cannot be blank");

            this.PersonId = effectivePersonId;
            this.OtherBenefits = otherBenefits;
            this.DisplayEmployeeCosts = displayEmployeeCosts;
            this.TotalCompensationPageHeader = totalCompPageHeader;
            this.SalaryAmount = salaryAmount;
            this.Bended = lstEmpBended;
            this.Taxes = lstEmpTaxes;
            this.Stipends = lstEmpStipends;

        }

        public EmployeeCompensation(string effectivePersonId, string errorCode, string errorMessage)
        {
            if (this.EmployeeCompensationError == null)
                this.EmployeeCompensationError = new EmployeeCompensationError();
            this.PersonId = effectivePersonId;
            this.EmployeeCompensationError.ErrorCode = errorCode;
            this.EmployeeCompensationError.ErrorMessage = errorMessage;
        }

    }

    /// <summary>
    /// Custom class containing Error details occured during Total Compensation Calculation
    /// </summary>
    [Serializable]
    public class EmployeeCompensationError
    {
        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

    }
}
