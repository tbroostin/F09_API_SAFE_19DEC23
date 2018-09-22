// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Room Wing
    /// </summary>
    [Serializable]
    public class RoomWing : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomWing"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RoomWing</param>
        /// <param name="description">Description or Title of the RoomWing</param>
        public RoomWing(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}