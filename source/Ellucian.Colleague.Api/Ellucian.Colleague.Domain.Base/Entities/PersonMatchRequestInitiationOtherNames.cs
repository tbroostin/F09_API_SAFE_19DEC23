//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonMatchRequestInitiationOtherNames
    {
        public PersonMatchRequestInitiationOtherNames()
        {

        }
        public PersonMatchRequestInitiationOtherNames(string first, string middle, string last)
        {
            FirstName = first;
            MiddleName = middle;
            LastName = last;
        }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }
    }
}
