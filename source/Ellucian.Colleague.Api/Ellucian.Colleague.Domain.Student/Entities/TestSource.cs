// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Test Source Code table
    /// </summary>
    [Serializable]
    public class TestSource : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSource"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the TestSource</param>
        /// <param name="description">Description or Title of the TestSource</param>
        public TestSource(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}