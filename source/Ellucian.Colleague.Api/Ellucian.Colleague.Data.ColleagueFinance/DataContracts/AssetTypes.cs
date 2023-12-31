//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/7/2017 3:13:42 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: ASSET.TYPES
//     Application: CORE
//     Environment: dvcoll_wstst01
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
	[DataContract(Name = "AssetTypes")]
	[ColleagueDataContract(GeneratedDateTime = "12/7/2017 3:13:42 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "ASSET.TYPES", EntityType = "PHYS")]
	public class AssetTypes : IColleagueGuidEntity
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
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: ASTP.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "ASTP.DESC")]
		public string AstpDesc { get; set; }
		
		/// <summary>
		/// CDD Name: ASTP.USEFUL.LIFE
		/// </summary>
		[DataMember(Order = 7, Name = "ASTP.USEFUL.LIFE")]
		public int? AstpUsefulLife { get; set; }
		
		/// <summary>
		/// CDD Name: ASTP.CALC.METHOD
		/// </summary>
		[DataMember(Order = 8, Name = "ASTP.CALC.METHOD")]
		public string AstpCalcMethod { get; set; }
		
		/// <summary>
		/// CDD Name: ASTP.SALVAGE.PCT
		/// </summary>
		[DataMember(Order = 10, Name = "ASTP.SALVAGE.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? AstpSalvagePct { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}