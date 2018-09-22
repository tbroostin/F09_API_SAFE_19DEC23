/* Copyright 2017 Ellucian Company L.P.and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Summary
    /// </summary>
    [Serializable]
    public class PayStatementSummary
    {
        /// <summary>
        /// Database Id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Statement or Check Date
        /// </summary>
        public DateTime PayStatementDate { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statementDate"></param>
        public PayStatementSummary(string id, DateTime payStatementDate)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            this.Id = id;
            this.PayStatementDate = payStatementDate;
        }

        /// <summary>
        /// Examines the equality of this object and another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return Id == ((PayStatementSummary)obj).Id;
        }

        /// <summary>
        /// Produces this objects hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
