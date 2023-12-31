//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/7/2021 8:04:33 PM by user sushrutcolleague
//
//     Type: CTX
//     Transaction ID: CREATE.UPDATE.PROXY.ACCESS
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
	public class ProxyPermissions
	{
		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PROXY.IDS", InBoundData = true)]
		public string ProxyIdentifiers { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PROXY.PERMISSION.IDS", InBoundData = true)]
		public string ProxyPermissionIdentifiers { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PROXY.PERMISSION.GRANTED.INDS", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]
		public bool ProxyPermissionGrantedIndicators { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PROXY.PERMISSION.START.DATES", InBoundData = true)]
		public Nullable<DateTime> ProxyPermissionStartDates { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PROXY.PERMISSION.END.DATES", InBoundData = true)]
		public Nullable<DateTime> ProxyPermissionEndDates { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.UPDATE.PROXY.ACCESS", GeneratedDateTime = "4/7/2021 8:04:33 PM", User = "sushrutcolleague")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class CreateUpdateProxyAccessRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PRINCIPAL.ID", InBoundData = true)]        
		public string PrincipalIdentifier { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DISCLOSURE.DOCUMENT.TXT", InBoundData = true)]        
		public List<string> DisclosureDocumentText { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "REAUTH.IND", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ReauthorizationIndicator { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:PROXY.IDS", InBoundData = true)]
		public List<ProxyPermissions> ProxyPermissions { get; set; }

		public CreateUpdateProxyAccessRequest()
		{	
			DisclosureDocumentText = new List<string>();
			ProxyPermissions = new List<ProxyPermissions>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.UPDATE.PROXY.ACCESS", GeneratedDateTime = "4/7/2021 8:04:33 PM", User = "sushrutcolleague")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class CreateUpdateProxyAccessResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PROXY.ACCESS.IDS", OutBoundData = true)]        
		public List<string> ProxyAccessIdentifiers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.IND", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorIndicator { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARN.IND", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool WarningIndicator { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MESSAGES", OutBoundData = true)]        
		public List<string> Messages { get; set; }

		public CreateUpdateProxyAccessResponse()
		{	
			ProxyAccessIdentifiers = new List<string>();
			Messages = new List<string>();
		}
	}
}
