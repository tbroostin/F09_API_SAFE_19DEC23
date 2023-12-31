//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:17:35 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: ENC.FYR
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
	[DataContract(Name = "EncFyr")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:17:35 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "ENC.FYR", EntityType = "PHYS")]
	public class EncFyr : IColleagueEntity
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
		/// CDD Name: ENC.PO.NO
		/// </summary>
		[DataMember(Order = 0, Name = "ENC.PO.NO")]
		public List<string> EncPoNo { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.PO.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "ENC.PO.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> EncPoDate { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.PO.VENDOR
		/// </summary>
		[DataMember(Order = 2, Name = "ENC.PO.VENDOR")]
		public List<string> EncPoVendor { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.PO.AMT
		/// </summary>
		[DataMember(Order = 3, Name = "ENC.PO.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> EncPoAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.REQ.NO
		/// </summary>
		[DataMember(Order = 4, Name = "ENC.REQ.NO")]
		public List<string> EncReqNo { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.REQ.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "ENC.REQ.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> EncReqDate { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.REQ.VENDOR
		/// </summary>
		[DataMember(Order = 6, Name = "ENC.REQ.VENDOR")]
		public List<string> EncReqVendor { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.REQ.AMT
		/// </summary>
		[DataMember(Order = 7, Name = "ENC.REQ.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> EncReqAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.PO.SOURCE
		/// </summary>
		[DataMember(Order = 8, Name = "ENC.PO.SOURCE")]
		public List<string> EncPoSource { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.PO.ID
		/// </summary>
		[DataMember(Order = 9, Name = "ENC.PO.ID")]
		public List<string> EncPoId { get; set; }
		
		/// <summary>
		/// CDD Name: ENC.REQ.ID
		/// </summary>
		[DataMember(Order = 10, Name = "ENC.REQ.ID")]
		public List<string> EncReqId { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<EncFyrEncPo> EncPoEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<EncFyrEncReq> EncReqEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: ENC.PO
			
			EncPoEntityAssociation = new List<EncFyrEncPo>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(EncPoNo != null)
			{
				int numEncPo = EncPoNo.Count;
				if (EncPoDate !=null && EncPoDate.Count > numEncPo) numEncPo = EncPoDate.Count;
				if (EncPoVendor !=null && EncPoVendor.Count > numEncPo) numEncPo = EncPoVendor.Count;
				if (EncPoAmt !=null && EncPoAmt.Count > numEncPo) numEncPo = EncPoAmt.Count;
				if (EncPoSource !=null && EncPoSource.Count > numEncPo) numEncPo = EncPoSource.Count;
				if (EncPoId !=null && EncPoId.Count > numEncPo) numEncPo = EncPoId.Count;

				for (int i = 0; i < numEncPo; i++)
				{

					string value0 = "";
					if (EncPoNo != null && i < EncPoNo.Count)
					{
						value0 = EncPoNo[i];
					}


					DateTime? value1 = null;
					if (EncPoDate != null && i < EncPoDate.Count)
					{
						value1 = EncPoDate[i];
					}


					string value2 = "";
					if (EncPoVendor != null && i < EncPoVendor.Count)
					{
						value2 = EncPoVendor[i];
					}


					Decimal? value3 = null;
					if (EncPoAmt != null && i < EncPoAmt.Count)
					{
						value3 = EncPoAmt[i];
					}


					string value4 = "";
					if (EncPoSource != null && i < EncPoSource.Count)
					{
						value4 = EncPoSource[i];
					}


					string value5 = "";
					if (EncPoId != null && i < EncPoId.Count)
					{
						value5 = EncPoId[i];
					}

					EncPoEntityAssociation.Add(new EncFyrEncPo( value0, value1, value2, value3, value4, value5));
				}
			}
			// EntityAssociation Name: ENC.REQ
			
			EncReqEntityAssociation = new List<EncFyrEncReq>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(EncReqNo != null)
			{
				int numEncReq = EncReqNo.Count;
				if (EncReqDate !=null && EncReqDate.Count > numEncReq) numEncReq = EncReqDate.Count;
				if (EncReqVendor !=null && EncReqVendor.Count > numEncReq) numEncReq = EncReqVendor.Count;
				if (EncReqAmt !=null && EncReqAmt.Count > numEncReq) numEncReq = EncReqAmt.Count;
				if (EncReqId !=null && EncReqId.Count > numEncReq) numEncReq = EncReqId.Count;

				for (int i = 0; i < numEncReq; i++)
				{

					string value0 = "";
					if (EncReqNo != null && i < EncReqNo.Count)
					{
						value0 = EncReqNo[i];
					}


					DateTime? value1 = null;
					if (EncReqDate != null && i < EncReqDate.Count)
					{
						value1 = EncReqDate[i];
					}


					string value2 = "";
					if (EncReqVendor != null && i < EncReqVendor.Count)
					{
						value2 = EncReqVendor[i];
					}


					Decimal? value3 = null;
					if (EncReqAmt != null && i < EncReqAmt.Count)
					{
						value3 = EncReqAmt[i];
					}


					string value4 = "";
					if (EncReqId != null && i < EncReqId.Count)
					{
						value4 = EncReqId[i];
					}

					EncReqEntityAssociation.Add(new EncFyrEncReq( value0, value1, value2, value3, value4));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class EncFyrEncPo
	{
		public string EncPoNoAssocMember;	
		public DateTime? EncPoDateAssocMember;	
		public string EncPoVendorAssocMember;	
		public Decimal? EncPoAmtAssocMember;	
		public string EncPoSourceAssocMember;	
		public string EncPoIdAssocMember;	
		public EncFyrEncPo() {}
		public EncFyrEncPo(
			string inEncPoNo,
			DateTime? inEncPoDate,
			string inEncPoVendor,
			Decimal? inEncPoAmt,
			string inEncPoSource,
			string inEncPoId)
		{
			EncPoNoAssocMember = inEncPoNo;
			EncPoDateAssocMember = inEncPoDate;
			EncPoVendorAssocMember = inEncPoVendor;
			EncPoAmtAssocMember = inEncPoAmt;
			EncPoSourceAssocMember = inEncPoSource;
			EncPoIdAssocMember = inEncPoId;
		}
	}
	
	[Serializable]
	public class EncFyrEncReq
	{
		public string EncReqNoAssocMember;	
		public DateTime? EncReqDateAssocMember;	
		public string EncReqVendorAssocMember;	
		public Decimal? EncReqAmtAssocMember;	
		public string EncReqIdAssocMember;	
		public EncFyrEncReq() {}
		public EncFyrEncReq(
			string inEncReqNo,
			DateTime? inEncReqDate,
			string inEncReqVendor,
			Decimal? inEncReqAmt,
			string inEncReqId)
		{
			EncReqNoAssocMember = inEncReqNo;
			EncReqDateAssocMember = inEncReqDate;
			EncReqVendorAssocMember = inEncReqVendor;
			EncReqAmtAssocMember = inEncReqAmt;
			EncReqIdAssocMember = inEncReqId;
		}
	}
}