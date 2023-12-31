//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/29/2020 2:18:00 PM by user dvcoll-srm
//
//     Type: ENTITY
//     Entity: INTG.GL.POSTINGS
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "IntgGlPostings")]
	[ColleagueDataContract(GeneratedDateTime = "1/29/2020 2:18:00 PM", User = "dvcoll-srm")]
	[EntityDataContract(EntityName = "INTG.GL.POSTINGS", EntityType = "PHYS")]
	public class IntgGlPostings : IColleagueGuidEntity
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
		/// CDD Name: IGP.REF.NO
		/// </summary>
		[DataMember(Order = 4, Name = "IGP.REF.NO")]
		public List<string> IgpRefNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.SOURCE
		/// </summary>
		[DataMember(Order = 5, Name = "IGP.SOURCE")]
		public List<string> IgpSource { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.ACCT.ID
		/// </summary>
		[DataMember(Order = 6, Name = "IGP.ACCT.ID")]
		public List<string> IgpAcctId { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.TR.DATE
		/// </summary>
		[DataMember(Order = 7, Name = "IGP.TR.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> IgpTrDate { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.SYS.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "IGP.SYS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> IgpSysDate { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.TRAN.NO
		/// </summary>
		[DataMember(Order = 9, Name = "IGP.TRAN.NO")]
		public List<string> IgpTranNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.TRAN.DETAILS
		/// </summary>
		[DataMember(Order = 10, Name = "IGP.TRAN.DETAILS")]
		public List<string> IgpTranDetails { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.SUBMITTED.BY
		/// </summary>
		[DataMember(Order = 11, Name = "IGP.SUBMITTED.BY")]
		public string IgpSubmittedBy { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.COMMENTS
		/// </summary>
		[DataMember(Order = 12, Name = "IGP.COMMENTS")]
		public string IgpComments { get; set; }
		
		/// <summary>
		/// CDD Name: IGP.EXT.BATCH
		/// </summary>
		[DataMember(Order = 15, Name = "IGP.EXT.BATCH")]
		public List<string> IgpExtBatch { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<IntgGlPostingsTranDetail> TranDetailEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: TRAN.DETAIL
			
			TranDetailEntityAssociation = new List<IntgGlPostingsTranDetail>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(IgpSource != null)
			{
				int numTranDetail = IgpSource.Count;
				if (IgpRefNo !=null && IgpRefNo.Count > numTranDetail) numTranDetail = IgpRefNo.Count;
				if (IgpAcctId !=null && IgpAcctId.Count > numTranDetail) numTranDetail = IgpAcctId.Count;
				if (IgpTrDate !=null && IgpTrDate.Count > numTranDetail) numTranDetail = IgpTrDate.Count;
				if (IgpSysDate !=null && IgpSysDate.Count > numTranDetail) numTranDetail = IgpSysDate.Count;
				if (IgpTranNo !=null && IgpTranNo.Count > numTranDetail) numTranDetail = IgpTranNo.Count;
				if (IgpTranDetails !=null && IgpTranDetails.Count > numTranDetail) numTranDetail = IgpTranDetails.Count;
				if (IgpExtBatch !=null && IgpExtBatch.Count > numTranDetail) numTranDetail = IgpExtBatch.Count;

				for (int i = 0; i < numTranDetail; i++)
				{

					string value0 = "";
					if (IgpRefNo != null && i < IgpRefNo.Count)
					{
						value0 = IgpRefNo[i];
					}


					string value1 = "";
					if (IgpSource != null && i < IgpSource.Count)
					{
						value1 = IgpSource[i];
					}


					string value2 = "";
					if (IgpAcctId != null && i < IgpAcctId.Count)
					{
						value2 = IgpAcctId[i];
					}


					DateTime? value3 = null;
					if (IgpTrDate != null && i < IgpTrDate.Count)
					{
						value3 = IgpTrDate[i];
					}


					DateTime? value4 = null;
					if (IgpSysDate != null && i < IgpSysDate.Count)
					{
						value4 = IgpSysDate[i];
					}


					string value5 = "";
					if (IgpTranNo != null && i < IgpTranNo.Count)
					{
						value5 = IgpTranNo[i];
					}


					string value6 = "";
					if (IgpTranDetails != null && i < IgpTranDetails.Count)
					{
						value6 = IgpTranDetails[i];
					}


					string value7 = "";
					if (IgpExtBatch != null && i < IgpExtBatch.Count)
					{
						value7 = IgpExtBatch[i];
					}

					TranDetailEntityAssociation.Add(new IntgGlPostingsTranDetail( value0, value1, value2, value3, value4, value5, value6, value7));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class IntgGlPostingsTranDetail
	{
		public string IgpRefNoAssocMember;	
		public string IgpSourceAssocMember;	
		public string IgpAcctIdAssocMember;	
		public DateTime? IgpTrDateAssocMember;	
		public DateTime? IgpSysDateAssocMember;	
		public string IgpTranNoAssocMember;	
		public string IgpTranDetailsAssocMember;	
		public string IgpExtBatchAssocMember;	
		public IntgGlPostingsTranDetail() {}
		public IntgGlPostingsTranDetail(
			string inIgpRefNo,
			string inIgpSource,
			string inIgpAcctId,
			DateTime? inIgpTrDate,
			DateTime? inIgpSysDate,
			string inIgpTranNo,
			string inIgpTranDetails,
			string inIgpExtBatch)
		{
			IgpRefNoAssocMember = inIgpRefNo;
			IgpSourceAssocMember = inIgpSource;
			IgpAcctIdAssocMember = inIgpAcctId;
			IgpTrDateAssocMember = inIgpTrDate;
			IgpSysDateAssocMember = inIgpSysDate;
			IgpTranNoAssocMember = inIgpTranNo;
			IgpTranDetailsAssocMember = inIgpTranDetails;
			IgpExtBatchAssocMember = inIgpExtBatch;
		}
	}
}