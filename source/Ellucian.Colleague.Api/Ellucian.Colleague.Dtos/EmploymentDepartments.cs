//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined employment departments. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmploymentDepartments : FilterCodeItem2
    {
        /// <summary>
        /// Status of the departement code table entry. (Active or Inactive).
        /// </summary>
        [DataMember(Name = "status", EmitDefaultValue = false)]
        public EmploymentDepartmentStatuses? Status { get; set; }
    }      
}          
