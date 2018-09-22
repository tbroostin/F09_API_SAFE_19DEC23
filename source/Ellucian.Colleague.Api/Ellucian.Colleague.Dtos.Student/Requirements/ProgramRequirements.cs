// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// The requirements and rules that must be evaluated against coursework to determine if
    /// a student has completed a Program, or what remains to be done.
    /// </summary>
    public class ProgramRequirements
    {
        /// <summary>
        /// The code (unique Id) of the academic program
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// The code (unique Id) of the catalog
        /// </summary>
        public string CatalogCode { get; set; }
        /// <summary>
        /// Minimum number of credits required
        /// </summary>
        public decimal MinimumCredits { get; set; }
        /// <summary>
        /// The maximum number of credits allowed for program completion. Maximum Credits
        /// are used by Financial Aid Academic Progress Evaluations
        /// </summary>
        public decimal? MaximumCredits { get; set; }
        /// <summary>
        /// Minimum number of institutional credits required
        /// </summary>
        public decimal MinimumInstitutionalCredits { get; set; }
        /// <summary>
        /// Minimum required overall GPA.
        /// </summary>
        public decimal MinOverallGpa { get; set; }
        /// <summary>
        /// Minimum required institutional GPA.
        /// </summary>
        public decimal MinInstGpa { get; set; }
        /// <summary>
        /// Grade scheme used for calculating GPA and credits
        /// </summary>
        public string GradeSchemeCode { get; set; }
        /// <summary>
        /// Body of requirements, subrequirements and groups that must be satisfied to complete this program
        /// <see cref="Requirement"/>
        /// </summary>
        public List<Requirement> Requirements { get; set; }

        /// <summary>
        /// Insertion point for additional CCDs when generating program evaluations
        /// </summary>
        public string RequirementToPrintCcdsAfter { get; set; }
        /// <summary>
        /// Insertion point for additional majors when generating program evaluations
        /// </summary>
        public string RequirementToPrintMajorsAfter { get; set; }
        /// <summary>
        /// Insertion point for additional minors when generating program evaluations
        /// </summary>
        public string RequirementToPrintMinorsAfter { get; set; }
        /// <summary>
        /// Insertion point for additional specializations when generating program evaluations
        /// </summary>
        public string RequirementToPrintSpecializationsAfter { get; set; }

        /// <summary>
        /// Override method for ToString 
        /// </summary>
        /// <returns>Program code followed by an asterisk followed by the catalog year</returns>
        public override string ToString()
        {
            return ProgramCode + "*" + CatalogCode;
        }
    }
}
