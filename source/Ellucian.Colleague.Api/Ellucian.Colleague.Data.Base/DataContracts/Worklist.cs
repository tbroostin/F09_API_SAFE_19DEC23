//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:59:44 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: WORKLIST
//     Application: UT
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Worklist")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:59:44 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "WORKLIST", EntityType = "PHYS")]
	public class Worklist : IColleagueEntity
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
		/// CDD Name: WKL.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "WKL.DESCRIPTION")]
		public string WklDescription { get; set; }
		
		/// <summary>
		/// CDD Name: WKL.START.DATE
		/// </summary>
		[DataMember(Order = 6, Name = "WKL.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? WklStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: WKL.START.TIME
		/// </summary>
		[DataMember(Order = 7, Name = "WKL.START.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? WklStartTime { get; set; }
		
		/// <summary>
		/// CDD Name: WKL.EXEC.STATE
		/// </summary>
		[DataMember(Order = 18, Name = "WKL.EXEC.STATE")]
		public string WklExecState { get; set; }
		
		/// <summary>
		/// CDD Name: WKL.CATEGORY
		/// </summary>
		[DataMember(Order = 41, Name = "WKL.CATEGORY")]
		public string WklCategory { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}