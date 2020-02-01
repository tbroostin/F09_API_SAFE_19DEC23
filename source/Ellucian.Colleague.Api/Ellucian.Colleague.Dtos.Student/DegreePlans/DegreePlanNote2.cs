// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// A note added to a student's degree plan
    /// </summary>
    public class DegreePlanNote2
    {
        /// <summary>
        /// Unique system id of this note (zero if new)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Date and time this note was added
        /// </summary>
        public DateTimeOffset? Date { get; set; }

        /// <summary>
        /// System Id of the person who added this note
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Note text, free-form
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Indicates the <see cref="PersonType">type of person</see> who authored the note so that correct endpoint may be used to get person's name
        /// May be Student or Advisor 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonType PersonType { get; set; }
    }
}
