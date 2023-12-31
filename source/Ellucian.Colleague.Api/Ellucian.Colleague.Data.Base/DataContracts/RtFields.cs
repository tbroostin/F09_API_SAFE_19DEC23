//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/1/2022 10:06:58 PM by user asainju1
//
//     Type: ENTITY
//     Entity: RT.FIELDS
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "RtFields")]
	[ColleagueDataContract(GeneratedDateTime = "3/1/2022 10:06:58 PM", User = "asainju1")]
	[EntityDataContract(EntityName = "RT.FIELDS", EntityType = "PHYS")]
	public class RtFields : IColleagueEntity
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
		/// CDD Name: RTFLDS.VIRTUAL.FIELD.DEF
		/// </summary>
		[DataMember(Order = 11, Name = "RTFLDS.VIRTUAL.FIELD.DEF")]
		public List<string> RtfldsVirtualFieldDef { get; set; }
		
		/// <summary>
		/// CDD Name: RTFLDS.VALIDATION.TABLE
		/// </summary>
		[DataMember(Order = 13, Name = "RTFLDS.VALIDATION.TABLE")]
		public string RtfldsValidationTable { get; set; }
		
		/// <summary>
		/// CDD Name: RTFLDS.VALIDATION.FILE
		/// </summary>
		[DataMember(Order = 14, Name = "RTFLDS.VALIDATION.FILE")]
		public string RtfldsValidationFile { get; set; }
		
		/// <summary>
		/// CDD Name: RTFLDS.VAL.TABLE.APPLICATION
		/// </summary>
		[DataMember(Order = 17, Name = "RTFLDS.VAL.TABLE.APPLICATION")]
		public string RtfldsValTableApplication { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<RtFieldsRtfldsOraVfd> RtfldsOraVfdEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: RTFLDS.ORA.VFD
			
			RtfldsOraVfdEntityAssociation = new List<RtFieldsRtfldsOraVfd>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(RtfldsVirtualFieldDef != null)
			{
				int numRtfldsOraVfd = RtfldsVirtualFieldDef.Count;

				for (int i = 0; i < numRtfldsOraVfd; i++)
				{

					string value0 = "";
					if (RtfldsVirtualFieldDef != null && i < RtfldsVirtualFieldDef.Count)
					{
						value0 = RtfldsVirtualFieldDef[i];
					}

					RtfldsOraVfdEntityAssociation.Add(new RtFieldsRtfldsOraVfd( value0));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class RtFieldsRtfldsOraVfd
	{
		public string RtfldsVirtualFieldDefAssocMember;	
		public RtFieldsRtfldsOraVfd() {}
		public RtFieldsRtfldsOraVfd(
			string inRtfldsVirtualFieldDef)
		{
			RtfldsVirtualFieldDefAssocMember = inRtfldsVirtualFieldDef;
		}
	}
}