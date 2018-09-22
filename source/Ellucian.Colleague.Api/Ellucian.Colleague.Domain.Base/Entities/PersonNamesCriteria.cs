// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    [Serializable]
    public class PersonNamesCriteria
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastNamePrefix { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public string Pedigree { get; set; }
    }
}
