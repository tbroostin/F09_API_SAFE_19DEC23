//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The AwardLetter DTO provides all the data on a student's award letter.
    /// This record is generated from data stored in the AwardLetterHistory file in Colleague.
    /// </summary>
    public class AwardLetter3
    {
        /// <summary>
        /// The key to the AwardLetterHistory used to create this AwardLetter
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The StudentId of the student that this award letter belongs to
        /// </summary>
        public string StudentId { get; set; }

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
        /// The AwardLetterYear that this award letter describes
        /// </summary>
        public string AwardLetterYear { get; set; }

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
        /// This is the date this award letter was created.
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// The parameter id used to generate this award letter
        /// </summary>
        public string AwardLetterParameterId { get; set; }

        /// <summary>
        /// The Contact name is the name used in an address label
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// The contact address is the address (street address, city, state, zip, phone) used in an address label.
        /// Each element in this address corresponds to a new line in the address label.
        /// </summary>
        public List<AwardLetterAddress> ContactAddress { get; set; }

        /// <summary>
        /// The opening paragraph to be displayed on the award letter before the student awards
        /// </summary>
        public string OpeningParagraph { get; set; }

        /// <summary>
        /// The closing paragraph to be displayed on the award letter after the student awards
        /// </summary>
        public string ClosingParagraph { get; set; }

        /// <summary>
        /// The student's Financial Aid Budget (Cost of school as known to FA)
        /// </summary>
        public int BudgetAmount { get; set; }

        /// <summary>
        /// The student's Estimated Family Contribution (how much the student can pay)
        /// </summary>
        public int? EstimatedFamilyContributionAmount { get; set; }

        /// <summary>
        /// The student's Financial Need (how much financial assistance the student needs)
        /// </summary>
        public int NeedAmount { get; set; }

        /// <summary>
        /// Housing code
        /// </summary>
        public HousingCode? HousingCode { get; set; }

        /// <summary>
        /// The FA Office assigned to this student when the Award Letter was created
        /// </summary>
        public string StudentOfficeCode { get; set; }

        /// <summary>
        /// List of the awards at an annual level that constitue the award letter.
        /// </summary>
        public List<AwardLetterAnnualAward> AwardLetterAnnualAwards { get; set; }

        /// <summary>
        /// List of the Groups used for this award letter
        /// </summary>
        public List<AwardLetterGroup> AwardLetterGroups { get; set; }
    }
}
