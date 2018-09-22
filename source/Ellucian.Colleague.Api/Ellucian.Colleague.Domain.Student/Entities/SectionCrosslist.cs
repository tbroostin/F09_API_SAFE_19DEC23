// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section Crosslist information
    /// </summary>
    [Serializable]
    public class SectionCrosslist
    {
        #region Constructors

        /// <summary>
        /// Constructor for SectionCrosslist
        /// </summary>
        /// <param name="id">id of SectionCrosslist</param>
        /// <param name="primarySectionId">primary section id for SectionCrosslist</param>
        /// <param name="sectionIds">list of sections to be crosslisted</param>
        /// <param name="guid">Guid of SectionCrosslist</param>
        /// <param name="capacity">Capacity of SectionCrosslist</param>
        /// <param name="waitlistFlag">waitlist flag for SectionCrosslist</param>
        /// <param name="waitlistMax">waitlist maximimum for SectionCrosslist</param>
        public SectionCrosslist(string id, string primarySectionId, List<string> sectionIds, string guid = "",
            int? capacity = null, string waitlistFlag = "", int? waitlistMax = null)
        {
            if (!sectionIds.Any())
            {
                if (!string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException(string.Concat("You must have at least two sections to cross list, sectioncrosslist id : ", id, " has none."));    
                }
                else
                {
                    throw new ArgumentException("You must have at least two sections to cross list and there are none present.");
                }
            }
            else if (sectionIds.Count <= 1)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException(string.Concat("You must have at least two sections to cross list, sectioncrosslist id : ", id, " has one."));
                }
                else
                {
                    throw new ArgumentException("You must have at least two sections to cross list and there is only one present.");
                }
            }

            if (!sectionIds.Contains(primarySectionId))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException(string.Concat("Primary Section must be one of the sections in the sections list, sectioncrosslist id : ", id, " does not have a primary section set."));
                }
                else
                {
                    throw new ArgumentException("Primary Section must be one of the sections in the sections list.");
                }
            }
            
            Id = id;
            SectionIds = sectionIds;
            PrimarySectionId = primarySectionId;
            Guid = guid;
            Capacity = capacity;
            WaitlistFlag = waitlistFlag;
            WaitlistMax = waitlistMax;
        }
        
        #endregion

        #region Required Fields

        /// <summary>
        /// Id of SectionCrossList
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Id of the Primary Section
        /// </summary>
        public string PrimarySectionId { get; private set; }

        /// <summary>
        /// List of Ids of sections to Crosslist
        /// </summary>
        public List<string> SectionIds { get; private set; }

        #endregion

        #region Optional Fields

        /// <summary>
        /// Guid of SectionCrossList
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// SectionCrosslist Capacity
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// SectionCrosslist Waitlist Flag
        /// </summary>
        public string WaitlistFlag { get; set; }

        /// <summary>
        /// SectionCrosslist Waitlist maximum
        /// </summary>
        public int? WaitlistMax { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// List of sectionIds to update property with
        /// </summary>
        /// <param name="sectionIds">List of section ids to update property with</param>
        public void UpdateSectionIdList(List<string> sectionIds)
        {
            if (!sectionIds.Any())
            {
                throw new ArgumentException("You must have at least two sections to cross list, sectionIds has none.");
            }
            else if (sectionIds.Count <= 1)
            {
                throw new ArgumentException("You must have at least two sections to cross list.");
            }

            SectionIds = sectionIds;
        }

        /// <summary>
        /// Primary Section Id, must be present in the SectionIds list
        /// </summary>
        /// <param name="primarySectionId">section id to set as primary</param>
        public void UpdatePrimarySectionId(string primarySectionId)
        {
            if (string.IsNullOrEmpty(primarySectionId))
            {
                throw new ArgumentException("Primary Section Id must be set.");
            }

            if (!SectionIds.Contains(primarySectionId))
            {
                throw new ArgumentException("Primary Section must be one of the sections in the sections list.");
            }

            PrimarySectionId = primarySectionId;
        }


        #endregion

    }
}
