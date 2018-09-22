//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// An AwardStatus is a code item that indicates a particular status of a student's award.
    /// Each award status means something different to different clients.
    /// The description of the AwardStatus explains what the award status means.
    /// The AwardStatusCategory indicates the basic state of an award to Colleague.
    /// </summary>
    [Serializable]
    public class AwardStatus : CodeItem
    {
        /// <summary>
        /// Private attribute containing the category of this AwardStatus.
        /// The category indicates the basic state of an award to Colleague.
        /// </summary>
        private AwardStatusCategory _Category;

        /// <summary>
        /// Public accessor for the category
        /// </summary>
        public AwardStatusCategory Category { get { return _Category; } set { _Category = value; } }

        /// <summary>
        /// This Financial Aid AwardStatus Constructor requires a code, a description and a category.
        /// </summary>
        /// <param name="code">The unique AwardStatus code, generally a single letter or number</param>
        /// <param name="desc">A short description of the code</param>
        /// <param name="category">The Colleague category of the AwardStatus</param>        
        public AwardStatus(string code, string desc, AwardStatusCategory category)
            : base(code, desc)
        {
            _Category = category;
        }
    }
}
