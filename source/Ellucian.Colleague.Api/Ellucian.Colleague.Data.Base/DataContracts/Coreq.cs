//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:27:11 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: COREQ
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Coreq")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:27:11 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "COREQ", EntityType = "PHYS")]
	public class Coreq : IColleagueEntity
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
		/// CDD Name: COREQ.CC.CODE
		/// </summary>
		[DataMember(Order = 2, Name = "COREQ.CC.CODE")]
		public List<string> CoreqCcCode { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.INSTANCE
		/// </summary>
		[DataMember(Order = 3, Name = "COREQ.CC.INSTANCE")]
		public List<string> CoreqCcInstance { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.STATUS
		/// </summary>
		[DataMember(Order = 4, Name = "COREQ.CC.STATUS")]
		public List<string> CoreqCcStatus { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "COREQ.CC.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> CoreqCcDate { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.COMMENT
		/// </summary>
		[DataMember(Order = 6, Name = "COREQ.CC.COMMENT")]
		public List<string> CoreqCcComment { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.REQUIRED
		/// </summary>
		[DataMember(Order = 7, Name = "COREQ.CC.REQUIRED")]
		public List<string> CoreqCcRequired { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.ASSIGN.DT
		/// </summary>
		[DataMember(Order = 8, Name = "COREQ.CC.ASSIGN.DT")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> CoreqCcAssignDt { get; set; }
		
		/// <summary>
		/// CDD Name: COREQ.CC.EXP.ACT.DT
		/// </summary>
		[DataMember(Order = 9, Name = "COREQ.CC.EXP.ACT.DT")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> CoreqCcExpActDt { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CoreqCoreqRequests> CoreqRequestsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: COREQ.REQUESTS
			
			CoreqRequestsEntityAssociation = new List<CoreqCoreqRequests>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(CoreqCcCode != null)
			{
				int numCoreqRequests = CoreqCcCode.Count;
				if (CoreqCcInstance !=null && CoreqCcInstance.Count > numCoreqRequests) numCoreqRequests = CoreqCcInstance.Count;
				if (CoreqCcStatus !=null && CoreqCcStatus.Count > numCoreqRequests) numCoreqRequests = CoreqCcStatus.Count;
				if (CoreqCcDate !=null && CoreqCcDate.Count > numCoreqRequests) numCoreqRequests = CoreqCcDate.Count;
				if (CoreqCcComment !=null && CoreqCcComment.Count > numCoreqRequests) numCoreqRequests = CoreqCcComment.Count;
				if (CoreqCcRequired !=null && CoreqCcRequired.Count > numCoreqRequests) numCoreqRequests = CoreqCcRequired.Count;
				if (CoreqCcAssignDt !=null && CoreqCcAssignDt.Count > numCoreqRequests) numCoreqRequests = CoreqCcAssignDt.Count;
				if (CoreqCcExpActDt !=null && CoreqCcExpActDt.Count > numCoreqRequests) numCoreqRequests = CoreqCcExpActDt.Count;

				for (int i = 0; i < numCoreqRequests; i++)
				{

					string value0 = "";
					if (CoreqCcCode != null && i < CoreqCcCode.Count)
					{
						value0 = CoreqCcCode[i];
					}


					string value1 = "";
					if (CoreqCcInstance != null && i < CoreqCcInstance.Count)
					{
						value1 = CoreqCcInstance[i];
					}


					string value2 = "";
					if (CoreqCcStatus != null && i < CoreqCcStatus.Count)
					{
						value2 = CoreqCcStatus[i];
					}


					DateTime? value3 = null;
					if (CoreqCcDate != null && i < CoreqCcDate.Count)
					{
						value3 = CoreqCcDate[i];
					}


					string value4 = "";
					if (CoreqCcComment != null && i < CoreqCcComment.Count)
					{
						value4 = CoreqCcComment[i];
					}


					string value5 = "";
					if (CoreqCcRequired != null && i < CoreqCcRequired.Count)
					{
						value5 = CoreqCcRequired[i];
					}


					DateTime? value6 = null;
					if (CoreqCcAssignDt != null && i < CoreqCcAssignDt.Count)
					{
						value6 = CoreqCcAssignDt[i];
					}


					DateTime? value7 = null;
					if (CoreqCcExpActDt != null && i < CoreqCcExpActDt.Count)
					{
						value7 = CoreqCcExpActDt[i];
					}

					CoreqRequestsEntityAssociation.Add(new CoreqCoreqRequests( value0, value1, value2, value3, value4, value5, value6, value7));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CoreqCoreqRequests
	{
		public string CoreqCcCodeAssocMember;	
		public string CoreqCcInstanceAssocMember;	
		public string CoreqCcStatusAssocMember;	
		public DateTime? CoreqCcDateAssocMember;	
		public string CoreqCcCommentAssocMember;	
		public string CoreqCcRequiredAssocMember;	
		public DateTime? CoreqCcAssignDtAssocMember;	
		public DateTime? CoreqCcExpActDtAssocMember;	
		public CoreqCoreqRequests() {}
		public CoreqCoreqRequests(
			string inCoreqCcCode,
			string inCoreqCcInstance,
			string inCoreqCcStatus,
			DateTime? inCoreqCcDate,
			string inCoreqCcComment,
			string inCoreqCcRequired,
			DateTime? inCoreqCcAssignDt,
			DateTime? inCoreqCcExpActDt)
		{
			CoreqCcCodeAssocMember = inCoreqCcCode;
			CoreqCcInstanceAssocMember = inCoreqCcInstance;
			CoreqCcStatusAssocMember = inCoreqCcStatus;
			CoreqCcDateAssocMember = inCoreqCcDate;
			CoreqCcCommentAssocMember = inCoreqCcComment;
			CoreqCcRequiredAssocMember = inCoreqCcRequired;
			CoreqCcAssignDtAssocMember = inCoreqCcAssignDt;
			CoreqCcExpActDtAssocMember = inCoreqCcExpActDt;
		}
	}
}