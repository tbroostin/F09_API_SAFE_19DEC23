//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PersonBeneficiary
    /// </summary>
    [Serializable]
    public class PersonBeneficiary
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The deduction arrangement associated with the benefit.
        /// </summary>
        public string DeductionArrangement { get { return deductionArrangement; } }
        private readonly string deductionArrangement;

        /// <summary>
        /// To identify a record in the PERSON file as an organization
        /// </summary>
        public string PersonCorpIndicator { get; set; }

        /// <summary>
        /// The global identifier for the Person/Organization/Insitution
        /// </summary>
        public string PerbenBeneficiaryId { get; set; }

        /// <summary>
        /// The name of the person or organization who is designated to receive the financial benefit.
        /// </summary>
        public string PerbenOrgBeneficiary { get; set; }

        /// <summary>
        /// Indicator that person-beneifciary is for a person.
        /// </summary>
        public bool Person { get; set; }

        /// <summary>
        /// Indicator that person-beneifciary is for a organization.
        /// </summary>
        public bool Organization { get; set; }

        /// <summary>
        /// Indicator that person-beneifciary is for a institution.
        /// </summary>
        public bool Institution { get; set; }

        /// <summary>
        /// The global identifier for the Preference.
        /// </summary>
        public string PerbenBeneficiaryType { get; set; }

        /// <summary>
        /// The global identifier for the Preference.
        /// </summary>
        public string PerbenOrgBfcyType { get; set; }

        /// <summary>
        /// The percentage of the allocation of the benefit to the beneficiary.
        /// </summary>
        public decimal? PerbenBfcyDesgntnPct { get; set; }

        /// <summary>
        /// The percentage of the allocation of the benefit to the beneficiary.
        /// </summary>
        public decimal? PerbenOrgBfcyDesgntnPct { get; set; }

        /// <summary>
        /// The first date when a beneficiary is applicable to a person's benefit.
        /// </summary>
        public DateTime? PerbenBfcyStartDate { get; set; }

        /// <summary>
        /// The first date when a beneficiary is applicable to a person's benefit.
        /// </summary>
        public DateTime? PerbenOrgStartDate { get; set; }

        /// <summary>
        /// The last date when a beneficiary is applicable to a person's benefit.
        /// </summary>
        public DateTime? PerbenBfcyEndDate { get; set; }

        /// <summary>
        /// The last date when a beneficiary is applicable to a person's benefit.
        /// </summary>
        public DateTime? PerbenOrgEndDate { get; set; }

		/// <summary>
        /// PersonBeneficiary constructor
        /// </summary>
        /// <param name="id"></param>
        public PersonBeneficiary(string id, string deductionArrangement)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrWhiteSpace(deductionArrangement))
            {
                throw new ArgumentNullException("deductionArrangement");
            }

            this.id = id;
            this.deductionArrangement = deductionArrangement;
        }

        /// <summary>
        /// Two pay cycles are equal when their ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payCycle = obj as PayCycle;
            return payCycle.Id == this.Id;
        }

        /// <summary>
        /// Hashcode representation of PayCycle (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}