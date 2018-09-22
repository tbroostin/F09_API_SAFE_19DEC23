/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Specific types of Financial Aid applications (FAFSA, PROFILE) share these attributes and should
    /// extend this class
    /// </summary>
    [Serializable]
    public abstract class FinancialAidApplication2
    {
        /// <summary>
        /// The database Id of this application
        /// </summary>
        public string Id { get { return _Id; } }
        private readonly string _Id;

        /// <summary>
        /// The year for the application
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Colleague PERSON Id this application applies to
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The federally calculated family contribution. Should only be considered
        /// if this application is federally flagged.
        /// </summary>
        public int? FamilyContribution { get; set; }

        /// <summary>
        /// The institutionally calculated family contribution. Should only be considered
        /// if this application is institutionally flagged.
        /// </summary>
        public int? InstitutionalFamilyContribution { get; set; }

        /// <summary>
        /// Indicates whether this application is the federally flagged application
        /// </summary>
        public bool IsFederallyFlagged { get; set; }

        /// <summary>
        /// Indicates whether this application is the institutionally flagged application
        /// </summary>
        public bool IsInstitutionallyFlagged { get; set; }

        /// <summary>
        /// Constructor to build a FinancialAidApplication.
        /// </summary>
        /// <param name="id">Must supply a unique identifier for this application. We expect this to be the database record id. Not supporting new 
        /// Applications yet, so this argument is required.</param>
        /// <param name="awardYear">The award year this application applies to</param>
        /// <param name="studentId">The Colleague PERSON id of the student this application belongs to</param>
        protected FinancialAidApplication2(string id, string awardYear, string studentId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            _Id = id;
            _AwardYear = awardYear;
            _StudentId = studentId;
        }

        /// <summary>
        /// Two Applications are equal when they share the same identifier and they 
        /// are the same type. 
        /// Example: Two Fafsa objects are equal if their ids are equal.
        /// Example: A Fafsa object does not equal a ProfileApplication object, even if their ids are equal.
        /// </summary>
        /// <param name="obj">The application to compare to this application</param>
        /// <returns>True if the applications are equal. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var financialAidApplication = obj as FinancialAidApplication2;

            if (financialAidApplication.Id == this.Id)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the HashCode of this application based on the application identifier.
        /// </summary>
        /// <returns>The HashCode of this application</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Gets a string representation of this application based on the application's identifier, award year and studentId.
        /// </summary>
        /// <returns>A string representation of this application</returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}*{2}", Id, AwardYear, StudentId);
        }
    }
}
