// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RoomCharacteristic : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomCharacteristic"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The room type</param>
        public RoomCharacteristic(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}