// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A person with whom another person has a relationship
    /// </summary>
    public class RelatedPerson
    {
        /// <summary>
        ///  Unique system ID of the related person
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The related person's preferred email address 
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The related person's preferred name
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Code for the related person's relationship type
        /// </summary>
        public string RelationshipCode { get; set; }
    }
}
