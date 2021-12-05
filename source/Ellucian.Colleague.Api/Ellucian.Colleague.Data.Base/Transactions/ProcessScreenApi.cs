//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/16/2021 3:17:43 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: PROCESS.SCREEN.API
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class ColumnData
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "COLUMN.NAMES", InBoundData = true, OutBoundData = true)]
		public string ColumnNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "COLUMN.VALUES", InBoundData = true, OutBoundData = true)]
		public string ColumnValues { get; set; }
	}

	[DataContract]
	public class KeyData
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "PRIMARY.KEY.NAMES", InBoundData = true, OutBoundData = true)]
		public string PrimaryKeyNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRIMARY.KEY.VALUES", InBoundData = true, OutBoundData = true)]
		public string PrimaryKeyValues { get; set; }
	}

	[DataContract]
	public class ProcessScreenApiErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "PROCESS.SCREEN.API", GeneratedDateTime = "8/16/2021 3:17:43 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class ProcessScreenApiRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PROCESS.ID", InBoundData = true)]        
		public string ProcessId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PROCESS.MODE", InBoundData = true)]        
		public string ProcessMode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CALL.CHAIN", InBoundData = true)]        
		public List<string> CallChain { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:COLUMN.NAMES", InBoundData = true)]
		public List<ColumnData> ColumnData { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:PRIMARY.KEY.NAMES", InBoundData = true)]
		public List<KeyData> KeyData { get; set; }

		public ProcessScreenApiRequest()
		{	
			CallChain = new List<string>();
			ColumnData = new List<ColumnData>();
			KeyData = new List<KeyData>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "PROCESS.SCREEN.API", GeneratedDateTime = "8/16/2021 3:17:43 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class ProcessScreenApiResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:COLUMN.NAMES", OutBoundData = true)]
		public List<ColumnData> ColumnData { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:PRIMARY.KEY.NAMES", OutBoundData = true)]
		public List<KeyData> KeyData { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<ProcessScreenApiErrors> ProcessScreenApiErrors { get; set; }

		public ProcessScreenApiResponse()
		{	
			ColumnData = new List<ColumnData>();
			KeyData = new List<KeyData>();
			ProcessScreenApiErrors = new List<ProcessScreenApiErrors>();
		}
	}
}
