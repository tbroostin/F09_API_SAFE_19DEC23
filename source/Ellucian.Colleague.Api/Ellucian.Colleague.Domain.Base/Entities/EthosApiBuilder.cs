// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    ///  Extended Data Entity
    /// </summary>
    [Serializable]
    public class EthosApiBuilder : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EthosApiBuilder"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Extended Data</param>
        /// <param name="code">Code representing the actual key to the Extended Data</param>
        /// <param name="description">Description Extended Data</param>
        public EthosApiBuilder(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}