// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;


namespace Ellucian.Colleague.Domain.Student.Entities
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
        PellGrant,

        /// <summary>
        /// Federal Unsubsidized Loan
        /// </summary>
        FederalUnsubsidizedLoan,

        /// <summary>
        /// Federal Subsidized Loan
        /// </summary>
        FederalSubsidizedLoan,

        /// <summary>
        /// Graduate Teaching Grant
        /// </summary>
        GraduateTeachingGrant,

        /// <summary>
        /// Undergraduate Teaching Grant
        /// </summary>
        UndergraduateTeachingGrant,

        /// <summary>
        /// Parent Plus Loan
        /// </summary>
        ParentPlusLoan,

        /// <summary>
        /// Graduate Plus Loan
        /// </summary>
        GraduatePlusLoan,

        /// <summary>
        /// Federal Work Study Program
        /// </summary>
        FederalWorkStudyProgram,

        /// <summary>
        /// Irag Afghanastan Service Grant
        /// </summary>
        IraqAfghanistanServiceGrant,

        /// <summary>
        /// Academic Competitiveness Grant
        /// </summary>
        AcademicCompetitivenessGrant,

        /// <summary>
        /// Bureau of Indian Affairs Federal Grant
        /// </summary>
        BureauOfIndianAffairsFederalGrant,

        /// <summary>
        /// Robert C Byrd Scholarship Program
        /// </summary>
        RobertCByrdScholarshipProgram,

        /// <summary>
        /// Paul Douglas Teacher Scholarship
        /// </summary>
        PaulDouglasTeacherScholarship,

        /// <summary>
        /// General Title IV Loan
        /// </summary>
        GeneralTitleIVloan,

        /// <summary>
        /// Health Education Assistance Loan
        /// </summary>
        HealthEducationAssistanceLoan,

        /// <summary>
        /// Health Professional Student Loan
        /// </summary>
        HealthProfessionalStudentLoan,

        /// <summary>
        /// Income Contingent Loan
        /// </summary>
        IncomeContingentLoan,

        /// <summary>
        /// Loan for Disadvantages Student
        /// </summary>
        LoanForDisadvantagesStudent,

        /// <summary>
        /// Leveraging Educational Assistance Partnership
        /// </summary>
        LeveragingEducationalAssistancePartnership,

        /// <summary>
        /// National Health Services Corps Scholarship
        /// </summary>
        NationalHealthServicesCorpsScholarship,

        /// <summary>
        /// Nursing Student Loan
        /// </summary>
        NursingStudentLoan,

        /// <summary>
        /// Primary Care Loan
        /// </summary>
        PrimaryCareLoan,

        /// <summary>
        /// Federal Perkins Loan
        /// </summary>
        FederalPerkinsLoan,

        /// <summary>
        /// ROTC Scholarship
        /// </summary>
        RotcScholarship,

        /// <summary>
        /// Federal Supplementary Educational Opportunity Grant
        /// </summary>
        FederalSupplementaryEducationalOpportunityGrant,

        /// <summary>
        /// Stay in School Program
        /// </summary>
        StayInSchoolProgram,

        /// <summary>
        /// Federal Supplementary Loan for Parent
        /// </summary>
        FederalSupplementaryLoanForParent,

        /// <summary>
        /// National Smart Grant
        /// </summary>
        NationalSmartGrant,

        /// <summary>
        /// States Student Incentive Grant
        /// </summary>
        StateStudentIncentiveGrant,

        /// <summary>
        /// VA Health Professionals Scholarship
        /// </summary>
        VaHealthProfessionsScholarship,

        /// <summary>
        /// Non-Governmental
        /// </summary>
        NonGovernmental
    }
} 


