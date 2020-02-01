/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base;
using System;

namespace Ellucian.Colleague.Domain.HumanResources
{
    [Serializable]
    public static class HumanResourcesPermissionCodes
    {
        /// <summary>
        /// Enables a user to view another employee's W2 information (ie: Tax Information Admin)
        /// </summary>
        public const string ViewEmployeeW2 = BasePermissionCodes.ViewEmployeeW2;

        /// <summary>
        /// Enables a user to view another employee's 1095C information (ie: Tax Information Admin)
        /// </summary>
        public const string ViewEmployee1095C = BasePermissionCodes.ViewEmployee1095C;

        /// <summary>
        /// Enables a user to view another employee's T4 information (ie: Tax Information Admin)
        /// </summary>
        public const string ViewEmployeeT4 = BasePermissionCodes.ViewEmployeeT4;

        /// <summary>
        /// Enables a user to view their own W2 information
        /// </summary>
        public const string ViewW2 = BasePermissionCodes.ViewW2;

        /// <summary>
        /// Enables a user to view their own 1095C information
        /// </summary>
        public const string View1095C = BasePermissionCodes.View1095C;

        /// <summary>
        /// Enables a user to view their own T4 information
        /// </summary>
        public const string ViewT4 = BasePermissionCodes.ViewT4;

        // Enables an employee to create, update, or delete payroll banking information 
        public const string EditPayrollBankingInformation = "EDIT.PAYROLL.BANKING.INFORMATION";

        /// <summary>
        /// In terms of Human Resources, this permission enables supervisers to view data for their supervisees.
        /// Enables a supervisor to approve and reject timecards over in the Time Management module, 
        /// </summary>
        public const string ViewSuperviseeData = "APPROVE.REJECT.TIME.ENTRY";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view data for employees. 
        /// </summary>
        public const string ViewEmployeeData = "VIEW.EMPLOYEE.DATA";

        /// <summary>
        /// Enables a user to view employee earning statement summaries or content
        /// </summary>
        public const string ViewAllEarningsStatements = "VIEW.ALL.EARNINGS.STATEMENTS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view institution position data. 
        /// </summary>
        public const string ViewInstitutionPosition = "VIEW.INSTITUTION.POSITION";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to create/update institution jobs. 
        /// </summary>
        public const string CreateInstitutionJob = "CREATE.UPDATE.INSTITUTION.JOB";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view institution jobs data. 
        /// </summary>
        public const string ViewInstitutionJob = "VIEW.INSTITUTION.JOB";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view institution job supervisors data. 
        /// </summary>
        public const string ViewInstitutionJobSupervisor = "VIEW.INSTITUTION.JOB.SUPERVISORS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view employee leave plans information. 
        /// </summary>
        public const string ViewEmployeeLeavePlans = "VIEW.EMP.LEAVE.PLANS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view employee leave transactions information. 
        /// </summary>
        public const string ViewEmployeeLeaveTransactions = "VIEW.EMPL.LEAVE.TRANSACTIONS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to create payroll deduction arrangements data. 
        /// </summary>
        public const string CreatePayrollDeductionArrangements = "CREATE.PAYROLL.DEDUCTION.ARRANGEMENTS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view contribution-payroll-deductions. 
        /// </summary>
        public const string ViewContributionPayrollDeductions = "VIEW.CONTR.PAYROLL.DEDUCTIONS";

        // Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague. 
        public const string ViewEmploymentPerformanceReview = "VIEW.EMPL.PERF.REVIEWS";

        // Provides an integration user permission to create/update holds (a.k.a. restrictions) from Colleague. 
        public const string CreateUpdateEmploymentPerformanceReview = "UPDATE.EMPL.PERF.REVIEWS";

        // Provides an integration user permission to delete a hold (a.k.a. a record from STUDENT.RESTRICTIONS) in Colleague.
        public const string DeleteEmploymentPerformanceReview = "DELETE.EMPL.PERF.REVIEWS";

        // Provides an integration user permission to update an employee in Colleague.
        public const string UpdateEmployee = "UPDATE.EMPLOYEE";

        // Provides an integration user permission to view pay scales in Colleague.
        public const string ViewPayScales = "VIEW.PAY.SCALES";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view job applications data. 
        /// </summary>
        public const string ViewJobApplications = "VIEW.JOB.APPLICATIONS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view person benefit dependents data. 
        /// </summary>
        public const string ViewDependents = "VIEW.DEPENDENTS";

        /// <summary>
        /// In terms of Human Resources, this permission enables users to view person employment proficiencies data. 
        /// </summary>
        public const string ViewPersonEmpProficiencies = "VIEW.PERSON.EMPL.PROFICIENCIES";

        /// <summary>
        /// In terms of Human Resources, this permission enables users(admins) to view any employee's paid time card history. 
        /// </summary>
        public const string ViewAllTimeHistory = "VIEW.ALL.TIME.HISTORY";

        /// <summary>
        /// In terms of Human Resources, this permission enables users(admins) to view any employee's total compensation statement. 
        /// </summary>
        public const string ViewAllTotalCompensation = "VIEW.ALL.TOTAL.COMPENSATION";
    }
}
