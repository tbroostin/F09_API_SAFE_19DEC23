//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/5/2017 2:44:59 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.SECTION.WAITLIST.STATUS
//     Application: ST
//     Environment: dvColl
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
	[ColleagueDataContract(ColleagueId = "GET.SECTION.WAITLIST.STATUS", GeneratedDateTime = "10/5/2017 2:44:59 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetSectionWaitlistStatusRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ID", InBoundData = true)]        
		public string Id { get; set; }

		public GetSectionWaitlistStatusRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.SECTION.WAITLIST.STATUS", GeneratedDateTime = "10/5/2017 2:44:59 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetSectionWaitlistStatusResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STATUS", OutBoundData = true)]        
		public string Status { get; set; }

		public GetSectionWaitlistStatusResponse()
		{	
			ErrorMessages = new List<string>();
		}
	}
}
