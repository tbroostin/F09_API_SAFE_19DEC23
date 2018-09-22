// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a blanket purchase order
    /// </summary>
    [Serializable]
    public class BlanketPurchaseOrder : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The blanket purchase order number
        /// </summary>
        public string Number { get { return number; } }

        /// <summary>
        /// Private current status for public getter
        /// </summary>
        private readonly BlanketPurchaseOrderStatus status;

        /// <summary>
        /// The blanket purchase order current status
        /// </summary>
        public BlanketPurchaseOrderStatus Status { get { return status; } }

        /// <summary>
        /// Private current status date for public getter
        /// </summary>
        private readonly DateTime statusDate;

        /// <summary>
        /// The blanket purchase order current status date
        /// </summary>
        public DateTime StatusDate { get { return statusDate; } }

        /// <summary>
        /// The blanket purchase order expiration date
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// The blanket purchase order initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The blanket purchase order Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The blanket purchase order internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// The private list of requisitions associated with the blanket purchase order
        /// </summary>
        private readonly List<string> requisitions = new List<string>();

        /// <summary>
        /// The public getter for the private list of requisitions associated with the blanket purchase order
        /// </summary>
        public ReadOnlyCollection<string> Requisitions { get; private set; }

        /// <summary>
        /// The private list of vouchers associated with the blanket purchase order
        /// </summary>
        private readonly List<string> vouchers = new List<string>();

        /// <summary>
        /// The public getter for the private list of vouchers associated with the blanket purchase order
        /// </summary>
        public ReadOnlyCollection<string> Vouchers { get; private set; }

        /// <summary>
        /// The private list of GL distributions for the blanket purchase order
        /// </summary>
        private readonly List<BlanketPurchaseOrderGlDistribution> glDistributions = new List<BlanketPurchaseOrderGlDistribution>();

        /// <summary>
        /// List of the Blanket Purchase Order GL distributions
        /// </summary>
        public ReadOnlyCollection<BlanketPurchaseOrderGlDistribution> GlDistributions { get; private set; }

        /// <summary>
        /// This constructor initializes the blanket purchase order domain entity.
        /// </summary>
        /// <param name="id">This is the blanket purchase order ID.</param>
        /// <param name="number">This is the blanket purchase order number.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public BlanketPurchaseOrder(string id, string number, string vendorName, BlanketPurchaseOrderStatus status, DateTime statusDate, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;
            this.status = status;
            this.statusDate = statusDate;
            Requisitions = this.requisitions.AsReadOnly();
            Vouchers = this.vouchers.AsReadOnly();
            GlDistributions = this.glDistributions.AsReadOnly();
        }

        /// <summary>
        /// This method adds a requisition to the list of requisitions for the blanket purchase order
        /// </summary>
        /// <param name="requisition">The associated requisition</param>
        public void AddRequisition(string requisition)
        {
            if (string.IsNullOrEmpty(requisition))
            {
                throw new ArgumentNullException("requisition", "Requisition cannot be null");
            }

            if (requisitions != null && !this.requisitions.Contains(requisition))
            {
                this.requisitions.Add(requisition);
            }
        }

        /// <summary>
        /// This method adds a voucher to the list of vouchers for the blanket purchase order
        /// </summary>
        /// <param name="voucher">The associated voucher</param>
        public void AddVoucher(string voucher)
        {
            if (string.IsNullOrEmpty(voucher))
            {
                throw new ArgumentNullException("voucher", "Voucher cannot be null");
            }

            if (vouchers != null && !this.vouchers.Contains(voucher))
            {
                this.vouchers.Add(voucher);
            }
        }

        /// <summary>
        /// This method adds a GL distribution to the list of GL distributions 
        /// that belong to the blanket purchase order
        /// </summary>
        /// <param name="glDistribution">This is the blanket purchase order GL distribution</param>
        public void AddGlDistribution(BlanketPurchaseOrderGlDistribution glDistribution)
        {
            if (glDistribution == null)
            {
                throw new ArgumentNullException("glDistribution", "GL distribution cannot be null");
            }

            bool isInList = false;
            if (glDistributions != null)
            {
                foreach (var glDistr in glDistributions)
                {
                    if ((glDistr.GlAccountNumber == glDistribution.GlAccountNumber) & (glDistr.ProjectLineItemId == glDistribution.ProjectLineItemId))
                    {
                        isInList = true;
                    }
                }
            }

            if (!isInList)
            {
                glDistributions.Add(glDistribution);
            }
        }
    }
}
