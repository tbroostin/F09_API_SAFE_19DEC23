// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Attributes of any requirement, subrequirement, or group.
    /// </summary>
    [Serializable]
    public abstract class BlockBase
    {

        /// <summary>
        /// This is the block id from ACAD.REQMT.BLOCKS
        /// </summary>
        private string _Id { get; set; }
        public string Id   { get { return _Id; }
                             set { _Id = value; }
        }

        /// <summary>
        /// For a Requirement, this is the ACAD.REQMTS key, like "HIST.CORE"
        /// For a SubRequirement or Group, this is the ACRB.LABEL field. 
        /// For a SubRequirement this is a similar looking code, like HIST.ANCIENT
        /// or HIST.MODERN.  For Groups this is often generated and just says "Group 1"
        /// or "Group 2."
        /// </summary>
 
        private string _Code { get; set; }
        public string Code { get { return _Code;}
                             set { _Code = value; }
        }

        /// <summary>
        /// English-language description of the block 
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Set if requirement block is waived or replaced by Colleague exception
        /// </summary>
        public bool IsWaived { get; set; }

        /// <summary>
        /// Set if requirement block is modified by Colleague exception
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Set if requirement block is modified by Colleague exception
        /// </summary>
        public List<string> ModificationMessages { get; set; }



        /// <summary>
        /// "EXTRA_PARAM value" on group or set on requirement/subrequirement
        /// </summary>
        public ExtraCourses ExtraCourseDirective { get; set; }

        /// <summary>
        /// "GRADE_PARAM Y|N" on group or set on requirement/subrequirement 
        /// </summary>
        public bool IncludeLowGradesInGpa { get; set; }

        /// <summary>
        /// "MINIMUM n INST.HOURS" on group or set on requirement/subrequirement
        /// </summary>
        public decimal? MinInstitutionalCredits { get; set; }

        /// <summary>
        /// "MIN Gpa n" on group or set on requirement/subrequirement
        /// </summary>
        public decimal? MinGpa { get; set; }

        /// <summary>
        /// "MININMUM GRADE OF value"
        /// </summary>
        public Grade MinGrade { get; set; }

        /// <summary>
        /// "MINIMUM GRADE OF value,allowed1,allowed2,.."
        /// </summary>
        public List<Grade> AllowedGrades { get; set; }

        // ACRB.TYPE
        public string InternalType { get; set; }

        /// <summary>
        /// Limit academic credit that can be applied. List may contain rules based on Course or AcademicCredit objects.
        /// </summary>
        public List<RequirementRule> AcademicCreditRules { get; set; }

        /// <summary>
        /// Boolean indicates if there are any rules based on AcademicCredit objects in the AcademicCreditRules collection.
        /// </summary>
        public bool HasAcademicCreditBasedRules
        {
            get 
            {
                return AcademicCreditRules != null && AcademicCreditRules.Where(r => r.CreditRule != null).Count() > 0;
            }
        }

        protected BlockBase(string id, string code)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            _Id = id;
            _Code = code;
            AcademicCreditRules = new List<RequirementRule>();
            AllowedGrades = new List<Grade>();
            IsWaived = false;
            IsModified = false;
            ModificationMessages = new List<string>();
            IncludeLowGradesInGpa = true;
        }

        public virtual List<RequirementRule> GetRules()
        {
            List<RequirementRule> blkrules = new List<RequirementRule>();

            if (AcademicCreditRules != null)
            {
                foreach (var rule in AcademicCreditRules)
                {
                    blkrules.Add(rule);
                }
            }
            return blkrules;
        }

        public override string ToString()
        {
            return _Code;
        }

        public List<RequirementRule> CourseBasedRules
        {
            get
            {
                return AcademicCreditRules == null ? new List<RequirementRule>() : AcademicCreditRules.Where(r => r.CourseRule != null).ToList();
            }
        }

        // ID to a sort specification that determines the order in which courses are matched to this 
        // requirement/subrequirement/group when evaluating an individual's progess
        public string SortSpecificationId { get; set; }

    }

    [Serializable]
    public enum ExtraCourses
    {
        Ignore,
        Display,
        SemiApply,
        Apply /* DEFAULT DEFAULT */
    }

}
