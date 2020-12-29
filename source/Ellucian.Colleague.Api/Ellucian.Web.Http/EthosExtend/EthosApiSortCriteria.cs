// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiSortCriteria
    {
        /// <summary>
        /// Sort Column Names
        /// </summary>
        public string SortColumn { get; private set; }

        /// <summary>
        /// Sort Sequence (BY, BY.DSND, etc.)
        /// </summary>
        public string SortSequence { get; private set; }

        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// <p
        public EthosApiSortCriteria(string sortColumn, string sortSequence)
        {
            SortColumn = sortColumn;
            SortSequence = sortSequence;
        }

    }
}