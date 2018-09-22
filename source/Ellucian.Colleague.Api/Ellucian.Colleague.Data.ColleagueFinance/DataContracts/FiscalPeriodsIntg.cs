//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:18:01 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: FISCAL.PERIODS.INTG
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
	[DataContract(Name = "FiscalPeriodsIntg")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:18:01 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "FISCAL.PERIODS.INTG", EntityType = "PHYS")]
	public class FiscalPeriodsIntg : IColleagueGuidEntity
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
		/// CDD Name: FPI.FISCAL.PERIOD
		/// </summary>
		[DataMember(Order = 4, Name = "FPI.FISCAL.PERIOD")]
		public int? FpiFiscalPeriod { get; set; }
		
		/// <summary>
		/// CDD Name: FPI.FISCAL.YEAR
		/// </summary>
		[DataMember(Order = 5, Name = "FPI.FISCAL.YEAR")]
		public int? FpiFiscalYear { get; set; }
		
		/// <summary>
		/// CDD Name: FPI.CURRENT.STATUS
		/// </summary>
		[DataMember(Order = 6, Name = "FPI.CURRENT.STATUS")]
		public string FpiCurrentStatus { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}