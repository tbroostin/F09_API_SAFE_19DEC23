//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/5/2019 4:13:22 PM by user dvcoll-srm
//
//     Type: ENTITY
//     Entity: RCPT.TENDER.GL.DISTR
//     Application: ST
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
	[DataContract(Name = "RcptTenderGlDistrBase")]
	[ColleagueDataContract(GeneratedDateTime = "12/5/2019 4:13:22 PM", User = "dvcoll-srm")]
	[EntityDataContract(EntityName = "RCPT.TENDER.GL.DISTR", EntityType = "PHYS")]
	public class RcptTenderGlDistrBase : IColleagueEntity
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
		/// CDD Name: RCPTT.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "RCPTT.DESC")]
		public string RcpttDesc { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}