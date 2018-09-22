//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/8/2017 8:50:41 AM by user bsf1
//
//     Type: ENTITY
//     Entity: CHECKS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Checks")]
	[ColleagueDataContract(GeneratedDateTime = "11/8/2017 8:50:41 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "CHECKS", EntityType = "PHYS")]
	public class Checks : IColleagueGuidEntity
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
		/// CDD Name: CHK.DATE
		/// </summary>
		[DataMember(Order = 0, Name = "CHK.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ChkDate { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.AMOUNT
		/// </summary>
		[DataMember(Order = 1, Name = "CHK.AMOUNT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ChkAmount { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.STATUS
		/// </summary>
		[DataMember(Order = 2, Name = "CHK.STATUS")]
		public List<string> ChkStatus { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.STATUS.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "CHK.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> ChkStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.VOUCHERS.IDS
		/// </summary>
		[DataMember(Order = 4, Name = "CHK.VOUCHERS.IDS")]
		public List<string> ChkVouchersIds { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.VENDOR
		/// </summary>
		[DataMember(Order = 5, Name = "CHK.VENDOR")]
		public string ChkVendor { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.SOURCE
		/// </summary>
		[DataMember(Order = 6, Name = "CHK.SOURCE")]
		public string ChkSource { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.MISC.NAME
		/// </summary>
		[DataMember(Order = 8, Name = "CHK.MISC.NAME")]
		public List<string> ChkMiscName { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.CURRENCY.CODE
		/// </summary>
		[DataMember(Order = 11, Name = "CHK.CURRENCY.CODE")]
		public string ChkCurrencyCode { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.ADDRESS
		/// </summary>
		[DataMember(Order = 16, Name = "CHK.ADDRESS")]
		public List<string> ChkAddress { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.CITY
		/// </summary>
		[DataMember(Order = 17, Name = "CHK.CITY")]
		public string ChkCity { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.STATE
		/// </summary>
		[DataMember(Order = 18, Name = "CHK.STATE")]
		public string ChkState { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.ZIP
		/// </summary>
		[DataMember(Order = 19, Name = "CHK.ZIP")]
		public string ChkZip { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.COUNTRY
		/// </summary>
		[DataMember(Order = 20, Name = "CHK.COUNTRY")]
		public string ChkCountry { get; set; }
		
		/// <summary>
		/// CDD Name: CHK.ECHECK.FLAG
		/// </summary>
		[DataMember(Order = 24, Name = "CHK.ECHECK.FLAG")]
		public string ChkEcheckFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<ChecksChkStat> ChkStatEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: CHK.STAT
			
			ChkStatEntityAssociation = new List<ChecksChkStat>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(ChkStatus != null)
			{
				int numChkStat = ChkStatus.Count;
				if (ChkStatusDate !=null && ChkStatusDate.Count > numChkStat) numChkStat = ChkStatusDate.Count;

				for (int i = 0; i < numChkStat; i++)
				{

					string value0 = "";
					if (ChkStatus != null && i < ChkStatus.Count)
					{
						value0 = ChkStatus[i];
					}


					DateTime? value1 = null;
					if (ChkStatusDate != null && i < ChkStatusDate.Count)
					{
						value1 = ChkStatusDate[i];
					}

					ChkStatEntityAssociation.Add(new ChecksChkStat( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class ChecksChkStat
	{
		public string ChkStatusAssocMember;	
		public DateTime? ChkStatusDateAssocMember;	
		public ChecksChkStat() {}
		public ChecksChkStat(
			string inChkStatus,
			DateTime? inChkStatusDate)
		{
			ChkStatusAssocMember = inChkStatus;
			ChkStatusDateAssocMember = inChkStatusDate;
		}
	}
}