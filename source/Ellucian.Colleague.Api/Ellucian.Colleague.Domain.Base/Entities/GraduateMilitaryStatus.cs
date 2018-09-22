// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
     /// <summary>
     /// The different types of military statuses
     /// </summary>
     [Serializable]
     public enum GraduateMilitaryStatus
     {
          /// <summary>
          /// Active Military
          /// </summary>
          ActiveMilitary,

          /// <summary>
          /// Has served, but is not active military
          /// </summary>
          Veteran,

          /// <summary>
          /// Has never served in the military
          /// </summary>
          NotApplicable
     }
}
