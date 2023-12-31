//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/17/2017 1:49:27 PM by user andrewbenge
//
//     Type: ENTITY
//     Entity: PAC.LOAD.PERIODS
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "PacLoadPeriods")]
	[ColleagueDataContract(GeneratedDateTime = "10/17/2017 1:49:27 PM", User = "andrewbenge")]
	[EntityDataContract(EntityName = "PAC.LOAD.PERIODS", EntityType = "PHYS")]
	public class PacLoadPeriods : IColleagueEntity
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
		/// CDD Name: PLP.PER.ASGMT.CONTRACT.ID
		/// </summary>
		[DataMember(Order = 0, Name = "PLP.PER.ASGMT.CONTRACT.ID")]
		public string PlpPerAsgmtContractId { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.LOAD.PERIOD
		/// </summary>
		[DataMember(Order = 1, Name = "PLP.LOAD.PERIOD")]
		public string PlpLoadPeriod { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.HRP.ID
		/// </summary>
		[DataMember(Order = 2, Name = "PLP.HRP.ID")]
		public string PlpHrpId { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.TOTAL.VALUE
		/// </summary>
		[DataMember(Order = 3, Name = "PLP.TOTAL.VALUE")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlpTotalValue { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.INTENDED.TOTAL.LOAD
		/// </summary>
		[DataMember(Order = 4, Name = "PLP.INTENDED.TOTAL.LOAD")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlpIntendedTotalLoad { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.STATUSES
		/// </summary>
		[DataMember(Order = 5, Name = "PLP.STATUSES")]
		public List<string> PlpStatuses { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.STATUS.DATES
		/// </summary>
		[DataMember(Order = 6, Name = "PLP.STATUS.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> PlpStatusDates { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.STATUS.CHGOPR
		/// </summary>
		[DataMember(Order = 7, Name = "PLP.STATUS.CHGOPR")]
		public List<string> PlpStatusChgopr { get; set; }
		
		/// <summary>
		/// CDD Name: PLP.PAC.LP.POSITION.IDS
		/// </summary>
		[DataMember(Order = 8, Name = "PLP.PAC.LP.POSITION.IDS")]
		public List<string> PlpPacLpPositionIds { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PacLoadPeriodsPlpstatus> PlpstatusEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: PLPSTATUS
			
			PlpstatusEntityAssociation = new List<PacLoadPeriodsPlpstatus>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PlpStatuses != null)
			{
				int numPlpstatus = PlpStatuses.Count;
				if (PlpStatusDates !=null && PlpStatusDates.Count > numPlpstatus) numPlpstatus = PlpStatusDates.Count;
				if (PlpStatusChgopr !=null && PlpStatusChgopr.Count > numPlpstatus) numPlpstatus = PlpStatusChgopr.Count;

				for (int i = 0; i < numPlpstatus; i++)
				{

					string value0 = "";
					if (PlpStatuses != null && i < PlpStatuses.Count)
					{
						value0 = PlpStatuses[i];
					}


					DateTime? value1 = null;
					if (PlpStatusDates != null && i < PlpStatusDates.Count)
					{
						value1 = PlpStatusDates[i];
					}


					string value2 = "";
					if (PlpStatusChgopr != null && i < PlpStatusChgopr.Count)
					{
						value2 = PlpStatusChgopr[i];
					}

					PlpstatusEntityAssociation.Add(new PacLoadPeriodsPlpstatus( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class PacLoadPeriodsPlpstatus
	{
		public string PlpStatusesAssocMember;	
		public DateTime? PlpStatusDatesAssocMember;	
		public string PlpStatusChgoprAssocMember;	
		public PacLoadPeriodsPlpstatus() {}
		public PacLoadPeriodsPlpstatus(
			string inPlpStatuses,
			DateTime? inPlpStatusDates,
			string inPlpStatusChgopr)
		{
			PlpStatusesAssocMember = inPlpStatuses;
			PlpStatusDatesAssocMember = inPlpStatusDates;
			PlpStatusChgoprAssocMember = inPlpStatusChgopr;
		}
	}
}