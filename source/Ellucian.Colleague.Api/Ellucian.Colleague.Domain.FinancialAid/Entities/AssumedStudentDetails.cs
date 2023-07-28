// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AssumedStudentDetails
    {
        /// <summary>
        /// Assumed Citizenship
        /// </summary>        
        public string Citizenship { get; set; }

        //ENUM
        /// <summary>
        /// Assumed Student Marital Status
        /// </summary>        
        public string StudentMaritalStatus { get; set; }

        /// <summary>
        /// Assumed Student AGI
        /// </summary>        
        public int? StudentAgi { get; set; }

        /// <summary>
        /// Assumed Student Taxes Paid
        /// </summary>        
        public int? StudentTaxPaid { get; set; }

        /// <summary>
        /// Assumed student income from work
        /// </summary>        
        public int? StudentWorkIncome { get; set; }

        /// <summary>
        /// Assumed Spouse Income from Work	
        /// </summary>        
        public int? SpouseWorkIncome { get; set; }

        /// <summary>
        /// Assumed Student Additional Financial Info amount
        /// </summary>        
        public int? StudentAddlFinInfoTotal { get; set; }

        /// <summary>
        /// Assumed Birth Date Prior
        /// </summary>         
        public string BirthDatePrior { get; set; }

        /// <summary>
        /// Assume the student is married/remarried.
        /// </summary>     
        public string StudentMarried { get; set; }

        /// <summary>
        /// Assumed to have children that you support.
        /// </summary>     
        public string DependentChildren { get; set; }

        /// <summary>
        /// Assumed to have legal dependents other than children or spouse.
        /// </summary>             
        public string OtherDependents { get; set; }

        /// <summary>
        /// Assumed student's number in family
        /// </summary>             
        public int? studentFamilySize { get; set; }

        /// <summary>
        /// Assumed Student's number in College
        /// </summary>             
        public int? StudentNumberInCollege { get; set; }

        /// <summary>
        /// Assumed Student's number in College
        /// </summary>             
        public bool? StudentAssetThreshold { get; set; }

        /// <summary>
        /// Assumed parent's marital status
        /// </summary>             
        public string ParentMaritalStatus { get; set; }

        /// <summary>
        /// Assumed parent 1 SSN
        /// </summary>             
        public bool? FirstParentSsn { get; set; }

        /// <summary>
        /// Assumed parent 2 SSN
        /// </summary>             
        public bool? SecondParentSsn { get; set; }

        /// <summary>
        /// Assumed Parents' Number in Family
        /// </summary>             
        public int? ParentFamilySize { get; set; }

        /// <summary>
        /// Assumed Parents' number in college
        /// </summary>             
        public int? ParentNumCollege { get; set; }

        /// <summary>
        /// Assumed Parents' Adjusted Gross Income
        /// </summary>             
        public int? ParentAgi { get; set; }

        /// <summary>
        /// Assumed Parents' U.S. Taxes Paid
        /// </summary>             
        public int? ParentTaxPaid { get; set; }

        /// <summary>
        /// Assumed Parent 1 income from work
        /// </summary>             
        public int? FirstParentWorkIncome { get; set; }

        /// <summary>
        /// Assumed Parent 2 income from work
        /// </summary>             
        public int? SecondParentWorkIncome { get; set; }

        /// <summary>
        /// Assumed Parents' Additional Financial Info Amount
        /// </summary>             
        public int? ParentAddlFinancial { get; set; }

        /// <summary>
        /// Assumed Parents' Asset Threshold Exceeded
        /// </summary>             
        public bool? ParentAssetThreshold { get; set; }

    }
}
