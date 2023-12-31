//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/27/2022 2:00:09 PM by user jsullivan
//
//     Type: CTX
//     Transaction ID: TX.CREATE.WEB.REQ
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Transactions
{
	[DataContract]
	public class AlReqLineItems
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LINE.ITEM.DESCS", InBoundData = true)]
		public string AlLineItemDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LINE.ITEM.QTYS", InBoundData = true)]
		public string AlLineItemQtys { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ITEM.PRICES", InBoundData = true)]
		public string AlItemPrices { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GL.ACCTS", InBoundData = true)]
		public string AlGlAccts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GL.ACCT.AMTS", InBoundData = true)]
		public string AlGlAcctAmts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROJECTS", InBoundData = true)]
		public string AlProjects { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.UNIT.OF.ISSUES", InBoundData = true)]
		public string AlUnitOfIssues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ITEMS", InBoundData = true)]
		public string AlVendorItems { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GL.ACCT.QTYS", InBoundData = true)]
		public string AlGlAcctQtys { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GL.ACCT.PCTS", InBoundData = true)]
		public string AlGlAcctPcts { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.CREATE.WEB.REQ", GeneratedDateTime = "1/27/2022 2:00:09 PM", User = "jsullivan")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxCreateWebRequisitionRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.REQ.DATE", InBoundData = true)]        
		public Nullable<DateTime> AReqDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.REQ.DESIRED.DATE", InBoundData = true)]        
		public Nullable<DateTime> AReqDesiredDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQ.INITIATOR.INITIALS", InBoundData = true)]        
		public string AReqInitiatorInitials { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQ.SHIP.TO", InBoundData = true)]        
		public string AReqShipToAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CONF.EMAIL.ADDRESSES", InBoundData = true)]        
		public List<string> AlConfEmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.VENDOR.ID", InBoundData = true)]        
		public string AVendorId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.VENDOR.IS.PERSON.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool AVendorIsPersonFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.AP.TYPE", InBoundData = true)]        
		public string AApType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PRINTED.COMMENTS", InBoundData = true)]        
		public List<string> AlPrintedComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.INTERNAL.COMMENTS", InBoundData = true)]        
		public List<string> AlInternalComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TAX.CODES", InBoundData = true)]        
		public List<string> AlTaxCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.NEXT.APPROVERS", InBoundData = true)]        
		public List<string> AlNextApprovers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.LINE.ITEM.DESCS", InBoundData = true)]
		public List<AlReqLineItems> AlReqLineItems { get; set; }

		public TxCreateWebRequisitionRequest()
		{	
			AlConfEmailAddresses = new List<string>();
			AlPrintedComments = new List<string>();
			AlInternalComments = new List<string>();
			AlTaxCodes = new List<string>();
			AlNextApprovers = new List<string>();
			AlReqLineItems = new List<AlReqLineItems>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.CREATE.WEB.REQ", GeneratedDateTime = "1/27/2022 2:00:09 PM", User = "jsullivan")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxCreateWebRequisitionResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUISITION.ID", OutBoundData = true)]        
		public string ARequisitionId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUISITION.NO", OutBoundData = true)]        
		public string ARequisitionNo { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.REQ.DATE", OutBoundData = true)]        
		public Nullable<DateTime> AReqDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.WARNING", OutBoundData = true)]        
		public string AWarning { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.WARNING.MESSAGES", OutBoundData = true)]        
		public List<string> AlWarningMessages { get; set; }

		public TxCreateWebRequisitionResponse()
		{	
			AlErrorMessages = new List<string>();
			AlWarningMessages = new List<string>();
		}
	}
}
