//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:06:40 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: CCDS
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Ccds")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:06:40 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "CCDS", EntityType = "PHYS")]
	public class Ccds : IColleagueEntity
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
		/// CDD Name: CCD.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "CCD.DESC")]
		public string CcdDesc { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER1
		/// </summary>
		[DataMember(Order = 1, Name = "CCD.USER1")]
		public string CcdUser1 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER2
		/// </summary>
		[DataMember(Order = 2, Name = "CCD.USER2")]
		public string CcdUser2 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER3
		/// </summary>
		[DataMember(Order = 3, Name = "CCD.USER3")]
		public string CcdUser3 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER4
		/// </summary>
		[DataMember(Order = 4, Name = "CCD.USER4")]
		public string CcdUser4 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER5
		/// </summary>
		[DataMember(Order = 5, Name = "CCD.USER5")]
		public string CcdUser5 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER6
		/// </summary>
		[DataMember(Order = 6, Name = "CCD.USER6")]
		public string CcdUser6 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER7
		/// </summary>
		[DataMember(Order = 7, Name = "CCD.USER7")]
		public string CcdUser7 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER8
		/// </summary>
		[DataMember(Order = 8, Name = "CCD.USER8")]
		public string CcdUser8 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER9
		/// </summary>
		[DataMember(Order = 9, Name = "CCD.USER9")]
		public string CcdUser9 { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.USER10
		/// </summary>
		[DataMember(Order = 10, Name = "CCD.USER10")]
		public string CcdUser10 { get; set; }
		
		/// <summary>
		/// CDD Name: CCDS.ADDOPR
		/// </summary>
		[DataMember(Order = 11, Name = "CCDS.ADDOPR")]
		public string CcdsAddopr { get; set; }
		
		/// <summary>
		/// CDD Name: CCDS.ADDDATE
		/// </summary>
		[DataMember(Order = 12, Name = "CCDS.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CcdsAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: CCDS.CHGOPR
		/// </summary>
		[DataMember(Order = 13, Name = "CCDS.CHGOPR")]
		public string CcdsChgopr { get; set; }
		
		/// <summary>
		/// CDD Name: CCDS.CHGDATE
		/// </summary>
		[DataMember(Order = 14, Name = "CCDS.CHGDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CcdsChgdate { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.TYPE
		/// </summary>
		[DataMember(Order = 15, Name = "CCD.TYPE")]
		public string CcdType { get; set; }
		
		/// <summary>
		/// CDD Name: CCD.GRANTOR.CODE
		/// </summary>
		[DataMember(Order = 16, Name = "CCD.GRANTOR.CODE")]
		public string CcdGrantorCode { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}