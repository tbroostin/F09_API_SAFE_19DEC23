// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Contains the configuration parameters for the General Ledger account number.
    /// </summary>
    [Serializable]
    public class GeneralLedgerAccountStructure 
    {
        /// <summary>
        /// List of the major GL components.
        /// </summary>
        public ReadOnlyCollection<GeneralLedgerComponent> MajorComponents { get; private set; }
        private readonly List<GeneralLedgerComponent> majorComponents = new List<GeneralLedgerComponent>();

        /// <summary>
        /// List of all GL subcomponents.
        /// </summary>
        public ReadOnlyCollection<GeneralLedgerComponent> Subcomponents { get; private set; }
        private readonly List<GeneralLedgerComponent> subcomponents = new List<GeneralLedgerComponent>();

        /// <summary>
        /// Public getter for the list of start positions for each major component in the GL account.
        /// </summary>
        public ReadOnlyCollection<string> MajorComponentStartPositions { get; private set; }
        private readonly List<string> majorComponentStartPositions = new List<string>();

        /// <summary>
        /// The length of the GL account without delimiters or underscores.
        /// </summary>
        public string GlAccountLength { get; set; }

        /// <summary>
        /// The colleague delimiter used to seperate the GL Components
        /// </summary>
        public string glDelimiter { get; set; }

        /// <summary>
        /// The GL security role that indicates full access
        /// </summary>
        public string FullAccessRole { get; set; }

        /// <summary>
        /// Indicates if available funds are to be tested
        /// </summary>
        public string CheckAvailableFunds { get; set; }


        /// <summary>
        /// Contains a list of override tokens used to verify
        /// override permissions on submitted by users for EEDM.
        /// </summary>
        public List<string> AccountOverrideTokens { get; set; }

        /// <summary>
        /// Initialize the GL account structure object.
        /// </summary>
        public GeneralLedgerAccountStructure()
        {
            MajorComponents = majorComponents.AsReadOnly();
            Subcomponents = subcomponents.AsReadOnly();
            MajorComponentStartPositions = majorComponentStartPositions.AsReadOnly();
        }

        /// <summary>
        /// Set the major component start positions for the GL configuration.
        /// </summary>
        /// <param name="startPositions">List of start positions for the major components.</param>
        public void SetMajorComponentStartPositions(IEnumerable<string> startPositions)
        {
            // Remove the previous set of start positions before adding the new ones.
            majorComponentStartPositions.RemoveAll(x => true);

            // Add the list of start positions.
            majorComponentStartPositions.AddRange(startPositions);
        }

        /// <summary>
        /// Add a major GL component.
        /// </summary>
        /// <param name="glComponent">A major Gl Component object.</param>
        public void AddMajorComponent(GeneralLedgerComponent glComponent)
        {
            if (glComponent == null)
            {
                throw new ArgumentNullException("glComponent", "glComponent must have a value.");
            }

            if (this.majorComponents.Where(x => x.ComponentName == glComponent.ComponentName).ToList().Count == 0)
            {
                this.majorComponents.Add(glComponent);
            }
        }

        /// <summary>
        /// Add a GL subcomponent.
        /// </summary>
        /// <param name="glComponent">A GL subcomponent object.</param>
        public void AddSubcomponent(GeneralLedgerComponent glComponent)
        {
            if (glComponent == null)
            {
                throw new ArgumentNullException("glComponent", "glComponent must have a value.");
            }

            if (this.subcomponents.Where(x => x.ComponentName == glComponent.ComponentName).ToList().Count == 0)
            {
                this.subcomponents.Add(glComponent);
            }
        }
    }
}