//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/15/2020 4:55:17 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: UPDATE.ETHOS.API.BUILDER
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
	public class UpdateEthosApiBuilderErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.ETHOS.API.BUILDER", GeneratedDateTime = "10/15/2020 4:55:17 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdateEthosApiBuilderRequest
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

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public UpdateEthosApiBuilderRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.ETHOS.API.BUILDER", GeneratedDateTime = "10/15/2020 4:55:17 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdateEthosApiBuilderResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ENTITY", OutBoundData = true)]        
		public string Entity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RECORD.KEY", OutBoundData = true)]        
		public string RecordKey { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RECORD.GUID", OutBoundData = true)]        
		public string RecordGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERRORS", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Errors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<UpdateEthosApiBuilderErrors> UpdateEthosApiBuilderErrors { get; set; }

		public UpdateEthosApiBuilderResponse()
		{	
			UpdateEthosApiBuilderErrors = new List<UpdateEthosApiBuilderErrors>();
		}
	}
}
