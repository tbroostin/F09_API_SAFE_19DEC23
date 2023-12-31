//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/25/2018 3:24:32 PM by user vinay_an
//
//     Type: ENTITY
//     Entity: T4.CONSENT.HISTORY
//     Application: HR
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "T4ConsentHistory")]
	[ColleagueDataContract(GeneratedDateTime = "10/25/2018 3:24:32 PM", User = "vinay_an")]
	[EntityDataContract(EntityName = "T4.CONSENT.HISTORY", EntityType = "PHYS")]
	public class T4ConsentHistory : IColleagueEntity
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		/// <summary>
		/// Record Key
		/// </summary>
		[DataMember]
		public string Recordkey { get; set; }
		
		public void setKey(string key)
		{
			Recordkey = key;
		}
		
		/// <summary>
		/// CDD Name: T4CH.HRPER.ID
		/// </summary>
		[DataMember(Order = 0, Name = "T4CH.HRPER.ID")]
		public string T4chHrperId { get; set; }
		
		/// <summary>
		/// CDD Name: T4CH.NEW.STATUS
		/// </summary>
		[DataMember(Order = 2, Name = "T4CH.NEW.STATUS")]
		public string T4chNewStatus { get; set; }
		
		/// <summary>
		/// CDD Name: T4.CONSENT.HISTORY.ADDDATE
		/// </summary>
		[DataMember(Order = 6, Name = "T4.CONSENT.HISTORY.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? T4ConsentHistoryAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: T4.CONSENT.HISTORY.ADDTIME
		/// </summary>
		[DataMember(Order = 7, Name = "T4.CONSENT.HISTORY.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? T4ConsentHistoryAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: T4CH.STATUS.DATE
		/// </summary>
		[DataMember(Order = 11, Name = "T4CH.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? T4chStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: T4CH.STATUS.TIME
		/// </summary>
		[DataMember(Order = 12, Name = "T4CH.STATUS.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? T4chStatusTime { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}