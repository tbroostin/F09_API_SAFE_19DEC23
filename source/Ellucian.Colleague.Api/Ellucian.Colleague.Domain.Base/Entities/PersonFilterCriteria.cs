// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    [Serializable]
    public class PersonFilterCriteria
    {
        public List<Tuple<string, string>> Credentials { get; set; }
        public List<Tuple<string, string>> AlternativeCredentials { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Emails { get; set; }     
        public List<PersonNamesCriteria> Names { get; set; }
    }
}
