/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{

     [Serializable]
     public class HumanResourceDemographics
     {
          public string Id { get; set; }

          public string FirstName { get; set; }

          public string LastName { get; set; }

          public string PreferredName { get; set; }

          public HumanResourceDemographics(string id, string firstName, string lastName, string preferredName)
          {
               Id = id;
               FirstName = firstName;
               LastName = lastName;
               PreferredName = preferredName;
          }
     }
}
