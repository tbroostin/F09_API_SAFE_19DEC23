// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
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
        /// Primary Entity Name
        /// </summary>
        public string PrimaryEntity { get; set; }

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
        public List<EthosApiSupportedMethods> HttpMethods { get; set; }

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
        public List<EthosApiSelectCriteria> SelectionCriteria { get; set; }

        /// <summary>
        /// Paragraph used to further down selection of records.
        /// </summary>
        public List<string> SelectParagraph { get; set; }

        /// <summary>
        /// Rules to be applied to selected records to further narrow down selection
        /// </summary>
        public List<string> SelectRules { get; set; }

        /// <summary>
        /// Sort columns and operations when sorting is applied.
        /// </summary>
        public List<EthosApiSortCriteria> SortColumns { get; set; }

        /// <summary>
        /// Saving Field Name
        /// </summary>
        public string SavingField { get; set; }

        /// <summary>
        /// Saving Field Option
        /// </summary>
        public string SavingOption { get; set; }

        /// <summary>
        /// Constructor for Ethos Api Configuration
        /// </summary>
        public EthosApiConfiguration()
        {
            HttpMethods = new List<EthosApiSupportedMethods>();
            SelectionCriteria = new List<EthosApiSelectCriteria>();
            SelectRules = new List<string>();
            SortColumns = new List<EthosApiSortCriteria>();
        }
    }
}