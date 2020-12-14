//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/12/2020 11:40:31 AM by user vaidyasubrahmanyadv
//
//     Type: ENTITY
//     Entity: TAX.1099NEC.DETAIL.REPOS
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
	[DataContract(Name = "Tax1099necDetailRepos")]
	[ColleagueDataContract(GeneratedDateTime = "8/12/2020 11:40:31 AM", User = "vaidyasubrahmanyadv")]
	[EntityDataContract(EntityName = "TAX.1099NEC.DETAIL.REPOS", EntityType = "PHYS")]
	public class Tax1099necDetailRepos : IColleagueEntity
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
		/// CDD Name: TNEDTLR.REPOS.ID
		/// </summary>
		[DataMember(Order = 0, Name = "TNEDTLR.REPOS.ID")]
		public string TnedtlrReposId { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.STATE.REPOS.ID
		/// </summary>
		[DataMember(Order = 1, Name = "TNEDTLR.STATE.REPOS.ID")]
		public string TnedtlrStateReposId { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.VENDOR.ID
		/// </summary>
		[DataMember(Order = 2, Name = "TNEDTLR.VENDOR.ID")]
		public string TnedtlrVendorId { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.EIN
		/// </summary>
		[DataMember(Order = 3, Name = "TNEDTLR.EIN")]
		public string TnedtlrEin { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.STATE.ID
		/// </summary>
		[DataMember(Order = 4, Name = "TNEDTLR.STATE.ID")]
		public string TnedtlrStateId { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.REF.ID
		/// </summary>
		[DataMember(Order = 5, Name = "TNEDTLR.REF.ID")]
		public string TnedtlrRefId { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.STATUS
		/// </summary>
		[DataMember(Order = 6, Name = "TNEDTLR.STATUS")]
		public string TnedtlrStatus { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.BOX.NUMBER
		/// </summary>
		[DataMember(Order = 7, Name = "TNEDTLR.BOX.NUMBER")]
		public List<string> TnedtlrBoxNumber { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.AMT
		/// </summary>
		[DataMember(Order = 8, Name = "TNEDTLR.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> TnedtlrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.QUALIFIED.FLAG
		/// </summary>
		[DataMember(Order = 9, Name = "TNEDTLR.QUALIFIED.FLAG")]
		public string TnedtlrQualifiedFlag { get; set; }
		
		/// <summary>
		/// CDD Name: TNEDTLR.YEAR
		/// </summary>
		[DataMember(Order = 10, Name = "TNEDTLR.YEAR")]
		public string TnedtlrYear { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<Tax1099necDetailReposTnedtlrBoxInfo> TnedtlrBoxInfoEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: TNEDTLR.BOX.INFO
			
			TnedtlrBoxInfoEntityAssociation = new List<Tax1099necDetailReposTnedtlrBoxInfo>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(TnedtlrBoxNumber != null)
			{
				int numTnedtlrBoxInfo = TnedtlrBoxNumber.Count;
				if (TnedtlrAmt !=null && TnedtlrAmt.Count > numTnedtlrBoxInfo) numTnedtlrBoxInfo = TnedtlrAmt.Count;

				for (int i = 0; i < numTnedtlrBoxInfo; i++)
				{

					string value0 = "";
					if (TnedtlrBoxNumber != null && i < TnedtlrBoxNumber.Count)
					{
						value0 = TnedtlrBoxNumber[i];
					}


					Decimal? value1 = null;
					if (TnedtlrAmt != null && i < TnedtlrAmt.Count)
					{
						value1 = TnedtlrAmt[i];
					}

					TnedtlrBoxInfoEntityAssociation.Add(new Tax1099necDetailReposTnedtlrBoxInfo( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class Tax1099necDetailReposTnedtlrBoxInfo
	{
		public string TnedtlrBoxNumberAssocMember;	
		public Decimal? TnedtlrAmtAssocMember;	
		public Tax1099necDetailReposTnedtlrBoxInfo() {}
		public Tax1099necDetailReposTnedtlrBoxInfo(
			string inTnedtlrBoxNumber,
			Decimal? inTnedtlrAmt)
		{
			TnedtlrBoxNumberAssocMember = inTnedtlrBoxNumber;
			TnedtlrAmtAssocMember = inTnedtlrAmt;
		}
	}
}