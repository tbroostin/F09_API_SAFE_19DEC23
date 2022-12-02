/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.HumanResources
{
     /// <summary>
     /// The demographics of a person
     /// </summary>
     public class HumanResourceDemographics
     {
          /// <summary>
          /// The person's Id
          /// </summary>
          public string Id { get; set; }

          /// <summary>
          /// The person's first name
          /// </summary>
          public string FirstName { get; set; }

          /// <summary>
          /// The person's last name
          /// </summary>
          public string LastName { get; set; }

          /// <summary>
          /// The person's middle name
          /// </summary>
          public string MiddleName { get; set; }

          /// <summary>
          /// The person's preferred name
          /// </summary>
          public string PreferredName { get; set; }

          /// <summary>
          /// This property is based on a Name Address Hierarcy and will be null if none is provided.
          /// </summary>
          public PersonHierarchyName PersonDisplayName { get; set; }
    }
}
