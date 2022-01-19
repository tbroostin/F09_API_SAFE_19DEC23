//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/18/2020 10:28:40 AM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: DELETE.ETHOS.API.BUILDER
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class DeleteEthosApiBuilderErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.ETHOS.API.BUILDER", GeneratedDateTime = "12/18/2020 10:28:40 AM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeleteEthosApiBuilderRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ENTITY", InBoundData = true)]        
		public string Entity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RECORD.KEY", InBoundData = true)]        
		public string RecordKey { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RECORD.GUID", InBoundData = true)]        
		public string RecordGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RESOURCE.NAME", InBoundData = true)]        
		public string ResourceName { get; set; }

		public DeleteEthosApiBuilderRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.ETHOS.API.BUILDER", GeneratedDateTime = "12/18/2020 10:28:40 AM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeleteEthosApiBuilderResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERRORS", OutBoundData = true)]        
		public string Errors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<DeleteEthosApiBuilderErrors> DeleteEthosApiBuilderErrors { get; set; }

		public DeleteEthosApiBuilderResponse()
		{	
			DeleteEthosApiBuilderErrors = new List<DeleteEthosApiBuilderErrors>();
		}
	}
}