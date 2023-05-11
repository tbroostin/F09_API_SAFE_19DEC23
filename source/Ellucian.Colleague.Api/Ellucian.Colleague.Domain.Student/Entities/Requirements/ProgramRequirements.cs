// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class ProgramRequirements
    {
        /// <summary>
        /// The identifying code for this program such as ENGL.BA
        /// </summary>
        private string _ProgramCode;
        public string ProgramCode
        {
            get { return _ProgramCode; }
            set
            {
                if (value == null)
                {
                    throw new NotSupportedException("Cannot set Program to null");
                }
                else
                {
                    _ProgramCode = value;
                }
            }
        }

        /// <summary>
        /// The Catalog code for this set of requirements
        /// </summary>
        private string _CatalogCode;
        public string CatalogCode { get { return _CatalogCode; } }
        /// <summary>
        /// Minimum number of overall credits required for program completion
        /// </summary>
        public decimal? MinimumCredits { get; set; }
        /// <summary>
        /// Minimum number of institutional credits required for program completion
        /// </summary>
        public decimal? MinimumInstitutionalCredits { get; set; }
        /// <summary>
        /// Minimum overall GPA required for program completion
        /// </summary>
        public decimal? MinOverallGpa { get; set; }
        /// <summary>
        /// Minimum institution GPA required for program completion
        /// </summary>
        public decimal? MinInstGpa { get; set; }
        /// <summary>
        /// Lowest allowable grade for academic credits to be applied and/or included in the GPA calculation
        /// </summary>
        public Grade MinGrade { get; set; }
        /// <summary>
        /// List of additional grades (with a lower value than above or with no value) that may be applied
        /// </summary>
        public List<Grade> AllowedGrades { get; set; }
        /// <summary>
        /// Rules used to filter out academic credits or courses from being applied
        /// </summary>
        public List<RequirementRule> ActivityEligibilityRules { get; set; }
        /// <summary>
        /// Id of the curriculum track defined as the sample degree plan
        /// </summary>
        public string CurriculumTrackCode { get; set; }
        /// <summary>
        /// The maximum number of credits allowed for program completion. Maximum Credits
        /// are used by Financial Aid Academic Progress Evaluations
        /// </summary>
        public decimal? MaximumCredits { get; set; }
        
        /// <summary>
        /// The requirements for this program, such as GENED.HIST, GENED.HUM. This is the top level of the requirement hierarchy.
        /// Each requirement contains a list of sub-requirements, and each subrequirement contains a list of groups. The groups
        /// contains the specifications for the courses and academic credits that can be applied to the program.
        /// </summary>
        public List<Requirement> Requirements { get; set; }

        /// <summary>
        /// Returns a list of activity eligibility rules that are course-based. Useful for catalog search from group requirements.
        /// </summary>
        public List<RequirementRule> CourseBasedRules
        {
            get
            {
                return ActivityEligibilityRules == null ? new List<RequirementRule>() : ActivityEligibilityRules.Where(r => r.CourseRule != null).ToList();
            }
        }

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
        /// Constructor for a ProgramRequirements object. Consists of a two-part identifier, Program*Catalog
        /// </summary>
        /// <param name="programCode">The Program to which these requirements pertain</param>
        /// <param name="catalogCode">The specific Catalog of these requirements for the given Program</param>
        public ProgramRequirements(string programCode, string catalogCode)
        {
            Requirements = new List<Requirement>();
            ActivityEligibilityRules = new List<RequirementRule>();

            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode");
            }
            if (string.IsNullOrEmpty(catalogCode))
            {
                throw new ArgumentNullException("catalogCode");
            }
            _ProgramCode = programCode;
            _CatalogCode = catalogCode;
        }

        /// <summary>
        /// Returns a list of Activity Eligibility Rules defined throughout the program requirements, all the way down to the group level.
        /// Used to evaluate all rules against all academic credits and planned courses in one batch before program evaluation.
        /// </summary>
        /// <returns></returns>
        public List<RequirementRule> GetAllRules()
        {
            List<RequirementRule> allrules = new List<RequirementRule>();

            if (ActivityEligibilityRules != null)
            {
                foreach (var aer in ActivityEligibilityRules)
                {
                    allrules.Add(aer);
                }
            }

            if (Requirements != null)
            {
                foreach (var req in Requirements)
                {
                    allrules.AddRange(req.GetAllRules());
                }
            }
 
            return allrules;
        }

        /// <summary>
        /// String representation of program requirements used for debugging
        /// </summary>
        /// <returns>program*code name of these requirements</returns>
        public override string ToString()
        {
            return _ProgramCode + "*" + _CatalogCode;
        }

    }
}
