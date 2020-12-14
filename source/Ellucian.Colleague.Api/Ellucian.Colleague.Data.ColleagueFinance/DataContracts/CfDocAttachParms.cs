//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/2/2020 8:29:42 AM by user jsullivan
//
//     Type: ENTITY
//     Entity: CF.DOC.ATTACH.PARMS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CfDocAttachParms")]
	[ColleagueDataContract(GeneratedDateTime = "7/2/2020 8:29:42 AM", User = "jsullivan")]
	[EntityDataContract(EntityName = "CF.DOC.ATTACH.PARMS", EntityType = "PERM")]
	public class CfDocAttachParms : IColleagueEntity
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
		/// CDD Name: CF.DOC.ATTACH.VOUCHER.COL
		/// </summary>
		[DataMember(Order = 0, Name = "CF.DOC.ATTACH.VOUCHER.COL")]
		public string CfDocAttachVoucherCol { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}