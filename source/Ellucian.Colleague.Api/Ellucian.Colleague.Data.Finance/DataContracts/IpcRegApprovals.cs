//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:35:55 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: IPC.REG.APPROVALS
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

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "IpcRegApprovals")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:35:55 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "IPC.REG.APPROVALS", EntityType = "PHYS")]
	public class IpcRegApprovals : IColleagueEntity
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
		/// CDD Name: IPCRA.REGISTRATION
		/// </summary>
		[DataMember(Order = 6, Name = "IPCRA.REGISTRATION")]
		public string IpcraRegistration { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.TERMS.RESPONSE
		/// </summary>
		[DataMember(Order = 7, Name = "IPCRA.TERMS.RESPONSE")]
		public string IpcraTermsResponse { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.COURSE.SECTIONS
		/// </summary>
		[DataMember(Order = 8, Name = "IPCRA.COURSE.SECTIONS")]
		public List<string> IpcraCourseSections { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.AR.INVOICES
		/// </summary>
		[DataMember(Order = 9, Name = "IPCRA.AR.INVOICES")]
		public List<string> IpcraArInvoices { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.ACK.DOCUMENT
		/// </summary>
		[DataMember(Order = 10, Name = "IPCRA.ACK.DOCUMENT")]
		public string IpcraAckDocument { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.ACK.DATE
		/// </summary>
		[DataMember(Order = 11, Name = "IPCRA.ACK.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? IpcraAckDate { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.ACK.TIME
		/// </summary>
		[DataMember(Order = 12, Name = "IPCRA.ACK.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? IpcraAckTime { get; set; }
		
		/// <summary>
		/// CDD Name: IPCRA.ACK.SHA1.HASH
		/// </summary>
		[DataMember(Order = 13, Name = "IPCRA.ACK.SHA1.HASH")]
		public string IpcraAckSha1Hash { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}