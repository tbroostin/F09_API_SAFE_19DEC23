// Copyright 2013-2020 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CareerGoal : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CareerGoals"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CareerGoal(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}