/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PositionFundingSource object describes where money comes from to pay a position
    /// or a person position
    /// </summary>
    [Serializable]
    public class PositionFundingSource
    {
        /// <summary>
        /// The Id of the FundingSource
        /// </summary>
        public string FundingSourceId { get { return fundingSourceId; } }
        private readonly string fundingSourceId;

        /// <summary>
        /// A sort order used to group funding sources.        
        /// </summary>
        public int FundingOrder { get { return fundingOrder; } }
        private int fundingOrder;

        /// <summary>
        /// The Id of the Project this funding source is assigned to. Could be null indicating there is no project
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// This number is the one used internally in an  institution to refer to a project
        /// </summary>
        public string ProjectRefNumber { get; set; }

        /// <summary>
        /// Build a PositionFundingSource
        /// </summary>
        /// <param name="fundingSourceId"></param>
        /// <param name="fundingOrder"></param>
        public PositionFundingSource(string fundingSourceId, int fundingOrder)
        {
            if (string.IsNullOrEmpty(fundingSourceId))
            {
                throw new ArgumentNullException("fundingSourceId");
            }
            if (fundingOrder < 0)
            {
                throw new ArgumentOutOfRangeException("fundingOrder", "fundingOrder must be non-negative");
            }

            this.fundingSourceId = fundingSourceId;
            this.fundingOrder = fundingOrder;
        }

        /// <summary>
        /// Two objects are equal when their funding source and funding order are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var fundingSource = obj as PositionFundingSource;

            return fundingSource.FundingSourceId == this.FundingSourceId &&
                fundingSource.FundingOrder == this.FundingOrder;
        }

        /// <summary>
        /// Computes the hashcode based on the source id and the order
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return FundingSourceId.GetHashCode() ^ FundingOrder.GetHashCode();
        }

        /// <summary>
        /// Returns a string with the format {FundingSourceId}*{FundingOrder}::{ProjectId}
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}::{2}", FundingSourceId, FundingOrder, ProjectId);
        }
    }
}
