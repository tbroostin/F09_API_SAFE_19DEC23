//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardYear class defines a Financial Aid Award Year. These objects
    /// are predominantly used for UI display purposes.
    /// </summary>
    [Serializable]
    public class AwardYear : CodeItem
    {

        /// <summary>
        /// Constructor for AwardYear object requires a code and a description
        /// </summary>
        /// <param name="code">The unique award year code.</param>
        /// <param name="desc">A description of the award year.</param>
        public AwardYear(string code, string description)
            : base(code, description)
        {

        }

        /// <summary>
        /// This attribute returns the AwardYear.Code as an integer representation of the
        /// Award Year. If, for some reason, the Award Year Code is invalid, this attribute
        /// returns -1;
        /// </summary>
        public int YearAsNumber
        {
            get
            {
                int year;
                if (int.TryParse(Code, out year))
                {
                    return year;
                }
                return -1;
            }
        }
    }
}
