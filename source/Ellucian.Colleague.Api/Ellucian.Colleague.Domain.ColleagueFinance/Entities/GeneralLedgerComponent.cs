// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Contains information describing how to define a general ledger component.
    /// </summary>
    [Serializable]
    public class GeneralLedgerComponent
    {
        /// <summary>
        /// Component name
        /// </summary>
        public string ComponentName { get { return componentName; } }
        private readonly string componentName;

        /// <summary>
        /// Is this component part of the cost center or object description?
        /// </summary>
        public bool IsPartOfDescription { get { return isPartOfDescription; } }
        private readonly bool isPartOfDescription;

        /// <summary>
        /// Component type.
        /// </summary>
        public GeneralLedgerComponentType ComponentType { get { return componentType; } }
        private readonly GeneralLedgerComponentType componentType;

        /// <summary>
        /// Start position of component in GL number.
        /// </summary>
        public int StartPosition { get { return startPosition; } }
        private readonly int startPosition;

        /// <summary>
        /// Length of component in GL number.
        /// </summary>
        public int ComponentLength { get { return componentLength; } }
        private readonly int componentLength;

        /// <summary>
        /// Create a GL component description object.
        /// </summary>
        /// <param name="componentName">Component ID</param>
        public GeneralLedgerComponent(string componentName, bool isPartOfDescription, GeneralLedgerComponentType componentType,
            string startPosition, string componentLength)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new ArgumentNullException("componentName", "componentName must have a value.");

            if (string.IsNullOrEmpty(componentLength))
                throw new ArgumentNullException("componentLength", "componentLength must have a value.");

            if (string.IsNullOrEmpty(startPosition))
                throw new ArgumentNullException("startPosition", "startPosition must have a value.");


            int requestedStartPosition;
            if (Int32.TryParse(startPosition, out requestedStartPosition))
            {
                if ((requestedStartPosition -1) < 0)
                {
                    throw new ApplicationException("The component start position cannot be negative.");
                }

                this.startPosition = requestedStartPosition - 1;
            }
            else
            {
                throw new ApplicationException("The component start is not an integer.");
            }

            int requestedLength;
            if (Int32.TryParse(componentLength, out requestedLength))
            {
                if ((requestedLength - 1) < 0)
                    throw new ApplicationException("Invalid length specified for GL component.");

                this.componentLength = requestedLength;
            }
            else
            {
                throw new ApplicationException("The component length is not an integer.");
            }

            this.componentName = componentName;
            this.isPartOfDescription = isPartOfDescription;
            this.componentType = componentType;
        }
    }
}