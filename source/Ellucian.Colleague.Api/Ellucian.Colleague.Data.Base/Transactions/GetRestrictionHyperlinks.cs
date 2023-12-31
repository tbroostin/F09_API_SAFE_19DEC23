//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:50:56 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.RESTRICTION.HYPERLINKS
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.RESTRICTION.HYPERLINKS", GeneratedDateTime = "10/4/2017 12:50:56 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetRestrictionHyperlinksRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RESTRICTION.IDS", InBoundData = true)]        
		public List<string> RestrictionIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINK.LABELS.IN", InBoundData = true)]        
		public List<string> LinkLabelsIn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINK.DEFS.IN", InBoundData = true)]        
		public List<string> LinkDefinitionsIn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINK.APPLS.IN", InBoundData = true)]        
		public List<string> LinkApplicationsIn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WA.FORMS.IN", InBoundData = true)]        
		public List<string> WaFormsIn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MTXT.FLAGS.IN", InBoundData = true)]        
		public List<string> MtxtFlagsIn { get; set; }

		public GetRestrictionHyperlinksRequest()
		{	
			RestrictionIds = new List<string>();
			LinkLabelsIn = new List<string>();
			LinkDefinitionsIn = new List<string>();
			LinkApplicationsIn = new List<string>();
			WaFormsIn = new List<string>();
			MtxtFlagsIn = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.RESTRICTION.HYPERLINKS", GeneratedDateTime = "10/4/2017 12:50:56 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetRestrictionHyperlinksResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINK.LABELS.OUT", OutBoundData = true)]        
		public List<string> LinkLabelsOut { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HYPERLINKS.OUT", OutBoundData = true)]        
		public List<string> HyperlinksOut { get; set; }

		public GetRestrictionHyperlinksResponse()
		{	
			LinkLabelsOut = new List<string>();
			HyperlinksOut = new List<string>();
		}
	}
}
