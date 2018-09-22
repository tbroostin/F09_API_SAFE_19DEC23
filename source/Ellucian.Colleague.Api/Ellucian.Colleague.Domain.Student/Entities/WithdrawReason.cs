//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class WithdrawReason :GuidCodeItem
    {
        /// <summary>
        /// Constructor for WithdrawReason
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public WithdrawReason(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
