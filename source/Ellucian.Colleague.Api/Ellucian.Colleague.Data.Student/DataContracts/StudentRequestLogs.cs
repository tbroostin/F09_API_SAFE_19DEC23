//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:18:54 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: STUDENT.REQUEST.LOGS
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "StudentRequestLogs")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:18:54 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "STUDENT.REQUEST.LOGS", EntityType = "PHYS")]
	public class StudentRequestLogs : IColleagueEntity
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
		/// CDD Name: STRL.STUDENT
		/// </summary>
		[DataMember(Order = 0, Name = "STRL.STUDENT")]
		public string StrlStudent { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.RECIPIENT
		/// </summary>
		[DataMember(Order = 1, Name = "STRL.RECIPIENT")]
		public string StrlRecipient { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.TYPE
		/// </summary>
		[DataMember(Order = 2, Name = "STRL.TYPE")]
		public string StrlType { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "STRL.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StrlDate { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.DOCUMENT.CODE
		/// </summary>
		[DataMember(Order = 4, Name = "STRL.DOCUMENT.CODE")]
		public string StrlDocumentCode { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.CHARGED.COPIES
		/// </summary>
		[DataMember(Order = 5, Name = "STRL.CHARGED.COPIES")]
		public int? StrlChargedCopies { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.INVOICE
		/// </summary>
		[DataMember(Order = 6, Name = "STRL.INVOICE")]
		public string StrlInvoice { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.ADDRESS.ID
		/// </summary>
		[DataMember(Order = 7, Name = "STRL.ADDRESS.ID")]
		public string StrlAddressId { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.PRINT.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "STRL.PRINT.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StrlPrintDate { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.COPIES
		/// </summary>
		[DataMember(Order = 9, Name = "STRL.COPIES")]
		public int? StrlCopies { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.TRANSCRIPT.GROUPINGS
		/// </summary>
		[DataMember(Order = 10, Name = "STRL.TRANSCRIPT.GROUPINGS")]
		public List<string> StrlTranscriptGroupings { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.ADDRESS.MODIFIER
		/// </summary>
		[DataMember(Order = 11, Name = "STRL.ADDRESS.MODIFIER")]
		public string StrlAddressModifier { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.ADDRESS
		/// </summary>
		[DataMember(Order = 12, Name = "STRL.ADDRESS")]
		public List<string> StrlAddress { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.CITY
		/// </summary>
		[DataMember(Order = 13, Name = "STRL.CITY")]
		public string StrlCity { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.STATE
		/// </summary>
		[DataMember(Order = 14, Name = "STRL.STATE")]
		public string StrlState { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.ZIP
		/// </summary>
		[DataMember(Order = 15, Name = "STRL.ZIP")]
		public string StrlZip { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.COUNTRY
		/// </summary>
		[DataMember(Order = 16, Name = "STRL.COUNTRY")]
		public string StrlCountry { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.RECIPIENT.RECD.DATE
		/// </summary>
		[DataMember(Order = 17, Name = "STRL.RECIPIENT.RECD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StrlRecipientRecdDate { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.COMMENTS
		/// </summary>
		[DataMember(Order = 18, Name = "STRL.COMMENTS")]
		public string StrlComments { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.DELIVERY.METHOD
		/// </summary>
		[DataMember(Order = 34, Name = "STRL.DELIVERY.METHOD")]
		public string StrlDeliveryMethod { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.RECIPIENT.NAME
		/// </summary>
		[DataMember(Order = 37, Name = "STRL.RECIPIENT.NAME")]
		public string StrlRecipientName { get; set; }
		
		/// <summary>
		/// CDD Name: STRL.STU.REQUEST.LOG.HOLDS
		/// </summary>
		[DataMember(Order = 38, Name = "STRL.STU.REQUEST.LOG.HOLDS")]
		public string StrlStuRequestLogHolds { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}