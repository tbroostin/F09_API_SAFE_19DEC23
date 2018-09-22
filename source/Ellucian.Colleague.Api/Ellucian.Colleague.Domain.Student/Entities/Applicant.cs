// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Applicant class
    /// </summary>
    [Serializable]
    public class Applicant : Person
    {

        /// <summary>
        /// Id of the student's Financial Aid counselor. If empty or null, the applicant has not been
        /// assigned a counselor yet.
        /// </summary>
        public string FinancialAidCounselorId { get; set; }

        /// <summary>
        /// Constructor creates an Applicant object
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <param name="lastName">Applicant's last name</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public Applicant(string applicantId, string lastName, string privacyStatusCode = null)
            : base(applicantId, lastName, privacyStatusCode)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            Applicant other = obj as Applicant;

            return other.Id.Equals(this.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
