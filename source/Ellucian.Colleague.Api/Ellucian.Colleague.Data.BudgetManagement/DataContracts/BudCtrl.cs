//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/6/2019 4:39:52 PM by user tglsql
//
//     Type: ENTITY
//     Entity: BUD.CTRL
//     Application: CF
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

namespace Ellucian.Colleague.Data.BudgetManagement.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "BudCtrl")]
	[ColleagueDataContract(GeneratedDateTime = "6/6/2019 4:39:52 PM", User = "tglsql")]
	[EntityDataContract(EntityName = "BUD.CTRL", EntityType = "PHYS")]
	public class BudCtrl : IColleagueEntity
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
		/// CDD Name: BC.BOF.ID
		/// </summary>
		[DataMember(Order = 1, Name = "BC.BOF.ID")]
		public string BcBofId { get; set; }
		
		/// <summary>
		/// CDD Name: BC.SUB
		/// </summary>
		[DataMember(Order = 3, Name = "BC.SUB")]
		public List<string> BcSub { get; set; }
		
		/// <summary>
		/// CDD Name: BC.SUP
		/// </summary>
		[DataMember(Order = 4, Name = "BC.SUP")]
		public string BcSup { get; set; }
		
		/// <summary>
		/// CDD Name: BC.AUTH.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "BC.AUTH.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? BcAuthDate { get; set; }
		
		/// <summary>
		/// CDD Name: BC.WORK.LINE.NO
		/// </summary>
		[DataMember(Order = 30, Name = "BC.WORK.LINE.NO")]
		public List<string> BcWorkLineNo { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}