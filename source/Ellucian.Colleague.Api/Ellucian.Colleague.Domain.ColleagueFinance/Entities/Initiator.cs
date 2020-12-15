// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Representation of staff
    /// </summary>
    [Serializable]
    public class Initiator
    {
        
        /// <summary>
        /// The Initiator id
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private id for public getter
        /// </summary>
        private readonly string id;

        /// <summary>
        /// The Initiator's name
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Private name for public getter
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The Initiator's Code
        /// </summary>
        public string Code { get { return code; } }

        /// <summary>
        /// Private code for public getter
        /// </summary>
        private readonly string code;

        /// <summary>
        /// Initializes a new instance of the <see cref="Initiator"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="code">The code for this initiator</param>
        public Initiator(string id, string name, string code)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            this.id = id;
            this.name = name;
            this.code = code;
        }
    }
}
