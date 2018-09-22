// Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Gown Sizes Code Table
    /// </summary>
    [Serializable]
    public class GownSize : CodeItem
    {
        /// <summary>
        /// Overloaded constructor for Gown Size class
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public GownSize(string code, string description) : base(code, description)
        {

        }
    }
}
