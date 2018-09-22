// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A Cashier entity
    /// </summary>
    [Serializable]
    public class Cashier
    {
        private readonly string _id;
        /// <summary>
        /// The cashier's Person identifier
        /// </summary>
        public string Id { get { return _id; } }

        private readonly bool _isECommerceEnabled;
        /// <summary>
        /// Is the cashier allowed to perform e-commerce transactions?
        /// </summary>
        public bool IsECommerceEnabled { get { return _isECommerceEnabled; } }

        private readonly string _login;
        /// <summary>
        /// The login of the cashier
        /// </summary>
        public string Login { get { return _login; } }

        private decimal? _checkLimitAmount;
        /// <summary>
        /// The largest amount that the cashier may process via a check.  If null, then no limit.
        /// </summary>
        public decimal? CheckLimitAmount
        {
            get { return _checkLimitAmount; }
            set
            {
                if (_checkLimitAmount.HasValue)
                {
                    throw new InvalidOperationException("Check limit amount already defined for cashier.");
                }
                if (value.HasValue && value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Check limit amount cannot be less than zero.");
                }
                _checkLimitAmount = value;
            }
        }

        private decimal? _creditCardLimitAmount;
        /// <summary>
        /// The largest amount that the cashier may process via a credit card.  If null, then no limit.
        /// </summary>
        public decimal? CreditCardLimitAmount
        {
            get { return _creditCardLimitAmount; }
            set
            {
                if (_creditCardLimitAmount.HasValue)
                {
                    throw new InvalidOperationException("Credit card limit amount already defined for cashier.");
                }
                if (value.HasValue && value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Credit card limit amount cannot be less than zero.");
                }
                _creditCardLimitAmount = value;
            }
        }

        /// <summary>
        /// Public constructor for a Cashier entity
        /// </summary>
        /// <param name="id">The person identifier of the cashier</param>
        /// <param name="login">Login ID of the cashier</param>
        /// <param name="isECommEnabled">Indicates whether the cashier can perform eCommerce transactions; default is false</param>
        public Cashier(string id, string login, bool isECommEnabled = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException("login");
            }

            _id = id;
            _isECommerceEnabled = isECommEnabled;
            _login = login;
        }
    }
}
