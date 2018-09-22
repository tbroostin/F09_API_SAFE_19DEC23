// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Class for project type
    /// </summary>
    [Serializable]
    public class ProjectType : CodeItem
    {
        /// <summary>
        /// Project type constructor
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public ProjectType(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}