//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:44:25 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PORTAL.SITES
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
	[DataContract(Name = "PortalSites")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:44:25 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PORTAL.SITES", EntityType = "PHYS")]
	public class PortalSites : IColleagueEntity
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
		/// CDD Name: PS.PRTL.SITE.GUID
		/// </summary>
		[DataMember(Order = 0, Name = "PS.PRTL.SITE.GUID")]
		public string PsPrtlSiteGuid { get; set; }
		
		/// <summary>
		/// CDD Name: PS.LEARNING.PROVIDER
		/// </summary>
		[DataMember(Order = 12, Name = "PS.LEARNING.PROVIDER")]
		public string PsLearningProvider { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}