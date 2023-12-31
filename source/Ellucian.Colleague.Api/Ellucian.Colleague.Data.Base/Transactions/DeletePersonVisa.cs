//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:48:55 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: DELETE.PERSON.VISA
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class DeleteVisaErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]
		public string ErrorCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MSG", OutBoundData = true)]
		public string ErrorMsg { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.PERSON.VISA", GeneratedDateTime = "10/4/2017 12:48:55 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeletePersonVisaRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FOREIGN.PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GUID", InBoundData = true)]        
		public string StrGuid { get; set; }

		public DeletePersonVisaRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.PERSON.VISA", GeneratedDateTime = "10/4/2017 12:48:55 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeletePersonVisaResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODE", OutBoundData = true)]
		public List<DeleteVisaErrors> DeleteVisaErrors { get; set; }

		public DeletePersonVisaResponse()
		{	
			DeleteVisaErrors = new List<DeleteVisaErrors>();
		}
	}
}
