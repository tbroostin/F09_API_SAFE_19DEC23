//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined financial aid satisfactory academic progress (SAP) types. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AdministrativeInstructionalMethods : CodeItem2
    {
        /// <summary>
        /// InstructionalMethod Primary Key.
        /// </summary>
        [DataMember(Name = "instructionalMethod", EmitDefaultValue = false)]
        public GuidObject2 InstructionalMethod { get; set; }
    }
}        
