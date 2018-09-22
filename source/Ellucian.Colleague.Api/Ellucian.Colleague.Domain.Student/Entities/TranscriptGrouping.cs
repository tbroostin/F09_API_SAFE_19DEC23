// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A TranscriptGrouping helps define the set of academic credit (completed courses, etc) that will be
    /// used when generating a transcript for a student.  For example, an institution could use different 
    /// groupings of courses/credits for undergraduate and graduate transcripts.
    /// </summary>
    [Serializable]
    public class TranscriptGrouping
    {
        /// <summary>
        /// The ID for the transcript grouping (user-defined)
        /// </summary>
        private readonly string id;

        /// <summary>
        /// The public getter for the id
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// A description of this transcript grouping
        /// </summary>
        private readonly string description;

        /// <summary>
        /// The public getter for the description
        /// </summary>
        public string Description { get { return description; } }

        /// <summary>
        /// Is this transcript group flagged as user-selectable?
        /// </summary>
        private readonly bool isUserSelectable;

        /// <summary>
        /// The public getter for isUserSelectable
        /// </summary>
        public bool IsUserSelectable { get { return isUserSelectable; } }

        /// <summary>
        /// Constructor for a transcript grouping
        /// </summary>
        /// <param name="id">The ID of the transcript grouping</param>
        /// <param name="description">The description of the transcript grouping</param>
        public TranscriptGrouping(string id, string description, bool isUserSelectable)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is a required attribute.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "description is a required attribute.");
            }

            this.id = id;
            this.description = description;
            this.isUserSelectable = isUserSelectable;
        }
    }
}