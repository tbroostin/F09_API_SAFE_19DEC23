// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// County codes.
    /// </summary>
    [Serializable]
    public class County : CodeItem
    {
        private readonly string _guid;
        /// <summary>
        /// Gets the Guid.
        /// </summary>
        /// <value>
        /// The Guid.
        /// </value>
        public string Guid { get { return _guid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="County"/> class.
        /// </summary>
        /// <param name="guid">The guid.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public County(string guid, string code, string description)
            : base(code, description)
        {
            _guid = guid;
        }
    }
}