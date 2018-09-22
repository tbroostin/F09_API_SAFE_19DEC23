// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
     /// <summary>
     /// The data table for the session cycle for a course
     /// </summary>
     [Serializable]
     public class SessionCycle : CodeItem
     {
          /// <summary>
          /// The SessionCycle constructor
          /// </summary>
          /// <param name="code"> The code of the session cycle </param>
          /// <param name="description"> The description of the session cycle</param>
          public SessionCycle(string code, string description)
               : base(code, description)
          {
          }
     }
}
