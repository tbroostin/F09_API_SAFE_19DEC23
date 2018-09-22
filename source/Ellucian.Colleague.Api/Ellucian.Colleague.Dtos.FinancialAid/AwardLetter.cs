//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The AwardLetter DTO provides all the data on a student's award letter.
    /// </summary>
    public class AwardLetter
    {
        /// <summary>
        /// The StudentId of the student that this award letter belongs to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The AwardYear that this award letter describes
        /// </summary>
        public string AwardYearCode { get; set; }

        /// <summary>
        /// The Description of the Award Year to be printed on the Award Letter.
        /// </summary>
        public string AwardYearDescription { get; set; }

        /// <summary>
        /// This is the date the award letter was signed and accepted by the student. 
        /// If the award letter is not accepted, this date is null.
        /// This attribute can be updated via the PUT endpoint
        /// </summary>
        public DateTime? AcceptedDate { get; set; }

        /// <summary>
        /// This is the date this award letter was generated.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Flag indicates whether the contact information should be displayed on the award letter
        /// </summary>
        public bool IsContactBlockActive { get; set; }

        /// <summary>
        /// The contact address is the address (street address, city, state, zip, phone) used in an address label.
        /// Each element in this address corresponds to a new line in the address label.
        /// </summary>
        public List<AwardLetterAddress> ContactAddress { get; set; }

        /// <summary>
        /// The student's preferred name to appear on the award letter
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// The student's address contains the address that can be used to snail-mail the award letter to the student. 
        /// Each element in this address corresponds to a new line in the address label.
        /// </summary>
        public List<AwardLetterAddress> StudentAddress { get; set; }

        /// <summary>
        /// The opening paragraph to be displayed on the award letter before the student awards
        /// </summary>
        public string OpeningParagraph { get; set; }

        /// <summary>
        /// The closing paragraph to be displayed on the award letter after the student awards
        /// </summary>
        public string ClosingParagraph { get; set; }

        /// <summary>
        /// Flag indicates whether the student's Financial Need information should be displayed on the award letter.
        /// </summary>
        public bool IsNeedBlockActive { get; set; }

        /// <summary>
        /// The student's Financial Aid Budget (Cost of school as known to FA)
        /// </summary>
        public int BudgetAmount { get; set; }

        /// <summary>
        /// The student's Estimated Family Contribution (how much the student can pay)
        /// </summary>
        public int EstimatedFamilyContributionAmount { get; set; }

        /// <summary>
        /// The student's Financial Need (how much financial assistance the student needs)
        /// </summary>
        public int NeedAmount { get; set; }

        /// <summary>
        /// Flag indicates whether the student's housing code should be displayed
        /// on the award letter
        /// </summary>
        public bool IsHousingCodeActive { get; set; }

        /// <summary>
        /// Housing code
        /// </summary>
        public HousingCode? HousingCode { get; set; }

        /// <summary>
        /// This list contains each award-specific row of the award table to appear on the award letter. The awards in the award table
        /// may not necessarily match the awards used in the students/{id}/awards/{year} endpoint, so it is preferred to use this list
        /// on the award letter. A student's financial aid office may specify in the configuration that certain awards should not be displayed on the award letter
        /// </summary>
        public List<AwardLetterAward> AwardTableRows { get; set; }

        /// <summary>
        /// The column header for the Awards column
        /// </summary>
        public string AwardColumnHeader { get; set; }

        /// <summary>
        /// The column header for the Total column
        /// </summary>
        public string TotalColumnHeader { get; set; }

        /// <summary>
        /// The column header for the first award period column
        /// </summary>
        public string AwardPeriod1ColumnHeader { get; set; }

        /// <summary>
        /// The column header for the second award period column
        /// </summary>
        public string AwardPeriod2ColumnHeader { get; set; }

        /// <summary>
        /// The column header for the third award period column
        /// </summary>
        public string AwardPeriod3ColumnHeader { get; set; }

        /// <summary>
        /// The column header for the fourth award period column
        /// </summary>
        public string AwardPeriod4ColumnHeader { get; set; }

        /// <summary>
        /// The column header for the fifth award period column
        /// </summary>
        public string AwardPeriod5ColumnHeader { get; set; }

        /// <summary>
        /// The column header for the sixth award period column
        /// </summary>
        public string AwardPeriod6ColumnHeader { get; set; }

        /// <summary>
        /// The number of award period columns to print on the report. This is used when creating
        /// the pdf report.
        /// </summary>
        public int NumberAwardPeriodColumns { get; set; }
    }
}
