// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PaymentMethod : CodeItem
    {
        private readonly PaymentMethodCategory _category;
        /// <summary>
        /// The category into which this payment method falls
        /// </summary>
        public PaymentMethodCategory Category {get { return _category; }}

        private readonly bool _isWebEnabled;
        /// <summary>
        /// Identifies whether this payment method is enabled for use on the Internet
        /// </summary>
        public bool IsWebEnabled { get { return _isWebEnabled; } }

        private readonly bool _isECommerceEnabled;
        /// <summary>
        /// Identifies whether this payment method is enabled for use with e-Commerce
        /// </summary>
        public bool IsECommerceEnabled { get { return _isECommerceEnabled; } }

        private readonly List<string> _officeCodes = new List<string>(); 
        /// <summary>
        /// The list of modules for which this category is valid
        /// </summary>
        public ReadOnlyCollection<string> OfficeCodes { get; private set; }

        /// <summary>
        /// Indicates if a student is allowed to use this payment method
        /// </summary>
        public bool IsValidForStudentReceivables 
        { get { return (
            _category == PaymentMethodCategory.Check || 
            _category == PaymentMethodCategory.CreditCard || 
            _category == PaymentMethodCategory.ElectronicFundsTransfer || 
            _category == PaymentMethodCategory.Other); } 
        }

        /// <summary>
        /// Constructor for PaymentMethod
        /// </summary>
        /// <param name="code">Payment method code</param>
        /// <param name="description">Payment method description</param>
        /// <param name="category">Payment method category</param>
        /// <param name="isWebEnabled">Is this payment method web enabled?</param>
        /// <param name="isECommerceEnabled">Is this payment method enabled for e-Commerce?</param>
        public PaymentMethod(string code, string description, PaymentMethodCategory category, bool isWebEnabled, bool isECommerceEnabled)
            : base(code, description)
        {
            // If a payment method is web enabled, then it must also be enabled for e-commerce
            if (isWebEnabled && !isECommerceEnabled)
            {
                throw new ArgumentOutOfRangeException("isECommerceEnabled", "A payment method that is web enabled must also be enabled for e-commerce.");
            }

            _category = category;
            _isWebEnabled = isWebEnabled;
            _isECommerceEnabled = isECommerceEnabled;

            OfficeCodes = _officeCodes.AsReadOnly();
        }

        /// <summary>
        /// Add an office code to the payment method
        /// </summary>
        /// <param name="officeCode">The office code to add</param>
        public void AddOfficeCode(string officeCode)
        {
            if (string.IsNullOrEmpty(officeCode))
            {
                throw new ArgumentNullException("officeCode", "An office code must be provided.");
            }

            // Don't add an office code that is already in the list
            if (!_officeCodes.Contains(officeCode))
            {
                _officeCodes.Add(officeCode);
            }
        }

    }
}
