// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
     /// <summary>
     /// The data table for the yearly cycle for a course
     /// </summary>
     [Serializable]
     public class YearlyCycle : CodeItem
     {
          /// <summary>
          /// The YearlyCycle constructor
          /// </summary>
          /// <param name="code"> The code of the yearly cycle </param>
          /// <param name="description"> The description of the yearly cycle</param>
          public YearlyCycle(string code, string description)
               : base(code, description)
          {
          }
     }
}
