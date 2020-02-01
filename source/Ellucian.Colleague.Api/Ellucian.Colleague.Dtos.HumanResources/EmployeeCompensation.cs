/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class containing Employee Compensation Information.
    /// It includes Benefits-Deduction enrrolled by employee, associated tax benefits and stipend information
    /// </summary>
    public class EmployeeCompensation
    {

        /// <summary>
        /// Employee id of the user
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Information about other benefits that institution provides at no cost.
        /// </summary>
        public string OtherBenefits { get; set; }

        /// <summary>
        /// Flag that decided if employee costs(contribution) information should be available (Configured in TCSP form)
        /// </summary>
        public string DisplayEmployeeCosts { get; set; }

        /// <summary>
        /// Infomative message regarding Total compensation Page (Configured in TCSP form)
        /// </summary>
        public string TotalCompensationPageHeader { get; set; }

        /// <summary>
        /// estimated annual value of the total compensation provided by institution
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
    }

    /// <summary>
    /// Custom class containing Error details occured during Total Compensation Calculation
    /// </summary>
    public class EmployeeCompensationError
    {
        /// <summary>
        /// Defines the type of error that occured in the CTX
        /// Values are one of these -RestrictedbyRules, NoActiveEmploymentStatus,EmployeeTerminated,NoActiveWage,OtherMessage
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Custom error message associated with error code
        /// </summary>
        public string ErrorMessage { get; set; }

    }

}
   



