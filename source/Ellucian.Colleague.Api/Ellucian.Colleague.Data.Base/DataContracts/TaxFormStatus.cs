//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:59:19 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: TAX.FORM.STATUS
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
	[DataContract(Name = "TaxFormStatus")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:59:19 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "TAX.FORM.STATUS", EntityType = "PHYS")]
	public class TaxFormStatus : IColleagueEntity
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
		/// CDD Name: TFS.GEN.DATE
		/// </summary>
		[DataMember(Order = 0, Name = "TFS.GEN.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? TfsGenDate { get; set; }
		
		/// <summary>
		/// CDD Name: TFS.STATUS
		/// </summary>
		[DataMember(Order = 20, Name = "TFS.STATUS")]
		public string TfsStatus { get; set; }
		
		/// <summary>
		/// CDD Name: TFS.TAX.YEAR
		/// </summary>
		[DataMember(Order = 24, Name = "TFS.TAX.YEAR")]
		public string TfsTaxYear { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}