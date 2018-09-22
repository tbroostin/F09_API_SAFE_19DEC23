// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{   
   /// <summary>
    /// Type of the financial aid fund categories.
    /// </summary>
    [Serializable]
    public enum FinancialAidFundAidCategoryType
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
                     
        /// <summary>
        /// Pell Grant
        /// </summary>
        pellGrant,

        /// <summary>
        /// Federal Unsubsidized Loan
        /// </summary>
        federalUnsubsidizedLoan,

        /// <summary>
        /// Federal Subsidized Loan
        /// </summary>
        federalSubsidizedLoan,

        /// <summary>
        /// Graduate Teaching Grant
        /// </summary>
        graduateTeachingGrant,

        /// <summary>
        /// Undergraduate Teaching Grant
        /// </summary>
        undergraduateTeachingGrant,

        /// <summary>
        /// Parent Plus Loan
        /// </summary>
        parentPlusLoan,

        /// <summary>
        /// Graduate Plus Loan
        /// </summary>
        graduatePlusLoan,

        /// <summary>
        /// Federal Work Study Program
        /// </summary>
        federalWorkStudyProgram,

        /// <summary>
        /// Irag Afghanastan Service Grant
        /// </summary>
        iraqAfghanistanServiceGrant,

        /// <summary>
        /// Academic Competitiveness Grant
        /// </summary>
        academicCompetitivenessGrant,

        /// <summary>
        /// Bureau of Indian Affairs Federal Grant
        /// </summary>
        bureauOfIndianAffairsFederalGrant,

        /// <summary>
        /// Robert C Byrd Scholarship Program
        /// </summary>
        robertCByrdScholarshipProgram,

        /// <summary>
        /// Paul Douglas Teacher Scholarship
        /// </summary>
        paulDouglasTeacherScholarship,

        /// <summary>
        /// General Title IV Loan
        /// </summary>
        generalTitleIVloan,

        /// <summary>
        /// Health Education Assistance Loan
        /// </summary>
        healthEducationAssistanceLoan,

        /// <summary>
        /// Health Professional Student Loan
        /// </summary>
        healthProfessionalStudentLoan,

        /// <summary>
        /// Income Contingent Loan
        /// </summary>
        incomeContingentLoan,

        /// <summary>
        /// Loan for Disadvantages Student
        /// </summary>
        loanForDisadvantagesStudent,

        /// <summary>
        /// Leveraging Educational Assistance Partnership
        /// </summary>
        leveragingEducationalAssistancePartnership,

        /// <summary>
        /// National Health Services Corps Scholarship
        /// </summary>
        nationalHealthServicesCorpsScholarship,

        /// <summary>
        /// Nursing Student Loan
        /// </summary>
        nursingStudentLoan,

        /// <summary>
        /// Primary Care Loan
        /// </summary>
        primaryCareLoan,

        /// <summary>
        /// Federal Perkins Loan
        /// </summary>
        federalPerkinsLoan,

        /// <summary>
        /// ROTC Scholarship
        /// </summary>
        rotcScholarship,

        /// <summary>
        /// Federal Supplementary Educational Opportunity Grant
        /// </summary>
        federalSupplementaryEducationalOpportunityGrant,

        /// <summary>
        /// Stay in School Program
        /// </summary>
        stayInSchoolProgram,

        /// <summary>
        /// Federal Supplementary Loan for Parent
        /// </summary>
        federalSupplementaryLoanForParent,

        /// <summary>
        /// National Smart Grant
        /// </summary>
        nationalSmartGrant,

        /// <summary>
        /// States Student Incentive Grant
        /// </summary>
        stateStudentIncentiveGrant,

        /// <summary>
        /// VA Health Professionals Scholarship
        /// </summary>
        vaHealthProfessionsScholarship,

        /// <summary>
        /// Non-Governmental
        /// </summary>
        nonGovernmental
    }
} 


