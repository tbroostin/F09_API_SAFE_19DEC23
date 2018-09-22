// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The data table for the cap sizes for graduation
    /// </summary>
    [Serializable]
    public class CapSize :CodeItem
    {
        /// <summary>
        /// The Cap Size constructor
        /// </summary>
        /// <param name="code"> The code of the cap size </param>
        /// <param name="description"> The description of the cap size</param>
        public CapSize(string code, string description)
            : base(code, description)
        {
        }
    }
}
