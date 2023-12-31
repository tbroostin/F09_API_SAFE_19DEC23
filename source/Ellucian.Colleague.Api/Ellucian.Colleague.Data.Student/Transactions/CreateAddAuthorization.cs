//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/2/2018 9:34:03 AM by user cindystair
//
//     Type: CTX
//     Transaction ID: CREATE.ADD.AUTHORIZATION
//     Application: ST
//     Environment: dvcoll
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.ADD.AUTHORIZATION", GeneratedDateTime = "5/2/2018 9:34:03 AM", User = "cindystair")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CreateAddAuthorizationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.SECTION.ID", InBoundData = true)]        
		public string SectionId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ASSIGNED.BY", InBoundData = true)]        
		public string AssignedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.ASSIGNED.DATE", InBoundData = true)]        
		public Nullable<DateTime> AssignedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "A.ASSIGNED.TIME", InBoundData = true)]        
		public Nullable<DateTime> AssignedTime { get; set; }

		public CreateAddAuthorizationRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.ADD.AUTHORIZATION", GeneratedDateTime = "5/2/2018 9:34:03 AM", User = "cindystair")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CreateAddAuthorizationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.NEW.ADD.AUTHORIZATION.ID", OutBoundData = true)]        
		public string NewAddAuthorizationId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public CreateAddAuthorizationResponse()
		{	
		}
	}
}
