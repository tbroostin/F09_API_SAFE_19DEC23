// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;


namespace Ellucian.Colleague.Domain.Base.Entities
{
     /// <summary>
     /// This class contains commencement sites for graduation
     /// </summary>
     [Serializable]
     public class CommencementSite : CodeItem
     {
          /// <summary>
          /// This is the constructor for CommencementSite
          /// </summary>
          /// <param name="code"></param>
          /// <param name="description"></param>
          public CommencementSite(string code, string description)
            : base(code, description)
          {
          }
     }
}



