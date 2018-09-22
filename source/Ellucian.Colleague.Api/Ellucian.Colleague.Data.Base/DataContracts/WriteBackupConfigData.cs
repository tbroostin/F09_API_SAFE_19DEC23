//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/27/2017 6:04:03 PM by user dat
//
//     Type: CTX
//     Transaction ID: WRITE.BACKUP.CONFIG.DATA
//     Application: UT
//     Environment: dvetk
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "WRITE.BACKUP.CONFIG.DATA", GeneratedDateTime = "10/27/2017 6:04:03 PM", User = "dat")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class WriteBackupConfigDataRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "CONFIG.DATA", InBoundData = true)]        
		public string ConfigData { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "CONFIG.VERSION", InBoundData = true)]        
		public string ConfigVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "NAMESPACE", InBoundData = true)]        
		public string Namespace { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PRODUCT.ID", InBoundData = true)]        
		public string ProductId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PRODUCT.VERSION", InBoundData = true)]        
		public string ProductVersion { get; set; }

		public WriteBackupConfigDataRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "WRITE.BACKUP.CONFIG.DATA", GeneratedDateTime = "10/27/2017 6:04:03 PM", User = "dat")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class WriteBackupConfigDataResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CONFIG.DATA.ID", OutBoundData = true)]        
		public string ConfigDataId { get; set; }

		public WriteBackupConfigDataResponse()
		{	
		}
	}
}
