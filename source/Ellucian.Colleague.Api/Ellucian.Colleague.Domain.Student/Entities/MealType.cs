﻿using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealType :GuidCodeItem
    {
        /// <summary>
        /// Constructor for MealType
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public MealType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
