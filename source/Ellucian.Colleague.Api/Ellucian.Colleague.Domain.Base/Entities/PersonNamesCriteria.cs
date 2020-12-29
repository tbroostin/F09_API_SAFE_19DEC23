// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
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

        // The ToString() is primarily used when getting the cacheKey.  Including the delimiters in the string output is important to distinguish different 
        // combinations of this filter (ie : firstname: John -vs- lastname: John).
        //public override string ToString() => $"{Title},{FirstName},{MiddleName},{LastNamePrefix},{LastName},{PreferredName},{Pedigree}";
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}", Title, FirstName, MiddleName, LastNamePrefix, LastName, PreferredName, Pedigree);
        }

    }
}
