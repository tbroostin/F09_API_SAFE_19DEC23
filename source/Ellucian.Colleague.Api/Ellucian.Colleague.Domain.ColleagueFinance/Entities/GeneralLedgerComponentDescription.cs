// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    [Serializable]
    public class GeneralLedgerComponentDescription
    {
        /// <summary>
        /// ID of the GL component.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Description of the GL component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of the GL component.
        /// </summary>
        public GeneralLedgerComponentType ComponentType { get { return componentType; } }
        private readonly GeneralLedgerComponentType componentType;

        /// <summary>
        /// Initialize the GL component description object.
        /// </summary>
        /// <param name="id">GL component code</param>
        /// <param name="componentType">GL component type</param>
        public GeneralLedgerComponentDescription(string id, GeneralLedgerComponentType componentType)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "id must have a value.");

            this.id = id;
            this.componentType = componentType;
        }
    }
}