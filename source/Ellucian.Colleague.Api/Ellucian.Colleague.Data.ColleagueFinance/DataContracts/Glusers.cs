//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/19/2021 3:17:32 PM by user tglsql
//
//     Type: ENTITY
//     Entity: GLUSERS
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
	[DataContract(Name = "Glusers")]
	[ColleagueDataContract(GeneratedDateTime = "11/19/2021 3:17:32 PM", User = "tglsql")]
	[EntityDataContract(EntityName = "GLUSERS", EntityType = "PHYS")]
	public class Glusers : IColleagueEntity
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
		/// CDD Name: GLUS.START.DATE
		/// </summary>
		[DataMember(Order = 0, Name = "GLUS.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlusStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.END.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "GLUS.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlusEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.ROLE.IDS
		/// </summary>
		[DataMember(Order = 2, Name = "GLUS.ROLE.IDS")]
		public List<string> GlusRoleIds { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.ROLE.START.DATES
		/// </summary>
		[DataMember(Order = 3, Name = "GLUS.ROLE.START.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> GlusRoleStartDates { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.ROLE.END.DATES
		/// </summary>
		[DataMember(Order = 4, Name = "GLUS.ROLE.END.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> GlusRoleEndDates { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.ROLE.IDS
		/// </summary>
		[DataMember(Order = 9, Name = "GLUS.APPR.ROLE.IDS")]
		public List<string> GlusApprRoleIds { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.START.DATES
		/// </summary>
		[DataMember(Order = 10, Name = "GLUS.APPR.START.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> GlusApprStartDates { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.END.DATES
		/// </summary>
		[DataMember(Order = 11, Name = "GLUS.APPR.END.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> GlusApprEndDates { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.REQ.AMT
		/// </summary>
		[DataMember(Order = 12, Name = "GLUS.APPR.REQ.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlusApprReqAmt { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.PO.AMT
		/// </summary>
		[DataMember(Order = 13, Name = "GLUS.APPR.PO.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlusApprPoAmt { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.VOU.AMT
		/// </summary>
		[DataMember(Order = 14, Name = "GLUS.APPR.VOU.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlusApprVouAmt { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.JE.AMT
		/// </summary>
		[DataMember(Order = 15, Name = "GLUS.APPR.JE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlusApprJeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.BE.AMT
		/// </summary>
		[DataMember(Order = 16, Name = "GLUS.APPR.BE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlusApprBeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: GLUS.APPR.POLICY.FLAG
		/// </summary>
		[DataMember(Order = 17, Name = "GLUS.APPR.POLICY.FLAG")]
		public List<string> GlusApprPolicyFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<GlusersApprovalRoles> ApprovalRolesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: APPROVAL.ROLES
			
			ApprovalRolesEntityAssociation = new List<GlusersApprovalRoles>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(GlusApprRoleIds != null)
			{
				int numApprovalRoles = GlusApprRoleIds.Count;
				if (GlusApprStartDates !=null && GlusApprStartDates.Count > numApprovalRoles) numApprovalRoles = GlusApprStartDates.Count;
				if (GlusApprEndDates !=null && GlusApprEndDates.Count > numApprovalRoles) numApprovalRoles = GlusApprEndDates.Count;
				if (GlusApprReqAmt !=null && GlusApprReqAmt.Count > numApprovalRoles) numApprovalRoles = GlusApprReqAmt.Count;
				if (GlusApprPoAmt !=null && GlusApprPoAmt.Count > numApprovalRoles) numApprovalRoles = GlusApprPoAmt.Count;
				if (GlusApprVouAmt !=null && GlusApprVouAmt.Count > numApprovalRoles) numApprovalRoles = GlusApprVouAmt.Count;
				if (GlusApprJeAmt !=null && GlusApprJeAmt.Count > numApprovalRoles) numApprovalRoles = GlusApprJeAmt.Count;
				if (GlusApprBeAmt !=null && GlusApprBeAmt.Count > numApprovalRoles) numApprovalRoles = GlusApprBeAmt.Count;
				if (GlusApprPolicyFlag !=null && GlusApprPolicyFlag.Count > numApprovalRoles) numApprovalRoles = GlusApprPolicyFlag.Count;

				for (int i = 0; i < numApprovalRoles; i++)
				{

					string value0 = "";
					if (GlusApprRoleIds != null && i < GlusApprRoleIds.Count)
					{
						value0 = GlusApprRoleIds[i];
					}


					DateTime? value1 = null;
					if (GlusApprStartDates != null && i < GlusApprStartDates.Count)
					{
						value1 = GlusApprStartDates[i];
					}


					DateTime? value2 = null;
					if (GlusApprEndDates != null && i < GlusApprEndDates.Count)
					{
						value2 = GlusApprEndDates[i];
					}


					Decimal? value3 = null;
					if (GlusApprReqAmt != null && i < GlusApprReqAmt.Count)
					{
						value3 = GlusApprReqAmt[i];
					}


					Decimal? value4 = null;
					if (GlusApprPoAmt != null && i < GlusApprPoAmt.Count)
					{
						value4 = GlusApprPoAmt[i];
					}


					Decimal? value5 = null;
					if (GlusApprVouAmt != null && i < GlusApprVouAmt.Count)
					{
						value5 = GlusApprVouAmt[i];
					}


					Decimal? value6 = null;
					if (GlusApprJeAmt != null && i < GlusApprJeAmt.Count)
					{
						value6 = GlusApprJeAmt[i];
					}


					Decimal? value7 = null;
					if (GlusApprBeAmt != null && i < GlusApprBeAmt.Count)
					{
						value7 = GlusApprBeAmt[i];
					}


					string value8 = "";
					if (GlusApprPolicyFlag != null && i < GlusApprPolicyFlag.Count)
					{
						value8 = GlusApprPolicyFlag[i];
					}

					ApprovalRolesEntityAssociation.Add(new GlusersApprovalRoles( value0, value1, value2, value3, value4, value5, value6, value7, value8));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class GlusersApprovalRoles
	{
		public string GlusApprRoleIdsAssocMember;	
		public DateTime? GlusApprStartDatesAssocMember;	
		public DateTime? GlusApprEndDatesAssocMember;	
		public Decimal? GlusApprReqAmtAssocMember;	
		public Decimal? GlusApprPoAmtAssocMember;	
		public Decimal? GlusApprVouAmtAssocMember;	
		public Decimal? GlusApprJeAmtAssocMember;	
		public Decimal? GlusApprBeAmtAssocMember;	
		public string GlusApprPolicyFlagAssocMember;	
		public GlusersApprovalRoles() {}
		public GlusersApprovalRoles(
			string inGlusApprRoleIds,
			DateTime? inGlusApprStartDates,
			DateTime? inGlusApprEndDates,
			Decimal? inGlusApprReqAmt,
			Decimal? inGlusApprPoAmt,
			Decimal? inGlusApprVouAmt,
			Decimal? inGlusApprJeAmt,
			Decimal? inGlusApprBeAmt,
			string inGlusApprPolicyFlag)
		{
			GlusApprRoleIdsAssocMember = inGlusApprRoleIds;
			GlusApprStartDatesAssocMember = inGlusApprStartDates;
			GlusApprEndDatesAssocMember = inGlusApprEndDates;
			GlusApprReqAmtAssocMember = inGlusApprReqAmt;
			GlusApprPoAmtAssocMember = inGlusApprPoAmt;
			GlusApprVouAmtAssocMember = inGlusApprVouAmt;
			GlusApprJeAmtAssocMember = inGlusApprJeAmt;
			GlusApprBeAmtAssocMember = inGlusApprBeAmt;
			GlusApprPolicyFlagAssocMember = inGlusApprPolicyFlag;
		}
	}
}