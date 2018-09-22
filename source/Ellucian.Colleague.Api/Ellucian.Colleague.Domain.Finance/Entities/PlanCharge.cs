// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A charge assigned to a payment plan
    /// </summary>
    [Serializable]
    public class PlanCharge
    {
        private string _planId;
        private Charge _charge;
        private decimal _amount;
        private bool _isSetupCharge;
        private bool _isAutomaticallyModifiable;

        /// <summary>
        /// ID of the payment plan charge
        /// </summary>
        public string Id { get { return (string.IsNullOrEmpty(PlanId)) ? null : PlanId + "*" + Charge.Id; } }

        /// <summary>
        /// ID of the payment plan to which the plan charge belongs
        /// </summary>
        public string PlanId 
        { 
            get { return _planId; }
            set
            {
                if (!string.IsNullOrEmpty(_planId))
                {
                    throw new InvalidOperationException("Cannot change plan ID once set.");
                }
                _planId = value;
            }
        }

        /// <summary>
        /// The Charge component of the plan charge
        /// </summary>
        public Charge Charge { get { return _charge; } }

        /// <summary>
        /// Amount of the plan charge
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// Determines whether the plan charge is a setup fee
        /// </summary>
        public bool IsSetupCharge { get { return _isSetupCharge; } }

        /// <summary>
        /// Determines whether the plan charge is fully allocated to a payment plan
        /// </summary>
        public bool IsAutomaticallyModifiable { get { return _isAutomaticallyModifiable; } }

        /// <summary>
        /// Constructor for the plan charge entity
        /// </summary>
        /// <param name="planId">ID of the payment plan to which the plan charge belongs</param>
        /// <param name="charge">The original Charge</param>
        /// <param name="amount">The amount of the plan charge</param>
        /// <param name="isSetupCharge">Indicator that the plan charge is a setup fee</param>
        /// <param name="isAutomaticallyModifiable">Allows automatic plan modification for the payment plan to which the plan charge is assigned</param>
        public PlanCharge(string planId, Charge charge, decimal amount, bool isSetupCharge, bool isAutomaticallyModifiable)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge", "Charge must be provided.");
            }

            _planId = planId;
            _charge = charge;
            _amount = amount;
            _isSetupCharge = isSetupCharge;
            _isAutomaticallyModifiable = isAutomaticallyModifiable;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            PlanCharge other = obj as PlanCharge;
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(PlanId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(other.PlanId))
            {
                return false;
            }

            return other.PlanId.Equals(PlanId) && other.Charge.Id.Equals(Charge.Id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Id == null) ? base.GetHashCode() : Id.GetHashCode();
        }
    }
}
