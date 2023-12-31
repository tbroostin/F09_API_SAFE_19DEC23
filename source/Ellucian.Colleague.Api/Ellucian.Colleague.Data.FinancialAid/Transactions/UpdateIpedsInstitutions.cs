//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:22:30 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: UPDATE.IPEDS.INSTITUTIONS
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[DataContract]
	public class IpedsInstitutionData
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "INST.RECORD.KEYS", InBoundData = true, OutBoundData = true)]
		public string InstRecordKeys { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INST.UNIT.IDS", InBoundData = true, OutBoundData = true)]
		public string InstUnitIds { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "INST.OPE.IDS", InBoundData = true, OutBoundData = true)]
		public string InstOpeIds { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "INST.NAMES", InBoundData = true, OutBoundData = true)]
		public string InstNames { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.IPEDS.INSTITUTIONS", GeneratedDateTime = "10/4/2017 2:22:30 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateIpedsInstitutionsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IPEDS.LAST.MODIFIED.DATE", InBoundData = true)]        
		public Nullable<DateTime> IpedsLastModifiedDate { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:INST.OPE.IDS", InBoundData = true)]
		public List<IpedsInstitutionData> IpedsInstitutionData { get; set; }

		public UpdateIpedsInstitutionsRequest()
		{	
			IpedsInstitutionData = new List<IpedsInstitutionData>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.IPEDS.INSTITUTIONS", GeneratedDateTime = "10/4/2017 2:22:30 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateIpedsInstitutionsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:INST.OPE.IDS", OutBoundData = true)]
		public List<IpedsInstitutionData> IpedsInstitutionData { get; set; }

		public UpdateIpedsInstitutionsResponse()
		{	
			IpedsInstitutionData = new List<IpedsInstitutionData>();
		}
	}
}
