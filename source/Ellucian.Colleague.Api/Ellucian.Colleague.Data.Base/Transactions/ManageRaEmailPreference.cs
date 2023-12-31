//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/31/2020 11:57:18 AM by user spirest
//
//     Type: CTX
//     Transaction ID: MANAGE.RA.EMAIL.PREFERENCE
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "MANAGE.RA.EMAIL.PREFERENCE", GeneratedDateTime = "3/31/2020 11:57:18 AM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class ManageRaEmailPreferenceRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ORG.ENTITY.ID", InBoundData = true)]        
		public string AOrgEntityId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACTION", InBoundData = true)]        
		public string AAction { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SEND.PREF", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ASendPref { get; set; }

		public ManageRaEmailPreferenceRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "MANAGE.RA.EMAIL.PREFERENCE", GeneratedDateTime = "3/31/2020 11:57:18 AM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class ManageRaEmailPreferenceResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SEND.PREF", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ASendPref { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.MESSAGE", OutBoundData = true)]        
		public string AMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		public ManageRaEmailPreferenceResponse()
		{	
			AlErrorMessages = new List<string>();
		}
	}
}
