// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A TranscriptGrouping helps define the set of academic credit (completed courses, etc) that will be
    /// used when generating a transcript for a student.  For example, an institution could use different 
    /// groupings of courses/credits for undergraduate and graduate transcripts.
    /// </summary>
    public class TranscriptGrouping
    {
        /// <summary>
        /// The ID for the transcript grouping (user-defined)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The description of this transcript grouping
        /// </summary>
        public string Description { get; set; }
    }
}