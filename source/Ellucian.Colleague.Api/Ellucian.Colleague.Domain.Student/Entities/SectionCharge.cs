// Copyright 2017 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Financial charge associated with a course section
    /// </summary>
    [Serializable]
    public class SectionCharge
    {
        private string _id;
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Id { get { return _id; } }

        private string _chargeCode;
        /// <summary>
        /// Financial charge code
        /// </summary>
        public string ChargeCode { get { return _chargeCode; } }

        private decimal _baseAmount;
        /// <summary>
        /// Base charge amount
        /// </summary>
        public decimal BaseAmount { get { return _baseAmount; } }

        private bool _isFlatFee;
        /// <summary>
        /// Flag indicating whether or not this charge is a flat fee
        /// </summary>
        public bool IsFlatFee { get { return _isFlatFee; } }

        private bool _isRuleBased;
        /// <summary>
        /// Flag indicating whether or not this charge is applicable based on a rule
        /// </summary>
        public bool IsRuleBased { get { return _isRuleBased; } }

        /// <summary>
        /// Creates a new <see cref="SectionCharge"/> object
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="chargeCode">Financial charge code</param>
        /// <param name="baseAmount">Charge amount</param>
        /// <param name="isFlatFee">Flag indicating whether or not this charge is a flat fee</param>
        /// <param name="isRuleBased">Flag indicating whether or not this charge is applicable based on a rule</param>
        public SectionCharge(string id, string chargeCode, decimal baseAmount, bool isFlatFee, bool isRuleBased)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id", "A section charge must have an ID.");
            if (string.IsNullOrEmpty(chargeCode)) throw new ArgumentNullException("chargeCode", "A section charge must have a charge code.");

            _id = id;
            _chargeCode = chargeCode;
            _baseAmount = baseAmount;
            _isFlatFee = isFlatFee;
            _isRuleBased = isRuleBased;
        }

        /// <summary>
        /// Compares equality of two <see cref="SectionCharge"/> objects
        /// </summary>
        /// <param name="obj"><see cref="SectionCharge"/> to compare</param>
        /// <returns>Boolean indicating equality or inequality</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SectionCharge other = obj as SectionCharge;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        /// <summary>
        /// Retrieves the hash code for the <see cref="SectionCharge"/> object
        /// </summary>
        /// <returns>The hash code for the <see cref="SectionCharge"/> object</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
