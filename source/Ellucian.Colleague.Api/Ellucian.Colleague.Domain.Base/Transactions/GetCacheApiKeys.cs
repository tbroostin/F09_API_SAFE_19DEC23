//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/1/2020 8:34:59 AM by user bsf1
//
//     Type: CTX
//     Transaction ID: GET.CACHE.API.KEYS
//     Application: CORE
//     Environment: dvcoll-2019
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

namespace Ellucian.Colleague.Domain.Base.Transactions
{
	[DataContract]
	public class KeyCacheInfo
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "KEY.CACHE.PART", InBoundData = true, OutBoundData = true)]
		public string KeyCachePart { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "KEY.CACHE.MIN", InBoundData = true, OutBoundData = true)]
		public Nullable<long> KeyCacheMin { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "KEY.CACHE.MAX", InBoundData = true, OutBoundData = true)]
		public Nullable<long> KeyCacheMax { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "KEY.CACHE.SIZE", InBoundData = true, OutBoundData = true)]
		public Nullable<int> KeyCacheSize { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.CACHE.API.KEYS", GeneratedDateTime = "7/1/2020 8:34:59 AM", User = "bsf1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetCacheApiKeysRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FORCE.CREATE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ForceCreate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CACHE.MODE", InBoundData = true)]        
		public string CacheMode { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "CACHE.NAME", InBoundData = true)]        
		public string CacheName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ENTITY", InBoundData = true)]        
		public string Entity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIMIT.LIST", InBoundData = true)]        
		public List<string> LimitList { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CRITERIA", InBoundData = true)]        
		public string Criteria { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OFFSET", InBoundData = true)]        
		public Nullable<int> Offset { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIMIT", InBoundData = true)]        
		public Nullable<int> Limit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOTAL.COUNT", InBoundData = true)]        
		public Nullable<int> TotalCount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STATEMENTS", InBoundData = true)]        
		public List<string> Statements { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:KEY.CACHE.PART", InBoundData = true)]
		public List<KeyCacheInfo> KeyCacheInfo { get; set; }

		public GetCacheApiKeysRequest()
		{	
			LimitList = new List<string>();
			Statements = new List<string>();
			KeyCacheInfo = new List<KeyCacheInfo>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.CACHE.API.KEYS", GeneratedDateTime = "7/1/2020 8:34:59 AM", User = "bsf1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetCacheApiKeysResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "CACHE.NAME", OutBoundData = true)]        
		public string CacheName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ENTITY", OutBoundData = true)]        
		public string Entity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIMIT.LIST", OutBoundData = true)]        
		public List<string> LimitList { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CRITERIA", OutBoundData = true)]        
		public string Criteria { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OFFSET", OutBoundData = true)]        
		public Nullable<int> Offset { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIMIT", OutBoundData = true)]        
		public Nullable<int> Limit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOTAL.COUNT", OutBoundData = true)]        
		public Nullable<int> TotalCount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CACHE.BUILT", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool CacheBuilt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SUBLIST", OutBoundData = true)]        
		public List<string> Sublist { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STATEMENTS", OutBoundData = true)]        
		public List<string> Statements { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:KEY.CACHE.PART", OutBoundData = true)]
		public List<KeyCacheInfo> KeyCacheInfo { get; set; }

		public GetCacheApiKeysResponse()
		{	
			LimitList = new List<string>();
			Sublist = new List<string>();
			Statements = new List<string>();
			KeyCacheInfo = new List<KeyCacheInfo>();
		}
	}
}
