// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class MiscellaneousText
    {
        /// <summary>
        /// Publicly exposed Miscellaneous Text ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Publicly exposed Miscellaneous Text.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Constructor for a MiscellaneousText entity.
        /// </summary>
        /// <param name="id">The ID for the MTXT entry</param>
        /// <param name="message">The text for the MTXT entry</param>
        public MiscellaneousText(string id, string message)
        {
            this.Id = id;
            this.Message = message;
        }
    }
}
