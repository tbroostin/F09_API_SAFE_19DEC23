//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/10/2017 12:15:00 PM by user bsf1
//
//     Type: ENTITY
//     Entity: AR.INVOICES
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ArInvoices")]
	[ColleagueDataContract(GeneratedDateTime = "10/10/2017 12:15:00 PM", User = "bsf1")]
	[EntityDataContract(EntityName = "AR.INVOICES", EntityType = "PHYS")]
	public class ArInvoices : IColleagueEntity
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
		/// CDD Name: INV.INVOICE.ITEMS
		/// </summary>
		[DataMember(Order = 6, Name = "INV.INVOICE.ITEMS")]
		public List<string> InvInvoiceItems { get; set; }
		
		/// <summary>
		/// CDD Name: INV.GL.REFERENCE.NOS
		/// </summary>
		[DataMember(Order = 17, Name = "INV.GL.REFERENCE.NOS")]
		public List<string> InvGlReferenceNos { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}