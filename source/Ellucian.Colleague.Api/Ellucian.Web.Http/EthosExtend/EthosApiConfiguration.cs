// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiConfiguration
    {
        /// <summary>
        /// Name of the Resource
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Type of the Resource
        /// </summary>
        public string ApiType { get; set; }

        /// <summary>
        /// Domain of the Resource
        /// </summary>
        public string ApiDomain { get; set; }

        /// <summary>
        /// Release Status of the Resource
        /// </summary>
        public string ReleaseStatus { get; set; }

        /// <summary>
        /// Process ID when type is Subroutine or Transaction
        /// </summary>
        public string ProcessId { get; set; }

        /// <summary>
        /// Mnemomic and the tile of the process 
        /// </summary>
        public string ProcessDesc { get; set; }

        /// <summary>
        /// List of file used by an API
        /// </summary>
        public List<string> ColleagueFileNames { get; set; }

        /// <summary>
        /// List of Keys to a file or Business Process UI form phantom list
        /// </summary>
        public List<string> ColleagueKeyNames { get; set; }

        /// <summary>
        /// Name of the Resource parent (Making this an alternate representation)
        /// </summary>
        public string ParentResourceName { get; set; }

        /// <summary>
        /// Primary Entity Name
        /// </summary>
        public string PrimaryEntity { get; set; }

        /// <summary>
        /// Primary Key Name
        /// </summary>
        public string PrimaryKeyName { get; set; }

        /// <summary>
        /// Secondary Key Name
        /// </summary>
        public string SecondaryKeyName { get; set; }

        /// <summary>
        /// Secondary Key Position
        /// </summary>
        public int? SecondaryKeyPosition { get; set; }

        /// <summary>
        /// Primary Validation Table Application
        /// </summary>
        public string PrimaryApplication { get; set; }

        /// <summary>
        /// Primary Validation Table Name
        /// </summary>
        public string PrimaryTableName { get; set; }

        /// <summary>
        /// Primary Entity Guid Source CDD element
        /// </summary>
        public string PrimaryGuidSource { get; set; }

        /// <summary>
        /// Primary Entity Guid Source file name
        /// </summary>
        public string PrimaryGuidFileName { get; set; }

        /// <summary>
        /// Primary Entity Guid Source database usage type
        /// </summary>
        public string PrimaryGuidDbType { get; set; }

        /// <summary>
        /// Api Paging Limit for GET all records
        /// </summary>
        public int? PageLimit { get; set; }

        /// <summary>
        /// Supported HTTP methods and permissions
        /// </summary>
        public IList<EthosApiSupportedMethods> HttpMethods { get; set; }

        /// <summary>
        /// Selection File Name for retrieval of records
        /// </summary>
        public string SelectFileName { get; set; }

        /// <summary>
        /// Selection Subroutine Name for retrieval of records
        /// </summary>
        public string SelectSubroutineName { get; set; }

        /// <summary>
        /// The column name when the criteria is applied to a filter or named query
        /// </summary>
        public string SelectColumnName { get; set; }

        /// <summary>
        /// List of Selection Criteria building elements
        /// </summary>
        public IList<EthosApiSelectCriteria> SelectionCriteria { get; set; }

        /// <summary>
        /// Paragraph used to further down selection of records.
        /// </summary>
        public IList<string> SelectParagraph { get; set; }

        /// <summary>
        /// Rules to be applied to selected records to further narrow down selection
        /// </summary>
        public IList<string> SelectRules { get; set; }

        /// <summary>
        /// Sort columns and operations when sorting is applied.
        /// </summary>
        public IList<EthosApiSortCriteria> SortColumns { get; set; }

        /// <summary>
        /// Saving Field Name
        /// </summary>
        public string SavingField { get; set; }

        /// <summary>
        /// Saving Field Option
        /// </summary>
        public string SavingOption { get; set; }

        /// <summary>
        /// Current User from API user required to pass data into repository
        /// </summary>
        public string CurrentUserId { get; set; }

        /// <summary>
        /// Current User ID path for getting records that match current API user only.
        /// </summary>
        public string CurrentUserIdPath { get; set; }

        /// <summary>
        /// API description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of prepared Responses for a UI form business process API
        /// </summary>
        public IList<EthosApiPreparedResponse> PreparedResponses { get; set; }

        /// <summary>
        /// Constructor for Ethos Api Configuration
        /// </summary>
        public EthosApiConfiguration()
        {
            HttpMethods = new List<EthosApiSupportedMethods>();
            SelectionCriteria = new List<EthosApiSelectCriteria>();
            SelectRules = new List<string>();
            SortColumns = new List<EthosApiSortCriteria>();
            PreparedResponses = new List<EthosApiPreparedResponse>();
        }
    }
}