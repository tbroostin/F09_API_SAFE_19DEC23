//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/8/2020 10:52:59 AM by user namrathak
//
//     Type: ENTITY
//     Entity: PERLVACC
//     Application: HR
//     Environment: Dvcoll
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Perlvacc")]
	[ColleagueDataContract(GeneratedDateTime = "4/8/2020 10:52:59 AM", User = "namrathak")]
	[EntityDataContract(EntityName = "PERLVACC", EntityType = "PHYS")]
	public class Perlvacc : IColleagueEntity
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
		/// CDD Name: PLA.HRP.ID
		/// </summary>
		[DataMember(Order = 0, Name = "PLA.HRP.ID")]
		public string PlaHrpId { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.LPN.ID
		/// </summary>
		[DataMember(Order = 1, Name = "PLA.LPN.ID")]
		public string PlaLpnId { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.ACCRUAL.HOURS
		/// </summary>
		[DataMember(Order = 2, Name = "PLA.ACCRUAL.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N4}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlaAccrualHours { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.AUTHORIZED.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "PLA.AUTHORIZED.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PlaAuthorizedDate { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.CARRYOVER.HOURS
		/// </summary>
		[DataMember(Order = 4, Name = "PLA.CARRYOVER.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlaCarryoverHours { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.MOVR.CARRYOVER.HOURS
		/// </summary>
		[DataMember(Order = 5, Name = "PLA.MOVR.CARRYOVER.HOURS")]
		public string PlaMovrCarryoverHours { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.ACCRUAL.LIMIT
		/// </summary>
		[DataMember(Order = 6, Name = "PLA.ACCRUAL.LIMIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlaAccrualLimit { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.MOVR.ACCRUAL.LIMIT
		/// </summary>
		[DataMember(Order = 7, Name = "PLA.MOVR.ACCRUAL.LIMIT")]
		public string PlaMovrAccrualLimit { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.MOVR.ACCRUAL.HOURS
		/// </summary>
		[DataMember(Order = 8, Name = "PLA.MOVR.ACCRUAL.HOURS")]
		public string PlaMovrAccrualHours { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.LAST.PAY.DATE
		/// </summary>
		[DataMember(Order = 9, Name = "PLA.LAST.PAY.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PlaLastPayDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERLVACC.ADD.OPERATOR
		/// </summary>
		[DataMember(Order = 10, Name = "PERLVACC.ADD.OPERATOR")]
		public string PerlvaccAddOperator { get; set; }
		
		/// <summary>
		/// CDD Name: PERLVACC.ADD.DATE
		/// </summary>
		[DataMember(Order = 11, Name = "PERLVACC.ADD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PerlvaccAddDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERLVACC.CHANGE.OPERATOR
		/// </summary>
		[DataMember(Order = 12, Name = "PERLVACC.CHANGE.OPERATOR")]
		public string PerlvaccChangeOperator { get; set; }
		
		/// <summary>
		/// CDD Name: PERLVACC.CHANGE.DATE
		/// </summary>
		[DataMember(Order = 13, Name = "PERLVACC.CHANGE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PerlvaccChangeDate { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.PAY.CYCLE
		/// </summary>
		[DataMember(Order = 14, Name = "PLA.PAY.CYCLE")]
		public string PlaPayCycle { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.START.DATE
		/// </summary>
		[DataMember(Order = 15, Name = "PLA.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PlaStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.END.DATE
		/// </summary>
		[DataMember(Order = 16, Name = "PLA.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PlaEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.PERLEAVE.ID
		/// </summary>
		[DataMember(Order = 17, Name = "PLA.PERLEAVE.ID")]
		public long? PlaPerleaveId { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.PAY.CLASS
		/// </summary>
		[DataMember(Order = 18, Name = "PLA.PAY.CLASS")]
		public string PlaPayClass { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.LVPLNACC.ID
		/// </summary>
		[DataMember(Order = 19, Name = "PLA.LVPLNACC.ID")]
		public string PlaLvplnaccId { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.ROLLOVER.MAXIMUM
		/// </summary>
		[DataMember(Order = 20, Name = "PLA.ROLLOVER.MAXIMUM")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlaRolloverMaximum { get; set; }
		
		/// <summary>
		/// CDD Name: PLA.MOVR.ROLLOVER.MAXIMUM
		/// </summary>
		[DataMember(Order = 21, Name = "PLA.MOVR.ROLLOVER.MAXIMUM")]
		public string PlaMovrRolloverMaximum { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}