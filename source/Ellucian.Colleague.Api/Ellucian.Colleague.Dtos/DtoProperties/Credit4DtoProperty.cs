//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The method of approval. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Credit4DtoProperty
    {
        /// <summary>
       /// The type of approval granted.
       /// </summary>
          
       [JsonProperty("measure")]  
       public StudentCourseTransferMeasure? Measure { get; set; }
     
        /// <summary>
       /// The entity granting approval.
       /// </summary>
          
       [JsonProperty("registrationCredit")]
       public decimal? RegistrationCredit { get; set; }
     }      
}  

