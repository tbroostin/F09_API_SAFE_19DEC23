// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
     /// <summary>
     /// Enumeration of VeteranStatusType: Types are Active military, Veteran, and No
     /// </summary>
     [JsonConverter(typeof(StringEnumConverter))]
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
